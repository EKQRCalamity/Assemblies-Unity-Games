using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace DialoguerCore;

public class DialoguerData
{
	public readonly DialoguerGlobalVariables globalVariables;

	public readonly List<DialoguerDialogue> dialogues;

	public readonly List<DialoguerTheme> themes;

	public DialoguerData(DialoguerGlobalVariables globalVariables, List<DialoguerDialogue> dialogues, List<DialoguerTheme> themes)
	{
		this.globalVariables = globalVariables;
		this.dialogues = dialogues;
		this.themes = themes;
	}

	public void loadGlobalVariablesState(string globalVariablesXml)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(DialoguerGlobalVariables));
		XmlReader xmlReader = XmlReader.Create(new StringReader(globalVariablesXml));
		DialoguerGlobalVariables dialoguerGlobalVariables = (DialoguerGlobalVariables)xmlSerializer.Deserialize(xmlReader);
		for (int i = 0; i < dialoguerGlobalVariables.booleans.Count && i < globalVariables.booleans.Count; i++)
		{
			globalVariables.booleans[i] = dialoguerGlobalVariables.booleans[i];
		}
		for (int j = 0; j < dialoguerGlobalVariables.floats.Count && j < globalVariables.floats.Count; j++)
		{
			globalVariables.floats[j] = dialoguerGlobalVariables.floats[j];
		}
		for (int k = 0; k < dialoguerGlobalVariables.strings.Count && k < globalVariables.strings.Count; k++)
		{
			globalVariables.strings[k] = dialoguerGlobalVariables.strings[k];
		}
	}
}
