using System.IO;
using UnityEngine;

namespace AmplifyImpostors;

public static class Texture2DEx
{
	public enum Compression
	{
		None,
		RLE
	}

	private static readonly byte[] Footer = new byte[18]
	{
		84, 82, 85, 69, 86, 73, 83, 73, 79, 78,
		45, 88, 70, 73, 76, 69, 46, 0
	};

	public static byte[] EncodeToTGA(this Texture2D tex, Compression compression = Compression.RLE)
	{
		int num = ((tex.format == TextureFormat.ARGB32 || tex.format == TextureFormat.RGBA32) ? 4 : 3);
		using MemoryStream memoryStream = new MemoryStream(18 + tex.width * tex.height * num);
		using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
		{
			binaryWriter.Write((byte)0);
			binaryWriter.Write((byte)0);
			binaryWriter.Write((byte)((compression == Compression.None) ? 2u : 10u));
			binaryWriter.Write((short)0);
			binaryWriter.Write((short)0);
			binaryWriter.Write((byte)0);
			binaryWriter.Write((short)0);
			binaryWriter.Write((short)0);
			binaryWriter.Write((short)tex.width);
			binaryWriter.Write((short)tex.height);
			binaryWriter.Write((byte)(num * 8));
			binaryWriter.Write((byte)8);
			Color32[] pixels = tex.GetPixels32();
			if (compression == Compression.None)
			{
				for (int i = 0; i < pixels.Length; i++)
				{
					Color32 color = pixels[i];
					binaryWriter.Write(color.r);
					binaryWriter.Write(color.g);
					binaryWriter.Write(color.b);
					if (num == 4)
					{
						binaryWriter.Write(color.a);
					}
				}
			}
			else
			{
				int num2 = 0;
				int num3 = 0;
				while (num2 < pixels.Length)
				{
					Color32 color2 = pixels[num2];
					bool flag = num2 != pixels.Length - 1 && Equals(pixels[num2], pixels[num2 + 1]);
					int num4 = (num2 / tex.width + 1) * tex.width;
					int num5 = Mathf.Min(num2 + 128, pixels.Length, num4);
					for (num3 = num2 + 1; num3 < num5; num3++)
					{
						bool flag2 = Equals(pixels[num3 - 1], pixels[num3]);
						if ((!flag && flag2) || (flag && !flag2))
						{
							break;
						}
					}
					int num6 = num3 - num2;
					if (flag)
					{
						binaryWriter.Write((byte)((uint)(num6 - 1) | 0x80u));
						binaryWriter.Write(color2.r);
						binaryWriter.Write(color2.g);
						binaryWriter.Write(color2.b);
						if (num == 4)
						{
							binaryWriter.Write(color2.a);
						}
					}
					else
					{
						binaryWriter.Write((byte)(num6 - 1));
						for (int j = num2; j < num3; j++)
						{
							Color32 color3 = pixels[j];
							binaryWriter.Write(color3.r);
							binaryWriter.Write(color3.g);
							binaryWriter.Write(color3.b);
							if (num == 4)
							{
								binaryWriter.Write(color3.a);
							}
						}
					}
					num2 = num3;
				}
			}
			binaryWriter.Write(0);
			binaryWriter.Write(0);
			binaryWriter.Write(Footer);
		}
		return memoryStream.ToArray();
	}

	private static bool Equals(Color32 first, Color32 second)
	{
		if (first.r == second.r && first.g == second.g && first.b == second.b)
		{
			return first.a == second.a;
		}
		return false;
	}
}
