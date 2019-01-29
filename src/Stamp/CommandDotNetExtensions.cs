using CommandDotNet;
using Grace.DependencyInjection;

namespace Stamp
{
	public static class CommandDotNetExtensions
	{
		public static AppRunner<T> UseGrace<T>(this AppRunner<T> appRunner, DependencyInjectionContainer container) where T :class
		{
			appRunner.UseDependencyResolver(new DependencyResolver(container));
			return appRunner;
		}
	}
}
