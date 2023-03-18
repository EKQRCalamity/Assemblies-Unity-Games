using UnityEngine;

namespace Framework.Util;

public class PixelPivotMovement : MonoBehaviour
{
	[SerializeField]
	private Transform target;

	private void LateUpdate()
	{
		if (target == null)
		{
			RepositionAllChilds();
		}
		else
		{
			RepositionTarget();
		}
	}

	private void RepositionAllChilds()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			GOUtil.PixelPerfectPosition(child.gameObject);
		}
	}

	private void RepositionTarget()
	{
		GOUtil.PixelPerfectPosition(base.transform.gameObject);
	}
}
