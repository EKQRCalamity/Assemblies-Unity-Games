using UnityEngine;

public class TextMeshProAnimatorOffset : ATextMeshProAnimator
{
	public Vector3 offsetMin;

	public Vector3 offsetMax;

	public AnimationCurve curve = AnimationCurve.Constant(1f, 1f, 1f);

	public Direction direction;

	[Range(0.001f, 10f)]
	public float repeats = 1f;

	[Range(-10f, 10f)]
	public float shiftSpeed;

	public WrapMethod wrap;

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
		vertexPosition += offsetMin.Lerp(offsetMax, curve.Evaluate(_GetSample(bounds, vertexPosition, wrap, direction, _shift, repeats))) * base.fadeAmount;
	}
}
