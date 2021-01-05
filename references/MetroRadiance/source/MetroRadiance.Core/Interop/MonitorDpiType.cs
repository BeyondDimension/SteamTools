using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetroRadiance.Interop
{
	/// <summary>
	/// Identifies dots per inch (dpi) type.
	/// </summary>
	public enum MonitorDpiType
	{
		/// <summary>
		/// MDT_Effective_DPI
		/// <para>Effective DPI that incorporates accessibility overrides and matches what Desktop Window Manage (DWM) uses to scale desktop applications.</para>
		/// </summary>
		EffectiveDpi = 0,

		/// <summary>
		/// MDT_Angular_DPI
		/// <para>DPI that ensures rendering at a compliant angular resolution on the screen, without incorporating accessibility overrides.</para>
		/// </summary>
		AngularDpi = 1,
		
		/// <summary>
		/// MDT_Raw_DPI
		/// <para>Linear DPI of the screen as measures on the screen itself.</para>
		/// </summary>
		RawDpi = 2,

		/// <summary>
		/// MDT_Default
		/// </summary>
		Default = EffectiveDpi,
	}
}
