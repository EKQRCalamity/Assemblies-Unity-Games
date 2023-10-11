public struct BlendedNoiseWaveFloat
{
	public NoiseWaveFloat value;

	public NoiseWaveFloatDataBlender blender;

	public BlendedNoiseWaveFloat(NoiseWaveFloatData data)
	{
		value = new NoiseWaveFloat(data);
		blender = new NoiseWaveFloatDataBlender(data, data);
	}

	public BlendedNoiseWaveFloat BlendToward(NoiseWaveFloatData target, float time)
	{
		blender = new NoiseWaveFloatDataBlender(value.data, target, time);
		return this;
	}

	public float Update(float deltaTime, float noiseOffset = 0f)
	{
		value.data = blender.GetBlend(deltaTime);
		return value.Update(deltaTime, noiseOffset);
	}

	public float UpdateMultiplier(float deltaTime, float noiseOffset = 0f)
	{
		value.data = blender.GetBlend(deltaTime);
		return value.UpdateMultiplier(deltaTime, noiseOffset);
	}
}
