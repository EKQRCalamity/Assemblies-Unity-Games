using System.Collections;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Gameplay.GameControllers.Penitent.Gizmos;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Menina.Attack;

public class MeninaAttack : EnemyAttack
{
	[FoldoutGroup("Ground Wave Attack Settings", true, 0)]
	public GameObject GroundWave;

	[FoldoutGroup("Ground Wave Attack Settings", true, 0)]
	public int WavesAmount = 3;

	[FoldoutGroup("Ground Wave Attack Settings", true, 0)]
	public float WavesSpawningLapse = 1f;

	[FoldoutGroup("Ground Wave Attack Settings", true, 0)]
	public float GroundWavesSpacing = 2f;

	[FoldoutGroup("Ground Wave Attack Settings", true, 0)]
	public bool SpawnWaves = true;

	private List<MeninaGroundWave> _meninaGroundWaves = new List<MeninaGroundWave>();

	private WaitForSeconds _spawningLapseWaiting;

	private WaitForEndOfFrame _endOfFrameWaiting;

	public RootMotionDriver GroundWaveRoot { get; set; }

	private Hit GetHit
	{
		get
		{
			Hit result = default(Hit);
			result.AttackingEntity = base.EntityOwner.gameObject;
			result.DamageAmount = base.EntityOwner.Stats.Strength.Final;
			result.DamageType = DamageType;
			result.Force = Force;
			result.HitSoundId = HitSound;
			result.Unnavoidable = true;
			return result;
		}
	}

	private Vector3 GroundWavePosition => (base.EntityOwner.Status.Orientation != 0) ? GroundWaveRoot.ReversePosition : GroundWaveRoot.transform.position;

	protected override void OnAwake()
	{
		base.OnAwake();
		base.CurrentEnemyWeapon = GetComponent<Weapon>();
		_spawningLapseWaiting = new WaitForSeconds(WavesSpawningLapse);
		_endOfFrameWaiting = new WaitForEndOfFrame();
	}

	protected override void OnStart()
	{
		base.OnStart();
		GroundWaveRoot = base.EntityOwner.GetComponentInChildren<RootMotionDriver>();
		if (GroundWave != null)
		{
			PoolManager.Instance.CreatePool(GroundWave, WavesAmount * 2);
		}
	}

	public override void CurrentWeaponAttack()
	{
		base.CurrentWeaponAttack();
		base.CurrentEnemyWeapon.Attack(GetHit);
		if (SpawnWaves)
		{
			StartCoroutine(SpawnGroundWaves());
		}
	}

	public void StopAttack()
	{
		StopCoroutine(SpawnGroundWaves());
	}

	private IEnumerator SpawnGroundWaves()
	{
		if (!GroundWave)
		{
			yield break;
		}
		float spacing = 0f;
		for (int i = 0; i < WavesAmount; i++)
		{
			Vector3 position = GroundWavePosition;
			if (base.EntityOwner.Status.Orientation == EntityOrientation.Right)
			{
				position.x += spacing;
			}
			else
			{
				position.x -= spacing;
			}
			PoolManager.ObjectInstance go = PoolManager.Instance.ReuseObject(GroundWave, position, Quaternion.identity);
			MeninaGroundWave groundWave = go.GameObject.GetComponentInChildren<MeninaGroundWave>();
			groundWave.SetOwner(base.EntityOwner);
			_meninaGroundWaves.Add(groundWave);
			spacing += GroundWavesSpacing;
			yield return _endOfFrameWaiting;
		}
		StartCoroutine(TriggerGroundWaves());
	}

	private IEnumerator TriggerGroundWaves()
	{
		List<MeninaGroundWave> groundWavesCopy = new List<MeninaGroundWave>(_meninaGroundWaves);
		foreach (MeninaGroundWave groundWaves in groundWavesCopy)
		{
			groundWaves.TriggerWave();
			yield return _spawningLapseWaiting;
		}
		_meninaGroundWaves.Clear();
	}
}
