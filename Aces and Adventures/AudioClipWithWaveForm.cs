using UnityEngine;

public class AudioClipWithWaveForm
{
	public AudioClip clip;

	public Texture2D texture;

	public void Destroy()
	{
		if ((bool)clip)
		{
			Object.Destroy(clip);
		}
		clip = null;
		if ((bool)texture)
		{
			Object.Destroy(texture);
		}
		texture = null;
	}

	public static implicit operator bool(AudioClipWithWaveForm data)
	{
		if (data != null && (bool)data.clip)
		{
			return data.texture;
		}
		return false;
	}

	public static implicit operator AudioClip(AudioClipWithWaveForm data)
	{
		return data?.clip;
	}

	public static implicit operator Texture2D(AudioClipWithWaveForm data)
	{
		return data?.texture;
	}
}
