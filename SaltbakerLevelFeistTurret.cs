using System.Collections;
using UnityEngine;

public class SaltbakerLevelFeistTurret : AbstractCollidableObject
{
	[SerializeField]
	private SaltbakerLevelTurretBullet bulletPrefab;

	[SerializeField]
	private SaltbakerLevelTurretBullet pinkBulletPrefab;

	private LevelProperties.Saltbaker.Turrets properties;

	private SaltbakerLevelSaltbaker parent;

	private Collider2D coll;

	private SpriteRenderer sprite;

	[SerializeField]
	private SpriteRenderer pepperText;

	[SerializeField]
	private SpriteRenderer pepperTextFlip;

	[SerializeField]
	private GameObject fxRend;

	[SerializeField]
	private GameObject sneezeFX;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	private float health;

	private float startHealth;

	private int index;

	private bool shootPink;

	private Coroutine shootCR;

	private Vector3 basePos;

	public bool IsActivated { get; private set; }

	private void Start()
	{
		SFX_SALTBAKER_P2_Saltshaker_Appear();
		basePos = base.transform.position;
		fxRend.transform.SetEulerAngles(0f, 0f, Random.Range(0, 360));
		coll.enabled = true;
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		if (health <= 0f && IsActivated)
		{
			return;
		}
		health -= info.damage;
		if (!(health <= 0f))
		{
			return;
		}
		if (IsActivated && parent.phaseTwoStarted)
		{
			if (!parent.preventAdditionalTurretLaunch)
			{
				if (parent.PreDamagePhaseTwoAndReturnWhetherDoomed(startHealth))
				{
					parent.preventAdditionalTurretLaunch = true;
				}
				StartCoroutine(fire_and_wait_to_respawn_cr());
			}
			else
			{
				health = 1f;
			}
		}
		else
		{
			health = 1f;
		}
	}

	private void FixedUpdate()
	{
		if (damageDealer != null)
		{
			damageDealer.FixedUpdate();
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

	public void Setup(LevelProperties.Saltbaker.Turrets properties, SaltbakerLevelSaltbaker parent, int index)
	{
		this.properties = properties;
		this.parent = parent;
		coll = GetComponent<Collider2D>();
		sprite = GetComponent<SpriteRenderer>();
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
		this.index = index;
	}

	private void AniEvent_Activate()
	{
		health = properties.turretHealth;
		startHealth = health;
		IsActivated = true;
		coll.enabled = true;
	}

	private IEnumerator fire_and_wait_to_respawn_cr()
	{
		base.animator.Play("Explode", 1, 0f);
		if (shootCR != null)
		{
			StopCoroutine(shootCR);
		}
		base.transform.position = basePos;
		base.animator.ResetTrigger("Shoot");
		coll.enabled = false;
		IsActivated = false;
		base.transform.localScale = new Vector3(1f, 1f);
		base.animator.Play("Attack" + index);
		SFX_SALTBAKER_P2_Saltshaker_DieLaunch();
		base.animator.Update(0f);
		yield return new WaitForEndOfFrame();
		yield return base.animator.WaitForAnimationToEnd(this, "Attack" + index);
		sprite.enabled = false;
		yield return CupheadTime.WaitForSeconds(this, properties.respawnTime - 0.75f);
		base.transform.localScale = new Vector3(0f - Mathf.Sign(base.transform.position.x), 1f);
		fxRend.transform.SetEulerAngles(0f, 0f, Random.Range(0, 360));
		sprite.sortingLayerName = "Projectiles";
		base.animator.Play("Intro");
		base.animator.Update(0f);
		SFX_SALTBAKER_P2_Saltshaker_Appear();
		sprite.enabled = true;
	}

	private void AniEvent_DamageSaltbaker()
	{
		SFX_SALTBAKER_P2_Saltshaker_LaunchImpact();
		parent.DamageSaltbaker(startHealth, index);
	}

	public void Shoot(bool isPink, float warning)
	{
		shootCR = StartCoroutine(shoot_cr(isPink, warning));
	}

	private IEnumerator shoot_cr(bool isPink, float warning)
	{
		Vector3 upPos = base.transform.position + Vector3.up * ((!(base.transform.position.y < 0f)) ? (-24) : 40);
		shootPink = isPink;
		base.animator.Play("ShootStart");
		base.animator.Update(0f);
		while (base.animator.GetCurrentAnimatorStateInfo(0).IsName("ShootStart"))
		{
			base.transform.position = Vector3.Lerp(basePos, upPos, EaseUtils.EaseOutSine(0f, 1f, base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime));
			yield return null;
		}
		base.transform.position = upPos;
		if (warning > 0.4f)
		{
			yield return CupheadTime.WaitForSeconds(this, warning - 0.4f);
			SFX_SALTBAKER_P2_Saltshaker_PreSneeze();
			yield return CupheadTime.WaitForSeconds(this, 0.4f);
		}
		else
		{
			SFX_SALTBAKER_P2_Saltshaker_PreSneeze();
			yield return CupheadTime.WaitForSeconds(this, warning);
		}
		base.animator.SetTrigger("Shoot");
		yield return base.animator.WaitForAnimationToStart(this, "ShootEnd");
		while (base.animator.GetCurrentAnimatorStateInfo(0).IsName("ShootEnd"))
		{
			base.transform.position = Vector3.Lerp(upPos, basePos, EaseUtils.EaseInSine(0f, 1f, base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime));
			yield return null;
		}
		base.transform.position = basePos;
	}

	private void AniEvent_SpawnProjectile()
	{
		float num = MathUtils.DirectionToAngle(PlayerManager.GetNext().center - base.transform.position);
		SaltbakerLevelTurretBullet saltbakerLevelTurretBullet = (shootPink ? pinkBulletPrefab : bulletPrefab);
		saltbakerLevelTurretBullet = saltbakerLevelTurretBullet.Create(sneezeFX.transform.position + (Vector3)MathUtils.AngleToDirection(num) * 150f, num, properties.shotSpeed, parent);
		saltbakerLevelTurretBullet.transform.localScale = base.transform.localScale;
		sneezeFX.transform.localScale = base.transform.localScale;
		sneezeFX.transform.eulerAngles = new Vector3(0f, 0f, num - 45f);
		SFX_SALTBAKER_P2_Saltshaker_SneezeAttack();
	}

	private void AniEvent_BottomLeftTurretSnapForward()
	{
		sprite.sortingLayerName = "Foreground";
	}

	private void LateUpdate()
	{
		pepperText.enabled = false;
		pepperTextFlip.enabled = false;
		SpriteRenderer spriteRenderer = ((base.transform.localScale.x != 1f) ? pepperTextFlip : pepperText);
		spriteRenderer.enabled = sprite.enabled && sprite.sprite != null && !base.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack0") && !base.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack1") && !base.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack2") && !base.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack3");
	}

	public void Die()
	{
		coll.enabled = false;
		sprite.sortingLayerName = "Projectiles";
		StopAllCoroutines();
		base.animator.ResetTrigger("Shoot");
		base.transform.position = basePos;
		base.animator.SetBool("Dead", value: true);
		SFX_SALTBAKER_P2_Saltshaker_Disappear();
	}

	private void SFX_SALTBAKER_P2_Saltshaker_Appear()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p2_saltshaker_appear");
		emitAudioFromObject.Add("sfx_dlc_saltbaker_p2_saltshaker_appear");
	}

	private void SFX_SALTBAKER_P2_Saltshaker_Disappear()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p2_saltshaker_disappear");
		emitAudioFromObject.Add("sfx_dlc_saltbaker_p2_saltshaker_disappear");
	}

	private void SFX_SALTBAKER_P2_Saltshaker_SneezeAttack()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p2_saltshaker_sneezeattack");
		emitAudioFromObject.Add("sfx_dlc_saltbaker_p2_saltshaker_sneezeattack");
	}

	private void SFX_SALTBAKER_P2_Saltshaker_PreSneeze()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p2_saltshaker_sneezepre");
		emitAudioFromObject.Add("sfx_dlc_saltbaker_p2_saltshaker_sneezepre");
	}

	private void SFX_SALTBAKER_P2_Saltshaker_DieLaunch()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p2_saltshaker_dielaunch");
		emitAudioFromObject.Add("sfx_dlc_saltbaker_p2_saltshaker_dielaunch");
	}

	private void SFX_SALTBAKER_P2_Saltshaker_LaunchImpact()
	{
		AudioManager.Play("sfx_DLC_Saltbaker_P2_Saltshaker_LaunchImpact");
	}
}
