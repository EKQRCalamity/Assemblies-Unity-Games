using System.Collections;
using UnityEngine;

public class SnowCultLevelYetiLegs : BasicDamageDealingObject
{
	private const float RUN_SPEED = 1000f;

	private const float RUN_DELAY = 1f;

	[SerializeField]
	private SpriteRenderer rend;

	private void Start()
	{
		StartCoroutine(run_away_cr());
	}

	private IEnumerator run_away_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 1f);
		rend.sortingLayerName = "Player";
		rend.sortingOrder = -19;
		base.animator.Play("Run");
		SFX_SNOWCULT_YetiLegsWalkOff();
		for (int i = 0; i < 1000; i++)
		{
			base.transform.position += Vector3.left * Mathf.Sign(base.transform.localScale.x) * 1000f * CupheadTime.FixedDelta;
			yield return new WaitForFixedUpdate();
		}
		Object.Destroy(base.gameObject);
	}

	private void SFX_SNOWCULT_YetiLegsWalkOff()
	{
		AudioManager.Play("sfx_dlc_snowcult_p2_snowmonster_death_stompoffscreen");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_snowmonster_death_stompoffscreen");
	}
}
