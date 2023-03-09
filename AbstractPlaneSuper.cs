using UnityEngine;

public abstract class AbstractPlaneSuper : AbstractCollidableObject
{
	protected PlanePlayerWeaponManager.States.Super state = PlanePlayerWeaponManager.States.Super.Intro;

	[SerializeField]
	[Header("Player Sprites")]
	private SpriteRenderer cuphead;

	[SerializeField]
	private SpriteRenderer mugman;

	[SerializeField]
	protected SpriteRenderer chalice;

	protected SpriteRenderer spriteRenderer;

	protected PlanePlayerController player;

	protected DamageDealer damageDealer;

	protected AnimationHelper animHelper;

	public PlanePlayerWeaponManager.States.Super State => state;

	protected override bool allowCollisionPlayer => false;

	protected override void Awake()
	{
		base.tag = "PlayerProjectile";
		base.Awake();
	}

	protected virtual void Start()
	{
		animHelper = GetComponent<AnimationHelper>();
		base.transform.position = player.transform.position;
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		if (damageDealer != null)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionEnemy(hit, phase);
	}

	public AbstractPlaneSuper Create(PlanePlayerController player)
	{
		AbstractPlaneSuper abstractPlaneSuper = InstantiatePrefab<AbstractPlaneSuper>();
		abstractPlaneSuper.player = player;
		if (player.stats.isChalice)
		{
			abstractPlaneSuper.spriteRenderer = chalice;
			abstractPlaneSuper.chalice.gameObject.SetActive(value: true);
			if ((bool)abstractPlaneSuper.cuphead)
			{
				abstractPlaneSuper.cuphead.gameObject.SetActive(value: false);
			}
			if ((bool)abstractPlaneSuper.mugman)
			{
				abstractPlaneSuper.mugman.gameObject.SetActive(value: false);
			}
		}
		else
		{
			PlayerId id = player.id;
			if (id == PlayerId.PlayerOne || id != PlayerId.PlayerTwo)
			{
				abstractPlaneSuper.spriteRenderer = ((!PlayerManager.player1IsMugman) ? cuphead : mugman);
				abstractPlaneSuper.cuphead.gameObject.SetActive(!PlayerManager.player1IsMugman);
				abstractPlaneSuper.mugman.gameObject.SetActive(PlayerManager.player1IsMugman);
			}
			else
			{
				abstractPlaneSuper.spriteRenderer = ((!PlayerManager.player1IsMugman) ? mugman : cuphead);
				abstractPlaneSuper.cuphead.gameObject.SetActive(PlayerManager.player1IsMugman);
				abstractPlaneSuper.mugman.gameObject.SetActive(!PlayerManager.player1IsMugman);
			}
		}
		abstractPlaneSuper.StartSuper();
		return abstractPlaneSuper;
	}

	protected virtual void StartSuper()
	{
		animHelper = GetComponent<AnimationHelper>();
		animHelper.IgnoreGlobal = true;
		PauseManager.Pause();
		player.PauseAll();
		player.SetSpriteVisible(visibility: false);
		AudioManager.SnapshotTransition(new string[3] { "Super", "Unpaused", "Unpaused_1920s" }, new float[3] { 1f, 0f, 0f }, 0.1f);
		AudioManager.ChangeBGMPitch(1.3f, 1.5f);
		AudioManager.Play("player_super_beam_start");
		base.transform.SetScale(player.transform.localScale.x, player.transform.localScale.y, 1f);
		base.transform.position = player.transform.position;
	}

	protected virtual void Fire()
	{
		state = PlanePlayerWeaponManager.States.Super.Ending;
	}

	protected void SnapshotAudio()
	{
		string[] array = new string[2] { "Super", null };
		if (SettingsData.Data.vintageAudioEnabled)
		{
			array[1] = "Unpaused_1920s";
		}
		else
		{
			array[1] = "Unpaused";
		}
		AudioManager.SnapshotTransition(array, new float[2] { 0f, 1f }, 4f);
		AudioManager.ChangeBGMPitch(1f, 4f);
	}

	protected virtual void StartCountdown()
	{
		SnapshotAudio();
		PauseManager.Unpause();
		player.UnpauseAll();
		player.SetSpriteVisible(visibility: true);
		animHelper.IgnoreGlobal = false;
		state = PlanePlayerWeaponManager.States.Super.Countdown;
	}
}
