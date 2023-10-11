using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public struct NoiseWaveFloatData
{
	[ProtoMember(1)]
	[UIField(min = 0, max = 10, stepSize = 0.01f)]
	public float waveFrequency;

	[ProtoMember(2)]
	[UIField]
	public RangeF waveRange;

	[ProtoMember(3)]
	[UIField(min = 0, max = 10, stepSize = 0.01f)]
	public float noiseFrequency;

	[ProtoMember(4)]
	[UIField]
	public RangeF noiseRange;

	public NoiseWaveFloatData(float waveFrequency, RangeF waveRange, float noiseFrequency, RangeF noiseRange)
	{
		this.waveFrequency = waveFrequency;
		this.waveRange = waveRange;
		this.noiseFrequency = noiseFrequency;
		this.noiseRange = noiseRange;
	}

	public NoiseWaveFloatData Lerp(NoiseWaveFloatData other, float t)
	{
		return new NoiseWaveFloatData(Mathf.Lerp(waveFrequency, other.waveFrequency, t), waveRange.LerpRange(other.waveRange, t), Mathf.Lerp(noiseFrequency, other.noiseFrequency, t), noiseRange.LerpRange(other.noiseRange, t));
	}

	public override string ToString()
	{
		string text = "";
		if (waveFrequency > 0f || waveRange.range > 0f)
		{
			text += $"Wave Frequency: {waveFrequency}, Wave Range: {waveRange} ";
		}
		if (noiseFrequency > 0f || noiseRange.range > 0f)
		{
			text += $"Noise Frequency: {noiseFrequency}, Noise Range: {noiseRange} ";
		}
		return text;
	}
}
