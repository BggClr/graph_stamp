using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Grace.DependencyInjection;
using Stamp.Services;

namespace Stamp.Dependencies
{
	public class MapperModule : IConfigurationModule
	{
		public void Configure(IExportRegistrationBlock builder)
		{
			var profiles = typeof(GithubAuthentication)
				.Assembly
				.GetTypes()
				.Where(p => typeof(Profile).IsAssignableFrom(p));

			foreach (var profile in profiles)
			{
				builder.Export(profile).As(typeof(Profile));
			}

			builder.ExportInstance((scope, context) => new MapperConfiguration(cfg =>
				{
					cfg.ConstructServicesUsing(scope.Locate);

					foreach (var profile in scope.Locate<IEnumerable<Profile>>())
					{
						cfg.AddProfile(profile);
					}
				}))
				.As<MapperConfiguration>()
				.Lifestyle.Singleton();

			builder.ExportInstance((scope, context) => new Mapper(scope.Locate<MapperConfiguration>()))
				.As<IMapper>()
				.Lifestyle.Singleton();
		}
	}
}
