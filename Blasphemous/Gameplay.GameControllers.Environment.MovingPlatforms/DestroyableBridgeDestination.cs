using UnityEngine;

namespace Gameplay.GameControllers.Environment.MovingPlatforms;

public class DestroyableBridgeDestination : MonoBehaviour
{
	private void Update()
	{
		base.gameObject.SetActive(value: false);
	}
}
