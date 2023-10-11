using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ConstrainWithinCollider : MonoBehaviour
{
	public Transform[] transformsToConstrain;

	private Collider _collider;

	public Collider collider
	{
		get
		{
			if (!_collider)
			{
				return _collider = GetComponent<Collider>();
			}
			return _collider;
		}
	}

	private void LateUpdate()
	{
		if (transformsToConstrain.IsNullOrEmpty())
		{
			return;
		}
		Transform[] array = transformsToConstrain;
		foreach (Transform transform in array)
		{
			if (!collider.ContainsPoint(transform.position))
			{
				transform.position = collider.ClosestPoint(transform.position);
			}
		}
	}

	public ConstrainWithinCollider SetData(params Transform[] transformsToConstrain)
	{
		this.transformsToConstrain = transformsToConstrain;
		return this;
	}
}
