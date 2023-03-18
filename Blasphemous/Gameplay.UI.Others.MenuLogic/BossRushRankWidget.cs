using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using Framework.BossRush;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.BossFight;
using Gameplay.UI.Widgets;
using I2.Loc;
using Rewired;
using Sirenix.OdinInspector;
using Tools.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class BossRushRankWidget : BasicUIBlockingWidget
{
	[Serializable]
	public struct ScoreLetter
	{
		public BossRushManager.BossRushCourseScore score;

		public GameObject letterGameObject;
	}

	[BoxGroup("Widgets Course", true, false, 0)]
	public Text CourseName;

	[BoxGroup("Widgets Course", true, false, 0)]
	public Text CourseCompletedSuffix;

	[BoxGroup("Widgets Course", true, false, 0)]
	public Text CourseFailedSuffix;

	[BoxGroup("Widgets Course", true, false, 0)]
	public Image CourseImage;

	[BoxGroup("Widgets Course", true, false, 0)]
	public Sprite Course_A_1;

	[BoxGroup("Widgets Course", true, false, 0)]
	public Sprite Course_A_2;

	[BoxGroup("Widgets Course", true, false, 0)]
	public Sprite Course_A_3;

	[BoxGroup("Widgets Course", true, false, 0)]
	public Sprite Course_B_1;

	[BoxGroup("Widgets Course", true, false, 0)]
	public Sprite Course_C_1;

	[BoxGroup("Widgets Course", true, false, 0)]
	public Sprite Course_D_1;

	[BoxGroup("Widgets Course", true, false, 0)]
	public GameObject ButtonRetry;

	[BoxGroup("Widgets Course", true, false, 0)]
	public Image FalseFade;

	[BoxGroup("Widgets Course", true, false, 0)]
	public GameObject NewRerecord;

	[BoxGroup("Widgets Course", true, false, 0)]
	public GameObject TextUnlocked;

	[BoxGroup("Widgets Dificult", true, false, 0)]
	public GameObject DificultNormal;

	[BoxGroup("Widgets Dificult", true, false, 0)]
	public GameObject DificultHard;

	[BoxGroup("Widgets Text", true, false, 0)]
	public GameObject FlaskBase;

	[BoxGroup("Widgets Text", true, false, 0)]
	public GameObject FlaskBase2;

	[BoxGroup("Widgets Text", true, false, 0)]
	public GameObject FlaskElement;

	[BoxGroup("Widgets Text", true, false, 0)]
	public Text DogesText;

	[BoxGroup("Widgets Text", true, false, 0)]
	public Text PrayerText;

	[BoxGroup("Widgets Text", true, false, 0)]
	public Text BloodText;

	[BoxGroup("Widgets Text", true, false, 0)]
	public Text HitsText;

	[BoxGroup("Widgets Grade", true, false, 0)]
	public Text TimeText;

	[BoxGroup("Widgets Grade", true, false, 0)]
	public Text PrevTimeText;

	[BoxGroup("Widgets Grade", true, false, 0)]
	public Image GradeImage;

	[BoxGroup("Grade Images", true, false, 0)]
	public List<ScoreLetter> ScoreLetters;

	[BoxGroup("Sounds", true, false, 0)]
	[EventRef]
	public string winMusic = string.Empty;

	[BoxGroup("Sounds", true, false, 0)]
	[EventRef]
	public string looseMusic = string.Empty;

	[BoxGroup("Sounds", true, false, 0)]
	[EventRef]
	public string rankLineSfx1 = string.Empty;

	[BoxGroup("Sounds", true, false, 0)]
	[EventRef]
	public string rankLineSfx2 = string.Empty;

	[BoxGroup("Sounds", true, false, 0)]
	[EventRef]
	public string lowGradeSfx = string.Empty;

	[BoxGroup("Sounds", true, false, 0)]
	[EventRef]
	public string highGradeSfx = string.Empty;

	private bool IsUnlockHard;

	private bool IsFailed;

	private bool IsSelected;

	private BossRushHighScore scoreShown;

	private bool showAnimationDone;

	public bool RetryPressed { get; private set; }

	public IEnumerator PlayBossRushRankAudio(bool complete)
	{
		BossFightManager BossFight = UnityEngine.Object.FindObjectOfType<BossFightManager>();
		if ((bool)BossFight)
		{
			BossFight.Audio.StopBossTrack();
		}
		yield return new WaitForSeconds(0.2f);
		string audio = ((!complete) ? looseMusic : winMusic);
		if (audio != string.Empty)
		{
			Core.Audio.Ambient.SetSceneParams(audio, string.Empty, new AudioParam[0], string.Empty);
		}
	}

	public void PlayRankLineSfx1()
	{
		Core.Audio.PlayOneShot(rankLineSfx1);
	}

	public void PlayRankLineSfx2()
	{
		Core.Audio.PlayOneShot(rankLineSfx2);
	}

	public void MarkShowAnimationAsDone()
	{
		showAnimationDone = true;
	}

	public void PlayGradeSfx()
	{
		if (scoreShown.Score > BossRushManager.BossRushCourseScore.A_PLUS)
		{
			Core.Audio.PlayOneShot(lowGradeSfx);
		}
		else
		{
			Core.Audio.PlayOneShot(highGradeSfx);
		}
	}

	public void ShowHighScore(BossRushHighScore score, bool pauseGame, bool complete, bool unlockHard)
	{
		scoreShown = score;
		FalseFade.gameObject.SetActive(value: false);
		FalseFade.color = new Color(0f, 0f, 0f, 0f);
		IsSelected = false;
		IsUnlockHard = unlockHard;
		RetryPressed = false;
		IsFailed = !complete;
		int num = (int)(score.CourseId + 1);
		switch (score.CourseId)
		{
		case BossRushManager.BossRushCourseId.COURSE_A_1:
			CourseName.text = ScriptLocalization.UI_BossRush.COURSE_A_1;
			break;
		case BossRushManager.BossRushCourseId.COURSE_A_2:
			CourseName.text = ScriptLocalization.UI_BossRush.COURSE_A_2;
			break;
		case BossRushManager.BossRushCourseId.COURSE_A_3:
			CourseName.text = ScriptLocalization.UI_BossRush.COURSE_A_3;
			break;
		case BossRushManager.BossRushCourseId.COURSE_B_1:
			CourseName.text = ScriptLocalization.UI_BossRush.COURSE_B_1;
			break;
		case BossRushManager.BossRushCourseId.COURSE_C_1:
			CourseName.text = ScriptLocalization.UI_BossRush.COURSE_C_1;
			break;
		case BossRushManager.BossRushCourseId.COURSE_D_1:
			CourseName.text = ScriptLocalization.UI_BossRush.COURSE_D_1;
			break;
		}
		if (complete)
		{
			CourseCompletedSuffix.gameObject.SetActive(value: true);
			CourseFailedSuffix.gameObject.SetActive(value: false);
		}
		else
		{
			CourseCompletedSuffix.gameObject.SetActive(value: false);
			CourseFailedSuffix.gameObject.SetActive(value: true);
		}
		if ((bool)NewRerecord)
		{
			NewRerecord.SetActive(score.IsNewHighScore);
		}
		ButtonRetry.SetActive(!complete);
		Sprite sprite = Course_A_1;
		switch (score.CourseId)
		{
		case BossRushManager.BossRushCourseId.COURSE_A_2:
			sprite = Course_A_2;
			break;
		case BossRushManager.BossRushCourseId.COURSE_A_3:
			sprite = Course_A_3;
			break;
		case BossRushManager.BossRushCourseId.COURSE_B_1:
			sprite = Course_B_1;
			break;
		case BossRushManager.BossRushCourseId.COURSE_C_1:
			sprite = Course_C_1;
			break;
		case BossRushManager.BossRushCourseId.COURSE_D_1:
			sprite = Course_D_1;
			break;
		}
		CourseImage.sprite = sprite;
		DificultNormal.SetActive(score.CourseMode == BossRushManager.BossRushCourseMode.NORMAL);
		DificultHard.SetActive(score.CourseMode == BossRushManager.BossRushCourseMode.HARD);
		foreach (Transform item in FlaskBase.transform)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in FlaskBase2.transform)
		{
			UnityEngine.Object.Destroy(item2.gameObject);
		}
		int num2 = 28;
		for (int i = 0; i < score.NumFlasksUsed && i < num2; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(FlaskElement, Vector3.zero, Quaternion.identity);
			if (i < num2 / 2)
			{
				gameObject.transform.SetParent(FlaskBase.transform);
			}
			else
			{
				gameObject.transform.SetParent(FlaskBase2.transform);
			}
			RectTransform rectTransform = (RectTransform)gameObject.transform;
			rectTransform.localRotation = Quaternion.identity;
			rectTransform.localScale = Vector3.one;
			rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0f);
			gameObject.SetActive(value: true);
		}
		FlaskElement.SetActive(value: false);
		DogesText.text = score.NumDodgesAchieved.ToString();
		BloodText.text = score.NumBloodPenancesUsed.ToString();
		HitsText.text = score.NumHitsReceived.ToString();
		PrayerText.text = score.NumPrayersUsed.ToString();
		TimeText.text = score.RunDurationInString();
		SetGrade(score.Score);
		BossRushHighScore bossRushHighScore = null;
		bossRushHighScore = ((!score.IsNewHighScore) ? Core.BossRushManager.GetHighScore(score.CourseId, score.CourseMode) : Core.BossRushManager.GetPrevHighScore(score.CourseId, score.CourseMode));
		if (bossRushHighScore != null)
		{
			PrevTimeText.text = bossRushHighScore.RunDurationInString();
		}
		else
		{
			PrevTimeText.text = "-- : -- : --";
		}
		FadeShow(checkInput: false, pauseGame, checkMainFade: false);
	}

	private void SetGrade(BossRushManager.BossRushCourseScore score)
	{
		ScoreLetters.ForEach(delegate(ScoreLetter x)
		{
			x.letterGameObject.SetActive(value: false);
		});
		ScoreLetters.Find((ScoreLetter x) => x.score == score).letterGameObject.SetActive(value: true);
	}

	public override bool AutomaticBack()
	{
		return false;
	}

	private void Update()
	{
		if (base.IsFading || ReInput.players.playerCount <= 0)
		{
			return;
		}
		Player player = ReInput.players.GetPlayer(0);
		bool flag = IsFailed && player.GetButtonDown(50);
		if (IsSelected || !showAnimationDone || FadeWidget.instance.Fading || (!flag && !player.GetButtonDown(51)))
		{
			return;
		}
		RetryPressed = flag;
		IsSelected = true;
		scoreShown = null;
		showAnimationDone = false;
		if (IsUnlockHard)
		{
			StartCoroutine(ShowPopUp());
			return;
		}
		FadeWidget.instance.Fade(toBlack: true, 1f, 0f, delegate
		{
			Hide();
		});
	}

	private IEnumerator ShowPopUp()
	{
		TextUnlocked.GetComponent<Text>().text = ScriptLocalization.UI_BossRush.TEXT_HARD_UNLOCKED;
		CanvasGroup group = TextUnlocked.GetComponent<CanvasGroup>();
		group.alpha = 0f;
		FalseFade.gameObject.SetActive(value: true);
		Tweener tween2 = FalseFade.DOColor(Color.black, 0.5f);
		yield return tween2.WaitForCompletion();
		tween2 = group.DOFade(1f, 0.4f);
		yield return new WaitForSecondsRealtime(4f);
		FadeWidget.instance.Fade(toBlack: true, 1f, 0f, delegate
		{
			Hide();
		});
	}
}
