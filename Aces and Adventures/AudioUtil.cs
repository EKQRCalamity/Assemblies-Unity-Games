using System;
using UnityEngine;
using UnityEngine.Audio;

public static class AudioUtil
{
	public const int AUDIO_CLIP_CHANNEL_VERTICES = 500;

	private const int AUDIO_CLIP_MESH_SAMPLE_RATE = 50;

	public static float[] GetAllSamples(this AudioClip clip)
	{
		float[] array = new float[clip.samples * clip.channels];
		if (clip.loadType == AudioClipLoadType.DecompressOnLoad)
		{
			clip.GetData(array, 0);
		}
		else
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = MathUtil.PowSigned(Mathf.Sin((float)i * 0.0001f * (MathF.PI * 2f)) * Mathf.Cos((float)i * 0.0001f * (MathF.PI * 2f) * 100f), 3f) * 0.666f;
			}
		}
		return array;
	}

	public static float TimeRemaining(this AudioSource source, bool accountForPitch = true)
	{
		if (!source || !source.clip)
		{
			return 0f;
		}
		if (source.loop)
		{
			return float.MaxValue;
		}
		return (source.clip.length - source.time) / (accountForPitch ? Mathf.Abs(source.pitch).InsureNonZero() : 1f);
	}

	public static float TimeRemainingRatio(this AudioSource source, bool accountForPitch = true)
	{
		return source.TimeRemaining(accountForPitch) / (source.clip ? (source.clip.length / (accountForPitch ? Mathf.Abs(source.pitch).InsureNonZero() : 1f)) : 1f);
	}

	public static float PlayTimeRatio(this AudioSource source, bool accountForPitch = true)
	{
		return Mathf.Clamp01(1f - source.TimeRemainingRatio(accountForPitch));
	}

	public static bool SetAsOutputGroup(this AudioMixerGroup group, GameObject gameObject)
	{
		bool result = false;
		AudioSource[] componentsInChildren = gameObject.GetComponentsInChildren<AudioSource>(includeInactive: true);
		foreach (AudioSource audioSource in componentsInChildren)
		{
			if (audioSource.outputAudioMixerGroup != group)
			{
				audioSource.outputAudioMixerGroup = group;
				result = true;
			}
		}
		return result;
	}

	public static float LoudnessToDB(float loudness)
	{
		return 10f * (float)Math.Log(Mathf.Max(0f, loudness), 2.0);
	}

	public static float DBToLoudness(float db)
	{
		return Mathf.Pow(MathF.E, db * (float)Math.Log(2.0) * 0.1f);
	}

	public static bool IsPlaying(this PooledAudioSource pooledAudioSource)
	{
		if ((bool)pooledAudioSource)
		{
			return pooledAudioSource.isPlaying;
		}
		return false;
	}

	public static long GetPlayId(this PooledAudioSource pooledAudioSource)
	{
		if (!pooledAudioSource)
		{
			return 0L;
		}
		return pooledAudioSource.playId;
	}

	public static double Length(this AudioClip clip)
	{
		return (double)clip.samples / (double)clip.frequency;
	}

	public static double GetTime(this AudioSource source)
	{
		return (double)source.timeSamples / (double)source.clip.frequency;
	}

	public static void SetTime(this AudioSource source, double time)
	{
		source.timeSamples = Math.Max(0, Math.Min((int)Math.Round((double)source.clip.frequency * time), source.clip.samples - 1));
	}

	public static bool IsPlayingOrPaused(this AudioSource source)
	{
		if ((bool)source)
		{
			if (!source.isPlaying)
			{
				return source.time > 0f;
			}
			return true;
		}
		return false;
	}

	public static void PlayScheduledDebug(this AudioSource source, double dspTime)
	{
		Debug.Log($"{source.name}.PlayScheduled({dspTime})");
		source.PlayScheduled(dspTime);
	}

	public static void SetScheduledEndTimeDebug(this AudioSource source, double dspTime)
	{
		Debug.Log($"{source.name}.SetScheduledEndTime({dspTime})");
		source.SetScheduledEndTime(dspTime);
	}

	public static Mesh CreateMesh(AudioClip clip, int verticesPerChannel = 500, int sampleRate = 50, Rect? meshBoundaries = null)
	{
		return CreateMesh(clip.GetAllSamples(), clip.channels, verticesPerChannel, sampleRate, meshBoundaries);
	}

	public static Mesh CreateMesh(float[] samples, int numberOfChannels, int verticesPerChannel = 500, int sampleRate = 50, Rect? meshBoundaries = null)
	{
		meshBoundaries = meshBoundaries ?? default(SRect).ToBoundingRect();
		if (verticesPerChannel * numberOfChannels > 65000)
		{
			verticesPerChannel = 65000 / numberOfChannels - 1;
		}
		sampleRate = Math.Max(numberOfChannels, sampleRate);
		int num = Math.Max(1, Mathf.CeilToInt((float)samples.Length / ((float)verticesPerChannel * (float)sampleRate)));
		verticesPerChannel = Mathf.RoundToInt((float)samples.Length / ((float)num * (float)sampleRate));
		int num2 = verticesPerChannel * numberOfChannels;
		Vector3[] array = new Vector3[num2];
		int[] array2 = new int[(num2 - 2) * 3];
		float num3 = 1f / (float)samples.Length;
		Vector2 size = new Vector2(meshBoundaries.Value.size.x, (0f - meshBoundaries.Value.size.y) / (float)numberOfChannels);
		Vector2 vector = new Vector2(0f, size.y);
		int num4 = 0;
		int num5 = 0;
		for (int i = 0; i < numberOfChannels; i++)
		{
			Rect rect = new Rect(new Vector2(meshBoundaries.Value.xMin, meshBoundaries.Value.yMax) + i * vector, size);
			float xMin = rect.xMin;
			float xMax = rect.xMax;
			float yMin = rect.yMin;
			float yMax = rect.yMax;
			float num6 = 0f;
			int num7 = 0;
			bool flag = true;
			for (int j = i; j < samples.Length; j += sampleRate)
			{
				num6 = (flag ? Math.Max(num6, samples[j]) : Math.Min(num6, samples[j]));
				num7++;
				if (num7 % num == 0)
				{
					array[num4] = new Vector3(Mathf.Lerp(xMin, xMax, (float)j * num3), Mathf.Lerp(yMax, yMin, num6 * 0.5f + 0.5f), 0f);
					num4++;
					num6 = 0f;
					num7 = 0;
					flag = !flag;
				}
			}
			bool flag2 = false;
			int num8 = (i + 1) * verticesPerChannel - 2;
			for (int k = i * verticesPerChannel; k < num8; k++)
			{
				if (flag2)
				{
					array2[num5++] = k;
					array2[num5++] = k + 1;
					array2[num5++] = k + 2;
				}
				else
				{
					array2[num5++] = k + 2;
					array2[num5++] = k + 1;
					array2[num5++] = k;
				}
				flag2 = !flag2;
			}
		}
		return new Mesh
		{
			vertices = array,
			triangles = array2,
			bounds = new Bounds(new Vector3(meshBoundaries.Value.center.x, meshBoundaries.Value.center.y, 0f), new Vector3(meshBoundaries.Value.size.x, meshBoundaries.Value.size.y, 0f))
		};
	}

	public static Vector2[] CreateAudioVertexDataFromBytes(byte[] compressedData, int waveFormResolution = 300, float waveFormBarWidth = 0.5f)
	{
		Vector2[] array = new Vector2[compressedData.Length * 2];
		Vector2 vector = new Vector2(0.5f / (float)waveFormResolution * waveFormBarWidth, 0f);
		int num = (compressedData.Length <= waveFormResolution * 2).ToInt(1, 2);
		float num2 = 1f / ((float)compressedData.Length * 0.5f) * (float)num;
		float num3 = num2;
		int num4 = 0;
		int num5 = 0;
		while (num5 < array.Length)
		{
			if (num > 1 && num4 != (num4 = (num5 + 4) / compressedData.Length))
			{
				num3 = num2;
			}
			int num6 = num5 / 2;
			Vector2 vector2 = Alpha2.SignedValue(compressedData[num6], compressedData[num6 + 1]);
			Vector2 vector3 = new Vector2(num3, vector2.x);
			Vector2 vector4 = new Vector2(num3, vector2.y);
			array[num5++] = vector3 - vector;
			array[num5++] = vector4 - vector;
			array[num5++] = vector4 + vector;
			array[num5++] = vector3 + vector;
			num3 += num2;
		}
		return array;
	}
}
