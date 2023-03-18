using Framework.FrameworkCore;
using Gameplay.GameControllers.Bosses.PietyMonster;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Bosses.Pietat.Attack;

public class SmashToIdleBehaviour : StateMachineBehaviour
{
	private PietyMonster _pietyMonster;

	private GameObject _target;

	private readonly int _smashToIdle = Animator.StringToHash("SmashToIdleReverse");

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_pietyMonster == null)
		{
			_pietyMonster = animator.GetComponentInParent<PietyMonster>();
		}
		_pietyMonster.BodyBarrier.EnableCollider();
		_target = _pietyMonster.PietyRootsManager.Target;
		Vector3 position = _target.transform.position;
		if (_pietyMonster.transform.position.x >= position.x && _pietyMonster.Status.Orientation == EntityOrientation.Right)
		{
			animator.Play(_smashToIdle);
		}
		if (_pietyMonster.transform.position.x < position.x && _pietyMonster.Status.Orientation == EntityOrientation.Left)
		{
			_pietyMonster.SetOrientation(EntityOrientation.Right);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (_pietyMonster.PietyBehaviour.ReadyToAttack)
		{
			_pietyMonster.PietyBehaviour.ReadyToAttack = false;
		}
	}
}
