using System.Collections;
using UnityEngine;

public class PlayerStatsManager : AbstractPlayerComponent
{
	public class DeathEvent : PlayerEvent<DeathEvent>
	{
	}

	public class ReviveEvent : PlayerEvent<ReviveEvent>
	{
	}

	public enum PlayerState
	{
		Ready,
		Super
	}

	public delegate void OnPlayerHealthChangeHandler(int health, PlayerId playerId);

	public delegate void OnPlayerSuperChangedHandler(float super, PlayerId playerId, bool playEffect);

	public delegate void OnPlayerWeaponChangedHandler(Weapon weapon);

	public delegate void OnPlayerDeathHandler(PlayerId playerId);

	public delegate void OnStoneHandler();

	public const int HEALTH_MAX = 3;

	public const int HEALTH_TRUE_MAX = 9;

	private const float TIME_HIT = 2f;

	private const float TIME_REVIVED = 3f;

	private const int SUPER_MAX = 50;

	private const float SUPER_ON_PARRY = 10f;

	private const float SUPER_ON_DEAL_DAMAGE = 0.0625f;

	private const float EX_COST = 10f;

	private const int HEALER_HP_MAX = 3;

	public bool isChalice;

	private int _curseCharmLevel;

	private int curseCharmDashCounter;

	private int curseCharmWhetstoneCounter;

	private float timeSinceStoned = 1000f;

	private float timeSinceReversed = 1000f;

	private bool hardInvincibility;

	private IEnumerator superBuilderRoutine;

	private const float STONE_REDUCTION = 0.1f;

	private Trilean2 lastMoveDir;

	private Trilean2 currentMoveDir;

	public static bool GlobalInvincibility { get; private set; }

	public int HealthMax { get; private set; }

	public int Health { get; private set; }

	public int HealerHP { get; private set; }

	public int HealerHPReceived { get; private set; }

	public int HealerHPCounter { get; private set; }

	public int CurseCharmLevel
	{
		get
		{
			return _curseCharmLevel;
		}
		private set
		{
			_curseCharmLevel = value;
		}
	}

	public bool CurseSmokeDash => Loadout.charm == Charm.charm_curse && CurseCharmLevel >= 0 && curseCharmDashCounter == 0;

	public bool CurseWhetsone => Loadout.charm == Charm.charm_curse && CurseCharmLevel >= 0 && curseCharmWhetstoneCounter == 0;

	public float SuperMeterMax { get; private set; }

	public float SuperMeter { get; private set; }

	public bool SuperInvincible { get; private set; }

	public bool ChaliceShieldOn { get; private set; }

	public bool CanGainSuperMeter => Loadout.charm != Charm.charm_EX && (!SuperInvincible || ChaliceShieldOn);

	public float ExCost { get; private set; }

	public int Deaths { get; private set; }

	public int ParriesThisJump { get; private set; }

	public float StoneTime { get; private set; }

	public float ReverseTime { get; private set; }

	public bool CanUseEx => Loadout.charm == Charm.charm_EX || (SuperMeter >= ExCost && CanGainSuperMeter);

	public PlayerData.PlayerLoadouts.PlayerLoadout Loadout { get; private set; }

	public PlayerState State { get; private set; }

	public bool DiceGameBonusHP { get; set; }

	public bool PartnerCanSteal => Health > 1;

	public static bool DebugInvincible { get; private set; }

	public event OnPlayerHealthChangeHandler OnHealthChangedEvent;

	public event OnPlayerSuperChangedHandler OnSuperChangedEvent;

	public event OnPlayerWeaponChangedHandler OnWeaponChangedEvent;

	public event OnPlayerDeathHandler OnPlayerDeathEvent;

	public event OnPlayerDeathHandler OnPlayerReviveEvent;

	public event OnStoneHandler OnStoneShake;

	public event OnStoneHandler OnStoned;

	protected override void OnAwake()
	{
		base.OnAwake();
		GlobalInvincibility = false;
		DebugInvincible = false;
		SuperInvincible = false;
		ChaliceShieldOn = false;
		base.basePlayer.damageReceiver.OnDamageTaken += OnDamageTaken;
		LevelPlayerController levelPlayerController = base.basePlayer as LevelPlayerController;
		PlanePlayerController planePlayerController = base.basePlayer as PlanePlayerController;
		if (levelPlayerController != null)
		{
			levelPlayerController.motor.OnDashStartEvent += onDashStartEventHandler;
			levelPlayerController.motor.OnParryEvent += onParryEventHandler;
		}
		else if (planePlayerController != null)
		{
			planePlayerController.animationController.OnShrinkEvent += onShrinkEventHandler;
			planePlayerController.parryController.OnParryStartEvent += onParryEventHandler;
		}
		LevelPlayerWeaponManager component = GetComponent<LevelPlayerWeaponManager>();
		if (component != null)
		{
			component.OnWeaponChangeEvent += OnWeaponChange;
			component.OnSuperEnd += OnSuperEnd;
		}
		PlanePlayerWeaponManager component2 = GetComponent<PlanePlayerWeaponManager>();
		if (component2 != null)
		{
			component2.OnWeaponChangeEvent += OnWeaponChange;
		}
		Deaths = 0;
		hardInvincibility = false;
	}

	private void OnEnable()
	{
		if (superBuilderRoutine != null)
		{
			StopCoroutine(superBuilderRoutine);
		}
		superBuilderRoutine = charmSuperBuilder_cr();
		StartCoroutine(superBuilderRoutine);
	}

	private void FixedUpdate()
	{
		UpdateStone();
		UpdateReverse();
	}

	public void LevelInit()
	{
		Level.Current.OnWinEvent += OnWin;
		Level.Current.OnLoseEvent += OnLose;
		Loadout = PlayerData.Data.Loadouts.GetPlayerLoadout(base.basePlayer.id);
		isChalice = Loadout.charm == Charm.charm_chalice && !Level.Current.BlockChaliceCharm[(int)base.basePlayer.id];
		if (!Level.Current.blockChalice && (PlayerManager.playerWasChalice[0] || PlayerManager.playerWasChalice[1]))
		{
			isChalice = PlayerManager.playerWasChalice[(int)base.basePlayer.id];
		}
		if (Level.IsDicePalace && !DicePalaceMainLevelGameInfo.IS_FIRST_ENTRY)
		{
			isChalice = base.basePlayer.id == (PlayerId)DicePalaceMainLevelGameInfo.CHALICE_PLAYER;
		}
		if (Loadout.charm == Charm.charm_curse)
		{
			CurseCharmLevel = CharmCurse.CalculateLevel(base.basePlayer.id);
		}
		ExCost = 10f;
		SuperMeterMax = 50f;
		CalculateHealthMax();
		PlayersStatsBossesHub playerStats = Level.GetPlayerStats(base.basePlayer.id);
		if (Level.IsInBossesHub && playerStats != null)
		{
			Health = playerStats.HP;
			SuperMeter = playerStats.SuperCharge;
			HealerHP = playerStats.healerHP;
			HealerHPReceived = playerStats.healerHPReceived;
			HealerHPCounter = playerStats.healerHPCounter;
		}
		else
		{
			Health = HealthMax;
			SuperMeter = 0f;
		}
		if (Health >= OnlineAchievementData.DLC.Triggers.HP9Trigger)
		{
			OnlineManager.Instance.Interface.UnlockAchievement(base.basePlayer.id, OnlineAchievementData.DLC.HP9);
		}
		UpdateHealerStats();
		if (isChalice)
		{
			if (Loadout.super == Super.level_super_beam)
			{
				Loadout.super = Super.level_super_chalice_vert_beam;
			}
			else if (Loadout.super == Super.level_super_invincible)
			{
				Loadout.super = Super.level_super_chalice_shield;
			}
			else if (Loadout.super == Super.level_super_ghost)
			{
				Loadout.super = Super.level_super_chalice_iii;
			}
		}
		else if (Loadout.super == Super.level_super_chalice_vert_beam)
		{
			Loadout.super = Super.level_super_beam;
		}
		else if (Loadout.super == Super.level_super_chalice_shield)
		{
			Loadout.super = Super.level_super_invincible;
		}
		else if (Loadout.super == Super.level_super_chalice_iii)
		{
			Loadout.super = Super.level_super_ghost;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Level.Current != null)
		{
			Level.Current.OnWinEvent -= OnWin;
			Level.Current.OnLoseEvent -= OnLose;
		}
		if (Loadout != null)
		{
			if (Loadout.super == Super.level_super_chalice_vert_beam)
			{
				Loadout.super = Super.level_super_beam;
			}
			else if (Loadout.super == Super.level_super_chalice_shield)
			{
				Loadout.super = Super.level_super_invincible;
			}
			else if (Loadout.super == Super.level_super_chalice_iii)
			{
				Loadout.super = Super.level_super_ghost;
			}
		}
	}

	public void UpdateHealerStats()
	{
		if ((Loadout.charm == Charm.charm_healer || (Loadout.charm == Charm.charm_curse && CurseCharmLevel >= 0)) && !Level.IsChessBoss)
		{
			PlayersStatsBossesHub playerStats = Level.GetPlayerStats(base.basePlayer.id);
			if (playerStats != null)
			{
				HealthMax += playerStats.healerHP;
			}
		}
	}

	private bool DjimmiInUse()
	{
		return PlayerData.Data.DjimmiActivatedCurrentRegion() && Level.Current.AllowDjimmi() && Level.Current.mode != Level.Mode.Hard;
	}

	private void CalculateHealthMax()
	{
		HealthMax = 3;
		if (Loadout.charm == Charm.charm_health_up_1 && !Level.IsChessBoss)
		{
			HealthMax += WeaponProperties.CharmHealthUpOne.healthIncrease;
		}
		else if (Loadout.charm == Charm.charm_health_up_2 && !Level.IsChessBoss)
		{
			HealthMax += WeaponProperties.CharmHealthUpTwo.healthIncrease;
		}
		else if (Loadout.charm == Charm.charm_healer && !Level.IsChessBoss)
		{
			HealthMax += HealerHP;
		}
		else if (Loadout.charm == Charm.charm_curse && CurseCharmLevel >= 0 && !Level.IsChessBoss)
		{
			HealthMax += HealerHP;
			HealthMax += CharmCurse.GetHealthModifier(CurseCharmLevel);
		}
		else if (isChalice)
		{
			HealthMax++;
		}
		if (DjimmiInUse())
		{
			HealthMax *= 2;
		}
		if (Level.IsInBossesHub)
		{
			PlayersStatsBossesHub playerStats = Level.GetPlayerStats(base.basePlayer.id);
			if (playerStats != null)
			{
				HealthMax += playerStats.BonusHP;
			}
		}
		if (HealthMax > 9)
		{
			HealthMax = 9;
		}
	}

	private void OnWin()
	{
		GlobalInvincibility = true;
	}

	private void OnLose()
	{
		GlobalInvincibility = true;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (SuperInvincible || ChaliceShieldOn)
		{
			return;
		}
		if (Loadout.charm == Charm.charm_pit_saver && !Level.IsChessBoss)
		{
			if (info.damageSource == DamageDealer.DamageSource.Pit)
			{
				return;
			}
			SuperMeter += WeaponProperties.CharmPitSaver.meterAmount;
			OnSuperChanged();
		}
		if (info.stoneTime > 0f)
		{
			GetStoned(info.stoneTime);
		}
		if (info.damage > 0f)
		{
			TakeDamage();
		}
	}

	public void GetStoned(float time)
	{
		if (time > 0f && StoneTime <= 0f && timeSinceStoned > 1f)
		{
			StoneTime = time;
			timeSinceStoned = 0f;
			this.OnStoned();
		}
	}

	public void ReverseControls(float reverseTime)
	{
		if (timeSinceReversed > 0f && ReverseTime <= 0f && timeSinceReversed > 1f)
		{
			ReverseTime = reverseTime;
			timeSinceReversed = 0f;
		}
	}

	private void TakeDamage()
	{
		if (SuperInvincible || hardInvincibility || Level.Current.Ending || (State != 0 && (!isChalice || Loadout.super != Super.level_super_ghost)))
		{
			return;
		}
		if (StoneTime > 0f)
		{
			StoneTime = 0f;
		}
		if (GlobalInvincibility || DebugInvincible)
		{
			return;
		}
		Health--;
		PlayersStatsBossesHub playerStats = Level.GetPlayerStats(base.basePlayer.id);
		if (Level.IsInBossesHub && playerStats != null)
		{
			if (playerStats.BonusHP > 0)
			{
				playerStats.LoseBonusHP();
			}
			else if (playerStats.healerHP > 0)
			{
				playerStats.LoseHealerHP();
			}
			CalculateHealthMax();
		}
		OnHealthChanged();
		if (Health < 3)
		{
			Level.ScoringData.numTimesHit++;
		}
		Vibrator.Vibrate(1f, 0.2f, base.basePlayer.id);
		if (Health <= 0)
		{
			OnStatsDeath();
		}
		else
		{
			StartCoroutine(hit_cr());
		}
	}

	public void OnPitKnockUp()
	{
		base.basePlayer.damageReceiver.TakeDamage(new DamageDealer.DamageInfo(1f, DamageDealer.Direction.Neutral, base.transform.position, DamageDealer.DamageSource.Pit));
	}

	public void OnDealDamage(float damage, DamageDealer dealer)
	{
		if (CanGainSuperMeter)
		{
			SuperMeter += 0.0625f * damage / dealer.DamageMultiplier;
			OnSuperChanged(playEffect: false);
		}
	}

	public void OnParry(float multiplier = 1f, bool countParryTowardsScore = true)
	{
		if ((Loadout.charm == Charm.charm_healer || (Loadout.charm == Charm.charm_curse && CurseCharmLevel >= 0)) && !Level.IsChessBoss)
		{
			if (HealerHPReceived < 3)
			{
				HealerCharm();
			}
			else
			{
				SuperChangedFromParry(multiplier);
			}
		}
		else
		{
			SuperChangedFromParry(multiplier);
		}
		if (countParryTowardsScore && !Level.Current.Ending)
		{
			Level.ScoringData.numParries++;
		}
		OnlineManager.Instance.Interface.IncrementStat(base.basePlayer.id, "Parries", 1);
		if (Level.Current.CurrentLevel != 0 && Level.Current.CurrentLevel != Levels.ShmupTutorial && (Level.Current.playerMode == PlayerMode.Level || Level.Current.playerMode == PlayerMode.Arcade))
		{
			ParriesThisJump++;
			if (ParriesThisJump > PlayerData.Data.GetNumParriesInRow(base.basePlayer.id))
			{
				PlayerData.Data.SetNumParriesInRow(base.basePlayer.id, ParriesThisJump);
			}
			if (ParriesThisJump == 5)
			{
				OnlineManager.Instance.Interface.UnlockAchievement(base.basePlayer.id, "ParryChain");
			}
		}
		if (SuperMeter == SuperMeterMax)
		{
			AudioManager.Play("player_parry_power_up_full");
		}
		else
		{
			AudioManager.Play("player_parry_power_up");
		}
	}

	private void SuperChangedFromParry(float multiplier)
	{
		if (CanGainSuperMeter)
		{
			SuperMeter += 10f * multiplier;
			OnSuperChanged();
		}
	}

	public bool NextParryActivatesHealerCharm()
	{
		if (Level.IsChessBoss)
		{
			return false;
		}
		if (Loadout.charm != Charm.charm_healer)
		{
			return false;
		}
		return HealerHPReceived == HealerHPCounter;
	}

	private void HealerCharm()
	{
		int num = HealerHPReceived + 1;
		if (Loadout.charm == Charm.charm_curse)
		{
			num = CharmCurse.GetHealerInterval(CurseCharmLevel, HealerHPReceived);
		}
		HealerHPCounter++;
		if (HealerHPCounter >= num)
		{
			HealerHP++;
			HealerHPReceived++;
			SetHealth(Health + 1);
			OnHealthChanged();
			HealerHPCounter = 0;
			LevelPlayerController levelPlayerController = base.basePlayer as LevelPlayerController;
			PlanePlayerController planePlayerController = base.basePlayer as PlanePlayerController;
			if (levelPlayerController != null)
			{
				levelPlayerController.animationController.OnHealerCharm();
			}
			else if (planePlayerController != null)
			{
				planePlayerController.animationController.OnHealerCharm();
			}
		}
	}

	public void ParryOneQuarter()
	{
		OnParry(0.25f);
	}

	public void ResetJumpParries()
	{
		ParriesThisJump = 0;
	}

	public void OnPartnerStealHealth()
	{
		if (PartnerCanSteal)
		{
			Health--;
			PlayersStatsBossesHub playerStats = Level.GetPlayerStats(base.basePlayer.id);
			if (Level.IsInBossesHub && playerStats != null)
			{
				playerStats.LoseBonusHP();
				CalculateHealthMax();
			}
			OnHealthChanged();
		}
	}

	public void OnSuper()
	{
		if (Loadout.super != Super.level_super_invincible || Level.Current.playerMode != 0)
		{
			SuperMeter = 0f;
			OnSuperChanged();
		}
		State = PlayerState.Super;
	}

	public void OnSuperEnd()
	{
		if (Loadout.super == Super.level_super_invincible && Level.Current.playerMode == PlayerMode.Level)
		{
			StartCoroutine(emptySuper_cr());
		}
		State = PlayerState.Ready;
	}

	public void OnEx()
	{
		if (Loadout.charm != Charm.charm_EX)
		{
			SuperMeter -= 10f;
			OnSuperChanged();
		}
	}

	private void OnWeaponChange(Weapon weapon)
	{
		if (this.OnWeaponChangedEvent != null)
		{
			this.OnWeaponChangedEvent(weapon);
		}
	}

	private void OnHealthChanged()
	{
		Health = Mathf.Clamp(Health, 0, HealthMax);
		if (this.OnHealthChangedEvent != null)
		{
			this.OnHealthChangedEvent(Health, base.basePlayer.id);
		}
	}

	private void OnSuperChanged(bool playEffect = true)
	{
		SuperMeter = Mathf.Clamp(SuperMeter, 0f, SuperMeterMax);
		if (this.OnSuperChangedEvent != null)
		{
			this.OnSuperChangedEvent(SuperMeter, base.basePlayer.id, playEffect);
		}
	}

	private void OnStatsDeath()
	{
		AudioManager.Play("player_die");
		StartCoroutine(death_sound_cr());
		if (this.OnPlayerDeathEvent != null)
		{
			this.OnPlayerDeathEvent(base.basePlayer.id);
		}
		EventManager.Instance.Raise(PlayerEvent<DeathEvent>.Shared(base.basePlayer.id));
		Deaths++;
		PlayerData.Data.Die(base.basePlayer.id);
	}

	public void OnPreRevive()
	{
		if (!Level.IsTowerOfPowerMain || TowerOfPowerLevelGameInfo.CURRENT_TURN > 0)
		{
			Health = 1;
		}
	}

	public void OnRevive()
	{
		OnHealthChanged();
		if (this.OnPlayerReviveEvent != null)
		{
			this.OnPlayerReviveEvent(base.basePlayer.id);
		}
		EventManager.Instance.Raise(PlayerEvent<ReviveEvent>.Shared(base.basePlayer.id));
	}

	public void SetHealth(int health)
	{
		Health = health;
		CalculateHealthMax();
		OnHealthChanged();
		if (health >= OnlineAchievementData.DLC.Triggers.HP9Trigger)
		{
			OnlineManager.Instance.Interface.UnlockAchievement(base.basePlayer.id, OnlineAchievementData.DLC.HP9);
		}
	}

	public void SetInvincible(bool superInvincible)
	{
		SuperInvincible = superInvincible;
	}

	public void SetChaliceShield(bool chaliceShield)
	{
		ChaliceShieldOn = chaliceShield;
	}

	public void AddEx()
	{
		if (CanGainSuperMeter)
		{
			SuperMeter += 10f;
			OnSuperChanged();
		}
	}

	private void onDashStartEventHandler()
	{
		if (Loadout.charm == Charm.charm_curse && CurseCharmLevel > -1)
		{
			curseCharmDashCounter++;
			if (curseCharmDashCounter > CharmCurse.GetSmokeDashInterval(CurseCharmLevel))
			{
				curseCharmDashCounter = 0;
			}
		}
	}

	private void onShrinkEventHandler()
	{
		if (Loadout.charm == Charm.charm_curse && CurseCharmLevel > -1)
		{
			curseCharmDashCounter++;
			if (curseCharmDashCounter > CharmCurse.GetSmokeDashInterval(CurseCharmLevel))
			{
				curseCharmDashCounter = 0;
			}
		}
	}

	private void onParryEventHandler()
	{
		if (Loadout.charm == Charm.charm_curse && CurseCharmLevel > -1)
		{
			curseCharmWhetstoneCounter++;
			if (curseCharmWhetstoneCounter > CharmCurse.GetWhetstoneInterval(CurseCharmLevel))
			{
				curseCharmWhetstoneCounter = 0;
			}
		}
	}

	private void UpdateStone()
	{
		PlanePlayerController planePlayerController = base.basePlayer as PlanePlayerController;
		if (planePlayerController != null)
		{
			currentMoveDir = planePlayerController.motor.MoveDirection;
		}
		lastMoveDir = currentMoveDir;
		timeSinceStoned += CupheadTime.FixedDelta;
		if (!(StoneTime <= 0f) && ((lastMoveDir != currentMoveDir && ((int)currentMoveDir.x != 0 || (int)currentMoveDir.y != 0)) || base.basePlayer.input.actions.GetAnyButtonDown()))
		{
			StoneTime -= CupheadTime.Delta;
			StoneTime -= 0.1f;
			this.OnStoneShake();
		}
	}

	private void UpdateReverse()
	{
		LevelPlayerController levelPlayerController = base.basePlayer as LevelPlayerController;
		if (!(levelPlayerController == null))
		{
			timeSinceReversed += CupheadTime.FixedDelta;
			if (!(ReverseTime <= 0f))
			{
				ReverseTime -= CupheadTime.Delta;
			}
		}
	}

	public override void StopAllCoroutines()
	{
	}

	private IEnumerator death_sound_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		AudioManager.Play("player_die_vinylscratch");
	}

	private IEnumerator hit_cr()
	{
		hardInvincibility = true;
		for (int i = 0; i < 10; i++)
		{
			yield return null;
		}
		hardInvincibility = false;
	}

	private IEnumerator charmSuperBuilder_cr()
	{
		while (Loadout == null)
		{
			yield return null;
		}
		if ((Loadout.charm != Charm.charm_super_builder && (Loadout.charm != Charm.charm_curse || CurseCharmLevel <= -1)) || Level.IsChessBoss)
		{
			yield break;
		}
		float delay = 0f;
		float amount = 0f;
		if (Loadout.charm == Charm.charm_super_builder)
		{
			delay = WeaponProperties.CharmSuperBuilder.delay;
			amount = WeaponProperties.CharmSuperBuilder.amount;
		}
		else if (Loadout.charm == Charm.charm_curse && CurseCharmLevel > -1)
		{
			delay = WeaponProperties.CharmCurse.superMeterDelay;
			amount = CharmCurse.GetSuperMeterAmount(CurseCharmLevel);
		}
		while (true)
		{
			yield return CupheadTime.WaitForSeconds(this, delay);
			if (CanGainSuperMeter)
			{
				SuperMeter += amount;
				OnSuperChanged(playEffect: false);
			}
		}
	}

	private IEnumerator emptySuper_cr()
	{
		while (SuperMeter > 0f)
		{
			SuperMeter -= SuperMeterMax * (float)CupheadTime.Delta / WeaponProperties.LevelSuperInvincibility.durationFX;
			OnSuperChanged();
			yield return null;
		}
		SuperMeter = 0f;
		OnSuperChanged();
	}

	public void DebugAddSuper()
	{
		AddEx();
	}

	public void DebugFillSuper()
	{
		SuperMeter = 50f;
		OnSuperChanged();
	}

	public static void DebugToggleInvincible()
	{
		DebugInvincible = !DebugInvincible;
		string text = ((!DebugInvincible) ? "red" : "green");
	}
}
