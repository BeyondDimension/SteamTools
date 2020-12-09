using System;
using System.Collections.Generic;
using System.Text;

namespace SteamTool.Auth
{
	/// <summary>
	/// Exception class wrapper for importing error
	/// </summary>
	public class ImportException : ApplicationException
	{
		public ImportException() : base() { }

		public ImportException(string message) : base(message) { }

		public ImportException(string message, Exception ex) : base(message, ex) { }

	}
}
