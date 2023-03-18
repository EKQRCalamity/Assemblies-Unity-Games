using Framework.Managers;
using Gameplay.GameControllers.AlliedCherub.AI;
using Gameplay.GameControllers.Effects.Player.Protection;
using Gameplay.GameControllers.Enemies.BellGhost;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.AlliedCherub;

public class AlliedCherub : Entity
{
	public Vector2 FlyingOffset;

	public GameObject vfxOnExitPrefab;

	public Vector3 FlyingPosition { get; private set; }

	public StateMachine StateMachine { get; private set; }

	public AlliedCherubBehaviour Behaviour { get; private set; }

	public Entity Ally { get; private set; }

	public BellGhostFloatingMotion FloatingMotion { get; private set; }

	public AlliedCherubSystem CherubSystem { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		StateMachine = GetComponent<StateMachine>();
		Behaviour = GetComponent<AlliedCherubBehaviour>();
		FloatingMotion = GetComponentInChildren<BellGhostFloatingMotion>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		PoolManager.Instance.CreatePool(vfxOnExitPrefab, 2);
	}

	public void Deploy(AlliedCherubSystem.AlliedCherubSlot cherubSlot, AlliedCherubSystem cherubSystem)
	{
		FlyingOffset = new Vector2(cherubSlot.Offset.x, cherubSlot.Offset.y);
		FlyingPosition = Core.Logic.Penitent.transform.position + (Vector3)FlyingOffset;
		Ally = Core.Logic.Penitent;
		CherubSystem = cherubSystem;
		ShowEffect(FlyingPosition);
	}

	public void Store()
	{
		if (!(CherubSystem == null))
		{
			ShowEffect(base.transform.position);
			CherubSystem.StoreCherub(base.gameObject);
		}
	}

	public void ShowEffect(Vector2 pos)
	{
		if (vfxOnExitPrefab != null)
		{
			PoolManager.Instance.ReuseObject(vfxOnExitPrefab, pos, base.transform.rotation);
		}
	}
}
