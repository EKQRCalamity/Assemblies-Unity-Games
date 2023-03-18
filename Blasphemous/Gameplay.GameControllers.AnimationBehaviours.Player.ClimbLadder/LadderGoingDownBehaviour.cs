using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Animator;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.ClimbLadder;

public class LadderGoingDownBehaviour : StateMachineBehaviour
{
	private readonly int _jumpForwardAnimHash = Animator.StringToHash("Jump Forward");

	private readonly int _ladderGoingUpAnimHash = Animator.StringToHash("ladder_going_up");

	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private AnimatorInyector _animatorInyector;

	private bool _soundClimbingLadder;

	private Vector2 _currentPosition;

	private Vector2 _lastPosition;

	[Range(0f, 1f)]
	public float verticalAxisThreshold = 0.5f;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (_penitent == null)
		{
			_penitent = Core.Logic.Penitent;
			_animatorInyector = _penitent.GetComponentInChildren<AnimatorInyector>();
		}
		if (_penitent.StartingGoingDownLadders)
		{
			_penitent.StartingGoingDownLadders = !_penitent.StartingGoingDownLadders;
		}
		_penitent.GrabLadder.SetClimbingSpeed(2.25f);
		if (_animatorInyector.ForwardJump)
		{
			_animatorInyector.ForwardJump = !_animatorInyector.ForwardJump;
		}
		if (!_penitent.CanJumpFromLadder)
		{
			_penitent.CanJumpFromLadder = true;
		}
		if (_penitent.IsCrouched)
		{
			_penitent.IsCrouched = false;
		}
		_lastPosition = new Vector2(_penitent.transform.position.x, _penitent.transform.position.y);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_currentPosition = _penitent.transform.position;
		if (!_penitent.IsClimbingLadder)
		{
			_penitent.JumpFromLadder = true;
			animator.Play(_jumpForwardAnimHash);
		}
		if (_penitent.PlatformCharacterInput.FVerAxis > 0f && _currentPosition != _lastPosition)
		{
			_lastPosition = _currentPosition;
			animator.speed = 1f;
			animator.Play(_ladderGoingUpAnimHash);
		}
		else if (_penitent.PlatformCharacterInput.FVerAxis <= 0f - verticalAxisThreshold)
		{
			animator.speed = 1f;
		}
		else
		{
			animator.speed = 0f;
		}
		if (!_penitent.IsLadderSliding)
		{
			_penitent.GrabLadder.SetClimbingSpeed(2.25f);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.speed = 1f;
	}
}
