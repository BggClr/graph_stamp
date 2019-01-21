using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Models;
using Stamp.Services.Components;
using Stamp.Services.Settings;

namespace Stamp.CLI.Tasks
{
	public class FetchComponentAppTask : BaseTask<Fetch.Query>
	{
		private readonly IMediator _mediator;

		public FetchComponentAppTask(IMediator mediator, SettingsStorage settingsStorage) : base(settingsStorage)
		{
			_mediator = mediator;
		}

		public Role Role => Role.User;

		protected override async Task ExecuteInternal(Fetch.Query args)
		{
			await _mediator.Send(args);
		}
	}
}
