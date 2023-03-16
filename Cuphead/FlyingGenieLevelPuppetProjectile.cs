using System.Collections;
using UnityEngine;

public class FlyingGenieLevelPuppetProjectile : BasicProjectile
{
	[SerializeField]
	private float _minRadius = 100f;

	[SerializeField]
	private float _maxRadius = 200f;

	[SerializeField]
	private Effect[] sparksBlue;

	[SerializeField]
	private Effect[] sparksPink;

	public float minRadius => _minRadius;

	public float maxRadius => _maxRadius;

	protected override void Start()
	{
		base.Start();
		StartCoroutine(spawn_spark_cr());
	}

	private IEnumerator spawn_spark_cr()
	{
		string[] pattern = new string[12]
		{
			"B", "P", "B", "P", "P", "B", "P", "B", "B", "P",
			"B", "P"
		};
		int patternIndex = Random.Range(0, pattern.Length);
		while (true)
		{
			Vector2 vector = base.baseTransform.position;
			Vector2 vector2 = new Vector2(Random.value * (float)(Rand.Bool() ? 1 : (-1)), Random.value * (float)(Rand.Bool() ? 1 : (-1)));
			Vector2 target = vector + vector2.normalized * Random.Range(minRadius, maxRadius);
			if (pattern[patternIndex] == "B")
			{
				sparksBlue[Random.Range(0, sparksBlue.Length)].Create(target);
			}
			else
			{
				sparksPink[Random.Range(0, sparksPink.Length)].Create(target);
			}
			patternIndex = (patternIndex + 1) % pattern.Length;
			yield return CupheadTime.WaitForSeconds(this, Random.Range(0.08f, 0.2f));
		}
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.color = new Color(1f, 0f, 0f, 1f);
		Gizmos.DrawWireSphere((Vector2)base.baseTransform.position, minRadius);
		Gizmos.color = new Color(0f, 0f, 1f, 1f);
		Gizmos.DrawWireSphere((Vector2)base.baseTransform.position, maxRadius);
	}

	protected override void Die()
	{
		base.Die();
		StopAllCoroutines();
	}
}
