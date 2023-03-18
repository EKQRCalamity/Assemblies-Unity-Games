using System;
using System.Collections;
using System.Collections.Generic;
using Framework.BossRush;
using Framework.FrameworkCore;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using Gameplay.GameControllers.Penitent.Damage;
using Gameplay.UI;
using Gameplay.UI.Widgets;
using UnityEngine;

namespace Framework.Managers;

public class BossRushManager : GameSystem
{
	public enum BossRushCourseId
	{
		COURSE_A_1 = 0,
		COURSE_A_2 = 1,
		COURSE_A_3 = 2,
		COURSE_B_1 = 10,
		COURSE_C_1 = 20,
		COURSE_D_1 = 30
	}

	public enum BossRushCourseMode
	{
		NORMAL,
		HARD
	}

	public enum BossRushCourseScore
	{
		F_MINUS,
		F,
		F_PLUS,
		E_MINUS,
		E,
		E_PLUS,
		D_MINUS,
		D,
		D_PLUS,
		C_MINUS,
		C,
		C_PLUS,
		B_MINUS,
		B,
		B_PLUS,
		A_MINUS,
		A,
		A_PLUS,
		S_MINUS,
		S,
		S_PLUS
	}

	[Serializable]
	private class CourseData
	{
		public Dictionary<BossRushCourseMode, BossRushHighScore> highScores = new Dictionary<BossRushCourseMode, BossRushHighScore>();

		public Dictionary<BossRushCourseMode, BossRushHighScore> prevHighScores = new Dictionary<BossRushCourseMode, BossRushHighScore>();

		public Dictionary<BossRushCourseMode, bool> unlocked = new Dictionary<BossRushCourseMode, bool>();

		public BossRushHighScore GetHighScore(BossRushCourseMode mode)
		{
			BossRushHighScore result = null;
			if (highScores.ContainsKey(mode))
			{
				result = highScores[mode];
			}
			return result;
		}

		public BossRushHighScore GetPrevHighScore(BossRushCourseMode mode)
		{
			BossRushHighScore result = null;
			if (prevHighScores.ContainsKey(mode))
			{
				result = prevHighScores[mode];
			}
			return result;
		}

		public void SetHighScoreIfBetter(BossRushCourseMode mode, BossRushHighScore score)
		{
			if (score.WasTheCourseCompleted)
			{
				BossRushHighScore highScore = GetHighScore(mode);
				bool flag = score.IsBetterThan(highScore);
				if (flag)
				{
					prevHighScores[mode] = highScore;
					highScores[mode] = score;
				}
				score.IsNewHighScore = flag;
			}
		}

		public bool HasAnyModeUnlocked()
		{
			bool flag = false;
			foreach (KeyValuePair<BossRushCourseMode, bool> item in unlocked)
			{
				flag = flag || item.Value;
			}
			return flag;
		}

		public bool IsModeUnlocked(BossRushCourseMode mode)
		{
			bool result = false;
			if (unlocked.ContainsKey(mode))
			{
				result = unlocked[mode];
			}
			return result;
		}
	}

	[Serializable]
	private class BossRushData : ILocalData
	{
		public Dictionary<BossRushCourseId, CourseData> Data = new Dictionary<BossRushCourseId, CourseData>();

		public string GetFileName()
		{
			return "BossRushData.config";
		}

		public void Clean()
		{
			Data.Clear();
		}

		public bool UnlockCourse(BossRushCourseId course, BossRushCourseMode mode = BossRushCourseMode.NORMAL)
		{
			if (!Data.ContainsKey(course))
			{
				Data[course] = new CourseData();
			}
			if (!Data[course].unlocked.ContainsKey(mode))
			{
				Data[course].unlocked[mode] = false;
			}
			bool result = !Data[course].unlocked[mode] && mode == BossRushCourseMode.HARD;
			Data[course].unlocked[mode] = true;
			return result;
		}
	}

	private const string HUB_SCENE = "D22Z01S00";

	private const string LAUDES_FLAG = "SANTOS_LAUDES_DEFEATED";

	private const string HUSK_FLAG = "PONTIFF_HUSK_DEFEATED";

	public const string COURSE_FLAG_SUFFIX = "_UNLOCKED";

	public const string HEALTH_FONT_FLAG = "BOSSRUSH_HEALTH_FONT";

	public const string FERVOUR_FONT_FLAG = "BOSSRUSH_FERVOUR_FONT";

	private const int BOSSRUSH_SLOT = 7;

	private Dictionary<BossRushCourseId, BossRushCourse> courseScenesByCourseId = new Dictionary<BossRushCourseId, BossRushCourse>();

	private BossRushCourseId currentCourseId;

	private BossRushCourseMode currentCourseMode;

	private int currentCourseSceneIndex;

	private BossRushHighScore currentScore;

	private int SourceSlot = -1;

	private const string FADE_TO_WHITE_SOUND = "event:/SFX/UI/FadeToWhite";

	private BossRushData courseData = new BossRushData();

	private LocalDataFile configFile;

	public override void Initialize()
	{
		configFile = new LocalDataFile(courseData);
		LoadCourseScenes();
	}

	private void LoadCourseScenes()
	{
		courseScenesByCourseId[BossRushCourseId.COURSE_A_1] = Resources.Load<BossRushCourse>("BossRush/COURSE_A_1");
		courseScenesByCourseId[BossRushCourseId.COURSE_A_2] = Resources.Load<BossRushCourse>("BossRush/COURSE_A_2");
		courseScenesByCourseId[BossRushCourseId.COURSE_A_3] = Resources.Load<BossRushCourse>("BossRush/COURSE_A_3");
		courseScenesByCourseId[BossRushCourseId.COURSE_B_1] = Resources.Load<BossRushCourse>("BossRush/COURSE_B_1");
		courseScenesByCourseId[BossRushCourseId.COURSE_C_1] = Resources.Load<BossRushCourse>("BossRush/COURSE_C_1");
		courseScenesByCourseId[BossRushCourseId.COURSE_D_1] = Resources.Load<BossRushRandomCourse>("BossRush/COURSE_D_1");
	}

	public override void Update()
	{
		base.Update();
		if (currentScore != null)
		{
			currentScore.UpdateTimer();
		}
	}

	public override void Dispose()
	{
	}

	public void StartCourse(BossRushCourseId courseId, BossRushCourseMode courseMode, int sourceSlot = -1)
	{
		SourceSlot = sourceSlot;
		currentCourseSceneIndex = -1;
		currentCourseId = courseId;
		currentCourseMode = courseMode;
		Debug.Log("--- Start Course, initial slot " + sourceSlot);
		if (SourceSlot != -1)
		{
			Core.Logic.ResetAllData();
			Core.Persistence.LoadGameWithOutRespawn(SourceSlot);
		}
		Core.GameModeManager.ChangeMode(GameModeManager.GAME_MODES.BOSS_RUSH);
		PersistentManager.SetAutomaticSlot(7);
		Core.Logic.Penitent.Stats.Life.SetToCurrentMax();
		Core.Logic.Penitent.Stats.Flask.SetToCurrentMax();
		Core.Logic.Penitent.Stats.Purge.SetToCurrentMax();
		Core.Logic.Penitent.Stats.Fervour.Current = 0f;
		Core.PenitenceManager.DeactivateCurrentPenitence();
		Core.InventoryManager.RemoveSword("HE201");
		Core.Persistence.SaveGame(fullSave: false);
		currentScore = new BossRushHighScore(currentCourseId, currentCourseMode);
		SuscribeToHighScoreRelatedEvents();
		if (courseScenesByCourseId[currentCourseId].GetType() == typeof(BossRushRandomCourse))
		{
			BossRushRandomCourse bossRushRandomCourse = (BossRushRandomCourse)courseScenesByCourseId[currentCourseId];
			bossRushRandomCourse.RandomizeNextScenesList = true;
		}
		LoadHub(whiteFade: false);
		UIController.instance.HidePurgePoints();
	}

	public BossRushCourseMode GetCurrentBossRushMode()
	{
		return currentCourseMode;
	}

	public string GetCurrentRunDurationString()
	{
		string result = string.Empty;
		if (currentScore != null)
		{
			result = currentScore.RunDurationInString();
		}
		return result;
	}

	public bool IsLastScene()
	{
		List<string> scenes = courseScenesByCourseId[currentCourseId].GetScenes();
		string value = scenes[scenes.Count - 1];
		return Core.LevelManager.currentLevel.LevelName.Equals(value);
	}

	public void LoadHub(bool whiteFade = true)
	{
		Core.SpawnManager.InitialScene = "D22Z01S00";
		Core.SpawnManager.SetInitialSpawn("D22Z01S00");
		Core.LevelManager.ActivatePrecachedScene();
		Core.SpawnManager.FirstSpanw = true;
		if (!Core.LevelManager.currentLevel.LevelName.Equals("D22Z01S00"))
		{
			if (whiteFade)
			{
				Singleton<Core>.Instance.StartCoroutine(LoadHubFadeWhiteCourrutine());
			}
			else
			{
				Core.LevelManager.ChangeLevel("D22Z01S00");
			}
		}
		UIController.instance.CanEquipSwordHearts = true;
		List<string> scenes = courseScenesByCourseId[currentCourseId].GetScenes();
		List<string> fontRechargingScenes = courseScenesByCourseId[currentCourseId].GetFontRechargingScenes();
		if (currentCourseSceneIndex >= 0 && fontRechargingScenes.Contains(scenes[currentCourseSceneIndex]))
		{
			if (Core.Events.GetFlag("BOSSRUSH_HEALTH_FONT"))
			{
				Core.Events.SetFlag("BOSSRUSH_HEALTH_FONT", b: false);
				Singleton<Core>.Instance.StartCoroutine(LaunchHealthFontRechargingVfx());
			}
			if (Core.Events.GetFlag("BOSSRUSH_FERVOUR_FONT"))
			{
				Core.Events.SetFlag("BOSSRUSH_FERVOUR_FONT", b: false);
				Singleton<Core>.Instance.StartCoroutine(LaunchFervourFontRechargingVfx());
			}
		}
	}

	private IEnumerator LaunchHealthFontRechargingVfx()
	{
		yield return new WaitUntil(() => Core.LevelManager.currentLevel.LevelName.Equals("D22Z01S00"));
		yield return new WaitForSeconds(1f);
		GameObject vfx = courseScenesByCourseId[currentCourseId].healthFontRechargingVfx;
		PoolManager.Instance.ReuseObject(vfx, new Vector2(-2f, -2.5f), Quaternion.identity, createPoolIfNeeded: true);
	}

	private IEnumerator LaunchFervourFontRechargingVfx()
	{
		yield return new WaitUntil(() => Core.LevelManager.currentLevel.LevelName.Equals("D22Z01S00"));
		yield return new WaitForSeconds(1f);
		GameObject vfx = courseScenesByCourseId[currentCourseId].fervourFontRechargingVfx;
		PoolManager.Instance.ReuseObject(vfx, new Vector2(2f, -2.5f), Quaternion.identity, createPoolIfNeeded: true);
	}

	private IEnumerator LoadHubFadeWhiteCourrutine()
	{
		Core.Audio.PlayOneShot("event:/SFX/UI/FadeToWhite");
		yield return new WaitForSecondsRealtime(1f);
		yield return FadeWidget.instance.FadeCoroutine(new Color(0f, 0f, 0f, 0f), Color.white, 2f, toBlack: true, null);
		Core.LevelManager.ChangeLevel("D22Z01S00", startFromEditor: false, useFade: false, forceDeactivate: false, Color.white);
	}

	public void LoadLastScene()
	{
		List<string> scenes = courseScenesByCourseId[currentCourseId].GetScenes();
		currentCourseSceneIndex = scenes.Count - 2;
		LoadCourseNextScene();
	}

	public void LoadCourseNextScene()
	{
		if (currentCourseSceneIndex == -1)
		{
			StartTimerAndShow();
		}
		List<string> scenes = courseScenesByCourseId[currentCourseId].GetScenes();
		if (currentCourseSceneIndex == scenes.Count - 1)
		{
			Debug.LogError("Course has already reached its final scene! Use EndCourse instead.");
		}
		else
		{
			currentCourseSceneIndex++;
			currentScore.NumScenesCompleted++;
		}
		string teleportId = "TELEPORT_" + scenes[currentCourseSceneIndex];
		Core.SpawnManager.Teleport(teleportId);
		UIController.instance.CanEquipSwordHearts = false;
	}

	public void EndCourse(bool completed)
	{
		bool unlockHard = false;
		if (completed)
		{
			currentScore.NumScenesCompleted++;
			if (currentCourseMode == BossRushCourseMode.NORMAL)
			{
				unlockHard = courseData.UnlockCourse(currentCourseId, BossRushCourseMode.HARD);
			}
		}
		UnsuscribeToHighScoreRelatedEvents();
		Core.GameModeManager.ChangeMode(GameModeManager.GAME_MODES.MENU);
		StopTimerAndHide();
		currentScore.CalculateScoreObtained(completed);
		CourseData value = null;
		if (!courseData.Data.TryGetValue(currentCourseId, out value))
		{
			courseData.UnlockCourse(currentCourseId, currentCourseMode);
			value = courseData.Data[currentCourseId];
		}
		value.SetHighScoreIfBetter(currentCourseMode, currentScore);
		configFile.SaveData();
		LogHighScoreObtained();
		Singleton<Core>.Instance.StartCoroutine(ShowScoreAndGoNext(pauseGame: true, completed, unlockHard));
		if (currentCourseId == BossRushCourseId.COURSE_C_1 && courseData.Data[currentCourseId].GetHighScore(BossRushCourseMode.HARD) != null && courseData.Data[currentCourseId].GetHighScore(BossRushCourseMode.HARD).WasTheCourseCompleted)
		{
			Core.ColorPaletteManager.UnlockBossRushColorPalette();
		}
		CheckToGrantBossRushRankSPalette();
	}

	private void CheckToGrantBossRushRankSPalette()
	{
		if (courseData == null || courseData.Data == null || !courseData.Data.ContainsKey(currentCourseId) || courseData.Data[currentCourseId].GetHighScore(currentCourseMode) == null || !courseData.Data[currentCourseId].GetHighScore(currentCourseMode).WasTheCourseCompleted)
		{
			return;
		}
		bool flag = true && courseData.Data.ContainsKey(BossRushCourseId.COURSE_A_1) && courseData.Data.ContainsKey(BossRushCourseId.COURSE_A_2) && courseData.Data.ContainsKey(BossRushCourseId.COURSE_A_3) && courseData.Data.ContainsKey(BossRushCourseId.COURSE_B_1) && courseData.Data.ContainsKey(BossRushCourseId.COURSE_C_1) && courseData.Data.ContainsKey(BossRushCourseId.COURSE_D_1);
		if (!flag)
		{
			return;
		}
		foreach (KeyValuePair<BossRushCourseId, CourseData> datum in courseData.Data)
		{
			bool flag2 = datum.Value.GetHighScore(BossRushCourseMode.NORMAL) != null && datum.Value.GetHighScore(BossRushCourseMode.NORMAL).Score >= BossRushCourseScore.S_MINUS && datum.Value.GetHighScore(BossRushCourseMode.HARD) != null && datum.Value.GetHighScore(BossRushCourseMode.HARD).Score >= BossRushCourseScore.S_MINUS;
			flag = flag && flag2;
			if (!flag)
			{
				break;
			}
		}
		if (flag)
		{
			Core.ColorPaletteManager.UnlockBossRushRankSColorPalette();
		}
	}

	public void LogHighScoreObtained()
	{
		Debug.Log("<b> - Number of hits received: " + currentScore.NumHitsReceived + "</b>");
		Debug.Log("<b> - Number of blood penances used: " + currentScore.NumBloodPenancesUsed + "</b>");
		Debug.Log("<b> - Number of dodges achieved: " + currentScore.NumDodgesAchieved + "</b>");
		Debug.Log("<b> - Number of flasks used: " + currentScore.NumFlasksUsed + "</b>");
		Debug.Log("<b> - Number of prayers used: " + currentScore.NumPrayersUsed + "</b>");
		Debug.Log("<b> - Run duration: " + currentScore.RunDuration + "</b>");
		Debug.Log(string.Concat("<b> - FINAL SCORE: ", currentScore.Score, "</b>"));
	}

	public bool IsAnyCourseUnlocked()
	{
		bool flag = false;
		foreach (KeyValuePair<BossRushCourseId, CourseData> datum in courseData.Data)
		{
			flag = flag || datum.Value.HasAnyModeUnlocked();
		}
		return flag;
	}

	public bool IsModeUnlocked(BossRushCourseId courseId, BossRushCourseMode mode)
	{
		bool result = false;
		if (courseData.Data.ContainsKey(courseId))
		{
			result = courseData.Data[courseId].IsModeUnlocked(mode);
		}
		return result;
	}

	public BossRushHighScore GetHighScore(BossRushCourseId courseId, BossRushCourseMode mode)
	{
		BossRushHighScore result = null;
		if (courseData.Data.ContainsKey(courseId))
		{
			result = courseData.Data[courseId].GetHighScore(mode);
		}
		return result;
	}

	public BossRushHighScore GetPrevHighScore(BossRushCourseId courseId, BossRushCourseMode mode)
	{
		BossRushHighScore result = null;
		if (courseData.Data.ContainsKey(courseId))
		{
			result = courseData.Data[courseId].GetPrevHighScore(mode);
		}
		return result;
	}

	public void DEBUGUnlockCourse(string courseId)
	{
		BossRushCourseId course = (BossRushCourseId)Enum.Parse(typeof(BossRushCourseId), courseId);
		courseData.UnlockCourse(course);
	}

	public List<BossRushCourseId> GetUnlockedCourses()
	{
		List<BossRushCourseId> list = new List<BossRushCourseId>();
		foreach (KeyValuePair<BossRushCourseId, CourseData> datum in courseData.Data)
		{
			if (datum.Value.HasAnyModeUnlocked())
			{
				list.Add(datum.Key);
			}
		}
		return list;
	}

	public static List<string> GetAllCoursesNames()
	{
		List<string> list = new List<string>();
		BossRushCourseId[] array = (BossRushCourseId[])Enum.GetValues(typeof(BossRushCourseId));
		BossRushCourseId[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			BossRushCourseId bossRushCourseId = array2[i];
			list.Add(bossRushCourseId.ToString());
		}
		return list;
	}

	public void CheckCoursesUnlockBySlots()
	{
		bool flag = false;
		for (int i = 0; i < 3; i++)
		{
			PersistentManager.PublicSlotData slotData = Core.Persistence.GetSlotData(i);
			if (slotData != null && !slotData.persistence.Corrupted && slotData.persistence != null)
			{
				if (CheckToUnlockCourseA1(slotData))
				{
					flag = true;
					courseData.UnlockCourse(BossRushCourseId.COURSE_A_1);
				}
				if (CheckToUnlockCourseA2(slotData))
				{
					flag = true;
					courseData.UnlockCourse(BossRushCourseId.COURSE_A_2);
				}
				if (CheckToUnlockCourseA3(slotData))
				{
					flag = true;
					courseData.UnlockCourse(BossRushCourseId.COURSE_A_3);
				}
				if (CheckToUnlockCourseB1(slotData))
				{
					flag = true;
					courseData.UnlockCourse(BossRushCourseId.COURSE_B_1);
				}
				if (CheckToUnlockCourseC1(slotData))
				{
					flag = true;
					courseData.UnlockCourse(BossRushCourseId.COURSE_C_1);
				}
				if (CheckToUnlockCourseD1(slotData))
				{
					flag = true;
					courseData.UnlockCourse(BossRushCourseId.COURSE_D_1);
				}
			}
		}
		if (flag)
		{
			configFile.SaveData();
		}
	}

	public int CountScenesInCourse(BossRushCourseId id)
	{
		return courseScenesByCourseId[id].GetScenes().Count;
	}

	public BossRushCourseScore GetRankByTimePassed(BossRushCourseId id, BossRushCourseMode mode, int minutes)
	{
		List<ScoreInterval> list = ((mode != 0) ? courseScenesByCourseId[id].ScoresByMinutesInHard : courseScenesByCourseId[id].ScoresByMinutesInNormal);
		foreach (ScoreInterval item in list)
		{
			if (item.score <= courseScenesByCourseId[id].MaxScoreForFailedRuns)
			{
				break;
			}
			if (item.timeRangeInMinutes.x <= (float)minutes && item.timeRangeInMinutes.y > (float)minutes)
			{
				return item.score;
			}
		}
		return courseScenesByCourseId[id].MaxScoreForFailedRuns;
	}

	public BossRushCourseScore GetRankByNumberOfCompletedBossfights(BossRushCourseId id, int numBossfightsCompleted)
	{
		int num = CountScenesInCourse(id);
		float num2 = (float)numBossfightsCompleted / (float)num;
		float num3 = (float)courseScenesByCourseId[id].MaxScoreForFailedRuns * num2;
		return (BossRushCourseScore)num3;
	}

	private IEnumerator ShowScoreAndGoNext(bool pauseGame, bool complete, bool unlockHard)
	{
		yield return Singleton<Core>.Instance.StartCoroutine(UIController.instance.ShowBossRushRanksAndWait(currentScore, pauseGame, complete, unlockHard));
		if (UIController.instance.BossRushRetryPressed)
		{
			StartCourse(currentCourseId, currentCourseMode, SourceSlot);
			yield break;
		}
		UIController.instance.ShowLoad(show: true, Color.black);
		Core.Logic.LoadMenuScene(useFade: false);
		UIController.instance.ShowPurgePoints();
	}

	private void SuscribeToHighScoreRelatedEvents()
	{
		UnsuscribeToHighScoreRelatedEvents();
		PenitentDamageArea.OnHitGlobal = (PenitentDamageArea.PlayerHitEvent)Delegate.Combine(PenitentDamageArea.OnHitGlobal, new PenitentDamageArea.PlayerHitEvent(OnHitGlobal));
		PenitentDamageArea.OnDamagedGlobal = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Combine(PenitentDamageArea.OnDamagedGlobal, new PenitentDamageArea.PlayerDamagedEvent(OnDamagedGlobal));
		FervourPenance.OnPenanceStart = (Core.SimpleEvent)Delegate.Combine(FervourPenance.OnPenanceStart, new Core.SimpleEvent(OnPenanceStart));
		Healing.OnHealingStart = (Core.SimpleEvent)Delegate.Combine(Healing.OnHealingStart, new Core.SimpleEvent(OnHealingStart));
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
		LevelManager.OnBeforeLevelLoad += OnLevelPreload;
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		PrayerUse prayerCast = Core.Logic.Penitent.PrayerCast;
		prayerCast.OnPrayerStart = (Core.SimpleEvent)Delegate.Combine(prayerCast.OnPrayerStart, new Core.SimpleEvent(OnPrayerStart));
	}

	private void OnLevelPreload(Level oldlevel, Level newlevel)
	{
		PrayerUse prayerCast = Core.Logic.Penitent.PrayerCast;
		prayerCast.OnPrayerStart = (Core.SimpleEvent)Delegate.Remove(prayerCast.OnPrayerStart, new Core.SimpleEvent(OnPrayerStart));
	}

	private void UnsuscribeToHighScoreRelatedEvents()
	{
		PenitentDamageArea.OnHitGlobal = (PenitentDamageArea.PlayerHitEvent)Delegate.Remove(PenitentDamageArea.OnHitGlobal, new PenitentDamageArea.PlayerHitEvent(OnHitGlobal));
		PenitentDamageArea.OnDamagedGlobal = (PenitentDamageArea.PlayerDamagedEvent)Delegate.Remove(PenitentDamageArea.OnDamagedGlobal, new PenitentDamageArea.PlayerDamagedEvent(OnDamagedGlobal));
		FervourPenance.OnPenanceStart = (Core.SimpleEvent)Delegate.Remove(FervourPenance.OnPenanceStart, new Core.SimpleEvent(OnPenanceStart));
		Healing.OnHealingStart = (Core.SimpleEvent)Delegate.Remove(Healing.OnHealingStart, new Core.SimpleEvent(OnHealingStart));
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
		LevelManager.OnBeforeLevelLoad -= OnLevelPreload;
		PrayerUse prayerCast = Core.Logic.Penitent.PrayerCast;
		prayerCast.OnPrayerStart = (Core.SimpleEvent)Delegate.Remove(prayerCast.OnPrayerStart, new Core.SimpleEvent(OnPrayerStart));
	}

	private void OnHitGlobal(Penitent penitent, Hit hit)
	{
		if (hit.DamageAmount > 0f && !hit.Unnavoidable && penitent.IsDashing)
		{
			currentScore.NumDodgesAchieved++;
		}
	}

	private void OnDamagedGlobal(Penitent damaged, Hit hit)
	{
		if (!(hit.DamageAmount <= 0f))
		{
			currentScore.NumHitsReceived++;
		}
	}

	private void OnPrayerStart()
	{
		currentScore.NumPrayersUsed++;
	}

	private void OnPenanceStart()
	{
		currentScore.NumBloodPenancesUsed++;
	}

	private void OnHealingStart()
	{
		currentScore.NumFlasksUsed++;
	}

	private void StartTimerAndShow()
	{
		currentScore.IsTimerActive = true;
		UIController.instance.ShowBossRushTimer();
	}

	private void StopTimerAndHide()
	{
		currentScore.IsTimerActive = false;
		UIController.instance.HideBossRushTimer();
	}

	private bool CheckToUnlockCourseA1(PersistentManager.PublicSlotData data)
	{
		if (courseData.Data.ContainsKey(BossRushCourseId.COURSE_A_1) && courseData.Data[BossRushCourseId.COURSE_A_1].HasAnyModeUnlocked())
		{
			return false;
		}
		return data.persistence.IsNewGamePlus || data.persistence.CanConvertToNewGamePlus;
	}

	private bool CheckToUnlockCourseA2(PersistentManager.PublicSlotData data)
	{
		if (courseData.Data.ContainsKey(BossRushCourseId.COURSE_A_2) && courseData.Data[BossRushCourseId.COURSE_A_2].HasAnyModeUnlocked())
		{
			return false;
		}
		return IsModeUnlocked(BossRushCourseId.COURSE_A_1, BossRushCourseMode.HARD);
	}

	private bool CheckToUnlockCourseA3(PersistentManager.PublicSlotData data)
	{
		if (courseData.Data.ContainsKey(BossRushCourseId.COURSE_A_3) && courseData.Data[BossRushCourseId.COURSE_A_3].HasAnyModeUnlocked())
		{
			return false;
		}
		return IsModeUnlocked(BossRushCourseId.COURSE_A_2, BossRushCourseMode.HARD);
	}

	private bool CheckToUnlockCourseB1(PersistentManager.PublicSlotData data)
	{
		if (courseData.Data.ContainsKey(BossRushCourseId.COURSE_B_1) && courseData.Data[BossRushCourseId.COURSE_B_1].HasAnyModeUnlocked())
		{
			return false;
		}
		string key = BossRushCourseId.COURSE_B_1.ToString() + "_UNLOCKED";
		bool result = false;
		if (data.flags != null)
		{
			if (data.flags.flags.ContainsKey(key))
			{
				result = data.flags.flags[key];
			}
			else if (data.flags.flags.ContainsKey("SANTOS_LAUDES_DEFEATED"))
			{
				result = data.flags.flags["SANTOS_LAUDES_DEFEATED"];
			}
		}
		return result;
	}

	private bool CheckToUnlockCourseC1(PersistentManager.PublicSlotData data)
	{
		if (courseData.Data.ContainsKey(BossRushCourseId.COURSE_C_1) && courseData.Data[BossRushCourseId.COURSE_C_1].HasAnyModeUnlocked())
		{
			return false;
		}
		return GetHighScore(BossRushCourseId.COURSE_A_1, BossRushCourseMode.HARD) != null && GetHighScore(BossRushCourseId.COURSE_A_1, BossRushCourseMode.HARD).WasTheCourseCompleted && GetHighScore(BossRushCourseId.COURSE_A_2, BossRushCourseMode.HARD) != null && GetHighScore(BossRushCourseId.COURSE_A_2, BossRushCourseMode.HARD).WasTheCourseCompleted && GetHighScore(BossRushCourseId.COURSE_A_3, BossRushCourseMode.HARD) != null && GetHighScore(BossRushCourseId.COURSE_A_3, BossRushCourseMode.HARD).WasTheCourseCompleted && GetHighScore(BossRushCourseId.COURSE_B_1, BossRushCourseMode.HARD) != null && GetHighScore(BossRushCourseId.COURSE_B_1, BossRushCourseMode.HARD).WasTheCourseCompleted;
	}

	private bool CheckToUnlockCourseD1(PersistentManager.PublicSlotData data)
	{
		if (courseData.Data.ContainsKey(BossRushCourseId.COURSE_D_1) && courseData.Data[BossRushCourseId.COURSE_D_1].HasAnyModeUnlocked())
		{
			return false;
		}
		string key = BossRushCourseId.COURSE_D_1.ToString() + "_UNLOCKED";
		if (GetHighScore(BossRushCourseId.COURSE_C_1, BossRushCourseMode.NORMAL) == null || !GetHighScore(BossRushCourseId.COURSE_C_1, BossRushCourseMode.NORMAL).WasTheCourseCompleted || GetHighScore(BossRushCourseId.COURSE_C_1, BossRushCourseMode.HARD) == null || !GetHighScore(BossRushCourseId.COURSE_C_1, BossRushCourseMode.HARD).WasTheCourseCompleted)
		{
			return false;
		}
		bool result = false;
		if (data.flags != null)
		{
			if (data.flags.flags.ContainsKey(key))
			{
				result = data.flags.flags[key];
			}
			else if (data.flags.flags.ContainsKey("PONTIFF_HUSK_DEFEATED"))
			{
				result = data.flags.flags["PONTIFF_HUSK_DEFEATED"];
			}
		}
		return result;
	}
}
