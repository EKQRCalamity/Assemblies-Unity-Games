using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Attack;

public class ChargedAttackBehaviour : StateMachineBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	[Tooltip("The damage amount done by the penitent in this attack")]
	public int DamageAmount;

	private readonly Vector2 attackAreaSize = new Vector2(5.25f, 3.26f);

	private readonly Vector2 attackAreaOffset = new Vector2(0.82f, 2.26f);

	private Vector2 defaultAttackAreaOffset;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_penitent.CurrentOutputDamage = DamageAmount;
		_penitent.Audio.AudioManager.StopSfx("PenitentLoadingChargedAttack");
		_penitent.AnimatorInyector.RaiseAttackEvent();
		Vector2 offset = _penitent.AttackArea.WeaponCollider.offset;
		defaultAttackAreaOffset = new Vector2(offset.x, offset.y);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.normalizedTime >= 0.25f && stateInfo.normalizedTime <= 0.75f)
		{
			_penitent.PenitentAttack.IsWeaponBlowingUp = true;
		}
		else
		{
			_penitent.PenitentAttack.IsWeaponBlowingUp = false;
		}
		_penitent.ReleaseChargedAttack = true;
		_penitent.PenitentAttack.IsHeavyAttacking = true;
		_penitent.AttackArea.SetSize(attackAreaSize);
		_penitent.AttackArea.SetOffset(attackAreaOffset);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent.IsAttackCharged)
		{
			_penitent.IsAttackCharged = !_penitent.IsAttackCharged;
		}
		_penitent.ReleaseChargedAttack = false;
		_penitent.EntityAttack.IsHeavyAttacking = false;
		_penitent.ChargedAttack.ResizeAttackArea(resize: false);
		_penitent.AttackArea.SetOffset(defaultAttackAreaOffset);
	}
}
