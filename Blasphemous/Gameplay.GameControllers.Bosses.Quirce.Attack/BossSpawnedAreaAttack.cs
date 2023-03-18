using System;
using System.Collections.Generic;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce.Attack;

public class BossSpawnedAreaAttack : Weapon, IDirectAttack
{
	private enum AREA_STATES
	{
		PREPARATION,
		LOOPING,
		ENDING
	}

	[FoldoutGroup("Design settings", 0)]
	public GameObject onHitEffect;

	[FoldoutGroup("Design settings", 0)]
	public bool prohibitEffectInSomeScenes;

	[FoldoutGroup("Design settings", 0)]
	[ShowIf("prohibitEffectInSomeScenes", true)]
	public List<string> prohibitedScenesForEffect = new List<string>();

	[FoldoutGroup("Design settings", 0)]
	public bool unavoidable;

	[FoldoutGroup("Design settings", 0)]
	public bool unparriable;

	[FoldoutGroup("Design settings", 0)]
	public bool unblockable;

	[FoldoutGroup("Design settings", 0)]
	public string firstState = "Intro";

	[FoldoutGroup("Design settings", 0)]
	public bool flipRandomly;

	[FoldoutGroup("Time settings", 0)]
	public float preparationSeconds = 1f;

	[FoldoutGroup("Time settings", 0)]
	public float loopSeconds = 3f;

	[FoldoutGroup("Time settings", 0)]
	public float timeBetweenTicks = 0.1f;

	[FoldoutGroup("Time settings", 0)]
	public float firstTickDealy;

	[FoldoutGroup("Damage settings", 0)]
	public float DamageAmount = 10f;

	[FoldoutGroup("Damage settings", 0)]
	public DamageArea.DamageType damageType;

	[FoldoutGroup("Damage settings", 0)]
	public DamageArea.DamageElement damageElement;

	[FoldoutGroup("Damage settings", 0)]
	public float force;

	[FoldoutGroup("Collision settings", 0)]
	public bool snapToGround;

	[FoldoutGroup("Collision settings", 0)]
	public LayerMask groundMask;

	[FoldoutGroup("Collision settings", 0)]
	public float RangeGroundDetection = 2f;

	[FoldoutGroup("Collision settings", 0)]
	public float yOffset;

	[FoldoutGroup("Sound settings", 0)]
	[EventRef]
	public string PreparationLoopSoundId;

	[FoldoutGroup("Sound settings", 0)]
	[EventRef]
	public string AttackLoopSoundId;

	[FoldoutGroup("Sound settings", 0)]
	[EventRef]
	public string EndingSoundId;

	[FoldoutGroup("Sound settings", 0)]
	[EventRef]
	public string HitSoundId;

	[FoldoutGroup("Design settings", 0)]
	public bool screenshake;

	[FoldoutGroup("Design settings", 0)]
	[ShowIf("screenshake", true)]
	public int vibrato = 40;

	[FoldoutGroup("Design settings", 0)]
	[ShowIf("screenshake", true)]
	public float shakeDuration = 0.5f;

	[FoldoutGroup("Design settings", 0)]
	[ShowIf("screenshake", true)]
	public Vector2 shakeOffset;

	private float actualPreparationSeconds = 1f;

	private float actualLoopSeconds = 1f;

	private AREA_STATES _state;

	private float _hitStrength = 1f;

	private float _cdCounter;

	private RaycastHit2D[] _bottomHits;

	private Hit _hit;

	private float timer;

	private Action callbackOnLoopFinish;

	public AttackArea AttackArea { get; set; }

	public SpriteRenderer SpriteRenderer { get; set; }

	public Animator Animator { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponentInChildren<AttackArea>();
		Animator = GetComponentInChildren<Animator>();
		SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
		_bottomHits = new RaycastHit2D[1];
		CreateHit();
		if (onHitEffect != null)
		{
			PoolManager.Instance.CreatePool(onHitEffect, 10);
		}
		_cdCounter = firstTickDealy;
	}

	public void CreateHit()
	{
		_hit = new Hit
		{
			AttackingEntity = base.gameObject,
			DamageType = damageType,
			DamageAmount = DamageAmount * _hitStrength,
			HitSoundId = HitSoundId,
			Force = force,
			ThrowbackDirByOwnerPosition = true,
			Unnavoidable = unavoidable,
			Unparriable = unparriable,
			Unblockable = unblockable,
			DamageElement = damageElement
		};
	}

	public void SetDamage(int dmg)
	{
		DamageAmount = dmg;
		CreateHit();
	}

	public void SetDamageStrength(float strength)
	{
		_hitStrength = strength;
		CreateHit();
	}

	public void SetCallbackOnLoopFinish(Action callbackOnLoopFinish)
	{
		this.callbackOnLoopFinish = callbackOnLoopFinish;
	}

	private void PlaySound(string s)
	{
		if (s != string.Empty)
		{
			Core.Audio.PlaySfx(s);
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (_cdCounter > 0f && _state == AREA_STATES.LOOPING)
		{
			_cdCounter -= Time.deltaTime;
		}
		switch (_state)
		{
		case AREA_STATES.PREPARATION:
			timer += Time.deltaTime;
			if (timer > actualPreparationSeconds)
			{
				timer = 0f;
				_state = AREA_STATES.LOOPING;
				OnEnterDamageLoop();
				if (Animator != null)
				{
					Animator.SetTrigger("LOOP");
				}
			}
			break;
		case AREA_STATES.LOOPING:
			timer += Time.deltaTime;
			CheckTick();
			if (timer > actualLoopSeconds)
			{
				_state = AREA_STATES.ENDING;
				OnEnterEnding();
				if (callbackOnLoopFinish != null)
				{
					callbackOnLoopFinish();
				}
				if (Animator != null)
				{
					Animator.SetTrigger("END");
				}
			}
			break;
		}
	}

	protected virtual void OnEnterDamageLoop()
	{
		PlaySound(AttackLoopSoundId);
		if (screenshake)
		{
			Core.Logic.CameraManager.ProCamera2DShake.Shake(shakeDuration, shakeOffset, vibrato, 0.1f, 0f, default(Vector3), 0.06f);
		}
	}

	protected virtual void OnEnterEnding()
	{
		PlaySound(EndingSoundId);
	}

	private void SnapToGround()
	{
		Vector2 vector = base.transform.position;
		if (Physics2D.LinecastNonAlloc(vector, vector + Vector2.down * RangeGroundDetection, _bottomHits, groundMask) > 0)
		{
			base.transform.position += Vector3.down * _bottomHits[0].distance;
		}
		else
		{
			base.transform.position += Vector3.down * RangeGroundDetection;
		}
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
		if (onHitEffect == null || (prohibitEffectInSomeScenes && prohibitedScenesForEffect.Exists((string x) => Core.LevelManager.currentLevel.LevelName.StartsWith(x))))
		{
			return;
		}
		foreach (IDamageable damageableEntity in DamageableEntities)
		{
			PoolManager.Instance.ReuseObject(onHitEffect, (damageableEntity as Component).transform.position, Quaternion.identity);
		}
	}

	private void CheckTick()
	{
		if (!(_cdCounter > 0f) && GetDamageableEntities().Count != 0)
		{
			_cdCounter = timeBetweenTicks;
			Attack(_hit);
		}
	}

	public void SetOwner(Entity owner)
	{
		WeaponOwner = owner;
		if (AttackArea == null)
		{
			AttackArea = GetComponentInChildren<AttackArea>();
		}
		AttackArea.Entity = owner;
	}

	public void SetRemainingPreparationTime(float remainingTime)
	{
		if (_state == AREA_STATES.PREPARATION)
		{
			timer = actualPreparationSeconds - remainingTime;
		}
	}

	public void SetCustomTimes(float newPreparationSeconds, float newLoopSeconds)
	{
		actualPreparationSeconds = newPreparationSeconds;
		actualLoopSeconds = newLoopSeconds;
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public void OnEndAnimationFinished()
	{
		Recycle();
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		_cdCounter = firstTickDealy;
		if (flipRandomly)
		{
			SpriteRenderer.flipX = UnityEngine.Random.Range(0f, 1f) > 0.5f;
		}
		if (Animator != null)
		{
			timer = 0f;
			Animator.ResetTrigger("LOOP");
			Animator.ResetTrigger("END");
			Animator.Play(firstState, 0, 0f);
		}
		SetCustomTimes(preparationSeconds, loopSeconds);
		_state = AREA_STATES.PREPARATION;
		if (snapToGround)
		{
			SnapToGround();
		}
		PlaySound(PreparationLoopSoundId);
	}

	public void Recycle()
	{
		Destroy();
	}
}
