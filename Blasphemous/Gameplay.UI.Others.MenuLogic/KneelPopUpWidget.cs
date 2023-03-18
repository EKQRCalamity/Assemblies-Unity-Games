using System.Collections;
using System.Collections.Generic;
using Framework.Managers;
using I2.Loc;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Gameplay.UI.Others.MenuLogic;

public class KneelPopUpWidget : SerializedMonoBehaviour
{
	public enum Modes
	{
		PrieDieu_sword,
		PrieDieu_teleport,
		PrieDieu_all,
		Altar
	}

	[SerializeField]
	[BoxGroup("Base", true, false, 0)]
	private GameObject messageRoot;

	[SerializeField]
	[BoxGroup("Base", true, false, 0)]
	private TextMeshProUGUI text;

	[SerializeField]
	[BoxGroup("Config", true, false, 0)]
	private Dictionary<Modes, LocalizedString> Config = new Dictionary<Modes, LocalizedString>();

	private const string ANIMATOR_VARIABLE = "IsEnabled";

	private Animator animator;

	private bool waitingEnd;

	private Modes CurrentMode;

	public bool IsShowing { get; private set; }

	private void Awake()
	{
		IsShowing = false;
		waitingEnd = false;
		animator = GetComponent<Animator>();
		messageRoot.SetActive(value: false);
	}

	private void OnEnable()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
	}

	private void OnDisable()
	{
		I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		UpdateText();
	}

	private void UpdateText()
	{
		if (IsShowing)
		{
			string localizedText = Config[CurrentMode];
			text.text = Framework.Managers.LocalizationManager.ParseMeshPro(localizedText, Config[CurrentMode].mTerm);
		}
	}

	public void ShowPopUp(Modes mode)
	{
		CurrentMode = mode;
		messageRoot.SetActive(value: true);
		IsShowing = true;
		waitingEnd = false;
		animator.SetBool("IsEnabled", value: true);
		UpdateText();
	}

	public void HidePopUp()
	{
		StartCoroutine(SafeEnd());
	}

	private IEnumerator SafeEnd()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		animator.SetBool("IsEnabled", value: false);
		IsShowing = false;
		waitingEnd = false;
		yield return new WaitForSecondsRealtime(0.15f);
		messageRoot.SetActive(value: false);
	}
}
