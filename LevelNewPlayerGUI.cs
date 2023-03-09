using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelNewPlayerGUI : AbstractMonoBehaviour
{
	[SerializeField]
	private Image background;

	[SerializeField]
	private Image card;

	[SerializeField]
	private Sprite cupheadCard;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private LocalizationHelper localizationHelper;

	[SerializeField]
	private Color cupheadColor;

	[SerializeField]
	private Color mugmanColor;

	private CanvasGroup canvasGroup;

	public static LevelNewPlayerGUI Current { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		if (PlayerManager.player1IsMugman)
		{
			card.sprite = cupheadCard;
		}
		Current = this;
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		if (Current == this)
		{
			Current = null;
		}
	}

	public void Init()
	{
		base.gameObject.SetActive(value: true);
		if (OnlineManager.Instance.Interface.SupportsMultipleUsers && OnlineManager.Instance.Interface.GetUser(PlayerId.PlayerTwo) != null)
		{
			localizationHelper.ApplyTranslation(Localization.Find("PlayerTwoJoinedWithUser"), new LocalizationHelper.LocalizationSubtext[1]
			{
				new LocalizationHelper.LocalizationSubtext("USERNAME", OnlineManager.Instance.Interface.GetUser(PlayerId.PlayerTwo).Name, dontTranslate: true)
			});
		}
		StartCoroutine(tweenIn_cr());
		StartCoroutine(text_cr());
	}

	protected IEnumerator tweenIn_cr()
	{
		base.animator.Play("In");
		float t = 0f;
		AudioManager.Play("player_joined");
		PauseManager.Pause();
		while (t < 0.2f)
		{
			float val = t / 0.2f;
			canvasGroup.alpha = Mathf.Lerp(0f, 1f, val);
			t += Time.deltaTime;
			yield return null;
		}
		canvasGroup.alpha = 1f;
		yield return new WaitForSeconds(2f);
		base.animator.Play("Out");
		StartCoroutine(tweenOut_cr());
	}

	protected IEnumerator tweenOut_cr()
	{
		float t = 0f;
		while (t < 0.2f)
		{
			float val = t / 0.2f;
			canvasGroup.alpha = Mathf.Lerp(1f, 0f, val);
			t += Time.deltaTime;
			yield return null;
		}
		canvasGroup.alpha = 0f;
		while (InterruptingPrompt.IsInterrupting())
		{
			yield return null;
		}
		PauseManager.Unpause();
		base.gameObject.SetActive(value: false);
	}

	private IEnumerator text_cr()
	{
		while (true)
		{
			text.color = Color.white;
			yield return new WaitForSeconds(1f / 24f);
			text.color = ((!PlayerManager.player1IsMugman) ? mugmanColor : cupheadColor);
			yield return new WaitForSeconds(1f / 24f);
		}
	}
}
