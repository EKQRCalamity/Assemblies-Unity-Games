using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Gizmos;

public class RootMotionDriver : MonoBehaviour
{
	public Vector3 ReversePosition
	{
		get
		{
			Vector3 position = base.transform.position;
			Vector3 localPosition = base.transform.localPosition;
			float x = position.x - localPosition.x * 2f;
			return new Vector3(x, position.y, 0f);
		}
	}

	public Vector3 FlipedPosition
	{
		get
		{
			Vector3 localPosition = base.transform.localPosition;
			Vector2 vector = new Vector2(0f - localPosition.x, localPosition.y);
			Vector3 vector2 = base.transform.TransformPoint(vector);
			return new Vector2(vector2.x, vector2.y - localPosition.y);
		}
	}

	private void OnDrawGizmosSelected()
	{
		UnityEngine.Gizmos.color = Color.yellow;
		UnityEngine.Gizmos.DrawSphere(base.transform.position, 0.1f);
		UnityEngine.Gizmos.color = Color.green;
		UnityEngine.Gizmos.DrawSphere(FlipedPosition, 0.1f);
		UnityEngine.Gizmos.color = Color.blue;
		UnityEngine.Gizmos.DrawSphere(ReversePosition, 0.1f);
	}
}
