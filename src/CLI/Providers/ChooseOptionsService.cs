using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using Stamp.Services;

namespace Stamp.CLI.Providers
{
	public class ChooseOptionsService : IChooseOptionsService
	{
		public Task<T> Choose<T>(IList<T> options, CancellationToken cancellationToken)
		{
			if (!options.Any())
			{
				throw new Exception();
			}

			if (options.Count == 1)
			{
				return Task.FromResult(options.First());
			}

			var optionsIndexed = options.Select((p, i) => (Index: i + 1, Value: p)).ToList();

			while (true)
			{
				Console.WriteLine("Please enter the option number to proceed or 0 to cancel");
				foreach (var option in optionsIndexed)
				{
					Console.WriteLine($"{option.Index}.\t{option.Value}");
				}
				int selectedOptionIndex;
				try
				{
					selectedOptionIndex = int.Parse(Console.ReadLine());
				}
				catch
				{
					Console.WriteLine("The value entered is not valid.");
					continue;
				}

				if (selectedOptionIndex == 0)
				{
					throw new OperationCanceledException (cancellationToken);
				}

				if (optionsIndexed.Any(p => p.Index == selectedOptionIndex))
				{
					var selectedOption = optionsIndexed.First(p => p.Index == selectedOptionIndex).Value;
					return Task.FromResult(selectedOption);
				}
			}
		}
	}
}
