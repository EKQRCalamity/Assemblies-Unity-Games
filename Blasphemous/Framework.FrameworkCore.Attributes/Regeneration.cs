using Framework.FrameworkCore.Attributes.Logic;

namespace Framework.FrameworkCore.Attributes;

public class Regeneration : Attribute
{
	public Regeneration(float baseValue, float upgradeValue, float baseMultiplier = 0f)
		: base(baseValue, upgradeValue, baseMultiplier)
	{
	}
}
