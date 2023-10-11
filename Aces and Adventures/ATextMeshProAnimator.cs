using System;
using System.Linq;
using TMPro;
using UnityEngine;

public abstract class ATextMeshProAnimator : MonoBehaviour
{
	public enum Direction
	{
		Horizontal,
		Vertical
	}

	public enum FinishAction
	{
		Disable,
		DisableGameObject,
		DisableTextGameObject
	}

	private TextBuilder _Builder;

	private static readonly ResourceBlueprint<GameObject> _HighlightBlueprint = "UI/TextAnimators/Highlight";

	private static readonly ResourceBlueprint<GameObject> _ColorBlueprint = "UI/TextAnimators/Color";

	[Range(1f, 20f, order = 1)]
	[HideInInspectorIf("_hideFadeInEaseSpeed", false)]
	public float fadeInEaseSpeed = 3f;

	[Range(-1f, 30f, order = 1)]
	[HideInInspectorIf("_hideDuration", false)]
	public float duration = 3f;

	[Range(1f, 20f, order = 1)]
	[HideInInspectorIf("_hideFadeOutEaseSpeed", false)]
	public float fadeOutEaseSpeed = 3f;

	[SerializeField]
	protected string _textToAnimate;

	public StringComparison textToAnimateComparison = StringComparison.OrdinalIgnoreCase;

	public FinishAction finishAction;

	private float _fadeAmount;

	private float _timeOfLastPlayRequest = float.MinValue;

	private TextMeshProUGUI _text;

	private int? _startIndex;

	private int _endIndex;

	private Rect _bounds;

	private string _previousText;

	private TextBuilder Builder => _Builder ?? (_Builder = new TextBuilder(clearOnToString: true));

	protected int _visibleCharacterIndex { get; private set; }

	public string textToAnimate
	{
		get
		{
			return _textToAnimate;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _textToAnimate, value))
			{
				_OnTextChange();
			}
		}
	}

	protected float timeOfLastPlayRequest
	{
		get
		{
			return _timeOfLastPlayRequest;
		}
		private set
		{
			base.enabled |= (_timeOfLastPlayRequest = value) >= Time.time;
		}
	}

	protected float elapsedTime => Time.time - timeOfLastPlayRequest;

	protected TextMeshProUGUI text => this.CacheComponentInParent(ref _text);

	protected float fadeAmount => _fadeAmount;

	protected int _visibleCharacterCount => _endIndex - _startIndex.GetValueOrDefault() + 1;

	protected virtual bool _hideFadeInEaseSpeed => false;

	protected virtual bool _hideDuration => false;

	protected virtual bool _hideFadeOutEaseSpeed => false;

	protected virtual bool _setDirtyOnUpdate => true;

	public static GameObject CreateHighlight(TextMeshProUGUI text, string textToAnimate, float duration, bool preventRedundantAnimations = true)
	{
		if (preventRedundantAnimations)
		{
			GameObject gameObject = _IsRedundantAnimation(text, _HighlightBlueprint, textToAnimate, duration);
			if ((object)gameObject != null)
			{
				return gameObject;
			}
		}
		return _SetData(Pools.Unpool(_HighlightBlueprint, text.transform), textToAnimate, duration);
	}

	public static void EndHighlights(GameObject gameObject)
	{
		if (!gameObject)
		{
			return;
		}
		foreach (ATextMeshProAnimator item in gameObject.GetComponentsInChildrenPooled<ATextMeshProAnimator>())
		{
			item.Stop();
		}
	}

	public static void EndHighlights(GameObject gameObject, string textToStopAnimating)
	{
		if (!gameObject)
		{
			return;
		}
		foreach (ATextMeshProAnimator item in gameObject.GetComponentsInChildrenPooled<ATextMeshProAnimator>())
		{
			if (item.textToAnimate == textToStopAnimating)
			{
				item.Stop();
			}
		}
	}

	public static GameObject CreateColor(TextMeshProUGUI text, string textToAnimate, float duration, Color32 color, bool preventRedundantAnimations = true, bool useAlphaAsBlend = false)
	{
		GameObject obj;
		if (preventRedundantAnimations)
		{
			GameObject gameObject = _IsRedundantAnimation(text, _ColorBlueprint, textToAnimate, duration);
			if ((object)gameObject != null)
			{
				obj = gameObject;
				goto IL_0041;
			}
		}
		obj = _SetData(Pools.Unpool(_ColorBlueprint, text.transform), textToAnimate, duration);
		goto IL_0041;
		IL_0041:
		TextMeshProAnimatorColor component = obj.GetComponent<TextMeshProAnimatorColor>();
		component.color = color;
		component.useAlphaAsBlend = useAlphaAsBlend;
		return obj;
	}

	public static GameObject Create(GameObject blueprint, TextMeshProUGUI text, string textToAnimate = "")
	{
		return _SetData(Pools.Unpool(blueprint, text.transform), textToAnimate, null);
	}

	private static GameObject _IsRedundantAnimation(TextMeshProUGUI text, GameObject blueprint, string textToAnimate, float duration)
	{
		foreach (ATextMeshProAnimator item in text.gameObject.GetComponentsInChildrenPooled<ATextMeshProAnimator>())
		{
			if (item.textToAnimate == textToAnimate && item.gameObject.name == blueprint.name)
			{
				return _SetData(item.gameObject, textToAnimate, duration);
			}
		}
		return null;
	}

	private static GameObject _SetData(GameObject go, string textToAnimate, float? duration)
	{
		foreach (ATextMeshProAnimator item in go.GetComponentsInChildrenPooled<ATextMeshProAnimator>())
		{
			item.SetData(duration, textToAnimate);
		}
		return go;
	}

	public static void StopAll(Transform transform)
	{
		foreach (ATextMeshProAnimator item in transform.gameObject.GetComponentsInChildrenPooled<ATextMeshProAnimator>())
		{
			item.Stop();
		}
	}

	protected virtual void _OnTextChange()
	{
		_startIndex = null;
	}

	private void _OnTextChange(UnityEngine.Object obj)
	{
		if (obj == text && SetPropertyUtility.SetObject(ref _previousText, text.text))
		{
			_OnTextChange();
		}
	}

	protected virtual void _OnIndexRangeCalculated()
	{
	}

	protected virtual void _OnPreRenderText(TMP_TextInfo textInfo)
	{
		if (!_startIndex.HasValue)
		{
			for (int i = 0; i < textInfo.characterCount; i++)
			{
				Builder.Append(textInfo.characterInfo[i].character);
			}
			string text = Builder.ToString();
			int valueOrDefault = _startIndex.GetValueOrDefault();
			if (!_startIndex.HasValue)
			{
				valueOrDefault = (textToAnimate.HasVisibleCharacter() ? text.IndexOf(textToAnimate, textToAnimateComparison) : 0);
				_startIndex = valueOrDefault;
			}
			if (_startIndex < 0)
			{
				return;
			}
			_endIndex = (textToAnimate.HasVisibleCharacter() ? (_startIndex.Value + textToAnimate.Length - 1) : (text.Length - 1));
			Vector2 lhs = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 lhs2 = new Vector2(float.MinValue, float.MinValue);
			for (int j = _startIndex.Value; j <= _endIndex; j++)
			{
				lhs = Vector2.Min(lhs, textInfo.characterInfo[j].bottomLeft.Project(AxisType.Z));
				lhs2 = Vector2.Max(lhs2, textInfo.characterInfo[j].topRight.Project(AxisType.Z));
			}
			_bounds = Rect.MinMaxRect(lhs.x, lhs.y, lhs2.x, lhs2.y);
			_OnIndexRangeCalculated();
		}
		if (_startIndex < 0)
		{
			return;
		}
		Rect bounds = _bounds;
		int animatedCharacterIndex = 0;
		for (int k = _startIndex.Value; k <= _endIndex; k++)
		{
			TMP_CharacterInfo tMP_CharacterInfo = textInfo.characterInfo[k];
			if (!char.IsWhiteSpace(tMP_CharacterInfo.character))
			{
				_visibleCharacterIndex = k - _startIndex.Value;
				TMP_MeshInfo tMP_MeshInfo = textInfo.meshInfo[tMP_CharacterInfo.materialReferenceIndex];
				int vertexIndex = tMP_CharacterInfo.vertexIndex;
				_AnimateVertex(ref tMP_MeshInfo.vertices[vertexIndex], ref tMP_MeshInfo.colors32[vertexIndex], ref bounds, animatedCharacterIndex);
				_AnimateVertex(ref tMP_MeshInfo.vertices[++vertexIndex], ref tMP_MeshInfo.colors32[vertexIndex], ref bounds, animatedCharacterIndex);
				_AnimateVertex(ref tMP_MeshInfo.vertices[++vertexIndex], ref tMP_MeshInfo.colors32[vertexIndex], ref bounds, animatedCharacterIndex);
				_AnimateVertex(ref tMP_MeshInfo.vertices[++vertexIndex], ref tMP_MeshInfo.colors32[vertexIndex], ref bounds, animatedCharacterIndex++);
			}
		}
	}

	private bool _AllAnimatorsFinished()
	{
		return !base.gameObject.GetComponentsInChildrenPooled<ATextMeshProAnimator>().AsEnumerable().Any((ATextMeshProAnimator textAnimator) => textAnimator.isActiveAndEnabled);
	}

	protected void _OnFinish()
	{
		base.enabled = false;
		switch (finishAction)
		{
		case FinishAction.DisableGameObject:
			if (_AllAnimatorsFinished())
			{
				base.gameObject.SetActive(value: false);
			}
			break;
		case FinishAction.DisableTextGameObject:
			if (_AllAnimatorsFinished())
			{
				text.gameObject.SetActive(value: false);
				base.gameObject.SetActive(value: false);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case FinishAction.Disable:
			break;
		}
	}

	protected float _GetSample(Rect bounds, Vector3 vertexPosition, WrapMethod wrap = WrapMethod.Mirror, Direction direction = Direction.Horizontal, float shift = 0f, float repeatsPerBounds = 1f)
	{
		return wrap.WrapShift(bounds.GetLerpAmounts(vertexPosition.Project(AxisType.Z))[(int)direction] * repeatsPerBounds + shift);
	}

	protected abstract void _AnimateVertex(ref Vector3 vertexPosition, ref Color32 vertexColor, ref Rect bounds, int animatedCharacterIndex);

	protected virtual void OnTransformParentChanged()
	{
		_text = null;
	}

	protected virtual void OnEnable()
	{
		TMPro_EventManager.TEXT_CHANGED_EVENT.Add(_OnTextChange);
		text.OnPreRenderText += _OnPreRenderText;
	}

	protected virtual void OnDisable()
	{
		TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(_OnTextChange);
		if ((bool)text)
		{
			text.OnPreRenderText -= _OnPreRenderText;
		}
		_previousText = (_textToAnimate = null);
		_startIndex = null;
	}

	protected virtual void Update()
	{
		bool flag = timeOfLastPlayRequest >= 0f && (duration < 0f || elapsedTime < duration);
		MathUtil.EaseSnap(ref _fadeAmount, flag ? 1 : 0, flag ? fadeInEaseSpeed : fadeOutEaseSpeed, Time.deltaTime);
		text.havePropertiesChanged = _setDirtyOnUpdate;
		if (!flag && _fadeAmount == 0f)
		{
			_OnFinish();
		}
	}

	public virtual void Play()
	{
		timeOfLastPlayRequest = Time.time;
	}

	public void Stop()
	{
		timeOfLastPlayRequest = float.MinValue;
	}

	public ATextMeshProAnimator SetData(float? durationToSet, string animatedText = "", bool play = true)
	{
		if (durationToSet.HasValue)
		{
			duration = durationToSet.Value;
		}
		textToAnimate = animatedText;
		Play();
		return this;
	}
}
