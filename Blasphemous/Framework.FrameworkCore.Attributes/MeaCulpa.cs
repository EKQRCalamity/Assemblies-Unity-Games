using Framework.FrameworkCore.Attributes.Logic;

namespace Framework.FrameworkCore.Attributes;

public class MeaCulpa : Attribute
{
	public MeaCulpa(float baseValue, float upgradeValue, float baseMultiplier)
		: base(baseValue, upgradeValue, baseMultiplier)
	{
	}

	public override bool CallArchivementWhenUpgrade()
	{
		return true;
	}
}
