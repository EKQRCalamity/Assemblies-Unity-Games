using System.Collections;
using UnityEngine;

public class DLCCreditsComicCutscene : Cutscene
{
	private static readonly float AdjustmentAmount = -1f;

	private static readonly float EndingAdjustment = 15f;

	private static readonly float ScrollDuration = 90.8f;

	[SerializeField]
	private Transform parentTransform;

	[SerializeField]
	private SpriteRenderer[] panels;

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
		canSkip = true;
		AudioManager.PlayBGM();
		float distance = panels.GetLast().transform.position.x - panels[0].transform.position.x - EndingAdjustment;
		float elapsedTime = 0f;
		while (elapsedTime < ScrollDuration)
		{
			yield return null;
			elapsedTime += (float)CupheadTime.Delta * multiplier;
			Vector3 position = parentTransform.position;
			position.x = Mathf.Lerp(0f, 0f - distance, elapsedTime / ScrollDuration);
			parentTransform.position = position;
		}
		yield return CupheadTime.WaitForSeconds(this, 5f);
		canSkip = false;
		goToNext();
	}

	private void goToNext()
	{
		SceneLoader.LoadScene(Scenes.scene_cutscene_dlc_credits, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade, SceneLoader.Icon.None);
	}
}
