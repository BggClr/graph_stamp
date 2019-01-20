using System;
using CommandDotNet;
using Grace.DependencyInjection;

namespace Stamp.CLI
{
	public class DependencyResolver : IDependencyResolver
	{
		private readonly DependencyInjectionContainer _container;

		public DependencyResolver(DependencyInjectionContainer container)
		{
			_container = container;
		}

		public object Resolve(Type type)
		{
			return _container.Locate(type);
		}
	}
}
