using UnityEngine;

public struct NoiseWaveVector3
{
	public NoiseWaveFloat x;

	public NoiseWaveFloat y;

	public NoiseWaveFloat z;

	public NoiseWaveVector3Data data
	{
		get
		{
			NoiseWaveVector3Data result = default(NoiseWaveVector3Data);
			result.x = x.data;
			result.y = y.data;
			result.z = z.data;
			return result;
		}
		set
		{
			x.data = value.x;
			y.data = value.y;
			z.data = value.z;
		}
	}

	public NoiseWaveVector3(NoiseWaveVector3Data data)
	{
		x = new NoiseWaveFloat(data.x);
		y = new NoiseWaveFloat(data.y);
		z = new NoiseWaveFloat(data.z);
	}

	public NoiseWaveVector3 Lerp(NoiseWaveVector3 other, float t)
	{
		NoiseWaveVector3 result = default(NoiseWaveVector3);
		result.x = x.Lerp(other.x, t);
		result.y = y.Lerp(other.y, t);
		result.z = z.Lerp(other.z, t);
		return result;
	}

	public Vector3 Update(float deltaTime, float noiseOffset = 0f)
	{
		return new Vector3(x.Update(deltaTime, noiseOffset), y.Update(deltaTime, noiseOffset + noiseOffset), z.Update(deltaTime, noiseOffset + noiseOffset + noiseOffset));
	}

	public Vector3 UpdateMultiplier(float deltaTime, float noiseOffset = 0f)
	{
		return new Vector3(x.UpdateMultiplier(deltaTime, noiseOffset), y.UpdateMultiplier(deltaTime, noiseOffset + noiseOffset), z.UpdateMultiplier(deltaTime, noiseOffset + noiseOffset + noiseOffset));
	}
}
