using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;

[Serializable]
public class GlossarySourceGlobalVariables : GlossarySource
{
	public override async IAsyncEnumerable<Term> GetTermsAsync(Locale locale)
	{
		PersistentVariablesSource globalVariableSource = LocalizationSettings.StringDatabase.SmartFormatter.SourceExtensions.OfType<PersistentVariablesSource>().FirstOrDefault();
		if (globalVariableSource == null)
		{
			yield break;
		}
		foreach (string key in globalVariableSource.Keys)
		{
			foreach (string key2 in globalVariableSource[key].Keys)
			{
				string term = key + "." + key2;
				if (term != null)
				{
					string text = "{" + term + "}";
					yield return new Term(text, text, verbatim: true, properNoun: false, ProtectedTermWhen.Always);
					string text2 = "{" + term + ":";
					yield return new Term(text2, text2, verbatim: true, properNoun: false, ProtectedTermWhen.Always);
				}
			}
		}
	}
}
