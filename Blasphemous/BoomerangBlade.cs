using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;

public class BoomerangBlade : Weapon
{
	private Animator _animator;

	public Enemy AttackingEntity;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string hitSound;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string flightSound;

	private Hit _hit;

	private EventInstance _flyingBladeAudioInstance;

	public AttackArea AttackArea { get; private set; }

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponentInChildren<AttackArea>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		AttackArea.OnEnter += AttackAreaOnEnter;
	}

	public new void SetHit(Hit hit)
	{
		_hit = hit;
		_hit.HitSoundId = hitSound;
	}

	private void AttackAreaOnEnter(object sender, Collider2DParam collider2DParam)
	{
		Attack(_hit);
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	public void SetOwner(Enemy enemy)
	{
		WeaponOwner = (AttackingEntity = enemy);
	}

	private void OnDestroy()
	{
		StopFlightFx();
		AttackArea.OnEnter -= AttackAreaOnEnter;
	}

	public void PlayFlightFx()
	{
		if (_flyingBladeAudioInstance.isValid())
		{
			StopFlightFx();
		}
		_flyingBladeAudioInstance = Core.Audio.CreateEvent(flightSound);
		_flyingBladeAudioInstance.start();
	}

	public void StopFlightFx()
	{
		if (_flyingBladeAudioInstance.isValid())
		{
			_flyingBladeAudioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_flyingBladeAudioInstance.release();
			_flyingBladeAudioInstance = default(EventInstance);
		}
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		PlayFlightFx();
	}

	public void Recycle()
	{
		StopFlightFx();
		Destroy();
	}
}
