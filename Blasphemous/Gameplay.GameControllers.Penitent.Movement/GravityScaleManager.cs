using CreativeSpore.SmartColliders;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent.InputSystem;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Movement;

[RequireComponent(typeof(Entity))]
[RequireComponent(typeof(PlatformCharacterController))]
[RequireComponent(typeof(PlatformCharacterInput))]
public class GravityScaleManager : MonoBehaviour
{
	private PlatformCharacterController _platformCharacterController;

	public const float MAX_GRAVITY_SCALE = 2.5f;

	public const float MIN_GRAVITY_SCALE = 0f;

	public const float MAX_GRAVITY = 9.8f;

	public bool gravityScaleEnabled;

	private bool _gravityIsModified;

	private void Awake()
	{
		_platformCharacterController = GetComponent<PlatformCharacterController>();
	}

	private void Start()
	{
		_gravityIsModified = _platformCharacterController.IsGrounded;
	}

	private void Update()
	{
		if (gravityScaleEnabled)
		{
			characterGravityScaling();
		}
	}

	private void characterGravityScaling()
	{
		if (!_platformCharacterController.IsGrounded)
		{
			if (!_gravityIsModified)
			{
				_gravityIsModified = true;
				SetGravityScale(2.5f);
			}
		}
		else if (_gravityIsModified)
		{
			_gravityIsModified = !_gravityIsModified;
			SetGravityScale(0f);
		}
	}

	public void SetGravityScale(float gravityScale)
	{
		if (!Mathf.Approximately(_platformCharacterController.PlatformCharacterPhysics.GravityScale, gravityScale))
		{
			_platformCharacterController.PlatformCharacterPhysics.GravityScale = gravityScale;
		}
	}

	public void SetGravity(float gravity)
	{
		if (!Mathf.Approximately(_platformCharacterController.PlatformCharacterPhysics.Gravity.y, gravity))
		{
			Vector3 gravity2 = new Vector3(0f, gravity, 0f);
			_platformCharacterController.PlatformCharacterPhysics.Gravity = gravity2;
		}
	}
}
