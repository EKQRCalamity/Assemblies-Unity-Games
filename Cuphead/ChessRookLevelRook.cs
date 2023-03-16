using System.Collections;
using UnityEngine;

public class ChessRookLevelRook : LevelProperties.ChessRook.Entity
{
	private static readonly int BaseLayerStateIndex;

	private static readonly int SparkLayerStateIndex = 1;

	private static readonly int WheelLayerStateIndex = 2;

	[SerializeField]
	private SpriteRenderer wheelRenderer;

	[SerializeField]
	private Transform cannonballSpawnRoot;

	[SerializeField]
	private ChessRookLevelPinkCannonBall cannonballPink;

	[SerializeField]
	private ChessRookLevelRegularCannonball cannonballRegular;

	[SerializeField]
	private BasicProjectile straightShot;

	[SerializeField]
	private Transform[] straightShotSpawnPoints;

	[SerializeField]
	private Effect hitSparkEffect;

	[SerializeField]
	private Effect straightShotSparkEffect;

	[SerializeField]
	private Effect smokeEffect;

	[SerializeField]
	private Animator spawnEffect;

	[SerializeField]
	private HitFlash hitFlash;

	private DamageDealer damageDealer;

	private Coroutine transitionCoroutine;

	private bool dead;

	private bool headTypeB;

	private int headZOffset;

	protected override void Awake()
	{
		base.Awake();
		damageDealer = DamageDealer.NewEnemy();
	}

	public override void LevelInit(LevelProperties.ChessRook properties)
	{
		base.LevelInit(properties);
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmosSelected();
		Gizmos.color = Color.cyan;
		Transform[] array = straightShotSpawnPoints;
		foreach (Transform transform in array)
		{
			Gizmos.DrawLine(transform.transform.position, transform.transform.position - Vector3.right * 1000f);
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

	protected override void OnCollisionEnemyProjectile(GameObject hit, CollisionPhase phase)
	{
		base.OnCollisionEnemyProjectile(hit, phase);
		if (phase == CollisionPhase.Enter)
		{
			ChessRookLevelPinkCannonBall component = hit.GetComponent<ChessRookLevelPinkCannonBall>();
			if ((bool)component && component.finishedOriginalArc)
			{
				damaged();
				component.Explosion();
			}
		}
	}

	private void damaged()
	{
		if (dead)
		{
			return;
		}
		AudioManager.Play("sfx_dlc_kog_rook_hurt");
		emitAudioFromObject.Add("sfx_dlc_kog_rook_hurt");
		hitFlash.Flash(0.7f);
		LevelProperties.ChessRook.States stateName = base.properties.CurrentState.stateName;
		base.properties.DealDamage((!PlayerManager.BothPlayersActive()) ? 10f : ChessKingLevelKing.multiplayerDamageNerf);
		if (base.properties.CurrentHealth <= 0f && !dead)
		{
			die();
		}
		else if (stateName == LevelProperties.ChessRook.States.PhaseThree || stateName == LevelProperties.ChessRook.States.PhaseFour)
		{
			if (transitionCoroutine != null)
			{
				StopCoroutine(transitionCoroutine);
				base.animator.ResetTrigger("Transition");
				transitionCoroutine = null;
				base.animator.Play("LateIntro", SparkLayerStateIndex);
			}
			hitSparkEffect.Create(base.transform.position);
			base.animator.Play("Hit2", 0, 0f);
		}
		else
		{
			base.animator.Play("Hit1", 0, 0f);
			base.animator.Play("HitSmoke", 3, 0f);
		}
	}

	public void OnPhaseChange()
	{
		StopAllCoroutines();
		StartAttacks();
		base.animator.ResetTrigger("SparkAttack");
		if (base.properties.CurrentState.stateName == LevelProperties.ChessRook.States.PhaseThree)
		{
			transitionCoroutine = StartCoroutine(transition_cr());
		}
	}

	private void Update()
	{
		if (damageDealer != null)
		{
			damageDealer.Update();
		}
	}

	private void animationEvent_IntroFinished()
	{
		StartAttacks();
		base.animator.Play("Intro", SparkLayerStateIndex);
		AudioManager.PlayLoop("sfx_dlc_kog_rook_grindingwheel_lowspeed");
		emitAudioFromObject.Add("sfx_dlc_kog_rook_grindingwheel_lowspeed");
		AudioManager.PlayLoop("sfx_dlc_kog_rook_grindingwheel_lowspeed_axeonwheel");
		AudioManager.FadeSFXVolume("sfx_dlc_kog_rook_grindingwheel_lowspeed_axeonwheel", 0.0001f, 0.0001f);
		emitAudioFromObject.Add("sfx_dlc_kog_rook_grindingwheel_lowspeed_axeonwheel");
		AudioManager.PlayLoop("sfx_dlc_kog_rook_sparks_loop");
	}

	private IEnumerator transition_cr()
	{
		yield return base.animator.WaitForAnimationToEnd(this, "Hit1");
		base.animator.SetTrigger("Transition");
		yield return base.animator.WaitForAnimationToEnd(this, "Transition");
		AudioManager.Stop("sfx_dlc_kog_rook_grindingwheel_lowspeed");
		AudioManager.PlayLoop("sfx_dlc_kog_rook_grindingwheel_highspeed");
		emitAudioFromObject.Add("sfx_dlc_kog_rook_grindingwheel_highspeed");
		transitionCoroutine = null;
	}

	private void animationEvent_EndEarlyPhaseSparks()
	{
		base.animator.Play("EarlyOutro", SparkLayerStateIndex);
	}

	private void animationEvent_StartLatePhaseSparks()
	{
		base.animator.Play("LateIntro", SparkLayerStateIndex);
	}

	private void StartAttacks()
	{
		StartCoroutine(pink_cannonballs_cr());
		StartCoroutine(regular_cannonballs_cr());
		if (base.properties.CurrentState.straightShooters.straightShotOn)
		{
			StartCoroutine(straight_shot_cr());
		}
	}

	private IEnumerator pink_cannonballs_cr()
	{
		LevelProperties.ChessRook.PinkCannonBall p = base.properties.CurrentState.pinkCannonBall;
		PatternString delayPattern = new PatternString(p.pinkShotDelayString);
		PatternString apexHeightPattern = new PatternString(p.pinkShotApexHeightString);
		PatternString targetPattern = new PatternString(p.pinkShotTargetString);
		while (true)
		{
			float delay = delayPattern.PopFloat();
			float apexHeight = apexHeightPattern.PopFloat();
			float targetDistance = targetPattern.PopFloat();
			yield return CupheadTime.WaitForSeconds(this, delay - 1f / 6f);
			spawnEffect.Play("Spawn" + ((!Rand.Bool()) ? "B" : "A") + "Head" + ((!headTypeB) ? "A" : "B"), 0, 0f);
			spawnEffect.Update(0f);
			yield return CupheadTime.WaitForSeconds(this, 1f / 6f);
			ChessRookLevelPinkCannonBall cannonBall = cannonballPink.Spawn();
			cannonBall.Create(cannonballSpawnRoot.position + base.transform.forward * 1E-05f * headZOffset, apexHeight, targetDistance, p);
			cannonBall.animator.Play((!headTypeB) ? "A" : "B");
			headTypeB = !headTypeB;
			headZOffset = (headZOffset + 1) % 10;
		}
	}

	private IEnumerator regular_cannonballs_cr()
	{
		LevelProperties.ChessRook.RegularCannonBall p = base.properties.CurrentState.regularCannonBall;
		PatternString delayPattern = new PatternString(p.cannonDelayString);
		PatternString apexHeightPattern = new PatternString(p.cannonApexHeightString);
		PatternString targetPattern = new PatternString(p.cannonTargetString);
		while (true)
		{
			float delay = delayPattern.PopFloat();
			float apexHeight = apexHeightPattern.PopFloat();
			float targetDistance = targetPattern.PopFloat();
			yield return CupheadTime.WaitForSeconds(this, delay - 1f / 6f);
			spawnEffect.Play("Spawn" + ((!Rand.Bool()) ? "B" : "A") + "Skull", 0, 0f);
			spawnEffect.Update(0f);
			yield return CupheadTime.WaitForSeconds(this, 1f / 6f);
			ChessRookLevelRegularCannonball cannonBall = cannonballRegular.Spawn();
			cannonBall.Create(cannonballSpawnRoot.position + base.transform.forward * 1E-05f * headZOffset, apexHeight, targetDistance, p);
			headZOffset = (headZOffset + 1) % 10;
		}
	}

	private IEnumerator straight_shot_cr()
	{
		LevelProperties.ChessRook.StraightShooters p = base.properties.CurrentState.straightShooters;
		PatternString sequencePattern = new PatternString(p.straightShotSeqString);
		PatternString delayPattern = new PatternString(p.straightShotDelayString);
		float EarlyPhaseTransitionOffset = 7f / 48f;
		Rangef EarlyPhaseShootOffsetRange = new Rangef(1f / 6f, 1f / 3f);
		while (true)
		{
			float delay2 = delayPattern.PopFloat();
			bool isEarlyPhase = base.properties.CurrentState.stateName == LevelProperties.ChessRook.States.Main || base.properties.CurrentState.stateName == LevelProperties.ChessRook.States.PhaseTwo;
			float shootDelay = 0f;
			if (isEarlyPhase)
			{
				shootDelay = Random.Range(EarlyPhaseShootOffsetRange.minimum, EarlyPhaseShootOffsetRange.maximum);
				delay2 -= EarlyPhaseTransitionOffset;
				delay2 -= shootDelay;
			}
			yield return CupheadTime.WaitForSeconds(this, delay2);
			if (isEarlyPhase)
			{
				base.animator.SetTrigger("SparkAttack");
				yield return base.animator.WaitForAnimationToEnd(this, "Idle1.Main");
				base.animator.Play("EarlyActiveA", SparkLayerStateIndex);
				yield return CupheadTime.WaitForSeconds(this, shootDelay);
			}
			char sequence = sequencePattern.PopLetter();
			int spawnPosIndex = 0;
			switch (sequence)
			{
			case 'T':
				spawnPosIndex = 0;
				break;
			case 'M':
				spawnPosIndex = 1;
				break;
			case 'B':
				spawnPosIndex = 2;
				break;
			}
			Vector3 position = straightShotSpawnPoints[spawnPosIndex].position;
			straightShot.Create(position, 180f, p.straightShotBulletSpeed);
			smokeEffect.Create(position);
			straightShotSparkEffect.Create(position);
			AudioManager.Play("sfx_dlc_kog_rook_sparks_singles");
		}
	}

	private void die()
	{
		dead = true;
		StopAllCoroutines();
		AudioManager.Play("sfx_dlc_kog_rook_death");
		AudioManager.Stop("sfx_dlc_kog_rook_sparks_loop");
		AudioManager.Stop("sfx_dlc_kog_rook_grindingwheel_lowspeed");
		AudioManager.Stop("sfx_dlc_kog_rook_grindingwheel_highspeed");
		AudioManager.Stop("sfx_dlc_kog_rook_grindingwheel_lowspeed_axeonwheel");
		base.animator.Play("Death", BaseLayerStateIndex);
		base.animator.Play("Off", SparkLayerStateIndex);
		base.animator.Play("Off", WheelLayerStateIndex);
		wheelRenderer.sortingOrder = 1000;
		wheelRenderer.sortingLayerName = "Foreground";
	}

	private void SFX_GrindAxe()
	{
		StartCoroutine(sfx_grind_axe_cr());
	}

	private IEnumerator sfx_grind_axe_cr()
	{
		AudioManager.FadeSFXVolume("sfx_dlc_kog_rook_grindingwheel_lowspeed_axeonwheel", 0.7f, 0.1f);
		yield return CupheadTime.WaitForSeconds(this, 0.3f);
		AudioManager.FadeSFXVolume("sfx_dlc_kog_rook_grindingwheel_lowspeed_axeonwheel", 0.0001f, 0.1f);
	}
}
