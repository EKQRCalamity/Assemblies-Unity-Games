using System.Collections.Generic;
using DialoguerCore;
using UnityEngine;

public struct DialoguerTextData
{
	public readonly int dialogueID;

	public readonly int nodeID;

	public readonly string rawText;

	public readonly string theme;

	public readonly bool newWindow;

	public readonly string name;

	public readonly string portrait;

	public readonly string metadata;

	public readonly string audio;

	public readonly float audioDelay;

	public readonly Rect rect;

	public readonly string[] choices;

	private string _cachedText;

	public string text
	{
		get
		{
			if (_cachedText == null)
			{
				_cachedText = DialoguerUtils.insertTextPhaseStringVariables(rawText);
			}
			return _cachedText;
		}
	}

	public bool usingPositionRect => rect.x != 0f || rect.y != 0f || rect.width != 0f || rect.height != 0f;

	public DialoguerTextPhaseType windowType => (choices != null) ? DialoguerTextPhaseType.BranchedText : DialoguerTextPhaseType.Text;

	public DialoguerTextData(string text, string themeName, bool newWindow, string name, string portrait, string metadata, string audio, float audioDelay, Rect rect, List<string> choices, int dialogueID, int nodeID)
	{
		this.dialogueID = dialogueID;
		this.nodeID = nodeID;
		rawText = text;
		theme = themeName;
		this.newWindow = newWindow;
		this.name = name;
		this.portrait = portrait;
		this.metadata = metadata;
		this.audio = audio;
		this.audioDelay = audioDelay;
		this.rect = new Rect(rect.x, rect.y, rect.width, rect.height);
		if (choices != null)
		{
			string[] array = choices.ToArray();
			this.choices = array.Clone() as string[];
		}
		else
		{
			this.choices = null;
		}
		_cachedText = null;
	}

	public override string ToString()
	{
		object[] obj = new object[22]
		{
			"\nTheme ID: ", theme, "\nNew Window: ", null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, null
		};
		bool flag = newWindow;
		obj[3] = flag.ToString();
		obj[4] = "\nName: ";
		obj[5] = name;
		obj[6] = "\nPortrait: ";
		obj[7] = portrait;
		obj[8] = "\nMetadata: ";
		obj[9] = metadata;
		obj[10] = "\nAudio Clip: ";
		obj[11] = audio;
		obj[12] = "\nAudio Delay: ";
		float num = audioDelay;
		obj[13] = num.ToString();
		obj[14] = "\nRect: ";
		Rect rect = this.rect;
		obj[15] = rect.ToString();
		obj[16] = "\nRaw Text: ";
		obj[17] = rawText;
		obj[18] = "\nDialogue ID:";
		obj[19] = dialogueID;
		obj[20] = "\nNode ID:";
		obj[21] = nodeID;
		return string.Concat(obj);
	}
}
