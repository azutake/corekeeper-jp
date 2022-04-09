using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.IL2CPP.Utils.Collections;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Networking;

namespace ckjp
{
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public class BepInExLoader : BasePlugin
	{
		public static BepInExLoader Inst { get; private set; }

		public override void Load()
		{
			// Plugin startup logic
			Inst = this;

			Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

			Bootstrapper.Setup();
		}
	}
}
