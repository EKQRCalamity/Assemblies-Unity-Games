using System;
using System.Collections;
using UnityEngine;

public class ChessQueenLevelCannon : AbstractCollidableObject
{
	public enum CannonPosition
	{
		Side,
		Center
	}

	private static readonly float BaseRotationDuration = 0.625f;

	private static readonly int BaseAnimatorLayer;

	private static readonly int CannonAnimatorLayer = 1;

	private static readonly int BlastFXAnimatorLayer = 3;

	[SerializeField]
	private ChessQueenLevelCannonball cannonBall;

	[SerializeField]
	private ParrySwitch[] parry;

	[SerializeField]
	private SpriteRenderer baseRenderer;

	[SerializeField]
	private SpriteRenderer barrelHighlightRenderer;

	[SerializeField]
	private SpriteRenderer baseTopperRenderer;

	[SerializeField]
	private Transform barrelTransform;

	[SerializeField]
	private Transform bulletSpawnPoint;

	[SerializeField]
	private Transform blastFXSpawnPoint;

	[SerializeField]
	private Transform blastFXTransform;

	[SerializeField]
	private Sprite[] baseSprites;

	[SerializeField]
	private Sprite[] barrelHighlightSprites;

	[SerializeField]
	private Transform wickTransform;

	[SerializeField]
	private Transform wickParabolaEndTransform;

	[SerializeField]
	private Transform wickBlastPositionerTransform;

	[SerializeField]
	private Animator mouseAnimator;

	[SerializeField]
	private ChessQueenLevelLooseMouse looseMouse;

	[SerializeField]
	private bool mouseReverses;

	private Collider2D[] parryColliders;

	private LevelProperties.ChessQueen.Turret properties;

	private float rotationTime;

	private float minAngle;

	private float maxAngle;

	private CannonPosition cannonPosition;

	private ChessQueenLevelQueen queen;

	private float mouseLookTime;

	private bool wickFollowsParabola = true;

	private Vector3 wickStartPosition;

	private float wickParabolaParameter;

	private float wickParametricDuration;

	public bool IsActive { get; set; }

	private void Start()
	{
		parryColliders = new Collider2D[parry.Length];
		for (int i = 0; i < parry.Length; i++)
		{
			parry[i].OnActivate += shootCannonball;
			parryColliders[i] = parry[i].GetComponent<Collider2D>();
		}
		SetActive(setActive: false);
		mouseAnimator.Play("Idle");
		mouseLookTime = UnityEngine.Random.Range(1f, 3f);
		setupWickParabola();
	}

	public void SetProperties(float minAngle, float maxAngle, float rotationTime, CannonPosition cannonPosition, LevelProperties.ChessQueen.Turret properties, ChessQueenLevelQueen queen)
	{
		this.minAngle = minAngle;
		this.maxAngle = maxAngle;
		this.rotationTime = rotationTime;
		this.cannonPosition = cannonPosition;
		this.properties = properties;
		this.queen = queen;
		move();
	}

	public void SetActive(bool setActive)
	{
		mouseAnimator.SetBool("Idle", !setActive);
		if (setActive)
		{
			mouseAnimator.SetBool("LookRight", value: false);
		}
		IsActive = setActive;
		Collider2D[] array = parryColliders;
		foreach (Collider2D collider2D in array)
		{
			collider2D.enabled = setActive;
		}
	}

	private void LateUpdate()
	{
		if (!IsActive)
		{
			mouseLookTime -= CupheadTime.Delta;
			if (mouseLookTime < 0f)
			{
				mouseAnimator.SetBool("LookRight", !mouseAnimator.GetBool("LookRight"));
				mouseLookTime += UnityEngine.Random.Range(1f, 3f);
			}
		}
		int num = Array.IndexOf(baseSprites, baseRenderer.sprite);
		if (num < 0)
		{
			if (base.animator.GetCurrentAnimatorStateInfo(0).IsTag("01"))
			{
				num = 0;
			}
			else if (base.animator.GetCurrentAnimatorStateInfo(0).IsTag("05"))
			{
				num = 4;
			}
			else if (base.animator.GetCurrentAnimatorStateInfo(0).IsTag("10"))
			{
				num = 9;
			}
			else
			{
				if (!base.animator.GetCurrentAnimatorStateInfo(0).IsTag("15"))
				{
					return;
				}
				num = 14;
			}
		}
		float num2 = (float)num / (float)(baseSprites.Length - 1);
		if (cannonPosition == CannonPosition.Side)
		{
			num2 = EaseUtils.EaseInOutCubic(0f, 1f, num2);
		}
		float num3 = Mathf.Lerp(minAngle, maxAngle, num2);
		if (cannonPosition == CannonPosition.Center)
		{
			num3 *= (float)((!baseRenderer.flipX) ? 1 : (-1));
		}
		barrelTransform.rotation = Quaternion.Euler(0f, 0f, num3);
		barrelHighlightRenderer.sprite = barrelHighlightSprites[Mathf.RoundToInt((1f - num2) * (float)(barrelHighlightSprites.Length - 1))];
		barrelHighlightRenderer.flipX = cannonPosition == CannonPosition.Center && baseRenderer.flipX;
		barrelHighlightRenderer.enabled = base.animator.GetCurrentAnimatorStateInfo(1).IsName("Idle");
		if (wickFollowsParabola)
		{
			float num4 = num2 * wickParametricDuration;
			Vector3 position = wickStartPosition;
			position.x -= 2f * wickParabolaParameter * num4 * (float)((!baseRenderer.flipX) ? 1 : (-1));
			position.y += wickParabolaParameter * num4 * num4;
			wickTransform.position = position;
		}
		if (Level.Current.Ending)
		{
			Collider2D[] array = parryColliders;
			foreach (Collider2D collider2D in array)
			{
				collider2D.enabled = false;
			}
		}
	}

	private void move()
	{
		if (cannonPosition == CannonPosition.Side)
		{
			base.animator.Play(0, BaseAnimatorLayer, 0.5f);
		}
		StartCoroutine(cannonActive_cr());
	}

	private IEnumerator cannonActive_cr()
	{
		while (true)
		{
			if (!IsActive)
			{
				yield return null;
				continue;
			}
			base.animator.SetBool("Moving", value: true);
			base.animator.SetFloat("BaseSpeed", BaseRotationDuration / rotationTime);
			base.animator.SetTrigger("WickIgnite");
			SFX_KOG_QUEEN_CannonFuseLoop();
			while (IsActive)
			{
				Collider2D[] array = parryColliders;
				foreach (Collider2D collider2D in array)
				{
					if (queen.activeLightning == null || queen.activeLightning.isGone)
					{
						collider2D.enabled = true;
					}
					else
					{
						collider2D.enabled = Mathf.Abs(collider2D.transform.position.x + collider2D.offset.x - queen.activeLightning.transform.position.x) > queen.lightningDisableRange;
					}
				}
				float animTime = base.animator.GetCurrentAnimatorStateInfo(BaseAnimatorLayer).normalizedTime % 1f;
				if (mouseReverses)
				{
					if (cannonPosition == CannonPosition.Side)
					{
						mouseAnimator.SetBool("Reverse", animTime > 0.4f && animTime <= 0.9f);
					}
					else
					{
						mouseAnimator.SetBool("Reverse", animTime > 0.5f != baseRenderer.flipX);
					}
				}
				yield return null;
			}
			base.animator.SetBool("Moving", value: false);
		}
	}

	private void shootCannonball()
	{
		if (IsActive)
		{
			base.animator.SetTrigger("CannonBlast");
			base.animator.SetTrigger("WickBlast");
			SFX_KOG_QUEEN_CannonShoot();
			SFX_KOG_QUEEN_CannonFuseLoopStop();
			SetActive(setActive: false);
			StartCoroutine(shoot_cr());
		}
	}

	private IEnumerator shoot_cr()
	{
		wickFollowsParabola = false;
		wickTransform.parent = wickBlastPositionerTransform;
		blastFXTransform.position = blastFXSpawnPoint.position;
		blastFXTransform.eulerAngles = barrelTransform.eulerAngles;
		base.animator.Play((!((ChessQueenLevel)Level.Current).cannonBlastFXVariant) ? "BlastFXB" : "BlastFXA", BlastFXAnimatorLayer, 0f);
		((ChessQueenLevel)Level.Current).cannonBlastFXVariant = !((ChessQueenLevel)Level.Current).cannonBlastFXVariant;
		base.animator.Update(0f);
		while (base.animator.GetCurrentAnimatorStateInfo(CannonAnimatorLayer).normalizedTime < 0.7f)
		{
			yield return null;
		}
		base.animator.SetFloat("BaseSpeed", BaseRotationDuration / rotationTime);
		base.animator.SetBool("Moving", value: false);
		wickTransform.parent = base.transform;
		wickFollowsParabola = true;
		wickTransform.localEulerAngles = Vector3.zero;
	}

	private void animationEvent_CannonFinishedCycle()
	{
		if (cannonPosition == CannonPosition.Center)
		{
			baseRenderer.flipX = !baseRenderer.flipX;
			baseTopperRenderer.flipX = baseRenderer.flipX;
		}
	}

	private void animationEvent_FireBullet()
	{
		if (!Level.Current.Ending)
		{
			looseMouse.CannonFired(cannonBall.Create(bulletSpawnPoint.transform.position, MathUtils.DirectionToAngle(barrelTransform.up), properties.turretCannonballSpeed).gameObject);
		}
	}

	private void setupWickParabola()
	{
		wickStartPosition = wickTransform.position;
		Vector3 vector = wickParabolaEndTransform.position - wickStartPosition;
		wickParabolaParameter = vector.x * vector.x / (4f * vector.y);
		wickParametricDuration = vector.x / (2f * wickParabolaParameter);
	}

	private void SFX_KOG_QUEEN_CannonShoot()
	{
		AudioManager.Stop("sfx_DLC_KOG_Queen_CannonFuse_Loop");
		AudioManager.Play("sfx_DLC_KOG_Queen_CannonShoot");
		AudioManager.Pan("sfx_DLC_KOG_Queen_CannonShoot", (!(Mathf.Abs(base.transform.position.x) > 100f)) ? 0f : Mathf.Sign(base.transform.position.x));
	}

	private void SFX_KOG_QUEEN_CannonFuseLoop()
	{
		AudioManager.PlayLoop("sfx_DLC_KOG_Queen_CannonFuse_Loop");
		AudioManager.Pan("sfx_DLC_KOG_Queen_CannonFuse_Loop", (!(Mathf.Abs(base.transform.position.x) > 100f)) ? 0f : Mathf.Sign(base.transform.position.x));
	}

	private void SFX_KOG_QUEEN_CannonFuseLoopStop()
	{
		AudioManager.Stop("sfx_DLC_KOG_Queen_CannonFuse_Loop");
	}
}
