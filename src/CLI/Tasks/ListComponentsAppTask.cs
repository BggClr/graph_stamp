using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Models;
using Stamp.Services.Components;
using Stamp.Services.Settings;

namespace Stamp.CLI.Tasks
{
	public class ListComponentsAppTask : BaseTask<List.Query>
	{
		private readonly IMediator _mediator;

		public ListComponentsAppTask(IMediator mediator, SettingsStorage settingsStorage) : base(settingsStorage)
		{
			_mediator = mediator;
		}

		public Role Role => Role.User;

		protected override async Task ExecuteInternal(List.Query args)
		{
			var response = await _mediator.Send(args);
			var result = new[]
			{
				new List.Model
				{
					Category = "Category",
					Name = "Name",
					Url = "Url"
				}
			}.Union(response.Repos);

			foreach (var repo in result)
			{
				Console.WriteLine($"{repo.Category,-15}\t\t{repo.Name,-50}\t\t{repo.Url}");
			}
		}
	}
}
