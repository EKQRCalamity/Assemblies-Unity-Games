using System.Collections;
using FMOD.Studio;
using Framework.Managers;
using Sirenix.OdinInspector;
using Tools.DataContainer;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleWidget : MonoBehaviour
{
	[BoxGroup("Text", true, false, 0)]
	public Text dialogLine;

	[BoxGroup("Text", true, false, 0)]
	public CanvasGroup canvasText;

	[BoxGroup("Text", true, false, 0)]
	public GameObject textBackground;

	[BoxGroup("Video", true, false, 0)]
	public RawImage videoRaw;

	private bool inPlay;

	private EventInstance currentSound;

	private void Awake()
	{
		inPlay = false;
		videoRaw.enabled = false;
		Core.Cinematics.VideoStarted += VideoStarted;
		Core.Cinematics.VideoEnded += VideoEnded;
	}

	private void OnDestroy()
	{
		Core.Cinematics.VideoStarted -= VideoStarted;
		Core.Cinematics.VideoEnded -= VideoEnded;
	}

	public void EnableSubtitleBackground(bool enable)
	{
		textBackground.SetActive(enable);
	}

	private void VideoStarted(CutsceneData cutscene)
	{
		if (!inPlay && cutscene.subtitles.Count != 0)
		{
			inPlay = true;
			StartCoroutine(ShowSubtitles(cutscene));
			StartCoroutine(PerformRumble(cutscene));
		}
	}

	private void VideoEnded()
	{
		if (inPlay)
		{
			StopAllCoroutines();
			inPlay = false;
			canvasText.alpha = 0f;
			StopCurrentSound();
		}
	}

	private void StopCurrentSound()
	{
		if (currentSound.isValid())
		{
			currentSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			currentSound.release();
			currentSound.clearHandle();
			currentSound = default(EventInstance);
		}
	}

	private IEnumerator ShowSubtitles(CutsceneData cutscene)
	{
		float currentTime2 = 0f;
		int idx = 0;
		foreach (SubTitleBlock subtitle in cutscene.subtitles)
		{
			float timetowait = subtitle.from - currentTime2;
			yield return new WaitForSeconds(timetowait);
			if (!string.IsNullOrEmpty(cutscene.voiceOverPrefix))
			{
				string key = cutscene.voiceOverPrefix + "_" + idx;
				StopCurrentSound();
				currentSound = Core.Dialog.PlayProgrammerSound(key);
			}
			idx++;
			currentTime2 += timetowait;
			canvasText.alpha = 1f;
			dialogLine.text = subtitle.text;
			float length = subtitle.to - subtitle.from;
			yield return new WaitForSeconds(length);
			currentTime2 += length;
			canvasText.alpha = 0f;
			dialogLine.text = string.Empty;
		}
		yield return 0;
	}

	private IEnumerator PerformRumble(CutsceneData cutscene)
	{
		float currentTime = 0f;
		foreach (RummbleBlock rumble in cutscene.rumbles)
		{
			float timetowait = rumble.from - currentTime;
			if (timetowait < 0f)
			{
				timetowait = 0f;
			}
			yield return new WaitForSeconds(timetowait);
			Core.Input.ApplyRumble(rumble.rumble);
			currentTime += timetowait;
		}
		yield return 0;
	}
}
