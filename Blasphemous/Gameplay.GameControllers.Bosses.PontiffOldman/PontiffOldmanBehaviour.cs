using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Bosses.Generic.Attacks;
using Gameplay.GameControllers.Bosses.PontiffOldman.AI;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Environment.AreaEffects;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffOldman;

public class PontiffOldmanBehaviour : EnemyBehaviour
{
	[Serializable]
	public struct PontiffOldmanPhases
	{
		public PontiffOldman_PHASES phaseId;

		public List<PontiffOldman_ATTACKS> availableAttacks;
	}

	public enum PontiffOldman_PHASES
	{
		FIRST,
		SECOND,
		LAST
	}

	[Serializable]
	public struct PontiffOldmanAttackConfig
	{
		public PontiffOldman_ATTACKS attackType;

		public float preparationSeconds;

		public float waitingSecondsAfterAttack;
	}

	public enum PontiffOldman_ATTACKS
	{
		COMBO_REST,
		REPOSITION,
		CAST_FIRE,
		CAST_TOXIC,
		CAST_LIGHTNING,
		CAST_MAGIC,
		REPOSITION_MID,
		COMBO_MAGIC_A,
		COMBO_MAGIC_B
	}

	[FoldoutGroup("Debug", true, 0)]
	public BOSS_STATES currentState;

	[FoldoutGroup("Debug", true, 0)]
	public PontiffOldman_ATTACKS lastAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossAreaSummonAttack lightningAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossAreaSummonAttack instantLightningAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossStraightProjectileAttack magicProjectileLauncher;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossMachinegunShooter fireMachineGun;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossStraightProjectileAttack toxicProjectileLauncher;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public PontiffOldmanBossfightPoints bossfightPoints;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public WindAreaEffect windArea;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public BossEnemySpawn ashChargerSpawn;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public SpriteRenderer castLoopFx;

	[FoldoutGroup("References", 0)]
	public List<MaterialsBySpellType> matsBySpell;

	public SpriteRenderer endingPanel;

	[SerializeField]
	[FoldoutGroup("Prefabs References", 0)]
	public GameObject fireSignPrefab;

	[SerializeField]
	[FoldoutGroup("Prefabs References", 0)]
	public GameObject lightningSignPrefab;

	[SerializeField]
	[FoldoutGroup("Prefabs References", 0)]
	public GameObject toxicSignPrefab;

	[SerializeField]
	[FoldoutGroup("Prefabs References", 0)]
	public GameObject magicSignPrefab;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private List<PontiffOldmanAttackConfig> attackLapses;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private List<PontiffOldmanPhases> phases;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private int maxHitsInRecovery = 3;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private LayerMask fightBoundariesLayerMask;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private float ashChargerSpawnLapse = 10f;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private float vanishSeconds = 0.5f;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private float minDistanceToVanish = 5f;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private Vector2 toxicOrbBounceBackOffset;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private float castSignOffsetDistance = 3f;

	public ParticleSystem endingParticles;

	private Transform currentTarget;

	private StateMachine<PontiffOldmanBehaviour> _fsm;

	private State<PontiffOldmanBehaviour> stAction;

	private State<PontiffOldmanBehaviour> stIntro;

	private State<PontiffOldmanBehaviour> stCast;

	private State<PontiffOldmanBehaviour> stDeath;

	private Coroutine currentCoroutine;

	private PontiffOldman_PHASES _currentPhase;

	[SerializeField]
	[FoldoutGroup("Combo settings", 0)]
	public List<PontiffOldman_ATTACKS> comboMagicA;

	[SerializeField]
	[FoldoutGroup("Combo settings", 0)]
	public List<PontiffOldman_ATTACKS> comboMagicB;

	private List<PontiffOldman_ATTACKS> currentlyAvailableAttacks;

	private List<PontiffOldman_ATTACKS> queuedActions;

	private RaycastHit2D[] results;

	private bool _recovering;

	private int _currentRecoveryHits;

	private bool _isBeingParried;

	private int _comboActionsRemaining;

	private float _ashChargerSpawnCounter;

	private bool _waitingForAnimationFinish;

	private bool _ashChargerActive;

	private Transform _lastRepositionPoint;

	public PontiffOldman PontiffOldman { get; set; }

	public event Action<PontiffOldmanBehaviour> OnActionFinished;

	public override void OnAwake()
	{
		base.OnAwake();
		stIntro = new PontiffOldman_StIntro();
		stAction = new PontiffOldman_StAction();
		stDeath = new PontiffOldman_StDeath();
		stCast = new PontiffOldman_StCasting();
		_fsm = new StateMachine<PontiffOldmanBehaviour>(this, stIntro);
		results = new RaycastHit2D[1];
		currentlyAvailableAttacks = new List<PontiffOldman_ATTACKS>
		{
			PontiffOldman_ATTACKS.CAST_FIRE,
			PontiffOldman_ATTACKS.CAST_MAGIC,
			PontiffOldman_ATTACKS.CAST_TOXIC
		};
	}

	public override void OnStart()
	{
		base.OnStart();
		PontiffOldman = (PontiffOldman)Entity;
		ChangeBossState(BOSS_STATES.WAITING);
		PoolManager.Instance.CreatePool(fireSignPrefab, 1);
		PoolManager.Instance.CreatePool(toxicSignPrefab, 1);
		PoolManager.Instance.CreatePool(lightningSignPrefab, 1);
		PoolManager.Instance.CreatePool(magicSignPrefab, 1);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		_fsm.DoUpdate();
	}

	private void CheckAshChargerSpawn()
	{
		_ashChargerSpawnCounter += Time.deltaTime;
		if (_ashChargerSpawnCounter >= ashChargerSpawnLapse)
		{
			SpawnNewCharger();
			_ashChargerSpawnCounter = 0f;
		}
	}

	private void SpawnNewCharger()
	{
		Vector2 vector = bossfightPoints.GetPointAwayOfPenitent(Core.Logic.Penitent.transform.position).position;
		ashChargerSpawn.Spawn(vector, GetDirToPenitent(vector));
	}

	private void SetCurrentCoroutine(Coroutine c)
	{
		if (currentCoroutine != null)
		{
			StopCoroutine(currentCoroutine);
		}
		currentCoroutine = c;
	}

	private void ChangeBossState(BOSS_STATES newState)
	{
		currentState = newState;
	}

	private void StartAttackAction()
	{
		ChangeBossState(BOSS_STATES.MID_ACTION);
		_comboActionsRemaining--;
		if (_comboActionsRemaining == 0)
		{
			QueuedActionsPush(PontiffOldman_ATTACKS.COMBO_REST);
		}
	}

	private void CancelCombo()
	{
		PontiffOldman.AnimatorInyector.CancelAll();
		queuedActions.Clear();
		_comboActionsRemaining = -1;
	}

	private void ActionFinished()
	{
		ChangeBossState(BOSS_STATES.AVAILABLE_FOR_ACTION);
		if (this.OnActionFinished != null)
		{
			this.OnActionFinished(this);
		}
	}

	public void LaunchAction(PontiffOldman_ATTACKS atk)
	{
		Debug.Log("TIME: " + Time.time + " Launching action: " + atk.ToString());
		switch (atk)
		{
		case PontiffOldman_ATTACKS.COMBO_REST:
			PontiffOldman.AnimatorInyector.ComboMode(active: false);
			StartWaitingPeriod(1.5f);
			break;
		case PontiffOldman_ATTACKS.REPOSITION:
			IssueReposition();
			break;
		case PontiffOldman_ATTACKS.CAST_FIRE:
			IssueCastFire();
			break;
		case PontiffOldman_ATTACKS.CAST_TOXIC:
			IssueCastToxic();
			break;
		case PontiffOldman_ATTACKS.CAST_LIGHTNING:
			IssueCastLightning();
			break;
		case PontiffOldman_ATTACKS.CAST_MAGIC:
			IssueCastMagic();
			break;
		case PontiffOldman_ATTACKS.REPOSITION_MID:
			IssueRepositionMid();
			break;
		}
		lastAttack = atk;
	}

	private bool LastAttackWasReposition()
	{
		return lastAttack == PontiffOldman_ATTACKS.REPOSITION || lastAttack == PontiffOldman_ATTACKS.REPOSITION_MID;
	}

	public PontiffOldman_ATTACKS GetNewAttack()
	{
		if (queuedActions != null && queuedActions.Count > 0)
		{
			return QueuedActionsPop();
		}
		PontiffOldman_ATTACKS[] array = new PontiffOldman_ATTACKS[currentlyAvailableAttacks.Count];
		currentlyAvailableAttacks.CopyTo(array);
		List<PontiffOldman_ATTACKS> list = new List<PontiffOldman_ATTACKS>(array);
		if (LastAttackWasReposition())
		{
			Debug.Log("<color=green> LAST ATTACK WAS REPOSITION, REMOVING THEM");
			list.Remove(PontiffOldman_ATTACKS.REPOSITION);
			list.Remove(PontiffOldman_ATTACKS.REPOSITION_MID);
		}
		else
		{
			list.Remove(lastAttack);
		}
		if (!IsInLightningPoint())
		{
			list.Remove(PontiffOldman_ATTACKS.CAST_LIGHTNING);
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	private bool IsInLightningPoint()
	{
		return _lastRepositionPoint == bossfightPoints.repositionPoints[0];
	}

	public IEnumerator WaitForState(State<PontiffOldmanBehaviour> st)
	{
		while (!_fsm.IsInState(st))
		{
			yield return null;
		}
	}

	public void LaunchRandomAction()
	{
		LaunchAction(GetNewAttack());
	}

	private void QueuedActionsPush(PontiffOldman_ATTACKS atk)
	{
		if (queuedActions == null)
		{
			queuedActions = new List<PontiffOldman_ATTACKS>();
		}
		queuedActions.Add(atk);
	}

	private PontiffOldman_ATTACKS QueuedActionsPop()
	{
		PontiffOldman_ATTACKS pontiffOldman_ATTACKS = queuedActions[0];
		queuedActions.Remove(pontiffOldman_ATTACKS);
		return pontiffOldman_ATTACKS;
	}

	public bool CanExecuteNewAction()
	{
		return currentState == BOSS_STATES.AVAILABLE_FOR_ACTION;
	}

	public float GetHealthPercentage()
	{
		return PontiffOldman.CurrentLife / PontiffOldman.Stats.Life.Base;
	}

	private void SetPhase(PontiffOldmanPhases p)
	{
		currentlyAvailableAttacks = p.availableAttacks;
		_currentPhase = p.phaseId;
	}

	private void ChangePhase(PontiffOldman_PHASES p)
	{
		PontiffOldmanPhases phase = phases.Find((PontiffOldmanPhases x) => x.phaseId == p);
		SetPhase(phase);
	}

	private void CheckNextPhase()
	{
	}

	public void IssueCombo(List<PontiffOldman_ATTACKS> testCombo)
	{
		for (int i = 0; i < testCombo.Count; i++)
		{
			QueuedActionsPush(testCombo[i]);
		}
		_comboActionsRemaining = testCombo.Count;
		StartWaitingPeriod(0.1f);
		PontiffOldman.AnimatorInyector.ComboMode(active: true);
	}

	private IEnumerator GetIntoStateAndCallback(State<PontiffOldmanBehaviour> newSt, float waitSeconds, Action callback)
	{
		_fsm.ChangeState(newSt);
		yield return new WaitForSeconds(2f);
		callback();
	}

	private void StartWaitingPeriod(float seconds)
	{
		Debug.Log(">> WAITING PERIOD: " + seconds);
		ChangeBossState(BOSS_STATES.WAITING);
		SetCurrentCoroutine(StartCoroutine(WaitingPeriodCoroutine(seconds, AfterWaitingPeriod)));
	}

	private IEnumerator WaitingPeriodCoroutine(float seconds, Action callback)
	{
		yield return new WaitForSeconds(seconds);
		callback();
	}

	private void AfterWaitingPeriod()
	{
		Debug.Log(">> READY FOR ACTION: " + Time.time);
		ActionFinished();
	}

	public void StartIntroSequence()
	{
		_fsm.ChangeState(stIntro);
		ActivateCollisions(activate: false);
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(IntroSequenceCoroutine()));
	}

	private IEnumerator IntroSequenceCoroutine()
	{
		ChangePhase(PontiffOldman_PHASES.FIRST);
		LookAtPenitent();
		yield return new WaitForSeconds(1.5f);
		base.BehaviourTree.StartBehaviour();
		ActivateCollisions(activate: true);
		StartWaitingPeriod(0.1f);
		_ashChargerActive = false;
	}

	private void ActivateCollisions(bool activate)
	{
		PontiffOldman.DamageArea.DamageAreaCollider.enabled = activate;
	}

	private void Shake()
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.5f, Vector3.down * 1f, 12, 0.2f, 0f, default(Vector3), 0f);
	}

	private void IssueCastFire()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(CastFireCoroutine()));
	}

	private IEnumerator CastFireCoroutine()
	{
		LookAtPenitent();
		PontiffOldman.AnimatorInyector.Cast(cast: true);
		CreateSpellFX(PONTIFF_SPELLS.FIRE, castSignOffsetDistance);
		yield return new WaitForSeconds(1f);
		float d = GetDirFromOrientation();
		fireMachineGun.transform.SetParent(null);
		fireMachineGun.transform.position = base.transform.position + Vector3.up * 8f;
		fireMachineGun.StartAttack(Core.Logic.Penitent.transform);
		yield return new WaitForSeconds(3f);
		PontiffOldman.AnimatorInyector.Cast(cast: false);
		OnCastFireEnds();
	}

	private void OnCastFireEnds()
	{
		StartWaitingPeriod(attackLapses.Find((PontiffOldmanAttackConfig x) => x.attackType == lastAttack).waitingSecondsAfterAttack);
	}

	private void IssueCastMagic()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(CastMagicCoroutine()));
	}

	private IEnumerator CastMagicCoroutine()
	{
		LookAtPenitent();
		PontiffOldman.AnimatorInyector.Cast(cast: true);
		CreateSpellFX(PONTIFF_SPELLS.MAGIC, 1f);
		yield return new WaitForSeconds(1f);
		int j = 4;
		Vector2 dir = GetDirFromOrientation() * Vector2.right;
		float offset = 0.7f;
		int castCounter2 = 0;
		for (int i = 0; i < j; i++)
		{
			castCounter2++;
			magicProjectileLauncher.Shoot(dir, Vector2.up * offset);
			if (castCounter2 > 3)
			{
				PontiffOldman.AnimatorInyector.Cast(cast: false);
			}
			yield return new WaitForSeconds(0.6f);
			castCounter2++;
			magicProjectileLauncher.Shoot(dir, Vector2.down * offset);
			yield return new WaitForSeconds(0.6f);
		}
		OnCastMagicEnds();
	}

	private void OnCastMagicEnds()
	{
		StartWaitingPeriod(attackLapses.Find((PontiffOldmanAttackConfig x) => x.attackType == lastAttack).waitingSecondsAfterAttack);
	}

	private void IssueCastLightning()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(CastLightningCoroutine()));
	}

	private IEnumerator CastLightningCoroutine()
	{
		LookAtPenitent();
		PontiffOldman.AnimatorInyector.Cast(cast: true);
		CreateSpellFX(PONTIFF_SPELLS.LIGHTNING, castSignOffsetDistance);
		yield return new WaitForSeconds(1f);
		windArea.SetMaxForce();
		PontiffOldman.Audio.PlayWind_AUDIO();
		int j = 5;
		for (int i = 0; i < j; i++)
		{
			if (i % 2 == 0)
			{
				lightningAttack.SummonAreaOnPoint(Core.Logic.Penitent.transform.position);
			}
			else
			{
				lightningAttack.SummonAreaOnPoint(Core.Logic.Penitent.transform.position + Vector3.right * 2f);
				lightningAttack.SummonAreaOnPoint(Core.Logic.Penitent.transform.position + Vector3.left * 2f);
			}
			yield return new WaitForSeconds(UnityEngine.Random.Range(1.25f, 2f));
			if ((float)i > (float)j / 2f)
			{
				PontiffOldman.AnimatorInyector.Cast(cast: false);
			}
		}
		PontiffOldman.AnimatorInyector.Cast(cast: false);
		windArea.SetMinForce();
		PontiffOldman.Audio.StopWind_AUDIO();
		OnCastLightningEnds();
	}

	private void CreateSpellFX(PONTIFF_SPELLS spellType, float signOffset)
	{
		float dirFromOrientation = GetDirFromOrientation();
		castLoopFx.material = matsBySpell.Find((MaterialsBySpellType x) => x.spellType == spellType).mat;
		castLoopFx.GetComponentInChildren<SpriteRenderer>().flipX = dirFromOrientation == -1f;
		StartCoroutine(CreateSignDelayed(spellType, signOffset, 1.2f));
	}

	private IEnumerator CreateSignDelayed(PONTIFF_SPELLS spellType, float signOffset, float delay)
	{
		yield return new WaitForSeconds(delay);
		GameObject signPrefab = null;
		switch (spellType)
		{
		case PONTIFF_SPELLS.FIRE:
			signPrefab = fireSignPrefab;
			break;
		case PONTIFF_SPELLS.LIGHTNING:
			signPrefab = lightningSignPrefab;
			break;
		case PONTIFF_SPELLS.TOXIC:
			signPrefab = toxicSignPrefab;
			break;
		case PONTIFF_SPELLS.MAGIC:
			signPrefab = magicSignPrefab;
			break;
		}
		float d = GetDirFromOrientation();
		GameObject go1 = PoolManager.Instance.ReuseObject(signPrefab, base.transform.position + d * signOffset * Vector3.right + Vector3.up * 2f, Quaternion.identity).GameObject;
		go1.GetComponentInChildren<SpriteRenderer>().flipX = d == -1f;
	}

	private void OnCastLightningEnds()
	{
		StartWaitingPeriod(attackLapses.Find((PontiffOldmanAttackConfig x) => x.attackType == lastAttack).waitingSecondsAfterAttack);
	}

	private void IssueCastToxic()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(CastToxicCoroutine()));
	}

	private IEnumerator CastToxicCoroutine()
	{
		LookAtPenitent();
		PontiffOldman.AnimatorInyector.Cast(cast: true);
		CreateSpellFX(PONTIFF_SPELLS.TOXIC, 1f);
		yield return new WaitForSeconds(1f);
		int j = 10;
		for (int i = 0; i < j; i++)
		{
			Transform TP = bossfightPoints.GetRandomToxicPoint();
			toxicProjectileLauncher.projectileSource = TP;
			Vector2 dir = GetDirToPenitent(TP.position);
			StraightProjectile p = toxicProjectileLauncher.Shoot(dir);
			AcceleratedProjectile ap = p.GetComponent<AcceleratedProjectile>();
			ap.SetAcceleration(dir.normalized * 6f);
			ap.SetBouncebackData(base.transform, toxicOrbBounceBackOffset);
			yield return new WaitForSeconds(0.8f);
		}
		PontiffOldman.AnimatorInyector.Cast(cast: false);
		OnCastToxicEnds();
	}

	private Vector2 GetDirToPenitent(Vector3 from)
	{
		return Core.Logic.Penitent.transform.position - from;
	}

	private void OnCastToxicEnds()
	{
		StartWaitingPeriod(attackLapses.Find((PontiffOldmanAttackConfig x) => x.attackType == lastAttack).waitingSecondsAfterAttack);
	}

	private void IssueReposition()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(RepositionFarCoroutine()));
	}

	private void IssueRepositionMid()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(RepositionCoroutine((_lastRepositionPoint = bossfightPoints.GetPointInCenter()).position)));
	}

	private IEnumerator RepositionFarCoroutine()
	{
		PontiffOldman.AnimatorInyector.Vanish(dissapear: true);
		ActivateCollisions(activate: false);
		yield return StartCoroutine(BlockUntilAnimationEnds());
		LookAtPenitent();
		yield return new WaitForSeconds(vanishSeconds);
		Transform p = bossfightPoints.GetPointAwayOfPenitent(Core.Logic.Penitent.transform.position);
		base.transform.position = p.position;
		_lastRepositionPoint = p;
		LookAtPenitent();
		PontiffOldman.AnimatorInyector.Vanish(dissapear: false);
		EndReposition();
	}

	private IEnumerator RepositionCoroutine(Vector2 newPosition)
	{
		PontiffOldman.AnimatorInyector.Vanish(dissapear: true);
		yield return StartCoroutine(BlockUntilAnimationEnds());
		base.transform.position = newPosition;
		LookAtPenitent();
		yield return new WaitForSeconds(vanishSeconds);
		PontiffOldman.AnimatorInyector.Vanish(dissapear: false);
		EndReposition();
	}

	private void EndReposition()
	{
		ActivateCollisions(activate: true);
		StartWaitingPeriod(attackLapses.Find((PontiffOldmanAttackConfig x) => x.attackType == lastAttack).waitingSecondsAfterAttack);
	}

	public void OnEnterCast()
	{
		_fsm.ChangeState(stCast);
	}

	public void OnExitCast()
	{
		_fsm.ChangeState(stAction);
	}

	public void OnVanishEnds()
	{
		_waitingForAnimationFinish = false;
	}

	private IEnumerator BlockUntilAnimationEnds()
	{
		_waitingForAnimationFinish = true;
		while (_waitingForAnimationFinish)
		{
			yield return null;
		}
		Debug.Log("<color=yellow>Melee animation ended</color>");
	}

	public void OnHitReactionAnimationCompleted()
	{
		Debug.Log("HIT REACTION COMPLETED. RECOVERING FALSE");
		SetRecovering(recovering: false);
		_currentRecoveryHits = 0;
	}

	public void AttackDisplacement(float duration = 0.4f, float displacement = 2f, bool trail = true)
	{
		SetGhostTrail(trail);
		PontiffOldman.DamageByContact = false;
		Ease ease = Ease.OutQuad;
		float num = ((Entity.Status.Orientation != 0) ? (-1f) : 1f);
		num *= displacement;
		Vector2 dir = Vector2.right * num;
		dir = ClampToFightBoundaries(dir);
		base.transform.DOMove(base.transform.position + (Vector3)dir, duration).SetEase(ease).OnComplete(delegate
		{
			AfterDisplacement();
		});
	}

	private void AfterDisplacement()
	{
		PontiffOldman.DamageByContact = true;
		SetGhostTrail(active: false);
	}

	public void BackDisplacement(float duration = 0.4f, float displacement = 2f)
	{
		SetGhostTrail(active: true);
		PontiffOldman.DamageByContact = false;
		Ease ease = Ease.OutQuad;
		float num = ((Entity.Status.Orientation != 0) ? 1f : (-1f));
		num *= displacement;
		Vector2 dir = Vector2.right * num;
		dir = ClampToFightBoundaries(dir);
		base.transform.DOMove(base.transform.position + (Vector3)dir, duration).SetEase(ease).OnComplete(delegate
		{
			AfterDisplacement();
		});
	}

	public bool IsRecovering()
	{
		return _recovering;
	}

	public void SetRecovering(bool recovering)
	{
		_recovering = recovering;
	}

	public bool CloseToPointX(Vector2 p, float closeDistance = 0.1f)
	{
		return Mathf.Abs(p.x - base.transform.position.x) < closeDistance;
	}

	public bool CloseToTarget(float closeDistance = 0.5f)
	{
		Transform target = GetTarget();
		return Mathf.Abs(target.position.x - base.transform.position.x) < closeDistance;
	}

	public void ChangeToAction()
	{
		_fsm.ChangeState(stAction);
	}

	private void StopAllMachineGuns()
	{
		fireMachineGun.StopMachinegun();
	}

	public void Death()
	{
		SetGhostTrail(active: false);
		StopAllCoroutines();
		GameplayUtils.DestroyAllProjectiles();
		Core.Logic.Penitent.Status.Invulnerable = true;
		StopAllMachineGuns();
		GameplayUtils.DestroyAllProjectiles();
		base.transform.DOKill(complete: true);
		StopBehaviour();
		PontiffOldman.AnimatorInyector.Death();
		_fsm.ChangeState(stDeath);
		StartCoroutine(TransitionCoroutine());
	}

	private IEnumerator TransitionCoroutine()
	{
		endingParticles.Play();
		endingPanel.DOFade(0.3f, 1f).SetEase(Ease.InOutQuad).OnComplete(delegate
		{
			endingPanel.DOFade(0.9f, 2f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
		});
		yield return new WaitForSeconds(1f);
		Core.Logic.Penitent.Status.Invulnerable = false;
	}

	public override void Idle()
	{
		StopMovement();
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public void LookAtPenitent()
	{
		if ((bool)Core.Logic.Penitent)
		{
			LookAtTarget(Core.Logic.Penitent.transform.position);
		}
	}

	public override void Chase(Transform targetPosition)
	{
	}

	public bool CanChase()
	{
		return true;
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void Damage()
	{
		CheckNextPhase();
		if (_currentRecoveryHits < maxHitsInRecovery)
		{
			StopAllCoroutines();
			PontiffOldman.AnimatorInyector.Hurt();
			base.transform.DOKill(complete: true);
			LookAtPenitent();
			float displacement = ((!PontiffOldman.IsGuarding) ? 0.4f : 1.2f);
			PontiffOldman.AnimatorInyector.Cast(cast: false);
			BackDisplacement(0.3f, displacement);
			Debug.Log("HURT //" + _currentRecoveryHits + " // Recovering = " + ((!_recovering) ? "FALSE" : "TRUE"));
			_currentRecoveryHits++;
			if (_currentRecoveryHits >= maxHitsInRecovery)
			{
				IssueReposition();
			}
			else
			{
				StartWaitingPeriod(1f);
			}
		}
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		PontiffOldman.SetOrientation((!(targetPos.x > PontiffOldman.transform.position.x)) ? EntityOrientation.Left : EntityOrientation.Right);
	}

	public override void StopMovement()
	{
	}

	public void SetGhostTrail(bool active)
	{
		PontiffOldman.GhostTrail.EnableGhostTrail = active;
	}

	private float GetDirFromOrientation()
	{
		return (Entity.Status.Orientation != 0) ? (-1f) : 1f;
	}

	private Vector2 ClampToFightBoundaries(Vector2 dir)
	{
		Vector2 vector = dir;
		Debug.Log("<color=cyan>DRAWING DIR LINE IN GREEN</color>");
		Debug.DrawLine(base.transform.position, base.transform.position + (Vector3)vector, Color.green, 5f);
		if (Physics2D.RaycastNonAlloc(base.transform.position, dir, results, dir.magnitude, fightBoundariesLayerMask) > 0)
		{
			Debug.DrawLine(base.transform.position, results[0].point, Color.red, 5f);
			vector = vector.normalized * results[0].distance;
			vector *= 0.9f;
			Debug.Log("<color=cyan>CLAMPING DISPLACEMENT</color>");
		}
		return vector;
	}

	public void OnDrawGizmos()
	{
	}
}
