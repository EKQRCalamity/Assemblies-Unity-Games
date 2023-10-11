using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.Tables;

[DisplayName("Input Formatter", null)]
public class InputFormatter : FormatterBase
{
	private static readonly char[] OptionsSplit = new char[1] { '|' };

	private static FieldInfo TableEntryField;

	private static readonly LocalizedString LocalizedString = new LocalizedString
	{
		Arguments = new List<object>()
	};

	public override string[] DefaultNames => new string[3] { "input", "in", "#" };

	public override bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		if (!((TableEntryField ?? (TableEntryField = formattingInfo.CurrentValue.GetType().GetField("m_StringTableEntry", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)))?.GetValue(formattingInfo.CurrentValue) is StringTableEntry stringTableEntry))
		{
			return false;
		}
		IVariableGroup localVariables = formattingInfo.FormatDetails.FormatCache.LocalVariables;
		IList<object> originalArgs = formattingInfo.FormatDetails.OriginalArgs;
		LocalizedString.SetReference(stringTableEntry.Table.TableCollectionName, stringTableEntry.KeyId);
		string[] array = formattingInfo.FormatterOptions.Split(OptionsSplit, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			int result;
			int result2;
			if (localVariables != null && localVariables.TryGetValue(text, out var value) && value is LocalizedStringData.AVariable aVariable)
			{
				LocalizedString.Arguments.Add(aVariable.argument);
			}
			else if (!originalArgs.IsNullOrEmpty() && int.TryParse(text, out result) && result < originalArgs.Count)
			{
				LocalizedString.Arguments.Add(originalArgs[result] ?? ((object)1));
			}
			else if (text[0] == '#' && int.TryParse(text.Substring(1), out result2))
			{
				LocalizedString.Arguments.Add(result2);
			}
			else if (text == "x" && !originalArgs.IsNullOrEmpty())
			{
				LocalizedString.Arguments.Add(originalArgs[ArgumentSource.GetArgumentIndex(originalArgs)]);
			}
			else
			{
				LocalizedString.Arguments.Add(text);
			}
		}
		formattingInfo.Write(LocalizedString.Localize() ?? LocalizedString.GetTableEntry().Value);
		LocalizedString.Arguments.Clear();
		return true;
	}
}
