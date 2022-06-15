using System;
using System.Collections.Generic;
using UnhollowerRuntimeLib;
using UnityEngine;
using System.Linq;
using HarmonyLib;
using I2.Loc;
using Il2CppSystem.Net;
using Il2CppSystem.IO;
using Il2CppSystem.Text;
using BepInEx.Configuration;
using System.Reflection;

namespace ckjp
{
	public class JapanesePatcher : MonoBehaviour
	{
		private static ConfigEntry<bool> _isFirstRun;
		private static ConfigEntry<bool> _isIgnoreItemName;
		private static ConfigEntry<string> _sheetFileUrl;

		internal static void Setup()
		{
			_isFirstRun = BepInExLoader.Inst.Config.Bind("General", "IsFirstRun", true, "初回起動を認識するためのオプションです。有効(true)にすると１回だけ強制的に日本語に切り替わります");
			_isIgnoreItemName = BepInExLoader.Inst.Config.Bind("General", "IsIgnoreItemName", false, "アイテム名を日本語化しないようにします。変更後の適用にはゲームの再起動が必要です");
			_sheetFileUrl = BepInExLoader.Inst.Config.Bind("General", "SheetFileUrl", "https://docs.google.com/spreadsheets/d/1csBM-ZqZtG_z_JdLaFvGHHy8UABZdxRRdT_ShJM5zTE/export?gid=0&format=tsv", "翻訳に使用するシートを指定します。tsvフォーマットである必要があります");
			BepInExLoader.Inst.Log.LogMessage("Japanese Patcher Injected.");
			ClassInjector.RegisterTypeInIl2Cpp<JapanesePatcher>();

			var obj = new GameObject("JapanesePatcher");
			DontDestroyOnLoad(obj);
			obj.hideFlags |= HideFlags.HideAndDontSave;
			obj.AddComponent<JapanesePatcher>();
		}
		private Dictionary<string, string> japaneses;

		internal void Start()
		{
			BepInExLoader.Inst.Log.LogMessage(">>>>>>> Japanese patching... <<<<<<<<<<<");

			var request = WebRequest.Create(_sheetFileUrl.Value);
			request.Method = "Get";
			WebResponse response;
			BepInExLoader.Inst.Log.LogMessage("Downloading...");
			try
			{
				response = request.GetResponse();
			}
			catch
			{
				response = null;
			}

			if (response == null)
				return;

			var st = response.GetResponseStream();
			var sr = new StreamReader(st, Encoding.GetEncoding("UTF-8"));
			string txt = sr.ReadToEnd();
			BepInExLoader.Inst.Log.LogMessage("Downloaded.");

			var rows = txt.Split("\r\n");
			japaneses = new Dictionary<string, string>();
			foreach (var row in rows.Skip(1).Select(row => row.Split("\t")).Where(row => !string.IsNullOrEmpty(row[2])))
			{
				if (row[2][0] != '\'')
					japaneses[row[0]] = row[2];
				else
					japaneses[row[0]] = row[2].Remove(0, 1);
			}

			sr.Close();
			st.Close();

			if (LocalizationManager.Sources.Count == 0)
				LocalizationManager.UpdateSources();

			var pred = delegate (I2.Loc.LanguageData x) { return x.Code == "ja"; };
			var jaLang = I2.Loc.LocalizationManager.Sources[0].mLanguages.Find(pred);
			var jaLangIndex = I2.Loc.LocalizationManager.Sources[0].mLanguages.IndexOf(jaLang);

			foreach (var term in I2.Loc.LocalizationManager.Sources[0].mTerms)
			{
				if (term.TermType != I2.Loc.eTermType.Text)
					continue;
				if (!japaneses.ContainsKey(term.Term))
					continue;

				if (string.IsNullOrWhiteSpace(japaneses[term.Term]))
					continue;

				if (_isIgnoreItemName.Value && term.Term.StartsWith("Items/") && !term.Term.EndsWith("Desc"))
					continue;

				term.SetTranslation(jaLangIndex, japaneses[term.Term]);
			}

			if (_isIgnoreItemName.Value)
			{
				foreach (var term in I2.Loc.LocalizationManager.Sources[0].mTerms)
				{
					if (!(term.Term.StartsWith("Items/") && !term.Term.EndsWith("Desc")))
						continue;

					term.SetTranslation(jaLangIndex, "");
				}
			}

			jaLang.Flags = 0;
			I2.Loc.LocalizationManager.Sources[0].UpdateDictionary();

			if (_isFirstRun.Value)
			{
				BepInExLoader.Inst.Log.LogMessage(">>>>>>> 初回起動のため強制的に日本語にします <<<<<<<<<<<");
				_isFirstRun.SetSerializedValue(false.ToString());
				I2.Loc.LocalizationManager.CurrentLanguage = "japanese";
				Bootstrapper.Instance.ForcePatching = true;
			}

			if (Bootstrapper.Instance.ForcePatching)
			{
				var texts = FindObjectsOfType<PugText>();
				foreach (var text in texts)
				{
					if (text.localize)
						text.Render(false);
				}
				Bootstrapper.Instance.ForcePatching = false;
			}

			BepInExLoader.Inst.Log.LogMessage(">>>>>>> Finished japanese patch <<<<<<<<<<<");
			Destroy(gameObject);
		}

		private static byte[] ReadFully(System.IO.Stream input)
		{
			using var ms = new System.IO.MemoryStream();
			byte[] buffer = new byte[81920];
			int read;
			while ((read = input.Read(buffer, 0, buffer.Length)) != 0)
				ms.Write(buffer, 0, read);
			return ms.ToArray();
		}
	}
}
