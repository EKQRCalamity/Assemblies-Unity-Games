using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Attack;

public class AirUpwardAttackBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	public Vector2 AttackAreaOffset;

	public Vector2 AttackAreaSize;

	private Vector2 _defaultAttackAreaOffset;

	private Vector2 _defaultAttackAreaSize;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
			_defaultAttackAreaOffset = new Vector2(_penitent.AttackArea.WeaponCollider.offset.x, _penitent.AttackArea.WeaponCollider.offset.y);
			_defaultAttackAreaSize = new Vector2(_penitent.AttackArea.WeaponCollider.bounds.size.x, _penitent.AttackArea.WeaponCollider.bounds.size.y);
		}
		animator.SetBool("AIR_ATTACKING", value: true);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		_penitent.AttackArea.SetOffset(AttackAreaOffset);
		_penitent.AttackArea.SetSize(AttackAreaSize);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		animator.SetBool("AIR_ATTACKING", value: false);
		_penitent.AttackArea.SetOffset(_defaultAttackAreaOffset);
		_penitent.AttackArea.SetSize(_defaultAttackAreaSize);
	}
}
