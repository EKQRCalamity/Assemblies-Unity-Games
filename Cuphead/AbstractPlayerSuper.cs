using System;
using UnityEngine;

public abstract class AbstractPlayerSuper : AbstractCollidableObject
{
	[SerializeField]
	[Header("Player Sprites")]
	private SpriteRenderer cuphead;

	[SerializeField]
	private SpriteRenderer mugman;

	[SerializeField]
	private bool isChaliceSuper;

	protected SpriteRenderer spriteRenderer;

	protected LevelPlayerController player;

	protected DamageDealer damageDealer;

	protected AnimationHelper animHelper;

	protected bool interrupted;

	protected override bool allowCollisionPlayer => false;

	public event Action OnEndedEvent;

	public event Action OnStartedEvent;

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

	protected virtual void Update()
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

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (player != null)
		{
			player.weaponManager.OnSuperInterrupt -= Interrupt;
		}
	}

	public AbstractPlayerSuper Create(LevelPlayerController player)
	{
		AbstractPlayerSuper abstractPlayerSuper = InstantiatePrefab<AbstractPlayerSuper>();
		abstractPlayerSuper.player = player;
		PlayerId id = player.id;
		if (id == PlayerId.PlayerOne || id != PlayerId.PlayerTwo)
		{
			if (!isChaliceSuper)
			{
				abstractPlayerSuper.spriteRenderer = ((!PlayerManager.player1IsMugman) ? cuphead : mugman);
				abstractPlayerSuper.cuphead.gameObject.SetActive(!PlayerManager.player1IsMugman);
				abstractPlayerSuper.mugman.gameObject.SetActive(PlayerManager.player1IsMugman);
			}
		}
		else if (!isChaliceSuper)
		{
			abstractPlayerSuper.spriteRenderer = ((!PlayerManager.player1IsMugman) ? mugman : cuphead);
			abstractPlayerSuper.cuphead.gameObject.SetActive(PlayerManager.player1IsMugman);
			abstractPlayerSuper.mugman.gameObject.SetActive(!PlayerManager.player1IsMugman);
		}
		interrupted = false;
		player.weaponManager.OnSuperInterrupt += abstractPlayerSuper.Interrupt;
		abstractPlayerSuper.StartSuper();
		return abstractPlayerSuper;
	}

	public virtual void Interrupt()
	{
		interrupted = true;
	}

	protected virtual void StartSuper()
	{
		AnimationHelper component = GetComponent<AnimationHelper>();
		component.IgnoreGlobal = true;
		PauseManager.Pause();
		AudioManager.HandleSnapshot(AudioManager.Snapshots.SuperStart.ToString(), 0.2f);
		AudioManager.ChangeBGMPitch(1.3f, 1.5f);
		base.transform.SetScale(player.transform.localScale.x, player.transform.localScale.y, 1f);
		base.transform.position = player.transform.position;
		if (this.OnStartedEvent != null)
		{
			this.OnStartedEvent();
		}
		this.OnStartedEvent = null;
	}

	protected virtual void Fire()
	{
		PauseManager.Unpause();
		AudioManager.HandleSnapshot(AudioManager.Snapshots.Super.ToString(), 0.2f);
		if (player == null)
		{
			Interrupt();
		}
		else
		{
			player.PauseAll();
		}
		AnimationHelper component = GetComponent<AnimationHelper>();
		component.IgnoreGlobal = false;
	}

	protected virtual void EndSuper(bool changePitch = true)
	{
		AudioManager.SnapshotReset(SceneLoader.SceneName, 1f);
		if (changePitch)
		{
			AudioManager.ChangeBGMPitch(1f, 2f);
		}
		if (player != null)
		{
			player.UnpauseAll();
		}
		if (this.OnEndedEvent != null)
		{
			this.OnEndedEvent();
		}
		this.OnEndedEvent = null;
	}
}
