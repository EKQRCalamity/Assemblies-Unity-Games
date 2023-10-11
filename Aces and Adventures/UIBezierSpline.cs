using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBezierSpline : MaskableGraphic, ICanvasRaycastFilter, ISelectableMarqueeFilter
{
	public enum UVWrapMode
	{
		Repeat,
		Stretch
	}

	private const float THICKNESS_MIN = 0.01f;

	private const float THICKNESS_MAX = 100f;

	private const int MAX_SEGMENTS_MIN = 1;

	private const int MAX_SEGMENTS_MAX = 1000;

	private const float MAX_CURVATURE_RADIANS = 0.08726646f;

	[Header("Appearance")]
	[SerializeField]
	[Range(0.01f, 100f)]
	protected float _thickness = 1f;

	[SerializeField]
	protected Sprite _sprite;

	[SerializeField]
	[HideInInspectorIf("_hideSpriteUVWrap", false)]
	protected UVWrapMode _spriteUVWrap;

	[SerializeField]
	protected bool _gradientEnabled;

	[SerializeField]
	[HideInInspectorIf("_hideGradient", false)]
	protected Gradient _gradient = new Gradient();

	[SerializeField]
	protected float _fadeStartDistance;

	[SerializeField]
	protected float _fadeEndDistance;

	[Header("Resolution")]
	[SerializeField]
	[Range(1f, 1000f)]
	protected int _maxSegments = 100;

	[SerializeField]
	[Range(0f, 1f)]
	protected float _segmentCurvatureThreshold = 0.5f;

	[Header("Advanced==============================================================================================")]
	[Header("UV Shifting")]
	[SerializeField]
	[HideInInspectorIf("_hideSpriteUVWrap", false)]
	protected float _uShift;

	[SerializeField]
	[HideInInspectorIf("_hideSpriteUVWrap", false)]
	protected float _vShift;

	[Header("Gradient")]
	[SerializeField]
	[HideInInspectorIf("_hideGradient", false)]
	protected float _gradiantArcLength;

	[SerializeField]
	[HideInInspectorIf("_hideGradient", false)]
	protected float _gradientShift;

	[SerializeField]
	[HideInInspectorIf("_hideGradient", false)]
	protected WrapMethod _gradientWrapMethod;

	[Header("Thickness Curve")]
	[SerializeField]
	protected bool _thicknessCurveEnabled;

	[SerializeField]
	[HideInInspectorIf("_hideThicknessCurve", false)]
	protected AnimationCurve _thicknessCurve = AnimationCurve.Constant(0f, 1f, 1f);

	[SerializeField]
	[HideInInspectorIf("_hideThicknessCurve", false)]
	protected float _thicknessCurveArcLength;

	[SerializeField]
	[HideInInspectorIf("_hideThicknessCurve", false)]
	protected float _thicknessCurveShift;

	[Header("Offset Curve")]
	[SerializeField]
	protected bool _offsetCurveEnabled;

	[SerializeField]
	[HideInInspectorIf("_hideOffsetCurve", false)]
	protected AnimationCurve _offsetCurve = AnimationCurve.Constant(0f, 1f, 0f);

	[SerializeField]
	[HideInInspectorIf("_hideOffsetCurve", false)]
	protected float _offsetCurveArcLength;

	[SerializeField]
	[HideInInspectorIf("_hideOffsetCurve", false)]
	protected float _offsetCurveMagnitude;

	[SerializeField]
	[HideInInspectorIf("_hideOffsetCurve", false)]
	protected float _offsetCurveShift;

	[SerializeField]
	[HideInInspectorIf("_hideOffsetCurve", false)]
	protected float _offsetCurveFadePower = 0.5f;

	[Header("Events================================================================================================")]
	[SerializeField]
	protected BezierSplineEvent _OnSplineChange;

	[Range(0f, 100f)]
	public float selectionThicknessPadding = 2f;

	private BezierSpline _bezierSpline;

	private bool _isRectTransformDirty;

	public float thickness
	{
		get
		{
			return _thickness;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _thickness, Mathf.Clamp(value, 0.01f, 100f)))
			{
				_SetRectTransformDirty();
			}
		}
	}

	public Sprite sprite
	{
		get
		{
			return _sprite;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _sprite, value))
			{
				SetAllDirty();
			}
		}
	}

	public UVWrapMode spriteUVWrap
	{
		get
		{
			return _spriteUVWrap;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _spriteUVWrap, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public float arcLengthPerSegment => Mathf.Sqrt(thickness);

	public int maxSegments
	{
		get
		{
			return _maxSegments;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _maxSegments, Mathf.Clamp(value, 1, 1000)))
			{
				SetVerticesDirty();
			}
		}
	}

	public float segmentCurvatureThreshold
	{
		get
		{
			return _segmentCurvatureThreshold;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _segmentCurvatureThreshold, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public BezierSpline bezierSpline => _bezierSpline ?? (_bezierSpline = new BezierSpline());

	public float uShift
	{
		get
		{
			return _uShift;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _uShift, value) && (bool)sprite)
			{
				SetVerticesDirty();
			}
		}
	}

	public float vShift
	{
		get
		{
			return _vShift;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _vShift, value) && (bool)sprite)
			{
				SetVerticesDirty();
			}
		}
	}

	public bool gradientEnabled
	{
		get
		{
			return _gradientEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _gradientEnabled, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public Gradient gradient
	{
		get
		{
			return _gradient;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _gradient, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public float gradientArcLength
	{
		get
		{
			return _gradiantArcLength;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _gradiantArcLength, Mathf.Max(0f, value)) && gradientEnabled)
			{
				SetVerticesDirty();
			}
		}
	}

	public float gradientShift
	{
		get
		{
			return _gradientShift;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _gradientShift, value) && gradientEnabled)
			{
				SetVerticesDirty();
			}
		}
	}

	public WrapMethod gradientWrapMethod
	{
		get
		{
			return _gradientWrapMethod;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _gradientWrapMethod, value) && gradientEnabled)
			{
				SetVerticesDirty();
			}
		}
	}

	public float fadeStartDistance
	{
		get
		{
			return _fadeStartDistance;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _fadeStartDistance, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public float fadeEndDistance
	{
		get
		{
			return _fadeEndDistance;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _fadeEndDistance, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public bool fadeExtremaEnabled
	{
		get
		{
			if (!(fadeStartDistance > 0f))
			{
				return fadeEndDistance > 0f;
			}
			return true;
		}
	}

	public bool thicknessCurveEnabled
	{
		get
		{
			return _thicknessCurveEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _thicknessCurveEnabled, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public AnimationCurve thicknessCurve
	{
		get
		{
			return _thicknessCurve;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _thicknessCurve, value) && thicknessCurveEnabled)
			{
				SetVerticesDirty();
			}
		}
	}

	public float thicknessCurveArcLength
	{
		get
		{
			return _thicknessCurveArcLength;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _thicknessCurveArcLength, Mathf.Max(0f, value)) && thicknessCurveEnabled)
			{
				SetVerticesDirty();
			}
		}
	}

	public float thicknessCurveShift
	{
		get
		{
			return _thicknessCurveShift;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _thicknessCurveShift, value) && thicknessCurveEnabled)
			{
				SetVerticesDirty();
			}
		}
	}

	public bool offsetCurveEnabled
	{
		get
		{
			return _offsetCurveEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _offsetCurveEnabled, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public AnimationCurve offsetCurve
	{
		get
		{
			return _offsetCurve;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _offsetCurve, value) && offsetCurveEnabled)
			{
				SetVerticesDirty();
			}
		}
	}

	public float offsetCurveArcLength
	{
		get
		{
			return _offsetCurveArcLength;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _offsetCurveArcLength, Mathf.Max(0f, value)) && offsetCurveEnabled)
			{
				SetVerticesDirty();
			}
		}
	}

	public float offsetCurveMagnitude
	{
		get
		{
			return _offsetCurveMagnitude;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _offsetCurveMagnitude, Mathf.Max(0f, value)) && offsetCurveEnabled)
			{
				_SetRectTransformDirty();
			}
		}
	}

	public float offsetCurveShift
	{
		get
		{
			return _offsetCurveShift;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _offsetCurveShift, value) && offsetCurveEnabled)
			{
				SetVerticesDirty();
			}
		}
	}

	public float offsetCurveFadePower
	{
		get
		{
			return _offsetCurveFadePower;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _offsetCurveFadePower, value) && offsetCurveEnabled)
			{
				SetVerticesDirty();
			}
		}
	}

	public BezierSplineEvent OnSplineChange => _OnSplineChange ?? (_OnSplineChange = new BezierSplineEvent());

	public override Texture mainTexture
	{
		get
		{
			if (!sprite)
			{
				if (!material || !material.mainTexture)
				{
					return Graphic.s_WhiteTexture;
				}
				return material.mainTexture;
			}
			return sprite.texture;
		}
	}

	private bool _hideSpriteUVWrap => !sprite;

	private bool _hideGradient => !_gradientEnabled;

	private bool _hideThicknessCurve => !_thicknessCurveEnabled;

	private bool _hideOffsetCurve => !_offsetCurveEnabled;

	private float _GetSample(float currentArcLength, float arcLength, float shift)
	{
		return currentArcLength / ((arcLength > 0f) ? arcLength : bezierSpline.arcLength) - shift;
	}

	private float _GetCurveOffset(float t, float currentArcLength)
	{
		return offsetCurve.Evaluate(_GetSample(currentArcLength, offsetCurveArcLength, offsetCurveShift)) * offsetCurveMagnitude * (1f - Mathf.Pow(Mathf.Abs(0.5f - t) * 2f, offsetCurveFadePower));
	}

	private float _GetCurveThickness(float currentArcLength)
	{
		return thickness * thicknessCurve.Evaluate(_GetSample(currentArcLength, thicknessCurveArcLength, thicknessCurveShift));
	}

	private float _GetFadeStartAndEndAlpha(float currentArcLength)
	{
		float num = 1f;
		if (fadeStartDistance > 0f && currentArcLength < fadeStartDistance)
		{
			num *= currentArcLength / fadeStartDistance;
		}
		float num2;
		if (fadeEndDistance > 0f && (num2 = bezierSpline.arcLength - currentArcLength) < fadeEndDistance)
		{
			num *= num2 / fadeEndDistance;
		}
		return num;
	}

	private void _UpdateRectTransformBounds()
	{
		if (!_isRectTransformDirty)
		{
			return;
		}
		_isRectTransformDirty = false;
		if (!_bezierSpline || bezierSpline.curveCount == 0)
		{
			return;
		}
		using PoolStructListHandle<Vector3> poolStructListHandle = Pools.UseStructList(bezierSpline.points);
		for (int i = 0; i < poolStructListHandle.Count; i++)
		{
			poolStructListHandle[i] = base.rectTransform.localToWorldMatrix.MultiplyPoint(poolStructListHandle[i]);
		}
		Bounds worldBounds = new Bounds(poolStructListHandle[0], Vector3.zero);
		for (int j = 1; j < bezierSpline.points.Count; j++)
		{
			worldBounds.Encapsulate(poolStructListHandle[j]);
		}
		Camera camera = (base.canvas.worldCamera ? base.canvas.worldCamera : (CameraManager.Instance.screenSpaceUICamera ? CameraManager.Instance.screenSpaceUICamera : CameraManager.Instance.mainCamera));
		base.rectTransform.SetWorldCornersPreserveScale(worldBounds.GetViewPortRect(camera).ToRect3D().ViewportToWorldRect(camera));
		for (int k = 0; k < poolStructListHandle.Count; k++)
		{
			poolStructListHandle[k] = base.rectTransform.worldToLocalMatrix.MultiplyPoint(poolStructListHandle[k]);
		}
		bezierSpline.SetData(poolStructListHandle);
		base.rectTransform.PadLocal(Vector2.one * (thickness + (offsetCurveEnabled ? Mathf.Abs(offsetCurveMagnitude) : 0f) + selectionThicknessPadding));
	}

	private void _SetRectTransformDirty()
	{
		_isRectTransformDirty = true;
		SetVerticesDirty();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_SetRectTransformDirty();
	}

	private void LateUpdate()
	{
		_UpdateRectTransformBounds();
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		if (!_bezierSpline)
		{
			return;
		}
		int num = Mathf.Min(maxSegments, Mathf.RoundToInt(bezierSpline.arcLength / arcLengthPerSegment));
		float num2 = 1f / (float)num;
		float num3 = 0f;
		Vector3? vector = null;
		Color? color = null;
		float? num4 = null;
		float? num5 = null;
		Vector3? vector2 = null;
		float num6 = Mathf.Cos(segmentCurvatureThreshold * 0.08726646f) + MathUtil.BigEpsilon;
		float num7 = thickness * 0.025f;
		float num8 = num7;
		int num9 = 0;
		Sprite sprite = this.sprite;
		float num10 = (((bool)sprite && spriteUVWrap != UVWrapMode.Stretch) ? (1f / (2f * this.sprite.rect.size.AspectRatio() * thickness)) : (1f / bezierSpline.arcLength));
		float y = 1f - vShift;
		float y2 = 0f - vShift;
		bool flag = gradientEnabled || fadeExtremaEnabled;
		for (int i = 0; i < num + 1; i++)
		{
			float distanceTraveled = bezierSpline.GetDistanceTraveled(num3);
			Vector3 direction = bezierSpline.GetDirection(num3);
			bool flag2 = i < num && vector.HasValue && Vector3.Dot(vector.Value, direction) > num6;
			Color color2 = (gradientEnabled ? gradient.Evaluate(gradientWrapMethod.WrapShift(_GetSample(distanceTraveled, gradientArcLength, gradientShift))).Multiply(this.color) : this.color).MultiplyAlpha(_GetFadeStartAndEndAlpha(distanceTraveled));
			if (flag)
			{
				flag2 = flag2 && color.HasValue && (color2.ToVector4() - color.Value.ToVector4()).sqrMagnitude < 0.0025f;
			}
			float num11 = (thicknessCurveEnabled ? _GetCurveThickness(distanceTraveled) : thickness);
			if (thicknessCurveEnabled)
			{
				flag2 = flag2 && num4.HasValue && Math.Abs(num11 - num4.Value) < num7;
			}
			float num12 = (offsetCurveEnabled ? _GetCurveOffset(num3, distanceTraveled) : 0f);
			if (offsetCurveEnabled)
			{
				flag2 = flag2 && num5.HasValue && Math.Abs(num12 - num5.Value) < num8;
			}
			if (flag2)
			{
				num3 += num2;
				continue;
			}
			vector = direction;
			color = color2;
			num4 = num11;
			num5 = num12;
			Vector3 position = bezierSpline.GetPosition(num3);
			Vector3 vector3 = bezierSpline.GetNormal(num3, Vector3.back);
			if (offsetCurveEnabled)
			{
				position += vector3 * num12;
				vector3 = (vector2.HasValue ? Vector3.Cross(position - vector2.Value, Vector3.back).normalized : vector3);
				vector2 = position;
			}
			Vector3 vector4 = vector3 * num11;
			float x = (sprite ? (num10 * distanceTraveled - uShift) : 0f);
			vh.AddVert(position + vector4, color2, new Vector2(x, y));
			vh.AddVert(position - vector4, color2, new Vector2(x, y2));
			num3 += num2;
			if (i < num)
			{
				int num13 = num9 + num9;
				int num14 = num13 + 3;
				vh.AddTriangle(num13, num14, num13 + 1);
				vh.AddTriangle(num13, num13 + 2, num14);
				num9++;
			}
		}
	}

	public UIBezierSpline SetDataLocal(List<Vector3> localPoints)
	{
		if (localPoints.SequenceEqual(bezierSpline.points, Vector3EqualityComparerApproximate.Default))
		{
			return this;
		}
		bezierSpline.SetData(localPoints);
		_SetRectTransformDirty();
		OnSplineChange.Invoke(base.transform.localToWorldMatrix, bezierSpline);
		return this;
	}

	public UIBezierSpline SetDataWorld(List<Vector3> worldPoints)
	{
		using PoolStructListHandle<Vector3> poolStructListHandle = Pools.UseStructList(worldPoints);
		for (int i = 0; i < poolStructListHandle.Count; i++)
		{
			poolStructListHandle[i] = base.transform.worldToLocalMatrix.MultiplyPoint(poolStructListHandle[i]);
		}
		return SetDataLocal(poolStructListHandle);
	}

	public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
	{
		if (!_bezierSpline)
		{
			return false;
		}
		if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(base.rectTransform, sp, eventCamera, out var worldPoint))
		{
			return false;
		}
		Vector3 vector = base.transform.worldToLocalMatrix.MultiplyPoint(worldPoint);
		if (!thicknessCurveEnabled && !offsetCurveEnabled)
		{
			return (bezierSpline.GetPointOnSplineNearestToPoint(vector) - vector).magnitude <= thickness + selectionThicknessPadding;
		}
		float normalizedPositionFromPoint = bezierSpline.GetNormalizedPositionFromPoint(vector);
		float distanceTraveled = bezierSpline.GetDistanceTraveled(normalizedPositionFromPoint);
		Vector3 vector2 = (offsetCurveEnabled ? (bezierSpline.GetNormal(normalizedPositionFromPoint, Vector3.back) * _GetCurveOffset(normalizedPositionFromPoint, distanceTraveled)) : Vector3.zero);
		float num = (thicknessCurveEnabled ? _GetCurveThickness(distanceTraveled) : thickness);
		return (bezierSpline.GetPosition(normalizedPositionFromPoint) + vector2 - vector).magnitude <= num + selectionThicknessPadding;
	}

	public bool IsMarqueeRectValid(Rect3D worldMarqueeRect)
	{
		if ((bool)_bezierSpline)
		{
			return (base.transform.worldToLocalMatrix * worldMarqueeRect).Intersects(bezierSpline);
		}
		return false;
	}
}
