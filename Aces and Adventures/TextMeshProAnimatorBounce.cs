using TMPro;
using UnityEngine;

public class TextMeshProAnimatorBounce : ATextMeshProAnimator
{
	public Vector3 offset = new Vector3(0f, 1000f, 0f);

	[Range(-1f, 1f)]
	public float initialSpeed;

	[Range(0.1f, 9999f)]
	public float gravity = 0.981f;

	[Range(0f, 1f)]
	public float bounciness = 0.25f;

	[Range(0f, 1f)]
	public float delayPerCharacter = 0.05f;

	public bool reverse;

	private bool _animating;

	private float _offsetMagnitude;

	private Vector3 _offsetDirection;

	private float _initialSpeed;

	private float _gravity;

	protected override bool _hideDuration => true;

	protected override bool _hideFadeInEaseSpeed => true;

	protected override bool _hideFadeOutEaseSpeed => true;

	protected override void _OnPreRenderText(TMP_TextInfo textInfo)
	{
		_offsetMagnitude = offset.magnitude;
		_offsetDirection = offset / _offsetMagnitude.InsureNonZero();
		_gravity = 0f - gravity;
		_initialSpeed = initialSpeed;
		_animating = false;
		base._OnPreRenderText(textInfo);
	}

	protected override void _AnimateVertex(ref Vector3 vertexPosition, ref Color32 vertexColor, ref Rect bounds, int animatedCharacterIndex)
	{
		(float, float) tuple = MathUtil.BounceHeight(Mathf.Max(0f, base.elapsedTime - delayPerCharacter * (float)animatedCharacterIndex), _offsetMagnitude, _initialSpeed, _gravity, 0f, bounciness);
		Vector3 vector = vertexPosition;
		Vector3 offsetDirection = _offsetDirection;
		float num;
		if (!reverse)
		{
			(num, _) = tuple;
		}
		else
		{
			num = _offsetMagnitude - tuple.Item1;
		}
		vertexPosition = vector + offsetDirection * num;
		_animating = _animating || tuple.Item1 > 0f || tuple.Item2 > 0f;
	}

	protected override void Update()
	{
		base.text.havePropertiesChanged = true;
		if (!_animating && base.elapsedTime > 0f)
		{
			_OnFinish();
		}
	}

	public override void Play()
	{
		base.Play();
		_animating = true;
	}
}
