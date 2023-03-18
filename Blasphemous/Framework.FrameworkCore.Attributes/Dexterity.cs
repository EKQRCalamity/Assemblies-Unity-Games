using Framework.FrameworkCore.Attributes.Logic;

namespace Framework.FrameworkCore.Attributes;

public class Dexterity : Attribute
{
	public Dexterity(float baseValue, float upgradeValue, float baseMultiplier = 0f)
		: base(baseValue, upgradeValue, baseMultiplier)
	{
	}
}
