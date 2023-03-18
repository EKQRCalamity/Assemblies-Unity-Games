using System.Collections;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Menina.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent.Gizmos;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.CrossCrawler.Attack;

public class CrossCrawlerAttack : EnemyAttack
{
	private Hit _weaponHit;

	public CrossCrawler crossCrawler;

	[FoldoutGroup("Ground Wave Attack Settings", true, 0)]
	public GameObject GroundWave;

	public float quakeSeconds;

	public float quakeVibrationStrenght;

	public float dodgeOpportunitySeconds;

	public LayerMask groundWaveLayerMask;

	private WaitForSeconds _waitSecondsQuake;

	private WaitForSeconds _waitSecondsDodge;

	private MeninaGroundWave _groundWave;

	public RootMotionDriver GroundWaveRoot { get; set; }

	private Vector3 GroundWavePosition => (base.EntityOwner.Status.Orientation != 0) ? GroundWaveRoot.ReversePosition : GroundWaveRoot.transform.position;

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponentInChildren<CrossCrawlerWeapon>();
		_waitSecondsQuake = new WaitForSeconds(quakeSeconds);
		_waitSecondsDodge = new WaitForSeconds(dodgeOpportunitySeconds);
	}

	protected override void OnStart()
	{
		base.OnStart();
		_weaponHit = new Hit
		{
			AttackingEntity = base.EntityOwner.gameObject,
			DamageAmount = base.EntityOwner.Stats.Strength.Final,
			DamageType = DamageType,
			Force = Force,
			HitSoundId = HitSound,
			Unnavoidable = false
		};
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		base.CurrentEnemyWeapon.Attack(_weaponHit);
	}

	private void CreateGroundwave()
	{
		Vector3 groundWavePosition = GroundWavePosition;
		PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(GroundWave, groundWavePosition, Quaternion.identity);
		_groundWave = objectInstance.GameObject.GetComponentInChildren<MeninaGroundWave>();
		_groundWave.SetOwner(base.EntityOwner);
		StartCoroutine(QuakeBelowPlayerCoroutine());
	}

	private IEnumerator QuakeBelowPlayerCoroutine()
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(quakeSeconds, new Vector2(0.1f, 3.4f), 40);
		Debug.Log("QUAKE START");
		yield return _waitSecondsQuake;
		Debug.Log("QUAKE END ");
		Vector2 targetPos2 = Core.Logic.Penitent.transform.position;
		bool groundExists = false;
		targetPos2 = GameplayUtils.GetGroundPosition(targetPos2, groundWaveLayerMask, out groundExists);
		_groundWave.transform.position = targetPos2;
		Core.Logic.CameraManager.ProCamera2DShake.ShakeUsingPreset("PietyStomp");
		yield return _waitSecondsDodge;
		_groundWave.TriggerWave();
	}
}
