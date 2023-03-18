using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.Attack;

public class FinishingComboStarterBehaviour : StateMachineBehaviour
{
	private enum JoyStickDirection
	{
		Up,
		Down,
		Center
	}

	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private float _attackBuffer;

	private float _joyStickBuffer;

	private float _verticalAxis;

	private const float COMBO_BUFFER_TIME = 0.25f;

	private bool _allowFinisherCombo;

	private JoyStickDirection _comboDirection;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
		}
		_attackBuffer = 0f;
		_joyStickBuffer = 0f;
		_allowFinisherCombo = false;
		_comboDirection = JoyStickDirection.Center;
		Vector2 size = new Vector2(3.57f, 2.25f);
		Vector2 offset = new Vector2(1.69f, 1.75f);
		_penitent.AttackArea.SetSize(size);
		_penitent.AttackArea.SetOffset(offset);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (stateInfo.normalizedTime > 0.25f)
		{
			_allowFinisherCombo = true;
		}
		_attackBuffer += Time.deltaTime;
		if (_penitent.PlatformCharacterInput.Attack)
		{
			_attackBuffer = 0f;
		}
		_verticalAxis = _penitent.PlatformCharacterInput.FVerAxis;
		if (_verticalAxis > 0f)
		{
			_comboDirection = JoyStickDirection.Up;
		}
		else if (_verticalAxis < 0f)
		{
			_comboDirection = JoyStickDirection.Down;
		}
		if (stateInfo.normalizedTime > 0.65f && _attackBuffer <= 0.25f)
		{
			PenitentAttack penitentAttack = (PenitentAttack)_penitent.EntityAttack;
			if (penitentAttack.Combo.IsAvailable)
			{
				SetFinisherByAbilityTier(penitentAttack.Combo.GetMaxSkill.id, animator);
			}
		}
		if (_comboDirection != JoyStickDirection.Center)
		{
			_joyStickBuffer += Time.deltaTime;
			if (_joyStickBuffer >= 0.5f)
			{
				_joyStickBuffer = 0f;
				_comboDirection = JoyStickDirection.Center;
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (_allowFinisherCombo)
		{
			PenitentAttack penitentAttack = (PenitentAttack)_penitent.EntityAttack;
			if (penitentAttack.Combo.IsAvailable)
			{
			}
		}
	}

	private void SetFinisherByAbilityTier(string skillId, Animator animator)
	{
		_attackBuffer = 0f;
		switch (skillId)
		{
		case "COMBO_1":
			animator.Play("Combo_4");
			break;
		case "COMBO_2":
			animator.Play((_comboDirection != 0) ? "Combo_4" : "ComboFinisherUp");
			break;
		case "COMBO_3":
			if (_comboDirection == JoyStickDirection.Down)
			{
				animator.Play("ComboFinisherDown");
			}
			else if (_comboDirection == JoyStickDirection.Up)
			{
				animator.Play("ComboFinisherUp");
			}
			else
			{
				animator.Play("Combo_4");
			}
			break;
		}
	}
}
