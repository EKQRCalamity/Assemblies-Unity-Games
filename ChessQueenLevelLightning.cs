using System.Collections;
using UnityEngine;

public class ChessQueenLevelLightning : AbstractProjectile
{
	private const float YPosition = -385f;

	private static readonly bool[] BottomInFront = new bool[16]
	{
		true, true, true, false, false, false, false, false, true, true,
		true, false, false, false, false, false
	};

	private static readonly bool[] MiddleInFront = new bool[16]
	{
		false, false, false, false, true, true, true, false, false, false,
		false, false, true, true, true, false
	};

	[SerializeField]
	private SpriteRenderer bottomRenderer;

	[SerializeField]
	private SpriteRenderer middleRenderer;

	[SerializeField]
	private SpriteRenderer topRenderer;

	[SerializeField]
	private SpriteRenderer dustRenderer;

	[SerializeField]
	private SpriteRenderer deathSparkRenderer;

	[SerializeField]
	private Sprite[] rotatingSprites;

	[SerializeField]
	private Sprite[] deathSparkSprites;

	[SerializeField]
	private Effect lionsLandDustFX;

	[SerializeField]
	private Transform dropDustPos;

	[SerializeField]
	private SpriteDeathParts deathParts;

	[SerializeField]
	private SpriteDeathPartsDLC deathDust;

	private LevelProperties.ChessQueen.Lightning properties;

	private float speed;

	public override float ParryMeterMultiplier => 0f;

	public bool isGone { get; private set; }

	public ChessQueenLevelLightning Create(float posX, LevelProperties.ChessQueen.Lightning properties)
	{
		ResetLifetime();
		ResetDistance();
		base.transform.position = new Vector3(posX, -385f);
		this.properties = properties;
		lionsLandDustFX.Create(dropDustPos.transform.position);
		StartCoroutine(move_cr());
		return this;
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public override void OnParry(AbstractPlayerController player)
	{
		StopAllCoroutines();
		Die();
		isGone = true;
		StartCoroutine(death_cr());
	}

	private void LateUpdate()
	{
		int num = Mathf.Clamp((int)(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime * (float)deathSparkSprites.Length), 0, deathSparkSprites.Length - 1);
		if (num >= 0)
		{
			bottomRenderer.sortingOrder = (BottomInFront[num] ? 1 : (-1));
			topRenderer.sortingOrder = ((!MiddleInFront[num]) ? 1 : (-1));
		}
	}

	private IEnumerator move_cr()
	{
		SFX_KOG_QUEEN_ChessPiecesFall();
		base.transform.localScale = new Vector3(Mathf.Sign(base.transform.position.x), 1f);
		yield return base.animator.WaitForAnimationToEnd(this, "Intro");
		float delayTime = properties.lightningDelayTime - 13f / 24f;
		yield return CupheadTime.WaitForSeconds(this, delayTime);
		SFX_KOG_QUEEN_ChessPieceRoar();
		speed = Mathf.Sign(base.transform.position.x) * (0f - properties.lightningSweepSpeed);
		YieldInstruction wait = new WaitForFixedUpdate();
		while (base.transform.position.x > (float)Level.Current.Left - 400f && base.transform.position.x < (float)Level.Current.Right + 400f)
		{
			base.transform.AddPosition(speed * CupheadTime.FixedDelta);
			yield return wait;
		}
		isGone = true;
		this.Recycle();
	}

	private IEnumerator death_cr()
	{
		AnimationHelper animationHelper = GetComponent<AnimationHelper>();
		animationHelper.Speed = 0f;
		int index = Mathf.Clamp((int)(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime * (float)deathSparkSprites.Length), 0, deathSparkSprites.Length - 1);
		if (index < 0)
		{
			index = 0;
		}
		bottomRenderer.enabled = false;
		deathSparkRenderer.sprite = deathSparkSprites[index];
		yield return CupheadTime.WaitForSeconds(this, 1f / 24f);
		deathSparkRenderer.sprite = null;
		bottomRenderer.enabled = true;
		animationHelper.Speed = 1f;
		base.animator.Play("Death");
		SFX_KOG_QUEEN_ChessPiecesParried();
		StartCoroutine(SFX_KOG_QUEEN_ChessPieceMeow_cr());
		base.animator.SetTrigger("DustEnd");
		float minSpeed = speed * 0.2f;
		float maxSpeed = speed * 0.8f;
		SpriteDeathParts part4 = deathParts.CreatePart(bottomRenderer.transform.position);
		part4.SetVelocityX(minSpeed, maxSpeed);
		part4.GetComponent<SpriteRenderer>().sortingOrder = 13;
		part4.transform.localScale = base.transform.localScale;
		part4 = deathParts.CreatePart(middleRenderer.transform.position);
		part4.SetVelocityX(minSpeed, maxSpeed);
		part4.GetComponent<SpriteRenderer>().sortingOrder = 14;
		part4.transform.localScale = new Vector3(0f - base.transform.localScale.x, 1f);
		part4 = deathParts.CreatePart(topRenderer.transform.position);
		part4.SetVelocityX(minSpeed, maxSpeed);
		part4.transform.localScale = base.transform.localScale;
		part4 = deathDust.CreatePart(dustRenderer.transform.position);
		part4.SetVelocityX(speed * 0.5f, speed * 0.5f);
		part4.transform.localScale = base.transform.localScale;
	}

	private void SFX_KOG_QUEEN_ChessPieceRoar()
	{
		AudioManager.Play("sfx_dlc_kog_queen_chesspieceroar");
		emitAudioFromObject.Add("sfx_dlc_kog_queen_chesspieceroar");
	}

	private void SFX_KOG_QUEEN_ChessPiecesFall()
	{
		AudioManager.Play("sfx_dlc_kog_queen_chesspiecesfall");
		emitAudioFromObject.Add("sfx_dlc_kog_queen_chesspiecesfall");
	}

	private void SFX_KOG_QUEEN_ChessPiecesParried()
	{
		AudioManager.Play("sfx_dlc_kog_queen_chesspiecesparried");
		emitAudioFromObject.Add("sfx_dlc_kog_queen_chesspiecesparried");
	}

	private IEnumerator SFX_KOG_QUEEN_ChessPieceMeow_cr()
	{
		AudioManager.Stop("sfx_dlc_kog_queen_chesspieceroar");
		yield return CupheadTime.WaitForSeconds(this, 0.17f);
		AudioManager.Play("sfx_dlc_kog_queen_chesspiecemeow");
		emitAudioFromObject.Add("sfx_dlc_kog_queen_chesspiecemeow");
	}
}
