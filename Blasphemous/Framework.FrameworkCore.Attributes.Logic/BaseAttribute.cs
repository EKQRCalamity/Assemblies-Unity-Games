namespace Framework.FrameworkCore.Attributes.Logic;

public class BaseAttribute
{
	public float Base { get; protected set; }

	public float Multiplier { get; protected set; }

	public BaseAttribute(float baseValue, float baseMultiplier = 1f)
	{
		Base = baseValue;
		Multiplier = baseMultiplier;
	}
}
