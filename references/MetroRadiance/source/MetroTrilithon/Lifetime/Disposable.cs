using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetroTrilithon.Lifetime
{
	public static class Disposable
	{
		public static IDisposable Create(Action dispose)
		{
			return new AnonymousDisposable(dispose);
		}

		private class AnonymousDisposable : IDisposable
		{
			private bool _isDisposed;
			private readonly Action _dispose;

			public AnonymousDisposable(Action dispose)
			{
				this._dispose = dispose;
			}

			public void Dispose()
			{
				if (this._isDisposed) return;

				this._isDisposed = true;
				this._dispose();
			}
		}
	}
}
