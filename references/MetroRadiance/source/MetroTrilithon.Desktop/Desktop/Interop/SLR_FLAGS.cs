using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace MetroTrilithon.Desktop.Interop
{
	/// <summary>
	/// IShellLink.Resolve fFlags.
	/// </summary>
	[Flags]
	public enum SLR_FLAGS
	{
		/// <summary>
		/// Do not display a dialog box if the link cannot be resolved. When SLR_NO_UI is set,
		/// the high-order word of fFlags can be set to a time-out value that specifies the 
		/// maximum amount of time to be spent resolving the link. The function returns if the
		/// link cannot be resolved within the time-out duration. If the high-order word is set
		/// to zero, the time-out duration will be set to the default value of 3,000 milliseconds
		/// (3 seconds). To specify a value, set the high word of fFlags to the desired time-out
		/// duration, in milliseconds.
		/// </summary>
		SLR_NO_UI = 0x01,

		/// <summary>
		/// Obsolete and no longer used.
		/// </summary>
		SLR_ANY_MATCH = 0x02,

		/// <summary>
		/// If the link object has changed, update its path and list of identifiers. 
		/// If SLR_UPDATE is set, you do not need to call IPersistFile::IsDirty to determine 
		/// whether or not the link object has changed.
		/// </summary>
		SLR_UPDATE = 0x04,

		/// <summary>
		/// Do not update the link information.
		/// </summary>
		SLR_NOUPDATE = 0x08,

		/// <summary>
		/// Do not execute the search heuristics.
		/// </summary>
		SLR_NOSEARCH = 0x10,

		/// <summary>
		/// Do not use distributed link tracking.
		/// </summary>
		SLR_NOTRACK = 0x20,

		/// <summary>
		/// Disable distributed link tracking. By default, distributed link tracking tracks
		/// removable media across multiple devices based on the volume name. It also uses the
		/// Universal Naming Convention (UNC) path to track remote file systems whose drive letter
		/// has changed. Setting SLR_NOLINKINFO disables both types of tracking.
		/// </summary>
		SLR_NOLINKINFO = 0x40,

		/// <summary>
		/// Call the Microsoft Windows Installer.
		/// </summary>
		SLR_INVOKE_MSI = 0x80,
	}
}
