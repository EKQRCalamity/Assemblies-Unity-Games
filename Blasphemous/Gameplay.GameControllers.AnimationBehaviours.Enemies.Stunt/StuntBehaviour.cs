using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using Tools.Level.Interactables;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Stunt;

public class StuntBehaviour : StateMachineBehaviour
{
	private Execution _execution;

	private Enemy _enemy;

	private float _currentStuntTime;

	private const float UnAttacableTimeThreshold = 0.3f;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		_execution = animator.GetComponentInParent<Execution>();
		_enemy = _execution.ExecutedEntity;
		_currentStuntTime = 0f;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		_currentStuntTime += Time.deltaTime;
		if (_currentStuntTime >= _enemy.StuntTime)
		{
			EnemyBehaviour component = _enemy.GetComponent<EnemyBehaviour>();
			if ((bool)component)
			{
				component.Alive();
			}
			Object.Destroy(animator.transform.parent.gameObject);
		}
	}
}
