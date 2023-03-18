using Framework.FrameworkCore.Attributes.Logic;

namespace Framework.FrameworkCore.Attributes;

public class Life : VariableAttribute
{
	public float MissingRatio => base.Current / Final;

	public Life(float baseValue, float upgradeValue, float maxValue, float baseMultiplier)
		: base(baseValue, upgradeValue, maxValue, baseMultiplier)
	{
	}

	public override bool CallArchivementWhenUpgrade()
	{
		return true;
	}
}
