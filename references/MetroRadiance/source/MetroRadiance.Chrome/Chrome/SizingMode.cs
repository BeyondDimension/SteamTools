using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetroRadiance.Chrome
{
	public enum SizingMode
	{
		None = 0,

		// HitTestValues 互換
		Left = 10,
		Top = 12,
		Right = 11,
		Bottom = 15,
		TopLeft = 13,
		TopRight = 14,
		BottomLeft = 16,
		BottomRight = 17,
	}
}
