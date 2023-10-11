using UnityEngine;

public struct NoiseWaveVector3DataBlender
{
	public NoiseWaveVector3Data start;

	public NoiseWaveVector3Data end;

	public float time;

	private float _elapsed;

	public NoiseWaveVector3DataBlender(NoiseWaveVector3Data start, NoiseWaveVector3Data end, float time = 1f)
	{
		this = default(NoiseWaveVector3DataBlender);
		this.start = start;
		this.end = end;
		this.time = time;
	}

	public NoiseWaveVector3Data GetBlend(float deltaTime)
	{
		return start.Lerp(end, Mathf.Clamp01((_elapsed += deltaTime) / time));
	}
}
