using System;
using System.Collections.Generic;
using System.Text;

namespace SteamTool.Composition.Interface
{
	[PluginFeature]
	public interface ITool
	{
		string Name { get; }

		object View { get; }
	}
}
