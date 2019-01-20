using CommandDotNet;
using Models;
using Stamp.Dependencies;
using Stamp.Services.Settings;

namespace Stamp.CLI
{
	internal static class Program
	{

		private static void Main(string[] args)
		{
			var container = Bootstrap.CreateContainerBuilder();

			container.Configure(c =>
				c.ExportInstance(new SettingsStorage(Bootstrap.GetSettings<Settings>()))
					.As<SettingsStorage>()
					.Lifestyle.Singleton()
				);

			var appRunner = new AppRunner<App>(new CommandDotNet.Models.AppSettings
			{
				Case = Case.LowerCase
			}).UseGrace(container);

			appRunner.Run(args);
		}
	}
}
