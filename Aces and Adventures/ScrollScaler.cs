using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ScrollScaler : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public RectTransform viewPort;

	public float maxZoomAmount = 2f;

	public float zoomAcceleration = 1f;

	public float zoomFriction = 0.01f;

	private RectTransform _rect;

	private float _scale;

	private float _zoomVelocity;

	private bool _pointerOver;

	private float _initialScale;

	private Vector3 _initialLocalPosition;

	private Vector3 _scaleMultiplier = Vector3.one;

	public FloatEvent OnScaleChange;

	private RectTransform rect
	{
		get
		{
			if (!_rect)
			{
				Awake();
			}
			return _rect;
		}
	}

	public float scale => _scale;

	public Vector3 scaleMultiplier
	{
		get
		{
			return _scaleMultiplier;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _scaleMultiplier, value))
			{
				SetScale(_scale);
			}
		}
	}

	public void Reset()
	{
		_scale = _initialScale;
		base.transform.localPosition = _initialLocalPosition;
		_zoomVelocity = 0f;
		Refesh();
	}

	public void Refesh()
	{
		float num = _scale;
		Vector2 v = rect.rect.size.Abs();
		float num2 = v.AbsMax();
		Vector2 v2 = viewPort.rect.size.Abs();
		float num3 = v2.AbsMax();
		Vector2 vector = new Vector2(Mathf.Max(v2.x / v.x, v2.y / v.y) + 0.0001f, Mathf.Max(maxZoomAmount, num3 / num2));
		vector.y = Mathf.Max(vector.x, vector.y);
		_scale += Mathf.Pow(Mathf.Abs(_zoomVelocity), Mathf.Sqrt(1f / _scale)) * Mathf.Sign(_zoomVelocity);
		_scale = Mathf.Clamp(_scale, vector.x, vector.y);
		_rect.localScale = new Vector3(_scale, _scale, _scale).Multiply(_scaleMultiplier);
		_rect.FillRect(viewPort);
		if (num != _scale)
		{
			OnScaleChange.Invoke(_scale);
		}
	}

	public void SetScale(float scale)
	{
		_scale = scale;
		Refesh();
		OnScaleChange.Invoke(_scale);
		_zoomVelocity = 0f;
	}

	private void Awake()
	{
		if (!_rect)
		{
			_rect = base.transform as RectTransform;
			_scale = base.transform.localScale.AbsMax();
			_initialScale = _scale;
			_initialLocalPosition = base.transform.localPosition;
		}
	}

	private void Update()
	{
		_zoomVelocity *= MathUtil.FrictionSubjectToTimeSmooth(zoomFriction, Time.unscaledDeltaTime);
		if (_pointerOver)
		{
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (axis != 0f)
			{
				_zoomVelocity += zoomAcceleration * axis * Time.unscaledDeltaTime;
			}
		}
	}

	private void LateUpdate()
	{
		if (_zoomVelocity != 0f)
		{
			Refesh();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_pointerOver = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_pointerOver = false;
	}
}
