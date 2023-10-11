using System;

namespace Dynamite3D.RealIvy;

[Serializable]
public class IvyParameterInt : IvyParameter
{
	public IvyParameterInt(int value)
	{
		base.value = value;
	}

	public override void UpdateValue(float value)
	{
		base.value = (int)value;
	}

	public static implicit operator int(IvyParameterInt intParameter)
	{
		return (int)intParameter.value;
	}

	public static implicit operator IvyParameterInt(int intValue)
	{
		return new IvyParameterInt(intValue);
	}
}
