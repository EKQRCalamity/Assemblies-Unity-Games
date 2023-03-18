using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.StateMachine;
using Gameplay.GameControllers.Familiar.AI;
using Plugins.GhostSprites2D.Scripts.GhostSprites;
using UnityEngine;

namespace Gameplay.GameControllers.Familiar;

public class Familiar : Entity, IDamageable
{
	public GameObject Target { get; set; }

	public Entity Owner { get; set; }

	public GhostSprites GhostTrail { get; private set; }

	public FamiliarBehaviour Behaviour { get; private set; }

	public StateMachine StateMachine { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		Behaviour = GetComponent<FamiliarBehaviour>();
		StateMachine = GetComponent<StateMachine>();
		GhostTrail = GetComponentInChildren<GhostSprites>();
	}

	public void Damage(Hit hit)
	{
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public void Dispose()
	{
		base.gameObject.SetActive(value: false);
	}
}
