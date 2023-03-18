using System;
using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Enemies.ChainedAngel;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Framework.Physics;
using Gameplay.GameControllers.Enemies.Processioner.AI;
using Gameplay.GameControllers.Enemies.Processioner.Animator;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Gizmos;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Processioner;

public class Processioner : Enemy
{
	private GameObject _angelLink;

	private RootMotionDriver _rootMotion;

	public bool ChainedAngelComposition;

	[ShowIf("ChainedAngelComposition", true)]
	public GameObject ChainedAngelPrefab;

	public EnemyDamageArea DamageArea { get; private set; }

	public VisionCone VisionCone { get; private set; }

	public ProcessionerBehaviour Behaviour { get; set; }

	public NPCInputs Input { get; private set; }

	public ProcessionerAnimator ProcessionerAnimator { get; private set; }

	public EntityMotionChecker MotionChecker { get; private set; }

	public ColorFlash ColorFlash { get; private set; }

	public ContactDamage ContactDamage { get; private set; }

	public Gameplay.GameControllers.Enemies.ChainedAngel.ChainedAngel ChainedAngel { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		base.Controller = GetComponentInChildren<PlatformCharacterController>();
		DamageArea = GetComponentInChildren<EnemyDamageArea>();
		VisionCone = GetComponentInChildren<VisionCone>();
		Behaviour = GetComponentInChildren<ProcessionerBehaviour>();
		Input = GetComponentInChildren<NPCInputs>();
		ProcessionerAnimator = GetComponentInChildren<ProcessionerAnimator>();
		MotionChecker = GetComponentInChildren<EntityMotionChecker>();
		ColorFlash = GetComponentInChildren<ColorFlash>();
		ContactDamage = GetComponentInChildren<ContactDamage>();
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
		base.Target = penitent.gameObject;
	}

	protected override void OnStart()
	{
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!base.Landing)
		{
			base.Landing = true;
			SetPositionAtStart();
			if (ChainedAngelComposition)
			{
				AddChainedAngel();
			}
		}
		if (ChainedAngelComposition)
		{
			UpdateChainedAngelPosition();
		}
	}

	public override void SetPositionAtStart()
	{
		base.SetPositionAtStart();
		float groundDist = base.Controller.GroundDist;
		Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y - groundDist, base.transform.position.z);
		base.transform.position = position;
	}

	public override EnemyFloorChecker EnemyFloorChecker()
	{
		throw new NotImplementedException();
	}

	public override EnemyAttack EnemyAttack()
	{
		throw new NotImplementedException();
	}

	public override EnemyBumper EnemyBumper()
	{
		throw new NotImplementedException();
	}

	protected override void EnablePhysics(bool enable = true)
	{
	}

	private void AddChainedAngel()
	{
		if ((bool)ChainedAngelPrefab)
		{
			_rootMotion = GetComponentInChildren<RootMotionDriver>();
			GameObject gameObject = UnityEngine.Object.Instantiate(ChainedAngelPrefab, base.transform.position, Quaternion.identity);
			ChainedAngel = gameObject.GetComponentInChildren<Gameplay.GameControllers.Enemies.ChainedAngel.ChainedAngel>();
			if ((bool)ChainedAngel)
			{
				ChainedAngel.Target = base.Target;
				ChainedAngel.Behaviour.enabled = true;
				_angelLink = ChainedAngel.GetLowerLink();
				_angelLink.transform.position = ((!base.SpriteRenderer.flipX) ? _rootMotion.transform.position : _rootMotion.ReversePosition);
				_angelLink.transform.parent.transform.parent = base.transform;
			}
		}
	}

	private void UpdateChainedAngelPosition()
	{
		if ((bool)_rootMotion && (bool)_angelLink)
		{
			_angelLink.transform.position = ((!base.SpriteRenderer.flipX) ? _rootMotion.transform.position : _rootMotion.ReversePosition);
		}
	}
}
