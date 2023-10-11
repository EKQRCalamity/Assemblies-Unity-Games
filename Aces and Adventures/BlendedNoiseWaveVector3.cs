using UnityEngine;

public struct BlendedNoiseWaveVector3
{
	public NoiseWaveVector3 value;

	public NoiseWaveVector3DataBlender blender;

	public BlendedNoiseWaveVector3(NoiseWaveVector3Data data)
	{
		value = new NoiseWaveVector3(data);
		blender = new NoiseWaveVector3DataBlender(data, data);
	}

	public BlendedNoiseWaveVector3 BlendToward(NoiseWaveVector3Data target, float time)
	{
		blender = new NoiseWaveVector3DataBlender(value.data, target, time);
		return this;
	}

	public Vector3 Update(float deltaTime, float noiseOffset = 0f)
	{
		value.data = blender.GetBlend(deltaTime);
		return value.Update(deltaTime, noiseOffset);
	}

	public Vector3 UpdateMultiplier(float deltaTime, float noiseOffset = 0f)
	{
		value.data = blender.GetBlend(deltaTime);
		return value.UpdateMultiplier(deltaTime, noiseOffset);
	}
}
