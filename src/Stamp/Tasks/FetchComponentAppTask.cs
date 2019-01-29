using System;
using System.Threading.Tasks;
using MediatR;
using Models;
using Stamp.Services.Components;
using Stamp.Services.Settings;

namespace Stamp.Tasks
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
			var result = await _mediator.Send(args);
			Console.Write("\nComponent ");
			ColorWrite($"{result.Owner}/{result.Name}", ConsoleColor.Cyan);
			Console.Write($" has been fetched to the ");
			ColorWrite(result.Destination, ConsoleColor.Cyan);
			Console.WriteLine($" folder");
			Console.Write($"For more info about the component please read ");
			ColorWrite(result.Url, ConsoleColor.Blue);
			Console.WriteLine();
		}

		private void ColorWrite(string text, ConsoleColor color)
		{
			var currentColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.Write(text);
			Console.ForegroundColor = currentColor;
		}
	}
}
