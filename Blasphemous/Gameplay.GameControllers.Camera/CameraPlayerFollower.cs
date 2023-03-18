using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

namespace Gameplay.GameControllers.Camera;

[RequireComponent(typeof(ProCamera2D))]
public class CameraPlayerFollower : MonoBehaviour
{
	private ProCamera2D procamera2D;

	private void Start()
	{
		procamera2D = GetComponent<ProCamera2D>();
	}

	public void FollowPlayer(bool follow)
	{
		procamera2D.FollowHorizontal = follow;
		procamera2D.FollowVertical = follow;
	}
}
