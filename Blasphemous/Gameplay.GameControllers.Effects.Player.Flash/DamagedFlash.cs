using System.Collections.Generic;
using Framework.Util;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Flash;

public class DamagedFlash : MonoBehaviour
{
	private enum FLASHSTATE
	{
		OFF,
		UP,
		HOLD,
		DOWN
	}

	public Color color = Color.red;

	public float holdTime = 0.5f;

	private bool isGetTimer;

	private bool isTimerStore;

	public float maxAlpha = 1f;

	private Texture2D pixel;

	public float rampDownTime = 0.5f;

	public float rampUpTime = 0.5f;

	public float startAlpha;

	private FLASHSTATE state;

	private Timer timer;

	private readonly List<Timer> timerList = new List<Timer>();

	private void Start()
	{
		pixel = new Texture2D(1, 1);
		color.a = startAlpha;
		pixel.SetPixel(0, 0, color);
		pixel.Apply();
	}

	private void Update()
	{
		switch (state)
		{
		case FLASHSTATE.UP:
			isTimerStore = false;
			if (timer.UpdateAndTest())
			{
				state = FLASHSTATE.HOLD;
				if (!isGetTimer)
				{
					timer = getTimer(holdTime);
				}
			}
			break;
		case FLASHSTATE.HOLD:
			isGetTimer = false;
			if (timer.UpdateAndTest())
			{
				state = FLASHSTATE.DOWN;
				if (!isGetTimer)
				{
					timer = getTimer(rampDownTime);
				}
			}
			break;
		case FLASHSTATE.DOWN:
			if (timer.UpdateAndTest() && !isTimerStore)
			{
				storeTimer(timer);
				isTimerStore = true;
			}
			break;
		}
	}

	private void SetPixelAlpha(float a)
	{
		color.a = a;
		pixel.SetPixel(0, 0, color);
		pixel.Apply();
	}

	public void OnGUI()
	{
		switch (state)
		{
		case FLASHSTATE.UP:
			SetPixelAlpha(Mathf.Lerp(startAlpha, maxAlpha, timer.Elapsed));
			break;
		case FLASHSTATE.DOWN:
			SetPixelAlpha(Mathf.Lerp(maxAlpha, startAlpha, timer.Elapsed));
			break;
		}
		GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), pixel);
	}

	public void TookDamage()
	{
		timer = getTimer(rampUpTime);
		isGetTimer = false;
		state = FLASHSTATE.UP;
	}

	private Timer getTimer(float _holdTime)
	{
		Timer timer;
		if (timerList.Count > 0)
		{
			timer = timerList[timerList.Count - 1];
			timer.TotalTime = _holdTime;
			timerList.Remove(timer);
		}
		else
		{
			timer = new Timer(_holdTime);
		}
		return timer;
	}

	private void storeTimer(Timer _timer)
	{
		timerList.Add(_timer);
	}

	private void drainTimerPool()
	{
		if (timerList.Count >= 0)
		{
			timerList.Clear();
		}
	}
}
