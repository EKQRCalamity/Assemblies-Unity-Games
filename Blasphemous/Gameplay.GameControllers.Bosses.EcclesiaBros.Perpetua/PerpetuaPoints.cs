using UnityEngine;

namespace Gameplay.GameControllers.Bosses.EcclesiaBros.Perpetua;

public class PerpetuaPoints : MonoBehaviour
{
	public Transform centerPoint;

	public Vector2 GetCenterPos()
	{
		return centerPoint.position;
	}
}
