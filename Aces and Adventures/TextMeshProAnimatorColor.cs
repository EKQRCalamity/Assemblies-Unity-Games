using UnityEngine;

public class TextMeshProAnimatorColor : ATextMeshProAnimator
{
	public Color32 color = Color.white;

	public bool useAlphaAsBlend;

	protected override bool _setDirtyOnUpdate
	{
		get
		{
			if (!(base.elapsedTime <= duration))
			{
				return base.timeOfLastPlayRequest < 0f;
			}
			return true;
		}
	}

	protected override void _AnimateVertex(ref Vector3 vertexPosition, ref Color32 vertexColor, ref Rect bounds, int animatedCharacterIndex)
	{
		vertexColor = Color32.Lerp(vertexColor, useAlphaAsBlend ? color.SetAlpha32(byte.MaxValue) : color, base.fadeAmount * (useAlphaAsBlend ? ((float)(int)color.a * 0.003921569f) : 1f));
	}
}
