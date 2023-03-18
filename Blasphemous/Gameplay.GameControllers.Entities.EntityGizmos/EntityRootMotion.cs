using UnityEngine;

namespace Gameplay.GameControllers.Entities.EntityGizmos;

public class EntityRootMotion : MonoBehaviour
{
	public Entity Entity;

	public Vector3 GetPosition()
	{
		return (Entity.Status.Orientation != 0) ? GetReversePosition() : GetStraightPosition();
	}

	private Vector3 GetStraightPosition()
	{
		return base.transform.position;
	}

	private Vector3 GetReversePosition()
	{
		Vector3 result = new Vector3(base.transform.position.x - base.transform.localPosition.x * 2f, base.transform.position.y, 0f);
		return result;
	}
}
