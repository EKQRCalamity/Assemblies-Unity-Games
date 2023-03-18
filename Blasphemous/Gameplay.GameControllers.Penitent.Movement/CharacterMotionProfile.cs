using System;
using CreativeSpore.SmartColliders;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Movement;

[CreateAssetMenu(fileName = "Character Motion Profile", menuName = "Blasphemous/Character/Motion Profile")]
public class CharacterMotionProfile : ScriptableObject
{
	[Header("Physics Parameters")]
	public float gravityScale;

	public float terminalVelocity;

	[Space(10f)]
	[Header("Moving Parameters")]
	public float walkingAcc;

	public float walkingDrag;

	[Range(0f, 10f)]
	public float maxWalkingSpeed;

	[Space(10f)]
	[Header("Jumping Parameters")]
	public float airborneAcc;

	public float jumpingSpeed;

	public float jumpingAcc;

	public float jumpingAccTime;

	public static event Action OnMotionProfileLoaded;

	public void Init(PlatformCharacterController controller)
	{
		controller.PlatformCharacterPhysics.GravityScale = gravityScale;
		controller.PlatformCharacterPhysics.TerminalVelocity = terminalVelocity;
		controller.WalkingAcc = walkingAcc;
		controller.WalkingDrag = walkingDrag;
		maxWalkingSpeed = controller.PlatformCharacterPhysics.SolveMaxSpeedWithAccAndDrag(controller.WalkingAcc, controller.WalkingDrag);
		controller.MaxWalkingSpeed = maxWalkingSpeed;
		controller.AirborneAcc = airborneAcc;
		controller.JumpingSpeed = jumpingSpeed;
		controller.JumpingAcc = jumpingAcc;
		controller.JumpingAccTime = jumpingAccTime;
		if (CharacterMotionProfile.OnMotionProfileLoaded != null)
		{
			CharacterMotionProfile.OnMotionProfileLoaded();
		}
	}
}
