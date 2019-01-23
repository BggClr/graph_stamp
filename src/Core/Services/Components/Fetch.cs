using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Build.Evaluation;
using Models;
using Newtonsoft.Json;
using RestSharp;

namespace Stamp.Services.Components
{
	public class Fetch
	{
		public class Query : IRequest
		{
			public string Category { get; set; }
			public string Name { get; set; }
			public string Ref { get; set; }
		}

		public class Model
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public string Category { get; set; }
			public string Url { get; set; }
		}

		public class MappingProfile : Profile
		{
			public MappingProfile()
			{
				CreateMap<Repository, Model>()
					.ForMember(d => d.Category, o => o.MapFrom(s => ComponentNameConverter.GetCategory(s.Name)))
					.ForMember(d => d.Name, o => o.MapFrom(s => ComponentNameConverter.GetName(s.Name)));
			}
		}

		public class QueryHandler : IRequestHandler<Query>
		{
			private readonly GithubAuthentication _githubAuthentication;
			private readonly IMapper _mapper;

			public QueryHandler(GithubAuthentication githubAuthentication, IMapper mapper)
			{
				_githubAuthentication = githubAuthentication;
				_mapper = mapper;
			}

			public async Task<Unit> Handle(Query request, CancellationToken cancellationToken)
			{
				var component = await GetComponent(request, cancellationToken);
				var url = $"{component.Url.Trim('/')}/zipball";
				var localComponentName =ComponentNameConverter.GetLocalName(component.Name);

				if (!string.IsNullOrEmpty(request.Ref))
				{
					url = $"{url}/{request.Ref}";
				}

				var archive = await DownloadZip(url, cancellationToken);

				DeployComponent(archive, localComponentName);

				return Unit.Value;
			}

			private static void DeployComponent(byte[] archive, string localComponentName)
			{
				var tmpFolderName = Guid.NewGuid().ToString();
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
					project = new Project(GetProjectFilePath(), null, null, projectCollection, ProjectLoadSettings.IgnoreMissingImports);
				}

				foreach (var file in files)
				{
					var relativePath = folderUri.MakeRelativeUri(new Uri(file));
					var targetPath = Path.Combine(manifest.Destination, localComponentName, relativePath.ToString());
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

				project?.Save();

				if (Directory.Exists(tmpFolderName))
				{
					Directory.Delete(tmpFolderName, true);
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

			private static string GetProjectFilePath()
			{
				return Directory.GetFiles(".", "*.csproj").FirstOrDefault();
			}

			private async Task<Model> GetComponent(Query request, CancellationToken cancellationToken)
			{
				var client = new RestClient("https://api.github.com/user/repos")
				{
					FollowRedirects = true
				};

				var reposRequest = new RestRequest(Method.GET);
				reposRequest.AddHeader("Authorization", _githubAuthentication.GetAuthorizationHeader());
				var repos = await client.ExecuteTaskAsync<List<Repository>>(reposRequest, cancellationToken);

				var matchingComponents = repos.Data
					.Select(_mapper.Map<Model>)
					.Where(p => p.Category.Equals(request.Category, StringComparison.InvariantCultureIgnoreCase))
					.Where(p => p.Name.Equals(request.Name, StringComparison.InvariantCultureIgnoreCase))
					.ToArray();

				if (matchingComponents.Count() > 1)
				{
					throw new Exception();
				}

				if (!matchingComponents.Any())
				{
					throw new Exception();
				}

				return matchingComponents.First();
			}

			private async Task<byte[]> DownloadZip(string url, CancellationToken cancellationToken)
			{
				var client = new RestClient(url)
				{
					FollowRedirects = true
				};

				var archiveRequest = new RestRequest(Method.GET);
				archiveRequest.AddHeader("Authorization", _githubAuthentication.GetAuthorizationHeader());
				var archive = await client.ExecuteTaskAsync(archiveRequest, cancellationToken);

				return archive.RawBytes;
			}
		}
	}
}
