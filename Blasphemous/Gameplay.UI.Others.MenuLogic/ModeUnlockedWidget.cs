using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DG.Tweening;
using Framework.Managers;
using FullSerializer;
using Rewired;
using UnityEngine;

namespace Gameplay.UI.Others.MenuLogic;

public class ModeUnlockedWidget : BaseMenuScreen
{
	public enum ModesToUnlock
	{
		BossRush
	}

	private readonly Dictionary<ModesToUnlock, string> APP_SETTINGS_MODES_UNLOCKED_KEYS = new Dictionary<ModesToUnlock, string> { 
	{
		ModesToUnlock.BossRush,
		"bossrush_unlocked"
	} };

	public bool isOpen;

	private CanvasGroup canvasGroup;

	private Player rewiredPlayer;

	private List<ModesToUnlock> unlockedModes = new List<ModesToUnlock>();

	public void Open(ModesToUnlock modeUnlocked)
	{
		string pathAppSettings = GetPathAppSettings();
		if (!File.Exists(pathAppSettings))
		{
			File.CreateText(pathAppSettings).Close();
		}
		else
		{
			ReadModesUnlocked(pathAppSettings);
		}
		if (!unlockedModes.Contains(modeUnlocked))
		{
			Open();
			unlockedModes.Add(modeUnlocked);
			WriteFileAppSettings(pathAppSettings);
		}
	}

	public override void Open()
	{
		base.Open();
		UIController.instance.StartCoroutine(DelayedOpen());
	}

	private IEnumerator DelayedOpen()
	{
		yield return new WaitForSeconds(0.1f);
		yield return new WaitUntil(() => !UIController.instance.IsPatchNotesShowing());
		base.gameObject.SetActive(value: true);
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		DOTween.To(() => canvasGroup.alpha, delegate(float x)
		{
			canvasGroup.alpha = x;
		}, 1f, 1f).OnComplete(OnOpen);
	}

	protected override void OnOpen()
	{
		isOpen = true;
		rewiredPlayer = ReInput.players.GetPlayer(0);
	}

	public override void Close()
	{
		base.Close();
		DOTween.To(() => canvasGroup.alpha, delegate(float x)
		{
			canvasGroup.alpha = x;
		}, 0f, 0.2f).OnComplete(OnClose);
	}

	protected override void OnClose()
	{
		isOpen = false;
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (rewiredPlayer != null && isOpen && (rewiredPlayer.GetAnyButtonDown() || rewiredPlayer.GetAxis(48) != 0f || rewiredPlayer.GetAxis(49) != 0f))
		{
			Close();
		}
	}

	private static string GetPathAppSettings()
	{
		return PersistentManager.GetPathAppSettings("/app_settings");
	}

	private void ReadModesUnlocked(string filePath)
	{
		fsData data = new fsData();
		if (!PersistentManager.TryToReadFile(filePath, out var fileData))
		{
			return;
		}
		byte[] bytes = Convert.FromBase64String(fileData);
		string @string = Encoding.UTF8.GetString(bytes);
		fsResult fsResult = fsJsonParser.Parse(@string, out data);
		if (fsResult.Failed && !fsResult.FormattedMessages.Equals("No input"))
		{
			Debug.LogError("Parsing error: " + fsResult.FormattedMessages);
		}
		else
		{
			if (!(data != null))
			{
				return;
			}
			Dictionary<string, fsData> asDictionary = data.AsDictionary;
			foreach (KeyValuePair<ModesToUnlock, string> aPP_SETTINGS_MODES_UNLOCKED_KEY in APP_SETTINGS_MODES_UNLOCKED_KEYS)
			{
				if (asDictionary.ContainsKey(aPP_SETTINGS_MODES_UNLOCKED_KEY.Value) && asDictionary[aPP_SETTINGS_MODES_UNLOCKED_KEY.Value].AsBool)
				{
					unlockedModes.Add(aPP_SETTINGS_MODES_UNLOCKED_KEY.Key);
				}
			}
		}
	}

	private void WriteFileAppSettings(string filePath)
	{
		fsData fsData = PersistentManager.ReadAppSettings(filePath);
		if (fsData == null || !fsData.IsDictionary)
		{
			return;
		}
		foreach (ModesToUnlock unlockedMode in unlockedModes)
		{
			fsData.AsDictionary[APP_SETTINGS_MODES_UNLOCKED_KEYS[unlockedMode]] = new fsData(boolean: true);
		}
		string s = fsJsonPrinter.CompressedJson(fsData);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		string contents = Convert.ToBase64String(bytes);
		File.WriteAllText(filePath, contents);
	}
}
