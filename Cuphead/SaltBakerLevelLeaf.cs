using System;
using System.Collections;
using UnityEngine;

public class SaltBakerLevelLeaf : AbstractProjectile
{
	private float xTime;

	private float xDistance;

	private float yGravity;

	private MinMax ySpeedMinMax;

	private SaltbakerLevelSaltbaker parent;

	private int animID;

	[SerializeField]
	private BoxCollider2D boxColl;

	public virtual SaltBakerLevelLeaf Init(Vector3 pos, float xTime, float xDistance, float yGravity, MinMax ySpeed, SaltbakerLevelSaltbaker parent, int animID)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = pos;
		this.xDistance = xDistance;
		this.xTime = xTime;
		ySpeedMinMax = ySpeed;
		this.yGravity = yGravity;
		Move();
		this.parent = parent;
		this.parent.OnDeathEvent += Death;
		this.animID = animID;
		return this;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	private void Move()
	{
		StartCoroutine(move_cr());
	}

	private IEnumerator move_cr()
	{
		float ground = Level.Current.Ground;
		YieldInstruction wait = new WaitForFixedUpdate();
		float val = 0f;
		float yVal = 0f;
		float xVal = 0f;
		float t = 0f;
		float startX = base.transform.position.x;
		float endX = base.transform.position.x + xDistance;
		AnimationHelper animationHelper = GetComponent<AnimationHelper>();
		animationHelper.Speed = 0f;
		bool dirRight = true;
		while (base.transform.position.y > ground)
		{
			val = t / xTime;
			xVal = EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, 0f, 1f, val);
			yVal = ((!(val < 0.5f)) ? (1f - val) : val);
			float ySpeed = ySpeedMinMax.GetFloatAt(yVal);
			float yPos = base.transform.position.y - (ySpeed + yGravity) * CupheadTime.FixedDelta;
			string animName = Convert.ToChar(65 + animID).ToString();
			t += CupheadTime.FixedDelta;
			base.transform.SetPosition(Mathf.Lerp(startX, endX, xVal), yPos);
			base.animator.Play(animName, 0, val / 2f + ((!dirRight) ? 0.5f : 0f));
			if (t >= xTime)
			{
				t = 0f;
				endX = startX;
				startX = base.transform.position.x;
				dirRight = !dirRight;
			}
			yield return wait;
		}
		boxColl.enabled = false;
		animationHelper.Speed = 1f;
		base.animator.SetTrigger("Die");
		yield return base.animator.WaitForAnimationToStart(this, "None");
		this.Recycle();
		yield return null;
	}

	private void Death()
	{
		this.Recycle();
	}

	protected override void OnDestroy()
	{
		parent.OnDeathEvent -= Death;
		base.OnDestroy();
	}
}
