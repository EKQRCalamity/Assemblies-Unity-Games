using ProtoBuf;

[ProtoContract]
[UIField]
public struct NoiseWaveVector3Data
{
	[ProtoMember(1, IsRequired = true)]
	[UIField(validateOnChange = true)]
	public bool uniform;

	[ProtoMember(2)]
	[UIField]
	[UIDeepValueChange]
	public NoiseWaveFloatData x;

	[ProtoMember(3)]
	[UIField]
	[UIHideIf("_hideUniform")]
	[UIDeepValueChange]
	public NoiseWaveFloatData y;

	[ProtoMember(4)]
	[UIField]
	[UIHideIf("_hideUniform")]
	[UIDeepValueChange]
	public NoiseWaveFloatData z;

	private bool _hideUniform => uniform;

	public NoiseWaveVector3Data(NoiseWaveFloatData data, bool uniform = true)
	{
		this.uniform = uniform;
		x = (y = (z = data));
	}

	public NoiseWaveVector3Data Lerp(NoiseWaveVector3Data other, float t)
	{
		NoiseWaveVector3Data result = default(NoiseWaveVector3Data);
		result.x = x.Lerp(other.x, t);
		result.y = y.Lerp(other.y, t);
		result.z = z.Lerp(other.z, t);
		return result;
	}

	public override string ToString()
	{
		string text = x.ToString();
		string text2 = y.ToString();
		string text3 = z.ToString();
		string text4 = "";
		if (text.HasVisibleCharacter())
		{
			text4 = text4 + (uniform ? "U" : "X") + ": " + text;
		}
		if (!uniform && text2.HasVisibleCharacter())
		{
			text4 = text4 + "Y: " + text2;
		}
		if (!uniform && text3.HasVisibleCharacter())
		{
			text4 = text4 + "Z: " + text3;
		}
		return text4;
	}

	[ProtoBeforeSerialization]
	private void _ProtoBeforeSerialization()
	{
		if (uniform)
		{
			z = (y = x);
		}
	}
}
