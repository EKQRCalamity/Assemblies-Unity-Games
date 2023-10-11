using UnityEngine;

public struct NoiseWaveFloatDataBlender
{
	public NoiseWaveFloatData start;

	public NoiseWaveFloatData end;

	public float time;

	private float _elapsed;

	public NoiseWaveFloatDataBlender(NoiseWaveFloatData start, NoiseWaveFloatData end, float time = 1f)
	{
		this = default(NoiseWaveFloatDataBlender);
		this.start = start;
		this.end = end;
		this.time = time;
	}

	public NoiseWaveFloatData GetBlend(float deltaTime)
	{
		return start.Lerp(end, Mathf.Clamp01((_elapsed += deltaTime) / time));
	}
}
