using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

[RequireComponent(typeof(TextMeshProUGUI), typeof(LocalizeStringEvent))]
public class AnimatedMessage : MonoBehaviour
{
	private static LocalizedString NULL_MESSAGE = new LocalizedString();

	[Range(0f, 30f)]
	public float duration = 6f;

	public GameObject animateOutBlueprint;

	private TextMeshProUGUI _text;

	private LocalizeStringEvent _localizeStringEvent;

	private float _durationRemaining;

	private TextMeshProUGUI text => this.CacheComponent(ref _text);

	private LocalizeStringEvent localizeStringEvent => this.CacheComponent(ref _localizeStringEvent);

	public LocalizedString activeMessage
	{
		get
		{
			if (_durationRemaining == float.MinValue)
			{
				return null;
			}
			return localizeStringEvent.StringReference;
		}
	}

	private void OnEnable()
	{
		_durationRemaining = duration;
		foreach (ATextMeshProAnimator item in base.gameObject.GetComponentsInChildrenPooled<ATextMeshProAnimator>(includeInactive: true))
		{
			item.Play();
		}
	}

	private void Update()
	{
		if (!(duration <= 0f) && !(_durationRemaining < 0f) && (_durationRemaining -= Time.deltaTime) <= 0f)
		{
			Finish();
		}
	}

	private void OnDisable()
	{
		localizeStringEvent.StringReference = NULL_MESSAGE;
	}

	public void Finish()
	{
		if (_durationRemaining != float.MinValue)
		{
			_durationRemaining = float.MinValue;
			ATextMeshProAnimator.Create(animateOutBlueprint, text);
		}
	}

	public void SetMessage(string message)
	{
		localizeStringEvent.StringReference = NULL_MESSAGE;
		text.text = message;
	}

	public void SetMessage(LocalizedString message)
	{
		localizeStringEvent.StringReference = message;
	}
}
