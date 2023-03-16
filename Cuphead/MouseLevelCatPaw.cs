using System.Collections;
using UnityEngine;

public class MouseLevelCatPaw : AbstractCollidableObject
{
	public enum State
	{
		Idle,
		Attack
	}

	private LevelProperties.Mouse.Claw properties;

	private Vector2 initialPos;

	private DamageDealer damageDealer;

	public State state { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		initialPos = base.transform.localPosition;
		damageDealer = DamageDealer.NewEnemy();
	}

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
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public void Attack(LevelProperties.Mouse.Claw properties)
	{
		this.properties = properties;
		if (state == State.Idle)
		{
			state = State.Attack;
			StartCoroutine(attack_cr());
		}
	}

	private IEnumerator attack_cr()
	{
		float totalMoveTime = 0.584f;
		float startX = initialPos.x;
		float endX = initialPos.x + totalMoveTime * properties.moveSpeed;
		int hitAnim = Animator.StringToHash(base.animator.GetLayerName(0) + ".Attack_Hit");
		float previousAnimationsTime = 0f;
		for (int i = 0; i < 3; i++)
		{
			base.animator.SetTrigger("Attack");
			float animationTime = ((i != 0) ? 0.167f : 0.25f);
			bool hitGround = false;
			while (!hitGround)
			{
				yield return new WaitForEndOfFrame();
				AnimatorStateInfo animState = base.animator.GetCurrentAnimatorStateInfo(0);
				if (animState.fullPathHash == hitAnim)
				{
					hitGround = true;
					previousAnimationsTime += animationTime;
					TransformExtensions.SetLocalPosition(x: Mathf.Lerp(startX, endX, previousAnimationsTime / totalMoveTime), transform: base.transform);
					CupheadLevelCamera.Current.Shake(15f, 1f);
					yield return CupheadTime.WaitForSeconds(this, properties.holdGroundTime);
				}
				else
				{
					float num = animState.normalizedTime * animationTime;
					float t2 = (previousAnimationsTime + num) / totalMoveTime;
					base.transform.SetLocalPosition(Mathf.Lerp(startX, endX, t2));
				}
			}
		}
		base.animator.SetTrigger("Leave");
		StartCoroutine(timedAudioCatMeow_cr());
		float moveStartX = base.transform.localPosition.x;
		float moveEndX = initialPos.x;
		float leaveTime = Mathf.Abs(moveEndX - moveStartX) / properties.leaveSpeed;
		float t = 0f;
		while (t < leaveTime)
		{
			t += (float)CupheadTime.Delta;
			base.transform.SetLocalPosition(EaseUtils.Ease(EaseUtils.EaseType.easeInSine, moveStartX, moveEndX, t / leaveTime));
			yield return null;
		}
		base.transform.SetLocalPosition(moveEndX);
		state = State.Idle;
		yield return null;
	}

	private IEnumerator timedAudioCatMeow_cr()
	{
		yield return new WaitForSeconds(1f);
		AudioManager.Play("level_mouse_cat_claw_end");
	}

	private void SoundCatPawAttack()
	{
		AudioManager.Play("level_mouse_cat_paw_attack");
		emitAudioFromObject.Add("level_mouse_cat_paw_attack");
	}

	private void SoundCatMeowVoice()
	{
		AudioManager.Play("level_mouse_cat_meow_voice");
		emitAudioFromObject.Add("level_mouse_cat_meow_voice");
	}
}
