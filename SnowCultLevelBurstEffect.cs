using System.Collections;
using UnityEngine;

public class SnowCultLevelBurstEffect : Effect
{
	private const float DIST_X_TO_MOVE = 127f;

	private const float Y_TO_SPAWN = 95f;

	private const float MOVE_SPEED = 150f;

	[SerializeField]
	private bool isSnowFall;

	[SerializeField]
	private bool isTypeA;

	[SerializeField]
	private SnowCultLevelBurstEffect typeA;

	[SerializeField]
	private SnowCultLevelBurstEffect typeB;

	private float startPosY;

	private float direction;

	public SnowCultLevelBurstEffect Create(Vector3 pos, float direction)
	{
		SnowCultLevelBurstEffect snowCultLevelBurstEffect = base.Create(pos) as SnowCultLevelBurstEffect;
		snowCultLevelBurstEffect.direction = direction;
		return snowCultLevelBurstEffect;
	}

	private void Start()
	{
		startPosY = base.transform.position.y;
		if (isSnowFall)
		{
			StartCoroutine(move_cr());
		}
	}

	private void SpawnEffect()
	{
		Vector3 pos = new Vector3(base.transform.position.x + 127f * direction, (!isSnowFall) ? 95f : startPosY);
		if (pos.x > -740f && pos.x < 740f)
		{
			if (isTypeA)
			{
				typeA.Create(pos, direction);
			}
			else
			{
				typeB.Create(pos, direction);
			}
		}
	}

	private IEnumerator move_cr()
	{
		while (base.transform.position.y > -360f)
		{
			base.transform.position += Vector3.down * 150f * CupheadTime.Delta;
			yield return null;
		}
		OnEffectComplete();
		yield return null;
	}
}
