using System.Threading.Tasks;
using CommandDotNet.Attributes;
using Stamp.CLI.Tasks;
using Stamp.Services.Components;

namespace Stamp.CLI
{
	[ApplicationMetadata(Name = "stamp")]
	public class App
	{
		[SubCommand]
		public class Component
		{
			[InjectProperty]
			public ListComponentsAppTask ListComponentsAppTask { get; set; }
			[InjectProperty]
			public FetchComponentAppTask FetchComponentAppTask { get; set; }

			public async Task List(
				[Option(ShortName = "q", LongName = "query", Description = "Query the component name")]string name
				)
			{
				await ListComponentsAppTask.Execute(new List.Query
				{
					Keyword = name
				});
			}

			public async Task Fetch(
				[Argument(Name = "Owner", Description = "Component owner")]string owner,
				[Argument(Name = "name", Description = "Component name")]string name,
				[Option(ShortName = "r", LongName = "ref", Description = "Component alteration (branch, tag or commit hash)")] string reference
				)
			{
				await FetchComponentAppTask.Execute(new Fetch.Query
				{
					Name = name,
					Owner = owner,
					Ref = reference
				});
			}
		}
	}
}
