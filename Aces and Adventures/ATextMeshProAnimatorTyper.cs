using System;
using TMPro;
using UnityEngine;

public abstract class ATextMeshProAnimatorTyper : ATextMeshProAnimator
{
	[Range(0.01f, 1f)]
	public float fadeInTime = 0.075f;

	private int _maxVisibleCharacters;

	private PoolKeepItemListHandle<float> _characterRevealTimes;

	private bool _animating;

	private bool _finished;

	public int maxVisibleCharacters
	{
		get
		{
			return _maxVisibleCharacters;
		}
		set
		{
			value = Math.Max(0, Math.Min(value, base._visibleCharacterCount));
			if (value != _maxVisibleCharacters)
			{
				_OnMaxVisibleCharactersChange(value);
			}
		}
	}

	private PoolKeepItemListHandle<float> characterRevealTimes => _characterRevealTimes ?? (_characterRevealTimes = Pools.UseKeepItemList<float>().FillToCount(base._visibleCharacterCount, float.MaxValue));

	protected override bool _hideDuration => true;

	protected override bool _hideFadeInEaseSpeed => true;

	protected override bool _hideFadeOutEaseSpeed => true;

	private void _OnMaxVisibleCharactersChange(int newValue)
	{
		if (newValue > _maxVisibleCharacters)
		{
			for (int i = _maxVisibleCharacters; i < newValue; i++)
			{
				characterRevealTimes[i] = Time.time;
			}
		}
		else
		{
			for (int num = newValue - 1; num >= _maxVisibleCharacters; num--)
			{
				characterRevealTimes[num] = float.MaxValue;
			}
		}
		_maxVisibleCharacters = newValue;
	}

	private void _Clear()
	{
		int num = _maxVisibleCharacters;
		_maxVisibleCharacters = 0;
		Pools.Repool(ref _characterRevealTimes);
		maxVisibleCharacters = num;
	}

	protected override void _OnIndexRangeCalculated()
	{
		_Clear();
	}

	protected override void _OnPreRenderText(TMP_TextInfo textInfo)
	{
		_animating = false;
		base._OnPreRenderText(textInfo);
	}

	protected sealed override void _AnimateVertex(ref Vector3 vertexPosition, ref Color32 vertexColor, ref Rect bounds, int animatedCharacterIndex)
	{
		float num = Mathf.Clamp01((Time.time - characterRevealTimes[base._visibleCharacterIndex]) / fadeInTime);
		_AnimateCharacter(num, ref vertexPosition, ref vertexColor, ref bounds, animatedCharacterIndex);
		_animating = _animating || num < 1f;
	}

	protected override void Update()
	{
		base.text.havePropertiesChanged = true;
		if (!_animating && base.elapsedTime > 0f && (_finished = true))
		{
			_OnFinish();
		}
	}

	public override void Play()
	{
		base.Play();
		_animating = true;
	}

	protected override void _OnTextChange()
	{
		base._OnTextChange();
		_Clear();
	}

	protected override void OnDisable()
	{
		if (!_finished && (bool)_characterRevealTimes && (bool)base.text)
		{
			for (int i = 0; i < _characterRevealTimes.Count; i++)
			{
				_characterRevealTimes[i] = 0f;
			}
			base.text.ForceMeshUpdate();
		}
		base.OnDisable();
		Pools.Repool(ref _characterRevealTimes);
	}

	protected abstract void _AnimateCharacter(float t, ref Vector3 vertexPosition, ref Color32 vertexColor, ref Rect bounds, int animatedCharacterIndex);
}
