using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.Generic.Attacks;
using Gameplay.GameControllers.Bosses.HighWills.Attack;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.HighWills;

[RequireComponent(typeof(HighWills))]
public class HighWillsBehaviour : EnemyBehaviour
{
	public enum HW_ATTACKS
	{
		MINES_1,
		BLAST
	}

	[Serializable]
	public struct HWAttacksWithWeight
	{
		public List<HW_ATTACKS> Attacks;

		public float Weight;
	}

	public class HighWillsSt_Inactive : State<HighWillsBehaviour>
	{
		public override void Enter(HighWillsBehaviour owner)
		{
		}

		public override void Execute(HighWillsBehaviour owner)
		{
			owner.UpdateInactiveState();
		}

		public override void Exit(HighWillsBehaviour owner)
		{
		}
	}

	public class HighWillsSt_Wait : State<HighWillsBehaviour>
	{
		public override void Enter(HighWillsBehaviour owner)
		{
		}

		public override void Execute(HighWillsBehaviour owner)
		{
			owner.UpdateWaitState();
		}

		public override void Exit(HighWillsBehaviour owner)
		{
		}
	}

	public class HighWillsSt_Action : State<HighWillsBehaviour>
	{
		public override void Enter(HighWillsBehaviour owner)
		{
		}

		public override void Execute(HighWillsBehaviour owner)
		{
			owner.UpdateActionState();
		}

		public override void Exit(HighWillsBehaviour owner)
		{
		}
	}

	public class HighWillsIntro_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			HighWillsBehaviour o = owner as HighWillsBehaviour;
			o.HighWills.Stats.Life.SetToCurrentMax();
			ACT_MOVE.StartAction(o, o.transform.position + new Vector3(-10f, 0f, 0f), 2f, Ease.OutQuad);
			yield return ACT_MOVE.waitForCompletion;
			ACT_MOVE.StartAction(o, o.transform.position + new Vector3(5f, 0f, 0f), 4f, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			FinishAction();
		}
	}

	public class HighWillsMineAttack_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			HighWillsBehaviour o = owner as HighWillsBehaviour;
			o.HighWills.ActivateMiddleHWEyes(0.2f, 2f, 0.2f);
			ACT_WAIT.StartAction(owner, 0.2f);
			yield return ACT_WAIT.waitForCompletion;
			o.DoMineAttack();
			ACT_WAIT.StartAction(owner, 0.2f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class HighWillsBlastAttack_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			HighWillsBehaviour o = owner as HighWillsBehaviour;
			o.HighWills.ActivateLeftHWEyes(0.2f, 1f, 0.2f);
			ACT_WAIT.StartAction(owner, 0.2f);
			yield return ACT_WAIT.waitForCompletion;
			o.DoBlastAttack();
			ACT_WAIT.StartAction(owner, 0.2f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class HighWillsDeath_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			HighWillsBehaviour o = owner as HighWillsBehaviour;
			o.ScrollManager.Stop();
			Core.Logic.Penitent.Physics.EnablePhysics(enable: false);
			ACT_WAIT.StartAction(owner, 0.1f);
			yield return ACT_WAIT.waitForCompletion;
			PlayMakerFSM.BroadcastEvent("BOSS DEAD");
			FinishAction();
		}
	}

	public HighWills HighWills;

	public HighWillsBossScrollManager ScrollManager;

	public BossMachinegunShooter fireMachineGun;

	public GameObject MineShooter1;

	public Transform MineShootingPoint1;

	public BossAreaSummonAttack BlastAttack;

	public Transform BlastShootingPointForHeight;

	public List<HWAttacksWithWeight> AttacksAndWeights = new List<HWAttacksWithWeight>();

	private StateMachine<HighWillsBehaviour> _fsm;

	private State<HighWillsBehaviour> stWaiting;

	private State<HighWillsBehaviour> stAction;

	private State<HighWillsBehaviour> stInactive;

	private EnemyAction currentAction;

	private EnemyAction mineAttackAction;

	private EnemyAction blastAttackAction;

	private EnemyAction introAction;

	private EnemyAction deathAction;

	private List<HW_ATTACKS> chosenAttacks = new List<HW_ATTACKS>();

	private float attackCD = 4f;

	private float attackTimer;

	public override void OnAwake()
	{
		base.OnAwake();
		HighWills = GetComponent<HighWills>();
		fireMachineGun = GetComponentInChildren<BossMachinegunShooter>();
		mineAttackAction = new HighWillsMineAttack_EnemyAction();
		blastAttackAction = new HighWillsBlastAttack_EnemyAction();
		deathAction = new HighWillsDeath_EnemyAction();
		introAction = new HighWillsIntro_EnemyAction();
		stInactive = new HighWillsSt_Inactive();
		stWaiting = new HighWillsSt_Wait();
		stAction = new HighWillsSt_Action();
		_fsm = new StateMachine<HighWillsBehaviour>(this, stInactive);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		_fsm.DoUpdate();
	}

	protected void UpdateActionState()
	{
	}

	protected void UpdateWaitState()
	{
		UpdateTimers();
		if ((bool)Core.Logic.Penitent && HighWills.VisionCone.CanSeeTarget(Core.Logic.Penitent.transform, "Penitent") && CanAttack())
		{
			ResetAttackTimer();
			ChooseAttacksAndLaunch();
		}
	}

	protected void ChooseAttacksAndLaunch()
	{
		if (chosenAttacks.Count == 0)
		{
			GetNewChosenAttacks();
		}
		switch (chosenAttacks[0])
		{
		case HW_ATTACKS.MINES_1:
			LaunchMineAttackAction();
			break;
		case HW_ATTACKS.BLAST:
			LaunchBlastAttackAction();
			break;
		}
		chosenAttacks.RemoveAt(0);
	}

	private void GetNewChosenAttacks()
	{
		float weightsSum = 0f;
		AttacksAndWeights.ForEach(delegate(HWAttacksWithWeight x)
		{
			weightsSum += x.Weight;
		});
		float num = UnityEngine.Random.Range(0f, weightsSum);
		weightsSum = 0f;
		for (int i = 0; i < AttacksAndWeights.Count; i++)
		{
			chosenAttacks = new List<HW_ATTACKS>(AttacksAndWeights[i].Attacks);
			weightsSum += AttacksAndWeights[i].Weight;
			if (weightsSum > num)
			{
				break;
			}
		}
	}

	protected void UpdateInactiveState()
	{
	}

	private bool CanAttack()
	{
		return attackTimer <= 0f;
	}

	private void ResetAttackTimer()
	{
		attackTimer = attackCD;
	}

	private void UpdateTimers()
	{
		if (attackTimer > 0f)
		{
			attackTimer -= Time.deltaTime;
		}
	}

	private void DoProjectileAttack()
	{
		fireMachineGun.StartAttack(Core.Logic.Penitent.transform);
	}

	private void DoMineAttack()
	{
		PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(MineShooter1, MineShootingPoint1.position, Quaternion.identity, createPoolIfNeeded: true, 3);
		RangedMineShooter component = objectInstance.GameObject.GetComponent<RangedMineShooter>();
		component.StartShootingMines(MineShootingPoint1.position);
	}

	private void DoBlastAttack()
	{
		Vector3 position = Core.Logic.Penitent.GetPosition();
		position.y = BlastShootingPointForHeight.position.y;
		BlastAttack.SummonAreaOnPoint(position);
	}

	protected void LaunchMineAttackAction()
	{
		LaunchAttackAction(mineAttackAction);
	}

	protected void LaunchBlastAttackAction()
	{
		LaunchAttackAction(blastAttackAction);
	}

	protected void LaunchAttackAction(EnemyAction attack)
	{
		StopCurrentAction();
		_fsm.ChangeState(stAction);
		currentAction = attack.StartAction(this);
		SuscribeToActionEvents();
	}

	public void LaunchIntroAction()
	{
		StopCurrentAction();
		_fsm.ChangeState(stAction);
		currentAction = introAction.StartAction(this);
		SuscribeToActionEvents();
		ClearMines();
	}

	private void ClearMines()
	{
		RangedMine[] array = UnityEngine.Object.FindObjectsOfType<RangedMine>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
	}

	public void LaunchDeathAction()
	{
		StopCurrentAction();
		_fsm.ChangeState(stInactive);
		currentAction = deathAction.StartAction(this);
		currentAction.OnActionEnds -= CurrentAction_OnActionEnds;
		currentAction.OnActionIsStopped -= CurrentAction_OnActionStops;
	}

	protected void SuscribeToActionEvents()
	{
		currentAction.OnActionEnds -= CurrentAction_OnActionEnds;
		currentAction.OnActionIsStopped -= CurrentAction_OnActionStops;
		currentAction.OnActionEnds += CurrentAction_OnActionEnds;
		currentAction.OnActionIsStopped += CurrentAction_OnActionStops;
	}

	private void CurrentAction_OnActionStops(EnemyAction e)
	{
		_fsm.ChangeState(stWaiting);
	}

	private void CurrentAction_OnActionEnds(EnemyAction e)
	{
		_fsm.ChangeState(stWaiting);
	}

	private void StopCurrentAction()
	{
		if (currentAction != null)
		{
			currentAction.StopAction();
		}
	}

	public override void Attack()
	{
	}

	public override void Chase(Transform targetPosition)
	{
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

	public override void Damage()
	{
	}
}
