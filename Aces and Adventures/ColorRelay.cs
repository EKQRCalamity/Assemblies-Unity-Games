using UnityEngine;

public class ColorRelay : MonoBehaviour
{
	private static readonly Color NULL_COLOR = new Color(-1f, -1f, -1f, -1f);

	[Range(0f, 5f)]
	public float responseTime;

	[Range(0.05f, 5f)]
	public float animationTime;

	[Range(0f, 100f)]
	public int maxQueued;

	public bool autoCompleteOnEnable;

	private Color _color = NULL_COLOR;

	private bool _enabledThisFrame;

	private FloatRelayer _hueRelayer = new FloatRelayer();

	private FloatRelayer _saturationRelayer = new FloatRelayer();

	private FloatRelayer _valueRelayer = new FloatRelayer();

	private FloatRelayer _alphaRelayer = new FloatRelayer();

	public ColorEvent OnColorChange;

	public Color32Event OnColor32Change;

	public float hue => Colors.WrapHue(_hueRelayer.value ?? _color.r);

	public float saturation => _saturationRelayer.value ?? _color.g;

	public float value => _valueRelayer.value ?? _color.b;

	public float alpha => _alphaRelayer.value ?? _color.a;

	public Color color => Color.HSVToRGB(hue, saturation, value).SetAlpha(alpha);

	private void OnEnable()
	{
		_enabledThisFrame = true;
	}

	private void Update()
	{
		float deltaTime = GameUtil.GetDeltaTime(useScaledTime: false);
		if (_hueRelayer.Update(deltaTime) | _saturationRelayer.Update(deltaTime) | _valueRelayer.Update(deltaTime) | _alphaRelayer.Update(deltaTime))
		{
			_SignalColorChange(color);
		}
		_enabledThisFrame = false;
	}

	private void OnDisable()
	{
		if ((bool)this)
		{
			_hueRelayer.Clear();
			_saturationRelayer.Clear();
			_valueRelayer.Clear();
			_alphaRelayer.Clear();
		}
	}

	private void _SignalColorChange(Color color)
	{
		OnColorChange.Invoke(color);
		OnColor32Change.Invoke(color);
	}

	public void ChangeColor(Color color)
	{
		if (autoCompleteOnEnable && _enabledThisFrame)
		{
			_color = color.ToHSV();
			_SignalColorChange(this.color);
			return;
		}
		Color color2 = color.ToHSV();
		if (_color == NULL_COLOR)
		{
			_color = color2;
		}
		_hueRelayer.Add(_color.r, Colors.HueInShortestDirection(_color.r, color2.r), animationTime, responseTime, maxQueued);
		_saturationRelayer.Add(_color.g, color2.g, animationTime, responseTime, maxQueued);
		_valueRelayer.Add(_color.b, color2.b, animationTime, responseTime, maxQueued);
		_alphaRelayer.Add(_color.a, color2.a, animationTime, responseTime, maxQueued);
		_color = color2;
	}

	public void ChangeColor(Color32 color)
	{
		ChangeColor((Color)color);
	}

	public void SetAlpha(float alpha)
	{
		_color = _color.SetAlpha(alpha);
		_SignalColorChange(color);
	}
}
