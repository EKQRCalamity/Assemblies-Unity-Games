using CreativeSpore.SmartColliders;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Movement;

public class PhysicsSwitcher : MonoBehaviour
{
	private bool _enablePhysics;

	private PlatformCharacterController _playerController;

	public SmartPlatformCollider Collider { get; private set; }

	private void Awake()
	{
		_playerController = GetComponent<PlatformCharacterController>();
		Collider = GetComponent<SmartPlatformCollider>();
	}

	public void EnablePhysics(bool enable = true)
	{
		if (!_playerController.enabled && enable)
		{
			_playerController.enabled = true;
		}
		else if (_playerController.enabled && !enable)
		{
			_playerController.enabled = false;
		}
	}

	public void Enable2DCollision(bool enable = true)
	{
		if (!Collider.EnableCollision2D && enable)
		{
			Collider.EnableCollision2D = true;
		}
		else if (Collider.EnableCollision2D && !enable)
		{
			Collider.EnableCollision2D = false;
		}
	}

	public void EnableColliders(bool enable = true)
	{
		if (!Collider.enabled && enable)
		{
			Collider.enabled = true;
		}
		else if (Collider.enabled && !enable)
		{
			Collider.enabled = false;
		}
	}
}
