using Framework.FrameworkCore.Attributes.Logic;

namespace Framework.FrameworkCore.Attributes;

public class MagicDmgReduction : Attribute
{
	public MagicDmgReduction(float baseValue, float upgradeValue, float baseMultiplier)
		: base(baseValue, upgradeValue, baseMultiplier)
	{
	}
}
