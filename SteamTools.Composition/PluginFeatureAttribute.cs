using System;
using System.Collections.Generic;
using System.Text;

namespace SteamTool.Composition
{
	[AttributeUsage(AttributeTargets.Interface)]
	public sealed class PluginFeatureAttribute : Attribute { }
}
