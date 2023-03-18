using System.Collections;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint.Attack;

public class BejeweledSaintCastArm : EnemyAttack
{
	public Enemy Owner;

	public BejeweledSaintBoss Boss { get; private set; }

	public Animator Animator { get; private set; }

	private void Awake()
	{
		base.EntityOwner = Owner;
		Boss = Owner.GetComponentInParent<BejeweledSaintBoss>();
		Animator = GetComponent<Animator>();
	}

	public void CastBeams()
	{
		if (!(Boss == null))
		{
			Boss.BeamManager.InstantiateDivineBeams();
		}
	}

	public void CastSingleBeam(Vector2 pos)
	{
		if (!(Boss == null))
		{
			Boss.BeamManager.InstantiateSingleBeam(pos);
		}
	}

	public void CastSingleBeamDelayed(Vector2 pos, float delay)
	{
		if (!(Boss == null))
		{
			StartCoroutine(DelayedSingleBeam(pos, delay));
		}
	}

	private IEnumerator DelayedSingleBeam(Vector2 pos, float delay)
	{
		yield return new WaitForSeconds(delay);
		Boss.BeamManager.InstantiateSingleBeam(pos);
	}

	public void DoCastSign()
	{
		if (!(Animator == null))
		{
			Animator.SetTrigger("CAST");
		}
	}
}
