using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using FMOD.Studio;
using FMODUnity;
using Framework.Dialog;
using Framework.FrameworkCore;
using Framework.Inventory;
using Gameplay.UI;
using I2.Loc;
using UnityEngine;

namespace Framework.Managers;

public class DialogManager : GameSystem
{
	private class DialogLines
	{
		public string Text { get; private set; }

		public bool LongText { get; private set; }

		public string AudioKey { get; private set; }

		public bool StopAudio { get; private set; }

		public Sprite Image { get; private set; }

		public Dictionary<string, string> Additional { get; private set; }

		public DialogLines(string text, string audioKey, bool stopAudio, Sprite sprite = null, Dictionary<string, string> additional = null, bool longText = false)
		{
			Text = text;
			AudioKey = audioKey;
			StopAudio = stopAudio;
			Image = sprite;
			LongText = longText;
			if (additional != null)
			{
				Additional = new Dictionary<string, string>(additional);
			}
			else
			{
				Additional = new Dictionary<string, string>();
			}
		}
	}

	public enum ModalMode
	{
		NoModal,
		Modal,
		Boss
	}

	public delegate void DialogEvent(string id, int response);

	private Dictionary<string, DialogObject> allDialogs = new Dictionary<string, DialogObject>();

	private Dictionary<string, MsgObject> allMessges = new Dictionary<string, MsgObject>();

	private int currentLine = -1;

	private DialogObject currentDialog;

	private List<DialogLines> currentLines;

	private ModalMode currentModal;

	private bool hideWidgetAtEnd;

	private bool endDialogSafeRunning;

	private int currentCost;

	private const string LANGUAGE_PREFAB_NAME = "Dialog/Languages";

	private static string currentLanguage = string.Empty;

	private EventInstance currentSound = default(EventInstance);

	private string currentBankName = string.Empty;

	private const string FMOD_EVENT_NAME = "event:/VoiceOver/All";

	private const string FMOD_BANK_NAME = "VoiceOver_";

	public const string GENERIC_QUEST = "GENERIC";

	private const int MAX_LINES = 2;

	private const int MAX_LINE_SEPARATOR = 100;

	private const string KEY_PURGE = "PURGE";

	private const string KEY_CAPTION = "CAPTION";

	private const string KEY_DESCRIPTION = "DESCRIPTION";

	public bool InDialog { get; private set; }

	public int LastDialogAnswer { get; private set; }

	public LanguageSource Language { get; private set; }

	public event DialogEvent OnDialogFinished;

	public override void Start()
	{
		Language = GetLanguageSource();
		Core.Localization.AddLanguageSource("Dialog/Languages");
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLocalizationChange;
		LocalizationManager.OnLocalizeAudioEvent += OnAudioLocalizationChange;
		InDialog = false;
		DialogObject[] array = Resources.LoadAll<DialogObject>("Dialog/");
		allDialogs.Clear();
		DialogObject[] array2 = array;
		foreach (DialogObject dialogObject in array2)
		{
			allDialogs[dialogObject.id] = dialogObject;
		}
		foreach (KeyValuePair<string, DialogObject> allDialog in allDialogs)
		{
			if (allDialog.Value.externalAnswersId != string.Empty)
			{
				if (!allDialogs.ContainsKey(allDialog.Value.externalAnswersId))
				{
					Debug.LogError("** Dialog " + allDialog.Key + " references missing external id: " + allDialog.Value.externalAnswersId);
				}
				else
				{
					allDialog.Value.answersLines = new List<string>(allDialogs[allDialog.Value.externalAnswersId].answersLines);
				}
			}
		}
		Log.Debug("Dialog", allDialogs.Count + " dialogs loaded succesfully.");
		allMessges.Clear();
		MsgObject[] array3 = Resources.LoadAll<MsgObject>("Dialog/");
		MsgObject[] array4 = array3;
		foreach (MsgObject msgObject in array4)
		{
			allMessges[msgObject.id] = msgObject;
		}
		Log.Debug("Dialog", allMessges.Count + " messages loaded succesfully.");
		currentLanguage = string.Empty;
		LastDialogAnswer = -1;
		OnLocalizationChange();
		OnAudioLocalizationChange(Core.Localization.GetCurrentAudioLanguageCode());
		hideWidgetAtEnd = true;
	}

	public ReadOnlyCollection<DialogObject> GetAllDialogs()
	{
		List<DialogObject> list = new List<DialogObject>();
		foreach (DialogObject value in allDialogs.Values)
		{
			list.Add(value);
		}
		return list.AsReadOnly();
	}

	public bool StartConversation(string conversiationId, bool modal, bool useOnlyLast, bool hideWidget = true, int objectCost = 0, bool useBackground = false)
	{
		if (InDialog || !allDialogs.ContainsKey(conversiationId))
		{
			return false;
		}
		UIController.instance.GetDialog().SetBackgound(useBackground);
		Core.UI.Fade.ClearFade();
		hideWidgetAtEnd = hideWidget;
		LastDialogAnswer = -1;
		currentLine = -1;
		int num = 0;
		currentDialog = allDialogs[conversiationId];
		switch (currentDialog.dialogType)
		{
		case DialogObject.DialogType.Lines:
			num = CalculateLines_Text();
			break;
		case DialogObject.DialogType.GiveObject:
			num = CalculateLines_Object();
			break;
		case DialogObject.DialogType.GivePurge:
			currentCost = objectCost;
			num = CalculateLines_Purge();
			break;
		case DialogObject.DialogType.BuyObject:
			currentCost = objectCost;
			num = CalculateLines_Buy();
			break;
		case DialogObject.DialogType.PurgeGeneric:
			currentCost = objectCost;
			num = CalculateLines_PurgeGeneric();
			break;
		}
		if (useOnlyLast)
		{
			currentLine = num - 1;
		}
		currentModal = (modal ? ModalMode.Modal : ModalMode.NoModal);
		if (currentDialog.modalBoss)
		{
			currentModal = ModalMode.Boss;
		}
		if (currentModal != 0)
		{
			Core.Input.SetBlocker("DIALOG", blocking: true);
		}
		InDialog = true;
		ShowNextLine();
		return true;
	}

	public bool ShowMessage(string messageId, int line, string eventSound = "", float timeToWait = 0f, bool blockPlayer = true)
	{
		if (InDialog || !allMessges.ContainsKey(messageId) || allMessges[messageId].msgLines.Count < line)
		{
			return false;
		}
		string message = allMessges[messageId].msgLines[line];
		UIController.instance.ShowPopUp(message, eventSound, timeToWait, blockPlayer);
		return true;
	}

	public void UIEvent_LineEnded(int response = -1)
	{
		LastDialogAnswer = response;
		ShowNextLine();
	}

	public EventInstance PlayProgrammerSound(string key, FMODAudioManager.ProgrammerSoundSeted eventSound = null)
	{
		return Core.Audio.PlayProgrammerSound("event:/VoiceOver/All", key, eventSound);
	}

	private void ShowNextLine()
	{
		if (currentLine >= currentLines.Count - 1)
		{
			EndDialog();
			return;
		}
		currentLine++;
		DialogLines dialogLines = currentLines[currentLine];
		List<string> responses = new List<string>();
		if (currentLine == currentLines.Count - 1)
		{
			responses = currentDialog.answersLines;
		}
		if (dialogLines.StopAudio && currentSound.isValid() && currentSound.isValid())
		{
			currentSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
		if (!string.IsNullOrEmpty(dialogLines.AudioKey) && dialogLines.Text.Length > 0)
		{
			currentSound = PlayProgrammerSound(dialogLines.AudioKey, UIController.instance.GetDialog().OnProgrammerSoundSeted);
		}
		switch (currentDialog.dialogType)
		{
		case DialogObject.DialogType.Lines:
			if (dialogLines.LongText)
			{
				UIController.instance.GetDialog().ShowLongText(dialogLines.Text, responses, currentModal);
			}
			else
			{
				UIController.instance.GetDialog().ShowText(dialogLines.Text, responses, currentModal);
			}
			break;
		case DialogObject.DialogType.GiveObject:
			UIController.instance.GetDialog().ShowItem(dialogLines.Text, dialogLines.Image, responses, currentModal);
			break;
		case DialogObject.DialogType.GivePurge:
			UIController.instance.GetDialog().ShowPurge(dialogLines.Text, responses, currentModal);
			break;
		case DialogObject.DialogType.BuyObject:
			UIController.instance.GetDialog().ShowBuy(dialogLines.Additional["PURGE"], dialogLines.Additional["CAPTION"], dialogLines.Additional["DESCRIPTION"], dialogLines.Image, responses, currentModal);
			break;
		case DialogObject.DialogType.PurgeGeneric:
		{
			string text = Regex.Replace(dialogLines.Text, "{PURGEPOINTS}", dialogLines.Additional["PURGE"], RegexOptions.IgnoreCase);
			UIController.instance.GetDialog().ShowPurgeGeneric(text, responses, currentModal);
			break;
		}
		}
	}

	private void EndDialog()
	{
		if (currentSound.isValid())
		{
			currentSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			currentSound = default(EventInstance);
		}
		if (!endDialogSafeRunning)
		{
			UIController.instance.StartCoroutine(EndDialogSafe());
		}
	}

	private int CalculateLines_Text()
	{
		int result = 0;
		currentLines = new List<DialogLines>();
		int num = 0;
		foreach (string dialogLine in currentDialog.dialogLines)
		{
			string text = currentDialog.id + "_" + num;
			if (currentDialog.useOverrideAudioKey)
			{
				text = currentDialog.overrideKey;
			}
			if (num == currentDialog.dialogLines.Count - 1)
			{
				result = currentLines.Count;
			}
			bool flag = UIController.instance.GetDialog().GetNumberOfLines(dialogLine) > 2;
			List<DialogLines> list = currentLines;
			string text2 = dialogLine;
			string audioKey = text;
			bool stopAudio = true;
			bool longText = flag;
			list.Add(new DialogLines(text2, audioKey, stopAudio, null, null, longText));
			num++;
		}
		if (currentLines.Count == 0 && currentDialog.answersLines.Count > 0)
		{
			currentLines.Add(new DialogLines(string.Empty, string.Empty, stopAudio: true));
		}
		return result;
	}

	private int CalculateLines_Object()
	{
		currentLines = new List<DialogLines>();
		string text = string.Empty;
		string value = string.Empty;
		Sprite sprite = null;
		switch (currentDialog.itemType)
		{
		case InventoryManager.ItemType.Relic:
		{
			Relic relic = Core.InventoryManager.GetRelic(currentDialog.item);
			if ((bool)relic)
			{
				text = relic.caption;
				sprite = relic.picture;
				value = relic.description;
			}
			break;
		}
		case InventoryManager.ItemType.Collectible:
		{
			Framework.Inventory.CollectibleItem collectibleItem = Core.InventoryManager.GetCollectibleItem(currentDialog.item);
			if ((bool)collectibleItem)
			{
				text = collectibleItem.caption;
				sprite = collectibleItem.picture;
				value = collectibleItem.description;
			}
			break;
		}
		case InventoryManager.ItemType.Quest:
		{
			QuestItem questItem = Core.InventoryManager.GetQuestItem(currentDialog.item);
			if ((bool)questItem)
			{
				text = questItem.caption;
				sprite = questItem.picture;
				value = questItem.description;
			}
			break;
		}
		case InventoryManager.ItemType.Prayer:
		{
			Prayer prayer = Core.InventoryManager.GetPrayer(currentDialog.item);
			if ((bool)prayer)
			{
				text = prayer.caption;
				sprite = prayer.picture;
				value = prayer.description;
			}
			break;
		}
		case InventoryManager.ItemType.Bead:
		{
			RosaryBead rosaryBead = Core.InventoryManager.GetRosaryBead(currentDialog.item);
			if ((bool)rosaryBead)
			{
				text = rosaryBead.caption;
				sprite = rosaryBead.picture;
				value = rosaryBead.description;
			}
			break;
		}
		case InventoryManager.ItemType.Sword:
		{
			Sword sword = Core.InventoryManager.GetSword(currentDialog.item);
			if ((bool)sword)
			{
				text = sword.caption;
				sprite = sword.picture;
				value = sword.description;
			}
			break;
		}
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("CAPTION", text);
		dictionary.Add("DESCRIPTION", value);
		Dictionary<string, string> additional = dictionary;
		DialogLines item = new DialogLines(text, string.Empty, stopAudio: true, sprite, additional);
		currentLines.Add(item);
		return 1;
	}

	private int CalculateLines_Buy()
	{
		CalculateLines_Object();
		DialogLines dialogLines = currentLines[currentLines.Count - 1];
		dialogLines.Additional["PURGE"] = currentCost.ToString("F0");
		return 1;
	}

	private int CalculateLines_Purge()
	{
		currentLines = new List<DialogLines>();
		DialogLines item = new DialogLines(currentCost.ToString("F0"), string.Empty, stopAudio: true);
		currentLines.Add(item);
		return 1;
	}

	private int CalculateLines_PurgeGeneric()
	{
		currentLines = new List<DialogLines>();
		foreach (string dialogLine in currentDialog.dialogLines)
		{
			DialogLines dialogLines = new DialogLines(dialogLine, string.Empty, stopAudio: true);
			dialogLines.Additional["PURGE"] = currentCost.ToString("F0");
			currentLines.Add(dialogLines);
		}
		return currentLines.Count;
	}

	private int GetCharacterAtMinDistance(string line, string separator)
	{
		int num = line.Length / 2;
		int num2 = line.IndexOf(separator, num);
		int num3 = line.Substring(0, num).LastIndexOf(separator);
		int num4 = num2;
		if (num4 == -1 || num - num3 <= num2 - num)
		{
			num4 = num2;
		}
		return num4;
	}

	private IEnumerator EndDialogSafe()
	{
		endDialogSafeRunning = true;
		string id = currentDialog.id;
		if (hideWidgetAtEnd)
		{
			yield return new WaitForSeconds(0.2f);
		}
		UIController.instance.GetDialog().Hide(hideWidgetAtEnd);
		InDialog = false;
		currentDialog = null;
		Core.Input.SetBlocker("DIALOG", blocking: false);
		if (this.OnDialogFinished != null)
		{
			this.OnDialogFinished(id, LastDialogAnswer);
		}
		yield return null;
		endDialogSafeRunning = false;
	}

	public static LanguageSource GetLanguageSource()
	{
		GameObject asset = ResourceManager.pInstance.GetAsset<GameObject>("Dialog/Languages");
		return (!asset) ? null : asset.GetComponent<LanguageSource>();
	}

	private void OnAudioLocalizationChange(string idlang)
	{
		string text = currentBankName;
		currentBankName = "VoiceOver_" + idlang;
		if (text != currentBankName || text == string.Empty)
		{
			if (text != string.Empty)
			{
				RuntimeManager.UnloadBank(text);
			}
			RuntimeManager.LoadBank(currentBankName, loadSamples: true);
			Debug.Log("Audio localization event, new bank " + currentBankName);
		}
	}

	private void OnLocalizationChange()
	{
		if (!(currentLanguage != I2.Loc.LocalizationManager.CurrentLanguage))
		{
			return;
		}
		if (currentLanguage != string.Empty)
		{
			Log.Debug("Dialog", "Language change, localize items to: " + I2.Loc.LocalizationManager.CurrentLanguage);
		}
		currentLanguage = I2.Loc.LocalizationManager.CurrentLanguage;
		int languageIndexFromCode = Language.GetLanguageIndexFromCode(I2.Loc.LocalizationManager.CurrentLanguageCode);
		foreach (DialogObject value in allDialogs.Values)
		{
			LocalizeList(value.GetBaseTranslationID(), languageIndexFromCode, value.dialogLines);
			string text = value.GetBaseTranslationID();
			if (value.externalAnswersId != string.Empty)
			{
				text = "GENERIC/" + value.externalAnswersId;
			}
			LocalizeList(text + "_ANSWER", languageIndexFromCode, value.answersLines);
		}
		foreach (MsgObject value2 in allMessges.Values)
		{
			LocalizeList(value2.GetBaseTranslationID(), languageIndexFromCode, value2.msgLines);
		}
	}

	private void LocalizeList(string id, int idxLanguage, List<string> list)
	{
		string text = id + "_";
		for (int i = 0; i < list.Count; i++)
		{
			string text2 = text + i;
			TermData termData = Language.GetTermData(text2);
			if (termData == null)
			{
				Debug.LogWarning("Term " + text2 + " not found in Dialog Localization");
			}
			else if (termData.Languages.ElementAtOrDefault(idxLanguage) != null)
			{
				string text3 = termData.Languages[idxLanguage];
				if (text3.Length == 0)
				{
					text3 = "!ERROR LOC, NO TEXT";
				}
				list[i] = text3;
			}
		}
	}
}
