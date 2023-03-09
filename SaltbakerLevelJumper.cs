using System.Collections;
using UnityEngine;

public class SaltbakerLevelJumper : AbstractProjectile
{
	private const float SWOOPER_DIVE_LENGTH = 0.375f;

	private const float JUMPER_INTRO_LENGTH = 5f / 12f;

	private const float JUMPER_GROUND_OFFSET = 68f;

	private const float SWOOPER_CEILING_OFFSET = -94f;

	private const float SWOOPER_FX_OFFSET = 27f;

	private const float JUMPER_ENTRANCE_Y_POS = 200f;

	private const float JUMP_LOOP_LENGTH = 5f / 12f;

	private const float SWOOPER_LOOP_EXIT_TIME = 0.65f;

	private const float JUMPER_LOOP_EXIT_TIME = 0.75f;

	private const float LEVEL_EDGE_OFFSET_SWOOPER = 75f;

	private const float LEVEL_EDGE_OFFSET_JUMPER = 260f;

	[SerializeField]
	private Animator FXbottom;

	private string[] FXanimNames = new string[3] { "Thin", "Medium", "Wide" };

	protected Vector3 aimPosition;

	private float levelEdgeOffset = 75f;

	protected SaltbakerLevel parent;

	protected float firstDelay;

	protected bool isSwooper;

	private CircleCollider2D coll;

	private int count;

	private float apexHeight;

	private float apexTime;

	private float initialFallDelay;

	private float jumpDelay;

	private new bool dead;

	public SaltbakerLevelJumper Create(Vector3 position, SaltbakerLevel parent, LevelProperties.Saltbaker.Swooper swooperProperties, LevelProperties.Saltbaker.Jumper jumperProperties, float firstDelay, bool isSwooper)
	{
		SaltbakerLevelJumper saltbakerLevelJumper = InstantiatePrefab<SaltbakerLevelJumper>();
		saltbakerLevelJumper.transform.position = position;
		if (isSwooper)
		{
			saltbakerLevelJumper.transform.position += Vector3.up * -94f;
		}
		saltbakerLevelJumper.count = ((!isSwooper) ? jumperProperties.numberFireJumpers : swooperProperties.numberFireSwoopers);
		saltbakerLevelJumper.apexHeight = ((!isSwooper) ? (jumperProperties.apexHeight - 68f) : (swooperProperties.apexHeight + -94f));
		saltbakerLevelJumper.apexTime = ((!isSwooper) ? jumperProperties.apexTime : swooperProperties.apexTime);
		saltbakerLevelJumper.initialFallDelay = ((!isSwooper) ? jumperProperties.initialFallDelay : swooperProperties.initialFallDelay);
		saltbakerLevelJumper.jumpDelay = ((!isSwooper) ? jumperProperties.jumpDelay : swooperProperties.jumpDelay);
		saltbakerLevelJumper.levelEdgeOffset = ((!isSwooper) ? 260f : 75f);
		saltbakerLevelJumper.parent = parent;
		saltbakerLevelJumper.firstDelay = firstDelay;
		saltbakerLevelJumper.isSwooper = isSwooper;
		saltbakerLevelJumper.coll = saltbakerLevelJumper.GetComponent<CircleCollider2D>();
		saltbakerLevelJumper.FXbottom.transform.parent = null;
		saltbakerLevelJumper.aimPosition = saltbakerLevelJumper.transform.position;
		if (saltbakerLevelJumper.isSwooper)
		{
			saltbakerLevelJumper.StartCoroutine(saltbakerLevelJumper.arc_cr());
		}
		else
		{
			saltbakerLevelJumper.GetComponent<SpriteRenderer>().sortingLayerName = "Player";
			saltbakerLevelJumper.StartCoroutine(saltbakerLevelJumper.fall_cr());
		}
		return saltbakerLevelJumper;
	}

	protected override void OnDieLifetime()
	{
	}

	public Vector3 GetAimPos()
	{
		return aimPosition;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private IEnumerator arc_cr()
	{
		AnimationHelper animHelper = GetComponent<AnimationHelper>();
		if (isSwooper)
		{
			base.animator.Play("SwooperIntro");
			yield return base.animator.WaitForAnimationToEnd(this, "SwooperIntro");
		}
		else
		{
			FXbottom.GetComponent<SpriteRenderer>().sortingLayerName = "Player";
			FXbottom.GetComponent<SpriteRenderer>().sortingOrder = -2;
		}
		float root = (float)(Level.Current.Left + Level.Current.Right) / 2f;
		yield return CupheadTime.WaitForSeconds(this, firstDelay);
		while (!dead)
		{
			if (isSwooper)
			{
				base.animator.Play("SwooperAntic");
				while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.888f)
				{
					yield return null;
				}
			}
			float t = 0f;
			float endPosY = ((!isSwooper) ? ((float)Level.Current.Ground + 68f) : (CupheadLevelCamera.Current.Bounds.yMax + -94f));
			aimPosition = new Vector3(Mathf.Clamp(PlayerManager.GetNext().center.x, (float)Level.Current.Left + levelEdgeOffset, (float)Level.Current.Right - levelEdgeOffset), endPosY);
			float offset = Mathf.Sign(root - aimPosition.x) * coll.bounds.size.x;
			bool foundPos = false;
			while (!foundPos)
			{
				if (aimPosition.x < (float)Level.Current.Left + levelEdgeOffset || aimPosition.x > (float)Level.Current.Right - levelEdgeOffset)
				{
					aimPosition = base.transform.position;
					foundPos = true;
				}
				else if (parent.IsPositionAvailable(aimPosition, this))
				{
					foundPos = true;
				}
				else
				{
					aimPosition.x += offset;
				}
			}
			float x = aimPosition.x - base.transform.position.x;
			float y = aimPosition.y - base.transform.position.y;
			float apexTime2 = apexTime * apexTime;
			float g = -2f * apexHeight / apexTime2;
			float viY = 2f * apexHeight / apexTime;
			float viX2 = viY * viY;
			float sqrtRooted = viX2 + 2f * g * y;
			float tEnd2 = (0f - viY + Mathf.Sqrt(sqrtRooted)) / g;
			float tEnd3 = (0f - viY - Mathf.Sqrt(sqrtRooted)) / g;
			float tEnd = Mathf.Max(tEnd2, tEnd3);
			float velocityX = x / tEnd;
			if (isSwooper)
			{
				viY = 0f - viY;
			}
			Vector3 vel = new Vector3(velocityX, viY);
			base.animator.SetInteger("ArcWidth", Mathf.Clamp((int)(Mathf.Abs(velocityX) / 250f), 0, 2));
			int jumpLoopHash = Animator.StringToHash(base.animator.GetLayerName(0) + "." + ((!isSwooper) ? "Jumper" : "Swooper") + "JumpLoop");
			float animTimeOnLand = -1f;
			base.animator.SetInteger("Variant", Random.Range(0, 2));
			if (isSwooper)
			{
				base.transform.localScale = new Vector3(Mathf.Sign(velocityX), 1f);
				yield return base.animator.WaitForAnimationToEnd(this, "SwooperAntic");
				tEnd -= 0.375f;
			}
			else
			{
				base.animator.SetTrigger("StartJumperAntic");
				yield return base.animator.WaitForAnimationToStart(this, "JumperAntic");
				base.transform.localScale = new Vector3(Mathf.Sign(0f - velocityX), 1f);
				yield return base.animator.WaitForAnimationToEnd(this, "JumperAntic");
			}
			FXbottom.transform.position = base.transform.position + Vector3.up * ((!isSwooper) ? (-20f) : 27f);
			FXbottom.transform.localScale = new Vector3(base.transform.localScale.x, (!isSwooper) ? 1 : (-1));
			FXbottom.Play(FXanimNames[base.animator.GetInteger("ArcWidth")], 0, 0f);
			bool stillMoving = true;
			YieldInstruction wait = new WaitForFixedUpdate();
			while (stillMoving)
			{
				if (animTimeOnLand < 0f && base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == jumpLoopHash)
				{
					float num = tEnd / (5f / 12f);
					animTimeOnLand = (num + base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime) % 1f;
					float num2 = animTimeOnLand - ((!isSwooper) ? 0.75f : 0.65f);
					float num3 = num - num2;
					animHelper.Speed = num3 / num;
				}
				if (isSwooper)
				{
					vel.y -= g * CupheadTime.FixedDelta;
				}
				else
				{
					vel.y += g * CupheadTime.FixedDelta;
				}
				base.transform.Translate(vel * CupheadTime.FixedDelta);
				tEnd -= CupheadTime.FixedDelta;
				yield return wait;
				t += CupheadTime.FixedDelta;
				if (!(t > apexTime))
				{
					continue;
				}
				if (isSwooper)
				{
					if (base.transform.position.y >= endPosY)
					{
						stillMoving = false;
					}
					if (tEnd <= 0f)
					{
						base.animator.SetBool("EndJump", value: true);
						animHelper.Speed = 1f;
					}
				}
				else if (base.transform.position.y <= endPosY)
				{
					stillMoving = false;
					base.animator.SetBool("EndJump", value: true);
					animHelper.Speed = 1f;
				}
			}
			base.transform.SetPosition(null, endPosY);
			if (!isSwooper)
			{
				yield return base.animator.WaitForAnimationToEnd(this, "JumperJumpLoop");
			}
			base.animator.SetBool("EndJump", value: false);
			if (!isSwooper)
			{
				yield return base.animator.WaitForAnimationToStart(this, "JumperIdle");
			}
			if (!dead)
			{
				yield return CupheadTime.WaitForSeconds(this, jumpDelay);
			}
			yield return null;
		}
	}

	private void AniEvent_FlipX()
	{
		base.transform.localScale = new Vector3(0f - base.transform.localScale.x, 1f);
	}

	private IEnumerator fall_cr()
	{
		base.animator.Play("JumperFallLoop");
		float endPosY = (float)Level.Current.Ground + 68f;
		float apexTime2 = apexTime * apexTime;
		float g = -2f * apexHeight / apexTime2;
		Vector3 vel = Vector3.zero;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (base.transform.position.y > 200f)
		{
			vel.y += g * CupheadTime.FixedDelta;
			base.transform.Translate(vel * CupheadTime.FixedDelta);
			yield return wait;
		}
		base.animator.SetTrigger("StartJumperIntro");
		while (base.transform.position.y > endPosY)
		{
			yield return wait;
			vel.y += g * CupheadTime.FixedDelta;
			base.transform.Translate(vel * CupheadTime.FixedDelta);
		}
		base.transform.SetPosition(null, endPosY);
		yield return base.animator.WaitForAnimationToStart(this, "JumperIdle");
		StartCoroutine(arc_cr());
		yield return null;
	}

	public new void Die()
	{
		dead = true;
		base.animator.SetTrigger("Die");
		GetComponent<Collider2D>().enabled = false;
	}

	public void AniEvent_DeathComplete()
	{
		Object.Destroy(FXbottom.gameObject);
		Object.Destroy(base.gameObject);
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		if (coll != null)
		{
			Gizmos.DrawWireSphere(aimPosition, coll.radius);
		}
	}
}
