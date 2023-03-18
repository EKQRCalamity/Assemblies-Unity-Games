using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.UI;
using Gameplay.UI.Others.UIGameLogic;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class FervourPenance : Ability
{
	public static Core.SimpleEvent OnPenanceStart;

	private Player _rewired;

	[FoldoutGroup("Ability Settings", true, 0)]
	public float BasePurgeConsumption;

	[FoldoutGroup("Ability Settings", true, 0)]
	public float PurgeCostScaleByLevel;

	[FoldoutGroup("Ability Settings", true, 0)]
	public float MinLifeConsumption;

	[FoldoutGroup("Ability Settings", true, 0)]
	public float FervourRestored;

	[FoldoutGroup("Event Fired", true, 0)]
	public string EventFired;

	[FoldoutGroup("Audio", true, 0)]
	[EventRef]
	public string FervourPenanceFx;

	private readonly int _fervourPenanceAnim = UnityEngine.Animator.StringToHash("FervourPenance");

	private bool _releaseTriggerButton;

	private bool _disabledAbilities;

	private bool CanDoPenanceForFervour
	{
		get
		{
			float num = ((!Core.PenitenceManager.UseStocksOfHealth) ? MinLifeConsumption : 30f);
			bool flag = base.EntityOwner.Stats.Purge.Current > GetPurgeCost();
			bool flag2 = base.EntityOwner.CurrentLife > num;
			return flag && flag2;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		_releaseTriggerButton = true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		_rewired = Core.Logic.Penitent.PlatformCharacterInput.Rewired;
		if (_rewired == null)
		{
			return;
		}
		if (_rewired.GetButtonUp("Range Attack"))
		{
			_releaseTriggerButton = true;
		}
		if (!base.Casting && _releaseTriggerButton && _disabledAbilities)
		{
			_disabledAbilities = false;
			Core.Logic.Penitent.RangeAttack.enabled = true;
		}
		if (base.Casting && !base.EntityOwner.Animator.GetCurrentAnimatorStateInfo(0).IsName("FervourPenance"))
		{
			StopCast();
		}
		if (_rewired.GetButtonShortPress("Range Attack") && !base.Casting && _releaseTriggerButton)
		{
			_releaseTriggerButton = false;
			Core.Logic.Penitent.RangeAttack.enabled = false;
			_disabledAbilities = true;
			if (!Core.Input.InputBlocked && !UIController.instance.IsShowingMenu && base.EntityOwner.Status.IsGrounded && CanDoPenanceForFervour)
			{
				Cast();
			}
		}
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		Core.Events.LaunchEvent(EventFired, string.Empty);
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		DrainResources();
		RestoreFervour(FervourRestored);
		base.EntityOwner.Animator.Play(_fervourPenanceAnim);
		Core.Audio.PlaySfx(FervourPenanceFx);
		if (OnPenanceStart != null)
		{
			OnPenanceStart();
		}
	}

	protected override void OnCastEnd(float castingTime)
	{
		base.OnCastEnd(castingTime);
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
	}

	private float GetPurgeCost()
	{
		float final = base.EntityOwner.Stats.MeaCulpa.Final;
		return BasePurgeConsumption + BasePurgeConsumption * final * PurgeCostScaleByLevel;
	}

	private void DrainResources()
	{
		base.EntityOwner.Stats.Purge.Current -= GetPurgeCost();
		float num = ((!Core.PenitenceManager.UseStocksOfHealth) ? MinLifeConsumption : PlayerHealthPE02.StocksDamage);
		base.EntityOwner.CurrentLife -= num;
		if (Core.PenitenceManager.UseStocksOfHealth)
		{
			Core.PenitenceManager.ResetRegeneration();
		}
	}

	private void RestoreFervour(float fervour)
	{
		float current = Mathf.Clamp(base.EntityOwner.Stats.Fervour.Current + fervour, base.EntityOwner.Stats.Fervour.Current, base.EntityOwner.Stats.Fervour.MaxValue);
		base.EntityOwner.Stats.Fervour.Current = current;
		PlayerFervour.Instance.ShowSpark();
	}
}
