using System.Collections;
using UnityEngine;

public class LevelPlayerChaliceDashEffect : Effect
{
	private float t;

	private void Start()
	{
		StartCoroutine(MoveUntilTail());
	}

	private IEnumerator MoveUntilTail()
	{
		yield return new WaitForEndOfFrame();
		float xFacing = base.transform.parent.transform.localScale.x;
		int target = Animator.StringToHash(base.animator.GetLayerName(0) + ".Start");
		while (base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == target && base.transform.parent.transform.localScale.x == xFacing)
		{
			yield return null;
		}
		if (base.transform.parent.transform.localScale.x != xFacing)
		{
			base.transform.localPosition = new Vector3(0f - base.transform.localPosition.x, base.transform.localPosition.y);
		}
		base.transform.parent = null;
		base.transform.localScale = new Vector3(xFacing, 1f);
	}
}
