using Framework.FrameworkCore.Attributes.Logic;

namespace Framework.FrameworkCore.Attributes;

public class Strength : Attribute
{
	private float finalStrengthMultiplier = 1f;

	public float FinalStrengthMultiplier
	{
		get
		{
			return finalStrengthMultiplier;
		}
		set
		{
			finalStrengthMultiplier = value;
		}
	}

	public override float Final => base.Final * FinalStrengthMultiplier;

	public Strength(float baseValue, float upgradeValue, float baseMultiplier)
		: base(baseValue, upgradeValue, baseMultiplier)
	{
	}
}
