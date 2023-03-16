using UnityEngine;

public class PlanePlayerController : AbstractPlayerController
{
	public const float INTRO_TIME = 1f;

	private PlanePlayerMotor _motor;

	private PlanePlayerAnimationController _animationController;

	private PlanePlayerAudioController _audioController;

	private PlanePlayerWeaponManager _weaponManager;

	private PlanePlayerParryController _parryController;

	[SerializeField]
	private PlayerDeathEffect deathEffect;

	public bool Shrunk => animationController.ShrinkState == PlanePlayerAnimationController.ShrinkStates.Shrunk;

	public bool Parrying => parryController.State == PlanePlayerParryController.ParryState.Parrying;

	public bool WeaponBusy => weaponManager.state != PlanePlayerWeaponManager.State.Ready || !weaponManager.CanInterupt;

	public PlanePlayerMotor motor
	{
		get
		{
			if (_motor == null)
			{
				_motor = GetComponent<PlanePlayerMotor>();
			}
			return _motor;
		}
	}

	public PlanePlayerAnimationController animationController
	{
		get
		{
			if (_animationController == null)
			{
				_animationController = GetComponent<PlanePlayerAnimationController>();
			}
			return _animationController;
		}
	}

	public PlanePlayerAudioController audioController
	{
		get
		{
			if (_audioController == null)
			{
				_audioController = GetComponent<PlanePlayerAudioController>();
			}
			return _audioController;
		}
	}

	public PlanePlayerWeaponManager weaponManager
	{
		get
		{
			if (_weaponManager == null)
			{
				_weaponManager = GetComponent<PlanePlayerWeaponManager>();
			}
			return _weaponManager;
		}
	}

	public PlanePlayerParryController parryController
	{
		get
		{
			if (_parryController == null)
			{
				_parryController = GetComponent<PlanePlayerParryController>();
			}
			return _parryController;
		}
	}

	public override bool CanTakeDamage
	{
		get
		{
			if (base.damageReceiver.state != 0)
			{
				return false;
			}
			if ((base.stats.Loadout.charm == Charm.charm_smoke_dash || base.stats.CurseSmokeDash) && animationController.Shrinking)
			{
				return false;
			}
			return true;
		}
	}

	private void Start()
	{
		if (!Level.Current.Started)
		{
			motor.enabled = false;
		}
	}

	public override void PlayIntro()
	{
		base.PlayIntro();
		animationController.PlayIntro();
	}

	protected override void LevelInit(PlayerId id)
	{
		base.LevelInit(id);
		animationController.LevelInit();
		audioController.LevelInit();
		if (base.stats.Health == 0)
		{
			StartDead();
		}
	}

	public override void LevelStart()
	{
		base.LevelStart();
		motor.enabled = true;
	}

	public void GetStoned(float stoneTime)
	{
		base.stats.GetStoned(stoneTime);
	}

	protected override void OnDeath(PlayerId playerId)
	{
		base.OnDeath(base.id);
		PlayerDeathEffect playerDeathEffect = deathEffect.Create(base.id, base.input, base.transform.position, base.stats.Deaths, PlayerMode.Plane, canParry: true);
		playerDeathEffect.OnPreReviveEvent += OnPreRevive;
		playerDeathEffect.OnReviveEvent += OnRevive;
		if (PauseManager.state == PauseManager.State.Paused)
		{
			PauseManager.Unpause();
		}
	}

	public override void OnLeave(PlayerId playerId)
	{
		if (!base.IsDead)
		{
			deathEffect.CreateExplosionOnly(base.id, base.transform.position, PlayerMode.Plane);
		}
		base.OnLeave(playerId);
	}

	private void StartDead()
	{
		base.gameObject.SetActive(value: false);
		Vector3 position = base.transform.position;
		position.y += 1000f;
		PlayerDeathEffect playerDeathEffect = deathEffect.Create(base.id, base.input, position, base.stats.Deaths, PlayerMode.Plane, canParry: true);
		playerDeathEffect.OnPreReviveEvent += OnPreRevive;
		playerDeathEffect.OnReviveEvent += OnRevive;
	}

	public void PauseAll()
	{
		AbstractPausableComponent[] components = GetComponents<AbstractPausableComponent>();
		foreach (AbstractPausableComponent abstractPausableComponent in components)
		{
			abstractPausableComponent.enabled = false;
		}
	}

	public void UnpauseAll(bool forced = false)
	{
		AbstractPausableComponent[] components = GetComponents<AbstractPausableComponent>();
		foreach (AbstractPausableComponent abstractPausableComponent in components)
		{
			if (forced)
			{
				abstractPausableComponent.preEnabled = true;
			}
			abstractPausableComponent.enabled = true;
		}
	}

	public void SetSpriteVisible(bool visibility)
	{
		animationController.SetSpriteVisible(visibility);
	}

	public override void BufferInputs()
	{
		base.BufferInputs();
		motor.BufferInputs();
	}
}
