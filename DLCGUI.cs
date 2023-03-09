using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DLCGUI : AbstractMonoBehaviour
{
	private static readonly float FaderAlpha = 0.5f;

	[SerializeField]
	private GameObject notInstalled;

	[SerializeField]
	private GameObject installed;

	[SerializeField]
	private Transform notInstalledScaler;

	[SerializeField]
	private Transform installedScaler;

	[SerializeField]
	private Image fader;

	[SerializeField]
	private Text notInstalledText;

	[SerializeField]
	private Text installedText;

	private CanvasGroup canvasGroup;

	private CupheadInput.AnyPlayerInput input;

	private float timeSinceStart;

	private float timeSinceConfirmPressed;

	private bool dlcEnabled;

	public bool dlcMenuOpen { get; private set; }

	public bool inputEnabled { get; private set; }

	public bool justClosed { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		dlcMenuOpen = false;
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
	}

	public void Init(bool checkIfDead)
	{
		input = new CupheadInput.AnyPlayerInput(checkIfDead);
	}

	private void Update()
	{
		justClosed = false;
		timeSinceStart += Time.deltaTime;
		timeSinceConfirmPressed += Time.deltaTime;
		if (!(timeSinceStart < 0.25f) && inputEnabled)
		{
			if (GetButtonDown(CupheadButton.Cancel))
			{
				StartCoroutine(hide_cr());
			}
			else if (!dlcEnabled && timeSinceConfirmPressed >= 0.5f && DLCManager.CanRedirectToStore() && GetButtonDown(CupheadButton.Accept))
			{
				timeSinceConfirmPressed = 0f;
				DLCManager.LaunchStore();
			}
		}
	}

	public void ShowDLCMenu()
	{
		dlcEnabled = DLCManager.DLCEnabled();
		timeSinceStart = 0f;
		timeSinceConfirmPressed = 0f;
		dlcMenuOpen = true;
		canvasGroup.alpha = 1f;
		StartCoroutine(show_cr());
	}

	private void hideDLCMenu()
	{
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		inputEnabled = false;
		dlcMenuOpen = false;
		justClosed = true;
	}

	private void interactable()
	{
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		inputEnabled = true;
	}

	private IEnumerator show_cr()
	{
		Transform scaler;
		Text text;
		if (dlcEnabled)
		{
			scaler = installedScaler;
			notInstalled.SetActive(value: false);
			installed.SetActive(value: true);
			text = installedText;
		}
		else
		{
			scaler = notInstalledScaler;
			notInstalled.SetActive(value: true);
			installed.SetActive(value: false);
			text = notInstalledText;
		}
		fader.color = new Color(0f, 0f, 0f, FaderAlpha);
		Image[] fadeImages = scaler.GetComponentsInChildren<Image>();
		Image[] array = fadeImages;
		foreach (Image image in array)
		{
			Color color2 = image.color;
			color2.a = 1f;
			image.color = color2;
		}
		float elapsedTime = 0f;
		while (elapsedTime < 0.4f)
		{
			elapsedTime += (float)CupheadTime.Delta;
			Vector3 scale = scaler.localScale;
			scale.x = (scale.y = EaseUtils.EaseOutCubic(2f, 1f, elapsedTime / 0.4f));
			scaler.localScale = scale;
			Color color = text.color;
			color.a = Mathf.Lerp(0f, 1f, elapsedTime / 0.4f);
			text.color = color;
			yield return null;
		}
		interactable();
	}

	private IEnumerator hide_cr()
	{
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		inputEnabled = false;
		Transform scaler;
		Text text;
		if (dlcEnabled)
		{
			scaler = installedScaler;
			notInstalled.SetActive(value: false);
			installed.SetActive(value: true);
			text = installedText;
		}
		else
		{
			scaler = notInstalledScaler;
			notInstalled.SetActive(value: true);
			installed.SetActive(value: false);
			text = notInstalledText;
		}
		Image[] fadeImages = scaler.GetComponentsInChildren<Image>();
		float elapsedTime = 0f;
		while (elapsedTime < 0.2f)
		{
			elapsedTime += (float)CupheadTime.Delta;
			Vector3 scale = scaler.localScale;
			scale.x = (scale.y = EaseUtils.EaseInCubic(1f, 2f, elapsedTime / 0.2f));
			scaler.localScale = scale;
			Color color3 = text.color;
			color3.a = Mathf.Lerp(1f, 0f, elapsedTime / 0.2f);
			text.color = color3;
			Image[] array = fadeImages;
			foreach (Image image in array)
			{
				color3 = image.color;
				color3.a = Mathf.Lerp(1f, 0f, elapsedTime / 0.2f);
				image.color = color3;
			}
			color3 = fader.color;
			color3.a = Mathf.Lerp(FaderAlpha, 0f, elapsedTime / 0.2f);
			fader.color = color3;
			yield return null;
		}
		hideDLCMenu();
	}

	protected bool GetButtonDown(CupheadButton button)
	{
		if (input.GetButtonDown(button))
		{
			AudioManager.Play("level_menu_select");
			return true;
		}
		return false;
	}
}
