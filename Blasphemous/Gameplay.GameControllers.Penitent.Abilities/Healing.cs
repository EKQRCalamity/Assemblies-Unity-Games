using CreativeSpore.SmartColliders;
using Framework.FrameworkCore;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Effects.Player.Healing;
using Gameplay.GameControllers.Entities;
using Gameplay.UI;
using Gameplay.UI.Others.UIGameLogic;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Abilities;

public class Healing : Ability
{
	public static Core.SimpleEvent OnHealingStart;

	private readonly int _healingAnimHash = UnityEngine.Animator.StringToHash("Healing");

	public GameObject HealingAura;

	private HealingAura _healingAura;

	private EntityStats _stats;

	private Sword swordHeart06;

	public bool IsHealing { get; private set; }

	public CameraPan CameraPan { get; private set; }

	public bool InvincibleEffect { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		_stats = base.EntityOwner.Stats;
		if (HealingAura == null)
		{
			Debug.LogError("You need a Healing Aura Prefab");
		}
		else
		{
			_healingAura = Object.Instantiate(HealingAura).GetComponent<HealingAura>();
		}
		base.EntityOwner.OnDeath += EntityOwnerOnEntityDie;
		base.EntityOwner.OnDamaged += OnOwnerDamaged;
		CameraPan = Object.FindObjectOfType<CameraPan>();
		swordHeart06 = Core.InventoryManager.GetSword("HE06");
	}

	private void LateUpdate()
	{
		if (_healingAura != null)
		{
			_healingAura.transform.position = base.EntityOwner.Animator.transform.position;
		}
		if (Core.Input.InputBlocked || UIController.instance.IsShowingMenu || !GetHealingInput())
		{
			return;
		}
		bool flag = base.Animator.GetCurrentAnimatorStateInfo(0).IsName("HardLanding");
		if (!IsHealing && !flag && base.EntityOwner.Status.IsGrounded && !base.EntityOwner.Status.IsHurt && !base.EntityOwner.Status.Dead && (!(swordHeart06 != null) || !swordHeart06.IsEquiped))
		{
			if (_stats.Flask.Current < 1f)
			{
				Core.Logic.Penitent.Audio.EmptyFlask();
			}
			else
			{
				Cast();
			}
		}
	}

	private bool GetHealingInput()
	{
		bool result = false;
		if (CameraPan != null)
		{
			result = base.Rewired.GetButtonDown(23) && !Core.Logic.Penitent.PlatformCharacterController.GetActionState(eControllerActions.Jump);
		}
		return result;
	}

	protected override void OnCastStart()
	{
		base.OnCastStart();
		Heal();
		base.Animator.Play(_healingAnimHash);
		IsHealing = true;
		_healingAura.StartAura(base.EntityOwner.Status.Orientation);
		Core.Logic.Penitent.Audio.UseFlask();
		RosaryBead rosaryBead = Core.InventoryManager.GetRosaryBead("RB28");
		if (rosaryBead != null && rosaryBead.IsEquiped)
		{
			base.EntityOwner.Status.Invulnerable = true;
			InvincibleEffect = true;
		}
		if (OnHealingStart != null)
		{
			OnHealingStart();
		}
	}

	private void Heal()
	{
		if (!(_stats.Flask.Current < 1f))
		{
			_stats.Flask.Current -= 1f;
			if (Core.PenitenceManager.UseFervourFlasks)
			{
				_stats.Fervour.Current += _stats.FlaskHealth.Final;
				PlayerFervour.Instance.ShowSpark();
			}
			else if (Core.PenitenceManager.UseStocksOfHealth)
			{
				_stats.Life.Current += PlayerHealthPE02.StocksHeal;
				Core.PenitenceManager.ResetRegeneration();
			}
			else
			{
				_stats.Life.Current += _stats.FlaskHealth.Final;
			}
		}
	}

	public void StopHeal()
	{
		if (IsHealing)
		{
			IsHealing = false;
			_healingAura.StopAura();
			Core.Logic.Penitent.Audio.StopUseFlask();
			StopCast();
			if (InvincibleEffect)
			{
				base.EntityOwner.Status.Invulnerable = false;
				InvincibleEffect = false;
			}
		}
	}

	private void OnOwnerDamaged()
	{
		if (IsHealing)
		{
			base.Animator.SetTrigger("HURT");
			StopHeal();
		}
	}

	private void EntityOwnerOnEntityDie()
	{
		if (!(_healingAura == null))
		{
			Object.Destroy(_healingAura.gameObject);
			base.enabled = false;
		}
	}

	private void OnDestroy()
	{
		if ((bool)base.EntityOwner)
		{
			base.EntityOwner.OnDeath -= EntityOwnerOnEntityDie;
			base.EntityOwner.OnDamaged -= OnOwnerDamaged;
		}
	}
}
