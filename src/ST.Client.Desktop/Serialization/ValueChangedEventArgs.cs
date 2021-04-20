using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.Serialization
{
	public class ValueChangedEventArgs<T> : EventArgs
	{
		public T OldValue { get; }
		public T NewValue { get; }

		public ValueChangedEventArgs(T oldValue, T newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}
	}
}