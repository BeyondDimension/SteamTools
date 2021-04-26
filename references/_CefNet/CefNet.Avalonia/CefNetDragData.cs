using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CefNet.Avalonia
{
	public class CefNetDragData : IDataObject
	{
		internal const string DataFormatUrl = "UniformResourceLocator";
		internal const string DataFormatUnicodeUrl = "UniformResourceLocatorW";
		internal const string DataFormatTextHtml = "text/html";
		internal const string DataFormatUnicodeText = "UnicodeText";
		internal const string DataFormatHtml = "HTML Format";
		internal const string DataFormatFileDrop = "FileDrop";
		internal const string DataFormatFileNames = "FileNames";

		public const string DataFormatCefNetDragData = nameof(CefNetDragData);
		public const string DataFormatCefDragData = nameof(CefDragData);

		private WeakReference<WebView> _source;
		private HashSet<string> _formats;

		/// <summary>
		/// Initializes a new instance of the <see cref="CefNetDragData"/> class.
		/// </summary>
		/// <param name="source">The source of the drag event.</param>
		/// <param name="data">The original drag data.</param>
		public CefNetDragData(WebView source, CefDragData data)
		{
			var formats = new HashSet<string>();
			formats.Add(DataFormatCefNetDragData);
			formats.Add(DataFormatCefDragData);
			if (data.IsFile)
			{
				formats.Add(DataFormatFileDrop);
				formats.Add(DataFormatFileNames);
			}
			if (data.IsLink)
			{
				formats.Add(DataFormatUnicodeUrl);
				formats.Add(DataFormatUnicodeText);
			}
			if (data.IsFragment)
			{
				formats.Add(DataFormatUnicodeText);
				formats.Add(DataFormatHtml);
				formats.Add(DataFormatTextHtml);
			}

			_source = new WeakReference<WebView>(source);
			_formats = formats;
			this.Data = data;
		}

		/// <summary>
		/// The original drag data.
		/// </summary>
		public CefDragData Data { get; }

		/// <summary>
		/// The source of the drag event.
		/// </summary>
		public WebView Source
		{
			get
			{
				if (_source.TryGetTarget(out WebView source))
					return source;
				return null;
			}
		}

		public bool Contains(string format)
		{
			return _formats.Contains(format);
		}

		public object Get(string format)
		{
			Debug.Print("GetData: " + format);
			string s;
			if (DataFormatUnicodeUrl.Equals(format, StringComparison.Ordinal))
			{
				s = Data.LinkUrl;
				return s is null ? null : new MemoryStream(Encoding.Unicode.GetBytes(s));
			}
			if (DataFormatUrl.Equals(format, StringComparison.Ordinal))
			{
				s = Data.LinkUrl;
				return s is null ? null : new MemoryStream(Encoding.ASCII.GetBytes(s));
			}
			if (DataFormatUnicodeText.Equals(format, StringComparison.Ordinal))
			{
				if (Data.IsLink)
					return Data.LinkUrl;
				return Data.FragmentText;
			}
			if (DataFormatHtml.Equals(format, StringComparison.Ordinal))
				return Data.FragmentHtml;
			if (DataFormats.Text.Equals(format, StringComparison.Ordinal))
				return Data.FragmentText;
			if (DataFormatTextHtml.Equals(format, StringComparison.Ordinal))
				return Data.FragmentHtml;
			if (DataFormatCefDragData.Equals(format, StringComparison.Ordinal))
				return Data;
			if (DataFormatCefNetDragData.Equals(format, StringComparison.Ordinal))
				return this;

			return null;
		}

		public IEnumerable<string> GetDataFormats()
		{
			return _formats;
		}

		public IEnumerable<string> GetFileNames()
		{
			using (var list = new CefStringList())
			{
				Data.GetFileNames(list);

				foreach (string s in list)
					yield return s;
			}
		}

		public string GetText()
		{
			if (Data.IsLink)
				return Data.LinkUrl;
			return Data.FragmentText;
		}
	}
}
