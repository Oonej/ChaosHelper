﻿using System;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace ChaosHelper
{
	public static class Globals
	{
		public static void Init(string pluginName, PluginHost host, CoreManager core)
		{
			PluginName = pluginName;

			Host = host;

			Core = core;
		}

		public static string PluginName { get; private set; }

		public static PluginHost Host { get; private set; }

		public static CoreManager Core { get; private set; }
	}
}
