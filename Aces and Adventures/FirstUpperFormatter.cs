using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.Core.Extensions;

[DisplayName("First Upper Formatter", null)]
public class FirstUpperFormatter : FormatterBase
{
	public override string[] DefaultNames => new string[2] { "FirstUpper", "<" };

	public override bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		string s = formattingInfo.CurrentValue?.ToString() ?? "";
		if (s.HasVisibleCharacter())
		{
			formattingInfo.Write(s.FirstUpper());
			return true;
		}
		return false;
	}
}
