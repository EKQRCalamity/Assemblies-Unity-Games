using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Actionables;

public class ShockwaveArea : Weapon
{
	public GameObject damageArea;

	public float radius;

	public float duration = 1.2f;

	private float counter;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	protected string shockwaveSound;

	public float maxAudioDistance = 20f;

	public float minAudioDistance = 7f;

	private Animator _animator;

	private static readonly int ActivateParam = Animator.StringToHash("ACTIVATE");

	public AttackArea AttackArea { get; set; }

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		damageArea.transform.localScale = Vector3.one;
		counter = 0f;
		_animator = GetComponentInChildren<Animator>();
		Activate();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		counter += Time.deltaTime;
		if (counter >= duration)
		{
			Destroy();
		}
	}

	private void ShockwaveEffect()
	{
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, duration, 0.2f, 0.7f);
	}

	protected virtual void Activate()
	{
		PlayAreaSound();
		_animator.SetTrigger(ActivateParam);
	}

	public void PlayAreaSound()
	{
		if (!string.IsNullOrEmpty(shockwaveSound))
		{
			Core.Audio.PlayOneShot(shockwaveSound, base.transform.position);
		}
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
		Debug.Log("ON AREA HIT");
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
}
