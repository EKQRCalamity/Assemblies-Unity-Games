using System;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Generic;

public class ParryAnimationBehaviour : StateMachineBehaviour
{
	public float WobblingSpeed;

	public float WobblingAmplitude;

	protected Vector2 DefaultSpritePosition;

	protected Entity Entity { get; private set; }

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (Entity == null)
		{
			Entity = animator.GetComponentInParent<Entity>();
		}
		Vector3 position = Entity.transform.position;
		DefaultSpritePosition = new Vector2(position.x, position.y);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Entity.transform.position = DefaultSpritePosition;
	}

	private void Wobbling()
	{
		Entity.transform.position = new Vector3(DefaultSpritePosition.x + (float)Math.Sin(Time.time) * WobblingSpeed, Entity.transform.position.y, Entity.transform.position.z);
	}
}
