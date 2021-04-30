using CefNet.Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace CefNet.Internal
{
	public interface IAvaloniaWebViewPrivate : IChromiumWebViewPrivate
	{
		void RaiseCefCursorChange(CursorChangeEventArgs e);
		void CefSetToolTip(string text);
		void CefSetStatusText(string statusText);
		void RaiseStartDragging(StartDraggingEventArgs e);
	}
}
