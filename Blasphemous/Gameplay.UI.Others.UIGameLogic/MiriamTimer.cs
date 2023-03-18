using System;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using Tools.DataContainer;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.UIGameLogic;

public class MiriamTimer : SerializedMonoBehaviour
{
	public Text Text;

	public Color NotStartedColor;

	public Color CompletedColor;

	public Color FailedColor;

	public Color RunningColor;

	public RumbleData Rumble;

	private float targetTime;

	private float remainingTime;

	private bool isTimerRunning;

	private string lastTenSecondsEvent = "event:/SFX/UI/TimerLastSeconds";

	private string failEvent = "event:/SFX/UI/TimeFail";

	private bool lastTenSecondsEventPlayed;

	public static Core.SimpleEvent OnTimerRunOut { get; internal set; }

	private void Start()
	{
		Text.color = NotStartedColor;
		Hide();
	}

	private void Update()
	{
		if (!(Singleton<Core>.Instance == null) && Core.ready && base.gameObject.activeInHierarchy)
		{
			if (isTimerRunning)
			{
				UpdateRemainingTime();
			}
			Text.text = RunDurationInString(remainingTime);
		}
	}

	private void UpdateRemainingTime()
	{
		if (remainingTime <= 0f)
		{
			return;
		}
		remainingTime -= Time.deltaTime;
		if (remainingTime < 0f)
		{
			remainingTime = 0f;
			StopTimer(completed: false);
			Core.Audio.PlayOneShot(failEvent);
		}
		else if (remainingTime < 10f)
		{
			if (!lastTenSecondsEventPlayed)
			{
				lastTenSecondsEventPlayed = true;
				Core.Audio.PlayNamedSound("event:/SFX/UI/TimerLastSeconds", lastTenSecondsEvent);
			}
			if (Core.Input.AppliedRumbles().Count == 0)
			{
				Core.Input.ApplyRumble(Rumble);
			}
		}
	}

	public string RunDurationInString(float remainingTime)
	{
		int num = Mathf.FloorToInt(remainingTime) / 60;
		TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
		int seconds = timeSpan.Seconds;
		int num2 = timeSpan.Milliseconds / 10;
		return $"{num:D2} : {seconds:D2} : {num2:D2}";
	}

	public void StartTimer()
	{
		isTimerRunning = true;
		Text.color = RunningColor;
		lastTenSecondsEventPlayed = false;
		Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, new Core.SimpleEvent(OnPlayerDead));
	}

	private void OnPlayerDead()
	{
		StopTimer(completed: false);
	}

	public void StopTimer(bool completed)
	{
		isTimerRunning = false;
		Text.color = ((!completed) ? FailedColor : CompletedColor);
		Core.Audio.StopNamedSound(lastTenSecondsEvent);
		if (remainingTime == 0f)
		{
			PlayMakerFSM.BroadcastEvent("ON TIMER RUNS OUT");
			if (OnTimerRunOut != null)
			{
				OnTimerRunOut();
			}
		}
		Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Remove(penitent.OnDead, new Core.SimpleEvent(OnPlayerDead));
	}

	public void SetTargetTime(float targetTime)
	{
		this.targetTime = targetTime;
		remainingTime = targetTime;
	}

	public void Show()
	{
		Text.color = NotStartedColor;
		UIController.instance.HidePurgePoints();
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		Text.color = NotStartedColor;
		base.gameObject.SetActive(value: false);
		UIController.instance.ShowPurgePoints();
	}
}
