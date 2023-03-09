using System.Collections;
using UnityEngine;

public class FlyingGenieLevelTinyMarionette : AbstractCollidableObject
{
	[SerializeField]
	private BasicProjectile projectile;

	[SerializeField]
	private BasicProjectile pinkProjectile;

	[SerializeField]
	private Effect shootFX;

	[SerializeField]
	private Transform shootRoot;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private float hp;

	private bool turningDown;

	private bool isClockwise;

	private bool startedDown;

	private bool hasStarted;

	private bool isDead;

	private int bulletMainIndex;

	private int bulletIndex;

	private LevelProperties.FlyingGenie.Scan properties;

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		damageReceiver.enabled = false;
	}

	private void FixedUpdate()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		hp -= info.damage;
		if (hp < 0f && !isDead)
		{
			isDead = true;
			Die();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public void Activate(Vector3 endPos, LevelProperties.FlyingGenie.Scan properties, bool movingClockwise)
	{
		this.properties = properties;
		hp = properties.miniHP;
		isClockwise = movingClockwise;
		StartCoroutine(tiny_marionette(endPos));
	}

	private IEnumerator bounce_marionette_cr()
	{
		float t = 0f;
		float time = 0.5f;
		float start = base.transform.position.y;
		float end = base.transform.position.y + 100f;
		while (t < time)
		{
			TransformExtensions.SetPosition(y: Mathf.Lerp(start, end, EaseUtils.Ease(EaseUtils.EaseType.easeOutBounce, 0f, 1f, t / time)), transform: base.transform);
			t += (float)CupheadTime.Delta;
			yield return null;
		}
	}

	private IEnumerator tiny_marionette(Vector3 endPos)
	{
		StartCoroutine(bounce_marionette_cr());
		yield return base.animator.WaitForAnimationToEnd(this, "Puppet_Intro");
		damageReceiver.enabled = true;
		float t = 0f;
		float time = properties.movementSpeed;
		Vector3 start = base.transform.position;
		yield return CupheadTime.WaitForSeconds(this, 0.5f);
		while (t < time)
		{
			float val = EaseUtils.Ease(EaseUtils.EaseType.easeInSine, 0f, 1f, t / time);
			base.transform.position = Vector3.Lerp(start, endPos, val);
			t += (float)CupheadTime.Delta;
			yield return new WaitForFixedUpdate();
		}
		yield return CupheadTime.WaitForSeconds(this, 1f);
		startedDown = Rand.Bool();
		turningDown = !startedDown;
		base.animator.SetBool("OnTurningDown", turningDown);
		string dirString = ((!startedDown) ? "Up_" : "Down_");
		base.animator.SetTrigger("OnStartCycle");
		base.animator.SetBool("IsDown", startedDown);
		bulletMainIndex = Random.Range(0, properties.bulletString.Length);
		string[] bulletString2 = properties.bulletString[bulletMainIndex].Split(',');
		bulletIndex = Random.Range(0, bulletString2.Length);
		yield return base.animator.WaitForAnimationToEnd(this, dirString + "Warning_Shoot");
		while (true)
		{
			bulletString2 = properties.bulletString[bulletMainIndex].Split(',');
			yield return CupheadTime.WaitForSeconds(this, properties.shootDelay);
			base.animator.SetTrigger("OnShoot");
			yield return base.animator.WaitForAnimationToEnd(this);
			if (bulletIndex < bulletString2.Length - 1)
			{
				bulletIndex++;
			}
			else
			{
				bulletMainIndex = (bulletMainIndex + 1) % properties.bulletString.Length;
				bulletIndex = 0;
			}
			yield return null;
		}
	}

	private void ShootBullet(Vector3 pos, float rotation)
	{
		string[] array = properties.bulletString[bulletMainIndex].Split(',');
		if (array[bulletIndex][0] == 'P')
		{
			pinkProjectile.Create(pos, rotation + 90f, properties.bulletSpeed);
		}
		else
		{
			projectile.Create(pos, rotation + 90f, properties.bulletSpeed);
		}
	}

	private void AniEventCheckFlip()
	{
		if (hasStarted)
		{
			base.transform.SetScale(base.transform.localScale.x * -1f);
			turningDown = !turningDown;
			base.animator.SetBool("OnTurningDown", turningDown);
			return;
		}
		hasStarted = true;
		if (!isClockwise && !startedDown)
		{
			base.transform.SetScale(base.transform.localScale.x * -1f);
		}
		else if (isClockwise && startedDown)
		{
			base.transform.SetScale(base.transform.localScale.x * -1f);
		}
	}

	private void AniEventShoot()
	{
		Effect effect = shootFX.Create(shootRoot.transform.position);
		AudioManager.Play("genie_puppetsmall_shoot");
		emitAudioFromObject.Add("genie_puppetsmall_shoot");
		effect.transform.SetEulerAngles(null, null, shootRoot.transform.eulerAngles.z);
		ShootBullet(shootRoot.transform.position, shootRoot.transform.eulerAngles.z);
	}

	public void Die()
	{
		base.animator.SetTrigger("OnDeath");
		AudioManager.Play("genie_puppetsmall_death");
		emitAudioFromObject.Add("genie_puppetsmall_death");
		StopAllCoroutines();
		StartCoroutine(dead_move_cr());
	}

	private IEnumerator dead_move_cr()
	{
		float t = 0f;
		float timer = 0.5f;
		float downTimer = 0.5f;
		float start = base.transform.position.y;
		float end = 660f;
		float downEnd = base.transform.position.y - 50f;
		GetComponent<LevelBossDeathExploder>().StartExplosion();
		yield return base.animator.WaitForAnimationToEnd(this, "Death_Start");
		while (t < downTimer)
		{
			TransformExtensions.SetPosition(y: Mathf.Lerp(start, downEnd, EaseUtils.Ease(EaseUtils.EaseType.easeInSine, 0f, 1f, t / downTimer)), transform: base.transform);
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		t = 0f;
		start = base.transform.position.y;
		while (t < timer)
		{
			TransformExtensions.SetPosition(y: Mathf.Lerp(start, end, EaseUtils.Ease(EaseUtils.EaseType.easeInSine, 0f, 1f, t / timer)), transform: base.transform);
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		Object.Destroy(base.gameObject);
		yield return null;
	}

	private void SoundPuppetSmallEnterPuppet()
	{
		AudioManager.Play("genie_puppetsmall_enter_puppetsmall");
		emitAudioFromObject.Add("genie_puppetsmall_enter_puppetsmall");
	}

	private void SoundPuppetSmallDance()
	{
		AudioManager.Play("genie_puppetsmall_move");
		emitAudioFromObject.Add("genie_puppetsmall_move");
	}

	private void SoundPuppetShootWarning()
	{
		AudioManager.Play("genie_puppetsmall_shootwarning");
		emitAudioFromObject.Add("genie_puppetsmall_shootwarning");
	}

	private void SoundPuppetWarningShot()
	{
		AudioManager.Play("genie_puppetsmall_warningshot");
		emitAudioFromObject.Add("genie_puppetsmall_warningshot");
	}
}
