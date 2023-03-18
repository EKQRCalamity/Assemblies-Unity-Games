using System;
using System.Collections;
using System.Collections.Generic;
using BezierSplines;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public class EnemyActionTesterBehaviour : EnemyBehaviour
{
	public class JumpBackAndShoot_EnemyAction : EnemyAction
	{
		private int n;

		private float distance;

		private float seconds;

		private Action jumpMethod;

		private Action shootMethod;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private LaunchMethod_EnemyAction ACT_JUMP = new LaunchMethod_EnemyAction();

		private LaunchMethod_EnemyAction ACT_SHOOT = new LaunchMethod_EnemyAction();

		protected override void DoOnStop()
		{
			base.DoOnStop();
			ACT_WAIT = new WaitSeconds_EnemyAction();
			ACT_JUMP = new LaunchMethod_EnemyAction();
			ACT_SHOOT = new LaunchMethod_EnemyAction();
		}

		public EnemyAction StartAction(EnemyBehaviour e, int _n, Action _jumpMethod, Action _shootMethod)
		{
			n = _n;
			shootMethod = _shootMethod;
			jumpMethod = _jumpMethod;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();
			LaunchMethod_EnemyAction ACT_JUMP = new LaunchMethod_EnemyAction();
			LaunchMethod_EnemyAction ACT_SHOOT = new LaunchMethod_EnemyAction();
			ACT_JUMP.StartAction(owner, jumpMethod);
			for (int i = 0; i < n; i++)
			{
				ACT_WAIT.StartAction(owner, 0.2f, 0.2f);
				yield return ACT_WAIT.waitForCompletion;
				ACT_SHOOT.StartAction(owner, shootMethod);
			}
			ACT_WAIT.StartAction(owner, 0.6f, 0.6f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class MoveAroundAndAttack_EnemyAction : EnemyAction
	{
		private int n;

		private float distance;

		private float seconds;

		private Action method;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private LaunchMethod_EnemyAction ACT_METHOD = new LaunchMethod_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, int _n, float _distance, float _seconds, Action _method)
		{
			n = _n;
			distance = _distance;
			seconds = _seconds;
			method = _method;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_METHOD.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			ACT_MOVE.StartAction(owner, (Vector2)owner.transform.position + Vector2.up * 6f, 2f, Ease.OutCirc);
			yield return ACT_MOVE.waitForCompletion;
			for (int i = 0; i < n; i++)
			{
				ACT_MOVE.StartAction(owner, distance, seconds, Ease.OutCirc);
				yield return ACT_MOVE.waitForCompletion;
				ACT_WAIT.StartAction(owner, 0.5f, 0.5f);
				yield return ACT_WAIT.waitForCompletion;
				ACT_METHOD.StartAction(owner, method);
				yield return ACT_METHOD.waitForCompletion;
				ACT_WAIT.StartAction(owner, 0.2f, 0.2f);
				yield return ACT_WAIT.waitForCompletion;
				ACT_METHOD.StartAction(owner, method);
				yield return ACT_METHOD.waitForCompletion;
				ACT_WAIT.StartAction(owner, 0.2f, 0.2f);
				yield return ACT_WAIT.waitForCompletion;
				ACT_METHOD.StartAction(owner, method);
				yield return ACT_METHOD.waitForCompletion;
				ACT_WAIT.StartAction(owner, 0.5f, 1.5f);
				yield return ACT_WAIT.waitForCompletion;
			}
			FinishAction();
		}
	}

	public class SetBulletsAndActivate_EnemyAction : EnemyAction
	{
		private int n;

		private Action setBulletMethod;

		private Action activateMethod;

		private AnimationCurve timeSlowCurve;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private LaunchMethod_EnemyAction ACT_SETBULLET = new LaunchMethod_EnemyAction();

		private LaunchMethod_EnemyAction ACT_ACTIVATE = new LaunchMethod_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, int _n, Action _setBulletMethod, Action _activateMethod, AnimationCurve _timeSlowCurve)
		{
			n = _n;
			setBulletMethod = _setBulletMethod;
			activateMethod = _activateMethod;
			timeSlowCurve = _timeSlowCurve;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_ACTIVATE.StopAction();
			ACT_SETBULLET.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			Vector2 pointA = (Vector2)Core.Logic.Penitent.transform.position + Vector2.up * 5f + Vector2.left * 5f;
			Vector2 pointB = pointA + Vector2.right * 10f;
			Core.Logic.ScreenFreeze.Freeze(0.15f, 3f, 0f, timeSlowCurve);
			Vector2 target;
			for (int i = 0; i < n; i++)
			{
				target = Vector2.Lerp(pointA, pointB, (float)i / (float)n);
				target += Vector2.up * UnityEngine.Random.Range(-1, 1) * 1f;
				ACT_MOVE.StartAction(owner, target, 0.15f, Ease.InOutQuad, null, _timeScaled: false);
				yield return ACT_MOVE.waitForCompletion;
				ACT_SETBULLET.StartAction(owner, setBulletMethod);
				yield return ACT_SETBULLET.waitForCompletion;
			}
			target = Vector2.Lerp(pointA, pointB, 0.5f) + Vector2.up * 2f;
			ACT_MOVE.StartAction(owner, target, 0.5f, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			ACT_ACTIVATE.StartAction(owner, activateMethod);
			yield return ACT_ACTIVATE.waitForCompletion;
			FinishAction();
		}
	}

	private CountdownFromTen_EnemyAction countDown_EA;

	private WaitSeconds_EnemyAction randomWait_EA;

	private WaitSeconds_EnemyAction waitBetweenActions_EA;

	private LaunchMethod_EnemyAction launchMethod_EA;

	private MoveEasing_EnemyAction moveEasing_EA;

	private MoveAroundAndAttack_EnemyAction moveAttacking_EA;

	private JumpBackAndShoot_EnemyAction jumpBackNShoot_EA;

	private SetBulletsAndActivate_EnemyAction setBulletNActivate_EA;

	private BossInstantProjectileAttack instantProjectileAttack;

	private BossJumpAttack jumpAttack;

	private List<EnemyAction> availableAttacks;

	private EnemyAction currentAction;

	private BossStraightProjectileAttack bulletTimeProjectileAttack;

	private List<BulletTimeProjectile> bullets;

	public AnimationCurve timeSlowCurve;

	public BezierSpline horizontalSpline;

	public GameObject amanecidaAxePrefab;

	public List<AmanecidaAxeBehaviour> axes;

	private Dictionary<string, float> damageParameters;

	private const string AXE_DAMAGE = "AXE_DMG";

	private void Start()
	{
		countDown_EA = new CountdownFromTen_EnemyAction();
		randomWait_EA = new WaitSeconds_EnemyAction();
		waitBetweenActions_EA = new WaitSeconds_EnemyAction();
		launchMethod_EA = new LaunchMethod_EnemyAction();
		moveEasing_EA = new MoveEasing_EnemyAction();
		moveAttacking_EA = new MoveAroundAndAttack_EnemyAction();
		jumpBackNShoot_EA = new JumpBackAndShoot_EnemyAction();
		setBulletNActivate_EA = new SetBulletsAndActivate_EnemyAction();
		damageParameters = new Dictionary<string, float>();
		instantProjectileAttack = GetComponentInChildren<BossInstantProjectileAttack>();
		jumpAttack = GetComponentInChildren<BossJumpAttack>();
		bulletTimeProjectileAttack = GetComponentInChildren<BossStraightProjectileAttack>();
		bullets = new List<BulletTimeProjectile>();
		PoolManager.Instance.CreatePool(amanecidaAxePrefab, 2);
		availableAttacks = new List<EnemyAction> { jumpBackNShoot_EA, moveAttacking_EA };
		SpawnAxe(Vector2.right * 2f);
		SpawnAxe(Vector2.left * 2f);
	}

	private void SpawnAxe(Vector2 dir)
	{
		if (axes == null)
		{
			axes = new List<AmanecidaAxeBehaviour>();
		}
		AmanecidaAxeBehaviour component = PoolManager.Instance.ReuseObject(amanecidaAxePrefab, (Vector2)base.transform.position + Vector2.up * 1f + dir, Quaternion.identity).GameObject.GetComponent<AmanecidaAxeBehaviour>();
		PathFollowingProjectile component2 = component.GetComponent<PathFollowingProjectile>();
		Hit hit = default(Hit);
		hit.AttackingEntity = base.gameObject;
		hit.DamageAmount = GetDamageParameter("AXE_DMG");
		hit.DamageType = component2.damageType;
		hit.DamageElement = component2.damageElement;
		hit.Force = component2.force;
		hit.HitSoundId = component2.hitSound;
		hit.Unnavoidable = component2.unavoidable;
		Hit h = hit;
		component.InitDamageData(h);
		axes.Add(component);
	}

	public void SetDamageParameter(string key, float value)
	{
		damageParameters[key] = value;
	}

	private float GetDamageParameter(string key)
	{
		return damageParameters[key];
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			LaunchNewAction();
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			currentAction.StopAction();
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			setBulletNActivate_EA.StartAction(this, 8, DoProjectileAttack, ActivateBullets, timeSlowCurve);
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			moveEasing_EA.StopAction();
			Debug.Log("TESTING RANDOM MOVE");
			moveEasing_EA.StartAction(this, 5f, 1f, Ease.InOutCubic);
		}
		if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			moveEasing_EA.StopAction();
			moveAttacking_EA.StopAction();
			Debug.Log("TESTING ATTACK ROUTINE MOVE");
			moveAttacking_EA.StartAction(this, 5, 3f, 1f, DoDummyAttack);
		}
		if (Input.GetKeyDown(KeyCode.Alpha7))
		{
			jumpBackNShoot_EA.StopAction();
			Debug.Log("TESTING JUMPBACK AND SHOOT");
			jumpBackNShoot_EA.StartAction(this, 3, DoDummyBackJump, DoDummyAttack);
		}
	}

	private void LaunchActionWithParameters(EnemyAction act)
	{
		if (act == jumpBackNShoot_EA)
		{
			jumpBackNShoot_EA.StartAction(this, 3, DoDummyBackJump, DoDummyAttack);
		}
		else if (act == moveAttacking_EA)
		{
			moveAttacking_EA.StartAction(this, 3, 3f, 1f, DoDummyAttack);
		}
	}

	private void LaunchNewAction()
	{
		if (currentAction != null)
		{
			currentAction.StopAction();
		}
		currentAction = availableAttacks[UnityEngine.Random.Range(0, availableAttacks.Count)];
		currentAction.OnActionEnds += CurrentAction_OnActionEnds;
		currentAction.OnActionIsStopped += CurrentAction_OnActionStops;
		LaunchActionWithParameters(currentAction);
	}

	private void WaitBetweenActions()
	{
		waitBetweenActions_EA.StartAction(this, 1f, 3f);
		waitBetweenActions_EA.OnActionEnds += CurrentAction_OnActionEnds;
	}

	private void CurrentAction_OnActionEnds(EnemyAction e)
	{
		e.OnActionEnds -= CurrentAction_OnActionEnds;
		if (e == waitBetweenActions_EA)
		{
			WaitBetweenActions();
		}
		else
		{
			LaunchNewAction();
		}
	}

	private void CurrentAction_OnActionStops(EnemyAction e)
	{
		e.OnActionIsStopped -= CurrentAction_OnActionStops;
	}

	private void Foo_PlayAnimation()
	{
		Debug.Log("PLAYING ANIMATION THROUGH THE ANIMATOR INYECTOR");
	}

	public void DoDummyAttack()
	{
		Debug.Log("<DUMMY ATTACK>");
		instantProjectileAttack.Shoot(base.transform.position, Core.Logic.Penitent.transform.position - base.transform.position);
	}

	public void DoDummyBackJump()
	{
		Debug.Log("<DUMMY ATTACK>");
		Vector2 vector = -base.transform.right * 4f;
		Vector2 vector2 = (Vector2)base.transform.position + vector;
		jumpAttack.Use(base.transform, vector2);
	}

	public void DoProjectileAttack()
	{
		Debug.Log("<PROJECTILE ATTACK>");
		Vector2 dirToPenitent = GetDirToPenitent(base.transform.position);
		StraightProjectile straightProjectile = bulletTimeProjectileAttack.Shoot(dirToPenitent);
		bullets.Add(straightProjectile as BulletTimeProjectile);
	}

	public void ActivateBullets()
	{
		Debug.Log("<ACTIVATE BULLETS>");
		if (bullets == null || bullets.Count == 0)
		{
			return;
		}
		foreach (BulletTimeProjectile bullet in bullets)
		{
			bullet.Accelerate();
		}
		bullets.Clear();
	}

	private Vector2 GetDirToPenitent(Vector2 point)
	{
		return (Vector2)Core.Logic.Penitent.transform.position - point;
	}

	public override void Attack()
	{
	}

	public override void Chase(Transform targetPosition)
	{
	}

	public override void Damage()
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
}
