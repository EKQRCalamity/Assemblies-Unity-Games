using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Framework.PatchNotes;
using FullSerializer;
using Gameplay.UI;
using UnityEngine;

namespace Framework.Managers;

public class PatchNotesManager : GameSystem
{
	private const string SEEN_PATCH_NOTES_PATH = "/app_settings";

	private const string APP_SETTINGS_KEY = "seen_patch_notes";

	private const string PATCH_NOTES_PATH = "Patch Notes/PATCH_NOTES_LIST";

	private List<string> seenPatchNotesVersions = new List<string>();

	private List<string> unseenPatchNotesVersions = new List<string>();

	private bool newPatchesAlreadyShownOnStart;

	public override void Initialize()
	{
		string pathSeenPatchNotes = GetPathSeenPatchNotes();
		if (!File.Exists(pathSeenPatchNotes))
		{
			File.CreateText(pathSeenPatchNotes).Close();
		}
		else
		{
			ReadFileSeenPatchNotes(pathSeenPatchNotes);
		}
		PatchNotesList patchNotesList = Resources.Load<PatchNotesList>("Patch Notes/PATCH_NOTES_LIST");
		FindUnseenPatchNotes(patchNotesList);
	}

	public void MarkPatchNotesAsSeen()
	{
		seenPatchNotesVersions.AddRange(unseenPatchNotesVersions);
		string pathSeenPatchNotes = GetPathSeenPatchNotes();
		WriteFileSeenPatchNotes(pathSeenPatchNotes);
	}

	public override void Update()
	{
		if (!newPatchesAlreadyShownOnStart && unseenPatchNotesVersions.Count > 0 && UIController.instance != null)
		{
			UIController.instance.ShowPatchNotes();
			newPatchesAlreadyShownOnStart = true;
		}
	}

	public List<string> GetPatchNotesToBeMarkedAsNew()
	{
		return unseenPatchNotesVersions;
	}

	private string GetPathSeenPatchNotes()
	{
		return PersistentManager.GetPathAppSettings("/app_settings");
	}

	private void ReadFileSeenPatchNotes(string filePath)
	{
		fsData data = new fsData();
		if (!TryToReadFile(filePath, out var fileData))
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
		else if (data != null)
		{
			Dictionary<string, fsData> asDictionary = data.AsDictionary;
			if (asDictionary.ContainsKey("seen_patch_notes"))
			{
				string[] collection = asDictionary["seen_patch_notes"].AsString.Trim().Split(',');
				seenPatchNotesVersions = new List<string>(collection);
			}
		}
	}

	private void WriteFileSeenPatchNotes(string filePath)
	{
		if (seenPatchNotesVersions.Count == 0)
		{
			return;
		}
		string text = string.Empty;
		foreach (string seenPatchNotesVersion in seenPatchNotesVersions)
		{
			text = text + seenPatchNotesVersion + ",";
		}
		text = text.Remove(text.Length - 1);
		fsData fsData = ReadAppSettings(filePath);
		fsData.AsDictionary["seen_patch_notes"] = new fsData(text);
		string s = fsJsonPrinter.CompressedJson(fsData);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		string contents = Convert.ToBase64String(bytes);
		File.WriteAllText(filePath, contents);
	}

	private fsData ReadAppSettings(string filePath)
	{
		fsData data = new fsData();
		if (TryToReadFile(filePath, out var fileData))
		{
			byte[] bytes = Convert.FromBase64String(fileData);
			string @string = Encoding.UTF8.GetString(bytes);
			fsResult fsResult = fsJsonParser.Parse(@string, out data);
			if (fsResult.Failed && !fsResult.FormattedMessages.Equals("No input"))
			{
				Debug.LogError("Parsing error: " + fsResult.FormattedMessages);
			}
		}
		return data;
	}

	private bool TryToReadFile(string filePath, out string fileData)
	{
		if (!File.Exists(filePath))
		{
			Debug.LogError("File at path '" + filePath + "' does not exists!");
			fileData = string.Empty;
			return false;
		}
		fileData = File.ReadAllText(filePath);
		if (fileData.Length == 0)
		{
			Debug.Log("File at path '" + filePath + "' is empty.");
			return false;
		}
		return true;
	}

	private void FindUnseenPatchNotes(PatchNotesList patchNotesList)
	{
		string version = FindLastVersionInList(seenPatchNotesVersions);
		List<int> versionDigits = GetVersionDigits(version);
		foreach (Framework.PatchNotes.PatchNotes patchNotes in patchNotesList.patchNotesList)
		{
			List<int> versionDigits2 = GetVersionDigits(patchNotes.version);
			for (int i = 0; i < versionDigits2.Count && versionDigits2[i] >= versionDigits[i]; i++)
			{
				if (versionDigits2[i] > versionDigits[i])
				{
					unseenPatchNotesVersions.Add(patchNotes.version);
					break;
				}
			}
		}
	}

	private string FindLastVersionInList(List<string> versions)
	{
		string text = "1.0";
		foreach (string version in versions)
		{
			List<int> versionDigits = GetVersionDigits(text);
			List<int> versionDigits2 = GetVersionDigits(version);
			if (versionDigits2 == null)
			{
				continue;
			}
			for (int i = 0; i < Mathf.Min(versionDigits2.Count, versionDigits.Count) && versionDigits2[i] >= versionDigits[i]; i++)
			{
				if (versionDigits2[i] > versionDigits[i])
				{
					text = version;
					break;
				}
			}
		}
		return text;
	}

	private List<int> GetVersionDigits(string version)
	{
		List<int> list = new List<int>();
		string[] array = version.Split('.');
		string[] array2 = array;
		foreach (string s in array2)
		{
			int result = 0;
			if (int.TryParse(s, out result))
			{
				list.Add(result);
				continue;
			}
			Debug.LogError("GetVersionDigits error! version '" + version + "' does not seem to be in a correct format!");
			list = null;
			break;
		}
		return list;
	}
}
