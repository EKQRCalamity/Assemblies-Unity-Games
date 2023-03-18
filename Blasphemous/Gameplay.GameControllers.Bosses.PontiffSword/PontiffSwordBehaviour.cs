using System;
using DG.Tweening;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Bosses.PontiffGiant;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using Sirenix.OdinInspector;
using Tools.Level.Actionables;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffSword;

public class PontiffSwordBehaviour : EnemyBehaviour
{
	public GameObject parentOfTrails;

	private AshPlatform lastOne;

	public float preSlashAngle = -120f;

	public float anticipationDuration = 1.5f;

	public Ease anticipationEasing = Ease.OutQuad;

	public float attackDuration = 1.4f;

	public Ease attackEasing = Ease.InOutQuad;

	public float plungeDuration = 0.25f;

	public float plungeDistance = -6f;

	private float slashXMovement = 8f;

	private float slashYMovement = -3f;

	private float _heightOffset;

	public ContactFilter2D filter;

	public RaycastHit2D[] groundHits;

	public SWORD_STATES currentSwordState;

	public PontiffSwordMeleeAttack meleeAttack;

	public PontiffGiantBossfightPoints bossfightPoints;

	public Transform eyeTransform;

	public EntityMotionChecker motionChecker;

	[FoldoutGroup("Floating settings", 0)]
	public float speedFactor = 0.1f;

	[FoldoutGroup("Floating settings", 0)]
	public float floatingFreq = 0.1f;

	[FoldoutGroup("Floating settings", 0)]
	public float rotatingFreq = 2.75f;

	[FoldoutGroup("Floating settings", 0)]
	public float floatingAmp = 1f;

	[FoldoutGroup("Floating settings", 0)]
	public float normalHeight = 4f;

	[FoldoutGroup("Floating settings", 0)]
	public float maxAngle = 10f;

	[FoldoutGroup("Floating settings", 0)]
	public float rotationDampFactor = 0.05f;

	[FoldoutGroup("Floating settings", 0)]
	public float yFloatingAccel = 0.5f;

	[FoldoutGroup("Floating new settings", 0)]
	public float accel = 0.5f;

	[FoldoutGroup("Floating new settings", 0)]
	public float maxSpeed = 0.5f;

	[FoldoutGroup("Floating new settings", 0)]
	public Vector2 velocity;

	[FoldoutGroup("Floating new settings", 0)]
	public Vector2 chasingOffset;

	private Tween currentXTween;

	private float _flyAroundCounter;

	public PontiffSword PontiffSword { get; set; }

	public event Action OnSwordDestroyed;

	public event Action OnPlungeFinished;

	public event Action OnSlashFinished;

	public override void OnAwake()
	{
		base.OnAwake();
		groundHits = new RaycastHit2D[1];
		PontiffSword = GetComponent<PontiffSword>();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (currentSwordState == SWORD_STATES.FLYING_AROUND)
		{
			UpdateHeightOffset();
			UpdateFlyingAround();
		}
	}

	private void UpdateHeightOffset()
	{
		if (!(Core.Logic.Penitent == null))
		{
			if (GetBaseHeight() < Core.Logic.Penitent.transform.position.y)
			{
				_heightOffset += Time.deltaTime * 1f;
			}
			else
			{
				_heightOffset -= Time.deltaTime * 1f;
			}
		}
	}

	public void ChangeSwordState(SWORD_STATES st)
	{
		currentSwordState = st;
		if (st == SWORD_STATES.FLYING_AROUND)
		{
			_flyAroundCounter = 0f;
		}
	}

	private Vector2 GetDirToPenitent(Vector3 from)
	{
		return Core.Logic.Penitent.transform.position - from;
	}

	private void UpdateFlyingAround()
	{
		Vector2 vector = new Vector2((0f - Mathf.Sign(((Vector2)Core.Logic.Penitent.transform.position - (Vector2)base.transform.position).x)) * chasingOffset.x, chasingOffset.y);
		Vector2 vector2 = (Vector2)Core.Logic.Penitent.transform.position + vector;
		_flyAroundCounter += Time.deltaTime;
		Vector2 normalized = (vector2 - (Vector2)base.transform.position).normalized;
		GameplayUtils.DrawDebugCross(vector2, Color.magenta, 0.1f);
		float z = Mathf.Sin(_flyAroundCounter * rotatingFreq) * maxAngle;
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.Euler(0f, 0f, z), rotationDampFactor);
		float num = ((Mathf.Sign(velocity.x) == Mathf.Sign(normalized.x)) ? 1 : 2);
		float num2 = Vector2.Distance(base.transform.position, vector2);
		float num3 = 2f;
		if (num2 < num3)
		{
			velocity -= velocity * 0.5f * Time.deltaTime;
		}
		else
		{
			velocity += normalized.normalized * accel * Time.deltaTime * num;
		}
		velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
		Vector3 vector3 = (Vector3)velocity * Time.deltaTime;
		base.transform.position += vector3;
	}

	private bool IsPointOutsideBattleBoundaries(Vector2 p)
	{
		float num = Mathf.Abs(p.x - bossfightPoints.fightCenterTransform.position.x);
		return num > 4.6f;
	}

	public void ActivateTrails(bool activate)
	{
		parentOfTrails.SetActive(activate);
	}

	public void Slash()
	{
		float postSlashAngle = 360f + -1f * preSlashAngle;
		float sign = Mathf.Sign(GetDirToPenitent(base.transform.position).x);
		base.transform.DOKill();
		currentXTween = base.transform.DOMoveX(base.transform.position.x + slashXMovement * sign, anticipationDuration + attackDuration * 0.5f).SetEase(Ease.InBack).OnUpdate(CheckCollision);
		base.transform.DOMoveY(base.transform.position.y + slashYMovement, anticipationDuration + attackDuration * 0.25f).SetEase(Ease.InOutQuad).SetLoops(2, LoopType.Yoyo);
		base.transform.DORotate(new Vector3(0f, 0f, preSlashAngle * sign), anticipationDuration, RotateMode.LocalAxisAdd).SetEase(anticipationEasing).OnComplete(delegate
		{
			PontiffSword.Audio.PlaySlash_AUDIO();
			ActivateTrails(activate: true);
			meleeAttack.damageOnEnterArea = true;
			meleeAttack.OnMeleeAttackGuarded += OnMeleeAttackGuarded;
			base.transform.DORotate(new Vector3(0f, 0f, postSlashAngle * sign), attackDuration, RotateMode.LocalAxisAdd).SetEase(attackEasing).OnComplete(delegate
			{
				OnMeleeAttackFinished();
			});
		});
	}

	private void CheckCollision()
	{
		if (motionChecker.HitsBlock)
		{
			currentXTween.Kill(complete: true);
		}
	}

	private void OnMeleeAttackGuarded()
	{
		meleeAttack.OnMeleeAttackGuarded -= OnMeleeAttackGuarded;
		base.transform.DOKill();
		float num = Mathf.Sign(GetDirToPenitent(base.transform.position).x);
		base.transform.DOMoveX(base.transform.position.x - slashXMovement * num, anticipationDuration + attackDuration * 0.5f).SetEase(Ease.InBack);
		base.transform.DORotate(new Vector3(0f, 0f, -90f * num), 0.3f, RotateMode.LocalAxisAdd).SetEase(Ease.OutBack).OnComplete(delegate
		{
			OnMeleeAttackFinished();
		});
	}

	private void Repullo()
	{
		base.transform.DOKill();
		Vector2 dirToPenitent = GetDirToPenitent(base.transform.position);
		ChangeSwordState(SWORD_STATES.STUN);
		velocity = Vector2.zero;
		base.transform.DOMove((Vector2)base.transform.position - dirToPenitent.normalized * 3f, 0.4f).SetEase(Ease.OutQuad).OnComplete(delegate
		{
			ChangeSwordState(SWORD_STATES.FLYING_AROUND);
		});
	}

	private void OnMeleeAttackFinished()
	{
		meleeAttack.OnMeleeAttackGuarded -= OnMeleeAttackGuarded;
		meleeAttack.damageOnEnterArea = false;
		ActivateTrails(activate: false);
		if (this.OnSlashFinished != null)
		{
			this.OnSlashFinished();
		}
	}

	public void Plunge()
	{
		base.transform.DOKill();
		base.transform.DORotate(Vector3.zero, 0.5f);
		base.transform.DOMoveY(base.transform.position.y + 3f, anticipationDuration).SetEase(Ease.InOutQuad).OnComplete(delegate
		{
			meleeAttack.damageOnEnterArea = true;
			PontiffSword.Audio.PlayPlunge_AUDIO();
			Physics2D.Raycast(base.transform.position, Vector2.down, filter, groundHits, 30f);
			float num = groundHits[0].distance - 1.5f;
			Debug.DrawLine(base.transform.position, groundHits[0].point, Color.green, 5f);
			ActivateTrails(activate: true);
			base.transform.DOMoveY(base.transform.position.y - num, plungeDuration).SetEase(Ease.InExpo).OnComplete(delegate
			{
				PlungeFinished();
			});
		});
	}

	private void PlungeFinished()
	{
		ActivateTrails(activate: false);
		meleeAttack.damageOnEnterArea = false;
		if (this.OnPlungeFinished != null)
		{
			this.OnPlungeFinished();
		}
	}

	private float GetBaseHeight()
	{
		return bossfightPoints.fightCenterTransform.position.y + normalHeight;
	}

	public void BackToFlyingAround()
	{
		base.transform.DOKill();
		float baseHeight = GetBaseHeight();
		base.transform.DOMoveY(baseHeight, 2.5f).SetEase(Ease.InQuint).OnComplete(delegate
		{
			ChangeSwordState(SWORD_STATES.FLYING_AROUND);
		});
		base.transform.DORotate(Vector3.zero, 0.5f);
	}

	public void Move(Vector2 pos, float duration = 0.5f, TweenCallback callback = null)
	{
		base.transform.DOMove(pos, duration).SetEase(Ease.InOutQuad).onComplete = callback;
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	public void Revive()
	{
		PontiffSword.animatorInyector.Alive(alive: true);
		Invoke("BackToFlyingAround", 1f);
		eyeTransform.gameObject.SetActive(value: true);
	}

	public void Death()
	{
		if (this.OnSwordDestroyed != null)
		{
			this.OnSwordDestroyed();
		}
		ChangeSwordState(SWORD_STATES.DESTROYED);
		eyeTransform.gameObject.SetActive(value: false);
		meleeAttack.OnMeleeAttackGuarded -= OnMeleeAttackGuarded;
		meleeAttack.damageOnEnterArea = false;
		ActivateTrails(activate: false);
		base.transform.DOKill();
		base.transform.DORotate(Vector3.zero, 0.3f).OnComplete(delegate
		{
			PontiffSword.animatorInyector.Alive(alive: false);
		});
		Invoke("ActivatePlatform", 1f);
	}

	public void ActivatePlatform()
	{
	}

	public override void Damage()
	{
		if (currentSwordState == SWORD_STATES.FLYING_AROUND)
		{
			Repullo();
		}
	}

	public override void Idle()
	{
	}

	public override void StopMovement()
	{
	}

	public override void Wander()
	{
	}
}
