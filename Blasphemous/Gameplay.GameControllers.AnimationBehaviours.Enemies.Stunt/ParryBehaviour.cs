using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Stunt;

public class ParryBehaviour : StateMachineBehaviour
{
	private Enemy _owner;

	public float RemainTimeParried = 1f;

	private float _currentTimeParried;

	private EnemyBehaviour _behaviour;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		_owner = animator.GetComponentInParent<Enemy>();
		_behaviour = _owner.GetComponentInChildren<EnemyBehaviour>();
		_currentTimeParried = 0f;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		_currentTimeParried += Time.deltaTime;
		if (_currentTimeParried >= RemainTimeParried && !Core.Logic.Penitent.IsOnExecution)
		{
			_behaviour.Alive();
		}
	}
}
