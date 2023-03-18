using Framework.Managers;
using UnityEngine;

public class LookAtPenitent : MonoBehaviour
{
	private Transform target;

	public float radius;

	public float maxDistance;

	private void Start()
	{
	}

	private void Update()
	{
		if (Core.Logic.Penitent != null)
		{
			target = Core.Logic.Penitent.transform;
			Vector3 vector = target.position - base.transform.parent.position;
			float num = radius;
			float num2 = Vector2.Distance(target.position, base.transform.parent.position);
			if (num2 < maxDistance)
			{
				num = Mathf.Lerp(0f, radius, num2 / maxDistance);
			}
			base.transform.position = Vector3.Lerp(base.transform.position, base.transform.parent.position + vector.normalized * num, 0.2f);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(base.transform.parent.position, radius);
	}
}
