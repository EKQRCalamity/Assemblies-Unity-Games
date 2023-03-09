using System;
using System.Collections;
using UnityEngine;

public class ChessPawnLevelPawn : AbstractProjectile
{
	private enum State
	{
		Idle,
		Jump,
		Run
	}

	private const float FALL_DISTANCE = 650f;

	private static readonly Vector3 DropPositionOffset = new Vector3(0f, 100f);

	[SerializeField]
	private Collider2D collider;

	[SerializeField]
	private SpriteRenderer bodyRenderer;

	[SerializeField]
	private SpriteRenderer headRenderer;

	[SerializeField]
	private SpriteDeathParts parriedHead;

	[SerializeField]
	private float deathTwitchDelayFixed;

	[SerializeField]
	private Rangef deathTwitchDelayRange;

	[SerializeField]
	private float deathFloatUpSpeed;

	[SerializeField]
	private Effect deathSmokeEffect;

	[SerializeField]
	private SpriteDeathParts deathBody;

	[SerializeField]
	private BoxCollider2D noHeadCollider;

	private ChessPawnLevel level;

	private State state;

	private Vector3 initialPosition;

	private bool beginFall;

	private int parryCount;

	private int lastIndex;

	public override float ParryMeterMultiplier => 0f;

	public float speed { get; private set; }

	public bool inUse { get; private set; }

	public int currentIndex { get; private set; }

	public ChessPawnLevelPawn Init(ChessPawnLevel level)
	{
		ChessPawnLevelPawn chessPawnLevelPawn = UnityEngine.Object.Instantiate(this, Camera.main.transform.position + Vector3.up * 2000f, Quaternion.identity);
		chessPawnLevelPawn.level = level;
		return chessPawnLevelPawn;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		level = null;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
		base.OnCollisionPlayer(hit, phase);
	}

	protected override void Die()
	{
		base.Die();
	}

	public void SetIndex(int i)
	{
		currentIndex = i;
		if (i >= 0)
		{
			lastIndex = i;
		}
	}

	protected override void OnDieDistance()
	{
	}

	protected override void OnDieLifetime()
	{
	}

	public override void OnLevelEnd()
	{
	}

	public override void OnParry(AbstractPlayerController player)
	{
		parryCount++;
		if (!PlayerManager.BothPlayersActive() || parryCount >= 2)
		{
			SetParryable(parryable: false);
			StartCoroutine(disable_collision_cr());
			if (state == State.Run)
			{
				base.animator.SetTrigger("Parry");
			}
			parriedHead.CreatePart(base.transform.position + Vector3.up * 100f);
			headRenderer.enabled = false;
			StartCoroutine(SFX_KOG_PAWN_PawnParry_cr());
			level.TakeDamage();
		}
	}

	public void StartIntro()
	{
		StartCoroutine(intro_cr());
	}

	private IEnumerator intro_cr()
	{
		inUse = true;
		yield return StartCoroutine(drop_cr(isIntro: true));
		inUse = false;
	}

	private IEnumerator drop_cr(bool isIntro)
	{
		AnimationHelper animationHelper = GetComponent<AnimationHelper>();
		Vector3 targetPosition = level.GetPosition(currentIndex);
		base.animator.Play("IntroStart");
		base.animator.SetInteger("Intro", isIntro ? (currentIndex % 2 + 1) : 0);
		targetPosition.z = (float)(currentIndex % 2) * 0.0001f;
		base.animator.Update(0f);
		animationHelper.Speed = 0f;
		bodyRenderer.sortingOrder = 0;
		headRenderer.sortingOrder = 1;
		float t = 0f;
		Vector3 dropPosition = targetPosition + DropPositionOffset;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (t < 1f)
		{
			base.transform.position = Vector3.Lerp(dropPosition, targetPosition, t);
			t += CupheadTime.FixedDelta * 8f;
			yield return wait;
		}
		animationHelper.Speed = 1f;
		base.transform.position = targetPosition;
		yield return base.animator.WaitForAnimationToStart(this, "Idle");
	}

	public void Attack(float warningTime, float horiztonalMovement, float dropSpeed, float runDelay, float runSpeed, float returnSpeed)
	{
		StartCoroutine(attack_cr(warningTime, horiztonalMovement, dropSpeed, runDelay, runSpeed, returnSpeed));
	}

	private IEnumerator attack_cr(float warningTime, float horizontalMovement, float dropSpeed, float runDelay, float runSpeed, float returnDelay)
	{
		inUse = true;
		initialPosition = base.transform.position;
		YieldInstruction wait = new WaitForFixedUpdate();
		base.animator.SetTrigger("JumpWarning");
		yield return CupheadTime.WaitForSeconds(this, warningTime);
		currentIndex = -1;
		state = State.Jump;
		collider.enabled = true;
		base.animator.SetTrigger("Jump");
		yield return CupheadTime.WaitForSeconds(this, 0.125f);
		if (horizontalMovement != 0f)
		{
			base.transform.SetScale(0f - Mathf.Sign(horizontalMovement));
		}
		Coroutine horizontalMovementCoroutine = StartCoroutine(horizontalMovement_cr(horizontalMovement, dropSpeed));
		while (!beginFall)
		{
			yield return null;
		}
		beginFall = false;
		float t = 0f;
		while (t < 1f)
		{
			Vector3 position = base.transform.position;
			position.y = initialPosition.y - 650f + 650f * Mathf.Sin((float)Math.PI / 2f + t * (float)Math.PI / 2f);
			if (position.y < base.transform.position.y)
			{
				bodyRenderer.sortingOrder = 20;
				headRenderer.sortingOrder = 21;
			}
			base.transform.position = position;
			t += CupheadTime.FixedDelta * dropSpeed;
			yield return wait;
		}
		StopCoroutine(horizontalMovementCoroutine);
		base.transform.position = new Vector3(base.transform.position.x, initialPosition.y - 650f);
		float testDir = Mathf.Sign(PlayerManager.GetNext().transform.position.x - base.transform.position.x);
		bool quickLand = level.ClearToRun(testDir, base.transform.position);
		if (runDelay == 0f && quickLand)
		{
			base.animator.SetInteger("Land", 1);
			base.transform.SetScale(testDir);
		}
		else
		{
			base.animator.SetInteger("Land", 2);
			float delay = runDelay - 0.625f;
			yield return CupheadTime.WaitForSeconds(this, runDelay);
			while (!level.ClearToRun(testDir, base.transform.position))
			{
				yield return wait;
				testDir = Mathf.Sign(PlayerManager.GetNext().transform.position.x - base.transform.position.x);
			}
			base.animator.SetInteger("Land", 3);
			base.transform.SetScale(testDir);
			yield return base.animator.WaitForAnimationToStart(this, "LandLongToRun");
		}
		state = State.Run;
		speed = runSpeed * testDir;
		while (Mathf.Abs(base.transform.position.x - Camera.main.transform.position.x) < 850f)
		{
			base.transform.position += speed * CupheadTime.FixedDelta * Vector3.right;
			yield return wait;
		}
		speed = 0f;
		state = State.Idle;
		base.animator.SetInteger("Land", 0);
		collider.enabled = false;
		currentIndex = level.GetReturnIndex();
		yield return StartCoroutine(drop_cr(isIntro: false));
		inUse = false;
	}

	private IEnumerator horizontalMovement_cr(float horizontalMovement, float dropSpeed)
	{
		float duration = 1f / dropSpeed;
		duration += 11f / 24f;
		float horizontalSpeed = horizontalMovement / duration;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			Vector3 position = base.transform.position;
			position.x += CupheadTime.FixedDelta * horizontalSpeed;
			base.transform.position = position;
			yield return wait;
		}
	}

	public void Death()
	{
		StopAllCoroutines();
		StartCoroutine(death_cr());
		if (headRenderer.enabled)
		{
			parriedHead.CreatePart(base.transform.position + Vector3.up * 100f);
			collider.enabled = false;
		}
	}

	private IEnumerator death_cr()
	{
		collider.enabled = false;
		base.transform.SetScale(1f);
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, initialPosition.x);
		GetComponent<AnimationHelper>().Speed = 1f;
		base.animator.Play("DeathTwitch", 0, (float)lastIndex * 0.125f);
		Effect smoke = deathSmokeEffect.Create(base.transform.position);
		StartCoroutine(move_smoke_cr(smoke));
		float delay = (float)(lastIndex % 4 * 2 + lastIndex / 2) * deathTwitchDelayFixed + UnityEngine.Random.Range(deathTwitchDelayRange.minimum, deathTwitchDelayRange.maximum);
		yield return CupheadTime.WaitForSeconds(this, delay);
		base.animator.Play("DeathAngel", 0, UnityEngine.Random.Range(0f, 1f));
		smoke.animator.Play("Explode");
		deathBody.CreatePart(base.transform.position);
		while (true)
		{
			Vector3 position = base.transform.position;
			position.y += deathFloatUpSpeed * (float)CupheadTime.Delta;
			base.transform.position = position;
			yield return null;
		}
	}

	private IEnumerator move_smoke_cr(Effect smoke)
	{
		SpriteRenderer smokeRenderer = smoke.GetComponent<SpriteRenderer>();
		while (smoke != null)
		{
			smoke.transform.position = base.transform.position + (Vector3)MathUtils.AngleToDirection(UnityEngine.Random.Range(0, 360)) * 50f;
			while (smokeRenderer != null && smokeRenderer.sprite != null)
			{
				yield return null;
			}
			yield return null;
		}
	}

	private void animationEvent_BeginFall()
	{
		beginFall = true;
	}

	private IEnumerator disable_collision_cr()
	{
		collider.enabled = false;
		yield return CupheadTime.WaitForSeconds(this, 0.2f);
		noHeadCollider.enabled = true;
	}

	private void AnimationEvent_SFX_KOG_PAWN_PawnLand()
	{
		AudioManager.Play("sfx_dlc_kog_pawn_land");
		emitAudioFromObject.Add("sfx_dlc_kog_pawn_land");
	}

	private void AnimationEvent_SFX_KOG_PAWN_PawnJumpDown()
	{
		AudioManager.Play("sfx_dlc_kog_pawn_jumpdown");
		emitAudioFromObject.Add("sfx_dlc_kog_pawn_jumpdown");
	}

	private void AnimationEvent_SFX_KOG_PAWN_PawnParryHit()
	{
		AudioManager.Play(string.Empty);
		emitAudioFromObject.Add("sfx_dlc_kog_pawn_parryhit");
	}

	private IEnumerator SFX_KOG_PAWN_PawnParry_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0.1f);
		AudioManager.Play("sfx_dlc_kog_pawn_parryhit");
		emitAudioFromObject.Add("sfx_dlc_kog_pawn_parryhit");
		yield return CupheadTime.WaitForSeconds(this, 0.15f);
		AudioManager.Play("sfx_dlc_kog_pawn_parrywoodbreak");
		emitAudioFromObject.Add("sfx_dlc_kog_pawn_parrywoodbreak");
	}
}
