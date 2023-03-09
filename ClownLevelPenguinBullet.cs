using System.Collections;
using UnityEngine;

public class ClownLevelPenguinBullet : BasicProjectile
{
	[SerializeField]
	private Effect bulletFX;

	[SerializeField]
	private Transform root;

	protected override void Start()
	{
		base.Start();
		move = false;
		StartCoroutine(timer_cr());
	}

	private IEnumerator timer_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.3f);
		move = true;
		bulletFX.Create(root.transform.position).transform.SetEulerAngles(null, null, base.transform.eulerAngles.z - 90f);
		yield return null;
	}
}
