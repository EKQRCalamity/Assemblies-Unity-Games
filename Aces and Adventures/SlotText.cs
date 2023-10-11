using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SlotText : MonoBehaviour
{
	private static System.Random _random;

	[Header("TRANSFORMS=====================================================================================================")]
	public Transform forwardFaceTransform;

	public List<TextMeshProUGUI> childFaces;

	[Header("SOUND==========================================================================================================")]
	[SerializeField]
	protected SlotTextSoundPack _soundPack;

	[Header("ANIMATION======================================================================================================")]
	public bool useScaledTime;

	[Range(0.01f, 1f)]
	public float animationPerFaceTime = 0.1f;

	[Range(0.1f, 10f)]
	public float maxAnimationTime = 2f;

	[Range(0f, 32f)]
	public int maxTransitionDistance;

	public AnimationCurve animationLerpCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private char _letter;

	private Color32 _textTint;

	private List<char> _pendingLetters = new List<char>();

	private int _rotationDirection;

	private int _currentFaceIndex;

	private Quaternion _rotationAtBeginAnimation;

	private Quaternion _rotationAtEndAnimation;

	private float _animationPerFaceTime;

	private float _elapsedAnimationTime;

	private static System.Random _Random => _random ?? (_random = new System.Random());

	protected SlotTextSoundPack soundPack
	{
		get
		{
			if (!_soundPack)
			{
				return _soundPack = ScriptableObject.CreateInstance<SlotTextSoundPack>();
			}
			return _soundPack;
		}
	}

	protected int _nextFaceIndex => MathUtil.Wrap(_currentFaceIndex, _rotationDirection, 0, childFaces.Count);

	protected char _shownLetter
	{
		get
		{
			if (childFaces[_currentFaceIndex].text.Length <= 0)
			{
				return ' ';
			}
			return childFaces[_currentFaceIndex].text[0];
		}
	}

	protected bool _isAnimating => _rotationDirection != 0;

	protected Transform _currentFaceTransform => childFaces[_currentFaceIndex].transform;

	public char letter
	{
		get
		{
			return _letter;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _letter, value))
			{
				_OnDesiredLetterChange();
			}
		}
	}

	public Color32 textTint
	{
		get
		{
			return _textTint;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _textTint, value))
			{
				_OnTextTintChange();
			}
		}
	}

	private void _OnDesiredLetterChange()
	{
		bool isAnimating = _isAnimating;
		_pendingLetters.Clear();
		foreach (char transitionCharacter in SlotTextGenerator.GetTransitionCharacters(_shownLetter, _letter, ref _rotationDirection))
		{
			_pendingLetters.Add(transitionCharacter);
		}
		if (_pendingLetters.Count == 0)
		{
			return;
		}
		_pendingLetters.RemoveAt(0);
		if (maxTransitionDistance > 0)
		{
			_pendingLetters.RemoveFromCenter(_pendingLetters.Count - maxTransitionDistance);
		}
		if (_BeginAnimateToNextFace(playTickSound: false))
		{
			_animationPerFaceTime = Mathf.Min(animationPerFaceTime, maxAnimationTime / (float)_pendingLetters.Count);
			if (!isAnimating)
			{
				soundPack.onBeginAnimation.PlaySafe(base.transform, _Random, soundPack.mixerGroup);
			}
		}
	}

	private void _OnTextTintChange()
	{
		foreach (TextMeshProUGUI childFace in childFaces)
		{
			childFace.color = _textTint;
		}
	}

	private void _SetPendingLettersToUpcomingFaces()
	{
		int value = _currentFaceIndex;
		int num = Mathf.Min(_pendingLetters.Count, childFaces.Count - 1);
		for (int i = 0; i < num; i++)
		{
			childFaces[value].text = _pendingLetters[i].ToString();
			MathUtil.Wrap(ref value, _rotationDirection, 0, childFaces.Count);
		}
	}

	private bool _BeginAnimateToNextFace(bool playTickSound, float elapsedTimeOverage = 0f)
	{
		if (_pendingLetters.Count == 0)
		{
			return false;
		}
		_currentFaceIndex = _nextFaceIndex;
		_SetPendingLettersToUpcomingFaces();
		_pendingLetters.RemoveAt(0);
		_elapsedAnimationTime = elapsedTimeOverage;
		_rotationAtBeginAnimation = base.transform.localRotation;
		_rotationAtEndAnimation = base.transform.localRotation * Quaternion.FromToRotation(base.transform.InverseTransformDirection(-_currentFaceTransform.forward), base.transform.InverseTransformDirection(forwardFaceTransform.forward));
		if (playTickSound)
		{
			soundPack.onTick.PlaySafe(base.transform, _Random, soundPack.mixerGroup);
		}
		return true;
	}

	private void Start()
	{
		forwardFaceTransform.SetParent(base.transform.parent, worldPositionStays: true);
	}

	private void Update()
	{
		if (_rotationDirection == 0)
		{
			return;
		}
		_elapsedAnimationTime += GameUtil.GetDeltaTime(useScaledTime);
		float num = Mathf.Clamp01(_elapsedAnimationTime / _animationPerFaceTime);
		base.transform.localRotation = Quaternion.LerpUnclamped(_rotationAtBeginAnimation, _rotationAtEndAnimation, animationLerpCurve.Evaluate(num));
		if (!(num < 1f))
		{
			_elapsedAnimationTime -= _animationPerFaceTime;
			if (!_BeginAnimateToNextFace(playTickSound: true, _elapsedAnimationTime))
			{
				soundPack.onEndAnimation.PlaySafe(base.transform, _Random, soundPack.mixerGroup);
				_rotationDirection = 0;
			}
		}
	}
}
