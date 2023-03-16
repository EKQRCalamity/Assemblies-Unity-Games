using System.Collections;
using UnityEngine;

public class DLCEndingCutscene : DLCGenericCutscene
{
	[SerializeField]
	private TrappedChar trappedChar;

	[SerializeField]
	private GameObject leftCuphead;

	[SerializeField]
	private GameObject leftMugman;

	[SerializeField]
	private GameObject rightMugman;

	[SerializeField]
	private GameObject rightChalice;

	[SerializeField]
	private GameObject rightCuphead;

	[SerializeField]
	private GameObject trappedChalice;

	[SerializeField]
	private GameObject trappedMugman;

	[SerializeField]
	private GameObject trappedCuphead;

	[SerializeField]
	private GameObject ghostBodyChalice;

	[SerializeField]
	private GameObject ghostBodyCHMM;

	[SerializeField]
	private Animator shot1Animator;

	[SerializeField]
	private GameObject[] altText;

	private float screenShakeAmt;

	private bool advanceMusic;

	[SerializeField]
	[Range(-1f, 3f)]
	private int fastForward = -1;

	protected override void Start()
	{
		base.Start();
		SceneLoader.OnLoaderCompleteEvent += StartMusic;
		if (trappedChar == TrappedChar.None)
		{
			trappedChar = DetectCharacter();
		}
		shot1Animator.SetBool("NeedToSwap", trappedChar != TrappedChar.Chalice);
		rightCuphead.SetActive(value: false);
		ghostBodyChalice.SetActive(trappedChar == TrappedChar.Chalice);
		ghostBodyCHMM.SetActive(trappedChar != TrappedChar.Chalice);
		switch (trappedChar)
		{
		case TrappedChar.Chalice:
			leftMugman.SetActive(value: false);
			rightChalice.SetActive(value: false);
			trappedMugman.SetActive(value: false);
			trappedCuphead.SetActive(value: false);
			break;
		case TrappedChar.Mugman:
			leftMugman.SetActive(value: false);
			rightMugman.SetActive(value: false);
			trappedChalice.SetActive(value: false);
			trappedCuphead.SetActive(value: false);
			text[0] = altText[0];
			break;
		case TrappedChar.Cuphead:
			leftCuphead.SetActive(value: false);
			rightMugman.SetActive(value: false);
			rightCuphead.SetActive(value: false);
			trappedChalice.SetActive(value: false);
			trappedMugman.SetActive(value: false);
			text[0] = altText[1];
			break;
		}
	}

	private void StartMusic()
	{
		StartCoroutine(handle_music_cr());
	}

	private IEnumerator handle_music_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		AudioManager.StartBGMAlternate(0);
		yield return StartCoroutine(hold_for_music_advance_and_loop(5.5f, string.Empty));
		AudioManager.StartBGMAlternate(1);
		yield return StartCoroutine(hold_for_music_advance_and_loop(3f, string.Empty));
		AudioManager.StartBGMAlternate(2);
	}

	private IEnumerator hold_for_music_advance_and_loop(float time, string loopName)
	{
		float t = 0f;
		advanceMusic = false;
		while (t < time && !advanceMusic)
		{
			t += Time.deltaTime;
			yield return null;
		}
		if (!advanceMusic)
		{
			AudioManager.PlayLoop(loopName);
		}
		while (!advanceMusic)
		{
			yield return null;
		}
		AudioManager.Stop(loopName);
	}

	private IEnumerator crossfade_final_music_cr()
	{
		AudioManager.FadeBGMVolume(0f, 1.5f, fadeOut: true);
		AudioManager.FadeSFXVolume("mus_dlc_ending_4", 0.0001f, 0.0001f);
		yield return null;
		AudioManager.Play("mus_dlc_ending_4");
		AudioManager.FadeSFXVolume("mus_dlc_ending_4", 0.4f, 1.5f);
	}

	public void AdvanceMusic()
	{
		advanceMusic = true;
	}

	public void SwapChars()
	{
		rightChalice.SetActive(value: false);
		trappedChalice.SetActive(value: true);
		ghostBodyChalice.SetActive(value: true);
		ghostBodyCHMM.SetActive(value: false);
		switch (trappedChar)
		{
		case TrappedChar.Mugman:
			rightMugman.SetActive(value: true);
			trappedMugman.SetActive(value: false);
			break;
		case TrappedChar.Cuphead:
			rightCuphead.SetActive(value: true);
			trappedCuphead.SetActive(value: false);
			break;
		}
	}

	public override void IrisOut()
	{
		SceneLoader.LoadScene(Scenes.scene_cutscene_dlc_credits_comic, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris, SceneLoader.Icon.None);
	}

	public void StartShake()
	{
		screenShakeAmt = 4f;
	}

	public void StopShake()
	{
		screenShakeAmt = 0f;
	}

	private void LateUpdate()
	{
		float x = Random.Range(0f - screenShakeAmt, screenShakeAmt);
		float y = Random.Range(0f - screenShakeAmt, screenShakeAmt);
		screens[curScreen].transform.localPosition = new Vector3(x, y, 0f);
	}

	protected override void OnScreenAdvance(int which)
	{
		if (which != 3)
		{
			return;
		}
		StartCoroutine(crossfade_final_music_cr());
		GameObject gameObject = GameObject.Find("Fader");
		if (gameObject != null)
		{
			Animator component = gameObject.GetComponent<Animator>();
			if (component != null)
			{
				component.Play("Transparent");
			}
		}
	}

	protected override void OnDestroy()
	{
		SceneLoader.OnLoaderCompleteEvent -= StartMusic;
		base.OnDestroy();
	}
}
