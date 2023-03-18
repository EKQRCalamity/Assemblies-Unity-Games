using Framework.FrameworkCore.Attributes.Logic;

namespace Framework.FrameworkCore.Attributes;

public class Defense : Attribute
{
	public Defense(float baseValue, float upgradeValue, float baseMultiplier)
		: base(baseValue, upgradeValue, baseMultiplier)
	{
	}
}
