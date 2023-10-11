using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public struct Color32HDR
{
	public static readonly Color32HDR White = new Color32HDR(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, 0f);

	private const float TO_COLOR = 0.003921569f;

	[ProtoMember(1)]
	public byte r;

	[ProtoMember(2)]
	public byte g;

	[ProtoMember(3)]
	public byte b;

	[ProtoMember(4)]
	public byte a;

	[ProtoMember(5)]
	public float intensity;

	public Color32 color32 => this;

	public Color32HDR(byte r, byte g, byte b, byte a, float intensity)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
		this.intensity = intensity;
	}

	public Color32HDR(Color32 color, float intensity)
		: this(color.r, color.g, color.b, color.a, intensity)
	{
	}

	public static implicit operator Color32(Color32HDR color32Hdr)
	{
		return new Color32(color32Hdr.r, color32Hdr.g, color32Hdr.b, color32Hdr.a);
	}

	public static implicit operator Color(Color32HDR color32Hdr)
	{
		float num = Mathf.Pow(2f, color32Hdr.intensity) * 0.003921569f;
		return new Color((float)(int)color32Hdr.r * num, (float)(int)color32Hdr.g * num, (float)(int)color32Hdr.b * num, (float)(int)color32Hdr.a * 0.003921569f);
	}
}
