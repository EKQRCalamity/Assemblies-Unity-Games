using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowCultLevelWizard : LevelProperties.SnowCult.Entity
{
	public enum States
	{
		Idle,
		Quad,
		Whale,
		Slam,
		Wind,
		SeriesShot
	}

	public States state;

	private const int NUM_OF_SLAM_SLOTS = 4;

	private const int NUM_OF_QUAD_SHOTS = 4;

	private const float QUAD_SHOT_START_SPACING_MULTIPLIER = 0.8f;

	private const float WIZARD_SLAM_OFFSET = 230f;

	private const float WHALE_ATTACK_HEIGHT = 200f;

	private const float WHALE_ATTACK_MOVE_DELAY = 0.22f;

	private const float WHALE_POSTATTACK_MOVE_DELAY = 0.4f;

	private const float WHALE_RANGE = 195f;

	private DamageDealer damageDealer;

	private DamageReceiver damageReceiver;

	[SerializeField]
	private BasicProjectile seriesShot;

	[SerializeField]
	private Animator whaleDropFX;

	[SerializeField]
	private SnowCultLevelTable table;

	[SerializeField]
	private Animator shootFX;

	[SerializeField]
	private SnowCultLevelQuadShot quadShotProjectile;

	[SerializeField]
	private Transform pivotPoint;

	[SerializeField]
	private Transform outroPos;

	[SerializeField]
	private SpriteMask quadshotMask;

	[SerializeField]
	private Effect cardSparkle;

	[SerializeField]
	private SpriteRenderer introWizRend;

	private Vector3 lineStartPos;

	private Vector3 lineEndPos;

	private bool goingLeft;

	private bool isMoving;

	private bool reachedApex;

	private bool notReachedMid;

	private Vector3 lastPos = Vector3.zero;

	private PatternString wizardHesitationString;

	private PatternString attackLocationString;

	private PatternString hazardDirectionString;

	private PatternString iceSummonString;

	private PatternString seriesShotCountString;

	private PatternString quadShotBallDelayString;

	private bool seriesShotFired;

	private bool seriesShotCanExit = true;

	private bool seriesShotActive;

	private PatternString seriesShotParryString;

	private bool dropAttackComplete;

	private float postWhalePositionLerpTimer = 1f;

	public bool dead;

	private bool turnAnimationPlaying;

	private float currentPosition = 1f;

	private bool outroWobbling;

	private float outroWobbleTime;

	public event Action OnDeathEvent;

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
		damageReceiver = GetComponent<DamageReceiver>();
		damageReceiver.OnDamageTaken += OnDamageTaken;
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private void OnDamageTaken(DamageDealer.DamageInfo info)
	{
		base.properties.DealDamage(info.damage);
	}

	public override void LevelInit(LevelProperties.SnowCult properties)
	{
		base.LevelInit(properties);
		state = States.Idle;
		wizardHesitationString = new PatternString(properties.CurrentState.wizard.wizardHesitationString);
		attackLocationString = new PatternString(properties.CurrentState.quadShot.attackLocationString);
		quadShotBallDelayString = new PatternString(properties.CurrentState.quadShot.ballDelayString);
		hazardDirectionString = new PatternString(properties.CurrentState.quadShot.hazardDirectionString);
		seriesShotCountString = new PatternString(properties.CurrentState.seriesShot.seriesShotCountString);
		seriesShotParryString = new PatternString(properties.CurrentState.seriesShot.parryString);
		quadShotBallDelayString.SetSubStringIndex(-1);
		hazardDirectionString.SetSubStringIndex(-1);
		StartCoroutine(intro_cr());
	}

	protected override void OnCollisionPlayer(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionPlayer(hit, phase);
		if (phase != CollisionPhase.Exit)
		{
			damageDealer.DealDamage(hit);
		}
	}

	public void PlayerHitByWhale(GameObject hit, CollisionPhase phase)
	{
		OnCollisionPlayer(hit, phase);
	}

	private IEnumerator intro_cr()
	{
		yield return base.animator.WaitForAnimationToStart(this, "Idle");
		float t = 0f;
		Vector3 startPos = base.transform.position;
		Vector3 endPos = new Vector3(pivotPoint.position.x + 540f, pivotPoint.transform.position.y);
		base.animator.SetBool("Turn", value: true);
		while (t < 1f)
		{
			float easedT = EaseUtils.EaseInOutSine(0f, 1f, t);
			base.transform.position = new Vector3(Mathf.Lerp(startPos.x, endPos.x, easedT), EaseUtils.EaseInSine(startPos.y, endPos.y, easedT));
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		yield return CupheadTime.WaitForSeconds(this, 0.25f);
		StartCoroutine(move_cr());
	}

	private void AniEvent_StartTurn()
	{
		turnAnimationPlaying = true;
	}

	private void AniEvent_CompleteTurn()
	{
		if (!dead && turnAnimationPlaying)
		{
			bool flag = goingLeft;
			if (currentPosition > 0.9f)
			{
				flag = !flag;
			}
			base.transform.localScale = new Vector3((!flag) ? 1 : (-1), base.transform.localScale.y);
			turnAnimationPlaying = false;
		}
	}

	private void AniEvent_AlignForOutro()
	{
		base.transform.localScale = new Vector3(Mathf.Sign(base.transform.position.x - Camera.main.transform.position.x), base.transform.localScale.y);
		outroWobbling = true;
	}

	public bool Turning()
	{
		return base.animator.GetBool("Turn");
	}

	private IEnumerator move_cr()
	{
		goingLeft = true;
		LevelProperties.SnowCult.Movement p = base.properties.CurrentState.movement;
		float startAngle = (float)Math.PI / 2f;
		float endAngle = -(float)Math.PI / 2f;
		float angle = endAngle;
		float loopSizeX = 540f;
		float loopSizeY = p.dipAmount;
		float loopSpeed = p.speed;
		float startSpeed = p.speed;
		float endSpeed = p.easing;
		bool easeIn = true;
		Vector3 handleRotationX = Vector3.zero;
		Vector3 handleRotationY = Vector3.zero;
		base.transform.SetPosition(pivotPoint.position.x + loopSizeX);
		float t = 1f;
		float time = 1f;
		isMoving = true;
		while (true)
		{
			if (!isMoving)
			{
				yield return null;
				continue;
			}
			angle += loopSpeed * CupheadTime.FixedDelta * postWhalePositionLerpTimer * ((!dead) ? 1f : 1.5f);
			if ((angle < endAngle && !goingLeft) || (angle > startAngle && goingLeft))
			{
				reachedApex = true;
				notReachedMid = true;
				loopSpeed = 0f - loopSpeed;
				goingLeft = !goingLeft;
				t = 0f;
				startSpeed = (easeIn ? ((!goingLeft) ? (0f - p.speed) : p.speed) : ((!goingLeft) ? (0f - p.easing) : p.easing));
				endSpeed = (easeIn ? ((!goingLeft) ? (0f - p.easing) : p.easing) : ((!goingLeft) ? (0f - p.speed) : p.speed));
				easeIn = true;
			}
			else
			{
				reachedApex = false;
			}
			if ((angle > startAngle - 1.5f && goingLeft && easeIn) || (angle < endAngle + 1.5f && !goingLeft && easeIn))
			{
				t = 0f;
				startSpeed = (easeIn ? ((!goingLeft) ? (0f - p.speed) : p.speed) : ((!goingLeft) ? (0f - p.easing) : p.easing));
				endSpeed = (easeIn ? ((!goingLeft) ? (0f - p.easing) : p.easing) : ((!goingLeft) ? (0f - p.speed) : p.speed));
				easeIn = false;
			}
			if (((goingLeft && base.transform.position.x < 0f) || (!goingLeft && base.transform.position.x > 0f)) && notReachedMid)
			{
				notReachedMid = false;
			}
			if (t < time)
			{
				t += CupheadTime.FixedDelta;
				loopSpeed = Mathf.Lerp(startSpeed, endSpeed, t / time);
			}
			Vector3 handleRotation = new Vector3((0f - Mathf.Sin(angle)) * loopSizeX, (0f - Mathf.Cos(angle)) * loopSizeY, 0f);
			Vector3 destinationPos = pivotPoint.position + handleRotation;
			lastPos = base.transform.position;
			base.transform.position = new Vector3(destinationPos.x, Mathf.Lerp(base.transform.position.y, destinationPos.y, postWhalePositionLerpTimer));
			postWhalePositionLerpTimer = Mathf.Clamp(postWhalePositionLerpTimer + CupheadTime.FixedDelta * 2.5f, 0f, 1f);
			currentPosition = Mathf.InverseLerp(startAngle, endAngle, angle);
			if (goingLeft)
			{
				currentPosition = 1f - currentPosition;
			}
			bool goingLeftOrientation = goingLeft;
			if (currentPosition > 0.9f)
			{
				goingLeftOrientation = !goingLeftOrientation;
			}
			if (!dead)
			{
				base.animator.SetBool("Turn", (int)base.transform.localScale.x != ((!goingLeftOrientation) ? 1 : (-1)) && !seriesShotActive);
			}
			yield return new WaitForFixedUpdate();
		}
	}

	public void StartQuadAttack()
	{
		StartCoroutine(quad_cr());
	}

	private IEnumerator quad_cr()
	{
		state = States.Quad;
		LevelProperties.SnowCult.QuadShot p = base.properties.CurrentState.quadShot;
		float targetPosX = attackLocationString.PopFloat();
		bool inAttackPos = false;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (!inAttackPos)
		{
			if (dead)
			{
				yield break;
			}
			if (Mathf.Abs(targetPosX - base.transform.position.x) < p.distToAttack && !turnAnimationPlaying)
			{
				inAttackPos = true;
			}
			yield return wait;
		}
		int curIdleFrame = Mathf.RoundToInt(base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime * 23f);
		if (curIdleFrame >= 14 && curIdleFrame <= 22)
		{
			base.animator.Play("QuadshotIntro", 0, 0.2857143f);
		}
		else if (curIdleFrame >= 2 && curIdleFrame <= 10)
		{
			base.animator.Play("QuadshotIntro", 0, 1f / 7f);
		}
		else
		{
			base.animator.Play("QuadshotIntro");
		}
		SFX_SNOWCULT_WizardQuadshotAttack();
		isMoving = false;
		List<SnowCultLevelQuadShot> quadShots = new List<SnowCultLevelQuadShot>();
		float downAmount = 0f;
		yield return CupheadTime.WaitForSeconds(this, p.preattackDelay);
		base.animator.Play("QuadshotContinue");
		yield return null;
		yield return base.animator.WaitForAnimationToEnd(this, "QuadshotContinue");
		quadshotMask.enabled = true;
		for (int i = 0; i < 4; i++)
		{
			downAmount = ((i <= 0 || i >= 3) ? 0f : p.distanceDown);
			Vector3 startPos = new Vector3(base.transform.position.x - p.distanceBetween * 0.8f * 2f + p.distanceBetween * 0.8f * 0.5f + p.distanceBetween * 0.8f * (float)i, base.transform.position.y - downAmount);
			Vector3 destPos = new Vector3(base.transform.position.x - p.distanceBetween * 2f + p.distanceBetween / 2f + p.distanceBetween * (float)i, base.transform.position.y - downAmount);
			SnowCultLevelQuadShot snowCultLevelQuadShot = quadShotProjectile.Spawn();
			float delay = quadShotBallDelayString.PopFloat() / 4f * p.ballDelay;
			snowCultLevelQuadShot.Init(startPos, destPos, p.shotVelocity, hazardDirectionString.PopString(), p, i, delay, p.distanceBetween, PlayerManager.GetNext());
			quadShots.Add(snowCultLevelQuadShot);
		}
		yield return CupheadTime.WaitForSeconds(this, 0.25f);
		quadshotMask.enabled = false;
		yield return CupheadTime.WaitForSeconds(this, p.attackDelay - 0.25f);
		base.animator.Play("QuadshotEnd");
		yield return null;
		while (base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.27272728f)
		{
			yield return null;
		}
		AbstractPlayerController player = PlayerManager.GetNext();
		float first = 1000f;
		float second = 1000f;
		SnowCultLevelQuadShot shotQuadChosen2 = null;
		SnowCultLevelQuadShot shotQuadChosen3 = null;
		for (int j = 0; j < 4; j++)
		{
			float num = Mathf.Abs(quadShots[j].transform.position.x - player.transform.position.x);
			if (num < first)
			{
				second = first;
				first = num;
				shotQuadChosen3 = shotQuadChosen2;
				shotQuadChosen2 = quadShots[j];
			}
			else if (num < second && num != first)
			{
				second = num;
				shotQuadChosen3 = quadShots[j];
			}
		}
		float offset = UnityEngine.Random.Range(0f, p.maxOffset);
		offset = ((!Rand.Bool()) ? (0f - offset) : offset);
		SnowCultLevelQuadShot shotQuadChosen = ((!Rand.Bool()) ? shotQuadChosen3 : shotQuadChosen2);
		Vector3 endPos = new Vector3(player.transform.position.x, Level.Current.Ground);
		Vector3 direction = endPos - shotQuadChosen.transform.position;
		Vector3 finalDirection = new Vector3(direction.x + offset, direction.y);
		lineStartPos = shotQuadChosen.transform.position;
		lineEndPos = new Vector3(player.transform.position.x + offset, endPos.y);
		bool startWithRight = Rand.Bool();
		int rightIndex = 3;
		for (int k = 0; k < 4; k++)
		{
			int index = ((!startWithRight) ? k : (rightIndex - k));
			quadShots[index].Shoot(MathUtils.DirectionToAngle(finalDirection));
		}
		yield return base.animator.WaitForAnimationToStart(this, "Idle");
		isMoving = true;
		yield return CupheadTime.WaitForSeconds(this, wizardHesitationString.PopFloat());
		state = States.Idle;
	}

	public void Whale()
	{
		StartCoroutine(whale_cr());
	}

	private IEnumerator whale_cr()
	{
		state = States.Whale;
		LevelProperties.SnowCult.Whale p = base.properties.CurrentState.whale;
		dropAttackComplete = false;
		bool drop = false;
		YieldInstruction wait = new WaitForFixedUpdate();
		AbstractPlayerController player = PlayerManager.GetNext();
		float lastPlayerOffset = base.transform.position.x - Mathf.Clamp(player.transform.position.x, -445f, 445f);
		while (!drop)
		{
			float playerClampedX = Mathf.Clamp(player.transform.position.x, -445f, 445f);
			if (Mathf.Abs(playerClampedX - base.transform.position.x) < p.distToDrop || Mathf.Sign(lastPlayerOffset) != Mathf.Sign(base.transform.position.x - playerClampedX))
			{
				drop = true;
			}
			lastPlayerOffset = base.transform.position.x - playerClampedX;
			yield return wait;
		}
		isMoving = false;
		yield return base.animator.WaitForAnimationToStart(this, "Idle");
		float currentAnimatorTime = base.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		base.animator.Play((!(currentAnimatorTime > 1f / 12f) || !(currentAnimatorTime < 7f / 12f)) ? "WhaleDrop_IntroAlt" : "WhaleDrop_Intro");
		SFX_SNOWCULT_WizardWhalesmashAttack();
		float t = 0f;
		float val2 = 0f;
		Vector3 startPos = base.transform.position;
		Vector3 endPos = new Vector3(startPos.x, 200f);
		while (t < 0.22f)
		{
			t += (float)CupheadTime.Delta;
			val2 = Mathf.InverseLerp(0f, 0.22f, t);
			base.transform.position = Vector3.Lerp(startPos, endPos, EaseUtils.EaseInSine(0f, 1f, val2));
			yield return null;
		}
		base.transform.position = endPos;
		t = 0f;
		while (t < p.attackDelay)
		{
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		base.animator.SetTrigger("DropWhale");
		while (!dropAttackComplete)
		{
			yield return null;
		}
		yield return CupheadTime.WaitForSeconds(this, 1f / 12f);
		postWhalePositionLerpTimer = 0f;
		isMoving = true;
		yield return CupheadTime.WaitForSeconds(this, p.recoveryDelay);
		yield return CupheadTime.WaitForSeconds(this, wizardHesitationString.PopFloat());
		state = States.Idle;
	}

	private void WhaleAttackImpact()
	{
		CupheadLevelCamera.Current.Shake(55f, 0.5f);
	}

	private void WhaleAttackComplete()
	{
		whaleDropFX.transform.position = new Vector3(base.transform.position.x, whaleDropFX.transform.position.y);
		whaleDropFX.gameObject.SetActive(value: true);
		whaleDropFX.Play("Main");
		dropAttackComplete = true;
	}

	public void SeriesShot()
	{
		StartCoroutine(series_shot_cr());
	}

	private IEnumerator series_shot_cr()
	{
		seriesShotCanExit = false;
		seriesShotActive = true;
		state = States.SeriesShot;
		LevelProperties.SnowCult.SeriesShot p = base.properties.CurrentState.seriesShot;
		int shotCount = seriesShotCountString.PopInt();
		float t = 0f;
		base.animator.SetTrigger("StartPeashot");
		yield return base.animator.WaitForAnimationToStart(this, "Peashot_Intro");
		table.Intro(base.transform.position - lastPos);
		for (int i = 0; i < shotCount; i++)
		{
			while (t < p.seriesShotWarningTime && !dead)
			{
				t += (float)CupheadTime.Delta;
				yield return null;
			}
			if (!dead)
			{
				base.animator.SetTrigger("OnShoot");
				while (!seriesShotFired)
				{
					yield return null;
				}
				SFX_SNOWCULT_WizardTarotCardAttackLaunch();
				seriesShotFired = false;
			}
			t = 0f;
			while (t < p.betweenShotDelay && !dead)
			{
				t += (float)CupheadTime.Delta;
				yield return null;
			}
			if (dead)
			{
				break;
			}
		}
		seriesShotCanExit = true;
		while (seriesShotActive)
		{
			yield return null;
		}
		if (!dead)
		{
			yield return CupheadTime.WaitForSeconds(this, wizardHesitationString.PopFloat());
		}
		state = States.Idle;
		yield return null;
	}

	private void CreatePeashot()
	{
		StartCoroutine(create_peashot());
	}

	private IEnumerator create_peashot()
	{
		shootFX.Play("ShootFX");
		shootFX.Update(0f);
		yield return CupheadTime.WaitForSeconds(this, 1f / 24f);
		AbstractPlayerController player = PlayerManager.GetNext();
		Vector3 dir = player.transform.position - shootFX.transform.position;
		BasicProjectile proj = seriesShot.Create(shootFX.transform.position, MathUtils.DirectionToAngle(dir) + 90f, base.properties.CurrentState.seriesShot.bulletSpeed);
		proj.transform.position += dir.normalized * 25f;
		proj.SetParryable(seriesShotParryString.PopLetter() == 'P');
		seriesShotFired = true;
		while (shootFX.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.8f)
		{
			Effect sparkle = cardSparkle.Create(shootFX.transform.position + (Vector3)MathUtils.AngleToDirection(UnityEngine.Random.Range(0, 360)) * shootFX.GetCurrentAnimatorStateInfo(0).normalizedTime * 200f);
			yield return CupheadTime.WaitForSeconds(this, UnityEngine.Random.Range(0.005f, 0.01f));
		}
	}

	private void CanExitPeashotLoop()
	{
		if (seriesShotCanExit)
		{
			base.animator.Play("Peashot_Outro_A");
			table.Outro();
		}
	}

	private void EndPeashotLoop()
	{
		seriesShotActive = false;
	}

	public void ToOutro(SnowCultLevelYeti yeti)
	{
		dead = true;
		StartCoroutine(outro_cr(yeti));
	}

	private void AniEvent_CultistsSummon()
	{
		((SnowCultLevel)Level.Current).CultistsSummon();
	}

	private IEnumerator outro_cr(SnowCultLevelYeti yeti)
	{
		while (!reachedApex)
		{
			yield return null;
		}
		if (base.transform.localScale.x != Mathf.Sign(base.transform.position.x - Camera.main.transform.position.x))
		{
			base.animator.SetBool("Turn", value: true);
		}
		state = States.Idle;
		isMoving = false;
		base.animator.SetTrigger("OnOutro");
		float t = 0f;
		Vector3 startPos = base.transform.position;
		if (base.transform.position.x < pivotPoint.position.x)
		{
			outroPos.position = new Vector3(pivotPoint.position.x + (pivotPoint.position.x - outroPos.position.x), outroPos.position.y);
			yeti.StartOnLeft(pivotPoint.position);
		}
		while (t < 0.5f)
		{
			base.transform.position = new Vector3(EaseUtils.EaseOutSine(startPos.x, outroPos.position.x, t * 2f), EaseUtils.EaseOutBack(startPos.y, outroPos.position.y, t * 2f));
			t += CupheadTime.FixedDelta;
			yield return new WaitForFixedUpdate();
		}
		while (base.animator.GetCurrentAnimatorStateInfo(0).fullPathHash != Animator.StringToHash(base.animator.GetLayerName(0) + ".OutroLoop"))
		{
			base.transform.position = outroPos.position;
			yield return null;
		}
		yeti.gameObject.SetActive(value: true);
		yeti.StartYeti();
		while (!yeti.introRibcageClosed)
		{
			base.transform.position = outroPos.position;
			yield return null;
		}
		OnDeath();
	}

	private void LateUpdate()
	{
		if (outroWobbling)
		{
			outroWobbleTime += CupheadTime.FixedDelta * 1.5f;
			base.transform.position += new Vector3(Mathf.Sin(outroWobbleTime * 3f) * 1f, Mathf.Cos(outroWobbleTime * 2f) * 5f);
		}
	}

	public void OnDeath()
	{
		if (this.OnDeathEvent != null)
		{
			this.OnDeathEvent();
		}
		StopAllCoroutines();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.DrawLine(lineStartPos, lineEndPos);
	}

	private void AnimationEvent_SFX_SNOWCULT_WizardIntro()
	{
		AudioManager.Play("sfx_dlc_snowcult_p1_wizard_intro");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p1_wizard_intro");
	}

	private void AnimationEvent_SFX_SNOWCULT_WizardQuadshot_Attack()
	{
		AudioManager.Play("sfx_dlc_snowcult_p1_wizard_quadshot_attack");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p1_wizard_quadshot_attack");
	}

	private void SFX_SNOWCULT_WizardWhalesmashAttack()
	{
		AudioManager.Play("sfx_dlc_snowcult_p1_wizard_whalesmash_attack");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p1_wizard_whalesmash_attack");
	}

	private void SFX_SNOWCULT_WizardTarotCardAttackLaunch()
	{
		AudioManager.Play("sfx_dlc_snowcult_p1_wizard_tarotcardattack_launch");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p1_wizard_tarotcardattack_launch");
	}

	private void SFX_SNOWCULT_WizardQuadshotAttack()
	{
		AudioManager.Play("sfx_dlc_snowcult_p1_wizard_quadshot_attack");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p1_wizard_quadshot_attack");
	}

	private void AnimationEvent_SFX_SNOWCULT_WizardYetiIntroBellComesToLife()
	{
		AudioManager.Play("sfx_dlc_snowcult_p2_snowmonster_intro_bell_comestolife");
		emitAudioFromObject.Add("sfx_dlc_snowcult_p2_snowmonster_intro_bell_comestolife");
	}

	private void AnimationEvent_SFX_SNOWCULT_WizardVoiceEffortLarge()
	{
		AudioManager.Stop("sfx_dlc_snowcult_wizard_voice_laugh");
		AudioManager.Play("sfx_dlc_snowcult_wizard_voice_effort_large");
		emitAudioFromObject.Add("sfx_dlc_snowcult_wizard_voice_effort_large");
	}

	private void AnimationEvent_SFX_SNOWCULT_WizardVoiceLaugh()
	{
		AudioManager.Stop("sfx_dlc_snowcult_wizard_voice_effort_large");
		AudioManager.Stop("sfx_dlc_snowcult_wizard_voice_laugh");
		AudioManager.Play("sfx_dlc_snowcult_wizard_voice_laugh");
		emitAudioFromObject.Add("sfx_dlc_snowcult_wizard_voice_laugh");
	}

	private void AnimationEvent_SFX_SNOWCULT_WizardVoiceWhee()
	{
		AudioManager.Play("sfx_dlc_snowcult_wizard_voice_whee");
		emitAudioFromObject.Add("sfx_dlc_snowcult_wizard_voice_whee");
	}
}
