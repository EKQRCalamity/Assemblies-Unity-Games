using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointerState3DColorTransition : MonoBehaviour
{
	[SerializeField]
	protected PointerState3DTracker _stateTracker;

	[Header("Time")]
	public bool useScaledTime;

	[Range(0f, 2f)]
	public float transitionTime = 0.1f;

	[Header("Color")]
	public Color normalColor = Color.white;

	public Color overColor = new Color(0.75f, 0.75f, 0.75f, 1f);

	public Color pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	[Range(0.5f, 2f)]
	public float colorMultiplier = 1f;

	[Header("Graphics")]
	[SerializeField]
	private List<Graphic> _graphics;

	[Header("Events")]
	[SerializeField]
	private ColorEvent _OnColorChange;

	public PointerState3DTracker stateTracker
	{
		get
		{
			return this.CacheComponentInParent(ref _stateTracker);
		}
		set
		{
			_SetTracker(value);
		}
	}

	public List<Graphic> graphics
	{
		get
		{
			List<Graphic> list = _graphics;
			if (list == null)
			{
				List<Graphic> obj = new List<Graphic> { GetComponentInChildren<Graphic>() };
				List<Graphic> list2 = obj;
				_graphics = obj;
				list = list2;
			}
			return list;
		}
	}

	public ColorEvent OnColorChange => _OnColorChange ?? (_OnColorChange = new ColorEvent());

	private void Awake()
	{
		if ((bool)stateTracker)
		{
			stateTracker.OnStateChange.AddListener(_OnStateChange);
		}
	}

	private void OnEnable()
	{
		if (_graphics.IsNullOrEmpty())
		{
			_graphics = new List<Graphic> { GetComponentInChildren<Graphic>() };
		}
		_SetColor(normalColor, 0f);
	}

	private void _SetTracker(PointerState3DTracker tracker)
	{
		if (!(_stateTracker == tracker))
		{
			if ((bool)_stateTracker)
			{
				_stateTracker.OnStateChange.RemoveListener(_OnStateChange);
			}
			_stateTracker = tracker;
			if ((bool)_stateTracker)
			{
				_stateTracker.OnStateChange.AddListener(_OnStateChange);
			}
		}
	}

	private void _OnStateChange(PointerState state)
	{
		switch (state)
		{
		case PointerState.None:
			_SetColor(normalColor);
			break;
		case PointerState.Over:
			_SetColor(overColor);
			break;
		case PointerState.Pressed:
			_SetColor(pressedColor);
			break;
		default:
			throw new ArgumentOutOfRangeException("state", state, null);
		}
	}

	private void _SetColor(Color color, float? transitionTimeOverride = null)
	{
		color *= colorMultiplier;
		foreach (Graphic graphic in graphics)
		{
			graphic.CrossFadeColor(color, transitionTimeOverride ?? transitionTime, !useScaledTime, useAlpha: true);
		}
		OnColorChange.Invoke(color);
	}

	public void SetNormalColor(Color color)
	{
		if (SetPropertyUtility.SetStruct(ref normalColor, color))
		{
			_SetColor(normalColor, 0f);
		}
	}
}
