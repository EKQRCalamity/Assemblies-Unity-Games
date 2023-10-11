using UnityEngine;

public struct NoiseWaveFloat
{
	public NoiseWaveFloatData data;

	public float waveElapsed;

	public float noiseElapsed;

	public NoiseWaveFloat(NoiseWaveFloatData data, float waveElapsed = 0f, float noiseElapsed = 0f)
	{
		this.data = data;
		this.waveElapsed = waveElapsed;
		this.noiseElapsed = noiseElapsed;
	}

	public NoiseWaveFloat Lerp(NoiseWaveFloat other, float t)
	{
		return new NoiseWaveFloat(data.Lerp(other.data, t), waveElapsed, noiseElapsed);
	}

	public float Update(float deltaTime, float noiseOffset = 0f)
	{
		waveElapsed += deltaTime * data.waveFrequency;
		noiseElapsed += deltaTime * data.noiseFrequency;
		return 0f + MathUtil.Remap(Mathf.Cos(waveElapsed), new Vector2(-1f, 1f), data.waveRange) + MathUtil.Remap(Mathf.PerlinNoise(noiseElapsed, noiseOffset), new Vector2(0f, 1f), data.noiseRange);
	}

	public float UpdateMultiplier(float deltaTime, float noiseOffset = 0f)
	{
		waveElapsed += deltaTime * data.waveFrequency;
		noiseElapsed += deltaTime * data.noiseFrequency;
		return 1f * MathUtil.Remap(Mathf.Cos(waveElapsed), new Vector2(-1f, 1f), data.waveRange) * MathUtil.Remap(Mathf.PerlinNoise(noiseElapsed, noiseOffset), new Vector2(0f, 1f), data.noiseRange);
	}
}
