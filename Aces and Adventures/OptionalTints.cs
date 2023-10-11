using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public struct OptionalTints
{
	[ProtoMember(1)]
	[UIField(">Tint", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	public Color32HDR? tint;

	[ProtoMember(2)]
	[UIField(">Second Tint", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
	public Color32HDR? secondTint;

	public Color? GetTint(System.Random random)
	{
		if (!tint.HasValue || !secondTint.HasValue)
		{
			return tint ?? secondTint;
		}
		float num = random.Value();
		return new Color32HDR(((Color32)tint.Value).LerpInHSVOutputRGB((Color32)secondTint.Value, num, hueInShortestDirection: false).SetAlpha32((byte)Mathf.RoundToInt(Mathf.Lerp((int)tint.Value.a, (int)secondTint.Value.a, num))), Mathf.Lerp(tint.Value.intensity, secondTint.Value.intensity, num));
	}

	public static implicit operator bool(OptionalTints optionalTints)
	{
		if (!optionalTints.tint.HasValue)
		{
			return optionalTints.secondTint.HasValue;
		}
		return true;
	}
}
