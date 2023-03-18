using Framework.Managers;
using Gameplay.GameControllers.Entities.Animations;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Attack;

public class LungeAttackBehaviour : StateMachineBehaviour
{
	private LungeAttack _lungeAttack;

	public VerticalAttack.AttackAreaDimensions AttackAreaDimensions;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_lungeAttack == null)
		{
			Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
			_lungeAttack = penitent.GetComponentInChildren<LungeAttack>();
		}
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		SetAttackAreaDimension();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		_lungeAttack.StopCast();
		Core.Logic.Penitent.PenitentMoveAnimations.EnabledGhostTrail(AttackAnimationsEvents.Activation.False);
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
	}

	public void SetAttackAreaDimension()
	{
		Core.Logic.Penitent.AttackArea.SetSize(AttackAreaDimensions.AttackAreaSize);
		Core.Logic.Penitent.AttackArea.SetOffset(AttackAreaDimensions.AttackAreaOffset);
	}
}
