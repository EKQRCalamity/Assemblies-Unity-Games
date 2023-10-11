using System;
using UnityEngine;

namespace AmplifyImpostors;

[Serializable]
public class TextureOutput
{
	[SerializeField]
	public int Index = -1;

	[SerializeField]
	public OverrideMask OverrideMask;

	public bool Active = true;

	public string Name = string.Empty;

	public TextureScale Scale = TextureScale.Full;

	public bool SRGB;

	public TextureChannels Channels;

	public TextureCompression Compression = TextureCompression.Normal;

	public ImageFormat ImageFormat = ImageFormat.TGA;

	public TextureOutput()
	{
	}

	public TextureOutput(bool a, string n, TextureScale s, bool sr, TextureChannels c, TextureCompression nc, ImageFormat i)
	{
		Active = a;
		Name = n;
		Scale = s;
		SRGB = sr;
		Channels = c;
		Compression = nc;
		ImageFormat = i;
	}

	public TextureOutput Clone()
	{
		return (TextureOutput)MemberwiseClone();
	}
}
