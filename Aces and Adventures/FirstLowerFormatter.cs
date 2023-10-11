using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.Core.Extensions;

[DisplayName("First Lower Formatter", null)]
public class FirstLowerFormatter : FormatterBase
{
	public override string[] DefaultNames => new string[2] { "FirstLower", ">" };

	public override bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		string s = formattingInfo.CurrentValue?.ToString() ?? "";
		if (s.HasVisibleCharacter())
		{
			formattingInfo.Write(s.FirstLower());
			return true;
		}
		return false;
	}
}
