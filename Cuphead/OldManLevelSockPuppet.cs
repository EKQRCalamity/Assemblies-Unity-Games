using System;
using System.Collections;
using UnityEngine;

public class OldManLevelSockPuppet : AbstractCollidableObject
{
	private const string MOVING_UP = "MovingUp";

	private const string MOVING_DOWN = "MovingDown";

	private const string MOVING_DOWN_SHORT = "MovingDownShort";

	private const string MOVE_UP_START = "Move_Up_Start";

	private const string MOVE_DOWN_START = "Move_Down_Start";

	private const string MOVE_DOWN_SHORT_START = "Move_Down_Short_Start";

	private const string MOVE_UP_END = "Move_Up_End";

	private const string MOVE_DOWN_END = "Move_Down_End";

	private const string MOVE_DOWN_SHORT_END = "Move_Down_Short_End";

	[SerializeField]
	private GameObject arms;

	[SerializeField]
	private GameObject armsHolding;

	[SerializeField]
	private Transform throwPos;

	[SerializeField]
	private Transform catchPos;

	public bool ready;

	private DamageDealer damageDealer;

	[SerializeField]
	private float armBowingXModifier;

	[SerializeField]
	private float wobbleX = 5f;

	[SerializeField]
	private float wobbleY = 5f;

	public Vector3 rootPosition;

	public Vector3 startPosition;

	[SerializeField]
	private float wobbleTimer;

	[SerializeField]
	private float moveTimeShort = 0.375f;

	[SerializeField]
	private float moveTimeLong = 0.5f;

	[SerializeField]
	private float moveOvershoot = 0.5f;

	[SerializeField]
	private bool isLeft;

	private bool entering = true;

	private bool dead;

	[SerializeField]
	private OldManLevelSockPuppetHandler main;

	private Collider2D[] colliders;

	private void Start()
	{
		rootPosition = base.transform.position;
		startPosition = base.transform.position;
		damageDealer = DamageDealer.NewEnemy();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		WORKAROUND_NullifyFields();
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
		base.transform.position = rootPosition + Mathf.Sin(wobbleTimer * 3f) * wobbleX * Vector3.right + Mathf.Sin(wobbleTimer * 2f) * wobbleY * Vector3.up;
		wobbleTimer += CupheadTime.Delta;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	private void AniEvent_IncCmonCount()
	{
		base.animator.SetInteger("CmonCount", base.animator.GetInteger("CmonCount") + 1);
	}

	private void AniEvent_ResetCmonCount()
	{
		base.animator.SetInteger("CmonCount", 0);
	}

	public Vector3 throwPosition()
	{
		return throwPos.position;
	}

	public Vector3 catchPosition()
	{
		return catchPos.position;
	}

	private float EaseOvershoot(float start, float end, float value, float overshoot)
	{
		float num = Mathf.Lerp(start, end, value);
		return num + Mathf.Sin(value * (float)Math.PI) * ((end - start) * overshoot);
	}

	public void MoveToPos(float endYPos, float distanceToCover)
	{
		if (distanceToCover == 0f && !entering)
		{
			ready = true;
			return;
		}
		ready = false;
		StartCoroutine(move_to_pos_cr(endYPos, distanceToCover));
	}

	private float InverseLerpUnclamped(float a, float b, float value)
	{
		return (value - a) / (b - a);
	}

	private IEnumerator move_to_pos_cr(float endYPos, float distanceToCover)
	{
		float t = 0f;
		float startYPos = rootPosition.y;
		float time = ((distanceToCover != 1f) ? moveTimeLong : moveTimeShort);
		if (entering)
		{
			time = 0.5f;
			if (isLeft)
			{
				SFX_OMM_P2_PuppetLeftRaiseUp();
				SFX_OMM_P2_PuppetLeftRaiseUpVocal();
			}
			else
			{
				SFX_OMM_P2_PuppetRightRaiseUp();
				SFX_OMM_P2_PuppetRightRaiseUpVocal();
			}
		}
		YieldInstruction wait = new WaitForFixedUpdate();
		bool startEndAnimation = false;
		bool movingUp = endYPos > rootPosition.y;
		string moveBool = (movingUp ? "MovingUp" : ((distanceToCover != 1f) ? "MovingDown" : "MovingDownShort"));
		base.animator.SetBool(moveBool, value: true);
		string startAnimation = (movingUp ? "Move_Up_Start" : ((distanceToCover != 1f) ? "Move_Down_Start" : "Move_Down_Short_Start"));
		if (!dead)
		{
			if (movingUp)
			{
				yield return base.animator.WaitForAnimationToStart(this, startAnimation);
			}
			else
			{
				yield return base.animator.WaitForAnimationToStart(this, startAnimation);
			}
		}
		WaitForFrameTimePersistent wait24fps = new WaitForFrameTimePersistent(1f / 24f);
		while (t < time)
		{
			t += CupheadTime.FixedDelta;
			rootPosition = new Vector3(startPosition.x + armBowingXModifier * Mathf.Sin(InverseLerpUnclamped(startYPos, endYPos, rootPosition.y) * (float)Math.PI), EaseOvershoot(startYPos, endYPos, t / time, moveOvershoot), base.transform.position.z);
			if (t / time >= 0.35f && !startEndAnimation)
			{
				base.animator.SetBool(moveBool, value: false);
				startEndAnimation = true;
			}
			if (t / time >= 0.6f && entering)
			{
				entering = false;
				colliders = GetComponentsInChildren<Collider2D>();
				Collider2D[] array = colliders;
				foreach (Collider2D collider2D in array)
				{
					collider2D.enabled = true;
				}
			}
			yield return wait24fps;
		}
		rootPosition.x = startPosition.x;
		armsHolding.GetComponent<SpriteRenderer>().sortingLayerName = "Enemies";
		arms.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = 10;
		int target = Animator.StringToHash(base.animator.GetLayerName(0) + "." + (movingUp ? "Move_Up_End" : ((distanceToCover != 1f) ? "Move_Down_End" : "Move_Down_Short_End")));
		while (base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash == target)
		{
			yield return null;
		}
		base.animator.SetBool("TauntA", !base.animator.GetBool("TauntA"));
		ready = true;
	}

	public void StopTaunt()
	{
		base.animator.SetInteger("CmonCount", 5);
	}

	private void AniEvent_Catch()
	{
		main.CatchBall();
	}

	public void AnIEvent_HoldingBall()
	{
		arms.SetActive(value: false);
		armsHolding.SetActive(value: true);
	}

	public void AnIEvent_NotHoldingBall()
	{
		arms.SetActive(value: true);
		armsHolding.SetActive(value: false);
	}

	public void Die()
	{
		dead = true;
		Collider2D[] array = colliders;
		foreach (Collider2D collider2D in array)
		{
			collider2D.enabled = false;
		}
		base.animator.Play("Death");
		GetComponent<LevelBossDeathExploder>().StartExplosion();
		if (rootPosition.y > 200f)
		{
			StopAllCoroutines();
			StartCoroutine(move_to_pos_cr(180f, 1f));
		}
	}

	private void AnimationEvent_SFX_OMM_P2_PuppetRightCatch()
	{
		AudioManager.Play("sfx_dlc_omm_p2_puppet_right_catch");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_puppet_right_catch");
	}

	private void SFX_OMM_P2_PuppetRightRaiseUp()
	{
		AudioManager.Play("sfx_dlc_omm_p2_puppet_right_raiseup");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_puppet_right_raiseup");
	}

	private void SFX_OMM_P2_PuppetRightRaiseUpVocal()
	{
		AudioManager.Play("sfx_dlc_omm_p2_puppet_right_raiseup_vocal");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_puppet_right_raiseup_vocal");
	}

	private void AnimationEvent_SFX_OMM_P2_PuppetRightThrow()
	{
		AudioManager.Play("sfx_dlc_omm_p2_puppet_right_throw");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_puppet_right_throw");
	}

	private void AnimationEvent_SFX_OMM_P2_PuppetRightThrowVocal()
	{
		AudioManager.Play("sfx_dlc_omm_p2_puppet_right_throw_vocal");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_puppet_right_throw_vocal");
	}

	private void AnimationEvent_SFX_OMM_P2_PuppetLeftCatch()
	{
		AudioManager.Play("sfx_dlc_omm_p2_puppet_left_catch");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_puppet_left_catch");
	}

	private void SFX_OMM_P2_PuppetLeftRaiseUpVocal()
	{
		AudioManager.Play("sfx_dlc_omm_p2_puppet_left_raiseup");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_puppet_left_raiseup");
	}

	private void SFX_OMM_P2_PuppetLeftRaiseUp()
	{
		StartCoroutine(SFX_OMM_P2_PuppetLeftRaiseUpVocal_cr());
	}

	private IEnumerator SFX_OMM_P2_PuppetLeftRaiseUpVocal_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 0f);
		AudioManager.Play("sfx_dlc_omm_p2_puppet_left_raiseup_vocal");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_puppet_left_raiseup_vocal");
	}

	private void AnimationEvent_SFX_OMM_P2_PuppetLeftThrow()
	{
		AudioManager.Play("sfx_dlc_omm_p2_puppet_left_throw");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_puppet_left_throw");
	}

	private void AnimationEvent_SFX_OMM_P2_PuppetLeftThrowVocal()
	{
		AudioManager.Play("sfx_dlc_omm_p2_puppet_left_throw_vocal");
		emitAudioFromObject.Add("sfx_dlc_omm_p2_puppet_left_throw_vocal");
	}

	private void WORKAROUND_NullifyFields()
	{
		arms = null;
		armsHolding = null;
		throwPos = null;
		catchPos = null;
		damageDealer = null;
		main = null;
		colliders = null;
	}
}
