using Framework.Managers;
using Gameplay.GameControllers.Penitent.Abilities;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Attack;

public class FinishingComboDownBehaviour : StateMachineBehaviour
{
	public VerticalAttack.AttackAreaDimensions AttackAreaDimensions;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		SetAttackAreaDimension();
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
	}

	public void SetAttackAreaDimension()
	{
		Core.Logic.Penitent.AttackArea.SetSize(AttackAreaDimensions.AttackAreaSize);
		Core.Logic.Penitent.AttackArea.SetOffset(AttackAreaDimensions.AttackAreaOffset);
	}
}
