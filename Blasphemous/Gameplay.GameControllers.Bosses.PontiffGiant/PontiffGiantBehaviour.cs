using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Bosses.Generic.Attacks;
using Gameplay.GameControllers.Bosses.PontiffGiant.AI;
using Gameplay.GameControllers.Bosses.PontiffOldman;
using Gameplay.GameControllers.Bosses.PontiffSword;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Environment;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffGiant;

public class PontiffGiantBehaviour : EnemyBehaviour
{
	[Serializable]
	public struct PontiffGiantPhases
	{
		public PontiffGiant_PHASES phaseId;

		public List<PontiffGiant_ATTACKS> availableAttacks;
	}

	public enum PontiffGiant_PHASES
	{
		FIRST,
		SECOND,
		LAST
	}

	[Serializable]
	public struct PontiffGiantAttackConfig
	{
		public PontiffGiant_ATTACKS attackType;

		public float preparationSeconds;

		public float waitingSecondsAfterAttack;
	}

	public enum PontiffGiant_ATTACKS
	{
		COMBO_REST,
		SLASH,
		CAST_FIRE,
		CAST_TOXIC,
		CAST_LIGHTNING,
		CAST_MAGIC,
		PLUNGE,
		REVIVE_SWORD,
		DOUBLE_SLASH,
		BEAM_ATTACK_1,
		BEAM_ATTACK_2
	}

	[FoldoutGroup("Debug", true, 0)]
	public BOSS_STATES currentState;

	[FoldoutGroup("Debug", true, 0)]
	public PontiffGiant_ATTACKS lastAttack;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private PontiffSwordBehaviour sword;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossAreaSummonAttack lightningAreas;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossAreaSummonAttack magicShockwave;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossStraightProjectileAttack toxicProjectileLauncher;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossStraightProjectileAttack magicProjectileLauncher;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public PontiffGiantBossfightPoints bossfightPoints;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public List<BossMachinegunShooter> machineGuns;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public GameObject deathExplosionPrefab;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public AshPlatformFightManager ashPlatformManager;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private BossAreaSummonAttack beamAttack;

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
	[FoldoutGroup("Prefabs References", 0)]
	public GameObject swordMagicPrefab;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private List<PontiffGiantPhases> phases;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private int maxHitsInRecovery = 3;

	[SerializeField]
	[FoldoutGroup("Design settings", 0)]
	private LayerMask fightBoundariesLayerMask;

	public UnityEngine.Animator deathEffectAnim;

	private Transform currentTarget;

	private StateMachine<PontiffGiantBehaviour> _fsm;

	private State<PontiffGiantBehaviour> stAction;

	private State<PontiffGiantBehaviour> stIntro;

	private State<PontiffGiantBehaviour> stCast;

	private State<PontiffGiantBehaviour> stDeath;

	private Coroutine currentCoroutine;

	private PontiffGiant_PHASES _currentPhase;

	[SerializeField]
	[FoldoutGroup("Combo settings", 0)]
	public List<PontiffGiant_ATTACKS> comboMagicA;

	[SerializeField]
	[FoldoutGroup("Combo settings", 0)]
	public List<PontiffGiant_ATTACKS> comboMagicB;

	private List<PontiffGiant_ATTACKS> currentlyAvailableAttacks;

	private List<PontiffGiant_ATTACKS> queuedActions;

	private RaycastHit2D[] results;

	public float maxSwordDeathTime = 10f;

	private bool _recovering;

	private int _currentRecoveryHits;

	private bool _isBeingParried;

	private int _comboActionsRemaining;

	private bool _waitingForAnimationFinish;

	private float _swordRevivalCounter;

	private int _swordAttacksRemaining;

	private bool _swordMoveFinished;

	public PontiffGiant PontiffGiant { get; set; }

	public event Action<PontiffGiantBehaviour> OnActionFinished;

	public override void OnAwake()
	{
		base.OnAwake();
		stIntro = new PontiffGiant_StIntro();
		stAction = new PontiffGiant_StAction();
		stDeath = new PontiffGiant_StDeath();
		_fsm = new StateMachine<PontiffGiantBehaviour>(this, stIntro);
		results = new RaycastHit2D[1];
		currentlyAvailableAttacks = new List<PontiffGiant_ATTACKS>
		{
			PontiffGiant_ATTACKS.CAST_FIRE,
			PontiffGiant_ATTACKS.CAST_MAGIC,
			PontiffGiant_ATTACKS.CAST_TOXIC
		};
	}

	public override void OnStart()
	{
		base.OnStart();
		PontiffGiant = (PontiffGiant)Entity;
		ChangeBossState(BOSS_STATES.WAITING);
		sword.OnSwordDestroyed += OnSwordDestroyed;
		PontiffGiant.IsGuarding = true;
		PoolManager.Instance.CreatePool(deathExplosionPrefab, 10);
		PoolManager.Instance.CreatePool(fireSignPrefab, 3);
		PoolManager.Instance.CreatePool(toxicSignPrefab, 3);
		PoolManager.Instance.CreatePool(lightningSignPrefab, 3);
		PoolManager.Instance.CreatePool(magicSignPrefab, 3);
		PoolManager.Instance.CreatePool(swordMagicPrefab, 3);
		StartIntroSequence();
	}

	private void OnSwordDestroyed()
	{
		PontiffGiant.AnimatorInyector.Open(open: true);
		StopAllCoroutines();
		_swordRevivalCounter = maxSwordDeathTime;
		StartWaitingPeriod(1.5f);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		_fsm.DoUpdate();
		if (_swordRevivalCounter > 0f && _fsm.IsInState(stAction))
		{
			_swordRevivalCounter -= Time.deltaTime;
		}
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
			QueuedActionsPush(PontiffGiant_ATTACKS.COMBO_REST);
		}
	}

	private void CancelCombo()
	{
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

	public void LaunchAction(PontiffGiant_ATTACKS atk)
	{
		switch (atk)
		{
		case PontiffGiant_ATTACKS.COMBO_REST:
			StartWaitingPeriod(1.5f);
			break;
		case PontiffGiant_ATTACKS.CAST_FIRE:
			IssueCastFire();
			break;
		case PontiffGiant_ATTACKS.CAST_TOXIC:
			IssueCastToxic();
			break;
		case PontiffGiant_ATTACKS.CAST_LIGHTNING:
			IssueCastLightning();
			break;
		case PontiffGiant_ATTACKS.CAST_MAGIC:
			IssueCastMagic();
			break;
		case PontiffGiant_ATTACKS.REVIVE_SWORD:
			PontiffGiant.AnimatorInyector.Open(open: false);
			sword.PontiffSword.Revive();
			StartWaitingPeriod(2f);
			break;
		case PontiffGiant_ATTACKS.SLASH:
			IssueSlash();
			break;
		case PontiffGiant_ATTACKS.DOUBLE_SLASH:
			IssueDoubleSlash();
			break;
		case PontiffGiant_ATTACKS.PLUNGE:
			IssuePlunge();
			break;
		case PontiffGiant_ATTACKS.BEAM_ATTACK_1:
			IssueCastBeam(0);
			break;
		case PontiffGiant_ATTACKS.BEAM_ATTACK_2:
			IssueCastBeam(1);
			break;
		}
		lastAttack = atk;
	}

	public PontiffGiant_ATTACKS GetNewAttack()
	{
		if (queuedActions != null && queuedActions.Count > 0)
		{
			return QueuedActionsPop();
		}
		if (_swordRevivalCounter < 0f)
		{
			_swordRevivalCounter = 0f;
			LaunchAction(PontiffGiant_ATTACKS.REVIVE_SWORD);
		}
		PontiffGiant_ATTACKS[] array = new PontiffGiant_ATTACKS[currentlyAvailableAttacks.Count];
		currentlyAvailableAttacks.CopyTo(array);
		List<PontiffGiant_ATTACKS> list = new List<PontiffGiant_ATTACKS>(array);
		if (sword.currentSwordState == SWORD_STATES.DESTROYED)
		{
			list.Remove(PontiffGiant_ATTACKS.SLASH);
			list.Remove(PontiffGiant_ATTACKS.DOUBLE_SLASH);
			list.Remove(PontiffGiant_ATTACKS.PLUNGE);
		}
		if (list.Count > 1)
		{
			list.Remove(lastAttack);
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public IEnumerator WaitForState(State<PontiffGiantBehaviour> st)
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

	private void QueuedActionsPush(PontiffGiant_ATTACKS atk)
	{
		if (queuedActions == null)
		{
			queuedActions = new List<PontiffGiant_ATTACKS>();
		}
		queuedActions.Add(atk);
	}

	private PontiffGiant_ATTACKS QueuedActionsPop()
	{
		PontiffGiant_ATTACKS pontiffGiant_ATTACKS = queuedActions[0];
		queuedActions.Remove(pontiffGiant_ATTACKS);
		return pontiffGiant_ATTACKS;
	}

	public bool CanExecuteNewAction()
	{
		return currentState == BOSS_STATES.AVAILABLE_FOR_ACTION;
	}

	public float GetHealthPercentage()
	{
		return PontiffGiant.CurrentLife / PontiffGiant.Stats.Life.Base;
	}

	private void SetPhase(PontiffGiantPhases p)
	{
		currentlyAvailableAttacks = p.availableAttacks;
		_currentPhase = p.phaseId;
	}

	private void ChangePhase(PontiffGiant_PHASES p)
	{
		PontiffGiantPhases phase = phases.Find((PontiffGiantPhases x) => x.phaseId == p);
		SetPhase(phase);
	}

	private void CheckNextPhase()
	{
		float healthPercentage = GetHealthPercentage();
		switch (_currentPhase)
		{
		case PontiffGiant_PHASES.FIRST:
			if (healthPercentage < 0.6f)
			{
				ChangePhase(PontiffGiant_PHASES.SECOND);
			}
			break;
		case PontiffGiant_PHASES.SECOND:
			if (healthPercentage < 0.3f)
			{
				ChangePhase(PontiffGiant_PHASES.LAST);
			}
			break;
		}
	}

	public void IssueCombo(List<PontiffGiant_ATTACKS> testCombo)
	{
		for (int i = 0; i < testCombo.Count; i++)
		{
			QueuedActionsPush(testCombo[i]);
		}
		_comboActionsRemaining = testCombo.Count;
		StartWaitingPeriod(0.1f);
	}

	private IEnumerator GetIntoStateAndCallback(State<PontiffGiantBehaviour> newSt, float waitSeconds, Action callback)
	{
		_fsm.ChangeState(newSt);
		yield return new WaitForSeconds(2f);
		callback();
	}

	private void StartWaitingPeriod(float seconds)
	{
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
		ChangePhase(PontiffGiant_PHASES.FIRST);
		PontiffGiant.AnimatorInyector.IntroBlend();
		yield return new WaitForSeconds(6.5f);
		base.BehaviourTree.StartBehaviour();
		ActivateCollisions(activate: true);
		sword.PontiffSword.Revive();
		_fsm.ChangeState(stAction);
		StartWaitingPeriod(1.5f);
		ashPlatformManager.Activate();
	}

	private void ActivateCollisions(bool activate)
	{
		PontiffGiant.DamageArea.DamageAreaCollider.enabled = activate;
	}

	private void Shake()
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.5f, Vector3.down * 1f, 12, 0.2f, 0f, default(Vector3), 0f);
	}

	private void IssueSlash()
	{
		StartAttackAction();
		sword.ChangeSwordState(SWORD_STATES.MID_ACTION);
		SetCurrentCoroutine(StartCoroutine(SlashCoroutine()));
	}

	private IEnumerator SlashCoroutine()
	{
		Vector2 swordOffset = new Vector2(4f, 6f);
		Vector2 dir = GetDirToPenitent(sword.transform.position);
		Vector3 offset = swordOffset;
		float sign = 0f - Mathf.Sign(dir.x);
		offset.x *= sign;
		sword.Move(Core.Logic.Penitent.transform.position + offset, 1.5f, OnSwordMoveFinished);
		yield return new WaitForSeconds(0.1f);
	}

	private void OnSwordMoveFinished()
	{
		sword.OnSlashFinished += Sword_OnSlashFinished;
		sword.Slash();
	}

	private void Sword_OnSlashFinished()
	{
		sword.OnSlashFinished -= Sword_OnSlashFinished;
		sword.BackToFlyingAround();
		StartWaitingPeriod(0.5f);
	}

	private void IssueDoubleSlash()
	{
		StartAttackAction();
		sword.ChangeSwordState(SWORD_STATES.MID_ACTION);
		_swordAttacksRemaining = 2;
		SetCurrentCoroutine(StartCoroutine(DoubleSlashCoroutine()));
	}

	private IEnumerator DoubleSlashCoroutine()
	{
		while (_swordAttacksRemaining > 0)
		{
			Vector2 swordOffset = new Vector2(4f, 6f);
			Vector2 dir = GetDirToPenitent(sword.transform.position);
			Vector3 offset = swordOffset;
			float sign = 0f - Mathf.Sign(dir.x);
			offset.x *= sign;
			_swordMoveFinished = false;
			sword.Move(Core.Logic.Penitent.transform.position + offset, 1.5f, OnSwordDoubleSlashMovementFinished);
			yield return WaitForSwordMovement();
			sword.Slash();
			yield return new WaitForSeconds(2f);
			_swordAttacksRemaining--;
		}
		Sword_OnDoubleSlashFinished();
	}

	private IEnumerator WaitForSwordMovement()
	{
		while (!_swordMoveFinished)
		{
			yield return null;
		}
	}

	private void OnSwordDoubleSlashMovementFinished()
	{
		_swordMoveFinished = true;
		sword.OnSlashFinished += Sword_OnDoubleSlashFinished;
	}

	private void Sword_OnDoubleSlashFinished()
	{
		sword.OnSlashFinished -= Sword_OnDoubleSlashFinished;
		sword.BackToFlyingAround();
		StartWaitingPeriod(0.5f);
	}

	private void IssueCastFire()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(CastFireCoroutine()));
	}

	private IEnumerator CastFireCoroutine()
	{
		CreateSpellFX(PONTIFF_SPELLS.FIRE, 2f);
		yield return new WaitForSeconds(1f);
		float d = GetDirFromOrientation();
		float machineGunDelay = 1f;
		foreach (BossMachinegunShooter item in machineGuns)
		{
			item.StartAttack(Core.Logic.Penitent.transform);
			yield return new WaitForSeconds(machineGunDelay);
		}
		yield return new WaitForSeconds(5.5f);
		OnCastFireEnds();
	}

	private void CreateFireSign()
	{
		float dirFromOrientation = GetDirFromOrientation();
		PoolManager.Instance.ReuseObject(fireSignPrefab, base.transform.position + dirFromOrientation * Vector3.right, Quaternion.identity);
	}

	private void OnCastFireEnds()
	{
		StartWaitingPeriod(0.1f);
	}

	private void IssueCastMagic()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(CastMagicCoroutine()));
	}

	private void IssueCastBeam(int type)
	{
		StartAttackAction();
		if (type == 0)
		{
			SetCurrentCoroutine(StartCoroutine(CastBeams()));
		}
		else
		{
			SetCurrentCoroutine(StartCoroutine(CastBeamsAlternate()));
		}
	}

	private IEnumerator CastBeams()
	{
		CreateSpellFX(PONTIFF_SPELLS.MAGIC, 3f);
		PontiffGiant.Audio.PlayPurpleSpell_AUDIO();
		yield return new WaitForSeconds(1f);
		float timeBetweenBeams = 3f;
		beamAttack.SummonAreaOnPoint(bossfightPoints.beamPoints[0].position);
		yield return new WaitForSeconds(timeBetweenBeams);
		beamAttack.SummonAreaOnPoint(bossfightPoints.beamPoints[1].position);
		beamAttack.SummonAreaOnPoint(bossfightPoints.beamPoints[2].position);
		yield return new WaitForSeconds(timeBetweenBeams);
		beamAttack.SummonAreaOnPoint(bossfightPoints.beamPoints[3].position);
		beamAttack.SummonAreaOnPoint(bossfightPoints.beamPoints[4].position);
		OnCastMagicEnds();
	}

	private IEnumerator CastBeamsAlternate()
	{
		CreateSpellFX(PONTIFF_SPELLS.MAGIC, 3f);
		PontiffGiant.Audio.PlayPurpleSpell_AUDIO();
		yield return new WaitForSeconds(1f);
		float timeBetweenBeams = 2f;
		beamAttack.SummonAreaOnPoint(bossfightPoints.beamPoints[3].position);
		beamAttack.SummonAreaOnPoint(bossfightPoints.beamPoints[4].position);
		yield return new WaitForSeconds(timeBetweenBeams);
		beamAttack.SummonAreaOnPoint(bossfightPoints.beamPoints[1].position);
		beamAttack.SummonAreaOnPoint(bossfightPoints.beamPoints[2].position);
		yield return new WaitForSeconds(timeBetweenBeams);
		beamAttack.SummonAreaOnPoint(bossfightPoints.beamPoints[0].position);
		OnCastMagicEnds();
	}

	private IEnumerator MagicExplosion(Vector2 attackPoint, float delay)
	{
		GameObject go = PoolManager.Instance.ReuseObject(swordMagicPrefab, attackPoint, Quaternion.identity).GameObject;
		yield return new WaitForSeconds(delay);
		MagicProyectileExplosion(attackPoint);
	}

	private IEnumerator CastMagicCoroutine()
	{
		CreateSpellFX(PONTIFF_SPELLS.MAGIC, 3f);
		PontiffGiant.Audio.PlayPurpleSpell_AUDIO();
		float explosionDelay = 0.8f;
		yield return new WaitForSeconds(1f);
		Vector2 pos3 = bossfightPoints.magicPoints[1].position;
		pos3 = bossfightPoints.magicPoints[1].position;
		StartCoroutine(MagicExplosion(pos3, explosionDelay));
		pos3 = bossfightPoints.magicPoints[2].position;
		StartCoroutine(MagicExplosion(pos3, explosionDelay));
		OnCastMagicEnds();
	}

	private void CreateSpellFX(PONTIFF_SPELLS spellType, float signOffset)
	{
		Vector3 position = bossfightPoints.magicPoints[1].position;
		StartCoroutine(CreateSignDelayed(spellType, position, 0f));
		position = bossfightPoints.magicPoints[2].position;
		StartCoroutine(CreateSignDelayed(spellType, position, 0f));
	}

	private void MagicProyectileExplosion(Vector2 point)
	{
		Vector2 vector = new Vector2(-1f, 0f);
		magicProjectileLauncher.projectileSource = magicProjectileLauncher.transform;
		magicProjectileLauncher.transform.position = point;
		magicProjectileLauncher.Shoot(vector.normalized);
		vector = new Vector2(1f, 0f);
		magicProjectileLauncher.Shoot(vector.normalized);
		vector = new Vector2(1f, 1f);
		magicProjectileLauncher.Shoot(vector.normalized);
		vector = new Vector2(-1f, 1f);
		magicProjectileLauncher.Shoot(vector.normalized);
		vector = new Vector2(1f, -1f);
		magicProjectileLauncher.Shoot(vector.normalized);
		vector = new Vector2(-1f, -1f);
		magicProjectileLauncher.Shoot(vector.normalized);
	}

	private IEnumerator CreateSignDelayed(PONTIFF_SPELLS spellType, Vector2 position, float delay)
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
		GameObject go1 = PoolManager.Instance.ReuseObject(signPrefab, position, Quaternion.identity).GameObject;
	}

	private void OnCastMagicEnds()
	{
		StartWaitingPeriod(2f);
	}

	private void IssuePlunge()
	{
		StartAttackAction();
		sword.ChangeSwordState(SWORD_STATES.MID_ACTION);
		SetCurrentCoroutine(StartCoroutine(PlungeCoroutine()));
	}

	private IEnumerator PlungeCoroutine()
	{
		sword.OnPlungeFinished += OnSwordPlungeFinished;
		sword.Plunge();
		yield return new WaitForSeconds(6f);
		OnPlungeEnds();
	}

	private void OnSwordPlungeFinished()
	{
		sword.OnPlungeFinished -= OnSwordPlungeFinished;
		Vector2 vector = sword.transform.position;
		magicShockwave.transform.position = vector + Vector2.up;
		magicShockwave.SummonAreas(Vector2.right);
		magicShockwave.SummonAreas(Vector2.left);
		if (sword.currentSwordState != SWORD_STATES.DESTROYED)
		{
			sword.BackToFlyingAround();
		}
	}

	private void OnPlungeEnds()
	{
		StartWaitingPeriod(0.1f);
	}

	private void IssueCastLightning()
	{
		StartAttackAction();
		if (sword.currentSwordState == SWORD_STATES.DESTROYED)
		{
			SetCurrentCoroutine(StartCoroutine(CastSingleLightningCoroutine()));
		}
		else
		{
			SetCurrentCoroutine(StartCoroutine(CastLightningCoroutine()));
		}
	}

	private IEnumerator CastSingleLightningCoroutine()
	{
		CreateSpellFX(PONTIFF_SPELLS.LIGHTNING, 0f);
		PontiffGiant.Audio.PlayBlueSpell_AUDIO();
		yield return new WaitForSeconds(1f);
		Vector2 pos3 = Core.Logic.Penitent.transform.position;
		lightningAreas.SummonAreaOnPoint(pos3);
		yield return new WaitForSeconds(2f);
		pos3 = Core.Logic.Penitent.transform.position;
		lightningAreas.SummonAreaOnPoint(pos3);
		yield return new WaitForSeconds(2f);
		pos3 = Core.Logic.Penitent.transform.position;
		lightningAreas.SummonAreaOnPoint(pos3);
		OnCastLightningEnds();
	}

	private IEnumerator CastLightningCoroutine()
	{
		CreateSpellFX(PONTIFF_SPELLS.LIGHTNING, 0f);
		PontiffGiant.Audio.PlayBlueSpell_AUDIO();
		yield return new WaitForSeconds(1f);
		lightningAreas.transform.position = bossfightPoints.leftLimitTransform.position;
		lightningAreas.SummonAreas(Vector2.right);
		yield return new WaitForSeconds(3f);
		lightningAreas.transform.position = bossfightPoints.rightLimitTransform.position;
		lightningAreas.SummonAreas(Vector2.left);
		yield return new WaitForSeconds(1f);
		OnCastLightningEnds();
	}

	private void OnCastLightningEnds()
	{
		StartWaitingPeriod(1f);
	}

	private void IssueCastToxic()
	{
		StartAttackAction();
		SetCurrentCoroutine(StartCoroutine(CastToxicCoroutine()));
	}

	private Vector2 GetPointAroundPenitent()
	{
		float num = 5f;
		return Core.Logic.Penitent.transform.position + (Vector3)GetRandomVector(-0.8f, 0.8f, 0f, 1f).normalized * num;
	}

	private Vector2 GetRandomVector(float minRandomX, float maxRandomX, float minRandomY, float maxRandomY)
	{
		return new Vector2(UnityEngine.Random.Range(minRandomX, maxRandomX), UnityEngine.Random.Range(minRandomY, maxRandomY));
	}

	private IEnumerator CastToxicCoroutine()
	{
		PontiffGiant.Audio.PlayGreenSpell_AUDIO();
		CreateSpellFX(PONTIFF_SPELLS.TOXIC, 0f);
		yield return new WaitForSeconds(1f);
		int j = 5;
		for (int i = 0; i < j; i++)
		{
			Vector2 TP = (Vector2)bossfightPoints.fightCenterTransform.position + Vector2.up * 8.5f - Vector2.right * 9f;
			TP += Vector2.right * i * 4.5f;
			toxicProjectileLauncher.transform.position = TP;
			Vector2 dir = GetDirToPenitent(TP);
			StraightProjectile p = toxicProjectileLauncher.Shoot(dir);
			AcceleratedProjectile ap = p as AcceleratedProjectile;
			ap.SetAcceleration(dir.normalized * 4f);
			if (sword.currentSwordState == SWORD_STATES.FLYING_AROUND)
			{
				ap.bounceBackToTarget = true;
				ap.SetBouncebackData(sword.PontiffSword.damageEffectScript.transform, Vector2.zero);
			}
			else
			{
				ap.bounceBackToTarget = false;
			}
			yield return new WaitForSeconds(0.7f);
		}
		OnCastToxicEnds();
	}

	private Vector2 GetDirToPenitent(Vector3 from)
	{
		return Core.Logic.Penitent.transform.position - from;
	}

	private void OnCastToxicEnds()
	{
		StartWaitingPeriod(2f);
	}

	public void OnMaskOpened()
	{
		PontiffGiant.IsGuarding = false;
		ashPlatformManager.heightLimitOn = false;
	}

	private void LaunchMaskOpenAttack()
	{
		magicProjectileLauncher.projectileSource = magicProjectileLauncher.transform;
		GameObject gameObject = PoolManager.Instance.ReuseObject(swordMagicPrefab, magicProjectileLauncher.projectileSource.transform.position, Quaternion.identity).GameObject;
		gameObject.transform.SetParent(magicProjectileLauncher.projectileSource, worldPositionStays: true);
		Vector2 up = Vector2.up;
		Vector2 zero = Vector2.zero;
		float num = 0f;
		int num2 = 6;
		up = Vector2.up;
		zero = Vector2.zero;
		float b = 170f;
		for (int i = 0; i < num2; i++)
		{
			num = Mathf.Lerp(0f, b, (float)i / (float)num2);
			Quaternion quaternion = Quaternion.Euler(0f, 0f, num);
			Quaternion quaternion2 = Quaternion.Euler(0f, 0f, 0f - num);
			zero = quaternion * up;
			magicProjectileLauncher.Shoot(zero.normalized);
			zero = quaternion2 * up;
			magicProjectileLauncher.Shoot(zero.normalized);
		}
	}

	public void OnMaskClosed()
	{
		PontiffGiant.IsGuarding = true;
		ashPlatformManager.heightLimitOn = false;
	}

	private IEnumerator BlockUntilAnimationEnds()
	{
		_waitingForAnimationFinish = true;
		while (_waitingForAnimationFinish)
		{
			yield return null;
		}
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

	[Button(ButtonSizes.Small)]
	public void Death()
	{
		StopAllAttacks();
		StopAllCoroutines();
		Core.Logic.Penitent.Status.Invulnerable = true;
		base.transform.DOKill(complete: true);
		StopBehaviour();
		GameplayUtils.DestroyAllProjectiles();
		_fsm.ChangeState(stDeath);
		sword.PontiffSword.Kill();
		ashPlatformManager.Deactivate();
		StartDeathSequence();
	}

	private void StopAllAttacks()
	{
		foreach (BossMachinegunShooter machineGun in machineGuns)
		{
			machineGun.StopMachinegun();
		}
	}

	private void StartDeathSequence()
	{
		StartCoroutine(DeathSequence());
	}

	private IEnumerator DeathSequence()
	{
		yield return new WaitForSeconds(1f);
		PontiffGiant.AnimatorInyector.Death();
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 2.2f, 0.3f, 1.8f);
		yield return new WaitForSeconds(1f);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.3f, new Vector2(1.3f, 0f), 16, 0.25f, 0f, default(Vector3), 0.04f, ignoreTimeScale: true);
		yield return new WaitForSeconds(2f);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.3f, new Vector2(1.3f, 0f), 16, 0.25f, 0f, default(Vector3), 0.04f, ignoreTimeScale: true);
		yield return new WaitForSeconds(1f);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(3f, new Vector2(1.3f, 1.5f), 120, 0.25f, 0f, default(Vector3), 0.04f, ignoreTimeScale: true);
		deathEffectAnim.SetTrigger("TRIGGER");
		Core.Logic.Penitent.Status.Invulnerable = false;
	}

	private IEnumerator RandomExplosions(float seconds, int totalExplosions, Transform center, float radius, GameObject poolableExplosion, Action OnExplosion = null, Action callback = null)
	{
		float counter = 0f;
		int expCounter = 0;
		while (counter < seconds)
		{
			counter += Time.deltaTime;
			if (counter > ((float)expCounter + 1f) / seconds)
			{
				expCounter++;
				Vector2 vector = center.position + new Vector3(UnityEngine.Random.Range(0f - radius, radius), UnityEngine.Random.Range(0f - radius, radius));
				PoolManager.Instance.ReuseObject(poolableExplosion, vector, Quaternion.identity);
				OnExplosion?.Invoke();
			}
			yield return null;
		}
		callback?.Invoke();
	}

	public override void Idle()
	{
		StopMovement();
	}

	public override void Wander()
	{
		throw new NotImplementedException();
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
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		PontiffGiant.SetOrientation((!(targetPos.x > PontiffGiant.transform.position.x)) ? EntityOrientation.Left : EntityOrientation.Right);
	}

	public override void StopMovement()
	{
	}

	public void SetGhostTrail(bool active)
	{
		PontiffGiant.GhostTrail.EnableGhostTrail = active;
	}

	private float GetDirFromOrientation()
	{
		return (Entity.Status.Orientation != 0) ? (-1f) : 1f;
	}

	private Vector2 ClampToFightBoundaries(Vector2 dir)
	{
		Vector2 result = dir;
		if (Physics2D.RaycastNonAlloc(base.transform.position, dir, results, dir.magnitude, fightBoundariesLayerMask) > 0)
		{
			result = result.normalized * results[0].distance;
			result *= 0.9f;
		}
		return result;
	}

	public void OnDrawGizmos()
	{
	}
}
