using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Nun.Attack;

public class OilPuddle : Weapon
{
	[FoldoutGroup("Attack Settings", true, 0)]
	public float BoilingTime = 5f;

	[FoldoutGroup("Attack Settings", true, 0)]
	public float Damage = 5f;

	[FoldoutGroup("Audio", true, 0)]
	[EventRef]
	public string HitSoundId;

	[FoldoutGroup("Audio", true, 0)]
	[EventRef]
	public string AppearSoundId;

	[FoldoutGroup("Audio", true, 0)]
	[EventRef]
	public string VanishSoundId;

	private float _currentBoilingTime;

	private bool _vanish;

	private AttackArea _attackArea;

	private Hit _oilPuddleHit;

	private EventInstance _oilPuddleBubble;

	private UnityEngine.Animator Animator { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		Animator = GetComponentInChildren<UnityEngine.Animator>();
		_attackArea.OnEnter += AttackAreaOnEnter;
		_oilPuddleHit = new Hit
		{
			AttackingEntity = base.gameObject,
			DamageAmount = Damage,
			DamageType = DamageArea.DamageType.Normal,
			Force = 0f,
			HitSoundId = HitSoundId,
			Unnavoidable = true
		};
	}

	private void AttackAreaOnEnter(object sender, Collider2DParam e)
	{
		Attack(_oilPuddleHit);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		_currentBoilingTime += Time.deltaTime;
		if (_currentBoilingTime >= BoilingTime && !_vanish)
		{
			_vanish = true;
			Animator.SetTrigger("VANISH");
			Core.Audio.PlaySfx(VanishSoundId);
			StopBubbles();
		}
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public void SetOwner(Entity owner)
	{
		if (WeaponOwner == null)
		{
			WeaponOwner = owner;
		}
		if (_attackArea == null)
		{
			_attackArea = GetComponentInChildren<AttackArea>();
		}
		_attackArea.Entity = WeaponOwner;
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		_vanish = false;
		_currentBoilingTime = 0f;
		PlayBubbles();
	}

	public void Dissappear()
	{
		Destroy();
	}

	public void PlayBubbles()
	{
		if (!_oilPuddleBubble.isValid())
		{
			_oilPuddleBubble = Core.Audio.CreateEvent(AppearSoundId);
			_oilPuddleBubble.start();
		}
	}

	public void StopBubbles()
	{
		if (_oilPuddleBubble.isValid())
		{
			_oilPuddleBubble.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_oilPuddleBubble.release();
			_oilPuddleBubble = default(EventInstance);
		}
	}
}
