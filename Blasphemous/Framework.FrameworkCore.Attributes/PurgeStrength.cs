using Framework.FrameworkCore.Attributes.Logic;

namespace Framework.FrameworkCore.Attributes;

public class PurgeStrength : Attribute
{
	public PurgeStrength(float baseValue, float upgradeValue, float baseMultiplier)
		: base(baseValue, upgradeValue, baseMultiplier)
	{
	}
}
