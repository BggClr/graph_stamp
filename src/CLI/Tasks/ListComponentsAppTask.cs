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
					Owner = "Owner",
					Name = "Name",
					Categories = new [] {"Categories"},
					Url = "Url"
				}
			}.Union(response.Repos);

			foreach (var repo in result)
			{
				Console.WriteLine($"{repo.Owner,-15}{repo.Name,-50}{string.Join(", ", repo.Categories),-100}{repo.Url}");
			}
		}
	}
}
