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
				[Option(ShortName = "c", LongName = "category", Description = "Component category")]string category,
				[Option(ShortName = "q", LongName = "query", Description = "Query the component name")]string name
				)
			{
				await ListComponentsAppTask.Execute(new List.Query
				{
					Keyword = name,
					Category = category
				});
			}

			public async Task Fetch(
				[Option(ShortName = "c", LongName = "category", Description = "Component category")]string category,
				[Option(ShortName = "n", LongName = "name", Description = "The component name")]string name
				)
			{
				await FetchComponentAppTask.Execute(new Fetch.Query
				{
					Name = name,
					Category = category
				});
			}
		}
	}
}
