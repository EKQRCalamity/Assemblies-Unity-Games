using System.Collections;
using UnityEngine;

public class SaltbakerLevelCutter : AbstractProjectile
{
	private const float SCREEN_EDGE_OFFSET = 50f;

	private float speed;

	private bool goingLeft;

	private bool turning;

	private int sfxID;

	private LevelProperties.Saltbaker.Cutter properties;

	[SerializeField]
	private GameObject dustFX;

	[SerializeField]
	private SpriteRenderer rend;

	protected override float DestroyLifetime => 0f;

	public SaltbakerLevelCutter Create(Vector3 position, float speed, bool goingLeft, int id)
	{
		SaltbakerLevelCutter saltbakerLevelCutter = InstantiatePrefab<SaltbakerLevelCutter>();
		saltbakerLevelCutter.transform.position = position;
		saltbakerLevelCutter.speed = speed;
		saltbakerLevelCutter.goingLeft = goingLeft;
		saltbakerLevelCutter.transform.localScale = new Vector3((!goingLeft) ? 1 : (-1), 1f);
		if (id == 1)
		{
			saltbakerLevelCutter.transform.position += Vector3.down * 5f;
		}
		saltbakerLevelCutter.sfxID = id;
		saltbakerLevelCutter.SFX_SALTBAKER_P3_PizzaWheel_Loop(id);
		saltbakerLevelCutter.rend.sortingOrder = id;
		return saltbakerLevelCutter;
	}

	private void AniEvent_StartMove()
	{
		StartCoroutine(move_cr());
		dustFX.transform.parent = null;
		dustFX.SetActive(value: true);
	}

	private void AniEvent_Variation()
	{
		if (((goingLeft && base.transform.position.x > (float)Level.Current.Left + 50f + 100f) || (!goingLeft && base.transform.position.x < (float)Level.Current.Right - 50f - 100f)) && Random.Range(0, 4) == 0)
		{
			base.animator.SetTrigger("Variation");
		}
	}

	private void AniEvent_ChangeDirection()
	{
		goingLeft = !goingLeft;
	}

	private void AniEvent_CompleteTurn()
	{
		base.transform.localScale = new Vector3(0f - base.transform.localScale.x, 1f);
		turning = false;
	}

	private void AniEvent_CompleteSink()
	{
		Object.Destroy(base.gameObject);
	}

	public void Sink()
	{
		base.animator.SetBool("Sink", value: true);
		SFX_SALTBAKER_P3_PizzaWheel_Dive(sfxID);
	}

	private IEnumerator move_cr()
	{
		float left = (float)Level.Current.Left + 50f;
		float right = (float)Level.Current.Right - 50f;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			Vector3 dir = ((!goingLeft) ? Vector3.right : Vector3.left);
			base.transform.position += dir * speed * CupheadTime.FixedDelta;
			if (!turning && ((goingLeft && base.transform.position.x < left) || (!goingLeft && base.transform.position.x > right)))
			{
				turning = true;
				base.animator.Play("Turn");
			}
			yield return wait;
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private void SFX_SALTBAKER_P3_PizzaWheel_Loop(int loopNumber)
	{
		string key = "sfx_dlc_saltbaker_p3_pizzawheel_movement_loop_" + (loopNumber + 1);
		AudioManager.PlayLoop(key);
		emitAudioFromObject.Add(key);
	}

	private void SFX_SALTBAKER_P3_PizzaWheel_Dive(int loopNumber)
	{
		AudioManager.Stop("sfx_dlc_saltbaker_p3_pizzawheel_movement_loop_" + (loopNumber + 1));
	}

	private void AnimationEvent_SFX_SALTBAKER_P3_RunnerWheel_Spawn()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p3_pizzawheel_spawn");
		emitAudioFromObject.Add("sfx_dlc_saltbaker_p3_pizzawheel_spawn");
	}
}
