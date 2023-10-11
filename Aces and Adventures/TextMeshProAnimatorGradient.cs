using UnityEngine;

public class TextMeshProAnimatorGradient : ATextMeshProAnimator
{
	public Gradient gradient;

	public Direction direction;

	[Range(0.001f, 10f)]
	public float repeats = 1f;

	[Range(-10f, 10f)]
	public float shiftSpeed;

	public WrapMethod wrap;

	public bool useGradientAlphaAsFadeAmount;

	private float _shift;

	protected override void OnEnable()
	{
		base.OnEnable();
		_shift = 0f;
	}

	protected override void Update()
	{
		base.Update();
		_shift -= shiftSpeed * Time.deltaTime;
	}

	protected override void _AnimateVertex(ref Vector3 vertexPosition, ref Color32 vertexColor, ref Rect bounds, int animatedCharacterIndex)
	{
		Color c = gradient.Evaluate(_GetSample(bounds, vertexPosition, wrap, direction, _shift, repeats));
		vertexColor = Color32.Lerp(vertexColor, c.SetAlpha(useGradientAlphaAsFadeAmount ? 1f : c.a), base.fadeAmount * (useGradientAlphaAsFadeAmount ? c.a : 1f));
	}
}
