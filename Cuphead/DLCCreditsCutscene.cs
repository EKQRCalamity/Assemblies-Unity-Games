using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DLCCreditsCutscene : Cutscene
{
	[SerializeField]
	private float scrollDuration;

	[SerializeField]
	private RectTransform contentTransform;

	[SerializeField]
	private float memphisFontSize;

	[SerializeField]
	private float vogueBoldFontSize;

	[SerializeField]
	private float vogueExtraBoldFontSize;

	private CupheadInput.AnyPlayerInput input;

	private bool canSkip;

	private float multiplier = 1f;

	protected override void Start()
	{
		base.Start();
		CutsceneGUI.Current.pause.pauseAllowed = false;
		input = new CupheadInput.AnyPlayerInput();
		StartCoroutine(credits_cr());
	}

	private void Update()
	{
		if (canSkip)
		{
			if (input.GetButtonDown(CupheadButton.Pause))
			{
				canSkip = false;
				StopAllCoroutines();
				goToNext();
			}
			else if (input.GetAnyButtonHeld() && !input.GetButtonDown(CupheadButton.Pause))
			{
				if (multiplier == 1f)
				{
					multiplier = 8f;
					AudioManager.ChangeBGMPitch(8f, 0.125f);
				}
			}
			else if (multiplier > 1f)
			{
				multiplier = 1f;
				AudioManager.ChangeBGMPitch(1f, 0.125f);
			}
		}
		else if (multiplier > 1f)
		{
			multiplier = 1f;
			AudioManager.ChangeBGMPitch(1f, 0.125f);
		}
	}

	private IEnumerator credits_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 3f);
		AudioManager.PlayBGM();
		canSkip = true;
		float preferredHeight = contentTransform.GetComponent<VerticalLayoutGroup>().preferredHeight;
		float speed = preferredHeight / scrollDuration;
		float elapsedTime = 0f;
		float accumulator = 0f;
		while (elapsedTime < scrollDuration)
		{
			yield return null;
			for (accumulator += (float)CupheadTime.Delta * multiplier; accumulator > 1f / 24f; accumulator -= 1f / 24f)
			{
				elapsedTime += 1f / 24f;
			}
			Vector2 position = contentTransform.anchoredPosition;
			position.y = Mathf.Lerp(0f, preferredHeight - 720f, elapsedTime / scrollDuration);
			contentTransform.anchoredPosition = position;
		}
		canSkip = false;
		yield return CupheadTime.WaitForSeconds(this, 3f);
		goToNext();
	}

	private void goToNext()
	{
		PlayerManager.ResetPlayers();
		SceneLoader.LoadScene(Scenes.scene_title, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade, SceneLoader.Icon.None);
	}
}
