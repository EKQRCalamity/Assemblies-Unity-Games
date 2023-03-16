using System.Collections;
using UnityEngine;

public class OldManLevelStomachPlatform : LevelPlatform
{
	[SerializeField]
	private SpriteRenderer[] rend;

	[SerializeField]
	private BoxCollider2D boxColl;

	public bool isTargeted;

	private bool spawnsParryable;

	public Animator sparkAnimator;

	private Vector3 tongueOffset = new Vector3(20f, 30f);

	public OldManLevelGnomeLeader main;

	[SerializeField]
	private Animator splashAnimator;

	[SerializeField]
	private Animator mouthBubble;

	[SerializeField]
	private Animator noseBubble;

	[SerializeField]
	private SpriteRenderer mouthBubbleRend;

	[SerializeField]
	private SpriteRenderer noseBubbleRend;

	[SerializeField]
	private MinMax noseBubbleRange = new MinMax(7f / 12f, 1f);

	[SerializeField]
	private MinMax mouthBubbleRange = new MinMax(1f, 1.5833334f);

	private Coroutine bubbleCoroutine;

	public bool isActivated { get; private set; }

	private void Start()
	{
		isActivated = true;
		base.animator.Play("Idle", 0, Random.Range(0f, 1f));
	}

	private void aniEvent_SpawnParryable()
	{
		main.SpawnParryable(base.transform.position + tongueOffset);
		splashAnimator.Play("OpenSplash");
		splashAnimator.Update(0f);
		SFX_BellLoop();
	}

	public void FlipX()
	{
		SpriteRenderer[] array = rend;
		foreach (SpriteRenderer spriteRenderer in array)
		{
			spriteRenderer.flipX = true;
		}
		boxColl.offset = new Vector2(0f - boxColl.offset.x, boxColl.offset.y);
		tongueOffset.x = 0f - tongueOffset.x;
	}

	public void Anticipation()
	{
		if (isActivated)
		{
			isTargeted = true;
			base.animator.SetTrigger("Anticipation");
		}
	}

	public void CancelAnticipation()
	{
		isTargeted = false;
		if (isActivated)
		{
			base.animator.Play("Idle");
			return;
		}
		isActivated = true;
		base.animator.SetBool("IsEating", value: false);
		base.animator.Play("ReverseEat", 0, 1f - base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
		base.animator.Play("Eat_Ripple_End", 1, 0f);
		if (bubbleCoroutine != null)
		{
			StopCoroutine(bubbleCoroutine);
		}
	}

	public Vector3 GetTonguePos()
	{
		return base.transform.position + tongueOffset;
	}

	public void DeactivatePlatform(bool spawnsParryable)
	{
		this.spawnsParryable = spawnsParryable;
		string text = ((!spawnsParryable) ? "IsEating" : "IsBell");
		if (spawnsParryable)
		{
			sparkAnimator.Play("Spark");
			SFX_BonkHead();
		}
		else
		{
			SFX_Chomp();
		}
		base.animator.SetBool(text, value: true);
		isActivated = false;
		isTargeted = false;
		if (!spawnsParryable)
		{
			bubbleCoroutine = StartCoroutine(bubble_cr());
		}
	}

	private IEnumerator bubble_cr()
	{
		float noseTimer = noseBubbleRange.RandomFloat();
		float mouthTimer = mouthBubbleRange.RandomFloat();
		while (true)
		{
			noseTimer -= (float)CupheadTime.Delta;
			mouthTimer -= (float)CupheadTime.Delta;
			if (noseTimer <= 0f)
			{
				noseBubble.Play("Bubble", 0, 0f);
				noseBubbleRend.flipX = Rand.Bool();
				noseTimer += noseBubbleRange.RandomFloat();
			}
			if (mouthTimer <= 0f)
			{
				mouthBubble.Play("Bubble", 0, 0f);
				mouthBubbleRend.flipX = Rand.Bool();
				mouthTimer += mouthBubbleRange.RandomFloat();
			}
			yield return null;
		}
	}

	public void ActivatePlatform()
	{
		StartCoroutine(activate_cr());
	}

	private IEnumerator activate_cr()
	{
		if (bubbleCoroutine != null)
		{
			StopCoroutine(bubbleCoroutine);
		}
		mouthBubble.Play("None");
		noseBubble.Play("None");
		if (base.animator.GetBool("IsBell"))
		{
			base.animator.SetBool("IsBell", value: false);
			base.animator.Play("Bell_End");
		}
		else
		{
			yield return CupheadTime.WaitForSeconds(this, Random.Range(0f, 0.2f));
			base.animator.SetBool("IsEating", value: false);
		}
		isActivated = true;
		spawnsParryable = false;
	}

	private void SFX_Chomp()
	{
		AudioManager.Play("sfx_dlc_omm_p3_dino_chomp");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_dino_chomp");
	}

	private void SFX_BonkHead()
	{
		AudioManager.Play("sfx_dlc_omm_p3_dinobells_bonkhead");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_dinobells_bonkhead");
	}

	private void SFX_BellLoop()
	{
		AudioManager.PlayLoop("sfx_dlc_omm_p3_dinobells_loop");
		emitAudioFromObject.Add("sfx_dlc_omm_p3_dinobells_loop");
	}

	private void AniEvent_SFX_BellLoopEnd()
	{
		AudioManager.Stop("sfx_dlc_omm_p3_dinobells_loop");
	}
}
