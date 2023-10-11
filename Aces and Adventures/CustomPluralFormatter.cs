using System;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.Utilities;

[Serializable]
public class CustomPluralFormatter : PluralLocalizationFormatter
{
	protected PluralRules.PluralRuleDelegate _defaultRule;

	protected override PluralRules.PluralRuleDelegate GetPluralRule(IFormattingInfo formattingInfo)
	{
		string formatterOptions = formattingInfo.FormatterOptions;
		PluralRules.PluralRuleDelegate pluralRuleDelegate;
		if (formatterOptions == null || formatterOptions.Length == 0)
		{
			pluralRuleDelegate = _defaultRule;
			if (pluralRuleDelegate == null)
			{
				return _defaultRule = PluralRules.GetPluralRule(base.DefaultTwoLetterISOLanguageName);
			}
		}
		else
		{
			pluralRuleDelegate = PluralRules.GetPluralRule(formatterOptions);
		}
		return pluralRuleDelegate;
	}
}
