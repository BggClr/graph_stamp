using Grace.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Stamp.Services.Settings;

namespace Stamp.Dependencies
{
	public class Bootstrap
	{
		public static DependencyInjectionContainer CreateContainerBuilder()
		{
			var container = new DependencyInjectionContainer();

			container.Configure(new MediatrModule());
			container.Configure(new MapperModule());

			return container;
		}

		public static T GetSettings<T>() where T : new()
		{
			var homePath = SettingsStorage.SettingsFilePath;

			var configurationBuilder = new ConfigurationBuilder()
				.AddJsonFile(SettingsStorage.SettingsFilePath, true);

			var configuration = configurationBuilder.Build();

			var settings = new T();
			configuration.Bind(settings);

			return settings;
		}
	}
}
