using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetroTrilithon.Lifetime
{
	public interface IDisposableHolder : IDisposable
	{
		ICollection<IDisposable> CompositeDisposable { get; }
	}
}
