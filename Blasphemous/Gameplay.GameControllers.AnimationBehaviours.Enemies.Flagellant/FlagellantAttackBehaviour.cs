using Framework.FrameworkCore;
using Gameplay.GameControllers.Enemies.Flagellant;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Enemies.Flagellant;

public class FlagellantAttackBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Enemies.Flagellant.Flagellant _flagellant;

	private float _normalizedTime;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_flagellant == null)
		{
			_flagellant = animator.GetComponentInParent<Gameplay.GameControllers.Enemies.Flagellant.Flagellant>();
		}
		_normalizedTime = 0f;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!(_flagellant == null))
		{
			_normalizedTime = stateInfo.normalizedTime - (float)Mathf.FloorToInt(stateInfo.normalizedTime);
			if (_normalizedTime < 0.1f)
			{
				LookAtTarget(_flagellant.Target.transform.position);
			}
			if (!_flagellant.IsAttacking)
			{
				_flagellant.IsAttacking = true;
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!(_flagellant == null) && _flagellant.IsAttacking)
		{
			_flagellant.IsAttacking = !_flagellant.IsAttacking;
		}
	}

	private void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos.x > _flagellant.transform.position.x)
		{
			if (_flagellant.Status.Orientation != 0)
			{
				_flagellant.SetOrientation(EntityOrientation.Right);
			}
		}
		else if (_flagellant.Status.Orientation != EntityOrientation.Left)
		{
			_flagellant.SetOrientation(EntityOrientation.Left);
		}
	}
}
