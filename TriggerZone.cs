using UnityEngine;

public class TriggerZone : MonoBehaviour
{
	[SerializeField]
	private Vector2 size;

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(base.transform.position, size);
	}

	public bool Contains(Vector3 position)
	{
		Rect zero = Rect.zero;
		zero.size = size;
		zero.center = base.transform.position;
		return zero.Contains(position);
	}
}
