using System.Collections;
using UnityEngine;

public class SaltbakerLevelDough : SaltbakerLevelPhaseOneProjectile
{
	private const float GROUND_OFFSET = 50f;

	private float speedX;

	private float speedY;

	private float gravity;

	private float hp;

	private bool fromLeft;

	private DamageReceiver damageReceiver;

	[SerializeField]
	private Collider2D coll;

	[SerializeField]
	private Effect dustEffect;

	[SerializeField]
	private Effect debris;

	private string[] clipNames = new string[3] { "Elephant", "Lion", "Camel" };

	private int animalType;

	public virtual SaltbakerLevelDough Init(Vector3 startPos, float speedX, float speedY, float gravity, float hp, int sortingOrder, int animalType)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = startPos;
		this.speedX = speedX;
		this.speedY = speedY;
		this.gravity = gravity;
		fromLeft = speedX > 0f;
		this.hp = hp;
		Jump();
		GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
		this.animalType = animalType;
		base.animator.Play(clipNames[animalType] + "Up");
		return this;
	}

	protected override void Start()
	{
		base.Start();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
	}

	protected override bool SparksFollow()
	{
		return Rand.Bool();
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		hp -= info.damage;
		if (hp <= 0f)
		{
			Die();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private void Jump()
	{
		StartCoroutine(jump_cr());
	}

	private IEnumerator jump_cr()
	{
		base.animator.Play(Random.Range(0, 4).ToString(), 1, 0f);
		base.transform.localScale = new Vector3(Mathf.Sign(speedX), 1f);
		float velocityX = speedX;
		float velocityY = speedY;
		float sizeX = GetComponent<Collider2D>().bounds.size.x;
		float sizeY = GetComponent<Collider2D>().bounds.size.y;
		float ground = (float)Level.Current.Ground + sizeY / 2f + 50f;
		bool jumping = false;
		bool goingUp2 = false;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			velocityY = speedY;
			velocityX = speedX;
			jumping = true;
			goingUp2 = true;
			bool arcTriggered = false;
			base.transform.AddPosition(velocityX * CupheadTime.FixedDelta, velocityY * CupheadTime.FixedDelta);
			while (jumping)
			{
				velocityY -= gravity * CupheadTime.FixedDelta;
				base.transform.AddPosition(velocityX * CupheadTime.FixedDelta, velocityY * CupheadTime.FixedDelta);
				HandleShadow(0f, 0f);
				if (goingUp2 && !arcTriggered && velocityY <= gravity * 4f * CupheadTime.FixedDelta)
				{
					base.animator.SetTrigger("Arc");
					arcTriggered = true;
				}
				if (velocityY < 0f && goingUp2)
				{
					goingUp2 = false;
					arcTriggered = false;
				}
				if (velocityY < 0f && jumping && base.transform.position.y - velocityY * CupheadTime.FixedDelta <= ground)
				{
					jumping = false;
					base.transform.position = new Vector3(base.transform.position.x, ground);
					HandleShadow(0f, 0f);
					base.animator.SetTrigger("Bounce");
				}
				if ((base.transform.position.x < (float)Level.Current.Left - sizeX && !fromLeft) || (base.transform.position.x > (float)Level.Current.Right + sizeX && fromLeft))
				{
					Die();
				}
				yield return wait;
			}
			yield return base.animator.WaitForAnimationToStart(this, clipNames[animalType] + "Bounce");
			while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.75f)
			{
				yield return null;
			}
		}
	}

	private void AniEvent_SpawnDustCloud()
	{
		dustEffect.Create(new Vector3(base.transform.position.x, Level.Current.Ground));
	}

	protected override void Die()
	{
		StopAllCoroutines();
		coll.enabled = false;
		shadow.enabled = false;
		int num = Random.Range(1, 4);
		for (int i = 0; i < num; i++)
		{
			debris.Create(base.transform.position + (Vector3)MathUtils.RandomPointInUnitCircle() * 20f);
		}
		int num2 = Random.Range(1, 3);
		base.animator.Play("Death_" + num2);
		base.transform.localScale = new Vector3(MathUtils.PlusOrMinus(), (num2 >= 2) ? 1 : MathUtils.PlusOrMinus());
		if (num2 < 2)
		{
			base.transform.eulerAngles = new Vector3(0f, 0f, Random.Range(0, 360));
		}
		base.animator.Update(0f);
	}

	private void AnimationEvent_SFX_SALTBAKER_CookieBounce()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_cookiebounce");
		emitAudioFromObject.Add("sfx_dlc_saltbaker_p1_cookiebounce");
	}

	private void AnimationEvent_SFX_SALTBAKER_Cookie_AnimalCamel()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_cookie_animalcamel");
		emitAudioFromObject.Add("sfx_dlc_saltbaker_p1_cookie_animalcamel");
	}

	private void AnimationEvent_SFX_SALTBAKER_Cookie_AnimalLion()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_cookie_animalLion");
		emitAudioFromObject.Add("sfx_dlc_saltbaker_p1_cookie_animalLion");
	}

	private void AnimationEvent_SFX_SALTBAKER_Cookie_AnimalElephant()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_cookie_animalElephant");
		emitAudioFromObject.Add("sfx_dlc_saltbaker_p1_cookie_animalElephant");
	}
}
