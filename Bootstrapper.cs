using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace ckjp
{
	public class Bootstrapper : MonoBehaviour
	{
		public static Bootstrapper Instance { get; private set; }

		internal bool ForcePatching;

		public Bootstrapper(IntPtr pointer) : base(pointer) { }

		internal static void Setup()
		{
			ClassInjector.RegisterTypeInIl2Cpp<Bootstrapper>();

			var obj = new GameObject("JpPatchBootstrapper");
			DontDestroyOnLoad(obj);
			obj.hideFlags |= HideFlags.HideAndDontSave;

			Instance = obj.AddComponent<Bootstrapper>();
		}

		internal void Start()
		{
			BepInExLoader.Inst.Log.LogMessage("JpPatcher - Bootstrapped.");
			JapanesePatcher.Setup();
		}

		internal void Update()
		{
			//TODO: Patcher Update
			if (Input.GetKey(KeyCode.F8))
			{
				if (ForcePatching)
					return;
				ForcePatching = true;
				Start();
			}
		}
	}
}
