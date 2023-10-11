using System;
using UnityEngine;

public class TextMeshProAnimatorTyper : ATextMeshProAnimatorTyper
{
	public Gradient fadeInGradient = new Gradient
	{
		alphaKeys = new GradientAlphaKey[2]
		{
			new GradientAlphaKey(0f, 0f),
			new GradientAlphaKey(1f, 1f)
		}
	};

	protected override void _AnimateCharacter(float t, ref Vector3 vertexPosition, ref Color32 vertexColor, ref Rect bounds, int animatedCharacterIndex)
	{
		vertexColor = vertexColor.SetAlpha32((byte)Math.Round(fadeInGradient.Evaluate(t).a * (float)(int)vertexColor.a));
	}
}
