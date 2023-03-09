using System.Collections;
using UnityEngine;

public class SnowCultLevelQuadShot : AbstractProjectile
{
	private const float GROUND_Y = -240f;

	[SerializeField]
	private float popOutWarningTimeNormalized = 0.8f;

	[SerializeField]
	private Effect sparkEffect;

	[SerializeField]
	private Effect snowLandEffect;

	[SerializeField]
	private Effect snowPopOutEffect;

	[SerializeField]
	private SpriteRenderer deathPuff;

	[SerializeField]
	private SpriteRenderer rend;

	private LevelProperties.SnowCult.QuadShot properties;

	private DamageReceiver damageReceiver;

	private float speed;

	private float delay;

	private float angle;

	private string hazardDirectionInstruction;

	private int rowPosition;

	private float distanceBetween;

	private AbstractPlayerController targetPlayer;

	private Vector3 startPos;

	private Vector3 destPos;

	private float health;

	private bool isDead;

	private bool grounded;

	private bool running;

	private char id;

	protected override float DestroyLifetime => 0f;

	public virtual SnowCultLevelQuadShot Init(Vector3 startPos, Vector3 destPos, float speed, string hazardDirectionInstruction, LevelProperties.SnowCult.QuadShot properties, int rowPosition, float delay, float distanceBetween, AbstractPlayerController targetPlayer)
	{
		ResetLifetime();
		ResetDistance();
		((SnowCultLevel)Level.Current).OnYetiHitGround += WhaleDeath;
		id = ((rowPosition % 2 != 0) ? 'B' : 'A');
		base.animator.Play("Emerge" + id);
		base.transform.position = startPos;
		this.startPos = startPos;
		this.destPos = destPos;
		this.properties = properties;
		this.speed = speed;
		this.hazardDirectionInstruction = hazardDirectionInstruction;
		this.rowPosition = rowPosition;
		this.distanceBetween = distanceBetween;
		this.targetPlayer = targetPlayer;
		this.delay = delay;
		base.transform.localScale = new Vector3((this.rowPosition <= 1) ? 1 : (-1), 1f);
		StartCoroutine(move_to_launch_pos_cr());
		base.tag = "EnemyProjectile";
		return this;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void OnCollisionEnemy(GameObject hit, CollisionPhase phase)
	{
		if (grounded)
		{
			base.OnCollisionEnemy(hit, phase);
			if (phase != CollisionPhase.Exit && ((bool)hit.GetComponent<SnowCultLevelWhaleCollision>() || (bool)hit.GetComponent<SnowCultLevelQuadShot>()) && phase == CollisionPhase.Enter)
			{
				base.transform.localScale = new Vector3(Mathf.Sign(base.transform.position.x - hit.gameObject.transform.position.x), 1f);
				WhaleDeath();
			}
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (!grounded)
		{
			return;
		}
		health -= info.damage;
		if (!(health < 0f))
		{
			return;
		}
		if (running)
		{
			if (!base.dead)
			{
				Level.Current.RegisterMinionKilled();
				base.transform.localScale = new Vector3(MathUtils.PlusOrMinus(), 1f);
				rend.flipX = Rand.Bool();
				Dead();
			}
		}
		else
		{
			running = true;
			health = properties.hazardHealth;
		}
	}

	private IEnumerator move_to_launch_pos_cr()
	{
		float t = 0f;
		while (t < 1f / 3f)
		{
			yield return CupheadTime.WaitForSeconds(this, 1f / 24f);
			t += 1f / 24f;
			base.transform.position = Vector3.Lerp(startPos, destPos, Mathf.InverseLerp(0f, 1f / 3f, t));
		}
	}

	public void Shoot(float angle)
	{
		this.angle = angle;
		base.transform.localScale = new Vector3((angle < -90f) ? 1 : (-1), 1f);
		base.animator.Play("Launch" + id);
		sparkEffect.Create(base.transform.position);
		if (rowPosition % 2 == 0)
		{
			rend.sortingOrder = 3;
		}
		StartCoroutine(shoot_cr());
	}

	private IEnumerator shoot_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, delay);
		while (base.transform.position.y > -240f)
		{
			base.transform.position += (Vector3)MathUtils.AngleToDirection(angle) * speed * CupheadTime.FixedDelta;
			yield return new WaitForFixedUpdate();
		}
		base.transform.position = new Vector3(base.transform.position.x, -240f);
		StartCoroutine(run_away_cr());
		yield return null;
	}

	private IEnumerator run_away_cr()
	{
		YieldInstruction wait = new WaitForFixedUpdate();
		AnimationHelper animHelper = GetComponent<AnimationHelper>();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		base.tag = "Enemy";
		grounded = true;
		health = properties.groundHealth;
		base.animator.Play("HitGround" + id);
		SFX_SNOWCULT_QuadshotMinionStuckInGround();
		snowLandEffect.Create(new Vector3(base.transform.position.x, Level.Current.Ground));
		float t2 = (properties.hazardMoveDelay - delay) * popOutWarningTimeNormalized;
		while (t2 > 0f && !running)
		{
			t2 -= (float)CupheadTime.Delta;
			yield return null;
		}
		animHelper.Speed = 1.5f;
		t2 = (properties.hazardMoveDelay - delay) * (1f - popOutWarningTimeNormalized);
		while (t2 > 0f && !running)
		{
			t2 -= (float)CupheadTime.Delta;
			yield return null;
		}
		animHelper.Speed = 1f;
		running = true;
		health = properties.hazardHealth;
		float direction = 0f;
		switch (hazardDirectionInstruction)
		{
		case "L":
			direction = -1f;
			break;
		case "R":
			direction = 1f;
			break;
		case "F":
			direction = ((!(Mathf.Abs(base.transform.position.x - (float)Level.Current.Left) > Mathf.Abs(base.transform.position.x - (float)Level.Current.Right))) ? 1 : (-1));
			break;
		case "G":
		{
			float num = base.transform.position.x + ((float)(2 - rowPosition) - 0.5f) * distanceBetween;
			direction = ((!(Mathf.Abs(num - (float)Level.Current.Left) > Mathf.Abs(num - (float)Level.Current.Right))) ? 1 : (-1));
			break;
		}
		case "P":
			direction = ((!(base.transform.position.x - targetPlayer.transform.position.x > 0f)) ? 1 : (-1));
			break;
		}
		base.transform.localScale = new Vector3(0f - direction, 1f);
		base.animator.Play("PopOut" + id);
		SFX_SNOWCULT_QuadshotMinionFlipUp();
		snowPopOutEffect.Create(new Vector3(base.transform.position.x, Level.Current.Ground));
		yield return null;
		while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f / 6f)
		{
			yield return null;
		}
		health = properties.hazardHealth;
		rend.sortingOrder = 1;
		t2 = 0f;
		while (base.transform.position.x > (float)(Level.Current.Left - 200) && base.transform.position.x < (float)(Level.Current.Right + 200))
		{
			t2 = Mathf.Clamp(t2 + CupheadTime.FixedDelta * 2f, 0f, 1f);
			base.transform.position += Vector3.right * direction * properties.hazardSpeed * CupheadTime.FixedDelta * t2;
			yield return wait;
		}
		this.Recycle();
	}

	private void WhaleDeath()
	{
		Dead();
		rend.sortingLayerName = "Foreground";
		base.animator.Play("WhaleDeath" + id);
	}

	private void Dead()
	{
		StopAllCoroutines();
		deathPuff.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 360));
		deathPuff.flipX = Rand.Bool();
		deathPuff.flipY = Rand.Bool();
		base.animator.Play("Death");
		SFX_SNOWCULT_QuadshotMinionDie();
		GetComponent<Collider2D>().enabled = false;
	}

	private void aniEvent_Dead()
	{
		this.Recycle();
	}

	protected override void OnDestroy()
	{
		if ((bool)Level.Current)
		{
			((SnowCultLevel)Level.Current).OnYetiHitGround -= WhaleDeath;
		}
		base.OnDestroy();
	}

	private void SFX_SNOWCULT_QuadshotMinionStuckInGround()
	{
		AudioManager.Play("sfx_dlc_snowcult_p1_minion_stuckinground");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p1_minion_stuckinground");
	}

	private void SFX_SNOWCULT_QuadshotMinionFlipUp()
	{
		AudioManager.Play("sfx_dlc_snowcult_p1_minion_flipup");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p1_minion_flipup");
	}

	private void SFX_SNOWCULT_QuadshotMinionDie()
	{
		AudioManager.Play("sfx_dlc_snowcult_p1_minion_death_explode");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p1_minion_death_explode");
	}
}
