using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce.Attack;

public class TestRicochetRay : MonoBehaviour
{
	public int numBounces = 10;

	public LayerMask mask;

	private RaycastHit2D[] results;

	private void Start()
	{
		results = new RaycastHit2D[1];
	}

	public void Update()
	{
		if (!Core.ready || Core.Logic.Penitent == null)
		{
			return;
		}
		Vector2[] array = new Vector2[numBounces];
		Vector2[] array2 = new Vector2[numBounces];
		Vector2[] array3 = new Vector2[numBounces];
		ref Vector2 reference = ref array[0];
		reference = base.transform.position;
		ref Vector2 reference2 = ref array2[0];
		reference2 = Core.Logic.Penitent.transform.position - base.transform.position;
		for (int i = 0; i < numBounces && ThrowRay(array[i], array2[i]); i++)
		{
			ref Vector2 reference3 = ref array3[i];
			reference3 = results[0].point;
			GizmoExtensions.DrawDebugCross(array3[i], Color.green, 0.5f);
			if (i + 1 < numBounces)
			{
				ref Vector2 reference4 = ref array2[i + 1];
				reference4 = CalculateBounceDirection(array3[i] - array[i], results[0]);
				ref Vector2 reference5 = ref array[i + 1];
				reference5 = array3[i] + array2[i + 1] * 0.01f;
			}
		}
	}

	private Vector2 CalculateBounceDirection(Vector2 direction, RaycastHit2D hit)
	{
		return Vector3.Reflect(direction, hit.normal).normalized;
	}

	private bool ThrowRay(Vector2 startPoint, Vector2 direction)
	{
		bool result = false;
		if (Physics2D.RaycastNonAlloc(startPoint, direction, results, 100f, mask) > 0)
		{
			Debug.DrawRay(startPoint, direction.normalized * results[0].distance, Color.red);
			result = true;
		}
		else
		{
			Debug.DrawRay(startPoint, direction.normalized * 100f, Color.yellow);
		}
		return result;
	}
}
