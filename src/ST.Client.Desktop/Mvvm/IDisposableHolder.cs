using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Mvvm
{
	public interface IDisposableHolder : IDisposable
	{
		ICollection<IDisposable> CompositeDisposable { get; }
	}
}
