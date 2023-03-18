using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Animator;
using UnityEngine;

namespace Gameplay.GameControllers.AnimationBehaviours.Player.ClimbLadder;

public class LadderGoingUpBehaviour : StateMachineBehaviour
{
	private readonly int _jumpForwardAnimHash = Animator.StringToHash("Jump Forward");

	private readonly int _releaseLadderToUpAnimHash = Animator.StringToHash("release_ladder_to_floor_up");

	private readonly int _ladderGoingDownAnimHash = Animator.StringToHash("ladder_going_down");

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
		_penitent.GrabLadder.SetClimbingSpeed(2.25f);
		_penitent.DamageArea.IncludeEnemyLayer(include: false);
		if (_animatorInyector.ForwardJump)
		{
			_animatorInyector.ForwardJump = !_animatorInyector.ForwardJump;
		}
		if (!_penitent.CanJumpFromLadder)
		{
			_penitent.CanJumpFromLadder = true;
		}
		_lastPosition = _penitent.transform.position;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_currentPosition = _penitent.transform.position;
		if (!_penitent.IsClimbingLadder)
		{
			_penitent.JumpFromLadder = true;
			animator.Play(_jumpForwardAnimHash);
		}
		if (_penitent.ReachTopLadder)
		{
			_penitent.transform.position = _penitent.RootMotionDrive;
			animator.Play(_releaseLadderToUpAnimHash);
		}
		else if (_penitent.PlatformCharacterInput.FVerAxis > verticalAxisThreshold)
		{
			animator.speed = 1f;
		}
		else if (_penitent.PlatformCharacterInput.FVerAxis < 0f && _currentPosition != _lastPosition)
		{
			animator.speed = 1f;
			animator.Play(_ladderGoingDownAnimHash);
		}
		else
		{
			animator.speed = 0f;
		}
		_lastPosition = _currentPosition;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_penitent.DamageArea.IncludeEnemyLayer();
		animator.speed = 1f;
		_penitent.ReachTopLadder = false;
		_penitent.RootMotionDrive = Vector3.zero;
	}
}
