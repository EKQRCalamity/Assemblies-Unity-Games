using Rewired;
using UnityEngine;

public class Vibrator : AbstractMonoBehaviour
{
	private static float PlatformMultiplier = 1f;

	private Coroutine vibrateCoroutine;

	private float[] durationsLeft = new float[2];

	private float[] currentVibrations = new float[2];

	public static Vibrator Current { get; private set; }

	public static void Vibrate(float amount, float time, PlayerId player)
	{
		Current._Vibrate(amount, time, player);
	}

	public static void StopVibrating(PlayerId player)
	{
		Current._StopVibrating(player);
	}

	protected override void Awake()
	{
		base.Awake();
		Current = this;
	}

	private void Update()
	{
		if (!SettingsData.Data.canVibrate)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			float num = durationsLeft[i];
			float num2 = currentVibrations[i];
			num -= (float)CupheadTime.Delta;
			if (num <= 0f)
			{
				if (num2 > 0f)
				{
					currentVibrations[i] = 0f;
					_StopVibrating((PlayerId)i);
				}
			}
			else if (num2 <= 0f)
			{
				num = 0f;
				_StopVibrating((PlayerId)i);
			}
			else
			{
				durationsLeft[i] = num;
			}
		}
	}

	private void _Vibrate(float amount, float time, PlayerId playerId)
	{
		if (!SettingsData.Data.canVibrate)
		{
			return;
		}
		if (amount <= 0f || time <= 0f)
		{
			_StopVibrating(playerId);
			return;
		}
		currentVibrations[(int)playerId] = amount;
		durationsLeft[(int)playerId] = time;
		Player player = ReInput.players.GetPlayer((int)playerId);
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			if (joystick.supportsVibration)
			{
				joystick.SetVibration(amount * PlatformMultiplier, amount * PlatformMultiplier);
			}
		}
	}

	private void _StopVibrating(PlayerId playerId)
	{
		if (!SettingsData.Data.canVibrate)
		{
			return;
		}
		Player player = ReInput.players.GetPlayer((int)playerId);
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			joystick.StopVibration();
		}
	}
}
