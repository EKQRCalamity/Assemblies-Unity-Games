using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

[UIField]
public class LocalizationManagerUI
{
	[UIField]
	private async void _SyncAllLocalizationTables()
	{
		await LocalizationUtil.SyncAllAsync(UIGeneratorType.GetActiveUITransform(this), pull: true, LocalizationUtil.ShouldForcePush());
	}

	[UIField]
	private async void _OverwriteAllSheets()
	{
		UIUtil.CreatePopup("Overwrite All Google Sheets", UIUtil.CreateMessageBox("Would you like to overwrite any existing Google Sheets with the data currently held in Unity localization tables?"), null, parent: UIGeneratorType.GetActiveUITransform(this), buttons: new string[2] { "Overwrite Sheets", "Cancel" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: async delegate(string s)
		{
			if (s == "Overwrite Sheets")
			{
				await LocalizationUtil.SyncAllAsync(UIGeneratorType.GetActiveUITransform(this), pull: false, forcePush: true);
			}
		});
	}

	[UIField]
	private void _CreateMissingTables()
	{
	}

	[UIField]
	private async Task _GenerateFontCharacterSets()
	{
		await LocalizationUtil.GenerateCharacterSetFiles();
	}

	[UIField]
	private void _CheckForOutOfDateEntries()
	{
		UIUtil.CreatePopup("Out of Date Entries", UIUtil.CreateMessageBox("Following out of date entries found:\n" + (from e in LocalizationUtil.GetOutOfDateEntries()
			group e by e.Table).ToDictionary((IGrouping<LocalizationTable, StringTableEntry> g) => g.Key, (IGrouping<LocalizationTable, StringTableEntry> g) => g.ToList()).ToStringSmart((KeyValuePair<LocalizationTable, List<StringTableEntry>> p) => $"{p.Key.name}: {p.Value.Count} entries.", "\n")), null, buttons: new string[2] { "Cancel", "Clear Out Of Date Entries" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (s == "Cancel")
			{
				return;
			}
			foreach (StringTableEntry outOfDateEntry in LocalizationUtil.GetOutOfDateEntries())
			{
				outOfDateEntry.Value = "";
			}
		});
	}

	[UIField]
	private void _DisplayLocalesThatWillBeSynced()
	{
		Debug.Log($"The following locales will be synced: Pull Only = {LocalizationUtil.Settings.pullOnly}, Force Push = {LocalizationUtil.Settings.forcePush}");
		foreach (Locale item in LocalizationUtil.GetLocalesToSync())
		{
			Debug.Log(item.LocaleName);
		}
	}

	[UIField]
	private async void _CheckForErrors()
	{
		await LocalizationUtil.CheckForErrors();
	}

	[UIField]
	private void _SoloFinishedLocales()
	{
		foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
		{
			if (locale != LocalizationUtil.ProjectLocale)
			{
				locale.SetSolo(!locale.IsWorkInProgress());
			}
		}
	}

	[UIField]
	private void _SoloWorkInProgressLocales()
	{
		foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
		{
			if (locale != LocalizationUtil.ProjectLocale)
			{
				locale.SetSolo(locale.IsWorkInProgress());
			}
		}
	}

	[UIField]
	private void _ClearSolo()
	{
		foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
		{
			if (locale != LocalizationUtil.ProjectLocale)
			{
				locale.SetSolo(solo: false);
			}
		}
	}
}
