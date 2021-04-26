using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CefNet.Avalonia
{
	/// <summary>
	/// Provides data for the <see cref="WebView.StartDragging"/> event.
	/// </summary>
	public class StartDraggingEventArgs : RoutedEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="StartDraggingEventArgs"/> class.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="data">The data associated with this event.</param>
		/// <param name="allowedOps">Drag-and-drop operations which allowed by the source of the drag event.</param>
		/// <param name="x">The X-location in screen coordinates.</param>
		/// <param name="y">The Y-location in screen coordinates.</param>
		public StartDraggingEventArgs(CefDragData data, CefDragOperationsMask allowedOps, int x, int y)
			: base(WebView.StartDraggingEvent)
		{
			this.Data = data;
			this.AllowedEffects = allowedOps;
			this.X = x;
			this.Y = y;
		}

		/// <summary>
		/// The contextual information about the dragged content.
		/// </summary>
		public CefDragData Data { get; }

		/// <summary>
		/// Gets which drag-and-drop operations are allowed by the originator (or source) of the drag event.
		/// </summary>
		public CefDragOperationsMask AllowedEffects { get; }

		/// <summary>
		/// Gets the X-location of the mouse pointer in screen coordinates.
		/// </summary>
		public int X { get; }

		/// <summary>
		/// Gets the Y-location of the mouse pointer in screen coordinates.
		/// </summary>
		public int Y { get; }

	}
}
