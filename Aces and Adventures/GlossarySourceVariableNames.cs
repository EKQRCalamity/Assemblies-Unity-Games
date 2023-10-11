using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[Serializable]
public class GlossarySourceVariableNames : GlossarySource
{
	public static readonly string[] ADDITIONAL_GLOSSARY_VARIABLES = new string[8] { "x", "null", "0", "1", "2", "3", "4", "5" };

	public static readonly string[] FUNCTION_NAMES = new string[3] { "time(", "choose(", "cond:" };

	public static readonly string[] ADDITIONAL_SYMBOLS = new string[1] { "{}" };

	public static readonly string[] SCANNED_TABLE_NAMES = new string[1] { "Message" };

	public override async IAsyncEnumerable<Term> GetTermsAsync(Locale locale)
	{
		foreach (string item in EnumUtil<LocalizedVariableName>.Values.Select(EnumUtil.Name).Concat(SCANNED_TABLE_NAMES.SelectMany((string tableName) => LocalizationSettings.StringDatabase.GetTable(tableName, LocalizationUtil.ProjectLocale).GetVariableNames())).Concat(ADDITIONAL_GLOSSARY_VARIABLES))
		{
			string text = "{" + item + "}";
			string nestedName = "{" + item + ":";
			string parameterName = "(" + item + ")";
			yield return new Term(text, text, verbatim: true, properNoun: false, ProtectedTermWhen.Always);
			yield return new Term(nestedName, nestedName, verbatim: true, properNoun: false, ProtectedTermWhen.Always);
			yield return new Term(parameterName, parameterName, verbatim: true, properNoun: false, ProtectedTermWhen.Always);
		}
		string[] fUNCTION_NAMES = FUNCTION_NAMES;
		foreach (string text2 in fUNCTION_NAMES)
		{
			yield return new Term(text2, text2, verbatim: true, properNoun: false, ProtectedTermWhen.Always);
		}
		fUNCTION_NAMES = ADDITIONAL_SYMBOLS;
		foreach (string text3 in fUNCTION_NAMES)
		{
			yield return new Term(text3, text3, verbatim: true, properNoun: false, ProtectedTermWhen.Always);
		}
	}
}
