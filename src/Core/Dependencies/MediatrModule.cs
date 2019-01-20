using System;
using Grace.DependencyInjection;
using MediatR;
using MediatR.Pipeline;
using Stamp.Services;

namespace Stamp.Dependencies
{
	public class MediatrModule : IConfigurationModule
	{
		public void Configure(IExportRegistrationBlock builder)
		{
			builder.Export<Mediator>()
				.As<IMediator>()
				.Lifestyle.Singleton();

			var mediatrOpenTypes = new[]
			{
				typeof(IRequestHandler<,>),
				typeof(IRequestPostProcessor<,>)
			};

			foreach (var mediatrOpenType in mediatrOpenTypes)
			{
				builder.ExportAssemblyContaining<GithubAuthentication>()
					.ByInterface(mediatrOpenType);
			}

			builder.ExportInstance<ServiceFactory>((scope, context) => scope.Locate);
		}
	}
}
