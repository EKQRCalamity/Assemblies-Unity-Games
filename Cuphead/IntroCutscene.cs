using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroCutscene : Cutscene
{
	private CupheadInput.AnyPlayerInput input;

	[SerializeField]
	private Image arrow;

	[SerializeField]
	private GameObject book;

	[SerializeField]
	private GameObject bookLocalized;

	private float arrowTransparency;

	private bool arrowVisible;

	protected override void Start()
	{
		base.Start();
		book.SetActive(Localization.language == Localization.Languages.English);
		bookLocalized.SetActive(Localization.language != Localization.Languages.English);
		input = new CupheadInput.AnyPlayerInput();
		CutsceneGUI.Current.pause.pauseAllowed = false;
		StartCoroutine(main_cr());
		StartCoroutine(skip_cr());
	}

	private IEnumerator main_cr()
	{
		int numScreens = 11;
		yield return CupheadTime.WaitForSeconds(this, 6f);
		for (int i = 0; i < numScreens; i++)
		{
			yield return CupheadTime.WaitForSeconds(this, 1.75f);
			arrowVisible = true;
			while (input.GetButtonDown(CupheadButton.Pause) || !input.GetAnyButtonDown())
			{
				yield return null;
			}
			arrowVisible = false;
			base.animator.SetTrigger("Continue");
			if (i != numScreens - 1)
			{
				NextPageSFX();
			}
			if (i == 2)
			{
				DevilLaugh();
			}
			if (i == 4)
			{
				DiceRoll();
			}
			if (i == 5)
			{
				DevilSlam();
			}
			if (i == 8)
			{
				DevilKick();
			}
		}
		AudioManager.FadeBGMVolume(0f, 0.75f, fadeOut: true);
		yield return CupheadTime.WaitForSeconds(this, 0.75f);
		SceneLoader.LoadLevel(Levels.House, SceneLoader.Transition.Fade);
	}

	private IEnumerator skip_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		while (true)
		{
			if (input.GetButtonDown(CupheadButton.Pause))
			{
				SceneLoader.LoadLevel(Levels.House, SceneLoader.Transition.Fade);
			}
			yield return null;
		}
	}

	private void Update()
	{
		if (arrowVisible)
		{
			arrowTransparency = Mathf.Clamp01(arrowTransparency + Time.deltaTime / 0.25f);
		}
		else
		{
			arrowTransparency = 0f;
		}
		arrow.color = new Color(1f, 1f, 1f, arrowTransparency * 0.35f);
	}

	private void NextPageSFX()
	{
		AudioManager.Play("ui_confirm");
		AudioManager.Play("ui_pageturn");
	}

	private void DevilLaugh()
	{
		AudioManager.Play("devil_laugh");
	}

	private void DiceRoll()
	{
		AudioManager.Play("dice_roll");
	}

	private void DevilSlam()
	{
		AudioManager.Play("devil_slam");
	}

	private void DevilKick()
	{
		AudioManager.Play("devil_kick");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		arrow = null;
	}
}
