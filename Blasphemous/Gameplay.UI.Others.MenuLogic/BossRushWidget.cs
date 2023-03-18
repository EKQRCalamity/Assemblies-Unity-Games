using System;
using System.Collections.Generic;
using Framework.BossRush;
using Framework.Managers;
using Gameplay.UI.Others.Buttons;
using Gameplay.UI.Widgets;
using I2.Loc;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class BossRushWidget : BasicUIBlockingWidget
{
	public enum States
	{
		Slot,
		Course,
		Difficulty
	}

	[BoxGroup("Widgets", true, false, 0)]
	public BasicUIBlockingWidget SlotSelector;

	[BoxGroup("Widgets", true, false, 0)]
	public BasicUIBlockingWidget CourseSelector;

	[BoxGroup("Widgets", true, false, 0)]
	public BasicUIBlockingWidget DifficultySelector;

	[BoxGroup("Slots", true, false, 0)]
	public SelectSaveSlots selectSaveSlotsWidget;

	[BoxGroup("Course", true, false, 0)]
	public RectTransform CoursesRoot;

	[BoxGroup("Course", true, false, 0)]
	public BossRushButton CourseA1Button;

	[BoxGroup("Course", true, false, 0)]
	public BossRushButton CourseA2Button;

	[BoxGroup("Course", true, false, 0)]
	public BossRushButton CourseA3Button;

	[BoxGroup("Course", true, false, 0)]
	public BossRushButton CourseB1Button;

	[BoxGroup("Course", true, false, 0)]
	public BossRushButton CourseC1Button;

	[BoxGroup("Course", true, false, 0)]
	public BossRushButton CourseD1Button;

	[BoxGroup("Course", true, false, 0)]
	public GameObject LeftArrowGO;

	[BoxGroup("Course", true, false, 0)]
	public GameObject RightArrowGO;

	[BoxGroup("Course", true, false, 0)]
	public Text HowToUnlockText;

	[BoxGroup("Difficult", true, false, 0)]
	public Text NormalModeText;

	[BoxGroup("Difficult", true, false, 0)]
	public Text HardModeText;

	[BoxGroup("Difficult", true, false, 0)]
	public Button HardModeButton;

	[BoxGroup("Difficult", true, false, 0)]
	public BossRushButton ResumeButtonA1;

	[BoxGroup("Difficult", true, false, 0)]
	public BossRushButton ResumeButtonA2;

	[BoxGroup("Difficult", true, false, 0)]
	public BossRushButton ResumeButtonA3;

	[BoxGroup("Difficult", true, false, 0)]
	public BossRushButton ResumeButtonB1;

	[BoxGroup("Difficult", true, false, 0)]
	public BossRushButton ResumeButtonC1;

	[BoxGroup("Difficult", true, false, 0)]
	public BossRushButton ResumeButtonD1;

	[BoxGroup("Difficult", true, false, 0)]
	public Image NormalRankMedal;

	[BoxGroup("Difficult", true, false, 0)]
	public Image HardRankMedal;

	[BoxGroup("Difficult", true, false, 0)]
	public List<RankMedal> AllRanksMedals;

	[BoxGroup("Controls", true, false, 0)]
	public GameObject AcceptButton;

	[BoxGroup("Controls", true, false, 0)]
	public GameObject AcceptButtonDisabled;

	private States CurrentState;

	private List<BasicUIBlockingWidget> stateWidgets;

	private List<BossRushManager.BossRushCourseId> UnlockedCourses = new List<BossRushManager.BossRushCourseId>();

	private bool IsHardModeEnabled;

	public bool IsAllSelected { get; private set; }

	public int CurrentSlot { get; private set; }

	public BossRushManager.BossRushCourseId SelectedCourse { get; private set; }

	public BossRushManager.BossRushCourseMode SelectedMode { get; private set; }

	protected override void OnWidgetInitialize()
	{
		CurrentSlot = 0;
		stateWidgets = new List<BasicUIBlockingWidget> { SlotSelector, CourseSelector, DifficultySelector };
		stateWidgets.ForEach(delegate(BasicUIBlockingWidget x)
		{
			x.InitializeWidget();
		});
		IsAllSelected = false;
		HowToUnlockText.text = string.Empty;
	}

	protected override void OnWidgetShow()
	{
		selectSaveSlotsWidget.Clear();
		selectSaveSlotsWidget.SetAllData(null, SelectSaveSlots.SlotsModes.BossRush);
		SetState(States.Slot);
		IsAllSelected = false;
		HowToUnlockText.text = string.Empty;
		OptionSelected(0);
	}

	private void Update()
	{
		if (ReInput.players.playerCount <= 0)
		{
			return;
		}
		Player player = ReInput.players.GetPlayer(0);
		if (player.GetButtonDown(51) && CheckFading())
		{
			if (CurrentState > States.Slot)
			{
				SetState(CurrentState - 1);
			}
			else
			{
				FadeHide();
			}
		}
	}

	public void OptionSelected(int option)
	{
		bool flag = true;
		bool flag2 = false;
		switch (CurrentState)
		{
		case States.Slot:
			CurrentSlot = option;
			UnlockedCourses = Core.BossRushManager.GetUnlockedCourses();
			flag = selectSaveSlotsWidget.CanLoadSelectedSlot;
			break;
		case States.Course:
			switch (option)
			{
			case 0:
				LeftArrowGO.SetActive(value: false);
				RightArrowGO.SetActive(value: true);
				SelectedCourse = BossRushManager.BossRushCourseId.COURSE_A_1;
				break;
			case 1:
				LeftArrowGO.SetActive(value: true);
				RightArrowGO.SetActive(value: true);
				SelectedCourse = BossRushManager.BossRushCourseId.COURSE_A_2;
				HowToUnlockText.text = ScriptLocalization.UI_BossRush.LABEL_UNLOCK_COURSE_A_2.Replace("%", Environment.NewLine);
				break;
			case 2:
				LeftArrowGO.SetActive(value: true);
				RightArrowGO.SetActive(value: true);
				SelectedCourse = BossRushManager.BossRushCourseId.COURSE_A_3;
				HowToUnlockText.text = ScriptLocalization.UI_BossRush.LABEL_UNLOCK_COURSE_A_3.Replace("%", Environment.NewLine);
				break;
			case 3:
				LeftArrowGO.SetActive(value: true);
				RightArrowGO.SetActive(value: true);
				SelectedCourse = BossRushManager.BossRushCourseId.COURSE_B_1;
				HowToUnlockText.text = ScriptLocalization.UI_BossRush.LABEL_UNLOCK_COURSE_B_1.Replace("%", Environment.NewLine);
				break;
			case 4:
				LeftArrowGO.SetActive(value: true);
				RightArrowGO.SetActive(value: true);
				SelectedCourse = BossRushManager.BossRushCourseId.COURSE_C_1;
				HowToUnlockText.text = ScriptLocalization.UI_BossRush.LABEL_UNLOCK_COURSE_C_1.Replace("%", Environment.NewLine);
				break;
			case 5:
				LeftArrowGO.SetActive(value: true);
				RightArrowGO.SetActive(value: false);
				SelectedCourse = BossRushManager.BossRushCourseId.COURSE_D_1;
				HowToUnlockText.text = ScriptLocalization.UI_BossRush.LABEL_UNLOCK_COURSE_D_1.Replace("%", Environment.NewLine);
				break;
			}
			if (Core.BossRushManager.IsModeUnlocked(SelectedCourse, BossRushManager.BossRushCourseMode.NORMAL))
			{
				HowToUnlockText.text = string.Empty;
			}
			flag = UnlockedCourses.Contains(SelectedCourse);
			CoursesRoot.localPosition = new Vector3(CoursesRoot.localPosition.x, 32 + option * 68, CoursesRoot.localPosition.z);
			break;
		case States.Difficulty:
			switch (option)
			{
			case 0:
				SelectedMode = BossRushManager.BossRushCourseMode.NORMAL;
				flag = true;
				break;
			case 1:
				SelectedMode = BossRushManager.BossRushCourseMode.HARD;
				flag = Core.BossRushManager.IsModeUnlocked(SelectedCourse, BossRushManager.BossRushCourseMode.HARD);
				break;
			}
			break;
		}
		AcceptButton.SetActive(flag);
		AcceptButtonDisabled.SetActive(!flag);
	}

	public void OptionPressed(int option)
	{
		bool flag = true;
		if (!CheckFading())
		{
			return;
		}
		switch (CurrentState)
		{
		case States.Slot:
			if (!selectSaveSlotsWidget.CanLoadSelectedSlot)
			{
				flag = false;
				break;
			}
			CourseA1Button.SetData(1, UnlockedCourses.Contains(BossRushManager.BossRushCourseId.COURSE_A_1));
			CourseA2Button.SetData(2, UnlockedCourses.Contains(BossRushManager.BossRushCourseId.COURSE_A_2));
			CourseA3Button.SetData(3, UnlockedCourses.Contains(BossRushManager.BossRushCourseId.COURSE_A_3));
			CourseB1Button.SetData(4, UnlockedCourses.Contains(BossRushManager.BossRushCourseId.COURSE_B_1));
			CourseC1Button.SetData(5, UnlockedCourses.Contains(BossRushManager.BossRushCourseId.COURSE_C_1));
			CourseD1Button.SetData(6, UnlockedCourses.Contains(BossRushManager.BossRushCourseId.COURSE_D_1));
			break;
		case States.Course:
			if (!UnlockedCourses.Contains(SelectedCourse))
			{
				flag = false;
				break;
			}
			DeactivateResumeButtons();
			switch (SelectedCourse)
			{
			case BossRushManager.BossRushCourseId.COURSE_A_1:
				ResumeButtonA1.gameObject.SetActive(value: true);
				ResumeButtonA1.SetData(1, UnlockedCourses.Contains(BossRushManager.BossRushCourseId.COURSE_A_1));
				break;
			case BossRushManager.BossRushCourseId.COURSE_A_2:
				ResumeButtonA2.gameObject.SetActive(value: true);
				ResumeButtonA2.SetData(2, UnlockedCourses.Contains(BossRushManager.BossRushCourseId.COURSE_A_2));
				break;
			case BossRushManager.BossRushCourseId.COURSE_A_3:
				ResumeButtonA3.gameObject.SetActive(value: true);
				ResumeButtonA3.SetData(3, UnlockedCourses.Contains(BossRushManager.BossRushCourseId.COURSE_A_3));
				break;
			case BossRushManager.BossRushCourseId.COURSE_B_1:
				ResumeButtonB1.gameObject.SetActive(value: true);
				ResumeButtonB1.SetData(4, UnlockedCourses.Contains(BossRushManager.BossRushCourseId.COURSE_B_1));
				break;
			case BossRushManager.BossRushCourseId.COURSE_C_1:
				ResumeButtonC1.gameObject.SetActive(value: true);
				ResumeButtonC1.SetData(5, UnlockedCourses.Contains(BossRushManager.BossRushCourseId.COURSE_C_1));
				break;
			case BossRushManager.BossRushCourseId.COURSE_D_1:
				ResumeButtonD1.gameObject.SetActive(value: true);
				ResumeButtonD1.SetData(6, UnlockedCourses.Contains(BossRushManager.BossRushCourseId.COURSE_D_1));
				break;
			}
			IsHardModeEnabled = Core.BossRushManager.IsModeUnlocked(SelectedCourse, BossRushManager.BossRushCourseMode.HARD);
			HardModeButton.interactable = IsHardModeEnabled;
			SetHighScore(NormalModeText, NormalRankMedal, BossRushManager.BossRushCourseMode.NORMAL);
			SetHighScore(HardModeText, HardRankMedal, BossRushManager.BossRushCourseMode.HARD);
			break;
		case States.Difficulty:
			flag = false;
			if (SelectedMode != BossRushManager.BossRushCourseMode.HARD || IsHardModeEnabled)
			{
				FadeWidget.instance.Fade(toBlack: true, 1f, 0f, delegate
				{
					IsAllSelected = true;
					Hide();
				});
			}
			break;
		}
		if (flag)
		{
			SetState(CurrentState + 1);
		}
	}

	private void DeactivateResumeButtons()
	{
		ResumeButtonA1.gameObject.SetActive(value: false);
		ResumeButtonA2.gameObject.SetActive(value: false);
		ResumeButtonA3.gameObject.SetActive(value: false);
		ResumeButtonB1.gameObject.SetActive(value: false);
		ResumeButtonC1.gameObject.SetActive(value: false);
		ResumeButtonD1.gameObject.SetActive(value: false);
	}

	public void SetSaveSlot(int slot)
	{
		CurrentSlot = slot;
	}

	private void SetState(States state)
	{
		CurrentState = state;
		for (int i = 0; i < Enum.GetValues(typeof(States)).Length; i++)
		{
			if (i == (int)state)
			{
				stateWidgets[i].FadeShow(checkInput: false, pauseGame: false);
			}
			else
			{
				stateWidgets[i].FadeHide();
			}
		}
	}

	private bool CheckFading()
	{
		bool flag = true;
		foreach (BasicUIBlockingWidget stateWidget in stateWidgets)
		{
			flag = flag && !stateWidget.IsFading;
		}
		return flag;
	}

	private void SetHighScore(Text control, Image rank, BossRushManager.BossRushCourseMode mode)
	{
		string newValue = "-- : -- : --";
		BossRushHighScore score = Core.BossRushManager.GetHighScore(SelectedCourse, mode);
		if (score != null)
		{
			newValue = score.RunDurationInString();
			rank.gameObject.SetActive(value: true);
			rank.sprite = AllRanksMedals.Find((RankMedal x) => x.score == score.Score).sprite;
		}
		else
		{
			rank.gameObject.SetActive(value: false);
		}
		control.text = ScriptLocalization.UI_BossRush.TEXT_BESTTIME.Replace("%", newValue);
	}
}
