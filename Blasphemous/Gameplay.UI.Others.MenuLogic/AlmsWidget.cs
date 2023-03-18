using System.Collections;
using Framework.Managers;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class AlmsWidget : BasicUIBlockingWidget
{
	[BoxGroup("Controls", true, false, 0)]
	public Text NumberText;

	private Player Rewired;

	private float CurrentNumber;

	private float TimePressing;

	private float TimesLastSound = -1f;

	private const float MovementEpsilon = 0.1f;

	private bool WaitToSkinUI;

	private int NumberBeforePress;

	private int FactorBeforePress;

	private bool IsPressing;

	protected override void OnWidgetInitialize()
	{
		Rewired = ReInput.players.GetPlayer(0);
	}

	protected override void OnWidgetShow()
	{
		CurrentNumber = 0f;
		NumberText.text = "0";
		TimePressing = 0f;
		TimesLastSound = -1f;
		WaitToSkinUI = false;
		IsPressing = false;
	}

	public void SubmitPressed()
	{
		if (WaitToSkinUI)
		{
			return;
		}
		bool flag = false;
		int tears = (int)CurrentNumber;
		if (CurrentNumber > 0f && Core.Alms.CanConsumeTears(tears))
		{
			string id = Core.Alms.Config.SoundAddedOk;
			if (Core.Alms.ConsumeTears(tears))
			{
				id = Core.Alms.Config.SoundNewTier;
			}
			WaitToSkinUI = UIController.instance.IsUnlockActive();
			flag = true;
			Core.Audio.PlaySfx(id);
		}
		if (flag)
		{
			if (WaitToSkinUI)
			{
				StartCoroutine(WaitSkinAndClose());
			}
			else
			{
				FadeHide();
			}
		}
	}

	private IEnumerator WaitSkinAndClose()
	{
		while (WaitToSkinUI)
		{
			WaitToSkinUI = UIController.instance.IsUnlockActive();
			yield return 0;
		}
		FadeHide();
	}

	private void Update()
	{
		if (Rewired == null || WaitToSkinUI)
		{
			return;
		}
		float currentNumber = CurrentNumber;
		float axisRaw = Rewired.GetAxisRaw(49);
		if (Mathf.Abs(axisRaw) >= 0.1f)
		{
			if (!IsPressing)
			{
				NumberBeforePress = (int)CurrentNumber;
				FactorBeforePress = ((!(axisRaw < 0f)) ? 1 : (-1));
				IsPressing = true;
			}
			TimePressing += Time.unscaledDeltaTime;
			float num = axisRaw * Core.Alms.Config.NumberSpeed * Core.Alms.Config.NumberFactorByTime.Evaluate(TimePressing) * Time.unscaledDeltaTime;
			CurrentNumber += num;
			CheckCurrent();
			float num2 = Core.Alms.Config.SoundChangeUpdate.Evaluate(TimePressing);
			if (currentNumber != CurrentNumber && (TimesLastSound < 0f || TimesLastSound >= num2))
			{
				Core.Audio.PlayOneShot(Core.Alms.Config.SoundChange);
				TimesLastSound = 0f;
			}
			else
			{
				TimesLastSound += Time.unscaledDeltaTime;
			}
		}
		else
		{
			if (IsPressing && NumberBeforePress == (int)CurrentNumber)
			{
				CurrentNumber += FactorBeforePress;
				CheckCurrent();
			}
			IsPressing = false;
			TimePressing = 0f;
			TimesLastSound = -1f;
		}
		if (currentNumber != CurrentNumber)
		{
			NumberText.text = ((int)CurrentNumber).ToString();
		}
	}

	private void CheckCurrent()
	{
		if (CurrentNumber < 0f)
		{
			CurrentNumber = 0f;
		}
		else if (CurrentNumber > (float)Core.Alms.Config.MaxNumber)
		{
			CurrentNumber = Core.Alms.Config.MaxNumber;
		}
		if (!Core.Alms.CanConsumeTears((int)CurrentNumber))
		{
			CurrentNumber = Core.Logic.Penitent.Stats.Purge.Current;
		}
	}
}
