using System;

namespace Dynamite3D.RealIvy;

[Serializable]
public class IvyParameterFloat : IvyParameter
{
	public IvyParameterFloat(float value)
	{
		base.value = value;
	}

	public override void UpdateValue(float value)
	{
		base.value = value;
	}

	public static implicit operator float(IvyParameterFloat floatParameter)
	{
		return floatParameter.value;
	}

	public static implicit operator IvyParameterFloat(float floatValue)
	{
		return new IvyParameterFloat(floatValue);
	}
}
