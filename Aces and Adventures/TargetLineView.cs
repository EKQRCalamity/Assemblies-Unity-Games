using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TargetLineView : MonoBehaviour
{
	private static readonly int COLOR_ID = Shader.PropertyToID("Color");

	private static readonly int TO_TARGET_LERP_ID = Shader.PropertyToID("ToTargetLerp");

	private static readonly int TANGENT_SCALE_ID = Shader.PropertyToID("TangentScale");

	private static readonly int END_CAP_SCALE_ID = Shader.PropertyToID("EndCapScale");

	private static readonly int TARGET_POSITION_ID = Shader.PropertyToID("TargetTransform_position");

	private static readonly int TARGET_ANGLES_ID = Shader.PropertyToID("TargetTransform_angles");

	private static readonly int TARGET_SCALE_ID = Shader.PropertyToID("TargetTransform_scale");

	private static readonly ResourceBlueprint<GameObject> _Blueprint = "GameState/TargetLineView";

	private static float? _EndCapScale;

	public const TargetLineTags TargetTags = TargetLineTags.Target | TargetLineTags.Target2 | TargetLineTags.Target3 | TargetLineTags.Target4 | TargetLineTags.Target5 | TargetLineTags.Target6 | TargetLineTags.Target7 | TargetLineTags.Target8 | TargetLineTags.Target9 | TargetLineTags.Target10;

	private static HashSet<TargetLineView> _Active = new HashSet<TargetLineView>();

	[Range(0.001f, 1f)]
	public float fadeTime = 0.25f;

	[Range(1f, 100f)]
	public float colorEaseSpeed = 5f;

	private Transform _start;

	private Transform _end;

	private Color _color;

	private Color _targetColor;

	private Quaternion _startRotation;

	private Quaternion? _targetStartRotation;

	private Quaternion _endRotation;

	private Quaternion? _targetEndRotation;

	private Vector3 _endOffset;

	private Vector3? _targetEndOffset;

	private float _tangentScale;

	private float _targetTangentScale;

	private float _endTangentScale;

	private float _targetEndTangentScale;

	private float _endCapScale;

	private float _targetEndCapScale;

	private float? _startTime;

	private float? _finishTime;

	private float _lerp;

	private float _visibility;

	private object _owner;

	private TargetLineTags _tags;

	private Action<TargetLineView> _onUpdate;

	private VisualEffect _vfx;

	private static float EndCapSCale
	{
		get
		{
			float valueOrDefault = _EndCapScale.GetValueOrDefault();
			if (!_EndCapScale.HasValue)
			{
				valueOrDefault = _Blueprint.value.GetComponent<VisualEffect>().GetFloat(END_CAP_SCALE_ID);
				_EndCapScale = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	private VisualEffect vfx => this.CacheComponentInChildren(ref _vfx);

	public Quaternion? targetEndRotation
	{
		get
		{
			return _targetEndRotation;
		}
		set
		{
			_targetEndRotation = value;
		}
	}

	public float targetTangentScale
	{
		get
		{
			return _targetTangentScale;
		}
		set
		{
			_targetTangentScale = value;
		}
	}

	public float targetEndTangentScale
	{
		get
		{
			return _targetEndTangentScale;
		}
		set
		{
			_targetEndTangentScale = value;
		}
	}

	public static TargetLineView Add(object owner, Color color, Transform start, Transform end, Quaternion? startRotation = null, Quaternion? endRotation = null, TargetLineTags tags = (TargetLineTags)0, float tangentScale = 1f, float endTangentScale = 1f, Action<TargetLineView> onUpdate = null, Vector3? endOffset = null, float endCapScale = 1f)
	{
		if (color.a <= 0f)
		{
			return null;
		}
		TargetLineView component = Pools.Unpool(_Blueprint).GetComponent<TargetLineView>();
		component._start = start;
		component._end = end;
		component._owner = owner;
		component._color = (component._targetColor = color);
		component._startRotation = startRotation ?? Quaternion.identity;
		component._targetStartRotation = startRotation;
		component._endRotation = endRotation ?? Quaternion.identity;
		component._targetEndRotation = endRotation;
		component._endOffset = endOffset ?? Vector3.zero;
		component._targetEndOffset = endOffset;
		component._tangentScale = (component._targetTangentScale = tangentScale);
		component._endTangentScale = (component._targetEndTangentScale = endTangentScale);
		component._endCapScale = (component._targetEndCapScale = endCapScale);
		component._tags = tags;
		component._onUpdate = onUpdate;
		if (onUpdate != null)
		{
			onUpdate(component);
			component._ForceToTargets();
		}
		_Active.Add(component);
		return component;
	}

	public static TargetLineView AddUnique(object owner, Color color, Transform start, Transform end, Quaternion? startRotation = null, Quaternion? endRotation = null, TargetLineTags tags = (TargetLineTags)0, float tangentScale = 1f, float endTangentScale = 1f, Action<TargetLineView> onUpdate = null, Vector3? endOffset = null, float endCapScale = 1f)
	{
		foreach (TargetLineView item in _Active)
		{
			if (item._start == start && item._end == end && item._tags == tags)
			{
				item._owner = owner;
				if (color.a <= 0f)
				{
					item.Finish();
					return null;
				}
				item._targetColor = color;
				if (startRotation.HasValue)
				{
					item._targetStartRotation = startRotation;
				}
				if (endRotation.HasValue)
				{
					item._targetEndRotation = endRotation;
				}
				if (endOffset.HasValue)
				{
					item._targetEndOffset = endOffset;
				}
				item._targetTangentScale = tangentScale;
				item._targetEndTangentScale = endTangentScale;
				item._targetEndCapScale = endCapScale;
				if (item._finishTime.HasValue)
				{
					item._startTime = Time.time - (item.fadeTime - (Time.time - item._finishTime.Value));
				}
				item._finishTime = null;
				item._onUpdate = onUpdate ?? item._onUpdate;
				return null;
			}
		}
		return Add(owner, color, start, end, startRotation, endRotation, tags, tangentScale, endTangentScale, onUpdate, endOffset, endCapScale);
	}

	public static void RemoveOwnedBy(object owner, TargetLineTags tags = (TargetLineTags)0)
	{
		foreach (TargetLineView item in _Active.EnumerateSafe())
		{
			if (item._owner == owner && (tags == (TargetLineTags)0 || item._tags == tags))
			{
				item.Finish();
			}
		}
	}

	public static void RemoveOwnedByExcept(object owner, TargetLineTags tags)
	{
		foreach (TargetLineView item in _Active.EnumerateSafe())
		{
			if (item._owner == owner && (item._tags & tags) == 0)
			{
				item.Finish();
			}
		}
	}

	public static void RemoveStartingAt(Transform start, TargetLineTags tags = (TargetLineTags)0)
	{
		foreach (TargetLineView item in _Active.EnumerateSafe())
		{
			if (item._start == start && (tags == (TargetLineTags)0 || item._tags == tags))
			{
				item.Finish();
			}
		}
	}

	public static void RemoveEndingAt(Transform end, TargetLineTags tags = (TargetLineTags)0)
	{
		foreach (TargetLineView item in _Active.EnumerateSafe())
		{
			if (item._end == end && (tags == (TargetLineTags)0 || item._tags == tags))
			{
				item.Finish();
			}
		}
	}

	public static void RemoveAtExtrema(Transform start, Transform end, TargetLineTags tags = (TargetLineTags)0)
	{
		foreach (TargetLineView item in _Active.EnumerateSafe())
		{
			if (item._start == start && item._end == end && (tags == (TargetLineTags)0 || item._tags == tags))
			{
				item.Finish();
			}
		}
	}

	public static void RemoveAll(TargetLineTags tags = (TargetLineTags)0)
	{
		foreach (TargetLineView item in _Active.EnumerateSafe())
		{
			if (tags == (TargetLineTags)0 || (item._tags & tags) != 0)
			{
				item.Finish();
			}
		}
	}

	public static void RemoveAllExcept(TargetLineTags tags)
	{
		foreach (TargetLineView item in _Active.EnumerateSafe())
		{
			if ((item._tags & tags) == 0)
			{
				item.Finish();
			}
		}
	}

	private void _SetLerp(float t)
	{
		t *= _visibility;
		vfx.SetFloat(TO_TARGET_LERP_ID, t);
		vfx.SetVector4(COLOR_ID, _color.SetAlpha(t));
		_lerp = t;
	}

	private void _ForceToTargets()
	{
		_color = _targetColor;
		if (_targetStartRotation.HasValue)
		{
			_startRotation = _targetStartRotation.Value;
		}
		if (_targetEndRotation.HasValue)
		{
			_endRotation = _targetEndRotation.Value;
		}
		if (_targetEndOffset.HasValue)
		{
			_endOffset = _targetEndOffset.Value;
		}
		_tangentScale = _targetTangentScale;
		_endTangentScale = _targetEndTangentScale;
	}

	private void OnDisable()
	{
		_start = (_end = null);
		_startRotation = (_endRotation = Quaternion.identity);
		_targetStartRotation = (_targetEndRotation = null);
		_targetEndOffset = null;
		_owner = null;
		_startTime = (_finishTime = null);
		_tags = (TargetLineTags)0;
		_onUpdate = null;
		_Active.Remove(this);
	}

	private void OnEnable()
	{
		_startTime = Time.time;
		_visibility = 1f;
	}

	private void Update()
	{
		float num = 1f;
		if (_finishTime.HasValue)
		{
			num = Mathf.Clamp01(1f - (Time.time - _finishTime.Value) / fadeTime);
			if (num == 0f)
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (_startTime.HasValue)
		{
			num = Mathf.Clamp01((Time.time - _startTime.Value) / fadeTime);
			if (num == 1f)
			{
				_startTime = null;
			}
		}
		_SetLerp(num);
	}

	private void LateUpdate()
	{
		_onUpdate?.Invoke(this);
		float num = MathUtil.CalculateEaseStiffnessSubjectToTime(colorEaseSpeed, Time.deltaTime);
		_color = _color.Lerp(_targetColor, num).DeltaSnap(_targetColor);
		if (_targetStartRotation.HasValue)
		{
			_startRotation = Quaternion.Slerp(_startRotation, _targetStartRotation.Value, num);
		}
		if (_targetEndRotation.HasValue)
		{
			_endRotation = Quaternion.Slerp(_endRotation, _targetEndRotation.Value, num);
		}
		if (_targetEndOffset.HasValue)
		{
			_endOffset = Vector3.Lerp(_endOffset, _targetEndOffset.Value, num);
		}
		MathUtil.Ease(ref _tangentScale, _targetTangentScale, num);
		MathUtil.Ease(ref _endTangentScale, _targetEndTangentScale, num);
		MathUtil.Ease(ref _endCapScale, _targetEndCapScale, num);
		base.transform.CopyFrom(_start);
		base.transform.rotation *= _startRotation;
		vfx.SetVector3(TARGET_POSITION_ID, _end.position + _end.TransformVector(_endOffset));
		vfx.SetVector3(TARGET_ANGLES_ID, (_end.rotation * _endRotation).eulerAngles);
		vfx.SetVector3(TARGET_SCALE_ID, _end.lossyScale * _endTangentScale);
		vfx.SetFloat(TANGENT_SCALE_ID, _tangentScale);
		vfx.SetFloat(END_CAP_SCALE_ID, _endCapScale * EndCapSCale);
		_visibility = Mathf.Clamp01(_visibility + (Cursor.visible ? Time.deltaTime : (0f - Time.deltaTime)) / fadeTime);
	}

	public void Finish()
	{
		float valueOrDefault = _finishTime.GetValueOrDefault();
		if (!_finishTime.HasValue)
		{
			valueOrDefault = Time.time - fadeTime * (1f - _lerp);
			_finishTime = valueOrDefault;
		}
	}
}
