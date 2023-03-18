using Framework.FrameworkCore.Attributes.Logic;

namespace Framework.FrameworkCore.Attributes;

public class ContactDmgReduction : Attribute
{
	public ContactDmgReduction(float baseValue, float upgradeValue, float baseMultiplier)
		: base(baseValue, upgradeValue, baseMultiplier)
	{
	}
}
