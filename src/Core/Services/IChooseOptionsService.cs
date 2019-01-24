using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stamp.Services
{
	public interface IChooseOptionsService
	{
		Task<T> Choose<T>(IList<T> options, CancellationToken cancellationToken);
	}
}
