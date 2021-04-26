using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using CefNet.Input;
using CefNet.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CefNet.Avalonia
{
	public static class CefNetAvaloniaExtensions
	{
#pragma warning disable IDE0060
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void Invoke<TEventArgs>(this RoutedEvent routedEvent, Interactive sender, TEventArgs e)
			where TEventArgs : class
		{
			sender.RaiseEvent(e as RoutedEventArgs);
		}
#pragma warning restore IDE0060

		/// <summary>
		/// Converts the specified <see cref="Key"/> into a virtual key.
		/// </summary>
		/// <param name="key">The key code.</param>
		/// <returns>The virtual key code.</returns>
		public static VirtualKeys ToVirtualKey(this Key key)
		{
			if (key >= Key.LeftShift && key <= Key.RightAlt)
				return (VirtualKeys)((key - Key.LeftShift) >> 1) | VirtualKeys.ShiftKey; // VK_SHIFT, VK_CONTROL, VK_MENU
			if (key == Key.System)
				return VirtualKeys.Menu; // VK_MENU
			return (VirtualKeys)KeyInterop.VirtualKeyFromKey(key);
		}

		public static Color ToColor(this CefColor color)
		{
			return Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		/// <summary>
		/// Converts a drag drop effects to the CEF dragging operation mask.
		/// </summary>
		/// <param name="self">The drag drop effects.</param>
		/// <returns></returns>
		public static CefDragOperationsMask ToCefDragOperationsMask(this DragDropEffects self)
		{
			CefDragOperationsMask effects = CefDragOperationsMask.None;
			if (self.HasFlag(DragDropEffects.Copy))
				effects |= CefDragOperationsMask.Copy;
			if (self.HasFlag(DragDropEffects.Move))
				effects |= CefDragOperationsMask.Move;
			if (self.HasFlag(DragDropEffects.Link))
				effects |= CefDragOperationsMask.Link;
			return effects;
		}

		/// <summary>
		/// Converts the CEF dragging operation mask to drag drop effects.
		/// </summary>
		/// <param name="self">The CEF dragging operation mask.</param>
		/// <returns></returns>
		public static DragDropEffects ToDragDropEffects(this CefDragOperationsMask self)
		{
			DragDropEffects effects = DragDropEffects.None;
			if (self.HasFlag(CefDragOperationsMask.Every))
				effects |= DragDropEffects.Copy | DragDropEffects.Link | DragDropEffects.Move;
			if (self.HasFlag(CefDragOperationsMask.Copy))
				effects |= DragDropEffects.Copy;
			if (self.HasFlag(CefDragOperationsMask.Move))
				effects |= DragDropEffects.Move;
			if (self.HasFlag(CefDragOperationsMask.Link))
				effects |= DragDropEffects.Link;
			return effects;
		}

		/// <summary>
		/// Gets the current state of the SHIFT, CTRL, and ALT keys, as well as the state of the mouse buttons.
		/// </summary>
		/// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
		/// <returns></returns>
		public static CefEventFlags GetModifiers(this DragEventArgs e)
		{
#pragma warning disable CS0618 // Obsoleted InputModifiers
			CefEventFlags flags = CefEventFlags.None;
			InputModifiers state = e.Modifiers;
			if (state.HasFlag(InputModifiers.LeftMouseButton))
				flags |= CefEventFlags.LeftMouseButton;
			if (state.HasFlag(InputModifiers.RightMouseButton))
				flags |= CefEventFlags.RightMouseButton;
			if (state.HasFlag(InputModifiers.Shift))
				flags |= CefEventFlags.ShiftDown;
			if (state.HasFlag(InputModifiers.Control))
				flags |= CefEventFlags.ControlDown;
			if (state.HasFlag(InputModifiers.MiddleMouseButton))
				flags |= CefEventFlags.MiddleMouseButton;
			if (state.HasFlag(InputModifiers.Alt))
				flags |= CefEventFlags.AltDown;
			if (state.HasFlag(InputModifiers.Windows))
				flags |= CefEventFlags.CommandDown;
			return flags;
#pragma warning restore CS0618
		}

		/// <summary>
		/// Gets the drag data.
		/// </summary>
		/// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
		public static CefDragData GetCefDragData(this DragEventArgs e)
		{
			CefDragData dragData;
			if (e.Data.Contains(nameof(CefDragData)))
			{
				dragData = (CefDragData)e.Data.Get(nameof(CefDragData));
				if (dragData != null)
				{
					dragData.ResetFileContents();
					return dragData;
				}
			}

			dragData = new CefDragData();

			string[] formats = e.Data.GetDataFormats().ToArray();

			if (TryGetFileDropData(e.Data, out IEnumerable<string> fileNames))
			{
				foreach (string filePath in fileNames)
				{
					dragData.AddFile(filePath.Replace("\\", "/"), Path.GetFileName(filePath));
				}
			}
			

			bool isUrl = false;
			string s = GetUrlString(e.Data, formats);
			if (!string.IsNullOrWhiteSpace(s))
			{
				isUrl = true;
				dragData.LinkUrl = s;
			}

			if (formats.Contains(CefNetDragData.DataFormatUnicodeText))
			{
				s = (string)e.Data.Get(CefNetDragData.DataFormatUnicodeText);

				if (!isUrl && Uri.IsWellFormedUriString(s, UriKind.Absolute))
					dragData.LinkUrl = s;
				dragData.FragmentText = s;
			}
			else if (formats.Contains(DataFormats.Text))
			{
				s = (string)e.Data.Get(DataFormats.Text);

				if (!isUrl && Uri.IsWellFormedUriString(s, UriKind.Absolute))
					dragData.LinkUrl = s;
				dragData.FragmentText = s;
			}

			if (formats.Contains(CefNetDragData.DataFormatHtml))
			{
				dragData.FragmentHtml = Encoding.UTF8.GetString((byte[])e.Data.Get(CefNetDragData.DataFormatHtml));
			}
			else if (formats.Contains(CefNetDragData.DataFormatTextHtml))
			{
				dragData.FragmentHtml = Encoding.UTF8.GetString((byte[])e.Data.Get(CefNetDragData.DataFormatTextHtml));
			}

			return dragData;
		}

		private static bool TryGetFileDropData(IDataObject data, out IEnumerable<string> fileNames)
		{
			if (data.Contains(CefNetDragData.DataFormatFileDrop))
				fileNames = (IEnumerable<string>)data.Get(CefNetDragData.DataFormatFileDrop);
			else if (data.Contains(CefNetDragData.DataFormatFileNames))
				fileNames = (IEnumerable<string>)data.Get(CefNetDragData.DataFormatFileNames);
			else
				fileNames = null;
			return fileNames != null;
		}

		private static string GetUrlString(IDataObject data, string[] formats)
		{
			if (formats.Contains(CefNetDragData.DataFormatUnicodeUrl))
				return Encoding.Unicode.GetString((byte[])data.Get(CefNetDragData.DataFormatUnicodeUrl));

			if (formats.Contains(CefNetDragData.DataFormatUrl))
				return Encoding.ASCII.GetString((byte[])data.Get(CefNetDragData.DataFormatUrl));

			return null;
		}

	}
}
