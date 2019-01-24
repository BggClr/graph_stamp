using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Build.Evaluation;
using Models;
using Newtonsoft.Json;
using RestSharp;

namespace Stamp.Services.Components
{
	public class Fetch
	{
		public class Query : IRequest<Response>
		{
			public string Owner { get; set; }
			public string Name { get; set; }
			public string Ref { get; set; }
		}

		public class Response
		{
			public string Owner { get; set; }
			public string Name { get; set; }
			public string Url { get; set; }
			public string Destination { get; set; }
		}

		public class QueryHandler : IRequestHandler<Query, Response>
		{
			private readonly IChooseOptionsService _chooseOptionsService;

			public QueryHandler(IChooseOptionsService chooseOptionsService)
			{
				_chooseOptionsService = chooseOptionsService;
			}

			public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
			{
				var url = $"https://api.github.com/repos/{request.Owner}/{request.Name}/zipball";

				if (!string.IsNullOrEmpty(request.Ref))
				{
					url = $"{url}/{request.Ref}";
				}

				var archive = await DownloadZip(url, cancellationToken);

				return await DeployComponent(request, archive, cancellationToken);
			}

			private async Task<Response> DeployComponent(Query request, byte[] archive, CancellationToken cancellationToken)
			{
				var tmpFolderName = Guid.NewGuid().ToString();
				Response result = null;
				try
				{
					using (var stream = new MemoryStream(archive))
					{
						var zip = new ZipArchive(stream);
						zip.ExtractToDirectory(tmpFolderName);
					}

					var extractedComponentRoot = Directory.GetDirectories(tmpFolderName).First();
					var folder = Path.GetFullPath(Path.Combine(extractedComponentRoot, "src"));

					var folderUri = new Uri($"{folder}/");

					var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);

					var manifest = GetManifest(extractedComponentRoot);
					Project project = null;

					if (manifest.IsAddToCsprojRequired)
					{
						var projectCollection = new ProjectCollection();
						var projectFilePath = await GetProjectFilePath(cancellationToken);
						project = new Project(projectFilePath, null, null, projectCollection, ProjectLoadSettings.IgnoreMissingImports);
					}

					foreach (var file in files)
					{
						var relativePath = folderUri.MakeRelativeUri(new Uri(file));
						var targetPath = Path.Combine(manifest.Destination, manifest.Name, relativePath.ToString());
						var targetFolder = Path.GetDirectoryName(targetPath);

						AddItemToProjectIfNeeded(project, targetPath);

						if (string.IsNullOrWhiteSpace(targetFolder))
						{
							continue;
						}

						if (!Directory.Exists(targetFolder))
						{
							Directory.CreateDirectory(targetFolder);
						}

						File.Copy(file, targetPath);
					}

					result = new Response
					{
						Name = request.Name,
						Owner = request.Owner,
						Destination = Path.Combine(manifest.Destination, manifest.Name),
						Url = $"https://github.com/{request.Owner}/{request.Name}/blob/{request.Ref ?? "master"}/README.md"
					};
					project?.Save();
					return result;
				}
				finally
				{
					if (Directory.Exists(tmpFolderName))
					{
						Directory.Delete(tmpFolderName, true);
					}
				}
			}

			private static void AddItemToProjectIfNeeded(Project project, string targetPath)
			{
				if (project == null || project.Items.Any(p => p.EvaluatedInclude.Equals(targetPath)))
				{
					return;
				}

				var refPath = targetPath.Replace("/", "\\");
				project.AddItem(
					Path.GetExtension(targetPath).Equals(".cs", StringComparison.InvariantCultureIgnoreCase)
						? "Compile"
						: "Content", refPath);
			}

			private static ComponentManifest GetManifest(string componentRootFolder)
			{
				var manifestFilePath = Path.Combine(componentRootFolder, "manifest.json");
				if (File.Exists(manifestFilePath))
				{
					return JsonConvert.DeserializeObject<ComponentManifest>(File.ReadAllText(manifestFilePath));
				}

				return new ComponentManifest();
			}

			private async Task<string> GetProjectFilePath(CancellationToken cancellationToken)
			{
				var projFiles = Directory.GetFiles(".", "*.csproj");
				return await _chooseOptionsService.Choose(projFiles, cancellationToken);
			}

			private async Task<byte[]> DownloadZip(string url, CancellationToken cancellationToken)
			{
				var client = new RestClient(url)
				{
					FollowRedirects = true
				};

				var archiveRequest = new RestRequest(Method.GET);
				var archive = await client.ExecuteTaskAsync(archiveRequest, cancellationToken);

				return archive.RawBytes;
			}
		}
	}
}
