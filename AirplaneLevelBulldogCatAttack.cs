using System.Collections;
using UnityEngine;

public class AirplaneLevelBulldogCatAttack : LevelProperties.Airplane.Entity
{
	[SerializeField]
	private AirplaneLevelBulldogPlane main;

	[SerializeField]
	private BasicProjectile projectile;

	[SerializeField]
	private Transform root;

	private DamageDealer damageDealer;

	private int count;

	public bool isAttacking { get; private set; }

	private void Start()
	{
		damageDealer = DamageDealer.NewEnemy();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	public override void LevelInit(LevelProperties.Airplane properties)
	{
		base.LevelInit(properties);
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	public void StartCat(Vector2 pos)
	{
		count = Random.Range(0, 3);
		isAttacking = true;
		base.transform.localScale = new Vector3(Mathf.Sign(pos.x), 1f);
		base.transform.position = pos;
		base.animator.SetBool("Exit", value: false);
		StartCoroutine(cat_cr());
	}

	private IEnumerator cat_cr()
	{
		LevelProperties.Airplane.Triple p = base.properties.CurrentState.triple;
		base.animator.Play("Intro");
		AudioManager.FadeSFXVolume("sfx_dlc_dogfight_p1_catattack_hover", 1E-05f, 1E-05f);
		AudioManager.Play("sfx_dlc_dogfight_p1_bulldog_whistle_in");
		emitAudioFromObject.Add("sfx_dlc_dogfight_p1_bulldog_whistle_in");
		yield return base.animator.WaitForAnimationToStart(this, "IntroLoop");
		yield return CupheadTime.WaitForSeconds(this, p.initialDelay);
		base.animator.SetTrigger("Continue");
		AudioManager.PlayLoop("sfx_dlc_dogfight_p1_catattack_hover");
		emitAudioFromObject.Add("sfx_dlc_dogfight_p1_catattack_hover");
		AudioManager.FadeSFXVolume("sfx_dlc_dogfight_p1_catattack_hover", 0.15f, 0.5f);
		AudioManager.Play("sfx_dlc_dogfight_p1_catattack_enter");
		emitAudioFromObject.Add("sfx_dlc_dogfight_p1_catattack_enter");
		yield return base.animator.WaitForAnimationToStart(this, "Idle");
		yield return CupheadTime.WaitForSeconds(this, p.shootWarning);
		SFX_DOGFIGHT_Cat_Shoot();
		base.animator.Play("ShootA");
		base.animator.Update(0f);
		yield return base.animator.WaitForAnimationToEnd(this, "ShootA");
		yield return CupheadTime.WaitForSeconds(this, p.delayAfterFirst.RandomFloat());
		SFX_DOGFIGHT_Cat_Shoot();
		base.animator.Play("ShootB");
		base.animator.Update(0f);
		yield return base.animator.WaitForAnimationToEnd(this, "ShootB");
		yield return CupheadTime.WaitForSeconds(this, p.delayAfterSecond.RandomFloat());
		SFX_DOGFIGHT_Cat_Shoot();
		base.animator.Play("ShootA");
		base.animator.Update(0f);
		yield return base.animator.WaitForAnimationToEnd(this, "ShootA");
		yield return CupheadTime.WaitForSeconds(this, p.shootRecovery);
		base.animator.SetBool("Exit", value: true);
		AudioManager.FadeSFXVolume("sfx_dlc_dogfight_p1_catattack_hover", 0f, 0.5f);
		AudioManager.Play("sfx_dlc_dogfight_p1_catattack_leave");
		emitAudioFromObject.Add("sfx_dlc_dogfight_p1_catattack_leave");
		yield return base.animator.WaitForAnimationToEnd(this, "Exit");
		yield return CupheadTime.WaitForSeconds(this, p.returnDelay);
		isAttacking = false;
	}

	public void EarlyExit()
	{
		StopAllCoroutines();
		StartCoroutine(early_exit_cr());
	}

	private IEnumerator early_exit_cr()
	{
		base.animator.SetBool("Exit", value: true);
		yield return base.animator.WaitForAnimationToStart(this, "None");
		yield return CupheadTime.WaitForSeconds(this, base.properties.CurrentState.triple.returnDelay);
		isAttacking = false;
	}

	private void AniEvent_Shoot()
	{
		LevelProperties.Airplane.Triple triple = base.properties.CurrentState.triple;
		float num = triple.attackAngleRange.RandomFloat();
		float num2 = ((!(base.transform.localScale.x < 0f)) ? 180f : 0f);
		BasicProjectile basicProjectile = projectile.Create(root.position, num2 + num, triple.bulletSpeed);
		basicProjectile.transform.localScale = new Vector3(1f, (!(num2 > 0f)) ? 1 : (-1));
		Animator component = basicProjectile.GetComponent<Animator>();
		component.Play((count % 3).ToString(), 0, Random.Range(0.375f, 0.75f));
		component.Update(0f);
		count++;
		base.animator.Play(Random.Range(0, 4).ToString(), 1, 0f);
		base.animator.Update(0f);
	}

	private void OnDisable()
	{
		GetComponent<HitFlash>().StopAllCoroutinesWithoutSettingScale();
		GetComponent<SpriteRenderer>().color = Color.black;
	}

	private void AniEvent_IntroFX()
	{
		base.animator.Play("IntroFX", 1);
		base.animator.Update(0f);
	}

	private void AniEvent_FlashA()
	{
		base.animator.Play("FlashA", 2);
		base.animator.Update(0f);
	}

	private void AniEvent_FlashB()
	{
		base.animator.Play("FlashB", 2);
		base.animator.Update(0f);
	}

	private void AniEvent_EmberA()
	{
		base.animator.Play("EmberA", 3);
	}

	private void AniEvent_EmberB()
	{
		base.animator.Play("EmberB", 3);
	}

	private void SFX_DOGFIGHT_Cat_Shoot()
	{
		AudioManager.Play("sfx_dlc_dogfight_catgun_shoot");
		emitAudioFromObject.Add("sfx_dlc_dogfight_catgun_shoot");
	}

	private void SFX_DOGFIGHT_Cat_StartMeow()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_catgunmeow");
		emitAudioFromObject.Add("sfx_dlc_dogfight_p1_catgunmeow");
	}
}
