using System.Collections;
using DG.Tweening;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Entities.Weapon;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.HighWills.Attack;

public class RangedMine : Weapon, IDamageable, IDirectAttack
{
	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	public string HitSound;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	public string DestroyedSound;

	public Transform TargetPositionAfterSpawn;

	[HideInInspector]
	public RangedMine PriorMine;

	[HideInInspector]
	public RangedMine NextMine;

	public bool CreatesLinkEffect;

	[ShowIf("CreatesLinkEffect", true)]
	public SpriteRenderer EffectRenderer;

	[MinMaxSlider(0f, 10f, true)]
	public Vector2 MinMaxHorSpeed;

	public float VerMovementTime = 1f;

	public float MaxRelativeHeight = 2f;

	public float MinRelativeHeight = -2f;

	private Hit mineHit;

	public float Life = 1f;

	public float DamageAmount;

	public bool GotDestroyed;

	public float MaxLifeTime = 20f;

	private bool startTweenDone;

	private Tween horTween;

	private Tween verTween;

	private float initialHeight;

	private float currentLifeTime;

	public BoxCollider2D Collider { get; set; }

	public Animator Animator { get; set; }

	public AttackArea AttackArea { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		AttackArea = GetComponentInChildren<AttackArea>();
		Collider = GetComponent<BoxCollider2D>();
		Animator = GetComponentInChildren<Animator>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		mineHit = GetHit();
		initialHeight = base.transform.position.y;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!startTweenDone)
		{
			return;
		}
		currentLifeTime += Time.deltaTime;
		if (currentLifeTime > MaxLifeTime)
		{
			ExplodeMine();
			return;
		}
		if (CreatesLinkEffect)
		{
			bool flag = PriorMine != null && PriorMine.gameObject.activeInHierarchy;
			EffectRenderer.enabled = flag;
			if (flag)
			{
				EffectRenderer.transform.position = Vector3.Lerp(PriorMine.transform.position, base.transform.position, 0.5f);
				float x = Vector3.Distance(PriorMine.transform.position, base.transform.position);
				EffectRenderer.size = new Vector2(x, EffectRenderer.size.y);
				Vector2 vector = (EffectRenderer.transform.position - base.transform.position).normalized;
				float z = 180f + 57.29578f * Mathf.Atan2(vector.y, vector.x);
				EffectRenderer.transform.rotation = Quaternion.Euler(0f, 0f, z);
			}
		}
		if (horTween == null)
		{
			float num = Mathf.Lerp(MinMaxHorSpeed.x, MinMaxHorSpeed.y, Random.Range(0f, 1f));
			horTween = base.transform.DOMoveX(base.transform.position.x + num, 1f).OnComplete(delegate
			{
				horTween = null;
			}).SetEase(Ease.Linear);
		}
		if (verTween == null)
		{
			float endValue = initialHeight + MaxRelativeHeight;
			float duration = VerMovementTime * 0.5f;
			if (base.transform.position.y < initialHeight)
			{
				endValue = initialHeight + MaxRelativeHeight;
				duration = VerMovementTime;
			}
			else if (base.transform.position.y > initialHeight)
			{
				endValue = initialHeight + MinRelativeHeight;
				duration = VerMovementTime;
			}
			verTween = base.transform.DOMoveY(endValue, duration).OnComplete(delegate
			{
				verTween = null;
			}).SetEase(Ease.InOutQuad);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Penitent"))
		{
			Gameplay.GameControllers.Penitent.Penitent componentInParent = other.GetComponentInParent<Gameplay.GameControllers.Penitent.Penitent>();
			if (componentInParent.Status.Unattacable)
			{
				ForcedAttackToTarget(componentInParent, mineHit);
				ExplodeMine();
			}
			else
			{
				Attack(mineHit);
				ExplodeMine();
			}
		}
	}

	private void OnEnable()
	{
		Collider.enabled = true;
		GotDestroyed = false;
		startTweenDone = false;
		StartCoroutine(WaitAndDoStartMove());
	}

	private IEnumerator WaitAndDoStartMove()
	{
		yield return new WaitForSeconds(0.2f);
		currentLifeTime = 0f;
		base.transform.DOMove(base.transform.position + TargetPositionAfterSpawn.localPosition, 0.4f).OnComplete(delegate
		{
			startTweenDone = true;
		}).SetEase(Ease.OutBack);
	}

	public void SetPriorMine(RangedMine mine)
	{
		if (!(mine == null))
		{
			PriorMine = mine;
			mine.NextMine = this;
		}
	}

	public override void Attack(Hit weapondHit)
	{
		GetDamageableEntities();
		AttackDamageableEntities(weapondHit);
	}

	public override void OnHit(Hit weaponHit)
	{
	}

	private void ForcedAttackToTarget(Gameplay.GameControllers.Penitent.Penitent penitent, Hit rootAttack)
	{
		penitent.DamageArea.TakeDamage(rootAttack, force: true);
	}

	private Hit GetHit()
	{
		Hit result = default(Hit);
		result.AttackingEntity = base.gameObject;
		result.DamageAmount = DamageAmount;
		result.DamageType = DamageArea.DamageType.Normal;
		result.HitSoundId = HitSound;
		return result;
	}

	public void Damage(Hit hit)
	{
		Life -= hit.DamageAmount;
		if (Life <= 0f)
		{
			DestroyMine();
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public void DestroyMine()
	{
		Core.Audio.EventOneShotPanned(DestroyedSound, base.transform.position);
		GotDestroyed = true;
		if (Collider.enabled)
		{
			Collider.enabled = false;
		}
		Animator.SetTrigger("DESTROY");
	}

	public void ExplodeMine()
	{
		if (Collider.enabled)
		{
			Collider.enabled = false;
		}
		Animator.SetTrigger("EXPLODE");
	}

	public bool BleedOnImpact()
	{
		return false;
	}

	public bool SparkOnImpact()
	{
		return true;
	}

	public void CreateHit()
	{
	}

	public void SetDamage(int damage)
	{
		mineHit.DamageAmount = damage;
		DamageAmount = damage;
	}
}
