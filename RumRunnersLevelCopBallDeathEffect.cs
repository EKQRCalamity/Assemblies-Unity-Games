using System.Collections.Generic;
using UnityEngine;

public class RumRunnersLevelCopBallDeathEffect : Effect
{
	[SerializeField]
	private Animator[] shrapnel;

	public override void Initialize(Vector3 position, Vector3 scale, bool randomR)
	{
		int value = Random.Range(0, base.animator.GetInteger("Count"));
		base.animator.SetInteger("Effect", value);
		Transform transform = base.transform;
		transform.position = position;
		transform.localScale = scale;
		if (randomR)
		{
			transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 8) * 45);
		}
		List<int> list = new List<int>();
		for (value = 0; value < 5; value++)
		{
			list.Add(value);
		}
		list.RemoveAt(Random.Range(0, list.Count));
		list.RemoveAt(Random.Range(0, list.Count));
		for (value = 0; value < 3; value++)
		{
			shrapnel[value].Play(list[value].ToString());
			shrapnel[value].transform.parent = null;
		}
	}
}
