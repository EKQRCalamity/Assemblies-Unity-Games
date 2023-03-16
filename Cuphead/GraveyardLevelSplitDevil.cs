using System.Collections;
using UnityEngine;

public class GraveyardLevelSplitDevil : LevelProperties.Graveyard.Entity
{
	private const float SING_ROAR_MAX_VOLUME = 0.4f;

	[SerializeField]
	private GraveyardLevelSplitDevilProjectile projectilePrefab;

	[SerializeField]
	private GraveyardLevelSplitDevilProjectile projectilePinkPrefab;

	[SerializeField]
	private GraveyardLevelSplitDevilBeam beamPrefab;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	[SerializeField]
	private GameObject bgSkellyMask;

	[SerializeField]
	private Collider2D coll;

	[SerializeField]
	private SpriteRenderer headRend;

	[SerializeField]
	private SpriteRenderer mainRend;

	[SerializeField]
	private SpriteRenderer headlessRend;

	[SerializeField]
	private SpriteRenderer haloRend;

	[SerializeField]
	private Transform projectileRoot;

	private PatternString numProjectiles;

	private PatternString projectileAngleOffset;

	private bool triggerShoot;

	public bool isAngel;

	public bool dead;

	private bool headLooping;

	[SerializeField]
	private SpriteRenderer[] shootFXRend;

	private int id = 1;

	private GraveyardLevel level;

	private PatternString projectilePinkString;

	private string sideString;

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (damageDealer != null && phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += GetComponentInParent<GraveyardLevelSplitDevil>().OnDamageTaken;
		if (base.transform.localScale.x > 0f)
		{
			SetIsAngel(value: true);
			id = 0;
		}
		sideString = ((!(base.transform.localScale.x < 0f)) ? "left" : "right");
	}

	public override void LevelInit(LevelProperties.Graveyard properties)
	{
		base.LevelInit(properties);
		numProjectiles = new PatternString(properties.CurrentState.splitDevilProjectiles.numProjectiles[id]);
		projectileAngleOffset = new PatternString(properties.CurrentState.splitDevilProjectiles.angleOffsetString);
		projectilePinkString = new PatternString(properties.CurrentState.splitDevilProjectiles.pinkString);
		level = Level.Current as GraveyardLevel;
		StartCoroutine(fade_in_cr());
	}

	private IEnumerator fade_in_cr()
	{
		mainRend.color = new Color(0f, 0f, 0f, 0f);
		haloRend.color = new Color(0f, 0f, 0f, 0f);
		float t = 0f;
		while (t < 4f)
		{
			mainRend.color = new Color(0f, 0f, 0f, t / 4f);
			haloRend.color = new Color(0f, 0f, 0f, t / 4f);
			t += (float)CupheadTime.Delta;
			yield return new WaitForFixedUpdate();
		}
		mainRend.color = new Color(0f, 0f, 0f, 1f);
		haloRend.color = new Color(0f, 0f, 0f, 1f);
	}

	private void SetIsAngel(bool value)
	{
		if (isAngel != value && !value)
		{
			AudioManager.Play("sfx_dlc_graveyard_changedirectionbad");
			emitAudioFromObject.Add("sfx_dlc_graveyard_changedirectionbad");
		}
		base.animator.SetBool("isAngel", value);
		coll.enabled = !value;
		isAngel = value;
		AudioManager.FadeSFXVolume("sfx_dlc_graveyard_angelsing_" + sideString, (!isAngel) ? 1E-05f : 0.4f, 0.4f);
		AudioManager.FadeSFXVolume("sfx_dlc_graveyard_devilangryrage_" + sideString, isAngel ? 1E-05f : 0.4f, 0.4f);
	}

	private void LateUpdate()
	{
		if (dead)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (allPlayer != null && !allPlayer.IsDead)
			{
				num2++;
				num += (int)Mathf.Sign(allPlayer.transform.localScale.x);
			}
		}
		if (Mathf.Abs(num) == num2)
		{
			if (isAngel != (Mathf.Sign(num) == Mathf.Sign(base.transform.localScale.x)) && headLooping)
			{
				ResyncHead();
			}
			SetIsAngel(Mathf.Sign(num) == Mathf.Sign(base.transform.localScale.x));
		}
		headlessRend.enabled = headRend.sprite != null;
		mainRend.enabled = !headlessRend.enabled;
	}

	public void NextPattern()
	{
		StartCoroutine((!level.CheckForBeamAttack()) ? projectile_cr() : roar_cr());
	}

	private IEnumerator roar_cr()
	{
		LevelProperties.Graveyard.SplitDevilBeam p = base.properties.CurrentState.splitDevilBeam;
		base.animator.SetBool("isSinging", value: true);
		int targetA = Animator.StringToHash(base.animator.GetLayerName(0) + ".SingStartAngel");
		int targetB = Animator.StringToHash(base.animator.GetLayerName(0) + ".SingStartDevil");
		while (base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash != targetA && base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash != targetB)
		{
			yield return null;
		}
		beamPrefab.Create(new Vector3(Mathf.Sign(base.transform.position.x) * (float)(Level.Current.Right - 50), 80f), p.speed.RandomFloat() * (0f - Mathf.Sign(base.transform.position.x)), p.warning, this);
		yield return new WaitForSeconds(1f);
		base.animator.SetBool("isSinging", value: false);
		yield return CupheadTime.WaitForSeconds(this, p.hesitateAfterAttack.RandomFloat());
		NextPattern();
	}

	private void ResyncHead()
	{
		float num = base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 0.25f;
		float num2 = base.animator.GetCurrentAnimatorStateInfo(1).normalizedTime - base.animator.GetCurrentAnimatorStateInfo(1).normalizedTime % 0.25f;
		if (base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash(base.animator.GetLayerName(0) + ".IdleAngel"))
		{
			base.animator.Play("ShootLoopAngel", 1, num2 + num);
			base.animator.Update(0f);
		}
		else if (base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash(base.animator.GetLayerName(0) + ".IdleDevil"))
		{
			base.animator.Play("ShootLoopDevil", 1, num2 + num);
			base.animator.Update(0f);
		}
	}

	private IEnumerator projectile_cr()
	{
		triggerShoot = true;
		while (triggerShoot)
		{
			yield return null;
		}
		base.animator.Play("Charge", 2, 0f);
		yield return CupheadTime.WaitForSeconds(this, 1f / 6f);
		headLooping = true;
		yield return base.animator.WaitForAnimationToEnd(this, "Charge", 2, waitForEndOfFrame: false, waitForStart: false);
		LevelProperties.Graveyard.SplitDevilProjectiles p = base.properties.CurrentState.splitDevilProjectiles;
		AbstractPlayerController player = PlayerManager.GetNext();
		float delayBetweenProjectiles = p.delayBetweenProjectiles.RandomFloat();
		LevelPlayerController p2 = PlayerManager.GetPlayer(PlayerId.PlayerOne) as LevelPlayerController;
		int projectileCount = numProjectiles.PopInt();
		for (int i = 0; i < projectileCount; i++)
		{
			if (player == null || player.IsDead)
			{
				player = PlayerManager.GetNext();
			}
			float rotation2 = MathUtils.DirectionToAngle(player.center - projectileRoot.transform.position);
			rotation2 += projectileAngleOffset.PopFloat();
			bool isPink = projectilePinkString.PopLetter() == 'P';
			if (isPink)
			{
				projectilePinkPrefab.Create(projectileRoot.transform.position, rotation2, p.projectileSpeed, this);
			}
			else
			{
				projectilePrefab.Create(projectileRoot.transform.position, rotation2, p.projectileSpeed, this);
			}
			base.animator.Play(isAngel ? "Light" : ((!Rand.Bool()) ? "FireB" : "FireA"), (!isPink) ? 3 : 4, 0f);
			shootFXRend[0].flipX = isAngel && Rand.Bool();
			shootFXRend[1].flipX = isAngel && Rand.Bool();
			shootFXRend[0].flipY = isAngel && Rand.Bool();
			shootFXRend[1].flipY = isAngel && Rand.Bool();
			SFX_SplitDevil_Shoot();
			if (i < projectileCount - 1)
			{
				yield return CupheadTime.WaitForSeconds(this, Mathf.Clamp(delayBetweenProjectiles - 11f / 24f, 0f, float.MaxValue));
				base.animator.Play("Charge", 2, 0f);
				yield return base.animator.WaitForAnimationToEnd(this, "Charge", 2);
				yield return CupheadTime.WaitForSeconds(this, 0.125f);
			}
		}
		headLooping = false;
		base.animator.SetBool("isShooting", value: false);
		yield return CupheadTime.WaitForSeconds(this, p.hesitateAfterAttack.RandomFloat());
		NextPattern();
	}

	public void AniEvent_CanStartShoot()
	{
		if (triggerShoot)
		{
			bool flag = base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash(base.animator.GetLayerName(0) + ".IdleAngel");
			if (flag != isAngel)
			{
				base.animator.Play((!flag) ? "ShootStartDevilToAngel" : "ShootStartAngelToDevil", 1, 0f);
			}
			else
			{
				base.animator.Play((!isAngel) ? "ShootStartDevil" : "ShootStartAngel", 1, 0f);
			}
			base.animator.SetBool("isShooting", value: true);
			triggerShoot = false;
		}
	}

	private void OnDisable()
	{
		if (!base.animator.GetBool("isShooting"))
		{
			headlessRend.enabled = false;
			mainRend.enabled = true;
		}
	}

	public void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
	}

	public void Die()
	{
		dead = true;
		headRend.enabled = false;
		headlessRend.enabled = false;
		mainRend.enabled = true;
		triggerShoot = false;
		StopAllCoroutines();
		base.animator.Play((!isAngel) ? "DeathDevilLoop" : "DeathAngelLoop");
		StartCoroutine(death_cr());
	}

	private IEnumerator death_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 2.5f);
		base.animator.SetTrigger("DeathContinue");
		yield return CupheadTime.WaitForSeconds(this, 0.25f);
		GetComponent<LevelBossDeathExploder>().StopExplosions();
	}

	private void ActivateBGSkellyMask()
	{
		bgSkellyMask.SetActive(value: true);
	}

	private void SFXSingRoar()
	{
		AudioManager.FadeSFXVolume("sfx_dlc_graveyard_angelsing_" + sideString, (!isAngel) ? 1E-05f : 0.4f, 0.01f);
		AudioManager.FadeSFXVolume("sfx_dlc_graveyard_devilangryrage_" + sideString, isAngel ? 1E-05f : 0.4f, 0.01f);
		AudioManager.Play("sfx_dlc_graveyard_angelsing_" + sideString);
		emitAudioFromObject.Add("sfx_dlc_graveyard_angelsing_" + sideString);
		AudioManager.Play("sfx_dlc_graveyard_devilangryrage_" + sideString);
		emitAudioFromObject.Add("sfx_dlc_graveyard_devilangryrage_" + sideString);
	}

	private void AnimationEvent_SFX_SplitDevil_AngelSing()
	{
		SFXSingRoar();
	}

	private void AnimationEvent_SFX_SplitDevil_DevilRage()
	{
		SFXSingRoar();
	}

	private void SFX_SplitDevil_Shoot()
	{
		AudioManager.Play((!isAngel) ? "sfx_dlc_graveyard_devil_shoot" : "sfx_DLC_Graveyard_Angel_Shoot");
		emitAudioFromObject.Add((!isAngel) ? "sfx_dlc_graveyard_devil_shoot" : "sfx_DLC_Graveyard_Angel_Shoot");
	}
}
