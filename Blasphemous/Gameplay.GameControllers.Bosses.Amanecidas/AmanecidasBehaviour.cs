using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BezierSplines;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using Maikel.StatelessFSM;
using Maikel.SteeringBehaviors;
using Plugins.Maikel.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Amanecidas;

public class AmanecidasBehaviour : EnemyBehaviour
{
	[Serializable]
	public struct AmanecidasFightParameters
	{
		public AmanecidasAnimatorInyector.AMANECIDA_WEAPON weapon;

		[ProgressBar(0.0, 1.0, 0.8f, 0f, 0.1f)]
		[SuffixLabel("%", false)]
		public float hpPercentageBeforeApply;

		[MinMaxSlider(0f, 5f, true)]
		public Vector2 minMaxWaitingTimeBetweenActions;

		[SuffixLabel("hits", true)]
		public int maxHitsInHurt;

		[SuffixLabel("actions", true)]
		public int maxActionsBeforeShieldRecharge;

		[SuffixLabel("seconds", true)]
		public int shieldRechargeTime;

		[SuffixLabel("seconds", true)]
		public int shieldShockwaveAnticipationTime;

		[SuffixLabel("damage", true)]
		public int maxDamageBeforeInterruptingRecharge;
	}

	public enum AMANECIDA_ATTACKS
	{
		AXE_FlyAndSpin = 0,
		AXE_DualThrow = 1,
		AXE_JumpSmash = 2,
		AXE_JumpSmashWithPillars = 39,
		AXE_DualThrowCross = 20,
		AXE_FlyAndToss = 21,
		AXE_MeleeAttack = 25,
		AXE_FlameBlade = 35,
		AXE_FollowAndAxeToss = 36,
		AXE_FollowAndCrawlerAxeToss = 43,
		AXE_FollowAndLavaBall = 37,
		AXE_COMBO_StompNLavaBalls = 101,
		BOW_RicochetShot = 3,
		BOW_FreezeTimeBlinkShots = 4,
		BOW_MineShots = 5,
		BOW_FreezeTimeMultiShots = 19,
		BOW_FastShot = 26,
		BOW_FastShots = 27,
		BOW_ChargedShot = 28,
		BOW_SpikesBlinkShot = 29,
		BOW_COMBO_FreezeTimeNRicochetShots = 102,
		BOW_LaserShot = 41,
		LANCE_JumpBackAndDash = 6,
		LANCE_BlinkDash = 7,
		LANCE_FreezeTimeLances = 8,
		LANCE_FreezeTimeLancesOnPenitent = 42,
		LANCE_HorizontalBlinkDashes = 13,
		LANCE_DiagonalBlinkDashes = 14,
		LANCE_MultiFrontalDash = 33,
		LANCE_FreezeTimeHail = 34,
		LANCE_COMBO_FreezeTimeNHorizontalDashes = 103,
		FALCATA_BlinkDash = 9,
		FALCATA_MeleeAttack = 15,
		FALCATA_SpinAttack = 16,
		FALCATA_SlashProjectile = 17,
		FALCATA_SlashBarrage = 18,
		FALCATA_QuickLunge = 24,
		FALCATA_ChainDash = 32,
		FALCATA_COMBO_STORM = 104,
		FALCATA_CounterAttack = 31,
		FALCATA_NoxiousBlade = 38,
		COMMON_BlinkAway = 10,
		COMMON_RechargeShield = 11,
		COMMON_Intro = 12,
		COMMON_StompAttack = 22,
		COMMON_ChangeWeapon = 23,
		COMMON_Death = 30,
		COMMON_MoveBattleBounds = 40
	}

	public enum AMANECIDA_GRUNTS
	{
		MULTI_FRONTAL_GRUNT,
		GRUNT2
	}

	public class HurtDisplacement_EnemyAction : EnemyAction
	{
		private float displacementAmount;

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_CHARACTER = new MoveToPointUsingAgent_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, float amount)
		{
			displacementAmount = amount;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_MOVE_CHARACTER.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour amanecida = owner as AmanecidasBehaviour;
			Vector2 dir = (owner.transform.position - Core.Logic.Penitent.transform.position).normalized;
			Vector2 pos = amanecida.GetValidPointInDirection(dir, displacementAmount);
			ACT_MOVE_CHARACTER.StartAction(owner, amanecida.agent, pos);
			yield return ACT_MOVE_CHARACTER.waitForCompletion;
			FinishAction();
		}
	}

	public class Hurt_EnemyAction : EnemyAction
	{
		private bool isLastHurt;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private HurtDisplacement_EnemyAction ACT_DISP = new HurtDisplacement_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, bool isLastHurt)
		{
			this.isLastHurt = isLastHurt;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_DISP.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			if (isLastHurt)
			{
				amanecidasBehaviour.SetInterruptable(state: false);
			}
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			ama.Amanecidas.AnimatorInyector.PlayHurt();
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: false);
			ACT_DISP.StartAction(owner, 0.5f);
			yield return ACT_DISP.waitForCompletion;
			if (isLastHurt)
			{
				ama.SetInterruptable(state: false);
			}
			float hurtRecoveryTime = 0.5f;
			ACT_WAIT.StartAction(owner, hurtRecoveryTime);
			yield return ACT_WAIT.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			FinishAction();
		}
	}

	public class Death_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		protected override void DoOnStop()
		{
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			ama.GetComponentInChildren<EnemyDamageArea>().DamageAreaCollider.enabled = false;
			ama.Amanecidas.AnimatorInyector.PlayDeath();
			ACT_WAIT.StartAction(owner, 1.2f);
			yield return ACT_WAIT.waitForCompletion;
			Core.Logic.Penitent.Status.Invulnerable = false;
			FinishAction();
			UnityEngine.Object.Destroy(ama.gameObject);
		}
	}

	public class ShieldShockwave : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		protected override void DoOnStop()
		{
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			Vector2 p = (Vector2)ama.transform.position + ama.centerBodyOffset;
			if (ama.HasSolidFloorBelow())
			{
				ama.SpikeWave(ama.transform.position);
			}
			ama.ShakeWave();
			PoolManager.Instance.ReuseObject(ama.shieldShockwave, p, Quaternion.identity);
			ama.Amanecidas.Audio.PlayShieldExplosion_AUDIO();
			ACT_WAIT.StartAction(owner, 0.15f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class MoveUsingSpline_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private LookAtPenitent_EnemyAction ACT_LOOK = new LookAtPenitent_EnemyAction();

		private SplineThrowData throwData;

		private Transform oldParent;

		private Vector2 endPoint;

		public EnemyAction StartAction(EnemyBehaviour owner, Vector2 point, SplineThrowData throwData)
		{
			this.throwData = throwData;
			endPoint = point;
			return StartAction(owner);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			throwData.spline.transform.SetParent(oldParent);
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.splineFollower.followActivated = false;
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			amanecidasBehaviour.DoSpinAnimation(spin: false);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			int right = ama.GetDirFromOrientation();
			BezierSpline spline = throwData.spline;
			oldParent = spline.transform.parent;
			spline.transform.localScale = new Vector3(spline.transform.localScale.x * (float)right, Mathf.Sign(UnityEngine.Random.Range(-1f, 1f)), 1f);
			spline.transform.SetParent(null, worldPositionStays: true);
			Vector2 controlPointOrigin = spline.points[1] - spline.points[0];
			Vector2 controlPointEnd = spline.points[2] - spline.points[3];
			Vector2 origin = spline.transform.InverseTransformPoint(ama.transform.position);
			endPoint = spline.transform.InverseTransformPoint(endPoint);
			spline.SetControlPoint(0, origin);
			spline.SetControlPoint(3, endPoint);
			spline.SetControlPoint(1, (Vector2)spline.points[0] + controlPointOrigin);
			spline.SetControlPoint(2, (Vector2)spline.points[3] + controlPointEnd);
			ama.DoSpinAnimation(spin: true);
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: false);
			ama.splineFollower.SetData(throwData);
			ama.splineFollower.StartFollowing(loop: false);
			ama.SmallDistortion();
			ACT_WAIT.StartAction(owner, throwData.duration);
			yield return ACT_WAIT.waitForCompletion;
			ama.DoSpinAnimation(spin: false);
			yield return new WaitUntilIdle(ama);
			ACT_LOOK.StartAction(owner);
			yield return ACT_LOOK.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			ama.SmallDistortion();
			ama.splineFollower.followActivated = false;
			spline.transform.SetParent(oldParent, worldPositionStays: true);
			FinishAction();
		}
	}

	public class DodgeAndCounterAttack_EnemyAction : EnemyAction
	{
		private float evadeDistance;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE = new MoveToPointUsingAgent_EnemyAction();

		private FalcataSlashProjectile_EnemyAction ACT_COUNTERACTION = new FalcataSlashProjectile_EnemyAction();

		private MoveUsingSpline_EnemyAction ACT_DODGE = new MoveUsingSpline_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour owner, float evadeDistance)
		{
			this.evadeDistance = evadeDistance;
			return StartAction(owner);
		}

		protected override void DoOnStop()
		{
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			int lookingRight = ama.GetDirFromOrientation();
			Vector2 dirToDodge = new Vector2(0f - Mathf.Sign(ama.GetDirToPenitent(ama.transform.position).x), 0f);
			Vector2 dodgePoint = (Vector2)owner.transform.position + dirToDodge * evadeDistance;
			if (!ama.battleBounds.Contains(dodgePoint))
			{
				dodgePoint = (Vector2)owner.transform.position - dirToDodge * evadeDistance;
			}
			ama.SpawnClone();
			ama.Amanecidas.Audio.PlayMoveFast_AUDIO();
			ACT_DODGE.StartAction(owner, dodgePoint, ama.dodgeSplineData);
			yield return ACT_DODGE.waitForCompletion;
			ACT_WAIT.StartAction(owner, 0.1f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class ShowAxes_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		protected override void DoOnStop()
		{
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour amanecida = owner as AmanecidasBehaviour;
			amanecida.ShowBothAxes(v: true);
			amanecida.Amanecidas.AnimatorInyector.SetAmanecidaWeapon(AmanecidasAnimatorInyector.AMANECIDA_WEAPON.HAND);
			ACT_WAIT.StartAction(owner, 0.1f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class FuseAxes_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		protected override void DoOnStop()
		{
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour amanecida = owner as AmanecidasBehaviour;
			amanecida.ShowBothAxes(v: false);
			amanecida.Amanecidas.AnimatorInyector.SetAmanecidaWeapon(AmanecidasAnimatorInyector.AMANECIDA_WEAPON.AXE);
			ACT_WAIT.StartAction(owner, 0.1f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class FollowSplineAndSpinAxesAround_EnemyAction : EnemyAction
	{
		private int n;

		private List<AmanecidaAxeBehaviour> axes;

		private List<BezierSpline> axeSplines;

		private List<SplineThrowData> throwData;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_CHARACTER = new MoveEasing_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_AXE1 = new MoveEasing_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_AXE2 = new MoveEasing_EnemyAction();

		private SpinAxesAround_EnemyAction ACT_SPIN = new SpinAxesAround_EnemyAction();

		private ShowSimpleVFX_EnemyAction ACT_MAGNET_VFX = new ShowSimpleVFX_EnemyAction();

		private SplineFollower splineFollower;

		private SplineThrowData mainSplineData;

		protected override void DoOnStop()
		{
			base.DoOnStop();
			ACT_WAIT.StopAction();
			ACT_MOVE_AXE1.StopAction();
			ACT_MOVE_AXE2.StopAction();
			ACT_BLINK.StopAction();
			ACT_SPIN.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			Amanecidas amanecidas = amanecidasBehaviour.Amanecidas;
			amanecidas.Behaviour.SetInterruptable(state: false);
			splineFollower.followActivated = false;
		}

		public EnemyAction StartAction(EnemyBehaviour e, int _n, SplineThrowData mainSplineData, List<AmanecidaAxeBehaviour> axes, List<BezierSpline> axeSplines, List<SplineThrowData> throwData)
		{
			n = _n;
			this.mainSplineData = mainSplineData;
			this.axes = axes;
			this.axeSplines = axeSplines;
			this.throwData = throwData;
			splineFollower = e.GetComponent<SplineFollower>();
			splineFollower.SetData(mainSplineData);
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			Amanecidas amanecida = o.Amanecidas;
			ACT_BLINK.StartAction(owner, mainSplineData.spline.GetPoint(0f), 1f, reappear: true, lookAtPenitent: true);
			yield return ACT_BLINK.waitForCompletion;
			if (o.IsWieldingAxe())
			{
				o.ShowBothAxes(v: true);
				o.ShowCurrentWeapon(show: false);
			}
			float secondsToArrive = 1.5f;
			axes[0].SetRepositionMode(isInReposition: true);
			axes[1].SetRepositionMode(isInReposition: true);
			axes[0].SetSeek(free: false);
			axes[1].SetSeek(free: false);
			ACT_MOVE_AXE1.StartAction(axes[0], axeSplines[0].GetPoint(0f), secondsToArrive, Ease.InOutQuad);
			ACT_MOVE_AXE2.StartAction(axes[1], axeSplines[1].GetPoint(0f), secondsToArrive, Ease.InOutQuad);
			yield return ACT_MOVE_AXE1.waitForCompletion;
			yield return ACT_MOVE_AXE2.waitForCompletion;
			axes[0].SetRepositionMode(isInReposition: false);
			axes[1].SetRepositionMode(isInReposition: false);
			splineFollower.StartFollowing(loop: true);
			ACT_SPIN.StartAction(owner, n, axes, axeSplines, throwData);
			int maxPossibleTurns = Mathf.FloorToInt(mainSplineData.duration / 0.7f);
			for (int i = 0; i < maxPossibleTurns; i++)
			{
				o.LookAtPenitent();
				ACT_WAIT.StartAction(owner, 0.7f);
				yield return ACT_WAIT.waitForCompletion;
				if (i == maxPossibleTurns - 1)
				{
					amanecida.Behaviour.SetInterruptable(state: true);
				}
			}
			yield return ACT_SPIN.waitForCompletion;
			amanecida.Behaviour.SetInterruptable(state: false);
			splineFollower.followActivated = false;
			FinishAction();
		}
	}

	public class InterruptablePeriod_EnemyAction : EnemyAction
	{
		private float seconds;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour owner, float seconds)
		{
			this.seconds = seconds;
			return StartAction(owner);
		}

		protected override void DoOnStop()
		{
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.SetInterruptable(state: false);
			ACT_WAIT.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			ama.SetInterruptable(state: true);
			ACT_WAIT.StartAction(owner, seconds);
			yield return ACT_WAIT.waitForCompletion;
			ama.SetInterruptable(state: false);
			FinishAction();
		}
	}

	public class EnergyChargePeriod_EnemyAction : EnemyAction
	{
		private float seconds;

		private bool playSfx;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour owner, float seconds, bool playSfx)
		{
			this.seconds = seconds;
			this.playSfx = playSfx;
			return StartAction(owner);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetEnergyCharge(active: false);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			if (playSfx)
			{
				ama.Amanecidas.Audio.PlayEnergyCharge_AUDIO();
			}
			ama.Amanecidas.AnimatorInyector.SetEnergyCharge(active: true);
			ACT_WAIT.StartAction(owner, seconds);
			yield return ACT_WAIT.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.SetEnergyCharge(active: false);
			FinishAction();
		}
	}

	public class TiredPeriod_EnemyAction : EnemyAction
	{
		private float seconds;

		private bool interruptable;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private InterruptablePeriod_EnemyAction ACT_INTERRUPTABLE = new InterruptablePeriod_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour owner, float seconds, bool interruptable)
		{
			this.seconds = seconds;
			this.interruptable = interruptable;
			return StartAction(owner);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_INTERRUPTABLE.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetTired(active: false);
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			amanecidasBehaviour.LookAtPenitent(instant: true);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			ama.Amanecidas.AnimatorInyector.SetTired(active: true);
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: false);
			if (interruptable)
			{
				ACT_INTERRUPTABLE.StartAction(owner, seconds);
				yield return ACT_INTERRUPTABLE.waitForCompletion;
			}
			else
			{
				ACT_WAIT.StartAction(owner, seconds);
				yield return ACT_WAIT.waitForCompletion;
			}
			ama.Amanecidas.AnimatorInyector.SetTired(active: false);
			yield return new WaitUntilIdle(ama);
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			FinishAction();
		}
	}

	public class ShowSimpleVFX_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private Vector2 targetPoint;

		private float seconds;

		private GameObject vfxGO;

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 _point, GameObject _vfxGO, float _seconds)
		{
			targetPoint = _point;
			seconds = _seconds;
			vfxGO = _vfxGO;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			GameObject newFX = PoolManager.Instance.ReuseObject(vfxGO, targetPoint, Quaternion.identity).GameObject;
			SimpleVFX svfx = newFX.GetComponent<SimpleVFX>();
			if (svfx != null)
			{
				svfx.SetMaxTTL(seconds);
			}
			ACT_WAIT.StartAction(owner, seconds);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class BlinkToPoint_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private Vector2 targetPoint;

		private float seconds;

		private bool reappear;

		private bool lookAtPenitent;

		private float baseBlinkoutSeconds = 0.55f;

		private float baseBlinkinSeconds = 0.4f;

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 _point, float _seconds, bool reappear = true, bool lookAtPenitent = false)
		{
			targetPoint = _point;
			seconds = _seconds;
			this.reappear = reappear;
			this.lookAtPenitent = lookAtPenitent;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			base.DoOnStop();
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			Amanecidas amanecidas = (owner as AmanecidasBehaviour).Amanecidas;
			if (reappear)
			{
				amanecidas.Behaviour.SetGhostTrail(active: true);
				amanecidas.AnimatorInyector.SetBlink(value: false);
			}
			if (lookAtPenitent)
			{
				amanecidas.Behaviour.LookAtPenitentUsingOrientation();
			}
		}

		protected override IEnumerator BaseCoroutine()
		{
			Amanecidas amanecida = (owner as AmanecidasBehaviour).Amanecidas;
			amanecida.AnimatorInyector.SetBlink(value: true);
			amanecida.Behaviour.SetGhostTrail(active: false);
			ACT_WAIT.StartAction(owner, baseBlinkoutSeconds + seconds);
			yield return ACT_WAIT.waitForCompletion;
			ACT_MOVE.StartAction(owner, targetPoint, 0.1f, Ease.InCubic);
			yield return ACT_MOVE.waitForCompletion;
			if (lookAtPenitent)
			{
				amanecida.Behaviour.LookAtPenitentUsingOrientation();
			}
			ACT_WAIT.StartAction(owner, 0.01f);
			yield return ACT_WAIT.waitForCompletion;
			if (reappear)
			{
				amanecida.Behaviour.SetGhostTrail(active: true);
				amanecida.AnimatorInyector.SetBlink(value: false);
				ACT_WAIT.StartAction(owner, baseBlinkinSeconds);
				yield return ACT_WAIT.waitForCompletion;
			}
			FinishAction();
		}
	}

	public class SpinAxesAround_EnemyAction : EnemyAction
	{
		private int n;

		private List<AmanecidaAxeBehaviour> axes;

		private List<BezierSpline> splines;

		private List<SplineThrowData> throwData;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_CHARACTER = new MoveEasing_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_AXE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			base.DoOnStop();
			ACT_WAIT.StopAction();
			ACT_MOVE_AXE.StopAction();
			foreach (AmanecidaAxeBehaviour axis in axes)
			{
				axis.axeSplineFollowAction.StopAction();
			}
		}

		public EnemyAction StartAction(EnemyBehaviour e, int _n, List<AmanecidaAxeBehaviour> axes, List<BezierSpline> splines, List<SplineThrowData> throwData)
		{
			n = _n;
			this.axes = axes;
			this.splines = splines;
			this.throwData = throwData;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			Transform p = splines[0].transform.parent;
			float maxradius = 3f;
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < axes.Count; j++)
				{
					AmanecidaAxeBehaviour amanecidaAxeBehaviour = axes[j];
					amanecidaAxeBehaviour.axeSplineFollowAction.StartAction(amanecidaAxeBehaviour, amanecidaAxeBehaviour.splineFollower, throwData[j], splines[j]);
				}
				Tween tw2 = p.DOScale(maxradius, throwData[0].duration / 2f).SetEase(Ease.InOutQuad);
				yield return tw2.WaitForCompletion();
				tw2 = p.DOScale(1f, throwData[0].duration / 2f).SetEase(Ease.InOutQuad);
				yield return tw2.WaitForCompletion();
			}
			FinishAction();
		}
	}

	public class FlyAndLaunchTwoAxes_EnemyAction : EnemyAction
	{
		private int n;

		private Vector2 dir;

		private List<AmanecidaAxeBehaviour> axes;

		private List<BezierSpline> splines;

		private List<SplineThrowData> throwData;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE = new MoveToPointUsingAgent_EnemyAction();

		private ShowAxes_EnemyAction ACT_SHOW_AXES = new ShowAxes_EnemyAction();

		private ShowSimpleVFX_EnemyAction ACT_MAGNET_VFX = new ShowSimpleVFX_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		protected override void DoOnStop()
		{
			base.DoOnStop();
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			foreach (AmanecidaAxeBehaviour axis in axes)
			{
				axis.axeSplineFollowAction.StopAction();
			}
			(owner as AmanecidasBehaviour).splineFollower.followActivated = false;
		}

		public EnemyAction StartAction(EnemyBehaviour e, int _n, Vector2 dir, List<AmanecidaAxeBehaviour> axes, List<BezierSpline> splines, List<SplineThrowData> throwData)
		{
			n = _n;
			this.axes = axes;
			this.dir = dir;
			this.splines = splines;
			this.throwData = throwData;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			ACT_MOVE.StartAction(owner, ama.agent, ama.battleBounds.center + Vector2.up * 3f);
			yield return ACT_MOVE.waitForCompletion;
			ACT_SHOW_AXES.StartAction(owner);
			yield return ACT_SHOW_AXES.waitForCompletion;
			ACT_WAIT.StartAction(owner, 1f);
			yield return ACT_WAIT.waitForCompletion;
			SplineThrowData sp = ama.flightPattern;
			ACT_BLINK.StartAction(owner, sp.spline.points[0], 0.3f, reappear: true, lookAtPenitent: true);
			yield return ACT_BLINK.waitForCompletion;
			ama.splineFollower.SetData(ama.flightPattern);
			ama.splineFollower.StartFollowing(loop: true);
			for (int i = 0; i < n; i++)
			{
				AmanecidaAxeBehaviour axe2 = axes[0];
				Vector2 originPoint2 = axe2.transform.position;
				Vector2 targetPoint2 = Core.Logic.Penitent.transform.position;
				axe2.axeSplineFollowAction.StartAction(axe2, axe2.splineFollower, originPoint2, targetPoint2, 3, throwData[0], splines[0]);
				ACT_WAIT.StartAction(owner, throwData[0].duration / 2f);
				yield return ACT_WAIT.waitForCompletion;
				axe2 = axes[1];
				originPoint2 = axe2.transform.position;
				targetPoint2 = Core.Logic.Penitent.transform.position;
				axe2.axeSplineFollowAction.StartAction(axe2, axe2.splineFollower, originPoint2, targetPoint2, 3, throwData[1], splines[1]);
				ACT_WAIT.StartAction(owner, throwData[1].duration / 2f);
				yield return ACT_WAIT.waitForCompletion;
			}
			ama.splineFollower.followActivated = false;
			ACT_WAIT.StartAction(owner, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class LaunchTwoAxesHorizontal_EnemyAction : EnemyAction
	{
		private int n;

		private int dir;

		private float distance;

		private Vector2 point;

		private List<AmanecidaAxeBehaviour> axes;

		private List<BezierSpline> splines;

		private List<SplineThrowData> throwData;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_EASING_1 = new MoveEasing_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_EASING_2 = new MoveEasing_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_AGENT = new MoveToPointUsingAgent_EnemyAction();

		private ShowSimpleVFX_EnemyAction ACT_MAGNET_VFX = new ShowSimpleVFX_EnemyAction();

		protected override void DoOnStop()
		{
			base.DoOnStop();
			ACT_MOVE_AGENT.StopAction();
			ACT_WAIT.StopAction();
			ACT_MOVE_EASING_1.StopAction();
			ACT_MOVE_EASING_2.StopAction();
			foreach (AmanecidaAxeBehaviour axis in axes)
			{
				axis.axeSplineFollowAction.StopAction();
			}
		}

		public EnemyAction StartAction(EnemyBehaviour e, int _n, Vector2 point, float distance, int dir, List<AmanecidaAxeBehaviour> axes, List<BezierSpline> splines, List<SplineThrowData> throwData)
		{
			n = _n;
			this.axes = axes;
			this.dir = dir;
			this.point = point;
			this.splines = splines;
			this.distance = distance;
			this.throwData = throwData;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			if (o.IsWieldingAxe())
			{
				o.ShowBothAxes(v: true);
				o.ShowCurrentWeapon(show: false);
			}
			axes[0].SetRepositionMode(isInReposition: true);
			axes[1].SetRepositionMode(isInReposition: true);
			ACT_MOVE_AGENT.StartAction(owner, owner.GetComponent<AutonomousAgent>(), point + Vector2.up);
			yield return ACT_MOVE_AGENT.waitForCompletion;
			AmanecidasBehaviour ab = owner as AmanecidasBehaviour;
			ab.LookAtPenitent();
			Vector2 originAxePoint = (Vector2)owner.transform.position + new Vector2(ab.axeOffset.x * (float)dir, ab.axeOffset.y);
			float secondsToArrive = 0.75f;
			axes[0].SetSeek(free: false);
			axes[1].SetSeek(free: false);
			ACT_MOVE_EASING_1.StartAction(axes[0], originAxePoint, secondsToArrive, Ease.InOutQuad);
			originAxePoint += Vector2.down * 1.5f;
			ACT_MOVE_EASING_2.StartAction(axes[1], originAxePoint, secondsToArrive, Ease.InOutQuad);
			yield return ACT_MOVE_EASING_1.waitForCompletion;
			yield return ACT_MOVE_EASING_2.waitForCompletion;
			axes[0].SetRepositionMode(isInReposition: false);
			axes[1].SetRepositionMode(isInReposition: false);
			ACT_WAIT.StartAction(owner, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					AmanecidaAxeBehaviour axe = axes[j];
					Vector2 originPoint = axe.transform.position;
					Vector2 targetPoint = originPoint + Vector2.right * distance * dir;
					o.Amanecidas.AnimatorInyector.PlayStompAttack(doStompDamage: true);
					o.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
					o.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: false);
					o.Amanecidas.Audio.PlayAxeThrow_AUDIO();
					axe.axeSplineFollowAction.StartAction(axe, axe.splineFollower, originPoint, targetPoint, 3, throwData[j], splines[j]);
					ACT_WAIT.StartAction(owner, 0.5f);
					yield return ACT_WAIT.waitForCompletion;
					o.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
					yield return new WaitUntilIdle(o);
					o.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
				}
				ACT_WAIT.StartAction(owner, throwData[0].duration);
				yield return ACT_WAIT.waitForCompletion;
			}
			o.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class JumpSmash_EnemyAction : EnemyAction
	{
		private Action jumpMethod;

		private Vector2 jumpOrigin;

		private bool getTiredAtTheEnd;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private LaunchMethod_EnemyAction ACT_JUMP = new LaunchMethod_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private CallAxe_EnemyAction ACT_CALLAXE1 = new CallAxe_EnemyAction();

		private CallAxe_EnemyAction ACT_CALLAXE2 = new CallAxe_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private TiredPeriod_EnemyAction ACT_TIRED = new TiredPeriod_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_JUMP.StopAction();
			ACT_BLINK.StopAction();
			ACT_CALLAXE1.StopAction();
			ACT_CALLAXE2.StopAction();
			ACT_MOVE.StopAction();
			ACT_TIRED.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 jumpOrigin, Action jumpMethod, bool getTiredAtTheEnd)
		{
			this.jumpOrigin = jumpOrigin;
			this.jumpMethod = jumpMethod;
			this.getTiredAtTheEnd = getTiredAtTheEnd;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			ACT_BLINK.StartAction(owner, jumpOrigin, 0.3f, reappear: true, lookAtPenitent: true);
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			Vector2 dir = ((!(jumpOrigin.x > ama.battleBounds.center.x)) ? Vector2.right : Vector2.left);
			Vector2 p = jumpOrigin + Vector2.up - dir * 0.75f;
			float seconds = 0.3f;
			if (!ama.IsWieldingAxe())
			{
				ama.axes[0].SetSeek(free: false);
				ama.axes[1].SetSeek(free: false);
				ACT_CALLAXE1.StartAction(owner, p, ama.axes[0], seconds, seconds);
				ACT_CALLAXE2.StartAction(owner, p, ama.axes[1], seconds, seconds);
				yield return ACT_CALLAXE1.waitForCompletion;
			}
			yield return ACT_BLINK.waitForCompletion;
			ama.ShowBothAxes(v: false);
			ama.ShowCurrentWeapon(show: true);
			ama.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: true);
			ama.Amanecidas.AnimatorInyector.PlayMeleeAttack();
			ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
			ama.Amanecidas.Audio.PlayAxeSmashPreattack_AUDIO();
			ACT_MOVE.StartAction(owner, (Vector2)owner.transform.position - dir, 0.5f, Ease.OutBack);
			yield return ACT_MOVE.waitForCompletion;
			ACT_JUMP.StartAction(owner, jumpMethod);
			ACT_WAIT.StartAction(owner, 0.6f);
			yield return ACT_WAIT.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
			ama.Amanecidas.Audio.PlayAxeSmash_AUDIO();
			ACT_WAIT.StartAction(owner, 1f);
			yield return ACT_WAIT.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			if (getTiredAtTheEnd)
			{
				ACT_TIRED.StartAction(owner, 3f, interruptable: true);
				yield return ACT_TIRED.waitForCompletion;
			}
			FinishAction();
		}
	}

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
			ACT_WAIT.StopAction();
			ACT_JUMP.StopAction();
			ACT_SHOOT.StopAction();
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

	public class ShowDashAnticipationTrail_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private Vector2 startPoint;

		private Vector2 endPoint;

		private float duration;

		protected override void DoOnStop()
		{
			base.DoOnStop();
			ACT_WAIT.StopAction();
		}

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 startPoint, Vector2 endPoint, float duration)
		{
			this.startPoint = startPoint;
			this.endPoint = endPoint;
			this.duration = duration;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner.GetComponent<AmanecidasBehaviour>();
			GameObject trailObject = ama.trailGameObject;
			trailObject.transform.position = startPoint;
			ParticleSystem particles = trailObject.GetComponent<ParticleSystem>();
			trailObject.transform.right = endPoint - startPoint;
			particles.Play();
			ACT_WAIT.StartAction(owner, duration);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class CallAxe_EnemyAction : EnemyAction
	{
		private Vector2 pointA;

		private AmanecidaAxeBehaviour axe;

		private float secondsBeforeAttraction;

		private float secondsToArrive;

		private ShowSimpleVFX_EnemyAction ACT_MAGNET_VFX = new ShowSimpleVFX_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_AXE_X = new MoveEasing_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_AXE_Y = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_MAGNET_VFX.StopAction();
			ACT_WAIT.StopAction();
			ACT_MOVE_AXE_X.StopAction();
			ACT_MOVE_AXE_Y.StopAction();
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 pointA, AmanecidaAxeBehaviour axe, float secondsBeforeAttraction, float secondsToArrive)
		{
			this.pointA = pointA;
			this.axe = axe;
			this.secondsBeforeAttraction = secondsBeforeAttraction;
			this.secondsToArrive = secondsToArrive;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ab = owner.GetComponent<AmanecidasBehaviour>();
			ACT_WAIT.StartAction(owner, secondsBeforeAttraction);
			yield return ACT_WAIT.waitForCompletion;
			axe.SetRepositionMode(isInReposition: true);
			if (pointA.y > ab.battleBounds.center.y)
			{
				ACT_MOVE_AXE_X.StartAction(axe, pointA, 0.6f, Ease.InBack, null, _timeScaled: true, null, _tweenOnX: true, _tweenOnY: false);
				ACT_MOVE_AXE_Y.StartAction(axe, pointA, 0.4f, Ease.OutBack, null, _timeScaled: true, null, _tweenOnX: false);
			}
			else
			{
				ACT_MOVE_AXE_X.StartAction(axe, pointA, 0.6f, Ease.OutBack, null, _timeScaled: true, null, _tweenOnX: true, _tweenOnY: false);
				ACT_MOVE_AXE_Y.StartAction(axe, pointA, 0.4f, Ease.InBack, null, _timeScaled: true, null, _tweenOnX: false);
			}
			yield return ACT_MOVE_AXE_Y.waitForCompletion;
			yield return ACT_MOVE_AXE_X.waitForCompletion;
			axe.SetRepositionMode(isInReposition: false);
			FinishAction();
		}
	}

	public class LaunchAxesToPenitent_EnemyAction : EnemyAction
	{
		private AmanecidaAxeBehaviour firstAxe;

		private AmanecidaAxeBehaviour secondAxe;

		private float minAnticipationSeconds;

		private int numThrows;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_CHARACTER = new MoveToPointUsingAgent_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_AXE_X = new MoveEasing_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_AXE_Y = new MoveEasing_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_AXE_FULL = new MoveEasing_EnemyAction();

		private CallAxe_EnemyAction ACT_CALLAXE1 = new CallAxe_EnemyAction();

		private CallAxe_EnemyAction ACT_CALLAXE2 = new CallAxe_EnemyAction();

		private KeepDistanceFromTPOUsingAgent_EnemyAction ACT_KEEPDISTANCE = new KeepDistanceFromTPOUsingAgent_EnemyAction();

		private KeepDistanceFromAmanecidaUsingAgent_EnemyAction ACT_KEEPDISTANCE_AXE = new KeepDistanceFromAmanecidaUsingAgent_EnemyAction();

		private LookAtPenitent_EnemyAction ACT_LOOK = new LookAtPenitent_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE_CHARACTER.StopAction();
			ACT_MOVE_AXE_X.StopAction();
			ACT_MOVE_AXE_Y.StopAction();
			ACT_CALLAXE1.StopAction();
			ACT_CALLAXE2.StopAction();
			ACT_MOVE_AXE_FULL.StopAction();
			ACT_KEEPDISTANCE.StopAction();
			ACT_KEEPDISTANCE_AXE.StopAction();
			ACT_LOOK.StopAction();
			AmanecidasBehaviour component = owner.GetComponent<AmanecidasBehaviour>();
			component.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
			component.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, AmanecidaAxeBehaviour firstAxe, AmanecidaAxeBehaviour secondAxe, float minAnticipationSeconds, int numThrows)
		{
			this.firstAxe = firstAxe;
			this.secondAxe = secondAxe;
			this.minAnticipationSeconds = minAnticipationSeconds;
			this.numThrows = numThrows;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner.GetComponent<AmanecidasBehaviour>();
			Vector2 startingPos = new Vector2(ama.transform.position.x, ama.battleBounds.yMax - 1.5f);
			ACT_MOVE_CHARACTER.StartAction(owner, ama.agent, startingPos);
			yield return ACT_MOVE_CHARACTER.waitForCompletion;
			for (int i = 0; i < numThrows; i++)
			{
				bool usingFirstAxe = i % 2 == 0;
				AmanecidaAxeBehaviour currentAxe = ((!usingFirstAxe) ? secondAxe : firstAxe);
				int dir3 = ama.GetDirFromOrientation();
				Vector2 axeStartingPos2 = (Vector2)ama.transform.position + new Vector2(dir3, 1.5f);
				if (i > 1)
				{
					ama.verticalFastBlastAxeAttack.SummonAreaOnPoint(currentAxe.transform.position);
					ACT_WAIT.StartAction(owner, 0.3f);
					yield return ACT_WAIT.waitForCompletion;
					ACT_MOVE_AXE_X.StartAction(currentAxe, axeStartingPos2, 0.6f, Ease.InBack, null, _timeScaled: true, null, _tweenOnX: true, _tweenOnY: false);
					ACT_MOVE_AXE_Y.StartAction(currentAxe, axeStartingPos2, 0.4f, Ease.OutBack, null, _timeScaled: true, null, _tweenOnX: false);
					yield return ACT_MOVE_AXE_Y.waitForCompletion;
					yield return ACT_MOVE_AXE_X.waitForCompletion;
				}
				ama.ShowAxe(show: false, usingFirstAxe);
				ama.ShowCurrentWeapon(show: true);
				ACT_LOOK.StartAction(owner);
				yield return ACT_LOOK.waitForCompletion;
				ama.LookAtPenitent(instant: true);
				ama.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: true);
				ama.Amanecidas.AnimatorInyector.PlayMeleeAttack();
				ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
				float anticipationSeconds = minAnticipationSeconds + Mathf.Lerp(0.5f, 1f, ama.GetDirToPenitent(ama.transform.position).x / ama.battleBounds.width);
				ACT_KEEPDISTANCE.StartAction(owner, anticipationSeconds, driftHoriontally: true, driftVertically: false, -ama.GetDirFromOrientation());
				ACT_WAIT.StartAction(owner, anticipationSeconds * 0.9f);
				yield return ACT_WAIT.waitForCompletion;
				ama.Amanecidas.Audio.PlayAxeThrowPreattack_AUDIO();
				yield return ACT_KEEPDISTANCE.waitForCompletion;
				ama.Amanecidas.Audio.PlayAxeThrow_AUDIO();
				ama.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
				ACT_WAIT.StartAction(owner, 0.1f);
				yield return ACT_WAIT.waitForCompletion;
				dir3 = ama.GetDirFromOrientation();
				axeStartingPos2 = (Vector2)ama.transform.position + new Vector2(dir3, 1.2f);
				ama.ShowCurrentWeapon(show: false);
				ama.ShowAxe(show: false, !usingFirstAxe);
				ama.ShowAxe(show: true, usingFirstAxe, setAxePosition: true, axeStartingPos2);
				ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
				dir3 = ama.GetDirFromOrientation();
				Vector2 targetPos = new Vector2(ama.transform.position.x + (float)ama.GetDirFromOrientation() * 1.5f, ama.battleBounds.yMin);
				float travelTime = 0.3f;
				yield return currentAxe.transform.DOPunchPosition(Vector2.up - Vector2.right * dir3 * 0.2f, 0.1f, 2, 0.1f).WaitForCompletion();
				ACT_MOVE_AXE_FULL.StartAction(currentAxe, targetPos, travelTime, Ease.OutQuad);
				yield return ACT_MOVE_AXE_FULL.waitForCompletion;
				ACT_WAIT.StartAction(owner, travelTime * 0.7f);
				yield return ACT_WAIT.waitForCompletion;
				ama.verticalNormalBlastAxeAttack.SummonAreas(currentAxe.transform.position, Vector3.right, EntityOrientation.Right);
				ama.verticalNormalBlastAxeAttack.SummonAreas(currentAxe.transform.position, Vector3.left, EntityOrientation.Left);
				yield return ACT_MOVE_AXE_FULL.waitForCompletion;
			}
			Vector2 endingPos = (Vector2)ama.transform.position + Vector2.up * 1.5f;
			ACT_CALLAXE1.StartAction(owner, endingPos + Vector2.right * ama.GetDirFromOrientation(), firstAxe, 0.2f, 0.5f);
			ACT_CALLAXE2.StartAction(owner, endingPos - Vector2.right * ama.GetDirFromOrientation(), secondAxe, 0.2f, 0.5f);
			yield return ACT_CALLAXE1.waitForCompletion;
			yield return ACT_CALLAXE2.waitForCompletion;
			FinishAction();
		}
	}

	public class LaunchCrawlerAxesToPenitent_EnemyAction : EnemyAction
	{
		private AmanecidaAxeBehaviour firstAxe;

		private AmanecidaAxeBehaviour secondAxe;

		private float minAnticipationSeconds;

		private int numThrows;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_CHARACTER = new MoveToPointUsingAgent_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_AXE_X = new MoveEasing_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_AXE_Y = new MoveEasing_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_AXE_FULL = new MoveEasing_EnemyAction();

		private CallAxe_EnemyAction ACT_CALLAXE1 = new CallAxe_EnemyAction();

		private CallAxe_EnemyAction ACT_CALLAXE2 = new CallAxe_EnemyAction();

		private KeepDistanceFromTPOUsingAgent_EnemyAction ACT_KEEPDISTANCE = new KeepDistanceFromTPOUsingAgent_EnemyAction();

		private KeepDistanceFromAmanecidaUsingAgent_EnemyAction ACT_KEEPDISTANCE_AXE = new KeepDistanceFromAmanecidaUsingAgent_EnemyAction();

		private LookAtPenitent_EnemyAction ACT_LOOK = new LookAtPenitent_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE_CHARACTER.StopAction();
			ACT_MOVE_AXE_X.StopAction();
			ACT_MOVE_AXE_Y.StopAction();
			ACT_CALLAXE1.StopAction();
			ACT_CALLAXE2.StopAction();
			ACT_MOVE_AXE_FULL.StopAction();
			ACT_KEEPDISTANCE.StopAction();
			ACT_KEEPDISTANCE_AXE.StopAction();
			ACT_LOOK.StopAction();
			AmanecidasBehaviour component = owner.GetComponent<AmanecidasBehaviour>();
			component.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
			component.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, AmanecidaAxeBehaviour firstAxe, AmanecidaAxeBehaviour secondAxe, float minAnticipationSeconds, int numThrows)
		{
			this.firstAxe = firstAxe;
			this.secondAxe = secondAxe;
			this.minAnticipationSeconds = minAnticipationSeconds;
			this.numThrows = numThrows;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner.GetComponent<AmanecidasBehaviour>();
			Vector2 startingPos = new Vector2(ama.transform.position.x, ama.battleBounds.yMax - 2f);
			ACT_MOVE_CHARACTER.StartAction(owner, ama.agent, startingPos);
			yield return ACT_MOVE_CHARACTER.waitForCompletion;
			for (int i = 0; i < numThrows; i++)
			{
				bool usingFirstAxe = i % 2 == 0;
				AmanecidaAxeBehaviour currentAxe = ((!usingFirstAxe) ? secondAxe : firstAxe);
				ama.ShowAxe(show: false, usingFirstAxe);
				if (!ama.IsWieldingAxe())
				{
					ama.DoSummonWeaponAnimation();
					ACT_WAIT.StartAction(owner, 0.4f);
					yield return ACT_WAIT.waitForCompletion;
				}
				ACT_LOOK.StartAction(owner);
				yield return ACT_LOOK.waitForCompletion;
				ama.LookAtPenitent(instant: true);
				ama.Amanecidas.Audio.PlayDashCharge_AUDIO();
				ama.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: true);
				ama.Amanecidas.AnimatorInyector.PlayMeleeAttack();
				ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
				float anticipationSeconds = minAnticipationSeconds + Mathf.Lerp(0.5f, 1f, ama.GetDirToPenitent(ama.transform.position).x / ama.battleBounds.width);
				ACT_KEEPDISTANCE.StartAction(owner, anticipationSeconds, driftHoriontally: true, driftVertically: false, -ama.GetDirFromOrientation());
				ACT_WAIT.StartAction(owner, anticipationSeconds * 0.95f);
				yield return ACT_WAIT.waitForCompletion;
				ama.Amanecidas.Audio.PlayAxeThrowPreattack_AUDIO();
				yield return ACT_KEEPDISTANCE.waitForCompletion;
				ama.Amanecidas.Audio.StopDashCharge_AUDIO();
				ama.Amanecidas.Audio.PlayAxeThrow_AUDIO();
				ama.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
				ACT_WAIT.StartAction(owner, 0.1f);
				yield return ACT_WAIT.waitForCompletion;
				int dir = ama.GetDirFromOrientation();
				Vector2 axeStartingPos = (Vector2)ama.transform.position + new Vector2(dir, 1.2f);
				ama.ShowCurrentWeapon(show: false);
				ama.ShowAxe(show: true, usingFirstAxe, setAxePosition: true, axeStartingPos);
				if (i < numThrows - 1)
				{
					ama.ShowAxe(show: false, !usingFirstAxe);
				}
				ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
				dir = ama.GetDirFromOrientation();
				Vector2 targetPos = new Vector2(ama.transform.position.x + (float)ama.GetDirFromOrientation() * 1.5f, ama.battleBounds.yMin);
				float travelTime = 0.2f;
				ACT_MOVE_AXE_FULL.StartAction(currentAxe, targetPos, travelTime, Ease.InSine);
				yield return ACT_MOVE_AXE_FULL.waitForCompletion;
				Vector2 exitDir = ((!(ama.GetDirToPenitent(targetPos).x > 0f)) ? Vector2.left : Vector2.right);
				EntityOrientation shockwaveOrientation = ((!(exitDir.x > 0f)) ? EntityOrientation.Left : EntityOrientation.Right);
				ama.shockwave.SummonAreaOnPoint(targetPos);
				ama.ShakeWave(doShockwave: false);
				ACT_WAIT.StartAction(owner, 0.1f);
				yield return ACT_WAIT.waitForCompletion;
				Vector2 outOfCameraPos = exitDir * ama.battleBounds.width * 2f;
				outOfCameraPos.y = currentAxe.transform.position.y;
				ACT_MOVE_AXE_FULL.StartAction(currentAxe, outOfCameraPos, 2f, Ease.InBack, null, _timeScaled: true, null, _tweenOnX: true, _tweenOnY: true, 1.4f);
				ACT_WAIT.StartAction(owner, 1f);
				yield return ACT_WAIT.waitForCompletion;
			}
			yield return ACT_MOVE_AXE_FULL.waitForCompletion;
			Vector2 endingPos = (Vector2)ama.transform.position + Vector2.up * 1.5f;
			ACT_CALLAXE1.StartAction(owner, endingPos + Vector2.right * ama.GetDirFromOrientation(), firstAxe, 0.2f, 0.5f);
			ACT_CALLAXE2.StartAction(owner, endingPos - Vector2.right * ama.GetDirFromOrientation(), secondAxe, 0.2f, 0.5f);
			yield return ACT_CALLAXE1.waitForCompletion;
			yield return ACT_CALLAXE2.waitForCompletion;
			FinishAction();
		}
	}

	public class LaunchBallsToPenitent_EnemyAction : EnemyAction
	{
		private float anticipationSeconds;

		private int numThrows;

		private bool keepDistance;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private KeepDistanceFromTPOUsingAgent_EnemyAction ACT_KEEPDISTANCE = new KeepDistanceFromTPOUsingAgent_EnemyAction();

		private LookAtPenitent_EnemyAction ACT_LOOK = new LookAtPenitent_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_KEEPDISTANCE.StopAction();
			ACT_LOOK.StopAction();
			AmanecidasBehaviour component = owner.GetComponent<AmanecidasBehaviour>();
			component.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
			component.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, float anticipationSeconds, int numThrows, bool keepDistance)
		{
			this.anticipationSeconds = anticipationSeconds;
			this.numThrows = numThrows;
			this.keepDistance = keepDistance;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner.GetComponent<AmanecidasBehaviour>();
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			ama.ShowBothAxes(v: false);
			ama.ShowCurrentWeapon(show: false);
			if (keepDistance)
			{
				Vector3 throwingPos = ama.transform.position;
				throwingPos.y = ama.battleBounds.yMin + 1f;
				throwingPos.x = ((!(p.GetPosition().x > throwingPos.x)) ? (ama.battleBounds.xMax - 2f) : (ama.battleBounds.xMin + 2f));
				ACT_MOVE.StartAction(owner, throwingPos, 0.1f, Ease.InOutQuad);
				yield return ACT_MOVE.waitForCompletion;
			}
			for (int i = 0; i < numThrows; i++)
			{
				if (keepDistance)
				{
					ACT_LOOK.StartAction(owner);
					yield return ACT_LOOK.waitForCompletion;
				}
				ama.LookAtPenitent(instant: true);
				ama.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: true);
				ama.Amanecidas.AnimatorInyector.PlayMeleeAttack();
				ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
				ama.PlayChargeEnergy(anticipationSeconds);
				if (keepDistance)
				{
					ACT_KEEPDISTANCE.StartAction(owner, anticipationSeconds, driftHoriontally: true, driftVertically: false, clampHorizontally: true, ama.battleBounds.xMin, ama.battleBounds.xMax);
					yield return ACT_KEEPDISTANCE.waitForCompletion;
					ACT_WAIT.StartAction(owner, 0.1f);
					yield return ACT_WAIT.waitForCompletion;
				}
				else
				{
					ACT_WAIT.StartAction(owner, anticipationSeconds);
					yield return ACT_WAIT.waitForCompletion;
				}
				ama.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
				Vector3 targetPos = p.GetPosition();
				targetPos.y = ama.battleBounds.yMin;
				ama.lavaBallAttack.Shoot(ama.GetDirToPenitent(ama.transform.position).normalized, Vector2.zero, (Vector3)Vector2.zero, 1f);
				ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
				ACT_WAIT.StartAction(owner, 0.5f);
				yield return ACT_WAIT.waitForCompletion;
			}
			FinishAction();
		}
	}

	public class ChangeWeapon_EnemyAction : EnemyAction
	{
		private float waitSeconds;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private RechargeShield_EnemyAction ACT_RECHARGE = new RechargeShield_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_CHARACTER = new MoveToPointUsingAgent_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_RECHARGE.StopAction();
			ACT_MOVE_CHARACTER.StopAction();
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, float waitSeconds)
		{
			this.waitSeconds = waitSeconds;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			ACT_RECHARGE.StartAction(owner, o.currentFightParameters.shieldRechargeTime, o.currentFightParameters.shieldShockwaveAnticipationTime, o.DoRechargeShield, o.DoAnticipateShockwave, o.DoShieldShockwave);
			ACT_WAIT.StartAction(owner, 2f);
			yield return ACT_WAIT.waitForCompletion;
			AmanecidasAnimatorInyector.AMANECIDA_WEAPON nextWeapon = o.currentFightParameters.weapon;
			o.SetWeapon(nextWeapon);
			o.SummonWeapon();
			yield return ACT_RECHARGE.waitForCompletion;
			o.currentMeleeAttack = o.meleeStompAttack;
			o.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
			o.Amanecidas.AnimatorInyector.PlayStompAttack(doStompDamage: false);
			o.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: false);
			o.Amanecidas.SetNextLaudesArena();
			ACT_MOVE_CHARACTER.StartAction(owner, o.agent, new Vector2(o.transform.position.x, o.battleBounds.yMax));
			ACT_WAIT.StartAction(owner, waitSeconds);
			yield return ACT_WAIT.waitForCompletion;
			yield return ACT_MOVE_CHARACTER.waitForCompletion;
			o.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			yield return new WaitUntilIdle(o);
			o.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			o.needsToSwapWeapon = false;
			FinishAction();
		}
	}

	public class FalcataSlashBarrage_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private FalcataSlashProjectile_EnemyAction ACT_SLASH = new FalcataSlashProjectile_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private LookAtPenitent_EnemyAction ACT_LOOK = new LookAtPenitent_EnemyAction();

		private bool startsFromRight;

		private float distance;

		private int n;

		private float anticipationSeconds;

		public EnemyAction StartAction(EnemyBehaviour e, int n, bool startsFromRight, float distance, float anticipationSeconds)
		{
			this.n = n;
			this.startsFromRight = startsFromRight;
			this.distance = distance;
			this.anticipationSeconds = anticipationSeconds;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_SLASH.StopAction();
			ACT_BLINK.StopAction();
			ACT_WAIT.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			Vector2 targetPos = Core.Logic.Penitent.GetPosition();
			int dir = (startsFromRight ? 1 : (-1));
			Vector2 p = targetPos + Vector2.right * distance * dir;
			p.x = Mathf.Clamp(p.x, ama.battleBounds.xMin, ama.battleBounds.xMax);
			p.y = ama.battleBounds.yMax;
			float seconds = 0.1f;
			float tooCloseDistance = 5f;
			if (ama.GetDirToPenitent().magnitude < tooCloseDistance || ama.GetDirToPenitent().y > 0f)
			{
				ACT_BLINK.StartAction(owner, p, seconds, reappear: true, lookAtPenitent: true);
				yield return ACT_BLINK.waitForCompletion;
			}
			else
			{
				ACT_LOOK.StartAction(owner);
				yield return ACT_LOOK.waitForCompletion;
			}
			Vector2 directionVector2 = ama.GetDirToPenitent(ama.transform.position);
			float maxAngle = 90f;
			float initialAngle = -45f;
			directionVector2 = Quaternion.Euler(0f, 0f, 0f - initialAngle) * directionVector2;
			Quaternion stepRotation = Quaternion.Euler(0f, 0f, (0f - maxAngle) / (float)n);
			List<Vector2> directions = new List<Vector2>();
			for (int i = 0; i < n; i++)
			{
				directions.Add(directionVector2);
				directionVector2 = stepRotation * directionVector2;
			}
			ACT_SLASH.StartAction(owner, directions, anticipationSeconds);
			yield return ACT_SLASH.waitForCompletion;
			FinishAction();
		}
	}

	public class ChainDash_EnemyAction : EnemyAction
	{
		private int n;

		private BlinkAndDashToPenitent_EnemyAction ACT_BLINKDASH = new BlinkAndDashToPenitent_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_CHARACTER = new MoveToPointUsingAgent_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, int n)
		{
			this.n = n;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_BLINKDASH.StopAction();
			ACT_WAIT.StopAction();
			ACT_MOVE_CHARACTER.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			int dir2 = ama.GetDirFromOrientation();
			ACT_MOVE_CHARACTER.StartAction(owner, ama.agent, (Vector2)owner.transform.position + new Vector2((float)(-dir2) * 0.3f, 0.3f));
			ACT_WAIT.StartAction(owner, 0.4f);
			yield return ACT_MOVE_CHARACTER.waitForCompletion;
			yield return ACT_WAIT.waitForCompletion;
			ama.throwbackExtraTime = 0f;
			for (int i = 0; i < n; i++)
			{
				if (ama.throwbackExtraTime > 0f)
				{
					ACT_WAIT.StartAction(owner, ama.throwbackExtraTime);
					yield return ACT_WAIT.waitForCompletion;
				}
				ama.SmallDistortion();
				ama.throwbackExtraTime = 0f;
				ACT_BLINKDASH.StartAction(owner, owner.transform.position, ama.slashDashAttack, 0.5f, skipReposition: true, endBlinkOut: true, showDashAim: true, 20f);
				yield return ACT_BLINKDASH.waitForCompletion;
				ama.ClearRotationAndFlip();
				float distance = 5f;
				dir2 = ama.GetDirFromOrientation();
				ama.transform.position = ama.GetPointBelowPenitent(stopOnOneWayDowns: true) + dir2 * Vector2.right * distance;
			}
			float clampedX = Mathf.Clamp(ama.transform.position.x, ama.battleBounds.xMin, ama.battleBounds.xMax);
			ama.transform.position = new Vector2(clampedX, ama.transform.position.y);
			ama.LookAtPenitentUsingOrientation();
			ama.Amanecidas.AnimatorInyector.SetBlink(value: false);
			yield return new WaitUntilIdle(ama);
			FinishAction();
		}
	}

	public class FalcataSlashProjectile_EnemyAction : EnemyAction
	{
		private List<Vector2> dirs;

		private float meleeAnticipationSeconds;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private LookAtPenitent_EnemyAction ACT_LOOK = new LookAtPenitent_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 dir, float meleeAnticipation)
		{
			return StartAction(e, new List<Vector2> { dir }, meleeAnticipation);
		}

		public EnemyAction StartAction(EnemyBehaviour e, List<Vector2> dirs, float meleeAnticipation)
		{
			this.dirs = dirs;
			meleeAnticipationSeconds = meleeAnticipation;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_LOOK.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			ACT_LOOK.StartAction(owner);
			yield return ACT_LOOK.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.PlayMeleeAttack();
			ama.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: true);
			ACT_WAIT.StartAction(owner, meleeAnticipationSeconds);
			yield return ACT_WAIT.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
			ACT_WAIT.StartAction(owner, 0.25f);
			yield return ACT_WAIT.waitForCompletion;
			ama.Amanecidas.Audio.PlaySwordAttack_AUDIO();
			ama.Amanecidas.Audio.PlaySwordFirePreattack_AUDIO();
			foreach (Vector2 dir in dirs)
			{
				ama.falcataSlashProjectileAttack.Shoot(dir, ama.GetDirFromOrientation() * Vector2.right);
			}
			ACT_WAIT.StartAction(owner, 0.2f);
			yield return ACT_WAIT.waitForCompletion;
			ACT_WAIT.StartAction(owner, 0.1f);
			yield return ACT_WAIT.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
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

	public class StompAttack_EnemyAction : EnemyAction
	{
		private Action jumpMethod;

		private Vector2 targetPoint;

		private bool doBlinkBeforeJump;

		private AmanecidasMeleeAttack previousMeleeAttack;

		private bool doStompDamage;

		private bool usePillars;

		private bool bounceAfterJump;

		private float bounceHeight;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private LaunchMethod_EnemyAction ACT_JUMP = new LaunchMethod_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private LookAtPenitent_EnemyAction ACT_LOOK = new LookAtPenitent_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_JUMP.StopAction();
			ACT_BLINK.StopAction();
			ACT_MOVE.StopAction();
			ACT_LOOK.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			amanecidasBehaviour.currentMeleeAttack.damageOnEnterArea = false;
			amanecidasBehaviour.currentMeleeAttack = previousMeleeAttack;
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, bool doBlinkBeforeJump, bool doStompDamage, bool usePillars, bool bounceAfterJump, float bounceHeight = 1f)
		{
			this.doBlinkBeforeJump = doBlinkBeforeJump;
			this.doStompDamage = doStompDamage;
			this.usePillars = usePillars;
			this.bounceAfterJump = bounceAfterJump;
			this.bounceHeight = bounceHeight;
			return StartAction(e);
		}

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 targetPoint, bool doBlinkBeforeJump, bool doStompDamage, bool usePillars, bool bounceAfterJump, float bounceHeight = 1f)
		{
			this.targetPoint = targetPoint;
			this.doBlinkBeforeJump = doBlinkBeforeJump;
			this.doStompDamage = doStompDamage;
			this.usePillars = usePillars;
			this.bounceAfterJump = bounceAfterJump;
			this.bounceHeight = bounceHeight;
			return StartAction(e);
		}

		public EnemyAction StartAction(EnemyBehaviour e, Action jumpMethod, bool doBlinkBeforeJump, bool doStompDamage, bool bounceAfterJump, float bounceHeight = 1f)
		{
			this.jumpMethod = jumpMethod;
			this.doBlinkBeforeJump = doBlinkBeforeJump;
			this.doStompDamage = doStompDamage;
			this.bounceAfterJump = bounceAfterJump;
			this.bounceHeight = bounceHeight;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			previousMeleeAttack = ama.currentMeleeAttack;
			if (doBlinkBeforeJump)
			{
				Vector2 target = ama.GetPointBelowPenitent(stopOnOneWayDowns: false);
				Vector2 point = target + Vector2.up * 4f;
				ACT_BLINK.StartAction(owner, point, 0.2f, reappear: true, lookAtPenitent: true);
				yield return ACT_BLINK.waitForCompletion;
			}
			else
			{
				ACT_LOOK.StartAction(owner);
				yield return ACT_LOOK.waitForCompletion;
			}
			Vector2 dir = Vector2.right * ama.GetDirFromOrientation();
			ama.currentMeleeAttack = ama.meleeStompAttack;
			ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
			ama.Amanecidas.AnimatorInyector.PlayStompAttack(doStompDamage);
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: false);
			if (jumpMethod != null)
			{
				jumpMethod();
			}
			else if (doBlinkBeforeJump)
			{
				ama.DoSmallJumpSmash(usePillars);
			}
			else
			{
				ama.DoSmallJumpSmashToPoint(targetPoint, usePillars);
			}
			ACT_WAIT.StartAction(owner, 1.25f);
			yield return ACT_WAIT.waitForCompletion;
			if (!doBlinkBeforeJump && bounceAfterJump)
			{
				ACT_MOVE.StartAction(owner, (Vector2)ama.transform.position + Vector2.up * bounceHeight, 0.3f, Ease.OutQuart);
				yield return ACT_MOVE.waitForCompletion;
			}
			ama.LookAtPenitentUsingOrientation();
			ama.Amanecidas.AnimatorInyector.ClearStompAttackTrigger();
			ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			yield return new WaitUntilIdle(ama);
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			ama.currentMeleeAttack = previousMeleeAttack;
			FinishAction();
		}
	}

	public class MultiStompAttack_EnemyAction : EnemyAction
	{
		private int n;

		private float secondsBetweenJumps;

		private Action jumpMethod;

		private bool doBlinkBeforeJump;

		private bool doStompDamage;

		private bool usePillars;

		private bool bounceAfterJump;

		private float bounceHeight;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private LaunchMethod_EnemyAction ACT_JUMP = new LaunchMethod_EnemyAction();

		private StompAttack_EnemyAction ACT_STOMP = new StompAttack_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, int n, float secondsBetweenJumps, bool doBlinkBeforeJump, bool doStompDamage, bool usePillars, bool bounceAfterJump, float bounceHeight = 1f)
		{
			this.n = n;
			this.secondsBetweenJumps = secondsBetweenJumps;
			this.doBlinkBeforeJump = doBlinkBeforeJump;
			this.doStompDamage = doStompDamage;
			this.usePillars = usePillars;
			this.bounceAfterJump = bounceAfterJump;
			this.bounceHeight = bounceHeight;
			return StartAction(e);
		}

		public EnemyAction StartAction(EnemyBehaviour e, int n, float secondsBetweenJumps, Action jumpMethod, bool doBlinkBeforeJump, bool doStompDamage, bool usePillars, bool bounceAfterJump, float bounceHeight = 1f)
		{
			this.n = n;
			this.secondsBetweenJumps = secondsBetweenJumps;
			this.jumpMethod = jumpMethod;
			this.doBlinkBeforeJump = doBlinkBeforeJump;
			this.doStompDamage = doStompDamage;
			this.bounceAfterJump = bounceAfterJump;
			this.bounceHeight = bounceHeight;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_JUMP.StopAction();
			ACT_STOMP.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			for (int i = 0; i < n; i++)
			{
				if (jumpMethod != null)
				{
					ACT_STOMP.StartAction(owner, jumpMethod, doBlinkBeforeJump, doStompDamage, bounceAfterJump, bounceHeight);
				}
				else
				{
					ACT_STOMP.StartAction(owner, doBlinkBeforeJump, doStompDamage, usePillars, bounceAfterJump, bounceHeight);
				}
				yield return ACT_STOMP.waitForCompletion;
				ACT_WAIT.StartAction(owner, secondsBetweenJumps);
				yield return ACT_WAIT.waitForCompletion;
			}
			FinishAction();
		}
	}

	public class Dash_EnemyAction : EnemyAction
	{
		private Vector2 dir;

		private float distance;

		private bool finished;

		private bool unblockable;

		public bool parried;

		private BossDashAttack dashAttack;

		protected override void DoOnStop()
		{
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, Transform target, BossDashAttack dashAttack)
		{
			dir = target.position - e.transform.position;
			distance = dir.magnitude;
			return StartAction(e, dir, dashAttack, distance);
		}

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 dir, BossDashAttack dashAttack, float distance = 10f, bool unblockable = false)
		{
			this.dir = dir;
			this.distance = distance;
			finished = false;
			this.dashAttack = dashAttack;
			this.unblockable = unblockable;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			parried = false;
			dashAttack.OnDashBlockedEvent -= DashAttack_OnDashBlocked;
			dashAttack.OnDashFinishedEvent -= DashAttack_OnDashFinished;
			dashAttack.OnDashBlockedEvent += DashAttack_OnDashBlocked;
			dashAttack.OnDashFinishedEvent += DashAttack_OnDashFinished;
			dashAttack.unblockable = unblockable;
			ama.Amanecidas.Audio.PlaySwordDash_AUDIO();
			dashAttack.Dash(ama.transform, dir, distance, 0f, updateHit: true);
			while (!finished)
			{
				yield return null;
			}
			FinishAction();
		}

		private void DashAttack_OnDashFinished()
		{
			finished = true;
		}

		private void DashAttack_OnDashBlocked(BossDashAttack obj)
		{
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.ShakeWave();
			parried = true;
			finished = true;
		}
	}

	public class MultiFrontalDash_EnemyAction : EnemyAction
	{
		private int numDashes;

		private float anticipationSeconds;

		private KeepDistanceFromTPOUsingAgent_EnemyAction ACT_KEEPDISTANCE = new KeepDistanceFromTPOUsingAgent_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE = new MoveToPointUsingAgent_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_EASE = new MoveEasing_EnemyAction();

		private Dash_EnemyAction ACT_DASH = new Dash_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private LungeAnimation_EnemyAction ACT_LUNGE = new LungeAnimation_EnemyAction();

		private LookAtPenitent_EnemyAction ACT_LOOK = new LookAtPenitent_EnemyAction();

		private TiredPeriod_EnemyAction ACT_TIRED = new TiredPeriod_EnemyAction();

		private StompAttack_EnemyAction ACT_STOMP = new StompAttack_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_KEEPDISTANCE.StopAction();
			ACT_MOVE.StopAction();
			ACT_MOVE_EASE.StopAction();
			ACT_DASH.StopAction();
			ACT_WAIT.StopAction();
			ACT_LUNGE.StopAction();
			ACT_LOOK.StopAction();
			ACT_TIRED.StopAction();
			ACT_STOMP.StopAction();
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, int numDashes, float anticipationSeconds)
		{
			this.numDashes = numDashes;
			this.anticipationSeconds = anticipationSeconds;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			Rect battleBounds = ama.battleBounds;
			AmanecidasAnimatorInyector anim = ama.Amanecidas.AnimatorInyector;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			int parryCounter = 0;
			float extraAnticipation = 0.3f;
			ama.PlayAnticipationGrunt(AMANECIDA_GRUNTS.MULTI_FRONTAL_GRUNT);
			for (int i = 0; i < numDashes; i++)
			{
				ACT_LOOK.StartAction(owner);
				yield return ACT_LOOK.waitForCompletion;
				Vector2 target = ama.GetPointBelowPenitent(stopOnOneWayDowns: true);
				Vector2 dashDir2 = (target - (Vector2)owner.transform.position).normalized;
				dashDir2.x += ama.GetDirFromOrientation();
				dashDir2.y = 0f;
				dashDir2 = dashDir2.normalized;
				ACT_MOVE_EASE.StartAction(owner, (Vector2)owner.transform.position - dashDir2 * 1.5f, extraAnticipation + anticipationSeconds * 0.4f, Ease.OutQuad);
				ACT_LUNGE.StartAction(owner, extraAnticipation + anticipationSeconds * 0.4f, 0f, 0.15f, isHorizontalCharge: true);
				ama.Amanecidas.Audio.PlaySwordDashPreattack_AUDIO();
				ACT_WAIT.StartAction(owner, extraAnticipation + anticipationSeconds);
				yield return ACT_WAIT.waitForCompletion;
				extraAnticipation = 0f;
				ama.throwbackExtraTime = 0f;
				ACT_DASH.StartAction(owner, dashDir2, ama.lanceDashAttack, 20f);
				yield return ACT_DASH.waitForCompletion;
				if (ACT_DASH.parried)
				{
					parryCounter++;
					if (parryCounter >= numDashes)
					{
						ama.InstantBreakShield();
					}
				}
				float secondstoBlinkOut = 0.6f;
				ACT_WAIT.StartAction(owner, secondstoBlinkOut);
				yield return ACT_WAIT.waitForCompletion;
				Vector2 newPos = ama.transform.position - Vector3.right * 3f * ama.GetDirFromOrientation();
				newPos.x = Mathf.Clamp(newPos.x, ama.battleBounds.xMin, ama.battleBounds.xMax);
				ACT_STOMP.StartAction(owner, newPos, doBlinkBeforeJump: false, doStompDamage: true, usePillars: false, bounceAfterJump: false);
				yield return ACT_STOMP.waitForCompletion;
				ACT_WAIT.StartAction(owner, ama.throwbackExtraTime);
				yield return ACT_WAIT.waitForCompletion;
				if (ama.IsPenitentInTop())
				{
					break;
				}
			}
			anim.SetBlink(value: false);
			ACT_WAIT.StartAction(owner, 0.4f);
			yield return ACT_WAIT.waitForCompletion;
			if (parryCounter >= numDashes)
			{
				ACT_TIRED.StartAction(owner, 3f, interruptable: true);
				yield return ACT_TIRED.waitForCompletion;
			}
			FinishAction();
		}
	}

	public class HorizontalBlinkDashes_EnemyAction : EnemyAction
	{
		private int numDashes;

		private float delay;

		private bool startDashesAwayFromPenitent;

		private KeepDistanceFromTPOUsingAgent_EnemyAction ACT_KEEPDISTANCE = new KeepDistanceFromTPOUsingAgent_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private Dash_EnemyAction ACT_DASH = new Dash_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private ShootFrozenLances_EnemyAction ACT_LANCES = new ShootFrozenLances_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_KEEPDISTANCE.StopAction();
			ACT_BLINK.StopAction();
			ACT_DASH.StopAction();
			ACT_WAIT.StopAction();
			ACT_LANCES.StopAction();
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, int numDashes, float delay, bool startDashesAwayFromPenitent)
		{
			this.numDashes = numDashes;
			this.delay = delay;
			this.startDashesAwayFromPenitent = startDashesAwayFromPenitent;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			Rect battleBounds = ama.battleBounds;
			AmanecidasAnimatorInyector anim = ama.Amanecidas.AnimatorInyector;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			float xOrigin = 0f;
			float xEnd = 0f;
			bool startFromTheRight = false;
			for (int i = 0; i < numDashes; i++)
			{
				startFromTheRight = ((i != 0) ? (!startFromTheRight) : ((startDashesAwayFromPenitent && p.GetPosition().x < battleBounds.center.x) || (!startDashesAwayFromPenitent && p.GetPosition().x > battleBounds.center.x)));
				float xOffset = -0.5f;
				if (startFromTheRight)
				{
					xOrigin = battleBounds.xMax;
					xEnd = battleBounds.xMin - xOffset;
				}
				else
				{
					xOrigin = battleBounds.xMin;
					xEnd = battleBounds.xMax + xOffset;
				}
				float y = ((i % 2 == 0) ? (battleBounds.yMin - 1f) : (battleBounds.yMin + 3f));
				Vector2 originPoint = new Vector2(xOrigin, y);
				Vector2 endPoint = new Vector2(xEnd, originPoint.y);
				Vector2 dir = endPoint - originPoint;
				if (i == 0)
				{
					Vector3 penitentPos = Core.Logic.Penitent.transform.position;
					Vector2 originLances = new Vector2(penitentPos.x, battleBounds.yMax + 2f);
					Vector2 endLances = new Vector2(penitentPos.x, battleBounds.yMax + 2f);
					int numLances = 3;
					originLances.x += (float)numLances * 0.55f * (float)ama.GetDirFromOrientation();
					endLances.x -= (float)numLances * 0.55f * (float)ama.GetDirFromOrientation();
					if (ama.IsPenitentInTop())
					{
						originLances.y += 2f;
						endLances.y += 2f;
					}
					ACT_LANCES.StartAction(owner, numLances, originLances, endLances, originPoint, ama.SetFrozenLance, ama.ActivateFrozenLances, 0.1f, 0.2f, 0f);
					yield return ACT_LANCES.waitForCompletion;
					ACT_WAIT.StartAction(owner, 1.5f);
					yield return ACT_WAIT.waitForCompletion;
				}
				else
				{
					ACT_BLINK.StartAction(owner, originPoint, 0.15f, reappear: false, lookAtPenitent: true);
					yield return ACT_BLINK.waitForCompletion;
				}
				ama.LookAtDirUsingOrientation((!startFromTheRight) ? Vector2.right : Vector2.left);
				ama.PlayChargeEnergy(1f, useLongRangeParticles: true);
				ACT_WAIT.StartAction(owner, delay);
				anim.PlayChargeAnticipation(isHorizontalCharge: true);
				yield return ACT_WAIT.waitForCompletion;
				anim.SetCharge(v: true);
				ama.lastX = ama.transform.position.x;
				ama.ActivateBeam();
				ama.beamLauncher.transform.right = -dir;
				ama.Amanecidas.Audio.PlaySwordDashPreattack_AUDIO();
				ACT_WAIT.StartAction(owner, 0.3f);
				yield return ACT_WAIT.waitForCompletion;
				float waitAfterDash = 0.4f;
				bool isLastDash = i == numDashes - 1;
				if (isLastDash)
				{
					ama.Amanecidas.AnimatorInyector.SetBlink(value: false);
					ama.Amanecidas.AnimatorInyector.SetStuck(active: true);
					waitAfterDash = 1f;
					ama.lanceDashAttack.checkCollisions = true;
				}
				else
				{
					ama.lanceDashAttack.checkCollisions = false;
				}
				ACT_DASH.StartAction(owner, dir, ama.lanceDashAttack, dir.magnitude, unblockable: true);
				anim.SetCharge(v: false);
				anim.SetLunge(v: true);
				yield return ACT_DASH.waitForCompletion;
				anim.SetLunge(v: false);
				float beamDelay = 0.3f;
				if (isLastDash)
				{
					beamDelay = 0f;
					ama.ClearRotationAndFlip();
					ama.ApplyStuckOffset();
				}
				ama.DeactivateBeam(beamDelay);
				ACT_WAIT.StartAction(owner, waitAfterDash);
				yield return ACT_WAIT.waitForCompletion;
			}
			ACT_WAIT.StartAction(owner, 0.1f);
			yield return ACT_WAIT.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.SetStuck(active: false);
			ACT_WAIT.StartAction(owner, 0.6f);
			yield return ACT_WAIT.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.SetHorizontalCharge(v: false);
			FinishAction();
		}
	}

	public class DiagonalBlinkDashes_EnemyAction : EnemyAction
	{
		private int numDashes;

		private float longAnticipationDelay;

		private float shortAnticipationDelay;

		private float vulnerabilityTime;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private KeepDistanceFromTPOUsingAgent_EnemyAction ACT_KEEPDISTANCE = new KeepDistanceFromTPOUsingAgent_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private Dash_EnemyAction ACT_DASH = new Dash_EnemyAction();

		protected override void DoOnStop()
		{
			base.DoOnStop();
			ACT_WAIT.StopAction();
			ACT_KEEPDISTANCE.StopAction();
			ACT_DASH.StopAction();
			ACT_BLINK.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.ClearRotationAndFlip();
		}

		public EnemyAction StartAction(EnemyBehaviour e, int numDashes, float anticipationDelay, float shortAnticipationDelay, float vulnerabilityTime)
		{
			this.numDashes = numDashes;
			longAnticipationDelay = anticipationDelay;
			this.shortAnticipationDelay = shortAnticipationDelay;
			this.vulnerabilityTime = vulnerabilityTime;
			return StartAction(e);
		}

		private Vector2 GetOriginPoint(bool startFromRight, AmanecidasBehaviour ama, Vector2 distanceToPlayer)
		{
			Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
			Rect battleBounds = ama.battleBounds;
			float value = ((!startFromRight) ? (penitent.GetPosition().x - distanceToPlayer.x) : (penitent.GetPosition().x + distanceToPlayer.x));
			float y = penitent.GetPosition().y + distanceToPlayer.y;
			value = Mathf.Clamp(value, battleBounds.xMin + 4f, battleBounds.xMax - 4f);
			return new Vector2(value, y);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			Rect battleBounds = ama.battleBounds;
			bool startFromRight = p.GetPosition().x < battleBounds.center.x;
			for (int i = 0; i < numDashes; i++)
			{
				ama.Amanecidas.Audio.PlayDashCharge_AUDIO();
				Vector2 distanceToPlayer = new Vector2(5f, 3f);
				Vector2 originPoint2 = Vector2.zero;
				startFromRight = !startFromRight;
				ACT_BLINK.StartAction(owner, originPoint2, 0.01f, reappear: false, lookAtPenitent: true);
				yield return ACT_BLINK.waitForCompletion;
				float secondsBeforeReappear = 0.1f;
				ACT_WAIT.StartAction(owner, secondsBeforeReappear);
				yield return ACT_WAIT.waitForCompletion;
				originPoint2 = GetOriginPoint(startFromRight, ama, distanceToPlayer);
				ama.transform.position = originPoint2;
				ama.LookAtPenitentUsingOrientation();
				ama.Amanecidas.AnimatorInyector.PlayChargeAnticipation(isHorizontalCharge: false);
				bool lastDash = i == numDashes - 1;
				float delayToUse = ((!lastDash) ? shortAnticipationDelay : longAnticipationDelay);
				ACT_KEEPDISTANCE.StartAction(owner, delayToUse, driftHoriontally: true, driftVertically: true, clampHorizontally: true, ama.battleBounds.xMin + 0.5f, ama.battleBounds.xMax - 0.5f);
				yield return ACT_KEEPDISTANCE.waitForCompletion;
				float easingSeconds = 0.4f;
				Vector2 dir = (float)ama.GetDirFromOrientation() * distanceToPlayer.x * Vector2.right + Vector2.down * distanceToPlayer.y;
				ama.transform.DOMove((Vector2)ama.transform.position - dir.normalized, easingSeconds).SetEase(Ease.OutQuad);
				if (lastDash)
				{
					ama.Amanecidas.Audio.StopDashCharge_AUDIO();
					ama.PlayChargeEnergy();
					easingSeconds = 0.5f;
					ama.Amanecidas.Audio.PlayBeamDashPreattack_AUDIO();
				}
				else
				{
					ama.Amanecidas.Audio.PlaySwordDashPreattack_AUDIO();
				}
				ACT_WAIT.StartAction(owner, easingSeconds);
				yield return ACT_WAIT.waitForCompletion;
				ama.LookAtDirUsingOrientation(dir);
				ama.AimMeleeDirection(dir, ama.lanceRotationDiference);
				ama.Amanecidas.AnimatorInyector.SetCharge(v: true);
				ama.ActivateBeam();
				ama.beamLauncher.transform.right = -dir;
				if (lastDash)
				{
					ama.lanceDashAttack.checkCollisions = true;
					ama.Amanecidas.AnimatorInyector.SetBlink(value: false);
					ama.Amanecidas.AnimatorInyector.SetStuck(active: true);
				}
				else
				{
					ama.Amanecidas.Audio.StopDashCharge_AUDIO();
					ama.lanceDashAttack.checkCollisions = false;
				}
				ama.SmallDistortion();
				ACT_DASH.StartAction(owner, dir, ama.lanceDashAttack, 15f, lastDash);
				ACT_WAIT.StartAction(owner, 0.2f);
				yield return ACT_WAIT.waitForCompletion;
				ama.Amanecidas.AnimatorInyector.SetCharge(v: false);
				yield return ACT_DASH.waitForCompletion;
				if (lastDash)
				{
					ama.ApplyStuckOffset();
					if (ama.HasSolidFloorBelow())
					{
						ama.SpikeWave(ama.transform.position);
					}
				}
				ama.Amanecidas.AnimatorInyector.SetLunge(v: false);
				ama.DeactivateBeam();
				float waitTime = ((!lastDash) ? 0.1f : vulnerabilityTime);
				if (!ama.HasSolidFloorBelow())
				{
					waitTime = 0.1f;
				}
				ama.ClearRotationAndFlip();
				ACT_WAIT.StartAction(owner, waitTime);
				yield return ACT_WAIT.waitForCompletion;
			}
			ama.Amanecidas.AnimatorInyector.SetStuck(active: false);
			float stuckToIdleTime = 0.4f;
			ACT_WAIT.StartAction(owner, stuckToIdleTime);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class BlinkAndDashToPenitent_EnemyAction : EnemyAction
	{
		private Vector2 dir;

		private Vector2 point;

		private BossDashAttack dashAttack;

		private float distance;

		private bool skipReposition;

		private bool endBlinkOut;

		private float anticipation;

		private bool showDashTrail;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private Dash_EnemyAction ACT_DASH = new Dash_EnemyAction();

		private ShowDashAnticipationTrail_EnemyAction ACT_TRAIL = new ShowDashAnticipationTrail_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_DASH.StopAction();
			ACT_BLINK.StopAction();
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 point, BossDashAttack dashAttack, float anticipation, bool skipReposition = false, bool endBlinkOut = false, bool showDashAim = false, float distance = 15f)
		{
			this.point = point;
			this.dashAttack = dashAttack;
			this.endBlinkOut = endBlinkOut;
			this.skipReposition = skipReposition;
			showDashTrail = showDashAim;
			this.anticipation = anticipation;
			this.distance = distance;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			if (!skipReposition)
			{
				ACT_BLINK.StartAction(owner, point, 0.4f, reappear: false, lookAtPenitent: true);
				yield return ACT_BLINK.waitForCompletion;
			}
			ama.SetGhostTrail(active: false);
			Vector2 targetPos = Core.Logic.Penitent.transform.position;
			Vector2 curPos = owner.transform.position;
			Vector2 dir = (targetPos - curPos).normalized;
			Vector2 dashPoint = targetPos + dir * 5f;
			ama.LookAtPenitentUsingOrientation();
			ama.AimMeleeDirection(difference: ama.GetAngleDifference(), dir: dir);
			ama.Amanecidas.Audio.PlaySwordDashPreattack_AUDIO();
			ama.Amanecidas.AnimatorInyector.PlayChargeAnticipation(isHorizontalCharge: false);
			float anticipationSeconds = anticipation;
			if (showDashTrail)
			{
				Vector2 offset = ama.Amanecidas.AnimatorInyector.GetCurrentUp() * 1.25f;
				ACT_TRAIL.StartAction(owner, (Vector2)ama.transform.position + offset, dashPoint + offset, anticipationSeconds);
				yield return ACT_TRAIL.waitForCompletion;
			}
			else
			{
				ACT_WAIT.StartAction(owner, anticipationSeconds);
				yield return ACT_WAIT.waitForCompletion;
			}
			ama.Amanecidas.AnimatorInyector.SetCharge(v: true);
			ACT_WAIT.StartAction(owner, 0.1f);
			yield return ACT_WAIT.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.SetBlink(endBlinkOut);
			dashAttack.checkCollisions = false;
			ama.Amanecidas.Audio.PlaySwordDash_AUDIO();
			ACT_DASH.StartAction(owner, dashPoint - (Vector2)owner.transform.position, dashAttack, distance);
			ACT_WAIT.StartAction(owner, 0.2f);
			yield return ACT_WAIT.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.SetCharge(v: false);
			ACT_WAIT.StartAction(owner, 0.2f);
			yield return ACT_WAIT.waitForCompletion;
			ama.ClearRotationAndFlip();
			ama.Amanecidas.SetOrientation((!(dir.x > 0f)) ? EntityOrientation.Left : EntityOrientation.Right);
			ama.Amanecidas.AnimatorInyector.SetLunge(v: false);
			ACT_WAIT.StartAction(owner, 0.15f);
			yield return ACT_WAIT.waitForCompletion;
			ama.SetGhostTrail(active: true);
			yield return ACT_DASH.waitForCompletion;
			ACT_WAIT.StartAction(owner, 0.25f);
			yield return ACT_WAIT.waitForCompletion;
			if (!endBlinkOut)
			{
				yield return new WaitUntilIdle(ama);
				ama.ClearRotationAndFlip();
			}
			FinishAction();
		}
	}

	private class WaitUntilIdle : WaitUntilAnimationState
	{
		public WaitUntilIdle(AmanecidasBehaviour ama, float timeout = 5f)
			: base(ama, "IDLE", timeout)
		{
		}
	}

	private class WaitUntilAnimationState : CustomYieldInstruction
	{
		private AmanecidasBehaviour ama;

		private string state;

		private float timeout;

		private float timeElapsed;

		public override bool keepWaiting
		{
			get
			{
				bool flag = !ama.Amanecidas.AnimatorInyector.bodyAnimator.GetCurrentAnimatorStateInfo(0).IsName(state);
				bool flag2 = timeElapsed >= timeout;
				timeElapsed += Time.deltaTime;
				return flag || flag2;
			}
		}

		public WaitUntilAnimationState(AmanecidasBehaviour ama, string state, float timeout)
		{
			this.ama = ama;
			this.state = state;
			this.timeout = timeout;
			timeElapsed = 0f;
		}
	}

	private class WaitUntilNotTurning : CustomYieldInstruction
	{
		private AmanecidasBehaviour ama;

		public override bool keepWaiting => ama.Amanecidas.AnimatorInyector.IsTurning();

		public WaitUntilNotTurning(AmanecidasBehaviour ama)
		{
			this.ama = ama;
		}
	}

	public class LookAtPenitent_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector anim = ama.Amanecidas.AnimatorInyector;
			int dirFromOrientation = ama.GetDirFromOrientation();
			Vector2 dirToPenitent = ama.GetDirToPenitent(ama.transform.position);
			ama.LookAtPenitent();
			if (Mathf.Sign(dirFromOrientation) != Mathf.Sign(dirToPenitent.x))
			{
				Debug.Log(string.Format("Turn needed. Orientation: {2} dirFromOrientation:{0} . dirToPenitent.x:{1} ", dirFromOrientation, dirToPenitent.x, ama.Amanecidas.Status.Orientation));
				Debug.DrawRay(owner.transform.position, dirToPenitent * 5f, Color.green, 5f);
				ACT_WAIT.StartAction(owner, 0.2f);
				yield return ACT_WAIT.waitForCompletion;
				yield return new WaitUntilIdle(ama);
			}
			else
			{
				Debug.Log(string.Format("Turn NOT needed. Orientation: {2} . dirFromOrientation:{0} . dirToPenitent.x:{1} ", dirFromOrientation, dirToPenitent.x, ama.Amanecidas.Status.Orientation));
				Debug.DrawRay(owner.transform.position, dirToPenitent * 5f, Color.red, 5f);
			}
			FinishAction();
		}
	}

	public class MeleeAttackProjectile_EnemyAction : EnemyAction
	{
		private float anticipationTime;

		private BossStraightProjectileAttack projectileAttack;

		private Vector2 originPoint;

		private int n;

		private Action playSfxAction;

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_AGENT = new MoveToPointUsingAgent_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private LookAtPenitent_EnemyAction ACT_LOOK = new LookAtPenitent_EnemyAction();

		private MeleeAttack_EnemyAction ACT_MELEE = new MeleeAttack_EnemyAction();

		private EnergyChargePeriod_EnemyAction ACT_CHARGE = new EnergyChargePeriod_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 originPoint, float anticipationTime, Action playSfxAction, BossStraightProjectileAttack projectileAttack, int number = 1)
		{
			this.originPoint = originPoint;
			this.anticipationTime = anticipationTime;
			this.projectileAttack = projectileAttack;
			n = number;
			this.playSfxAction = playSfxAction;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_MOVE_AGENT.StopAction();
			ACT_MOVE.StopAction();
			ACT_WAIT.StopAction();
			ACT_LOOK.StopAction();
			ACT_MELEE.StopAction();
			ACT_CHARGE.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			ACT_MOVE_AGENT.StartAction(owner, ama.agent, originPoint);
			yield return ACT_MOVE_AGENT.waitForCompletion;
			ACT_LOOK.StartAction(owner);
			yield return ACT_LOOK.waitForCompletion;
			ACT_CHARGE.StartAction(owner, anticipationTime * 0.5f, playSfx: true);
			ama.Amanecidas.Audio.PlayHorizontalPreattack_AUDIO();
			ama.throwbackExtraTime = 0f;
			for (int i = 0; i < n; i++)
			{
				float actualAnticipation = anticipationTime;
				if (i > 0)
				{
					if (ama.throwbackExtraTime > 0f)
					{
						ACT_WAIT.StartAction(owner, ama.throwbackExtraTime);
						yield return ACT_WAIT.waitForCompletion;
					}
					actualAnticipation = 0.7f;
				}
				ACT_MELEE.StartAction(owner, actualAnticipation, 0f, 0.5f);
				yield return ACT_MELEE.waitForCallback;
				playSfxAction();
				int right = ama.GetDirFromOrientation();
				Vector2 d = Vector2.right * right;
				projectileAttack.Shoot(d);
				yield return ACT_MELEE.waitForCompletion;
			}
			ACT_WAIT.StartAction(owner, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class MeleeAttack_EnemyAction : EnemyAction
	{
		private float anticipationTime;

		private float advanceDistance;

		private float afterAttackSeconds;

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private LookAtPenitent_EnemyAction ACT_LOOK = new LookAtPenitent_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, float anticipationTime, float advanceDistance, float afterAttackSeconds)
		{
			this.anticipationTime = anticipationTime;
			this.advanceDistance = advanceDistance;
			this.afterAttackSeconds = afterAttackSeconds;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_MOVE.StopAction();
			ACT_WAIT.StopAction();
			ACT_LOOK.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			Vector2 target2 = p.GetPosition();
			Vector2 dir2 = o.GetDirToPenitent(owner.transform.position);
			target2 -= Vector2.right * Mathf.Sign(dir2.x) * advanceDistance;
			ACT_LOOK.StartAction(owner);
			yield return ACT_LOOK.waitForCompletion;
			o.LookAtPenitent(instant: true);
			o.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: true);
			o.Amanecidas.AnimatorInyector.PlayMeleeAttack();
			ACT_WAIT.StartAction(owner, anticipationTime);
			yield return ACT_WAIT.waitForCompletion;
			o.LookAtPenitentUsingOrientation();
			dir2 = o.GetDirToPenitent(o.transform.position);
			float maxDashDistance = 5f;
			Vector2 dashVector = dir2.normalized * maxDashDistance;
			target2 = (Vector2)o.transform.position + dashVector;
			o.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
			o.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
			float secondsBeforeSlash = 0.15f;
			ACT_WAIT.StartAction(owner, secondsBeforeSlash);
			yield return ACT_WAIT.waitForCompletion;
			Callback();
			if (advanceDistance > 0f)
			{
				float moveTime = 0.3f;
				ACT_MOVE.StartAction(owner, target2, moveTime, Ease.OutQuad);
				yield return ACT_MOVE.waitForCompletion;
			}
			ACT_WAIT.StartAction(owner, afterAttackSeconds);
			yield return ACT_WAIT.waitForCompletion;
			o.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			ACT_WAIT.StartAction(owner, 0.15f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class MeleeAttackTowardsPenitent_EnemyAction : EnemyAction
	{
		private float anticipationTime;

		private bool blinkOut;

		private Action launchAudioMethod;

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_AGENT = new MoveToPointUsingAgent_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private LookAtPenitent_EnemyAction ACT_LOOK = new LookAtPenitent_EnemyAction();

		private LaunchMethod_EnemyAction ACT_METHOD = new LaunchMethod_EnemyAction();

		private ShowSimpleVFX_EnemyAction ACT_DUST_VFX = new ShowSimpleVFX_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, float anticipationTime, Action launchAudioMethod, bool blinkOut = false)
		{
			this.anticipationTime = anticipationTime;
			this.blinkOut = blinkOut;
			this.launchAudioMethod = launchAudioMethod;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			if (o.currentWeapon == AmanecidasAnimatorInyector.AMANECIDA_WEAPON.AXE)
			{
				o.currentMeleeAttack = o.meleeAxeAttack;
			}
			else if (o.currentWeapon == AmanecidasAnimatorInyector.AMANECIDA_WEAPON.SWORD)
			{
				o.currentMeleeAttack = o.meleeFalcataAttack;
			}
			float distance = 4f;
			float minDashDistance = 1f;
			Vector2 dir2 = o.GetDirToPenitent(owner.transform.position);
			Vector2 target2 = o.GetPointBelowPenitent(stopOnOneWayDowns: true) - Vector2.right * Mathf.Sign(dir2.x) * distance;
			if (dir2.magnitude <= minDashDistance)
			{
				target2 = o.GetPointBelowPenitent(stopOnOneWayDowns: true) - Vector2.right * Mathf.Sign(dir2.x) * (distance / 2f);
			}
			ACT_MOVE_AGENT.StartAction(owner, o.agent, target2);
			yield return ACT_MOVE_AGENT.waitForCallback;
			ACT_LOOK.StartAction(owner);
			yield return ACT_LOOK.waitForCompletion;
			o.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: true);
			o.Amanecidas.AnimatorInyector.PlayMeleeAttack();
			ACT_WAIT.StartAction(owner, anticipationTime);
			yield return ACT_WAIT.waitForCompletion;
			dir2 = o.GetDirToPenitent(o.transform.position);
			dir2.y = 0f;
			float finalDistance = Mathf.Clamp(Mathf.Abs(dir2.x) + 1f, minDashDistance, 10f);
			Vector2 attackDir = o.GetDirFromOrientation() * Vector2.right;
			Vector2 dashVector = attackDir * finalDistance;
			target2 = (Vector2)o.transform.position + dashVector;
			if (o.currentWeapon == AmanecidasAnimatorInyector.AMANECIDA_WEAPON.AXE)
			{
				target2.y = o.transform.position.y;
			}
			target2.x = Mathf.Clamp(target2.x, o.battleBounds.xMin, o.battleBounds.xMax);
			o.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
			float moveTime = 0.8f;
			ACT_MOVE.StartAction(owner, target2, moveTime, Ease.InBack, null, _timeScaled: true, null, _tweenOnX: true, _tweenOnY: true, 2f);
			ACT_WAIT.StartAction(owner, moveTime * 0.1f);
			yield return ACT_WAIT.waitForCompletion;
			o.Amanecidas.Audio.PlaySwordPreattack_AUDIO();
			ACT_WAIT.StartAction(owner, moveTime * 0.35f);
			yield return ACT_WAIT.waitForCompletion;
			o.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
			if (blinkOut)
			{
				o.Amanecidas.AnimatorInyector.SetBlink(blinkOut);
			}
			ACT_METHOD.StartAction(owner, launchAudioMethod);
			ACT_WAIT.StartAction(owner, moveTime * 0.3f);
			yield return ACT_WAIT.waitForCompletion;
			o.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			yield return ACT_MOVE.waitForCompletion;
			Vector2 driftTarget = target2;
			if (dashVector.x > 0f)
			{
				driftTarget.x += 1f;
			}
			else
			{
				driftTarget.x -= 1f;
			}
			ACT_MOVE.StartAction(owner, driftTarget, 0.2f, Ease.OutExpo);
			if (o.currentWeapon == AmanecidasAnimatorInyector.AMANECIDA_WEAPON.AXE)
			{
				ACT_DUST_VFX.StartAction(owner, target2 + Vector2.down * 0.2f, o.dustVFX, 0.3f);
				ACT_WAIT.StartAction(owner, 0.1f);
				yield return ACT_WAIT.waitForCompletion;
				ACT_DUST_VFX.StartAction(owner, driftTarget + Vector2.down * 0.2f, o.dustVFX, 0.3f);
			}
			yield return ACT_MOVE.waitForCompletion;
			FinishAction();
		}

		protected override void DoOnStop()
		{
			ACT_MOVE_AGENT.StopAction();
			ACT_MOVE.StopAction();
			ACT_WAIT.StopAction();
			ACT_LOOK.StopAction();
			ACT_METHOD.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			base.DoOnStop();
		}
	}

	public class GhostProjectile_EnemyAction : EnemyAction
	{
		private float anticipationSeconds;

		private Action<Vector2, Vector2> shootProjectileMethod;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_AGENT = new MoveToPointUsingAgent_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private LaunchMethodWithTwoVectors_EnemyAction ACT_METHODLAUNCH_VECTORS = new LaunchMethodWithTwoVectors_EnemyAction();

		private LungeAnimation_EnemyAction ACT_LUNGE = new LungeAnimation_EnemyAction();

		private EaseAnticipation_EnemyAction ACT_PUNCH = new EaseAnticipation_EnemyAction();

		private InterruptablePeriod_EnemyAction ACT_INTERRUPTABLE = new InterruptablePeriod_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, Action<Vector2, Vector2> shootProjectileMethod, float anticipationSeconds)
		{
			this.shootProjectileMethod = shootProjectileMethod;
			this.anticipationSeconds = anticipationSeconds;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_INTERRUPTABLE.StopAction();
			ACT_WAIT.StopAction();
			ACT_BLINK.StopAction();
			ACT_METHODLAUNCH_VECTORS.StopAction();
			ACT_LUNGE.StopAction();
			ACT_PUNCH.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			yield return new WaitUntil(() => !Core.Logic.Penitent.IsJumping);
			Vector2 targetPos = o.battleBounds.center + Vector2.up * 2f;
			ACT_MOVE_AGENT.StartAction(o, o.agent, targetPos);
			yield return ACT_MOVE_AGENT.waitForCompletion;
			int j = 5;
			for (int i = 0; i < j; i++)
			{
				Vector2 targetDir = o.GetDirToPenitent(o.transform.position);
				ACT_LUNGE.StartAction(owner, anticipationSeconds * 0.4f, 0f, 0.1f, isHorizontalCharge: true);
				ACT_PUNCH.StartAction(owner, -targetDir.normalized, 1f, anticipationSeconds);
				o.Amanecidas.Audio.PlaySwordDashPreattack_AUDIO();
				yield return ACT_PUNCH.waitForCompletion;
				o.SetGhostTrail(active: false);
				o.Amanecidas.Audio.PlaySwordDash_AUDIO();
				ACT_METHODLAUNCH_VECTORS.StartAction(owner, shootProjectileMethod, targetPos, targetDir);
				yield return ACT_METHODLAUNCH_VECTORS.waitForCompletion;
				yield return ACT_LUNGE.waitForCompletion;
				o.ClearRotationAndFlip();
				yield return new WaitUntilIdle(o);
			}
			o.SetGhostTrail(active: true);
			FinishAction();
		}
	}

	public class EaseAnticipation_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private Vector2 targetDir;

		private float distance;

		private float easeSeconds;

		public EnemyAction StartAction(EnemyBehaviour owner, Vector2 dir, float distance, float easeSeconds)
		{
			targetDir = dir;
			this.distance = distance;
			this.easeSeconds = easeSeconds;
			return StartAction(owner);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			owner.transform.DOKill();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			Sequence s = DOTween.Sequence();
			s.Append(owner.transform.DOMove((Vector2)owner.transform.position + targetDir * distance, 0.75f * easeSeconds).SetEase(Ease.OutCirc));
			s.Append(owner.transform.DOMove((Vector2)owner.transform.position, 0.25f * easeSeconds).SetEase(Ease.InCirc));
			s.Play();
			ACT_WAIT.StartAction(owner, easeSeconds);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class QuickLunge_EnemyAction : EnemyAction
	{
		private LungeAnimation_EnemyAction ACT_LUNGE = new LungeAnimation_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_AGENT = new MoveToPointUsingAgent_EnemyAction();

		private int number = 3;

		public EnemyAction StartAction(EnemyBehaviour e, int number)
		{
			this.number = number;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_MOVE_AGENT.StopAction();
			ACT_LUNGE.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			Vector2 target = p.GetPosition();
			target += ((!(owner.transform.position.x > target.x)) ? (Vector2.left * 0.5f) : (Vector2.right * 0.5f));
			o.LookAtPenitent();
			ACT_MOVE_AGENT.StartAction(owner, owner.GetComponent<AutonomousAgent>(), target);
			yield return ACT_MOVE_AGENT.waitForCompletion;
			for (int i = 0; i < number; i++)
			{
				Vector2 dir = o.GetDirFromOrientation() * Vector2.right;
				float baseDistance = 1.5f;
				float finalDistance = 3f;
				if (i == number - 1)
				{
					dir *= finalDistance;
				}
				else
				{
					dir *= baseDistance;
				}
				float delay = 0.8f;
				Sequence s = DOTween.Sequence();
				s.Append(owner.transform.DOMove((Vector2)owner.transform.position - dir * 0.25f, 0.7f).SetEase(Ease.OutCirc));
				s.Append(owner.transform.DOMove((Vector2)owner.transform.position + dir * 0.75f, 0.2f).SetEase(Ease.InCirc));
				s.SetDelay(delay);
				s.Play();
				o.Amanecidas.Audio.PlaySwordDash_AUDIO();
				ACT_LUNGE.StartAction(owner, 0.5f, 0f, 0.2f, isHorizontalCharge: true);
				yield return ACT_LUNGE.waitForCompletion;
			}
			FinishAction();
		}
	}

	public class LungeAnimation_EnemyAction : EnemyAction
	{
		private bool isHorizontalCharge;

		private float anticipationSeconds;

		private float chargeSeconds;

		private float lungeSeconds;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		public void StartAction(EnemyBehaviour e, float anticipationSeconds, float chargeSeconds, float lungeSeconds, bool isHorizontalCharge)
		{
			this.anticipationSeconds = anticipationSeconds;
			this.chargeSeconds = chargeSeconds;
			this.lungeSeconds = lungeSeconds;
			this.isHorizontalCharge = isHorizontalCharge;
			StartAction(e);
		}

		protected override void DoOnStop()
		{
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			float baseAnticipationSeconds = 0.13f;
			float baseChargeSeconds = 0.15f;
			float baseLungeSeconds = 0.12f;
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector anim = ama.Amanecidas.AnimatorInyector;
			anim.PlayChargeAnticipation(isHorizontalCharge);
			ACT_WAIT.StartAction(owner, baseAnticipationSeconds + anticipationSeconds);
			yield return ACT_WAIT.waitForCompletion;
			anim.SetCharge(v: true);
			ACT_WAIT.StartAction(owner, baseChargeSeconds + chargeSeconds);
			yield return ACT_WAIT.waitForCompletion;
			anim.SetCharge(v: false);
			anim.SetLunge(v: true);
			ACT_WAIT.StartAction(owner, baseLungeSeconds + lungeSeconds);
			yield return ACT_WAIT.waitForCompletion;
			anim.SetLunge(v: false);
			if (isHorizontalCharge)
			{
				ama.Amanecidas.AnimatorInyector.SetHorizontalCharge(v: false);
			}
			FinishAction();
		}
	}

	public class RechargeShield_EnemyAction : EnemyAction
	{
		private Action activateShieldMethod;

		private Action rechargeShieldMethod;

		private Action anticipationMethod;

		private float rechargeTime;

		private float anticipationTime;

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE = new MoveToPointUsingAgent_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_EASE = new MoveEasing_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private LaunchMethod_EnemyAction ACT_METHOD = new LaunchMethod_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private CallAxe_EnemyAction ACT_CALLAXE1 = new CallAxe_EnemyAction();

		private CallAxe_EnemyAction ACT_CALLAXE2 = new CallAxe_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, float _rechargeTime, float _anticipationTime, Action _rechargeShieldMethod, Action _anticipationMethod, Action _activateShieldMethod)
		{
			activateShieldMethod = _activateShieldMethod;
			rechargeShieldMethod = _rechargeShieldMethod;
			anticipationMethod = _anticipationMethod;
			rechargeTime = _rechargeTime;
			anticipationTime = _anticipationTime;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_MOVE.StopAction();
			ACT_MOVE_EASE.StopAction();
			ACT_WAIT.StopAction();
			ACT_METHOD.StopAction();
			ACT_BLINK.StopAction();
			ACT_CALLAXE1.StopAction();
			ACT_CALLAXE2.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetRecharging(active: false);
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetShockwaveAnticipation(v: false);
			amanecidasBehaviour.ShowCurrentWeapon(show: true);
			base.DoOnStop();
		}

		private bool IsInValidRechargePoint(Vector2 p)
		{
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			return amanecidasBehaviour.battleBounds.Contains(p);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			if (!IsInValidRechargePoint(ama.transform.position))
			{
				ACT_BLINK.StartAction(owner, ama.battleBounds.center, 0.1f);
				yield return ACT_BLINK.waitForCompletion;
			}
			if (ama.currentWeapon == AmanecidasAnimatorInyector.AMANECIDA_WEAPON.LANCE && ama.DoCrystalLancesPlatformsExist())
			{
				ama.DestroyCrystalLancesPlatforms(0.75f, 2.5f);
				ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
				ama.Amanecidas.AnimatorInyector.PlayStompAttack(doStompDamage: true);
				ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: false);
				ACT_WAIT.StartAction(ama, 1f);
				yield return ACT_WAIT.waitForCompletion;
				ACT_WAIT.StartAction(ama, 1.5f);
				ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
				yield return new WaitUntilIdle(ama);
				ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
				yield return ACT_WAIT.waitForCompletion;
			}
			Vector2 p = ama.GetPointBelowMe(stopOnOneWayDowns: true);
			float moveTime = Vector2.Distance(owner.transform.position, p) * 0.25f + 0.5f;
			ACT_MOVE_EASE.StartAction(owner, p, moveTime, Ease.InOutQuad);
			yield return ACT_MOVE_EASE.waitForCompletion;
			if (ama.currentWeapon == AmanecidasAnimatorInyector.AMANECIDA_WEAPON.AXE && !ama.IsWieldingAxe())
			{
				ACT_CALLAXE1.StartAction(owner, owner.transform.position + Vector3.up * 1.75f, ama.axes[0], 0.1f, 0.3f);
				ACT_CALLAXE2.StartAction(owner, owner.transform.position + Vector3.up * 1.75f, ama.axes[1], 0.1f, 0.3f);
				yield return ACT_CALLAXE1.waitForCompletion;
				yield return ACT_CALLAXE2.waitForCompletion;
			}
			ACT_METHOD.StartAction(owner, rechargeShieldMethod);
			ACT_WAIT.StartAction(owner, rechargeTime);
			yield return ACT_WAIT.waitForCompletion;
			ACT_METHOD.StartAction(owner, anticipationMethod);
			float distance = 1f;
			Vector2 explosionPoint = owner.transform.position + Vector3.up * distance;
			ACT_MOVE_EASE.StartAction(owner, explosionPoint, anticipationTime, Ease.InCubic);
			yield return ACT_MOVE_EASE.waitForCompletion;
			ACT_METHOD.StartAction(owner, activateShieldMethod);
			ACT_WAIT.StartAction(owner, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			ACT_MOVE_EASE.StartAction(owner, explosionPoint - Vector2.up * distance, anticipationTime * 0.1f, Ease.InOutCubic);
			yield return ACT_MOVE_EASE.waitForCompletion;
			yield return new WaitUntilIdle(ama);
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			ama.ShowCurrentWeapon(show: true);
			ama.lastShieldRechargeWasInterrupted = false;
			FinishAction();
		}
	}

	public class ShootRicochetArrow_EnemyAction : EnemyAction
	{
		private int numBounces;

		private Action<Vector2, Vector2> showArrowTrailMethod;

		private Action<Vector2, Vector2> shootArrowMethod;

		private float waitTime;

		private LayerMask mask;

		private bool isPartOfCombo;

		private bool launchToTheRight;

		private RaycastHit2D[] results;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private LaunchMethodWithTwoVectors_EnemyAction ACT_METHODLAUNCH_VECTORS = new LaunchMethodWithTwoVectors_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, int numBounces, Action<Vector2, Vector2> showArrowTrailMethod, Action<Vector2, Vector2> shootArrowMethod, float waitTime, LayerMask mask, bool isPartOfCombo = false, bool launchToTheRight = false)
		{
			this.numBounces = numBounces;
			this.showArrowTrailMethod = showArrowTrailMethod;
			this.shootArrowMethod = shootArrowMethod;
			this.waitTime = waitTime;
			this.mask = mask;
			this.isPartOfCombo = isPartOfCombo;
			this.launchToTheRight = launchToTheRight;
			results = new RaycastHit2D[1];
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_BLINK.StopAction();
			ACT_METHODLAUNCH_VECTORS.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector animatorInyector = amanecidasBehaviour.Amanecidas.AnimatorInyector;
			animatorInyector.SetBow(value: false);
			animatorInyector.SetBlink(value: false);
			amanecidasBehaviour.SetGhostTrail(active: true);
			amanecidasBehaviour.LookAtPenitent(instant: true);
			amanecidasBehaviour.ClearRotationAndFlip();
			amanecidasBehaviour.Amanecidas.SetOrientation(amanecidasBehaviour.Amanecidas.Status.Orientation);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			Rect battleBounds = o.battleBounds;
			Vector2[] startPoints = new Vector2[numBounces];
			Vector2[] targetDirections = new Vector2[numBounces];
			Vector2[] targetPoints = new Vector2[numBounces];
			if (isPartOfCombo)
			{
				ref Vector2 reference = ref targetDirections[0];
				reference = Vector2.down + ((!launchToTheRight) ? (Vector2.left * 1.3f) : (Vector2.right * 1.3f));
				ref Vector2 reference2 = ref startPoints[0];
				reference2 = (Vector2)o.transform.position + new Vector2(0.5f, -1.1f);
			}
			else
			{
				Vector2 blinkPoint = new Vector2((!(Core.Logic.Penitent.GetPosition().x > battleBounds.center.x)) ? battleBounds.xMax : battleBounds.xMin, battleBounds.yMin);
				ref Vector2 reference3 = ref targetDirections[0];
				reference3 = Vector2.down + ((!(Core.Logic.Penitent.GetPosition().x > battleBounds.center.x)) ? (Vector2.left * UnityEngine.Random.Range(1f, 3f)) : (Vector2.right * UnityEngine.Random.Range(1f, 3f)));
				ref Vector2 reference4 = ref startPoints[0];
				reference4 = blinkPoint + Vector2.up * 0.75f;
				ACT_BLINK.StartAction(owner, blinkPoint, 0.15f, reappear: true, lookAtPenitent: true);
				yield return ACT_BLINK.waitForCompletion;
			}
			startPoints[0] += Vector2.up;
			AmanecidasAnimatorInyector anim = o.Amanecidas.AnimatorInyector;
			anim.SetBow(value: true);
			float bowReadyTime = 0.3f;
			ACT_WAIT.StartAction(owner, bowReadyTime);
			yield return ACT_WAIT.waitForCompletion;
			o.SetGhostTrail(active: false);
			if (isPartOfCombo)
			{
				o.AimToPointWithBow((Vector2)o.transform.position + Vector2.down);
			}
			else
			{
				o.AimToPointWithBow((Vector2)o.transform.position + targetDirections[0]);
			}
			int bounceCounter = 0;
			for (int j = 0; j < numBounces && ThrowRay(startPoints[j], targetDirections[j]); j++)
			{
				bounceCounter++;
				ref Vector2 reference5 = ref targetPoints[j];
				reference5 = results[0].point;
				GizmoExtensions.DrawDebugCross(targetPoints[j], Color.green, 1f);
				ACT_METHODLAUNCH_VECTORS.StartAction(owner, showArrowTrailMethod, startPoints[j], targetPoints[j]);
				yield return ACT_METHODLAUNCH_VECTORS.waitForCompletion;
				if (j + 1 < numBounces)
				{
					ref Vector2 reference6 = ref targetDirections[j + 1];
					reference6 = CalculateBounceDirection(targetPoints[j] - startPoints[j], results[0]);
					ref Vector2 reference7 = ref startPoints[j + 1];
					reference7 = targetPoints[j] + targetDirections[j + 1] * 0.01f;
				}
			}
			ACT_WAIT.StartAction(owner, waitTime);
			yield return ACT_WAIT.waitForCompletion;
			anim.SetBow(value: false);
			float bowShotTime = 0.15f;
			ACT_WAIT.StartAction(owner, bowShotTime);
			yield return ACT_WAIT.waitForCompletion;
			o.SetGhostTrail(active: true);
			o.LookAtPenitent(instant: true);
			o.ClearRotationAndFlip();
			o.Amanecidas.SetOrientation(o.Amanecidas.Status.Orientation);
			o.Amanecidas.Audio.PlayArrowFireFast_AUDIO();
			for (int i = 0; i < bounceCounter; i++)
			{
				ACT_METHODLAUNCH_VECTORS.StartAction(owner, shootArrowMethod, startPoints[i], targetPoints[i]);
				yield return ACT_METHODLAUNCH_VECTORS.waitForCompletion;
			}
			yield return new WaitUntilIdle(o);
			FinishAction();
		}

		private Vector2 CalculateBounceDirection(Vector2 direction, RaycastHit2D hit)
		{
			return Vector3.Reflect(direction, hit.normal).normalized;
		}

		private bool ThrowRay(Vector2 startPoint, Vector2 direction)
		{
			bool result = false;
			if (Physics2D.RaycastNonAlloc(startPoint, direction, results, 100f, mask) > 0)
			{
				Debug.DrawRay(startPoint, direction.normalized * results[0].distance, Color.red, 1f);
				result = true;
			}
			else
			{
				Debug.DrawRay(startPoint, direction.normalized * 100f, Color.yellow, 1f);
			}
			return result;
		}
	}

	public class ShootLaserArrow_EnemyAction : EnemyAction
	{
		private Action<Vector2, Vector2> showArrowTrailMethod;

		private Action<Vector2, Vector2> shootArrowMethod;

		private float waitTime;

		private LayerMask mask;

		private RaycastHit2D[] results;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private LaunchMethodWithTwoVectors_EnemyAction ACT_METHODLAUNCH_VECTORS = new LaunchMethodWithTwoVectors_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_CHARACTER = new MoveToPointUsingAgent_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, float waitTime, LayerMask mask, Action<Vector2, Vector2> showArrowTrailMethod, Action<Vector2, Vector2> shootArrowMethod)
		{
			this.showArrowTrailMethod = showArrowTrailMethod;
			this.shootArrowMethod = shootArrowMethod;
			this.waitTime = waitTime;
			this.mask = mask;
			results = new RaycastHit2D[1];
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_BLINK.StopAction();
			ACT_METHODLAUNCH_VECTORS.StopAction();
			ACT_MOVE_CHARACTER.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector animatorInyector = amanecidasBehaviour.Amanecidas.AnimatorInyector;
			animatorInyector.SetBow(value: false);
			animatorInyector.SetBlink(value: false);
			amanecidasBehaviour.SetGhostTrail(active: true);
			amanecidasBehaviour.LookAtPenitent(instant: true);
			amanecidasBehaviour.ClearRotationAndFlip();
			amanecidasBehaviour.Amanecidas.SetOrientation(amanecidasBehaviour.Amanecidas.Status.Orientation);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector anim = o.Amanecidas.AnimatorInyector;
			anim.SetBow(value: false);
			anim.SetBlink(value: false);
			yield return new WaitUntilIdle(o, 1f);
			ACT_MOVE_CHARACTER.StartAction(owner, o.agent, o.battleBounds.center + Vector2.up * 1.5f);
			yield return ACT_MOVE_CHARACTER.waitForCompletion;
			Vector2 startPoint = (Vector2)o.transform.position + new Vector2(0f, 1.75f);
			Vector2 targetDirection = o.GetDirToPenitent(startPoint);
			targetDirection.x += UnityEngine.Random.Range(-0.2f, 0.2f);
			if (Core.Logic.Penitent.IsJumping || Core.Logic.Penitent.IsDashing)
			{
				targetDirection.x += ((Core.Logic.Penitent.GetOrientation() != 0) ? (-0.5f) : 0.5f);
			}
			anim.SetBow(value: true);
			float bowReadyTime = 0.3f;
			ACT_WAIT.StartAction(owner, bowReadyTime);
			yield return ACT_WAIT.waitForCompletion;
			o.SetGhostTrail(active: false);
			o.AimToPointWithBow((Vector2)o.transform.position + targetDirection);
			ThrowRay(startPoint, targetDirection);
			Vector2 targetPoint = results[0].point;
			GizmoExtensions.DrawDebugCross(targetPoint, Color.green, 1f);
			ACT_METHODLAUNCH_VECTORS.StartAction(owner, showArrowTrailMethod, startPoint, targetPoint);
			yield return ACT_METHODLAUNCH_VECTORS.waitForCompletion;
			ACT_WAIT.StartAction(owner, waitTime);
			yield return ACT_WAIT.waitForCompletion;
			anim.SetBow(value: false);
			float bowShotTime = 0.15f;
			ACT_WAIT.StartAction(owner, bowShotTime);
			yield return ACT_WAIT.waitForCompletion;
			o.SetGhostTrail(active: true);
			o.LookAtPenitent(instant: true);
			o.ClearRotationAndFlip();
			o.Amanecidas.SetOrientation(o.Amanecidas.Status.Orientation);
			o.Amanecidas.Audio.PlayArrowFireFast_AUDIO();
			ACT_METHODLAUNCH_VECTORS.StartAction(owner, shootArrowMethod, startPoint, targetPoint);
			yield return ACT_METHODLAUNCH_VECTORS.waitForCompletion;
			yield return new WaitUntilIdle(o, 1f);
			FinishAction();
		}

		private bool ThrowRay(Vector2 startPoint, Vector2 direction)
		{
			bool result = false;
			if (Physics2D.RaycastNonAlloc(startPoint, direction, results, 100f, mask) > 0)
			{
				Debug.DrawRay(startPoint, direction.normalized * results[0].distance, Color.red, 1f);
				result = true;
			}
			else
			{
				Debug.DrawRay(startPoint, direction.normalized * 100f, Color.yellow, 1f);
			}
			return result;
		}
	}

	public class ShootMineArrows_EnemyAction : EnemyAction
	{
		private int numBlinks;

		private Vector2 originPoint;

		private Vector2 endPoint;

		private Action shootMineMethod;

		private Action activateMinesMethod;

		private Action<Vector2, Vector2> showArrowTrailMethod;

		private float anticipationtWaitTime;

		private float blinksWaitTime;

		private float afterEndReachedWaitTime;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private LaunchMethod_EnemyAction ACT_METHODLAUNCH = new LaunchMethod_EnemyAction();

		private LaunchMethodWithTwoVectors_EnemyAction ACT_METHODLAUNCH_VECTORS = new LaunchMethodWithTwoVectors_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_CHARACTER = new MoveToPointUsingAgent_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, int numBlinks, Vector2 originPoint, Vector2 endPoint, Action shootMineMethod, Action activateMinesMethod, Action<Vector2, Vector2> showArrowTrailMethod, float anticipationtWaitTime, float blinksWaitTime, float afterEndReachedWaitTime)
		{
			this.numBlinks = numBlinks;
			this.originPoint = originPoint;
			this.endPoint = endPoint;
			this.shootMineMethod = shootMineMethod;
			this.activateMinesMethod = activateMinesMethod;
			this.showArrowTrailMethod = showArrowTrailMethod;
			this.anticipationtWaitTime = anticipationtWaitTime;
			this.blinksWaitTime = blinksWaitTime;
			this.afterEndReachedWaitTime = afterEndReachedWaitTime;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_METHODLAUNCH.StopAction();
			ACT_METHODLAUNCH_VECTORS.StopAction();
			ACT_MOVE_CHARACTER.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector animatorInyector = amanecidasBehaviour.Amanecidas.AnimatorInyector;
			animatorInyector.SetBow(value: false);
			animatorInyector.SetBlink(value: false);
			amanecidasBehaviour.SetGhostTrail(active: true);
			amanecidasBehaviour.LookAtPenitent(instant: true);
			amanecidasBehaviour.ClearRotationAndFlip();
			amanecidasBehaviour.Amanecidas.SetOrientation(amanecidasBehaviour.Amanecidas.Status.Orientation);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector anim = o.Amanecidas.AnimatorInyector;
			for (int i = 0; i < numBlinks; i++)
			{
				Vector2 target = Vector2.Lerp(originPoint, endPoint, (float)i / (float)numBlinks);
				if (i == 0)
				{
					ACT_MOVE_CHARACTER.StartAction(owner, o.agent, target);
					yield return ACT_MOVE_CHARACTER.waitForCompletion;
					anim.SetBow(value: true);
					anim.SetBlink(value: false);
					o.SetGhostTrail(active: false);
					Vector2 dir = ((!(o.transform.position.x > o.battleBounds.center.x)) ? Vector2.right : Vector2.left);
					o.AimToPointWithBow((Vector2)o.transform.position + dir);
					Vector2 trailStartPos = (Vector2)o.transform.position + new Vector2(0f - dir.x, 1.75f);
					Vector2 trailEndPos = trailStartPos + dir * 11f;
					ACT_METHODLAUNCH_VECTORS.StartAction(owner, showArrowTrailMethod, trailStartPos, trailEndPos);
					yield return ACT_METHODLAUNCH_VECTORS.waitForCompletion;
					ACT_WAIT.StartAction(owner, anticipationtWaitTime);
					yield return ACT_WAIT.waitForCompletion;
					anim.SetBow(value: false);
					anim.SetBlink(value: true);
				}
				else
				{
					ACT_MOVE.StartAction(owner, target, 0.2f, Ease.InCubic);
					yield return ACT_MOVE.waitForCompletion;
					anim.PlayBlinkshot();
				}
				ACT_METHODLAUNCH.StartAction(owner, shootMineMethod);
				yield return ACT_METHODLAUNCH.waitForCompletion;
				ACT_WAIT.StartAction(owner, blinksWaitTime);
				yield return ACT_WAIT.waitForCompletion;
			}
			ACT_WAIT.StartAction(owner, afterEndReachedWaitTime);
			yield return ACT_WAIT.waitForCompletion;
			ACT_METHODLAUNCH.StartAction(owner, activateMinesMethod);
			yield return ACT_METHODLAUNCH.waitForCompletion;
			ACT_MOVE.StartAction(owner, o.battleBounds.center, 0.2f, Ease.InCubic);
			anim.SetBlink(value: false);
			o.SetGhostTrail(active: true);
			ACT_WAIT.StartAction(owner, afterEndReachedWaitTime);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class ShootFrozenLances_EnemyAction : EnemyAction
	{
		private int numLances;

		private Vector2 originPoint;

		private Vector2 endPoint;

		private Vector2 targetPosition;

		private Action<Vector2> setLanceMethod;

		private Action activateLancesMethod;

		private float anticipationtWaitTime;

		private float afterEndReachedWaitTime;

		private float timeBetweenLances;

		private bool shouldSpin;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private LaunchMethod_EnemyAction ACT_METHODLAUNCH = new LaunchMethod_EnemyAction();

		private LaunchMethodWithVector_EnemyAction ACT_METHODLAUNCH_VECTOR = new LaunchMethodWithVector_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, int numLances, Vector2 originPoint, Vector2 endPoint, Vector2 targetPosition, Action<Vector2> setLanceMethod, Action activateLancesMethod, float anticipationtWaitTime, float afterEndReachedWaitTime, float timeBetweenLances = 0.1f, bool shouldSpin = false)
		{
			this.numLances = numLances;
			this.originPoint = originPoint;
			this.endPoint = endPoint;
			this.targetPosition = targetPosition;
			this.setLanceMethod = setLanceMethod;
			this.activateLancesMethod = activateLancesMethod;
			this.anticipationtWaitTime = anticipationtWaitTime;
			this.afterEndReachedWaitTime = afterEndReachedWaitTime;
			this.timeBetweenLances = timeBetweenLances;
			this.shouldSpin = shouldSpin;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_BLINK.StopAction();
			ACT_METHODLAUNCH.StopAction();
			ACT_METHODLAUNCH_VECTOR.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector anim = o.Amanecidas.AnimatorInyector;
			if (Vector3.Distance(o.transform.position, targetPosition) > 1f)
			{
				ACT_BLINK.StartAction(owner, targetPosition, anticipationtWaitTime, reappear: true, lookAtPenitent: true);
				yield return ACT_BLINK.waitForCompletion;
			}
			if (shouldSpin)
			{
				o.currentMeleeAttack = o.meleeStompAttack;
				o.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
				o.Amanecidas.AnimatorInyector.PlayStompAttack(doStompDamage: true);
				o.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: false);
			}
			for (int i = 0; i < numLances; i++)
			{
				Vector2 target = Vector2.Lerp(t: (float)i / (float)(numLances - 1), a: originPoint, b: endPoint);
				ACT_METHODLAUNCH_VECTOR.StartAction(owner, setLanceMethod, target);
				yield return ACT_METHODLAUNCH_VECTOR.waitForCompletion;
				ACT_WAIT.StartAction(owner, timeBetweenLances);
				yield return ACT_WAIT.waitForCompletion;
			}
			if (shouldSpin)
			{
				o.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
				yield return new WaitUntilIdle(o);
				o.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			}
			if (afterEndReachedWaitTime > 0f)
			{
				ACT_WAIT.StartAction(owner, afterEndReachedWaitTime);
				yield return ACT_WAIT.waitForCompletion;
			}
			ACT_METHODLAUNCH.StartAction(owner, activateLancesMethod);
			yield return ACT_METHODLAUNCH.waitForCompletion;
			FinishAction();
		}
	}

	public class DoubleShootFrozenLances_EnemyAction : EnemyAction
	{
		private int totalNumLances;

		private Vector2 targetPosition;

		private Vector2 firstOriginPoint;

		private Vector2 firstEndPoint;

		private Vector2 secondOriginPoint;

		private Vector2 secondEndPoint;

		private Action<Vector2> setLanceMethod;

		private Action activateLancesMethod;

		private float anticipationtWaitTime;

		private float afterEndReachedWaitTime;

		private float timeBetweenLances;

		private bool shouldSpin;

		private ShootFrozenLances_EnemyAction ACT_SHOOT_1 = new ShootFrozenLances_EnemyAction();

		private ShootFrozenLances_EnemyAction ACT_SHOOT_2 = new ShootFrozenLances_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, int totalNumLances, Vector2 targetPosition, Vector2 firstOriginPoint, Vector2 firstEndPoint, Vector2 secondOriginPoint, Vector2 secondEndPoint, Action<Vector2> setLanceMethod, Action activateLancesMethod, float anticipationtWaitTime, float afterEndReachedWaitTime, float timeBetweenLances = 0.1f, bool shouldSpin = false)
		{
			this.totalNumLances = totalNumLances;
			this.firstOriginPoint = firstOriginPoint;
			this.firstEndPoint = firstEndPoint;
			this.secondOriginPoint = secondOriginPoint;
			this.secondEndPoint = secondEndPoint;
			this.targetPosition = targetPosition;
			this.setLanceMethod = setLanceMethod;
			this.activateLancesMethod = activateLancesMethod;
			this.anticipationtWaitTime = anticipationtWaitTime;
			this.afterEndReachedWaitTime = afterEndReachedWaitTime;
			this.timeBetweenLances = timeBetweenLances;
			this.shouldSpin = shouldSpin;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_SHOOT_1.StopAction();
			ACT_SHOOT_2.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			ACT_SHOOT_1.StartAction(owner, totalNumLances / 2, firstOriginPoint, firstEndPoint, targetPosition, setLanceMethod, activateLancesMethod, anticipationtWaitTime, afterEndReachedWaitTime, timeBetweenLances, shouldSpin);
			ACT_SHOOT_2.StartAction(owner, totalNumLances / 2, secondOriginPoint, secondEndPoint, targetPosition, setLanceMethod, activateLancesMethod, anticipationtWaitTime, afterEndReachedWaitTime, timeBetweenLances, shouldSpin);
			yield return ACT_SHOOT_1.waitForCompletion;
			yield return ACT_SHOOT_2.waitForCompletion;
			FinishAction();
		}
	}

	public class FreezeTimeBlinkShots_EnemyAction : EnemyAction
	{
		private int numBullets;

		private float waitTime;

		private bool doRandomDisplacement;

		private Action setBulletMethod;

		private Action activateMethod;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private LaunchMethod_EnemyAction ACT_SETBULLET = new LaunchMethod_EnemyAction();

		private LaunchMethod_EnemyAction ACT_ACTIVATE = new LaunchMethod_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, int numBullets, float waitTime, bool doRandomDisplacement, Action setBulletMethod, Action activateMethod)
		{
			this.numBullets = numBullets;
			this.waitTime = waitTime;
			this.doRandomDisplacement = doRandomDisplacement;
			this.setBulletMethod = setBulletMethod;
			this.activateMethod = activateMethod;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_ACTIVATE.StopAction();
			ACT_SETBULLET.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector animatorInyector = amanecidasBehaviour.Amanecidas.AnimatorInyector;
			animatorInyector.SetBow(value: false);
			animatorInyector.SetBlink(value: false);
			amanecidasBehaviour.SetGhostTrail(active: true);
			amanecidasBehaviour.LookAtPenitent(instant: true);
			amanecidasBehaviour.ClearRotationAndFlip();
			amanecidasBehaviour.Amanecidas.SetOrientation(amanecidasBehaviour.Amanecidas.Status.Orientation);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			Vector2 pointA = o.battleBounds.center + Vector2.left * 5f + Vector2.up * 3.5f;
			Vector2 pointB = pointA + Vector2.right * 10f;
			AmanecidasAnimatorInyector anim = o.Amanecidas.AnimatorInyector;
			anim.SetBlink(value: true);
			o.SetGhostTrail(active: false);
			ACT_WAIT.StartAction(owner, 1f);
			yield return ACT_WAIT.waitForCompletion;
			Vector2 target;
			for (int i = 0; i < numBullets; i++)
			{
				target = Vector2.Lerp(pointA, pointB, (float)i / ((float)numBullets - 1f));
				if (doRandomDisplacement)
				{
					target += Vector2.up * UnityEngine.Random.Range(-1, 1) * 0.8f;
				}
				ACT_MOVE.StartAction(owner, target, 0.15f, Ease.InOutQuad);
				yield return ACT_MOVE.waitForCompletion;
				ACT_SETBULLET.StartAction(owner, setBulletMethod);
				yield return ACT_SETBULLET.waitForCompletion;
				ACT_WAIT.StartAction(owner, waitTime);
				yield return ACT_WAIT.waitForCompletion;
			}
			target = Vector2.Lerp(pointA, pointB, 0.5f) + new Vector2(-0.5f, 1.75f);
			ACT_MOVE.StartAction(owner, target, 0.5f, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			anim.SetBlink(value: false);
			o.SetGhostTrail(active: true);
			o.ClearRotationAndFlip();
			ACT_ACTIVATE.StartAction(owner, activateMethod);
			yield return ACT_ACTIVATE.waitForCompletion;
			FinishAction();
		}
	}

	public class FreezeTimeMultiShots_EnemyAction : EnemyAction
	{
		private int numJumps;

		private float waitTime;

		private Action setBulletMethod;

		private Action activateMethod;

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private LaunchMethod_EnemyAction ACT_SETBULLET = new LaunchMethod_EnemyAction();

		private LaunchMethod_EnemyAction ACT_ACTIVATE = new LaunchMethod_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, int numJumps, float waitTime, Action setBulletMethod, Action activateMethod)
		{
			this.numJumps = numJumps;
			this.waitTime = waitTime;
			this.setBulletMethod = setBulletMethod;
			this.activateMethod = activateMethod;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_BLINK.StopAction();
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_ACTIVATE.StopAction();
			ACT_SETBULLET.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector animatorInyector = amanecidasBehaviour.Amanecidas.AnimatorInyector;
			animatorInyector.SetBow(value: false);
			animatorInyector.SetBlink(value: false);
			amanecidasBehaviour.SetGhostTrail(active: true);
			amanecidasBehaviour.LookAtPenitent(instant: true);
			amanecidasBehaviour.ClearRotationAndFlip();
			amanecidasBehaviour.Amanecidas.SetOrientation(amanecidasBehaviour.Amanecidas.Status.Orientation);
			activateMethod();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			List<Vector2> points = new List<Vector2>();
			float startX = ((!(p.transform.position.x > o.battleBounds.center.x)) ? o.battleBounds.xMax : o.battleBounds.xMin);
			float endX = ((!(p.transform.position.x > o.battleBounds.center.x)) ? (o.battleBounds.xMin + 2f) : (o.battleBounds.xMax - 2f));
			float startY = o.battleBounds.yMin + 1f;
			float endY = o.battleBounds.yMax + 1f;
			for (int j = 0; j < numJumps; j++)
			{
				Vector2 zero = Vector2.zero;
				zero.x = Mathf.Lerp(startX, endX, (float)j / ((float)numJumps - 1f));
				zero.y = Mathf.Lerp(startY, endY, (float)j / ((float)numJumps - 1f));
				points.Add(zero);
			}
			AmanecidasAnimatorInyector anim = o.Amanecidas.AnimatorInyector;
			anim.SetBlink(value: false);
			anim.SetBow(value: true);
			o.SetGhostTrail(active: false);
			for (int i = 0; i < numJumps; i++)
			{
				if (i % 2 == 0)
				{
					ACT_MOVE.StartAction(owner, points[i], waitTime, Ease.OutQuad, null, _timeScaled: true, o.AimToPenitentWithBow);
					yield return ACT_MOVE.waitForCompletion;
					anim.SetBow(value: false);
					o.SetGhostTrail(active: true);
					ACT_SETBULLET.StartAction(owner, setBulletMethod);
					yield return ACT_SETBULLET.waitForCompletion;
					o.LookAtPenitent();
					yield return new WaitUntilIdle(o, 1f);
				}
				else
				{
					Vector2 targetPoint = points[i];
					targetPoint.y = (points[i - 1].y + points[i + 1].y) / 2f;
					ACT_MOVE.StartAction(owner, targetPoint, waitTime, Ease.InQuad);
					yield return ACT_MOVE.waitForCompletion;
					if (i < numJumps - 1)
					{
						anim.SetBow(value: true);
						o.SetGhostTrail(active: false);
					}
					ACT_ACTIVATE.StartAction(owner, activateMethod);
					yield return ACT_ACTIVATE.waitForCompletion;
					o.LookAtPenitent(instant: true);
					if (o.Amanecidas.IsLaudes && o.CanUseLaserShotAttack())
					{
						anim.SetBow(value: false);
						anim.SetBlink(value: false);
						o.SetGhostTrail(active: true);
						o.LookAtPenitent(instant: true);
						o.ClearRotationAndFlip();
						o.Amanecidas.SetOrientation(o.Amanecidas.Status.Orientation);
						o.SetExtraRecoverySeconds(-0.5f);
						break;
					}
				}
				ACT_ACTIVATE.StartAction(owner, activateMethod);
				yield return ACT_ACTIVATE.waitForCompletion;
				o.ClearRotationAndFlip();
				o.Amanecidas.SetOrientation(o.Amanecidas.Status.Orientation);
			}
			FinishAction();
		}
	}

	public class FastShot_EnemyAction : EnemyAction
	{
		private float waitTime;

		private BossStraightProjectileAttack attack;

		private KeepDistanceFromTPOUsingAgent_EnemyAction ACT_KEEPDISTANCE = new KeepDistanceFromTPOUsingAgent_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_AGENT = new MoveToPointUsingAgent_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, float waitTime, BossStraightProjectileAttack attack)
		{
			this.waitTime = waitTime;
			this.attack = attack;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_KEEPDISTANCE.StopAction();
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_MOVE_AGENT.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector animatorInyector = amanecidasBehaviour.Amanecidas.AnimatorInyector;
			animatorInyector.SetBow(value: false);
			animatorInyector.SetBlink(value: false);
			amanecidasBehaviour.SetGhostTrail(active: true);
			amanecidasBehaviour.LookAtPenitent(instant: true);
			amanecidasBehaviour.ClearRotationAndFlip();
			amanecidasBehaviour.Amanecidas.SetOrientation(amanecidasBehaviour.Amanecidas.Status.Orientation);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			AmanecidasAnimatorInyector anim = o.Amanecidas.AnimatorInyector;
			anim.SetBlink(value: false);
			anim.SetBow(value: false);
			o.LookAtPenitent();
			yield return new WaitUntilIdle(o, 1f);
			anim.SetBow(value: true);
			o.SetGhostTrail(active: false);
			Vector2 pPos = p.transform.position;
			Vector2 newPos = (Vector2)o.transform.position + o.GetDirToPenitent(o.transform.position).normalized;
			if (pPos.y > o.transform.position.y - 1f)
			{
				newPos = new Vector2(o.transform.position.x, pPos.y);
			}
			ACT_MOVE_AGENT.StartAction(o, o.agent, newPos);
			yield return ACT_MOVE_AGENT.waitForCompletion;
			Vector2 dir = o.GetDirToPenitent(o.transform.position);
			o.AimToPointWithBow((Vector2)o.transform.position + dir);
			ACT_WAIT.StartAction(owner, waitTime);
			yield return ACT_WAIT.waitForCompletion;
			BulletTimeProjectile arrow = (BulletTimeProjectile)attack.Shoot(dir, Vector2.up * 1.25f);
			arrow.Accelerate(5.2f);
			o.Amanecidas.Audio.PlayArrowFire_AUDIO();
			anim.SetBow(value: false);
			o.SetGhostTrail(active: true);
			ACT_WAIT.StartAction(owner, 0.12f);
			yield return ACT_WAIT.waitForCompletion;
			ACT_MOVE.StartAction(owner, (Vector2)o.transform.position - dir.normalized * 1.5f, 0.2f, Ease.OutBack);
			yield return ACT_MOVE.waitForCompletion;
			o.ClearRotationAndFlip();
			o.Amanecidas.SetOrientation(o.Amanecidas.Status.Orientation);
			o.LookAtPenitent();
			FinishAction();
		}
	}

	public class KeepDistanceFromTPOUsingAgent_EnemyAction : EnemyAction
	{
		private float seconds;

		private bool driftHoriontally;

		private bool driftVertically;

		private bool clampHorizontally;

		private float minHorizontalPos;

		private float maxHorizontalPos;

		private bool clampVertically;

		private float minVerticalPos;

		private float maxVerticalPos;

		private bool forceOffset;

		private float horizontalOffset;

		private float verticalOffset;

		public EnemyAction StartAction(EnemyBehaviour e, float seconds, bool driftHoriontally, bool driftVertically, bool clampHorizontally = false, float minHorizontalPos = 0f, float maxHorizontalPos = 0f, bool clampVertically = false, float minVerticalPos = 0f, float maxVerticalPos = 0f)
		{
			this.seconds = seconds;
			this.driftHoriontally = driftHoriontally;
			this.driftVertically = driftVertically;
			this.clampHorizontally = clampHorizontally;
			this.minHorizontalPos = minHorizontalPos;
			this.maxHorizontalPos = maxHorizontalPos;
			this.clampVertically = clampVertically;
			this.minVerticalPos = minVerticalPos;
			this.maxVerticalPos = maxVerticalPos;
			forceOffset = false;
			return StartAction(e);
		}

		public EnemyAction StartAction(EnemyBehaviour e, float seconds, bool driftHoriontally, bool driftVertically, float horizontalOffset = 0f, float verticalOffset = 0f)
		{
			this.seconds = seconds;
			this.driftHoriontally = driftHoriontally;
			this.driftVertically = driftVertically;
			clampHorizontally = false;
			clampVertically = false;
			forceOffset = true;
			this.horizontalOffset = horizontalOffset;
			this.verticalOffset = verticalOffset;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			AmanecidasBehaviour component = owner.GetComponent<AmanecidasBehaviour>();
			component.agent.SetConfig(component.actionConfig);
			component.agent.enabled = false;
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			if (!forceOffset)
			{
				horizontalOffset = owner.transform.position.x - p.GetPosition().x;
				verticalOffset = owner.transform.position.y - p.GetPosition().y;
			}
			AmanecidasBehaviour ama = owner.GetComponent<AmanecidasBehaviour>();
			Arrive arrive = ama.agent.GetComponent<Arrive>();
			ama.agent.enabled = true;
			ama.agent.SetConfig(ama.keepDistanceConfig);
			float counter = 0f;
			float timeStep = 1f / 30f;
			while (counter < seconds)
			{
				Vector2 targetPoint = owner.transform.position;
				if (driftHoriontally)
				{
					targetPoint.x = horizontalOffset + p.GetPosition().x;
					if (clampHorizontally)
					{
						targetPoint.x = Mathf.Clamp(targetPoint.x, minHorizontalPos, maxHorizontalPos);
						float x = Mathf.Clamp(ama.transform.position.x, minHorizontalPos, maxHorizontalPos);
						ama.transform.position = new Vector2(x, ama.transform.position.y);
					}
				}
				if (driftVertically)
				{
					targetPoint.y = verticalOffset + p.GetPosition().y;
					if (clampVertically)
					{
						targetPoint.y = Mathf.Clamp(targetPoint.y, minVerticalPos, maxVerticalPos);
						float y = Mathf.Clamp(ama.transform.position.y, minVerticalPos, maxVerticalPos);
						ama.transform.position = new Vector2(ama.transform.position.x, y);
					}
				}
				arrive.target = targetPoint;
				counter += timeStep;
				yield return new WaitForSeconds(timeStep);
			}
			ama.agent.enabled = false;
			ama.agent.SetConfig(ama.actionConfig);
			FinishAction();
		}
	}

	public class KeepDistanceFromAmanecidaUsingAgent_EnemyAction : EnemyAction
	{
		private AmanecidasBehaviour ama;

		private float seconds;

		private float horizontalOffset;

		private float verticalOffset;

		public EnemyAction StartAction(EnemyBehaviour e, AmanecidasBehaviour ama, float seconds, float horizontalOffset, float verticalOffset)
		{
			this.ama = ama;
			this.seconds = seconds;
			this.horizontalOffset = horizontalOffset;
			this.verticalOffset = verticalOffset;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidaAxeBehaviour axe = owner.GetComponent<AmanecidaAxeBehaviour>();
			axe.ActivateAgent(active: true);
			float counter = 0f;
			float timeStep = 1f / 30f;
			while (counter < seconds)
			{
				Vector2 targetPoint = ama.transform.position;
				targetPoint.x += horizontalOffset;
				targetPoint.y += verticalOffset;
				axe.SeekTarget(targetPoint);
				counter += timeStep;
				yield return new WaitForSeconds(timeStep);
			}
			axe.ActivateAgent(active: false);
			FinishAction();
		}
	}

	public class FastShots_EnemyAction : EnemyAction
	{
		private float waitTime;

		private int numShots;

		private BossStraightProjectileAttack attack;

		private KeepDistanceFromTPOUsingAgent_EnemyAction ACT_KEEPDISTANCE = new KeepDistanceFromTPOUsingAgent_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_AGENT = new MoveToPointUsingAgent_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, float waitTime, int numShots, BossStraightProjectileAttack attack)
		{
			this.waitTime = waitTime;
			this.numShots = numShots;
			this.attack = attack;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_KEEPDISTANCE.StopAction();
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_MOVE_AGENT.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector animatorInyector = amanecidasBehaviour.Amanecidas.AnimatorInyector;
			animatorInyector.SetBow(value: false);
			animatorInyector.SetBlink(value: false);
			amanecidasBehaviour.SetGhostTrail(active: true);
			amanecidasBehaviour.LookAtPenitent(instant: true);
			amanecidasBehaviour.ClearRotationAndFlip();
			amanecidasBehaviour.Amanecidas.SetOrientation(amanecidasBehaviour.Amanecidas.Status.Orientation);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			AmanecidasAnimatorInyector anim = o.Amanecidas.AnimatorInyector;
			anim.SetBlink(value: false);
			o.SetGhostTrail(active: false);
			for (int i = 0; i < numShots; i++)
			{
				anim.SetBow(value: true);
				Vector2 pPos = p.transform.position;
				Vector2 newPos = (Vector2)o.transform.position + o.GetDirToPenitent(o.transform.position).normalized;
				if (pPos.y > o.transform.position.y - 1f)
				{
					newPos = new Vector2(o.transform.position.x, pPos.y);
				}
				ACT_MOVE_AGENT.StartAction(o, o.agent, newPos);
				yield return ACT_MOVE_AGENT.waitForCompletion;
				Vector2 dir = o.GetDirToPenitent(o.transform.position);
				o.AimToPointWithBow((Vector2)o.transform.position + dir);
				ACT_WAIT.StartAction(owner, waitTime);
				yield return ACT_WAIT.waitForCompletion;
				BulletTimeProjectile arrow = (BulletTimeProjectile)attack.Shoot(dir, Vector2.up);
				arrow.Accelerate(1.1f);
				o.Amanecidas.Audio.PlayArrowFire_AUDIO();
				ACT_MOVE.StartAction(owner, (Vector2)o.transform.position - dir.normalized * 0.8f, 0.1f, Ease.OutExpo);
				yield return ACT_MOVE.waitForCompletion;
				anim.SetBow(value: false);
				ACT_WAIT.StartAction(owner, 0.3f);
				yield return ACT_WAIT.waitForCompletion;
				if (o.throwbackExtraTime > 0f)
				{
					break;
				}
			}
			o.SetGhostTrail(active: true);
			o.ClearRotationAndFlip();
			o.Amanecidas.SetOrientation(o.Amanecidas.Status.Orientation);
			o.LookAtPenitent();
			FinishAction();
		}
	}

	public class ChargedShot_EnemyAction : EnemyAction
	{
		private Action chargeMethod;

		private Action releaseMethod;

		private float anticipationtWaitTime;

		private float recoveryTime;

		private TiredPeriod_EnemyAction ACT_TIRED = new TiredPeriod_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private LaunchMethod_EnemyAction ACT_METHODLAUNCH = new LaunchMethod_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, Action chargeMethod, Action releaseMethod, float anticipationtWaitTime, float recoveryTime)
		{
			this.chargeMethod = chargeMethod;
			this.releaseMethod = releaseMethod;
			this.anticipationtWaitTime = anticipationtWaitTime;
			this.recoveryTime = recoveryTime;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_TIRED.StopAction();
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_METHODLAUNCH.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector animatorInyector = amanecidasBehaviour.Amanecidas.AnimatorInyector;
			animatorInyector.SetBow(value: false);
			animatorInyector.SetBlink(value: false);
			amanecidasBehaviour.SetGhostTrail(active: true);
			amanecidasBehaviour.LookAtPenitent(instant: true);
			amanecidasBehaviour.ClearRotationAndFlip();
			amanecidasBehaviour.Amanecidas.SetOrientation(amanecidasBehaviour.Amanecidas.Status.Orientation);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector anim = o.Amanecidas.AnimatorInyector;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			Vector2 target2 = Vector2.zero;
			target2 = ((!(p.GetPosition().x > o.battleBounds.center.x)) ? new Vector2(o.battleBounds.xMin - 0.5f, o.battleBounds.yMin + 0.2f) : new Vector2(o.battleBounds.xMax + 0.5f, o.battleBounds.yMin + 0.2f));
			ACT_MOVE.StartAction(owner, target2, 0.1f, Ease.InCubic);
			yield return ACT_MOVE.waitForCompletion;
			o.ClearRotationAndFlip();
			anim.SetBow(value: true);
			o.SetGhostTrail(active: false);
			Vector2 dir = ((!(o.transform.position.x > o.battleBounds.center.x)) ? Vector2.right : Vector2.left);
			o.AimToPointWithBow((Vector2)o.transform.position + dir);
			o.PlayChargeEnergy(anticipationtWaitTime - 1f);
			ACT_METHODLAUNCH.StartAction(owner, chargeMethod);
			yield return ACT_METHODLAUNCH.waitForCompletion;
			ACT_WAIT.StartAction(owner, anticipationtWaitTime);
			yield return ACT_WAIT.waitForCompletion;
			anim.SetBow(value: false);
			ACT_METHODLAUNCH.StartAction(owner, releaseMethod);
			yield return ACT_METHODLAUNCH.waitForCompletion;
			o.ClearRotationAndFlip();
			o.LookAtDirUsingOrientation(dir);
			o.SetGhostTrail(active: true);
			ACT_TIRED.StartAction(owner, recoveryTime, interruptable: true);
			yield return ACT_TIRED.waitForCompletion;
			FinishAction();
		}
	}

	public class SpikesBlinkShots_EnemyAction : EnemyAction
	{
		private int numBlinks;

		private float anticipationtWaitTime;

		private float blinksWaitTime;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_CHARACTER = new MoveToPointUsingAgent_EnemyAction();

		private LaunchMethod_EnemyAction ACT_METHODLAUNCH = new LaunchMethod_EnemyAction();

		private Action shootMethod;

		public EnemyAction StartAction(EnemyBehaviour e, int numBlinks, float anticipationtWaitTime, float blinksWaitTime, Action shootMethod)
		{
			this.numBlinks = numBlinks;
			this.anticipationtWaitTime = anticipationtWaitTime;
			this.blinksWaitTime = blinksWaitTime;
			this.shootMethod = shootMethod;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_MOVE_CHARACTER.StopAction();
			ACT_METHODLAUNCH.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector animatorInyector = amanecidasBehaviour.Amanecidas.AnimatorInyector;
			animatorInyector.SetBow(value: false);
			animatorInyector.SetBlink(value: false);
			amanecidasBehaviour.SetGhostTrail(active: true);
			amanecidasBehaviour.LookAtPenitent(instant: true);
			amanecidasBehaviour.ClearRotationAndFlip();
			amanecidasBehaviour.Amanecidas.SetOrientation(amanecidasBehaviour.Amanecidas.Status.Orientation);
			amanecidasBehaviour.verticalSlowBlastArrowAttack.ClearAll();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour o = owner as AmanecidasBehaviour;
			AmanecidasAnimatorInyector anim = o.Amanecidas.AnimatorInyector;
			float minX = o.battleBounds.center.x - o.battleBounds.width / 3f;
			float maxX = o.battleBounds.center.x + o.battleBounds.width / 3f;
			int indexOffset = UnityEngine.Random.Range(0, numBlinks);
			anim.SetBow(value: false);
			anim.SetBlink(value: false);
			yield return new WaitUntilIdle(o, 1f);
			for (int i = 0; i < numBlinks; i++)
			{
				float lerpPercentage = (float)((i + indexOffset) * 2 % numBlinks) / (float)(numBlinks - 1);
				float targetX2 = Mathf.Lerp(minX, maxX, lerpPercentage);
				targetX2 += UnityEngine.Random.Range(-0.5f, 0.5f);
				Vector2 target = new Vector2(targetX2, o.battleBounds.yMax);
				if (i == 0)
				{
					ACT_MOVE_CHARACTER.StartAction(owner, o.agent, target);
					yield return ACT_MOVE_CHARACTER.waitForCompletion;
				}
				else
				{
					ACT_MOVE.StartAction(owner, target, 0.2f, Ease.InCubic);
					yield return ACT_MOVE.waitForCompletion;
				}
				anim.SetBow(value: true);
				anim.SetBlink(value: false);
				o.SetGhostTrail(active: false);
				o.ClearRotationAndFlip();
				o.AimToPointWithBow((Vector2)o.transform.position + Vector2.down);
				if (i == 0)
				{
					Vector2 vector = new Vector2(o.battleBounds.xMin - 0.5f, o.battleBounds.yMin);
					Vector2 vector2 = new Vector2(o.battleBounds.xMax + 0.5f, o.battleBounds.yMin);
					o.verticalSlowBlastArrowAttack.SummonAreaOnPoint(vector, 270f);
					o.verticalSlowBlastArrowAttack.SummonAreaOnPoint(vector2, 270f);
				}
				o.Amanecidas.Audio.PlayVerticalPreattack_AUDIO();
				ACT_WAIT.StartAction(owner, anticipationtWaitTime);
				yield return ACT_WAIT.waitForCompletion;
				anim.SetBow(value: false);
				anim.SetBlink(value: true);
				ACT_METHODLAUNCH.StartAction(owner, shootMethod);
				yield return ACT_METHODLAUNCH.waitForCompletion;
				ACT_WAIT.StartAction(owner, blinksWaitTime);
				yield return ACT_WAIT.waitForCompletion;
			}
			o.verticalSlowBlastArrowAttack.ClearAll();
			anim.SetBlink(value: false);
			o.ClearRotationAndFlip();
			o.LookAtPenitent(instant: true);
			o.Amanecidas.SetOrientation(o.Amanecidas.Status.Orientation);
			o.SetGhostTrail(active: true);
			FinishAction();
		}
	}

	public class FreezeTimeNRicochetShots_EnemyComboAction : EnemyAction
	{
		private int numBullets;

		private bool doRandomDisplacement;

		private float freezeTimeWaitTime;

		private Action setBulletMethod;

		private Action activateMethod;

		private int numBounces;

		private float ricochetWaitTime;

		private LayerMask mask;

		private Action<Vector2, Vector2> showArrowTrailMethod;

		private Action<Vector2, Vector2> shootArrowMethod;

		private FreezeTimeBlinkShots_EnemyAction ACT_FREEZE = new FreezeTimeBlinkShots_EnemyAction();

		private ShootRicochetArrow_EnemyAction ACT_RICOCHET_1 = new ShootRicochetArrow_EnemyAction();

		private ShootRicochetArrow_EnemyAction ACT_RICOCHET_2 = new ShootRicochetArrow_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, int numBullets, float freezeTimeWaitTime, bool doRandomDisplacement, Action setBulletMethod, Action activateMethod, int numBounces, Action<Vector2, Vector2> showArrowTrailMethod, Action<Vector2, Vector2> shootArrowMethod, float ricochetWaitTime, LayerMask mask)
		{
			this.numBullets = numBullets;
			this.freezeTimeWaitTime = freezeTimeWaitTime;
			this.doRandomDisplacement = doRandomDisplacement;
			this.setBulletMethod = setBulletMethod;
			this.activateMethod = activateMethod;
			this.numBounces = numBounces;
			this.showArrowTrailMethod = showArrowTrailMethod;
			this.shootArrowMethod = shootArrowMethod;
			this.ricochetWaitTime = ricochetWaitTime;
			this.mask = mask;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_FREEZE.StopAction();
			ACT_RICOCHET_1.StopAction();
			ACT_RICOCHET_2.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			ACT_FREEZE.StartAction(owner, numBullets, freezeTimeWaitTime, doRandomDisplacement, setBulletMethod, delegate
			{
			});
			yield return ACT_FREEZE.waitForCompletion;
			ACT_RICOCHET_1.StartAction(owner, numBounces, showArrowTrailMethod, shootArrowMethod, ricochetWaitTime, mask, isPartOfCombo: true);
			ACT_RICOCHET_2.StartAction(owner, numBounces, showArrowTrailMethod, shootArrowMethod, ricochetWaitTime, mask, isPartOfCombo: true, launchToTheRight: true);
			yield return ACT_RICOCHET_1.waitForCompletion;
			yield return ACT_RICOCHET_2.waitForCompletion;
			activateMethod();
			FinishAction();
		}
	}

	public class MultiStompNLavaBalls_EnemyComboAction : EnemyAction
	{
		private Action jumpMethod;

		private int numJumps;

		private MultiStompAttack_EnemyAction ACT_STOMP = new MultiStompAttack_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private CallAxe_EnemyAction ACT_CALLAXE1 = new CallAxe_EnemyAction();

		private CallAxe_EnemyAction ACT_CALLAXE2 = new CallAxe_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, Action jumpMethod, int numJumps)
		{
			this.jumpMethod = jumpMethod;
			this.numJumps = numJumps;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_STOMP.StopAction();
			ACT_BLINK.StopAction();
			ACT_WAIT.StopAction();
			ACT_CALLAXE1.StopAction();
			ACT_CALLAXE2.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			amanecidasBehaviour.ShowBothAxes(v: false);
			amanecidasBehaviour.currentMeleeAttack = amanecidasBehaviour.meleeAxeAttack;
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			ACT_BLINK.StartAction(owner, ama.battleBounds.center + Vector2.up * 1.4f, 0.2f);
			yield return ACT_BLINK.waitForCompletion;
			bool recallingAxes = false;
			if (!ama.IsWieldingAxe())
			{
				ACT_CALLAXE1.StartAction(owner, owner.transform.position + Vector3.up * 1.75f, ama.axes[0], 0.1f, 0.3f);
				ACT_CALLAXE2.StartAction(owner, owner.transform.position + Vector3.up * 1.75f, ama.axes[1], 0.1f, 0.3f);
				recallingAxes = true;
			}
			ama.currentMeleeAttack = ama.meleeStompAttack;
			ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
			ama.Amanecidas.AnimatorInyector.PlayStompAttack(doStompDamage: false);
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: false);
			ama.Amanecidas.Audio.PlayAttackCharge_AUDIO();
			ama.PlayChargeEnergy(0.4f, useLongRangeParticles: false, playSfx: false);
			ACT_WAIT.StartAction(owner, 1f);
			if (recallingAxes)
			{
				yield return ACT_CALLAXE1.waitForCompletion;
				yield return ACT_CALLAXE2.waitForCompletion;
			}
			ama.ShowBothAxes(v: false);
			ama.SetGhostTrail(active: false);
			yield return ACT_WAIT.waitForCompletion;
			ACT_STOMP.StartAction(ama, numJumps, 0.3f, jumpMethod, doBlinkBeforeJump: false, doStompDamage: false, usePillars: true, bounceAfterJump: true, 3f);
			yield return ACT_STOMP.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			yield return new WaitUntilIdle(ama);
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			ama.currentMeleeAttack = ama.meleeAxeAttack;
			ama.SetGhostTrail(active: true);
			ACT_WAIT.StartAction(owner, 0.3f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class MoveBattleBounds_EnemyAction : EnemyAction
	{
		private Vector2 direction;

		private float transitionTime;

		private AmanecidaArena.WEAPON_FIGHT_PHASE fightPhase;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private MoveToPointUsingAgent_EnemyAction ACT_MOVE_CHARACTER = new MoveToPointUsingAgent_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 direction, float transitionTime, AmanecidaArena.WEAPON_FIGHT_PHASE fightPhase)
		{
			this.direction = direction;
			this.transitionTime = transitionTime;
			this.fightPhase = fightPhase;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_MOVE_CHARACTER.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			ACT_MOVE_CHARACTER.StartAction(owner, ama.agent, ama.battleBounds.center + Vector2.up * 1.5f);
			yield return ACT_MOVE_CHARACTER.waitForCompletion;
			ama.currentMeleeAttack = ama.meleeStompAttack;
			ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
			ama.Amanecidas.AnimatorInyector.PlayStompAttack(doStompDamage: false);
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: false);
			ACT_WAIT.StartAction(owner, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			Vector2 targetPos = ama.battleBounds.center;
			targetPos.y += direction.y + 2f;
			ama.Amanecidas.LaudesArena.ActivateGameObjectsByWeaponFightPhase(ama.currentWeapon, fightPhase);
			ACT_MOVE.StartAction(owner, targetPos, 5f, Ease.InOutCirc);
			yield return ACT_MOVE.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			yield return new WaitUntilIdle(ama, 1f);
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			ama.currentMeleeAttack = ama.meleeFalcataAttack;
			ama.battleBounds.center += direction;
			ACT_WAIT.StartAction(owner, transitionTime);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class FalcataSlashStorm_EnemyComboAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, int n)
		{
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			amanecidasBehaviour.currentMeleeAttack = amanecidasBehaviour.meleeFalcataAttack;
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			int j = 20;
			ACT_BLINK.StartAction(owner, ama.GetPointBelow(ama.battleBounds.center, stopOnOneWayDowns: true), 0.1f);
			yield return ACT_BLINK.waitForCompletion;
			ama.currentMeleeAttack = ama.meleeStompAttack;
			ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
			ama.Amanecidas.AnimatorInyector.PlayStompAttack(doStompDamage: false);
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: false);
			ama.PlayChargeEnergy();
			ACT_WAIT.StartAction(owner, 1.2f);
			yield return ACT_WAIT.waitForCompletion;
			float timeBetweenProjectiles = 0.4f;
			float distance = 5f;
			float seconds = (float)j * timeBetweenProjectiles;
			ama.transform.DOMoveY(ama.transform.position.y + distance, seconds).SetEase(Ease.InOutCubic);
			Vector2 dir = Vector2.up;
			Quaternion rot = Quaternion.Euler(0f, 0f, -15f);
			Quaternion rotQuarter = Quaternion.Euler(0f, 0f, -90f);
			Quaternion rotHalf = Quaternion.Euler(0f, 0f, -180f);
			Quaternion rotThreeQuarter = Quaternion.Euler(0f, 0f, -270f);
			for (int i = 0; i < j; i++)
			{
				ama.falcataSlashProjectileAttack.Shoot(dir);
				ama.falcataSlashProjectileAttack.Shoot(rotHalf * dir);
				dir = rot * dir;
				ACT_WAIT.StartAction(owner, timeBetweenProjectiles);
				yield return ACT_WAIT.waitForCompletion;
			}
			ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			yield return new WaitUntilIdle(ama);
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			ama.currentMeleeAttack = ama.meleeFalcataAttack;
			ACT_WAIT.StartAction(owner, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class FalcataSlashOnFloor_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private BlinkToPoint_EnemyAction ACT_BLINK = new BlinkToPoint_EnemyAction();

		private int numberOfLoops = 2;

		public EnemyAction StartAction(EnemyBehaviour e, int repetitions)
		{
			numberOfLoops = repetitions;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeAnticipation(v: false);
			amanecidasBehaviour.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			amanecidasBehaviour.currentMeleeAttack = amanecidasBehaviour.meleeFalcataAttack;
			base.DoOnStop();
		}

		private void LaunchProjectiles()
		{
			AmanecidasBehaviour amanecidasBehaviour = owner as AmanecidasBehaviour;
			Vector2 right = Vector2.right;
			Quaternion quaternion = Quaternion.Euler(0f, 0f, -90f);
			Quaternion quaternion2 = Quaternion.Euler(0f, 0f, -180f);
			Quaternion quaternion3 = Quaternion.Euler(0f, 0f, -270f);
			Vector2 offset = Vector2.up * 0.5f;
			amanecidasBehaviour.falcataSlashProjectileAttack.Shoot(right, offset);
			amanecidasBehaviour.falcataSlashProjectileAttack.Shoot(quaternion * right, offset);
			amanecidasBehaviour.falcataSlashProjectileAttack.Shoot(quaternion2 * right, offset);
			amanecidasBehaviour.falcataSlashProjectileAttack.Shoot(quaternion3 * right, offset);
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour ama = owner as AmanecidasBehaviour;
			float distanceFromCenter = 5f;
			ACT_BLINK.StartAction(owner, ama.GetPointBelow(ama.battleBounds.center + Vector2.right * distanceFromCenter, stopOnOneWayDowns: true), 0.1f);
			yield return ACT_BLINK.waitForCompletion;
			ama.currentMeleeAttack = ama.meleeStompAttack;
			ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
			ama.Amanecidas.AnimatorInyector.PlayStompAttack(doStompDamage: true);
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: false);
			ACT_WAIT.StartAction(owner, 1.2f);
			yield return ACT_WAIT.waitForCompletion;
			float singleLoopTime = 1.5f;
			float seconds = (float)numberOfLoops * singleLoopTime;
			ama.transform.DOMoveX(ama.transform.position.x - distanceFromCenter * 2f, singleLoopTime).SetEase(Ease.InOutCubic).SetLoops(numberOfLoops, LoopType.Yoyo)
				.OnStepComplete(delegate
				{
					LaunchProjectiles();
				});
			ACT_WAIT.StartAction(owner, seconds * 0.98f);
			yield return ACT_WAIT.waitForCompletion;
			ama.Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
			yield return new WaitUntilIdle(ama);
			ama.Amanecidas.AnimatorInyector.SetWeaponVisible(visible: true);
			ACT_WAIT.StartAction(owner, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			ama.currentMeleeAttack = ama.meleeFalcataAttack;
			FinishAction();
		}
	}

	public class FreezeTimeNHorizontalDashes_EnemyComboAction : EnemyAction
	{
		private int numLances;

		private Vector2 originPoint;

		private Vector2 endPoint;

		private Vector2 targetPosition;

		private Action<Vector2> setLanceMethod;

		private Action activateLancesMethod;

		private float anticipationtWaitTime;

		private float afterEndReachedWaitTime;

		private bool skipOne;

		private int numDashes;

		private float delay;

		private bool startDashesAwayFromPenitent;

		private ShootFrozenLances_EnemyAction ACT_LANCES = new ShootFrozenLances_EnemyAction();

		private HorizontalBlinkDashes_EnemyAction ACT_DASHES = new HorizontalBlinkDashes_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, int numLances, Vector2 originPoint, Vector2 endPoint, Vector2 targetPosition, Action<Vector2> setLanceMethod, Action activateLancesMethod, float anticipationtWaitTime, float afterEndReachedWaitTime, bool skipOne, int numDashes, float delay, bool startDashesAwayFromPenitent)
		{
			this.numLances = numLances;
			this.originPoint = originPoint;
			this.endPoint = endPoint;
			this.targetPosition = targetPosition;
			this.setLanceMethod = setLanceMethod;
			this.activateLancesMethod = activateLancesMethod;
			this.anticipationtWaitTime = anticipationtWaitTime;
			this.afterEndReachedWaitTime = afterEndReachedWaitTime;
			this.skipOne = skipOne;
			this.numDashes = numDashes;
			this.delay = delay;
			this.startDashesAwayFromPenitent = startDashesAwayFromPenitent;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_LANCES.StopAction();
			ACT_DASHES.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			yield return null;
			FinishAction();
		}
	}

	public class Intro_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private ShowAxes_EnemyAction ACT_AXES = new ShowAxes_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_AXES.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			AmanecidasBehaviour amanecida = owner as AmanecidasBehaviour;
			Vector2 introPointStart2 = amanecida.battleBounds.center + Vector2.left * 5f;
			introPointStart2.y = Core.Logic.Penitent.transform.position.y;
			Vector2 introPointEnd2 = introPointStart2 + Vector2.up * 4f;
			Debug.DrawRay(introPointStart2, introPointEnd2, Color.green, 10f);
			if (amanecida.Amanecidas.IsLaudes)
			{
				float secondsFloating2 = 7f;
				introPointStart2 = amanecida.battleBounds.center + Vector2.left * 5f + Vector2.up * 5.5f;
				introPointEnd2 = introPointStart2 + Vector2.down * 7f;
				amanecida.transform.position = introPointStart2;
				ACT_MOVE.StartAction(owner, introPointEnd2, secondsFloating2, Ease.OutCubic);
				yield return ACT_MOVE.waitForCompletion;
			}
			else
			{
				amanecida.ShowSprites(show: false);
				amanecida.Amanecidas.AnimatorInyector.ActivateIntroColor();
				GameObject area = amanecida.introBeamAttack.SummonAreaOnPoint(introPointStart2);
				area.GetComponentInChildren<SpriteRenderer>().material = amanecida.Amanecidas.AnimatorInyector.GetCurrentBeamMaterial();
				amanecida.transform.position = introPointStart2;
				amanecida.SetGhostTrail(active: false);
				ACT_WAIT.StartAction(owner, 0.7f);
				yield return ACT_WAIT.waitForCompletion;
				amanecida.ShowSprites(show: true);
				float secondsFloating = 4f;
				ACT_MOVE.StartAction(owner, introPointEnd2, secondsFloating, Ease.OutCubic);
				ACT_WAIT.StartAction(owner, 0.5f);
				yield return ACT_WAIT.waitForCompletion;
				amanecida.Amanecidas.AnimatorInyector.DeactivateIntroColor();
				area.GetComponentInChildren<SpriteRenderer>().sortingLayerName = "Before Player";
				ACT_WAIT.StartAction(owner, secondsFloating * 0.2f);
				yield return ACT_WAIT.waitForCompletion;
				amanecida.DoSummonWeaponAnimation();
				yield return ACT_MOVE.waitForCompletion;
			}
			amanecida.SetGhostTrail(active: true);
			amanecida.ShowBothAxes(v: false);
			FinishAction();
		}
	}

	[FoldoutGroup("Character settings", 0)]
	public AnimationCurve timeSlowCurve;

	[FoldoutGroup("Character settings", 0)]
	public Vector2 centerBodyOffset = new Vector2(0f, 1.2f);

	[FoldoutGroup("Battle area", 0)]
	public Rect battleBounds;

	[FoldoutGroup("Battle area", 0)]
	public Transform combatAreaParent;

	[FoldoutGroup("Movement", 0)]
	public AutonomousAgentConfig floatingConfig;

	[FoldoutGroup("Movement", 0)]
	public AutonomousAgentConfig actionConfig;

	[FoldoutGroup("Movement", 0)]
	public AutonomousAgentConfig keepDistanceConfig;

	[FoldoutGroup("Movement", 0)]
	public float minFloatingPointCD = 1f;

	[FoldoutGroup("Movement", 0)]
	public float maxFloatingPointCD = 3f;

	[FoldoutGroup("Movement", 0)]
	public LayerMask floorNOneWayDownMask;

	[FoldoutGroup("Movement", 0)]
	public LayerMask floorMask;

	[FoldoutGroup("Attacks config", 0)]
	public AmanecidaAttackScriptableConfig attackConfigData;

	[FoldoutGroup("Laudes only", 0)]
	public AmanecidaAttackScriptableConfig laudesAttackConfigData;

	[TableList]
	[FoldoutGroup("Laudes only", 0)]
	public List<AmanecidasFightParameters> laudesFightParameters;

	[TableList]
	[FoldoutGroup("Axe", 0)]
	public List<AmanecidasFightParameters> axeFightParameters;

	[FoldoutGroup("Axe", 0)]
	public SplineThrowData horizontalThrow;

	[FoldoutGroup("Axe", 0)]
	public SplineThrowData circleDownThrow;

	[FoldoutGroup("Axe", 0)]
	public SplineThrowData crossThrow;

	[FoldoutGroup("Axe", 0)]
	public SplineThrowData aroundThrow;

	[FoldoutGroup("Axe", 0)]
	public SplineThrowData aroundThrow2;

	[FoldoutGroup("Axe", 0)]
	public SplineThrowData straightThrow;

	[FoldoutGroup("Axe", 0)]
	public GameObject poolSplinePrefab;

	[FoldoutGroup("Axe", 0)]
	public GameObject amanecidaAxePrefab;

	[FoldoutGroup("Axe", 0)]
	public List<AmanecidaAxeBehaviour> axes;

	[FoldoutGroup("Axe", 0)]
	public Transform splineParent;

	[FoldoutGroup("Axe", 0)]
	public SplineFollower splineFollower;

	[FoldoutGroup("Axe", 0)]
	public SplineThrowData flightPattern;

	[FoldoutGroup("Axe", 0)]
	public Vector2 axeOffset;

	[FoldoutGroup("Axe", 0)]
	public List<Transform> axeTargets;

	[TableList]
	[FoldoutGroup("Bow", 0)]
	public List<AmanecidasFightParameters> bowFightParameters;

	[FoldoutGroup("Bow", 0)]
	public LayerMask ricochetMask;

	[FoldoutGroup("Bow", 0)]
	public float bowAngleDifference = 40f;

	[TableList]
	[FoldoutGroup("Lance", 0)]
	public List<AmanecidasFightParameters> lanceFightParameters;

	[FoldoutGroup("Lance", 0)]
	public float lanceRotationDiference = 40f;

	[TableList]
	[FoldoutGroup("Falcata", 0)]
	public List<AmanecidasFightParameters> falcataFightParameters;

	[FoldoutGroup("Falcata", 0)]
	public float falcataRotationDiference = 30f;

	[FoldoutGroup("Falcata", 0)]
	public SplineThrowData dodgeSplineData;

	[FoldoutGroup("Falcata", 0)]
	public float dodgeMaxCooldown;

	[FoldoutGroup("Falcata", 0)]
	public float dodgeDistance;

	[FoldoutGroup("Falcata", 0)]
	public GameObject clonePrefab;

	[FoldoutGroup("Shield", 0)]
	public GameObject shieldShockwave;

	[FoldoutGroup("VFX", 0)]
	public GameObject vortexVFX;

	[FoldoutGroup("VFX", 0)]
	public GameObject dustVFX;

	[FoldoutGroup("VFX", 0)]
	public ParticleSystem chargeParticles;

	[FoldoutGroup("VFX", 0)]
	public ParticleSystem chargeParticlesLongRange;

	[FoldoutGroup("Debug", 0)]
	public bool debugDrawCurrentAction;

	public GameObject trailGameObject;

	private StateMachine<AmanecidasBehaviour> _fsm;

	private State<AmanecidasBehaviour> stFloating;

	private State<AmanecidasBehaviour> stAction;

	private State<AmanecidasBehaviour> stIntro;

	private State<AmanecidasBehaviour> stDeath;

	private State<AmanecidasBehaviour> stHurt;

	private State<AmanecidasBehaviour> stRecharge;

	private WaitSeconds_EnemyAction waitBetweenActions_EA;

	private JumpBackAndShoot_EnemyAction jumpBackNShoot_EA;

	private ShootRicochetArrow_EnemyAction shootRicochetArrow_EA;

	private ShootLaserArrow_EnemyAction shootLaserArrow_EA;

	private ShootMineArrows_EnemyAction shootMineArrows_EA;

	private FreezeTimeBlinkShots_EnemyAction freezeTimeBlinkShots_EA;

	private FreezeTimeMultiShots_EnemyAction freezeTimeMultiShots_EA;

	private FastShot_EnemyAction fastShot_EA;

	private FastShots_EnemyAction fastShots_EA;

	private ChargedShot_EnemyAction chargedShot_EA;

	private SpikesBlinkShots_EnemyAction spikesBlinkShots_EA;

	private FreezeTimeNRicochetShots_EnemyComboAction freezeTimeNRicochetShots_ECA;

	private MoveBattleBounds_EnemyAction moveBattleBounds_EA;

	private ShootFrozenLances_EnemyAction shootFrozenLances_EA;

	private DoubleShootFrozenLances_EnemyAction doubleShootFrozenLances_EA;

	private HorizontalBlinkDashes_EnemyAction horizontalBlinkDashes_EA;

	private MultiFrontalDash_EnemyAction multiFrontalDash_EA;

	private DiagonalBlinkDashes_EnemyAction diagonalBlinkDashes_EA;

	private FreezeTimeNHorizontalDashes_EnemyComboAction freezeTimeNHorizontalDashes_ECA;

	private FalcataSlashStorm_EnemyComboAction falcataSlashStorm_ECA;

	private LaunchTwoAxesHorizontal_EnemyAction dualAxeThrow_EA;

	private HurtDisplacement_EnemyAction hurtDisplacement_EA;

	private Hurt_EnemyAction hurt_EA;

	private FlyAndLaunchTwoAxes_EnemyAction dualAxeFlyingThrow_EA;

	private SpinAxesAround_EnemyAction spinAxesAround_EA;

	private FollowSplineAndSpinAxesAround_EnemyAction followAndSpinAxes_EA;

	private JumpSmash_EnemyAction jumpSmash_EA;

	private LaunchAxesToPenitent_EnemyAction launchAxesToPenitent_EA;

	private LaunchCrawlerAxesToPenitent_EnemyAction launchCrawlerAxesToPenitent_EA;

	private LaunchBallsToPenitent_EnemyAction launchBallsToPenitent_EA;

	private MultiStompNLavaBalls_EnemyComboAction multiStompNLavaBalls_ECA;

	private BlinkToPoint_EnemyAction blink_EA;

	private RechargeShield_EnemyAction recoverShield_EA;

	private BlinkAndDashToPenitent_EnemyAction blinkDash_EA;

	private MeleeAttackTowardsPenitent_EnemyAction meleeAttack_EA;

	private MeleeAttackProjectile_EnemyAction meleeProjectile_EA;

	private GhostProjectile_EnemyAction ghostProjectile_EA;

	private Intro_EnemyAction intro_EA;

	private Death_EnemyAction death_EA;

	private FalcataSlashProjectile_EnemyAction falcataSlashProjectile_EA;

	private FalcataSlashBarrage_EnemyAction falcataSlashBarrage_EA;

	private StompAttack_EnemyAction stompAttack_EA;

	private MultiStompAttack_EnemyAction multiStompAttack_EA;

	private ChangeWeapon_EnemyAction changeWeapon_EA;

	private QuickLunge_EnemyAction quickLunge_EA;

	private DodgeAndCounterAttack_EnemyAction falcataCounter_EA;

	private ChainDash_EnemyAction chainDash_EA;

	private ShieldShockwave shieldShockwave_EA;

	private EnergyChargePeriod_EnemyAction chargeEnergy_EA;

	private FalcataSlashOnFloor_EnemyAction spinAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossInstantProjectileAttack instantProjectileAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossInstantProjectileAttack arrowTrailInstantProjectileAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossInstantProjectileAttack arrowTrailFastInstantProjectileAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossInstantProjectileAttack ricochetArrowInstantProjectileAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossInstantProjectileAttack arrowInstantProjectileAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossInstantProjectileAttack mineArrowProjectileAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossAreaSummonAttack chargedArrowExplosionAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossAreaSummonAttack verticalBlastArrowAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossAreaSummonAttack verticalSlowBlastArrowAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossAreaSummonAttack verticalBlastAxeAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossAreaSummonAttack verticalCrystalBeam;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossAreaSummonAttack verticalFastBlastAxeAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossAreaSummonAttack verticalNormalBlastAxeAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossAreaSummonAttack introBeamAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossJumpAttack jumpAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossDashAttack slashDashAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossDashAttack lanceDashAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossStraightProjectileAttack bulletTimeProjectileAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossStraightProjectileAttack bulletTimeLanceAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossStraightProjectileAttack bulletTimeHailAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossStraightProjectileAttack falcataSlashProjectileAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossStraightProjectileAttack lavaBallAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossStraightProjectileAttack flameBladeAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	private BossStraightProjectileAttack noxiousBladeAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public AmanecidasMeleeAttack meleeAxeAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public BossStraightProjectileAttack ghostProjectileAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public AmanecidasMeleeAttack meleeFalcataAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public AmanecidasMeleeAttack meleeStompAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public BossSpawnedGeoAttack spikePrefab;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public BossAreaSummonAttack shockwave;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public TileableBeamLauncher beamLauncher;

	public AmanecidasAnimatorInyector.AMANECIDA_WEAPON currentWeapon;

	private List<AMANECIDA_ATTACKS> availableAttacks = new List<AMANECIDA_ATTACKS>
	{
		AMANECIDA_ATTACKS.AXE_DualThrow,
		AMANECIDA_ATTACKS.AXE_FlyAndSpin,
		AMANECIDA_ATTACKS.AXE_JumpSmash
	};

	private AMANECIDA_ATTACKS lastAttack;

	private AMANECIDA_ATTACKS attackBeforeLastAttack;

	private Dictionary<AMANECIDA_ATTACKS, Func<EnemyAction>> actionsDictionary = new Dictionary<AMANECIDA_ATTACKS, Func<EnemyAction>>();

	private EnemyAction currentAction;

	private List<AmanecidasFightParameters> currentFightParameterList;

	private AutonomousAgent agent;

	private RaycastHit2D[] results;

	private int _currentHurtHits;

	private AmanecidasFightParameters currentFightParameters;

	private float floatingCounter;

	private int actionsBeforeShieldRecharge;

	private float damageWhileRecharging;

	private bool _interruptable;

	private float extraRecoverySeconds;

	private Healing penitentHealing;

	private bool lastShieldRechargeWasInterrupted;

	private float throwbackExtraTime;

	private float maxThrowbackExtraTime = 0.5f;

	private bool dodge;

	private Vector2 bowAimTarget;

	private Dictionary<string, float> damageParameters;

	private float dodgeCooldown;

	private List<BulletTimeProjectile> bullets;

	private List<BulletTimeProjectile> multiBullets;

	private float lastX;

	private List<BossSpawnedAreaAttack> mines;

	private AmanecidasMeleeAttack currentMeleeAttack;

	private int numActiveFlamePillarPairs;

	public bool needsToSwapWeapon;

	private AmanecidaArena.WEAPON_FIGHT_PHASE laudesBowFightPhase;

	private int actionsBeforeMovingBattlebounds = 5;

	private const string AXE_DAMAGE = "AXE_DMG";

	public Amanecidas Amanecidas { get; set; }

	private void Start()
	{
		Amanecidas = (Amanecidas)Entity;
		agent = GetComponent<AutonomousAgent>();
		waitBetweenActions_EA = new WaitSeconds_EnemyAction();
		hurtDisplacement_EA = new HurtDisplacement_EnemyAction();
		jumpBackNShoot_EA = new JumpBackAndShoot_EnemyAction();
		shootRicochetArrow_EA = new ShootRicochetArrow_EnemyAction();
		shootLaserArrow_EA = new ShootLaserArrow_EnemyAction();
		shootMineArrows_EA = new ShootMineArrows_EnemyAction();
		freezeTimeBlinkShots_EA = new FreezeTimeBlinkShots_EnemyAction();
		freezeTimeMultiShots_EA = new FreezeTimeMultiShots_EnemyAction();
		fastShot_EA = new FastShot_EnemyAction();
		fastShots_EA = new FastShots_EnemyAction();
		chargedShot_EA = new ChargedShot_EnemyAction();
		spikesBlinkShots_EA = new SpikesBlinkShots_EnemyAction();
		freezeTimeNRicochetShots_ECA = new FreezeTimeNRicochetShots_EnemyComboAction();
		shootFrozenLances_EA = new ShootFrozenLances_EnemyAction();
		doubleShootFrozenLances_EA = new DoubleShootFrozenLances_EnemyAction();
		horizontalBlinkDashes_EA = new HorizontalBlinkDashes_EnemyAction();
		multiFrontalDash_EA = new MultiFrontalDash_EnemyAction();
		diagonalBlinkDashes_EA = new DiagonalBlinkDashes_EnemyAction();
		freezeTimeNHorizontalDashes_ECA = new FreezeTimeNHorizontalDashes_EnemyComboAction();
		dualAxeThrow_EA = new LaunchTwoAxesHorizontal_EnemyAction();
		dualAxeFlyingThrow_EA = new FlyAndLaunchTwoAxes_EnemyAction();
		spinAxesAround_EA = new SpinAxesAround_EnemyAction();
		followAndSpinAxes_EA = new FollowSplineAndSpinAxesAround_EnemyAction();
		jumpSmash_EA = new JumpSmash_EnemyAction();
		multiStompNLavaBalls_ECA = new MultiStompNLavaBalls_EnemyComboAction();
		blink_EA = new BlinkToPoint_EnemyAction();
		blinkDash_EA = new BlinkAndDashToPenitent_EnemyAction();
		chainDash_EA = new ChainDash_EnemyAction();
		falcataSlashProjectile_EA = new FalcataSlashProjectile_EnemyAction();
		falcataSlashBarrage_EA = new FalcataSlashBarrage_EnemyAction();
		meleeAttack_EA = new MeleeAttackTowardsPenitent_EnemyAction();
		meleeProjectile_EA = new MeleeAttackProjectile_EnemyAction();
		ghostProjectile_EA = new GhostProjectile_EnemyAction();
		recoverShield_EA = new RechargeShield_EnemyAction();
		stompAttack_EA = new StompAttack_EnemyAction();
		launchAxesToPenitent_EA = new LaunchAxesToPenitent_EnemyAction();
		launchCrawlerAxesToPenitent_EA = new LaunchCrawlerAxesToPenitent_EnemyAction();
		launchBallsToPenitent_EA = new LaunchBallsToPenitent_EnemyAction();
		multiStompAttack_EA = new MultiStompAttack_EnemyAction();
		changeWeapon_EA = new ChangeWeapon_EnemyAction();
		quickLunge_EA = new QuickLunge_EnemyAction();
		falcataSlashStorm_ECA = new FalcataSlashStorm_EnemyComboAction();
		intro_EA = new Intro_EnemyAction();
		death_EA = new Death_EnemyAction();
		falcataCounter_EA = new DodgeAndCounterAttack_EnemyAction();
		shieldShockwave_EA = new ShieldShockwave();
		chargeEnergy_EA = new EnergyChargePeriod_EnemyAction();
		moveBattleBounds_EA = new MoveBattleBounds_EnemyAction();
		spinAttack = new FalcataSlashOnFloor_EnemyAction();
		hurt_EA = new Hurt_EnemyAction();
		damageParameters = new Dictionary<string, float>();
		SetBalanceData();
		bullets = new List<BulletTimeProjectile>();
		multiBullets = new List<BulletTimeProjectile>();
		mines = new List<BossSpawnedAreaAttack>();
		PoolManager.Instance.CreatePool(amanecidaAxePrefab, 2);
		PoolManager.Instance.CreatePool(poolSplinePrefab, 2);
		PoolManager.Instance.CreatePool(spikePrefab.gameObject, 30);
		PoolManager.Instance.CreatePool(vortexVFX, 2);
		PoolManager.Instance.CreatePool(dustVFX, 2);
		PoolManager.Instance.CreatePool(shieldShockwave, 3);
		PoolManager.Instance.CreatePool(clonePrefab, 2);
		results = new RaycastHit2D[1];
		SpawnBothAxes();
		InitAI();
		InitActionDictionary();
		InitCombatArea();
		Amanecidas.AnimatorInyector.SetAmanecidaWeapon(AmanecidasAnimatorInyector.AMANECIDA_WEAPON.HAND);
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnDestroy()
	{
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		penitentHealing = penitent.GetComponentInChildren<Healing>();
	}

	public void SetWeapon(AmanecidasAnimatorInyector.AMANECIDA_WEAPON wpn)
	{
		availableAttacks.Clear();
		currentWeapon = wpn;
		if (Amanecidas == null)
		{
			Amanecidas = (Amanecidas)Entity;
		}
		if (Amanecidas.IsLaudes)
		{
			if (currentFightParameterList == null)
			{
				currentFightParameterList = laudesFightParameters;
				currentFightParameters = currentFightParameterList[0];
			}
			availableAttacks = laudesAttackConfigData.GetAttackIdsByWeapon(wpn, useHP: false);
		}
		else
		{
			switch (currentWeapon)
			{
			case AmanecidasAnimatorInyector.AMANECIDA_WEAPON.HAND:
				currentFightParameterList = falcataFightParameters;
				break;
			case AmanecidasAnimatorInyector.AMANECIDA_WEAPON.AXE:
				currentFightParameterList = axeFightParameters;
				lastAttack = AMANECIDA_ATTACKS.AXE_FlameBlade;
				attackBeforeLastAttack = AMANECIDA_ATTACKS.AXE_FlameBlade;
				break;
			case AmanecidasAnimatorInyector.AMANECIDA_WEAPON.LANCE:
				currentFightParameterList = lanceFightParameters;
				lastAttack = AMANECIDA_ATTACKS.LANCE_HorizontalBlinkDashes;
				attackBeforeLastAttack = AMANECIDA_ATTACKS.LANCE_HorizontalBlinkDashes;
				break;
			case AmanecidasAnimatorInyector.AMANECIDA_WEAPON.BOW:
				currentFightParameterList = bowFightParameters;
				lastAttack = AMANECIDA_ATTACKS.BOW_FastShot;
				attackBeforeLastAttack = AMANECIDA_ATTACKS.BOW_FastShot;
				break;
			case AmanecidasAnimatorInyector.AMANECIDA_WEAPON.SWORD:
				currentFightParameterList = falcataFightParameters;
				lastAttack = AMANECIDA_ATTACKS.FALCATA_SpinAttack;
				attackBeforeLastAttack = AMANECIDA_ATTACKS.FALCATA_SpinAttack;
				break;
			}
			currentFightParameters = currentFightParameterList[0];
			availableAttacks = attackConfigData.GetAttackIdsByWeapon(wpn, useHP: true, GetHpPercentage());
		}
		currentMeleeAttack = ((currentWeapon != AmanecidasAnimatorInyector.AMANECIDA_WEAPON.AXE) ? meleeFalcataAttack : meleeAxeAttack);
	}

	private float GetHpPercentage()
	{
		return Amanecidas.GetHpPercentage();
	}

	private void InitAI()
	{
		stFloating = new Amanecidas_StFloating();
		stAction = new Amanecidas_StAction();
		stDeath = new Amanecidas_StAction();
		stIntro = new Amanecidas_StAction();
		stHurt = new Amanecidas_StHurt();
		stRecharge = new Amanecidas_StRecharging();
		_fsm = new StateMachine<AmanecidasBehaviour>(this, stIntro);
		actionsBeforeShieldRecharge = currentFightParameters.maxActionsBeforeShieldRecharge;
	}

	private void InitCombatArea()
	{
		combatAreaParent.SetParent(null);
		combatAreaParent.transform.position = battleBounds.center;
	}

	private void InitActionDictionary()
	{
		actionsDictionary.Add(AMANECIDA_ATTACKS.AXE_FlyAndSpin, LaunchAction_FlyAndSpin);
		actionsDictionary.Add(AMANECIDA_ATTACKS.AXE_DualThrow, LaunchAction_DualThrow);
		actionsDictionary.Add(AMANECIDA_ATTACKS.AXE_JumpSmash, LaunchAction_JumpSmash);
		actionsDictionary.Add(AMANECIDA_ATTACKS.AXE_JumpSmashWithPillars, LaunchAction_JumpSmashWithPillars);
		actionsDictionary.Add(AMANECIDA_ATTACKS.AXE_DualThrowCross, LaunchAction_DualThrowCross);
		actionsDictionary.Add(AMANECIDA_ATTACKS.AXE_FlyAndToss, LaunchAction_FlyAndToss);
		actionsDictionary.Add(AMANECIDA_ATTACKS.AXE_MeleeAttack, LaunchAction_AxeMeleeAttack);
		actionsDictionary.Add(AMANECIDA_ATTACKS.AXE_FlameBlade, LaunchAction_FlameBlade);
		actionsDictionary.Add(AMANECIDA_ATTACKS.AXE_FollowAndAxeToss, LaunchAction_FollowAndTossAxes);
		actionsDictionary.Add(AMANECIDA_ATTACKS.AXE_FollowAndCrawlerAxeToss, LaunchAction_FollowAndTossCrawlerAxes);
		actionsDictionary.Add(AMANECIDA_ATTACKS.AXE_FollowAndLavaBall, LaunchAction_FollowAndLavaBall);
		actionsDictionary.Add(AMANECIDA_ATTACKS.AXE_COMBO_StompNLavaBalls, LaunchComboAction_StompNLavaBalls);
		actionsDictionary.Add(AMANECIDA_ATTACKS.BOW_FreezeTimeBlinkShots, LaunchAction_FreezeTimeBlinkShots);
		actionsDictionary.Add(AMANECIDA_ATTACKS.BOW_RicochetShot, LaunchAction_RicochetShot);
		actionsDictionary.Add(AMANECIDA_ATTACKS.BOW_MineShots, LaunchAction_MineShots);
		actionsDictionary.Add(AMANECIDA_ATTACKS.BOW_FreezeTimeMultiShots, LaunchAction_FreezeTimeMultiShots);
		actionsDictionary.Add(AMANECIDA_ATTACKS.BOW_FastShot, LaunchAction_FastShot);
		actionsDictionary.Add(AMANECIDA_ATTACKS.BOW_FastShots, LaunchAction_FastShots);
		actionsDictionary.Add(AMANECIDA_ATTACKS.BOW_ChargedShot, LaunchAction_ChargedShot);
		actionsDictionary.Add(AMANECIDA_ATTACKS.BOW_SpikesBlinkShot, LaunchAction_SpikesBlinkShot);
		actionsDictionary.Add(AMANECIDA_ATTACKS.BOW_COMBO_FreezeTimeNRicochetShots, LaunchComboAction_FreezeTimeNRicochetShots);
		actionsDictionary.Add(AMANECIDA_ATTACKS.BOW_LaserShot, LaunchAction_LaserShot);
		actionsDictionary.Add(AMANECIDA_ATTACKS.LANCE_BlinkDash, LaunchAction_LanceBlinkDash);
		actionsDictionary.Add(AMANECIDA_ATTACKS.LANCE_FreezeTimeLances, LaunchAction_FreezeTimeLances);
		actionsDictionary.Add(AMANECIDA_ATTACKS.LANCE_FreezeTimeLancesOnPenitent, LaunchAction_FreezeTimeLancesOnPenitent);
		actionsDictionary.Add(AMANECIDA_ATTACKS.LANCE_FreezeTimeHail, LaunchAction_FreezeTimeHail);
		actionsDictionary.Add(AMANECIDA_ATTACKS.LANCE_JumpBackAndDash, LaunchAction_JumpBackNDash);
		actionsDictionary.Add(AMANECIDA_ATTACKS.LANCE_HorizontalBlinkDashes, LaunchAction_HorizontalBlinkDashes);
		actionsDictionary.Add(AMANECIDA_ATTACKS.LANCE_MultiFrontalDash, LaunchAction_MultiFrontalDash);
		actionsDictionary.Add(AMANECIDA_ATTACKS.LANCE_DiagonalBlinkDashes, LaunchAction_DiagonalBlinkDashes);
		actionsDictionary.Add(AMANECIDA_ATTACKS.LANCE_COMBO_FreezeTimeNHorizontalDashes, LaunchComboAction_FreezeTimeNHorizontalDashes);
		actionsDictionary.Add(AMANECIDA_ATTACKS.FALCATA_BlinkDash, LaunchAction_FalcataBlinkBehindAndDash);
		actionsDictionary.Add(AMANECIDA_ATTACKS.FALCATA_MeleeAttack, LaunchAction_FalcataMeleeAttack);
		actionsDictionary.Add(AMANECIDA_ATTACKS.FALCATA_SpinAttack, LaunchAction_SpinAttack);
		actionsDictionary.Add(AMANECIDA_ATTACKS.FALCATA_SlashProjectile, LaunchAction_FalcataSlashProjectile);
		actionsDictionary.Add(AMANECIDA_ATTACKS.FALCATA_SlashBarrage, LaunchAction_FalcataSlashBarrage);
		actionsDictionary.Add(AMANECIDA_ATTACKS.FALCATA_QuickLunge, LaunchAction_MultiLunge);
		actionsDictionary.Add(AMANECIDA_ATTACKS.FALCATA_COMBO_STORM, LaunchAction_FalcataCombo);
		actionsDictionary.Add(AMANECIDA_ATTACKS.FALCATA_CounterAttack, LaunchAction_Counter);
		actionsDictionary.Add(AMANECIDA_ATTACKS.FALCATA_ChainDash, LaunchAction_ChainDash);
		actionsDictionary.Add(AMANECIDA_ATTACKS.FALCATA_NoxiousBlade, LaunchAction_NoxiousBlade);
		actionsDictionary.Add(AMANECIDA_ATTACKS.COMMON_BlinkAway, LaunchAction_BlinkAway);
		actionsDictionary.Add(AMANECIDA_ATTACKS.COMMON_RechargeShield, LaunchAction_RechargeShield);
		actionsDictionary.Add(AMANECIDA_ATTACKS.COMMON_Intro, LaunchAction_Intro);
		actionsDictionary.Add(AMANECIDA_ATTACKS.COMMON_StompAttack, LaunchAction_StompSmash);
		actionsDictionary.Add(AMANECIDA_ATTACKS.COMMON_ChangeWeapon, LaunchAction_ChangeWeapon);
		actionsDictionary.Add(AMANECIDA_ATTACKS.COMMON_Death, LaunchAction_Death);
		actionsDictionary.Add(AMANECIDA_ATTACKS.COMMON_MoveBattleBounds, LaunchAction_MoveBattleBounds);
	}

	private void SpawnBothAxes()
	{
		SpawnAxe(Vector2.right * 2f);
		SpawnAxe(Vector2.left * 2f);
		for (int i = 0; i < 2; i++)
		{
			axes[i].target = axeTargets[i];
		}
	}

	private void SpawnAxe(Vector2 dir)
	{
		if (axes == null)
		{
			axes = new List<AmanecidaAxeBehaviour>();
		}
		AmanecidaAxeBehaviour component = PoolManager.Instance.ReuseObject(amanecidaAxePrefab, (Vector2)base.transform.position + Vector2.up * 1f + dir, Quaternion.identity).GameObject.GetComponent<AmanecidaAxeBehaviour>();
		component.GetComponentInChildren<AmanecidasAnimationEventsController>().amanecidasBehaviour = this;
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
		component.SetVisible(v: false);
	}

	private void SetBalanceData()
	{
		SetDamageParameter("AXE_DMG", 20f);
	}

	public void SetDamageParameter(string key, float value)
	{
		damageParameters[key] = value;
	}

	private float GetDamageParameter(string key)
	{
		return damageParameters[key];
	}

	public bool CanBeInterrupted()
	{
		return _interruptable && lastAttack != AMANECIDA_ATTACKS.COMMON_RechargeShield;
	}

	public void SetInterruptable(bool state)
	{
		_interruptable = state;
	}

	public void ActivateBeam()
	{
		float delay = 0f;
		beamLauncher.gameObject.SetActive(value: true);
		beamLauncher.ActivateDelayedBeam(delay, warningAnimation: true);
		Debug.Log("ACTIVATING BEAM");
	}

	public void DeactivateBeam(float delay = 0.3f)
	{
		Debug.Log("DEACTIVATING BEAM");
		beamLauncher.ActivateBeamAnimation(active: false);
		StartCoroutine(DelayedBeamDeactivation(delay));
	}

	private IEnumerator DelayedBeamDeactivation(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		beamLauncher.gameObject.SetActive(value: false);
	}

	private bool DoCrystalLancesPlatformsExist()
	{
		return UnityEngine.Object.FindObjectsOfType<AmanecidaCrystal>().Length > 0;
	}

	private void DestroyCrystalLancesPlatforms(float multiplierToTtl, float maxTtl)
	{
		UnityEngine.Object.FindObjectsOfType<AmanecidaCrystal>().ToList().ForEach(delegate(AmanecidaCrystal x)
		{
			x.MultiplyCurrentTimeToLive(multiplierToTtl, maxTtl);
		});
	}

	private void UpdateCooldowns()
	{
		if (dodgeCooldown > 0f)
		{
			dodgeCooldown = Mathf.Clamp(dodgeCooldown - Time.deltaTime, 0f, dodgeMaxCooldown);
		}
	}

	private void CheckDebugActions()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		Dictionary<KeyCode, AMANECIDA_ATTACKS> debugActions = amanecidaAttackScriptableConfig.debugActions;
		if (debugActions == null)
		{
			return;
		}
		foreach (KeyCode key in debugActions.Keys)
		{
			if (Input.GetKeyDown(key))
			{
				LaunchAction(debugActions[key]);
			}
		}
	}

	private void UpdateThrowbackCounter()
	{
		if (IsPenitentThrown())
		{
			throwbackExtraTime = maxThrowbackExtraTime;
		}
	}

	private void Update()
	{
		_fsm.DoUpdate();
		UpdateCooldowns();
		UpdateAnimationParameters();
		UpdateThrowbackCounter();
	}

	public IEnumerator WaitForState(State<AmanecidasBehaviour> st)
	{
		while (!_fsm.IsInState(st))
		{
			yield return null;
		}
	}

	public void ActivateFloating(bool active)
	{
		agent.enabled = active;
		if (active)
		{
			agent.SetConfig(floatingConfig);
		}
		else
		{
			agent.SetConfig(actionConfig);
		}
	}

	public void CheckCounterAttack()
	{
		if (currentWeapon == AmanecidasAnimatorInyector.AMANECIDA_WEAPON.SWORD)
		{
			SetDodge(active: true);
		}
	}

	public bool IsDodging()
	{
		return dodge && dodgeCooldown == 0f;
	}

	public void OnCrystalExplode(AmanecidaCrystal crystal)
	{
		verticalCrystalBeam.SummonAreaOnPoint(crystal.transform.position, 270f);
	}

	public void SetDodge(bool active)
	{
		dodge = active;
		Amanecidas.sparksOnImpact = !active;
	}

	public void UpdateFloatingCounter()
	{
		floatingCounter -= Time.deltaTime;
		if (floatingCounter < 0f)
		{
			floatingCounter = UnityEngine.Random.Range(minFloatingPointCD, maxFloatingPointCD);
			ChangeFloatingPoint(GetNewValidFloatPoint());
		}
	}

	private bool IsFarEnoughToTurn()
	{
		float num = 4f;
		Arrive component = GetComponent<Arrive>();
		return Vector2.Distance(component.target, base.transform.position) > num;
	}

	public void UpdateAnimationParameters()
	{
	}

	private bool IsWieldingAxe()
	{
		return Amanecidas.AnimatorInyector.GetWeapon() == AmanecidasAnimatorInyector.AMANECIDA_WEAPON.AXE;
	}

	public void ApplyStuckOffset()
	{
		Vector3 vector = new Vector3(-0.15f * (float)GetDirFromOrientation(), 0.5f, 0f);
		base.transform.position += vector;
	}

	private void ChangeFloatingPoint(Vector2 newPoint)
	{
		agent.GetComponent<Arrive>().target = newPoint;
	}

	private Vector2 GetNewValidFloatPoint()
	{
		Vector2 vector = RandomPointInsideRect(battleBounds);
		Vector2 a = vector - (Vector2)agent.GetComponent<Arrive>().target;
		int num = 0;
		int num2 = 10;
		while (num < num2 && Vector2.SqrMagnitude(a) < 2f)
		{
			vector = RandomPointInsideRect(battleBounds);
			a = vector - (Vector2)agent.GetComponent<Arrive>().target;
		}
		return vector;
	}

	private Vector2 RandomPointInsideRect(Rect r)
	{
		return new Vector2(UnityEngine.Random.Range(r.xMin, r.xMin + r.width), UnityEngine.Random.Range(r.yMin, r.yMin + r.height));
	}

	private void ResetShieldActions()
	{
		actionsBeforeShieldRecharge = currentFightParameters.maxActionsBeforeShieldRecharge;
	}

	private bool ShouldRechargeShield()
	{
		return Amanecidas.shieldCurrentHP <= 0f && actionsBeforeShieldRecharge <= 0;
	}

	public void InitShieldRechargeDamage()
	{
		damageWhileRecharging = currentFightParameters.maxDamageBeforeInterruptingRecharge;
	}

	public void CheckShieldRechargeDamage()
	{
		if (damageWhileRecharging < 0f)
		{
			Debug.Log("SHIELD RECHARGE INTERRUPTED!");
			InterruptShieldRecharge();
		}
	}

	private void InterruptShieldRecharge()
	{
		StopCurrentAction();
		ResetShieldActions();
		Amanecidas.AnimatorInyector.SetRecharging(active: false);
		_fsm.ChangeState(stHurt);
		Amanecidas.shield.InterruptShieldRecharge();
		Amanecidas.Audio.StopShieldRecharge_AUDIO();
		currentAction = LaunchAction_Hurt(isLastHurt: false);
		currentAction.OnActionEnds -= OnHurtActionEnds;
		currentAction.OnActionEnds += OnHurtActionEnds;
		lastShieldRechargeWasInterrupted = true;
	}

	private List<AMANECIDA_ATTACKS> GetFilteredAttacks(List<AMANECIDA_ATTACKS> originalList)
	{
		List<AMANECIDA_ATTACKS> list = new List<AMANECIDA_ATTACKS>(originalList);
		if (actionsBeforeShieldRecharge == currentFightParameters.maxActionsBeforeShieldRecharge && Amanecidas.shieldCurrentHP <= 0f && !lastShieldRechargeWasInterrupted && !Amanecidas.IsLaudes)
		{
			list.RemoveAll((AMANECIDA_ATTACKS x) => x < (AMANECIDA_ATTACKS)100);
		}
		else
		{
			list.RemoveAll((AMANECIDA_ATTACKS x) => x > (AMANECIDA_ATTACKS)100);
			switch (currentWeapon)
			{
			case AmanecidasAnimatorInyector.AMANECIDA_WEAPON.AXE:
				FilterAxeAttacks(list);
				break;
			case AmanecidasAnimatorInyector.AMANECIDA_WEAPON.LANCE:
				FilterLanceAttacks(list);
				break;
			case AmanecidasAnimatorInyector.AMANECIDA_WEAPON.BOW:
				FilterBowAttacks(list);
				break;
			case AmanecidasAnimatorInyector.AMANECIDA_WEAPON.SWORD:
				FilterFalcataAttacks(list);
				break;
			}
			if (list.Count > 1)
			{
				list.Remove(lastAttack);
			}
			else if (list.Count == 0)
			{
				Debug.Log(string.Concat("We have filtered the attacks a bit too much! lastAttack: ", lastAttack, ", attackBeforeLastAttack: ", attackBeforeLastAttack));
				list.Add(attackBeforeLastAttack);
			}
		}
		return list;
	}

	private void FilterGenericWeapon(List<AMANECIDA_ATTACKS> attacks, AmanecidasAnimatorInyector.AMANECIDA_WEAPON wpn)
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig atkConfig = amanecidaAttackScriptableConfig.GetAttackConfig(wpn, lastAttack);
		if (atkConfig.cantBeFollowedBy != null && atkConfig.cantBeFollowedBy.Count > 0)
		{
			attacks.RemoveAll((AMANECIDA_ATTACKS x) => atkConfig.cantBeFollowedBy.Contains(x));
		}
		if (atkConfig.alwaysFollowedBy != null && atkConfig.alwaysFollowedBy.Count > 0)
		{
			attacks.RemoveAll((AMANECIDA_ATTACKS x) => !atkConfig.alwaysFollowedBy.Contains(x));
		}
	}

	private void FilterAxeAttacks(List<AMANECIDA_ATTACKS> attacks)
	{
		FilterGenericWeapon(attacks, AmanecidasAnimatorInyector.AMANECIDA_WEAPON.AXE);
		if (IsWieldingAxe())
		{
			Debug.Log("REMOVING ATTACKS WITHOUT WIELDING AXES");
			attacks.Remove(AMANECIDA_ATTACKS.AXE_DualThrow);
			attacks.Remove(AMANECIDA_ATTACKS.AXE_DualThrowCross);
			attacks.Remove(AMANECIDA_ATTACKS.AXE_FollowAndLavaBall);
			attacks.Remove(AMANECIDA_ATTACKS.AXE_FlyAndSpin);
			attacks.Remove(AMANECIDA_ATTACKS.AXE_JumpSmash);
		}
		else
		{
			Debug.Log("REMOVING ATTACKS WIELDING AXE");
			attacks.Remove(AMANECIDA_ATTACKS.AXE_MeleeAttack);
			attacks.Remove(AMANECIDA_ATTACKS.AXE_FollowAndAxeToss);
			attacks.Remove(AMANECIDA_ATTACKS.AXE_FollowAndCrawlerAxeToss);
			attacks.Remove(AMANECIDA_ATTACKS.AXE_FlameBlade);
			attacks.Remove(AMANECIDA_ATTACKS.COMMON_StompAttack);
		}
		if (Amanecidas.IsLaudes)
		{
			return;
		}
		int num = (int)(Amanecidas.CurrentLife * 3f / Amanecidas.Stats.Life.Final);
		if (numActiveFlamePillarPairs < 3 - num)
		{
			attacks.RemoveAll((AMANECIDA_ATTACKS x) => x != AMANECIDA_ATTACKS.AXE_JumpSmashWithPillars);
		}
		else
		{
			attacks.Remove(AMANECIDA_ATTACKS.AXE_JumpSmashWithPillars);
		}
	}

	private void FilterLanceAttacks(List<AMANECIDA_ATTACKS> attacks)
	{
		FilterGenericWeapon(attacks, AmanecidasAnimatorInyector.AMANECIDA_WEAPON.LANCE);
		if (IsPenitentInTop())
		{
			attacks.Remove(AMANECIDA_ATTACKS.COMMON_StompAttack);
			attacks.Remove(AMANECIDA_ATTACKS.LANCE_MultiFrontalDash);
		}
		else
		{
			attacks.Remove(AMANECIDA_ATTACKS.LANCE_FreezeTimeHail);
		}
		if (actionsBeforeShieldRecharge <= 1)
		{
			attacks.Remove(AMANECIDA_ATTACKS.LANCE_FreezeTimeLances);
			attacks.Remove(AMANECIDA_ATTACKS.LANCE_FreezeTimeLancesOnPenitent);
		}
		if (attacks.Count > 1)
		{
			attacks.Remove(attackBeforeLastAttack);
		}
	}

	private void FilterBowAttacks(List<AMANECIDA_ATTACKS> attacks)
	{
		FilterGenericWeapon(attacks, AmanecidasAnimatorInyector.AMANECIDA_WEAPON.BOW);
		if (Amanecidas.IsLaudes)
		{
			if (CanUseLaserShotAttack())
			{
				attacks.RemoveAll((AMANECIDA_ATTACKS x) => x != AMANECIDA_ATTACKS.BOW_LaserShot);
			}
			else
			{
				attacks.Remove(AMANECIDA_ATTACKS.BOW_LaserShot);
			}
		}
		else if (lastAttack != AMANECIDA_ATTACKS.BOW_FastShot && lastAttack != AMANECIDA_ATTACKS.BOW_FastShot)
		{
			if (penitentHealing != null && penitentHealing.IsHealing)
			{
				attacks.RemoveAll((AMANECIDA_ATTACKS x) => x != AMANECIDA_ATTACKS.BOW_FastShot);
			}
			else
			{
				if (!CanUseFastShotsAttacks() && CanRemoveFastShotsAttacks(attacks))
				{
					RemoveFastShotsAttacks(attacks);
				}
				attacks.Remove(attackBeforeLastAttack);
			}
		}
		switch (lastAttack)
		{
		}
	}

	public bool CanUseLaserShotAttack()
	{
		return Core.Logic.Penitent.GetPosition().y < battleBounds.center.y - battleBounds.height / 1.8f;
	}

	private void FilterFalcataAttacks(List<AMANECIDA_ATTACKS> attacks)
	{
		FilterGenericWeapon(attacks, AmanecidasAnimatorInyector.AMANECIDA_WEAPON.SWORD);
		if (attacks.Count > 1)
		{
			attacks.Remove(attackBeforeLastAttack);
		}
	}

	private bool CanUseFastShotsAttacks()
	{
		return base.transform.position.y > Core.Logic.Penitent.GetPosition().y + 1f && base.transform.position.y < battleBounds.yMax + 3f && (base.transform.position - Core.Logic.Penitent.GetPosition()).magnitude > 2.5f;
	}

	private bool CanRemoveFastShotsAttacks(List<AMANECIDA_ATTACKS> attacks)
	{
		return attacks.Count > 2 || attacks.Any((AMANECIDA_ATTACKS x) => x != AMANECIDA_ATTACKS.BOW_FastShot && x != AMANECIDA_ATTACKS.BOW_FastShots);
	}

	private void RemoveFastShotsAttacks(List<AMANECIDA_ATTACKS> attacks)
	{
		attacks.RemoveAll((AMANECIDA_ATTACKS x) => x == AMANECIDA_ATTACKS.BOW_FastShot || x == AMANECIDA_ATTACKS.BOW_FastShots);
	}

	private int RandomizeUsingWeights(List<AMANECIDA_ATTACKS> filteredAtks)
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		List<float> weights = amanecidaAttackScriptableConfig.GetWeights(filteredAtks, currentWeapon);
		float max = weights.Sum();
		float num = UnityEngine.Random.Range(0f, max);
		float num2 = 0f;
		for (int i = 0; i < filteredAtks.Count; i++)
		{
			num2 += weights[i];
			if (num2 > num)
			{
				return i;
			}
		}
		return 0;
	}

	private void LaunchAutomaticAction()
	{
		AMANECIDA_ATTACKS aMANECIDA_ATTACKS = AMANECIDA_ATTACKS.COMMON_BlinkAway;
		if (needsToSwapWeapon)
		{
			Debug.Log("<<<NEED TO SWAP WEAPON>>>");
			aMANECIDA_ATTACKS = AMANECIDA_ATTACKS.COMMON_ChangeWeapon;
		}
		else if (ShouldRechargeShield())
		{
			Debug.Log("<<<NEED TO RECHARGE SHIELD>>>");
			aMANECIDA_ATTACKS = AMANECIDA_ATTACKS.COMMON_RechargeShield;
		}
		else if (Amanecidas.IsLaudes && currentWeapon == AmanecidasAnimatorInyector.AMANECIDA_WEAPON.BOW && actionsBeforeMovingBattlebounds <= 0 && laudesBowFightPhase != AmanecidaArena.WEAPON_FIGHT_PHASE.THIRD)
		{
			Debug.Log("<<<NEED TO MOVE BATTLEBOUNDS>>>");
			aMANECIDA_ATTACKS = AMANECIDA_ATTACKS.COMMON_MoveBattleBounds;
		}
		else
		{
			List<AMANECIDA_ATTACKS> filteredAttacks = GetFilteredAttacks(availableAttacks);
			int index = RandomizeUsingWeights(filteredAttacks);
			aMANECIDA_ATTACKS = filteredAttacks[index];
			if (Amanecidas.shieldCurrentHP <= 0f)
			{
				actionsBeforeShieldRecharge--;
				if (Amanecidas.IsLaudes && currentWeapon == AmanecidasAnimatorInyector.AMANECIDA_WEAPON.BOW && laudesBowFightPhase != AmanecidaArena.WEAPON_FIGHT_PHASE.THIRD)
				{
					actionsBeforeMovingBattlebounds--;
				}
			}
		}
		LaunchAction(aMANECIDA_ATTACKS);
		attackBeforeLastAttack = lastAttack;
		lastAttack = aMANECIDA_ATTACKS;
	}

	protected void LaunchAction(AMANECIDA_ATTACKS action)
	{
		StopCurrentAction();
		DoWeaponSpecificOnLaunch();
		_fsm.ChangeState(stAction);
		currentAction = actionsDictionary[action]();
		currentAction.OnActionEnds += CurrentAction_OnActionEnds;
		currentAction.OnActionIsStopped += CurrentAction_OnActionStops;
	}

	private void StopCurrentAction()
	{
		if (currentAction != null)
		{
			currentAction.StopAction();
		}
	}

	private void DoWeaponSpecificOnLaunch()
	{
		foreach (AmanecidaAxeBehaviour axis in axes)
		{
		}
	}

	private void DoWeaponSpecificOnWait()
	{
		foreach (AmanecidaAxeBehaviour axis in axes)
		{
			axis.SetSeek(free: true);
		}
	}

	private bool IsPenitentInTop()
	{
		return Core.Logic.Penitent.transform.position.y > battleBounds.center.y - 1f;
	}

	public void StartCombat()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, new Core.SimpleEvent(Amanecidas.Audio.StopAll));
		StartCoroutine(StartCombatCoroutine());
	}

	private IEnumerator StartCombatCoroutine()
	{
		_fsm.ChangeState(stAction);
		if (Amanecidas.IsLaudes)
		{
			DoSummonWeaponAnimation();
		}
		yield return new WaitForSeconds(0.5f);
		extraRecoverySeconds = 1f;
		WaitBetweenActions();
	}

	public void SetExtraRecoverySeconds(float newRecovery)
	{
		extraRecoverySeconds = newRecovery;
	}

	private void WaitBetweenActions()
	{
		_fsm.ChangeState(stFloating);
		DoWeaponSpecificOnWait();
		StartWait(extraRecoverySeconds + currentFightParameters.minMaxWaitingTimeBetweenActions.x, extraRecoverySeconds + currentFightParameters.minMaxWaitingTimeBetweenActions.y);
		extraRecoverySeconds = 0f;
	}

	private void StartWait(float min, float max)
	{
		StopCurrentAction();
		LookAtPenitent();
		currentAction = waitBetweenActions_EA.StartAction(this, min, max);
		currentAction.OnActionEnds += CurrentAction_OnActionEnds;
	}

	private void CurrentAction_OnActionEnds(EnemyAction e)
	{
		e.OnActionEnds -= CurrentAction_OnActionEnds;
		e.OnActionIsStopped -= CurrentAction_OnActionStops;
		if (e != intro_EA && !_fsm.IsInState(stDeath))
		{
			if (e != waitBetweenActions_EA)
			{
				WaitBetweenActions();
				return;
			}
			SetGhostTrail(active: true);
			LaunchAutomaticAction();
		}
	}

	private void CurrentAction_OnActionStops(EnemyAction e)
	{
		Amanecidas.AnimatorInyector.ClearAll(includeTriggers: true);
		Amanecidas.AnimatorInyector.ClearRotationAndFlip();
		e.OnActionEnds -= CurrentAction_OnActionEnds;
		e.OnActionIsStopped -= CurrentAction_OnActionStops;
	}

	public void DoActivateCollisions(bool b)
	{
		Amanecidas.DamageArea.DamageAreaCollider.enabled = b;
	}

	public void SummonWeapon()
	{
		Amanecidas.AnimatorInyector.SetAmanecidaWeapon(currentWeapon);
	}

	public void OnMeleeAttackStarts()
	{
		currentMeleeAttack.damageOnEnterArea = true;
		currentMeleeAttack.CurrentWeaponAttack();
	}

	public void OnMeleeAttackFinished()
	{
		currentMeleeAttack.damageOnEnterArea = false;
	}

	[FoldoutGroup("Battle area", 0)]
	[Button("CenterBattleAreaHere", ButtonSizes.Medium)]
	private void CenterBattleAreaHere()
	{
		battleBounds.center = base.transform.position;
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

	public void DoSummonWeaponAnimation()
	{
		Amanecidas.AnimatorInyector.PlaySummonWeapon();
	}

	public void ShowSprites(bool show)
	{
		Amanecidas.AnimatorInyector.ShowSprites(show);
	}

	public void DoSpinAnimation(bool spin, bool doDamage = false)
	{
		if (spin)
		{
			Amanecidas.AnimatorInyector.SetMeleeHold(value: true);
			Amanecidas.AnimatorInyector.PlayStompAttack(doDamage);
		}
		else
		{
			Amanecidas.AnimatorInyector.SetMeleeHold(value: false);
		}
	}

	public void OnLanceDashAdvance(float value)
	{
		float num = 2f;
		if (Mathf.Abs(lastX - base.transform.position.x) >= num)
		{
			SummonSpike(base.transform.position);
			lastX = base.transform.position.x;
		}
	}

	public void DoSmallJumpSmash(bool usePillars)
	{
		Vector2 pointBelowPenitent = GetPointBelowPenitent(stopOnOneWayDowns: false);
		DoSmallJumpSmashToPoint(pointBelowPenitent, usePillars);
	}

	public void DoSmallJumpSmashToPoint(Vector2 p, bool usePillars)
	{
		ClearJumpEvents();
		jumpAttack.Use(base.transform, p);
		if (usePillars)
		{
			jumpAttack.OnJumpLanded += OnSmallJumpSmashLanded;
		}
		else
		{
			jumpAttack.OnJumpLanded += Amanecidas.Audio.PlayGroundAttack_AUDIO;
		}
	}

	public void DoLavaBallJumpSmash()
	{
		ClearJumpEvents();
		bool flag = false;
		flag = ((!Mathf.Approximately(base.transform.position.x, battleBounds.center.x)) ? (base.transform.position.x < battleBounds.center.x) : ((double)UnityEngine.Random.value < 0.5));
		Vector2 zero = Vector2.zero;
		zero = ((!flag) ? new Vector2(battleBounds.xMin, battleBounds.yMin) : new Vector2(battleBounds.xMax, battleBounds.yMin));
		jumpAttack.Use(base.transform, GetPointBelow(zero));
		jumpAttack.OnJumpLanded += OnLavaBallJumpSmashLanded;
	}

	public void DoJumpSmash()
	{
		ClearJumpEvents();
		Vector2 pointBelowPenitent = GetPointBelowPenitent(stopOnOneWayDowns: false);
		jumpAttack.Use(base.transform, pointBelowPenitent);
		jumpAttack.OnJumpLanded += OnJumpSmashLanded;
	}

	public void DoJumpSmashWithPillars()
	{
		ClearJumpEvents();
		Vector2 pointBelowPenitent = GetPointBelowPenitent(stopOnOneWayDowns: false);
		pointBelowPenitent.x = Mathf.Clamp(pointBelowPenitent.x, battleBounds.xMin + 1f, battleBounds.xMax - 1f);
		jumpAttack.Use(base.transform, pointBelowPenitent);
		jumpAttack.OnJumpLanded += OnJumpSmashWithPillarsLanded;
	}

	private void ClearJumpEvents()
	{
		jumpAttack.OnJumpLanded -= OnJumpSmashLanded;
		jumpAttack.OnJumpLanded -= OnSmallJumpSmashLanded;
		jumpAttack.OnJumpLanded -= OnJumpSmashWithPillarsLanded;
		jumpAttack.OnJumpLanded -= OnLavaBallJumpSmashLanded;
		jumpAttack.OnJumpLanded -= Amanecidas.Audio.PlayGroundAttack_AUDIO;
	}

	private void OnJumpSmashLanded()
	{
		ShakeWave();
		SpikeWave(base.transform.position, 0.6f, 15, doShockWaves: true, 1.1f);
		shockwave.SummonAreaOnPoint(base.transform.position);
	}

	private void OnJumpSmashWithPillarsLanded()
	{
		ShakeWave();
		SpikeWave(base.transform.position, 1f, 15);
		numActiveFlamePillarPairs++;
		GameObject gameObject = verticalBlastAxeAttack.SummonAreaOnPoint(new Vector2(battleBounds.xMin - 1f, battleBounds.yMin), 0f, 1f, IncreaseBattleBoundsWidth);
		gameObject.GetComponent<Entity>().SetOrientation(EntityOrientation.Right, allowFlipRenderer: true, searchForRenderer: true);
		gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder = 4 - numActiveFlamePillarPairs;
		gameObject = verticalBlastAxeAttack.SummonAreaOnPoint(new Vector2(battleBounds.xMax + 1f, battleBounds.yMin));
		gameObject.GetComponent<Entity>().SetOrientation(EntityOrientation.Left, allowFlipRenderer: true, searchForRenderer: true);
		gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder = 4 - numActiveFlamePillarPairs;
		DecreaseBattleBoundsWidth();
		shockwave.SummonAreaOnPoint(base.transform.position);
	}

	private void IncreaseBattleBoundsWidth()
	{
		float num = 2f;
		battleBounds.center = new Vector2(battleBounds.center.x - num / 2f, battleBounds.center.y);
		battleBounds.width += num;
	}

	private void DecreaseBattleBoundsWidth()
	{
		float num = 2f;
		battleBounds.center = new Vector2(battleBounds.center.x + num / 2f, battleBounds.center.y);
		battleBounds.width -= num;
	}

	public void SpikeWave(Vector2 center, float heightPercentage = 1f, int n = 14, bool doShockWaves = true, float totalSeconds = 1.5f)
	{
		int num = 1;
		float num2 = 1f;
		for (int i = 0; i < 2; i++)
		{
			if (doShockWaves)
			{
				shockwave.SummonAreas(center, Vector2.right * num);
			}
			for (int j = 0; j < n; j++)
			{
				Vector2 vector = center + Vector2.right * ((float)(j + 1) * num2) * num;
				if (vector.x > battleBounds.xMin && vector.x < battleBounds.xMax)
				{
					BossSpawnedGeoAttack bossSpawnedGeoAttack = SummonSpike(vector);
					float num3 = (float)j / (float)n;
					bossSpawnedGeoAttack.SpawnGeo(num3 * totalSeconds, heightPercentage);
				}
			}
			num = -1;
		}
	}

	private BossSpawnedGeoAttack SummonSpike(Vector3 position)
	{
		return PoolManager.Instance.ReuseObject(spikePrefab.gameObject, position + Vector3.up, Quaternion.identity).GameObject.GetComponent<BossSpawnedGeoAttack>();
	}

	public void ShowRicochetArrowTrail(Vector2 startPoint, Vector2 endPoint)
	{
		Vector2 dir = endPoint - startPoint;
		arrowTrailInstantProjectileAttack.Shoot(startPoint, dir);
		Amanecidas.Audio.PlayLaserPreattack_AUDIO();
	}

	public void ShowRicochetArrowTrailFast(Vector2 startPoint, Vector2 endPoint)
	{
		Vector2 dir = endPoint - startPoint;
		arrowTrailFastInstantProjectileAttack.Shoot(startPoint, dir);
		Amanecidas.Audio.PlayHorizontalPreattack_AUDIO();
	}

	public void ShowMineArrowTrail(Vector2 startPoint, Vector2 endPoint)
	{
		Vector2 dir = endPoint - startPoint;
		arrowTrailInstantProjectileAttack.Shoot(startPoint, dir);
		Amanecidas.Audio.PlayHorizontalPreattack_AUDIO();
	}

	public void ShootRicochetArrow(Vector2 startPoint, Vector2 endPoint)
	{
		ricochetArrowInstantProjectileAttack.Shoot(startPoint, endPoint - startPoint);
	}

	private void OnSmallJumpSmashLanded()
	{
		ShakeWave(doShockwave: false);
		SpikeWave(base.transform.position, 0.8f, 10, doShockWaves: false, 1f);
		Amanecidas.Audio.PlayGroundAttack_AUDIO();
		shockwave.SummonAreaOnPoint(base.transform.position);
	}

	private void OnLavaBallJumpSmashLanded()
	{
		ShakeWave(doShockwave: false);
		SpikeWave(base.transform.position, 0.6f, 20, doShockWaves: false);
		shockwave.SummonAreas((!(base.transform.position.x < battleBounds.center.x)) ? Vector2.left : Vector2.right);
		flameBladeAttack.Shoot(Vector2.left, Vector2.down * 0.4f);
		flameBladeAttack.Shoot(Vector2.right, Vector2.down * 0.4f);
		Amanecidas.Audio.PlayGroundAttack_AUDIO();
		shockwave.SummonAreaOnPoint(base.transform.position);
	}

	public void ShootMineArrow()
	{
		Vector2 vector = ((!(base.transform.position.x > battleBounds.center.x)) ? Vector2.right : Vector2.left);
		AimToPointWithBow((Vector2)base.transform.position + vector);
		List<GameObject> list = mineArrowProjectileAttack.Shoot((Vector2)base.transform.position + new Vector2(0f, 1.75f), vector);
		foreach (GameObject item in list)
		{
			mines.Add(item.GetComponent<BossSpawnedAreaAttack>());
		}
	}

	public void StartChargingArrow()
	{
		Amanecidas.Audio.PlayAttackCharge_AUDIO();
	}

	public void ReleaseChargedArrow()
	{
		float angle = ((!(Amanecidas.transform.position.x > battleBounds.center.x)) ? 180f : 0f);
		chargedArrowExplosionAttack.SummonAreaOnPoint((Vector2)Amanecidas.transform.position + Vector2.up, angle);
		shockwave.SummonAreaOnPoint(Amanecidas.transform.position);
		ShakeWave();
		Amanecidas.AnimatorInyector.SetBow(value: false);
	}

	public void ShootSpikeSummoningArrow()
	{
		Vector2 vector = Amanecidas.transform.position + Vector3.right;
		arrowInstantProjectileAttack.Shoot(vector, Vector2.down);
		Amanecidas.Audio.PlayArrowFireFast_AUDIO();
		vector.y = battleBounds.yMin;
		verticalBlastArrowAttack.SummonAreaOnPoint(vector, 270f);
		Core.Logic.ScreenFreeze.Freeze(0.05f, 0.2f);
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 0.5f, 0.3f, 2f);
		SpikeWave(vector, 1f, 20, doShockWaves: true, 1.8f);
		Amanecidas.AnimatorInyector.SetBow(value: false);
	}

	public void ShakeWave(bool doShockwave = true)
	{
		Core.Logic.ScreenFreeze.Freeze(0.05f, 0.2f);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.35f, Vector3.down * 0.5f, 12, 0.2f, 0.01f, default(Vector3), 0.01f, ignoreTimeScale: true);
		if (doShockwave)
		{
			Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 0.5f, 0.3f, 2f);
		}
	}

	public void SmallDistortion()
	{
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position, 0.2f, 0.1f, 0.5f);
	}

	public void ActivateMines()
	{
		if (mines != null && mines.Count != 0)
		{
			for (int i = 0; i < mines.Count; i++)
			{
				mines[i].SetRemainingPreparationTime((float)i * 0.8f);
			}
			mines.Clear();
			ClearRotationAndFlip();
			Amanecidas.AnimatorInyector.SetBow(value: false);
			Amanecidas.SetOrientation(Amanecidas.Status.Orientation);
		}
	}

	public void SetFrozenArrow()
	{
		Amanecidas.AnimatorInyector.PlayBlinkshot();
		Vector2 dirToPenitent = GetDirToPenitent(base.transform.position);
		StraightProjectile straightProjectile = bulletTimeProjectileAttack.Shoot(dirToPenitent);
		AimToPointWithBow((Vector2)base.transform.position + dirToPenitent);
		bullets.Add(straightProjectile as BulletTimeProjectile);
		Amanecidas.Audio.PlayArrowCharge_AUDIO();
	}

	public void PlayChargeEnergy(float seconds = 1f, bool useLongRangeParticles = false, bool playSfx = true)
	{
		chargeParticles.Play();
		if (useLongRangeParticles)
		{
			chargeParticlesLongRange.Play();
		}
		chargeEnergy_EA.StartAction(this, seconds, playSfx);
	}

	public void SetFrozenArrowVariation()
	{
		Amanecidas.AnimatorInyector.PlayBlinkshot();
		Vector2 vector = (Vector2)base.transform.position - new Vector2(battleBounds.center.x, battleBounds.yMax + 4f);
		StraightProjectile straightProjectile = bulletTimeProjectileAttack.Shoot(vector);
		AimToPointWithBow((Vector2)base.transform.position + vector);
		bullets.Add(straightProjectile as BulletTimeProjectile);
		Amanecidas.Audio.PlayArrowCharge_AUDIO();
	}

	public void SetThreeMultiFrozenArrows()
	{
		SetMultiFrozenArrows(3, 1.2f);
	}

	public void SetFourMultiFrozenArrows()
	{
		SetMultiFrozenArrows(4, 1.4f);
	}

	private void SetMultiFrozenArrows(int numArrows, float width)
	{
		Vector2 dirToPenitent = GetDirToPenitent(base.transform.position);
		Vector2 normalized = new Vector2(dirToPenitent.y, 0f - dirToPenitent.x).normalized;
		for (int i = 0; i < numArrows; i++)
		{
			Vector2 offset = Vector2.Lerp(normalized * (0f - width), normalized * width, (float)i / ((float)numArrows - 1f));
			offset.y += 0.9f;
			Vector2 dir = dirToPenitent + normalized * 1f * i;
			StraightProjectile straightProjectile = bulletTimeProjectileAttack.Shoot(dir, offset);
			multiBullets.Add(straightProjectile as BulletTimeProjectile);
		}
		Amanecidas.Audio.PlayArrowCharge_AUDIO();
	}

	public void SetFrozenHail(Vector2 lancePosition)
	{
		Vector2 offset = lancePosition - (Vector2)base.transform.position + UnityEngine.Random.Range(-1.5f, 1.5f) * Vector2.down;
		offset += UnityEngine.Random.Range(-0.5f, 0.5f) * Vector2.right;
		Vector2 vector = Vector2.down + UnityEngine.Random.Range(-0.25f, 0.25f) * Vector2.right;
		StraightProjectile straightProjectile = bulletTimeHailAttack.Shoot(vector.normalized, offset);
		bullets.Add(straightProjectile as BulletTimeProjectile);
	}

	public void SetFrozenLance(Vector2 lancePosition)
	{
		if (!(lancePosition.x > battleBounds.xMax) && !(lancePosition.x < battleBounds.xMin))
		{
			Vector2 offset = lancePosition - (Vector2)base.transform.position;
			StraightProjectile straightProjectile = bulletTimeLanceAttack.Shoot(Vector2.down, offset);
			bullets.Add(straightProjectile as BulletTimeProjectile);
		}
	}

	public void ShootGhostProjectile(Vector2 startPoint, Vector2 dir)
	{
		Vector3 rotation = new Vector3(0f, 0f, Mathf.Atan2(dir.y, dir.x) * 57.29578f);
		if (dir.x < 0f)
		{
			rotation.y = 180f;
		}
		ghostProjectileAttack.Shoot(dir, Vector2.zero, rotation);
	}

	public void UpdateBowAimRotation()
	{
		Amanecidas.AnimatorInyector.SetBow(value: true);
		AimToPointWithBow(bowAimTarget);
	}

	private void AimToPointWithBow(Vector2 point)
	{
		Vector2 vector = point - (Vector2)base.transform.position;
		float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		num = (num + 360f) % 360f;
		Amanecidas.AnimatorInyector.FlipSpriteWithAngle(num);
		Amanecidas.AnimatorInyector.SetSpriteRotation(num, bowAngleDifference);
	}

	private void AimMeleeDirection(Vector2 dir, float difference = 15f)
	{
		float num = Mathf.Atan2(dir.y, dir.x) * 57.29578f;
		num = (num + 360f) % 360f;
		Amanecidas.AnimatorInyector.FlipSpriteWithAngle(num);
		Amanecidas.AnimatorInyector.SetSpriteRotation(num, difference);
	}

	public void DoLanceProjectile()
	{
		Debug.Log("<PROJECTILE ATTACK>");
		Vector2 dirToPenitent = GetDirToPenitent(base.transform.position);
		dirToPenitent.y = 0f;
		StraightProjectile straightProjectile = bulletTimeLanceAttack.Shoot(dirToPenitent);
		bullets.Add(straightProjectile as BulletTimeProjectile);
	}

	public void ClearRotationAndFlip()
	{
		Amanecidas.AnimatorInyector.ClearRotationAndFlip();
		ReapplyOrientation();
	}

	private void ReapplyOrientation()
	{
		Amanecidas.SetOrientation(Amanecidas.Status.Orientation);
	}

	public void ActivateFrozenArrows()
	{
		Debug.Log("<ACTIVATE FROZEN ARROWS>");
		if (bullets == null || bullets.Count == 0)
		{
			return;
		}
		foreach (BulletTimeProjectile bullet in bullets)
		{
			bullet.Accelerate(1.1f);
		}
		bullets.Clear();
		ClearRotationAndFlip();
		Amanecidas.AnimatorInyector.SetBow(value: false);
		Amanecidas.SetOrientation(Amanecidas.Status.Orientation);
		Amanecidas.Audio.PlayArrowFire_AUDIO();
	}

	public void ActivateMultiFrozenArrows()
	{
		Debug.Log("<Activate Multi Frozen Arrows>");
		if (multiBullets == null || multiBullets.Count == 0)
		{
			return;
		}
		foreach (BulletTimeProjectile multiBullet in multiBullets)
		{
			multiBullet.Accelerate();
		}
		multiBullets.Clear();
		Amanecidas.Audio.PlayArrowFire_AUDIO();
	}

	public void ShowBothAxes(bool v)
	{
		ShowFirstAxe(v);
		ShowSecondAxe(v);
	}

	public void ShowAxe(bool show, bool isFirstAxe)
	{
		ShowAxe(show, isFirstAxe, setAxePosition: false, Vector2.zero);
	}

	public void ShowAxe(bool show, bool isFirstAxe, bool setAxePosition, Vector2 axePosition)
	{
		int index = ((!isFirstAxe) ? 1 : 0);
		if (show)
		{
			int dirFromOrientation = GetDirFromOrientation();
			if (setAxePosition)
			{
				axes[index].transform.position = axePosition;
			}
			else
			{
				axes[index].transform.position = (Vector2)base.transform.position + new Vector2(axeOffset.x * (float)dirFromOrientation, axeOffset.y);
			}
			axes[index].SetSeek(free: false);
		}
		axes[index].SetVisible(show);
	}

	public void ShowFirstAxe(bool v)
	{
		ShowAxe(v, isFirstAxe: true);
	}

	public void ShowSecondAxe(bool v)
	{
		ShowAxe(v, isFirstAxe: false);
	}

	public void ActivateFrozenHail()
	{
		StartCoroutine(ActivateFrozenHailRandomDelayed(0.01f));
	}

	public void ActivateFrozenLances()
	{
		Debug.Log("<Activate Frozen Lances>");
		StartCoroutine(ActivateFrozenLancesDelayed());
	}

	private IEnumerator ActivateFrozenLancesDelayed(float delay = 0.1f)
	{
		Debug.Log("<Activate Frozen Lances>");
		if (bullets == null || bullets.Count == 0)
		{
			yield break;
		}
		foreach (BulletTimeProjectile item in bullets)
		{
			item.Accelerate();
			yield return new WaitForSeconds(delay);
		}
		bullets.Clear();
	}

	private IEnumerator ActivateFrozenHailRandomDelayed(float delay = 0.1f)
	{
		Debug.Log("<Activate Frozen HAIL>");
		if (bullets != null && bullets.Count != 0)
		{
			while (bullets.Count > 0)
			{
				BulletTimeProjectile item = bullets[UnityEngine.Random.Range(0, bullets.Count)];
				item.Accelerate();
				yield return new WaitForSeconds(delay);
				bullets.Remove(item);
			}
			bullets.Clear();
		}
	}

	public void DoAnticipateShockwave()
	{
		Debug.Log("SHIELD RECHARGE FINISHED");
		Debug.Log("ANTICIPATE SHOCKWAVE");
		PlayChargeEnergy(1f, useLongRangeParticles: false, playSfx: false);
		Amanecidas.AnimatorInyector.SetShockwaveAnticipation(v: true);
		Amanecidas.AnimatorInyector.SetRecharging(active: false);
		Amanecidas.IsGuarding = true;
		Amanecidas.Audio.PlayShieldRecharge_AUDIO();
	}

	public void DoShieldShockwave()
	{
		Debug.Log("ACTIVATE SHIELD");
		Amanecidas.AnimatorInyector.SetShockwaveAnticipation(v: false);
		_fsm.ChangeState(stAction);
		ShockwaveOnSelf();
		ResetShieldActions();
		Amanecidas.ActivateShield();
	}

	public void ShockwaveOnSelf()
	{
		shieldShockwave_EA.StartAction(this);
	}

	public void SetGhostTrail(bool active)
	{
		Amanecidas.GhostTrail.EnableGhostTrail = active;
	}

	private void DoRechargeShield()
	{
		Debug.Log("START RECHARGE SHIELD LOOP");
		Amanecidas.AnimatorInyector.SetWeaponVisible(visible: false);
		ShowBothAxes(v: false);
		Amanecidas.AnimatorInyector.SetRecharging(active: true);
		Amanecidas.shield.StartToRecoverShield((float)currentFightParameters.shieldRechargeTime * 0.8f, (float)currentFightParameters.shieldRechargeTime * 0.2f, (float)currentFightParameters.shieldShockwaveAnticipationTime * 0.2f, (float)currentFightParameters.shieldShockwaveAnticipationTime * 0.8f);
		_fsm.ChangeState(stRecharge);
	}

	private void LookAtPenitent(bool instant = false)
	{
		LookAtTarget(Core.Logic.Penitent.transform.position, instant);
	}

	private void LookAtPenitentUsingOrientation()
	{
		Vector2 dirToPenitent = GetDirToPenitent(base.transform.position);
		LookAtDirUsingOrientation(dirToPenitent);
	}

	private void LookAtPointUsingOrientation(Vector2 p)
	{
		Vector2 v = p - (Vector2)base.transform.position;
		LookAtDirUsingOrientation(v);
	}

	public void PlayAnticipationGrunt(AMANECIDA_GRUNTS grunt)
	{
	}

	private void LookAtDirUsingOrientation(Vector2 v)
	{
		Amanecidas.SetOrientation((!(v.x > 0f)) ? EntityOrientation.Left : EntityOrientation.Right);
	}

	public override void LookAtTarget(Vector3 targetPos)
	{
		base.LookAtTarget(targetPos);
		throw new Exception("You really really didn't mean to use this method. Look at the one below: that is the one you are looking for.");
	}

	public void LookAtTarget(Vector3 targetPos, bool instant)
	{
		bool flag = targetPos.x > Amanecidas.transform.position.x;
		if ((flag && Amanecidas.Status.Orientation == EntityOrientation.Left) || (!flag && Amanecidas.Status.Orientation == EntityOrientation.Right))
		{
			ForceTurnAround(instant);
		}
	}

	private void ForceTurnAround(bool instant = false)
	{
		Debug.Log(string.Format("<color=red> !!! <<TURNING>> Orientation before: {2}. myX:{0} penitentX:{1} </color>", base.transform.position.x, Core.Logic.Penitent.GetPosition().x, Amanecidas.Status.Orientation));
		Amanecidas.AnimatorInyector.PlayTurnAround(instant);
	}

	private Vector2 GetPointBelowPenitent(bool stopOnOneWayDowns)
	{
		return GetPointBelow(Core.Logic.Penitent.transform.position + Vector3.up * 0.25f, stopOnOneWayDowns);
	}

	private Vector2 GetDirToPenitent()
	{
		return (Vector2)Core.Logic.Penitent.transform.position - (Vector2)base.transform.position;
	}

	private Vector2 GetDirToPenitent(Vector2 point)
	{
		return (Vector2)Core.Logic.Penitent.transform.position - point;
	}

	public float GetAngleDifference()
	{
		return (currentWeapon != AmanecidasAnimatorInyector.AMANECIDA_WEAPON.SWORD) ? lanceRotationDiference : falcataRotationDiference;
	}

	public int GetDirFromOrientation()
	{
		return (Amanecidas.Status.Orientation == EntityOrientation.Right) ? 1 : (-1);
	}

	private bool IsInsideGround(Vector2 p, LayerMask m)
	{
		if (Physics2D.RaycastNonAlloc(p, Vector2.down, results, 0.1f, m) > 0)
		{
			Debug.DrawLine(p, results[0].point, Color.cyan, 5f);
			Debug.Log("IS INSIDE GROUND");
			return true;
		}
		return false;
	}

	private Vector2 GetPointBelow(Vector2 p, bool stopOnOneWayDowns = false, float maxDistance = 100f)
	{
		float num = 0.4f;
		LayerMask layerMask = ((!stopOnOneWayDowns) ? floorMask : floorNOneWayDownMask);
		if (Physics2D.RaycastNonAlloc(p, Vector2.down, results, maxDistance, layerMask) > 0)
		{
			return results[0].point + Vector2.up * num;
		}
		return p;
	}

	private bool HasSolidFloorBelow(bool checkOneWayDowns = false)
	{
		Vector2 pointBelowMe = GetPointBelowMe(checkOneWayDowns, 0.9f);
		if (Vector2.Distance(pointBelowMe, base.transform.position) < Mathf.Epsilon)
		{
			return false;
		}
		return true;
	}

	private Vector2 GetPointBelowMe(bool stopOnOneWayDowns = false, float maxDistance = 100f)
	{
		float num = 0f;
		LayerMask m = ((!stopOnOneWayDowns) ? floorMask : floorNOneWayDownMask);
		if (IsInsideGround(base.transform.position, m))
		{
			num = 2f;
		}
		return GetPointBelow((Vector2)base.transform.position + Vector2.up * num, stopOnOneWayDowns, maxDistance);
	}

	private Vector2 GetValidPointInDirection(Vector2 dir, float amount)
	{
		Vector2 normalized = dir.normalized;
		Vector2 vector = base.transform.position;
		if (Physics2D.RaycastNonAlloc(vector, normalized, results, amount, BlockLayerMask) > 0)
		{
			Debug.DrawLine(vector, results[0].point, Color.red, 5f);
			amount = results[0].distance;
		}
		else
		{
			Debug.DrawLine(vector, vector + dir * amount, Color.green, 5f);
		}
		return vector + normalized * amount;
	}

	public override void Parry()
	{
		base.Parry();
		StopCurrentAction();
		LookAtPenitent(instant: true);
		SetGhostTrail(active: false);
		base.transform.DOKill(complete: true);
		Amanecidas.AnimatorInyector.Parry();
		SetInterruptable(state: true);
		WaitAfterParry();
	}

	private void WaitAfterParry()
	{
		float num = 1.5f;
		StartWait(num, num);
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

	public void SpawnClone()
	{
		GameObject gameObject = PoolManager.Instance.ReuseObject(clonePrefab, base.transform.position, Quaternion.identity).GameObject;
		SimpleDamageableObject component = gameObject.GetComponent<SimpleDamageableObject>();
		component.SetFlip(Amanecidas.Status.Orientation == EntityOrientation.Left);
	}

	private bool CheckAndSwapFightParameters()
	{
		bool result = false;
		float hpPercentage = Amanecidas.GetHpPercentage();
		if (Amanecidas.IsLaudes)
		{
			AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = laudesAttackConfigData;
			availableAttacks = amanecidaAttackScriptableConfig.GetAttackIdsByWeapon(currentWeapon, useHP: false);
		}
		else
		{
			AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig2 = attackConfigData;
			availableAttacks = amanecidaAttackScriptableConfig2.GetAttackIdsByWeapon(currentWeapon, useHP: true, GetHpPercentage());
		}
		for (int i = 0; i < currentFightParameterList.Count; i++)
		{
			if (currentFightParameterList[i].hpPercentageBeforeApply < currentFightParameters.hpPercentageBeforeApply && currentFightParameterList[i].hpPercentageBeforeApply > hpPercentage && !currentFightParameters.Equals(currentFightParameterList[i]))
			{
				currentFightParameters = currentFightParameterList[i];
				result = true;
				break;
			}
		}
		return result;
	}

	public void ShieldDamage(Hit hit)
	{
	}

	public void Damage(Hit hit)
	{
		if (CheckAndSwapFightParameters() && Amanecidas.IsLaudes)
		{
			needsToSwapWeapon = true;
		}
		if (_fsm.IsInState(stRecharge))
		{
			damageWhileRecharging -= hit.DamageAmount;
			Debug.Log("DAMAGE BEFORE RECHARGE " + damageWhileRecharging);
		}
		if (CanBeInterrupted() && _currentHurtHits < currentFightParameters.maxHitsInHurt)
		{
			bool isLastHurt = false;
			_fsm.ChangeState(stHurt);
			StopCurrentAction();
			base.transform.DOKill(complete: true);
			LookAtPenitent(instant: true);
			_currentHurtHits++;
			if (_currentHurtHits >= currentFightParameters.maxHitsInHurt)
			{
				_currentHurtHits = 0;
				isLastHurt = true;
			}
			currentAction = LaunchAction_Hurt(isLastHurt);
			currentAction.OnActionEnds -= OnHurtActionEnds;
			currentAction.OnActionEnds += OnHurtActionEnds;
		}
	}

	private void OnHurtActionEnds(EnemyAction e)
	{
		e.OnActionEnds -= OnHurtActionEnds;
		LaunchAutomaticAction();
	}

	public bool DodgeHit(Hit h)
	{
		if (IsDodging() && lastAttack != AMANECIDA_ATTACKS.COMMON_ChangeWeapon)
		{
			LaunchAction(AMANECIDA_ATTACKS.FALCATA_CounterAttack);
			dodgeCooldown = dodgeMaxCooldown;
			return true;
		}
		return false;
	}

	public void Death()
	{
		PlayMakerFSM.BroadcastEvent("BOSS DEAD");
		Amanecidas.Audio.StopAll();
		CleanAll();
		StopCurrentAction();
		StopAllCoroutines();
		base.transform.DOKill();
		_fsm.ChangeState(stDeath);
		LookAtPenitentUsingOrientation();
		Core.Logic.Penitent.Status.Invulnerable = true;
		LaunchAction(AMANECIDA_ATTACKS.COMMON_Death);
	}

	private float GetAttackRecoverySeconds(AmanecidaAttackScriptableConfig.AmanecidaAttackConfig config)
	{
		float hpPercentage = GetHpPercentage();
		if (hpPercentage > 0.66f)
		{
			return config.recoverySeconds;
		}
		if (hpPercentage > 0.33f)
		{
			return config.recoverySeconds2;
		}
		return config.recoverySeconds3;
	}

	private int GetAttackNumberOfRepetitions(AmanecidaAttackScriptableConfig.AmanecidaAttackConfig config)
	{
		float hpPercentage = GetHpPercentage();
		if (hpPercentage > 0.66f)
		{
			return config.repetitions;
		}
		if (hpPercentage > 0.33f)
		{
			return config.repetitions2nd;
		}
		return config.repetitions3rd;
	}

	private float GetAttackWeight(AmanecidaAttackScriptableConfig.AmanecidaAttackConfig config)
	{
		float hpPercentage = GetHpPercentage();
		if (hpPercentage > 0.66f)
		{
			return config.weight;
		}
		if (hpPercentage > 0.33f)
		{
			return config.weight2;
		}
		return config.weight3;
	}

	private void CleanAll()
	{
		ShowBothAxes(v: false);
		if ((bullets != null) & (bullets.Count > 0))
		{
			foreach (BulletTimeProjectile bullet in bullets)
			{
				bullet.gameObject.SetActive(value: false);
			}
		}
		GameplayUtils.DestroyAllProjectiles();
	}

	public bool IsShieldBroken()
	{
		return Amanecidas.shieldCurrentHP <= 0f;
	}

	public void InstantBreakShield()
	{
		if (!IsShieldBroken())
		{
			Amanecidas.ForceBreakShield();
		}
	}

	public bool IsPenitentThrown()
	{
		return Core.ready && Core.Logic.Penitent != null && Core.Logic.Penitent.ThrowBack.IsThrown;
	}

	public void ShowCurrentWeapon(bool show)
	{
		if (show)
		{
			Amanecidas.AnimatorInyector.SetAmanecidaWeapon(currentWeapon);
		}
		else
		{
			Amanecidas.AnimatorInyector.SetAmanecidaWeapon(AmanecidasAnimatorInyector.AMANECIDA_WEAPON.HAND);
		}
	}

	private Vector2 GetTargetPosition(float error = 0f)
	{
		Vector2 vector = (Vector2)Core.Logic.Penitent.transform.position + Vector2.up;
		return vector + new Vector2(UnityEngine.Random.Range(0f - error, error), UnityEngine.Random.Range(0f - error, error));
	}

	private BezierSpline CopySplineFrom(BezierSpline spline)
	{
		BezierSpline component = PoolManager.Instance.ReuseObject(poolSplinePrefab, spline.transform.position, spline.transform.rotation).GameObject.GetComponent<BezierSpline>();
		component.transform.localScale = Vector3.one;
		component.Copy(spline);
		return component;
	}

	public void StartIntro()
	{
		LaunchAction(AMANECIDA_ATTACKS.COMMON_Intro);
	}

	private EnemyAction LaunchAction_StompSmash()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.COMMON_StompAttack);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return multiStompAttack_EA.StartAction(this, attackNumberOfRepetitions, 0f, doBlinkBeforeJump: true, doStompDamage: true, usePillars: true, bounceAfterJump: false);
	}

	private EnemyAction LaunchAction_FlyAndSpin()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.AXE_FlyAndSpin);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return FollowAndSpinAxes(aroundThrow, aroundThrow2);
	}

	private EnemyAction LaunchAction_FollowAndLavaBall()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.AXE_FollowAndLavaBall);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return (!(Amanecidas.shieldCurrentHP <= 0f)) ? launchBallsToPenitent_EA.StartAction(this, 1f, 2, keepDistance: true) : launchBallsToPenitent_EA.StartAction(this, 0.8f, 5, keepDistance: true);
	}

	private EnemyAction LaunchAction_FollowAndTossAxes()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.AXE_FollowAndAxeToss);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return launchAxesToPenitent_EA.StartAction(this, axes[0], axes[1], 1f, attackNumberOfRepetitions);
	}

	private EnemyAction LaunchAction_FollowAndTossCrawlerAxes()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.AXE_FollowAndCrawlerAxeToss);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return launchCrawlerAxesToPenitent_EA.StartAction(this, axes[0], axes[1], 1f, attackNumberOfRepetitions);
	}

	private EnemyAction LaunchAction_FlameBlade()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.AXE_FlameBlade);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		Vector2 zero = Vector2.zero;
		zero = ((!(base.transform.position.x > battleBounds.center.x)) ? new Vector2(battleBounds.xMin, battleBounds.yMin - 0.2f) : new Vector2(battleBounds.xMax, battleBounds.yMin - 0.2f));
		return meleeProjectile_EA.StartAction(this, zero, 1.5f, Amanecidas.Audio.PlayAxeAttack_AUDIO, flameBladeAttack, attackNumberOfRepetitions);
	}

	private EnemyAction LaunchAction_DualThrow()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.AXE_DualThrow);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return LaunchDualAxesSameThrow(horizontalThrow, Vector2.zero);
	}

	private EnemyAction LaunchAction_DualThrowCross()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.AXE_DualThrowCross);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return LaunchDualAxesSameThrow(crossThrow, Vector2.up, mirroredX: false, mirroredY: true);
	}

	private EnemyAction LaunchAction_FlyAndToss()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.AXE_FlyAndToss);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return LaunchDualAxesVertical(straightThrow, mirroredX: true);
	}

	private EnemyAction LaunchAction_JumpSmash()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.AXE_JumpSmash);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return JumpSmashAttack(pillars: false);
	}

	private EnemyAction LaunchAction_JumpSmashWithPillars()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.AXE_JumpSmashWithPillars);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return JumpSmashAttack(pillars: true);
	}

	private EnemyAction LaunchAction_AxeMeleeAttack()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.AXE_MeleeAttack);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return meleeAttack_EA.StartAction(this, 0.7f, Amanecidas.Audio.PlayAxeAttack_AUDIO);
	}

	private EnemyAction LaunchComboAction_StompNLavaBalls()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.AXE_COMBO_StompNLavaBalls);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return (!(Amanecidas.CurrentLife <= Amanecidas.Stats.Life.Final / 2f)) ? multiStompNLavaBalls_ECA.StartAction(this, DoLavaBallJumpSmash, attackNumberOfRepetitions) : multiStompNLavaBalls_ECA.StartAction(this, DoLavaBallJumpSmash, attackNumberOfRepetitions);
	}

	private EnemyAction LaunchAction_RicochetShot()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.BOW_RicochetShot);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return (!(Amanecidas.shieldCurrentHP <= 0f)) ? shootRicochetArrow_EA.StartAction(this, Mathf.RoundToInt((float)attackConfig.repetitions * 1.5f), ShowRicochetArrowTrail, ShootRicochetArrow, 2f, ricochetMask) : shootRicochetArrow_EA.StartAction(this, attackConfig.repetitions, ShowRicochetArrowTrail, ShootRicochetArrow, 1.5f, ricochetMask);
	}

	private EnemyAction LaunchAction_FreezeTimeBlinkShots()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.BOW_FreezeTimeBlinkShots);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return (!(Amanecidas.shieldCurrentHP <= 0f)) ? freezeTimeBlinkShots_EA.StartAction(this, attackNumberOfRepetitions, 0.1f, doRandomDisplacement: true, SetFrozenArrow, ActivateFrozenArrows) : freezeTimeBlinkShots_EA.StartAction(this, attackNumberOfRepetitions + 4, 0f, doRandomDisplacement: false, SetFrozenArrowVariation, ActivateFrozenArrows);
	}

	private EnemyAction LaunchAction_FreezeTimeMultiShots()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.BOW_FreezeTimeMultiShots);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return (!(Amanecidas.shieldCurrentHP <= 0f)) ? freezeTimeMultiShots_EA.StartAction(this, attackNumberOfRepetitions, 0.9f, SetThreeMultiFrozenArrows, ActivateMultiFrozenArrows) : freezeTimeMultiShots_EA.StartAction(this, attackNumberOfRepetitions + 4, 0.7f, SetFourMultiFrozenArrows, ActivateMultiFrozenArrows);
	}

	private EnemyAction LaunchAction_MineShots()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.BOW_MineShots);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		Vector2 originPoint;
		Vector2 endPoint;
		if (Core.Logic.Penitent.GetPosition().x > battleBounds.center.x)
		{
			originPoint = new Vector2(battleBounds.xMin - 0.45f, battleBounds.yMin - 0.55f);
			endPoint = new Vector2(battleBounds.xMin + 2f, battleBounds.yMax + 1.7f);
		}
		else
		{
			originPoint = new Vector2(battleBounds.xMax + 0.45f, battleBounds.yMin - 0.55f);
			endPoint = new Vector2(battleBounds.xMax - 2f, battleBounds.yMax + 1.7f);
		}
		return (!(Amanecidas.shieldCurrentHP <= 0f)) ? shootMineArrows_EA.StartAction(this, attackNumberOfRepetitions, originPoint, endPoint, ShootMineArrow, ActivateMines, ShowMineArrowTrail, 2.4f, 0.55f, 1f) : shootMineArrows_EA.StartAction(this, attackNumberOfRepetitions + 2, originPoint, endPoint, ShootMineArrow, ActivateMines, ShowMineArrowTrail, 2.4f, 0.45f, 0.5f);
	}

	private EnemyAction LaunchAction_FastShot()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.BOW_FastShot);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return (!(Amanecidas.shieldCurrentHP <= 0f)) ? fastShot_EA.StartAction(this, 0.3f, bulletTimeProjectileAttack) : fastShot_EA.StartAction(this, 0.2f, bulletTimeProjectileAttack);
	}

	private EnemyAction LaunchAction_FastShots()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.BOW_FastShots);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return (!(Amanecidas.shieldCurrentHP <= 0f)) ? fastShots_EA.StartAction(this, 0.4f, attackNumberOfRepetitions, bulletTimeProjectileAttack) : fastShots_EA.StartAction(this, 0.4f, attackNumberOfRepetitions + 1, bulletTimeProjectileAttack);
	}

	private EnemyAction LaunchAction_ChargedShot()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.BOW_ChargedShot);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return (!(Amanecidas.shieldCurrentHP <= 0f)) ? chargedShot_EA.StartAction(this, StartChargingArrow, ReleaseChargedArrow, 4f, 4f) : chargedShot_EA.StartAction(this, StartChargingArrow, ReleaseChargedArrow, 4f, 5f);
	}

	private EnemyAction LaunchAction_SpikesBlinkShot()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.BOW_SpikesBlinkShot);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return (!(Amanecidas.shieldCurrentHP <= 0f)) ? spikesBlinkShots_EA.StartAction(this, attackNumberOfRepetitions, 1.2f, 0.3f, ShootSpikeSummoningArrow) : spikesBlinkShots_EA.StartAction(this, attackNumberOfRepetitions + 2, 1f, 0.2f, ShootSpikeSummoningArrow);
	}

	private EnemyAction LaunchComboAction_FreezeTimeNRicochetShots()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.BOW_COMBO_FreezeTimeNRicochetShots);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return (!(Amanecidas.CurrentLife <= Amanecidas.Stats.Life.Final / 2f)) ? freezeTimeNRicochetShots_ECA.StartAction(this, attackNumberOfRepetitions + 3, 0f, doRandomDisplacement: false, SetFrozenArrowVariation, ActivateFrozenArrows, attackNumberOfRepetitions, ShowRicochetArrowTrail, ShootRicochetArrow, 1.8f, ricochetMask) : freezeTimeNRicochetShots_ECA.StartAction(this, attackNumberOfRepetitions + 5, 0f, doRandomDisplacement: false, SetFrozenArrowVariation, ActivateFrozenArrows, attackNumberOfRepetitions + 2, ShowRicochetArrowTrail, ShootRicochetArrow, 1.6f, ricochetMask);
	}

	private EnemyAction LaunchAction_MoveBattleBounds()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		extraRecoverySeconds = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.COMMON_MoveBattleBounds).recoverySeconds;
		Vector2 zero = Vector2.zero;
		zero = ((laudesBowFightPhase != 0) ? new Vector2(2f, 8.5f) : new Vector2(-2f, 8.5f));
		laudesBowFightPhase++;
		return moveBattleBounds_EA.StartAction(this, zero, 0.2f, laudesBowFightPhase);
	}

	private EnemyAction LaunchAction_LaserShot()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		extraRecoverySeconds = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.BOW_LaserShot).recoverySeconds;
		return shootLaserArrow_EA.StartAction(this, 0.3f, floorMask, ShowRicochetArrowTrailFast, ShootRicochetArrow);
	}

	private EnemyAction LaunchAction_FreezeTimeHail()
	{
		Vector2 firstOriginPoint = new Vector2(battleBounds.center.x + 0.1f, battleBounds.yMax);
		Vector2 firstEndPoint = new Vector2(battleBounds.xMax + 0.5f, battleBounds.yMax);
		Vector2 secondOriginPoint = new Vector2(battleBounds.center.x - 0.1f, battleBounds.yMax);
		Vector2 secondEndPoint = new Vector2(battleBounds.xMin - 0.5f, battleBounds.yMax);
		if (IsPenitentInTop())
		{
			firstOriginPoint.y += 2f;
			firstEndPoint.y += 2f;
			secondOriginPoint.y += 2f;
			secondEndPoint.y += 2f;
		}
		Vector2 targetPosition = battleBounds.center + Vector2.up;
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.LANCE_FreezeTimeHail);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return doubleShootFrozenLances_EA.StartAction(this, attackNumberOfRepetitions, targetPosition, firstOriginPoint, firstEndPoint, secondOriginPoint, secondEndPoint, SetFrozenHail, ActivateFrozenHail, 0.2f, 1f, 0f, shouldSpin: true);
	}

	private EnemyAction LaunchAction_FreezeTimeLances()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		Vector2 originPoint;
		Vector2 endPoint;
		if (penitent.GetPosition().x > battleBounds.center.x)
		{
			originPoint = new Vector2(battleBounds.xMin + 0.5f, battleBounds.yMax + 2f);
			endPoint = new Vector2(battleBounds.xMax - 0.5f, battleBounds.yMax + 2f);
		}
		else
		{
			originPoint = new Vector2(battleBounds.xMax - 0.5f, battleBounds.yMax + 2f);
			endPoint = new Vector2(battleBounds.xMin + 0.5f, battleBounds.yMax + 2f);
		}
		if (IsPenitentInTop())
		{
			originPoint.y += 2f;
			endPoint.y += 2f;
		}
		Vector2 targetPosition = battleBounds.center + Vector2.up;
		float num = UnityEngine.Random.Range(-0.1f, 0.1f);
		originPoint.x += num;
		endPoint.x += num;
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.LANCE_FreezeTimeLances);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return (!(Amanecidas.shieldCurrentHP <= 0f)) ? shootFrozenLances_EA.StartAction(this, attackNumberOfRepetitions, originPoint, endPoint, targetPosition, SetFrozenLance, ActivateFrozenLances, 0.2f, 0.3f) : shootFrozenLances_EA.StartAction(this, Mathf.FloorToInt((float)attackNumberOfRepetitions * 1.5f), originPoint, endPoint, targetPosition, SetFrozenLance, ActivateFrozenLances, 0.1f, 0.2f);
	}

	private EnemyAction LaunchAction_FreezeTimeLancesOnPenitent()
	{
		Vector3 position = Core.Logic.Penitent.transform.position;
		Vector2 originPoint = new Vector2(position.x, battleBounds.yMax + 2f);
		Vector2 endPoint = new Vector2(position.x, battleBounds.yMax + 2f);
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.LANCE_FreezeTimeLancesOnPenitent);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		int num = ((!(Amanecidas.shieldCurrentHP <= 0f)) ? attackNumberOfRepetitions : Mathf.FloorToInt((float)attackNumberOfRepetitions * 1.5f));
		originPoint.x += (float)num * 0.55f * (float)GetDirFromOrientation();
		endPoint.x -= (float)num * 0.55f * (float)GetDirFromOrientation();
		if (IsPenitentInTop())
		{
			originPoint.y += 2f;
			endPoint.y += 2f;
		}
		Vector2 targetPosition = new Vector2(endPoint.x - (float)GetDirFromOrientation(), battleBounds.yMax + 1f);
		return (!(Amanecidas.shieldCurrentHP <= 0f)) ? shootFrozenLances_EA.StartAction(this, attackNumberOfRepetitions, originPoint, endPoint, targetPosition, SetFrozenLance, ActivateFrozenLances, 0.05f, 0.2f, 0f) : shootFrozenLances_EA.StartAction(this, num, originPoint, endPoint, targetPosition, SetFrozenLance, ActivateFrozenLances, 0.05f, 0.1f, 0f);
	}

	private EnemyAction LaunchAction_LanceBlinkDash()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		float num = UnityEngine.Random.Range(0f, 1f);
		Vector2 point = ((!(num > 0.5f)) ? (penitent.GetPosition() + Vector3.right * 6f) : (penitent.GetPosition() + Vector3.left * 6f));
		point += Vector2.up * 2f;
		BossDashAttack dashAttack = lanceDashAttack;
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.LANCE_BlinkDash);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		return blinkDash_EA.StartAction(this, point, dashAttack, 0.5f, skipReposition: false, endBlinkOut: false, showDashAim: true);
	}

	private EnemyAction LaunchAction_ChainDash()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.FALCATA_ChainDash);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return chainDash_EA.StartAction(this, attackNumberOfRepetitions);
	}

	private EnemyAction LaunchAction_FalcataBlinkBehindAndDash()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		float num = 3f;
		Vector2 pointBelowPenitent = GetPointBelowPenitent(stopOnOneWayDowns: true);
		Vector2 point = ((penitent.GetOrientation() != 0) ? (pointBelowPenitent + Vector2.right * num) : (pointBelowPenitent + Vector2.left * num));
		point += Vector2.up * 1f;
		BossDashAttack dashAttack = slashDashAttack;
		return blinkDash_EA.StartAction(this, point, dashAttack, 0.5f, skipReposition: false, endBlinkOut: false, showDashAim: true);
	}

	private EnemyAction LaunchAction_MultiLunge()
	{
		return quickLunge_EA.StartAction(this, 3);
	}

	private EnemyAction LaunchAction_Counter()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(AmanecidasAnimatorInyector.AMANECIDA_WEAPON.SWORD, AMANECIDA_ATTACKS.FALCATA_CounterAttack);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		return falcataCounter_EA.StartAction(this, dodgeDistance);
	}

	private EnemyAction LaunchAction_JumpBackNDash()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		Vector2 point = ((penitent.GetOrientation() != 0) ? (penitent.GetPosition() + Vector3.right * 6f) : (penitent.GetPosition() + Vector3.left * 6f));
		point += Vector2.up * 2f;
		BossDashAttack dashAttack = lanceDashAttack;
		return blinkDash_EA.StartAction(this, point, dashAttack, 0.5f);
	}

	private EnemyAction LaunchAction_HorizontalBlinkDashes()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.LANCE_HorizontalBlinkDashes);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return (!(Amanecidas.shieldCurrentHP <= 0f)) ? horizontalBlinkDashes_EA.StartAction(this, attackNumberOfRepetitions, 0.75f, startDashesAwayFromPenitent: true) : horizontalBlinkDashes_EA.StartAction(this, attackNumberOfRepetitions, 0.5f, startDashesAwayFromPenitent: false);
	}

	private EnemyAction LaunchAction_MultiFrontalDash()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.LANCE_MultiFrontalDash);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return multiFrontalDash_EA.StartAction(this, attackNumberOfRepetitions, 0.5f);
	}

	private EnemyAction LaunchAction_DiagonalBlinkDashes()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.LANCE_DiagonalBlinkDashes);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return (!(Amanecidas.shieldCurrentHP <= 0f)) ? diagonalBlinkDashes_EA.StartAction(this, attackNumberOfRepetitions, 1.2f, 0.8f, 0.8f) : diagonalBlinkDashes_EA.StartAction(this, attackNumberOfRepetitions, 1.2f, 0.8f, 1.2f);
	}

	private EnemyAction LaunchComboAction_FreezeTimeNHorizontalDashes()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		Vector2 originPoint;
		Vector2 endPoint;
		Vector2 targetPosition = default(Vector2);
		if (penitent.GetPosition().x > battleBounds.center.x)
		{
			originPoint = new Vector2(battleBounds.xMin - 0.5f, battleBounds.yMax + 3f);
			endPoint = new Vector2(battleBounds.xMax + 0.5f, battleBounds.yMax + 3f);
			targetPosition.x = Mathf.Clamp(penitent.GetPosition().x - 8f, battleBounds.xMin, battleBounds.xMax);
		}
		else
		{
			originPoint = new Vector2(battleBounds.xMax + 0.5f, battleBounds.yMax + 3f);
			endPoint = new Vector2(battleBounds.xMin - 0.5f, battleBounds.yMax + 3f);
			targetPosition.x = Mathf.Clamp(penitent.GetPosition().x + 8f, battleBounds.xMin, battleBounds.xMax);
		}
		targetPosition.y = battleBounds.yMax + 1f;
		float num = UnityEngine.Random.Range(-0.1f, 0.1f);
		originPoint.x += num;
		endPoint.x += num;
		return (!(Amanecidas.CurrentLife <= Amanecidas.Stats.Life.Final / 2f)) ? freezeTimeNHorizontalDashes_ECA.StartAction(this, 16, originPoint, endPoint, targetPosition, SetFrozenLance, ActivateFrozenLances, 0.5f, 0.3f, skipOne: false, 3, 0.5f, startDashesAwayFromPenitent: true) : freezeTimeNHorizontalDashes_ECA.StartAction(this, 32, originPoint, endPoint, targetPosition, SetFrozenLance, ActivateFrozenLances, 0.5f, 1f, skipOne: true, 5, 0.3f, startDashesAwayFromPenitent: false);
	}

	private EnemyAction LaunchAction_FalcataSlashProjectile()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.FALCATA_SlashProjectile);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		List<Vector2> list = new List<Vector2>();
		list.Add(GetDirToPenitent(base.transform.position));
		List<Vector2> dirs = list;
		return falcataSlashProjectile_EA.StartAction(this, dirs, 0.15f);
	}

	private EnemyAction LaunchAction_FalcataSlashBarrage()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.FALCATA_SlashBarrage);
		int num = GetAttackNumberOfRepetitions(attackConfig);
		bool startsFromRight = true;
		if (IsShieldBroken())
		{
			num += 2;
			startsFromRight = false;
		}
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		return falcataSlashBarrage_EA.StartAction(this, num, startsFromRight, 5f, 0.4f);
	}

	private EnemyAction LaunchAction_NoxiousBlade()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.FALCATA_NoxiousBlade);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		Vector2 zero = Vector2.zero;
		zero = ((!(base.transform.position.x > battleBounds.center.x)) ? new Vector2(battleBounds.xMin, battleBounds.yMin - 0.2f) : new Vector2(battleBounds.xMax, battleBounds.yMin - 0.2f));
		return meleeProjectile_EA.StartAction(this, zero, 1f, Amanecidas.Audio.PlaySwordAttack_AUDIO, noxiousBladeAttack, attackNumberOfRepetitions);
	}

	private EnemyAction LaunchAction_SpinAttack()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.FALCATA_SpinAttack);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return spinAttack.StartAction(this, attackNumberOfRepetitions);
	}

	private EnemyAction LaunchAction_FalcataCombo()
	{
		return falcataSlashStorm_ECA.StartAction(this, 1);
	}

	private EnemyAction LaunchAction_Intro()
	{
		return intro_EA.StartAction(this);
	}

	private EnemyAction LaunchAction_Death()
	{
		return death_EA.StartAction(this);
	}

	private EnemyAction LaunchAction_HurtDisplacement()
	{
		return hurtDisplacement_EA.StartAction(this, 0.5f);
	}

	private EnemyAction LaunchAction_Hurt(bool isLastHurt)
	{
		return hurt_EA.StartAction(this, isLastHurt);
	}

	private EnemyAction LaunchAction_BlinkAway()
	{
		bool flag = false;
		Vector2 vector = battleBounds.center;
		while (!flag)
		{
			vector = RandomPointInsideRect(battleBounds);
			if (Vector2.Distance(vector, Core.Logic.Penitent.transform.position) > 2f)
			{
				flag = true;
			}
		}
		return blink_EA.StartAction(this, vector, 2f);
	}

	private EnemyAction LaunchAction_ChangeWeapon()
	{
		return changeWeapon_EA.StartAction(this, 1f);
	}

	private EnemyAction LaunchAction_RechargeShield()
	{
		return recoverShield_EA.StartAction(this, currentFightParameters.shieldRechargeTime, currentFightParameters.shieldShockwaveAnticipationTime, DoRechargeShield, DoAnticipateShockwave, DoShieldShockwave);
	}

	private EnemyAction LaunchAction_FalcataMeleeAttack()
	{
		AmanecidaAttackScriptableConfig amanecidaAttackScriptableConfig = ((!Amanecidas.IsLaudes) ? attackConfigData : laudesAttackConfigData);
		AmanecidaAttackScriptableConfig.AmanecidaAttackConfig attackConfig = amanecidaAttackScriptableConfig.GetAttackConfig(currentWeapon, AMANECIDA_ATTACKS.FALCATA_MeleeAttack);
		extraRecoverySeconds = GetAttackRecoverySeconds(attackConfig);
		return meleeAttack_EA.StartAction(this, 0.5f, Amanecidas.Audio.PlaySwordAttack_AUDIO);
	}

	private EnemyAction JumpSmashAttack(bool pillars)
	{
		Vector2 jumpOrigin = new Vector2(battleBounds.xMin, battleBounds.yMin);
		if (Core.Logic.Penitent.GetPosition().x < battleBounds.center.x)
		{
			jumpOrigin.x = battleBounds.xMax;
		}
		return (!pillars) ? jumpSmash_EA.StartAction(this, jumpOrigin, DoJumpSmash, getTiredAtTheEnd: false) : jumpSmash_EA.StartAction(this, jumpOrigin, DoJumpSmashWithPillars, getTiredAtTheEnd: true);
	}

	private EnemyAction ThrowAxeAtPlayer()
	{
		Vector2 origin = base.transform.position + Vector3.right * 2f + Vector3.up * 1f;
		Vector2 targetPosition = GetTargetPosition();
		BezierSpline spline = CopySplineFrom(horizontalThrow.spline);
		return axes[0].axeSplineFollowAction.StartAction(axes[0], axes[0].splineFollower, origin, targetPosition, 3, horizontalThrow, spline);
	}

	private EnemyAction ThrowAxeAtDir(AmanecidaAxeBehaviour axe, Vector2 dir)
	{
		Vector2 vector = axe.transform.position;
		Vector2 end = vector + dir;
		BezierSpline spline = CopySplineFrom(horizontalThrow.spline);
		return axes[0].axeSplineFollowAction.StartAction(axes[0], axes[0].splineFollower, vector, end, 3, horizontalThrow, spline);
	}

	private EnemyAction LaunchDualAxesSameThrow(SplineThrowData throwData, Vector2 offset, bool mirroredX = false, bool mirroredY = false)
	{
		BezierSpline item = CopySplineFrom(throwData.spline);
		BezierSpline bezierSpline = CopySplineFrom(throwData.spline);
		if (mirroredX || mirroredY)
		{
			bezierSpline.transform.localScale = new Vector3((!mirroredX) ? 1 : (-1), (!mirroredY) ? 1 : (-1), 1f);
		}
		float x = Core.Logic.Penitent.transform.position.x;
		int dir = 1;
		Vector2 point = new Vector2(battleBounds.xMin + 1.4f, battleBounds.yMin) + offset;
		if (x < battleBounds.center.x)
		{
			dir = -1;
			point += Vector2.right * (battleBounds.width - 2.8f);
		}
		return dualAxeThrow_EA.StartAction(this, 2, point, battleBounds.width + 3f, dir, axes, new List<BezierSpline> { item, bezierSpline }, new List<SplineThrowData> { throwData, throwData });
	}

	private EnemyAction LaunchDualAxesVertical(SplineThrowData throwData, bool mirroredX = false, bool mirroredY = false)
	{
		BezierSpline item = CopySplineFrom(throwData.spline);
		BezierSpline bezierSpline = CopySplineFrom(throwData.spline);
		if (mirroredX || mirroredY)
		{
			bezierSpline.transform.localScale = new Vector3((!mirroredX) ? 1 : (-1), (!mirroredY) ? 1 : (-1), 1f);
		}
		int n = 4;
		if (Amanecidas.shieldCurrentHP <= 0f)
		{
			n = 8;
		}
		return dualAxeFlyingThrow_EA.StartAction(this, n, Vector2.down * 8f, axes, new List<BezierSpline> { item, bezierSpline }, new List<SplineThrowData> { throwData, throwData });
	}

	private EnemyAction SpinAxes(SplineThrowData throwData1, SplineThrowData throwData2)
	{
		return spinAxesAround_EA.StartAction(this, 3, axes, new List<BezierSpline> { throwData1.spline, throwData2.spline }, new List<SplineThrowData> { throwData1, throwData2 });
	}

	private EnemyAction FollowAndSpinAxes(SplineThrowData throwData1, SplineThrowData throwData2)
	{
		return followAndSpinAxes_EA.StartAction(this, 3, flightPattern, axes, new List<BezierSpline> { throwData1.spline, throwData2.spline }, new List<SplineThrowData> { throwData1, throwData2 });
	}

	public bool MoveBattleBoundsIfNeeded()
	{
		if (laudesBowFightPhase == AmanecidaArena.WEAPON_FIGHT_PHASE.THIRD)
		{
			return false;
		}
		_fsm.ChangeState(stAction);
		StopCurrentAction();
		LaunchAction(AMANECIDA_ATTACKS.COMMON_MoveBattleBounds);
		return true;
	}

	public void AimToPenitentWithBow()
	{
		Vector2 dirToPenitent = GetDirToPenitent(base.transform.position);
		AimToPointWithBow((Vector2)base.transform.position + dirToPenitent);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(battleBounds.center, battleBounds.size);
		Gizmos.DrawWireSphere(base.transform.position + (Vector3)axeOffset, 0.1f);
		if (debugDrawCurrentAction)
		{
			string text = "Waiting action";
			if (currentAction != null)
			{
				text = currentAction.ToString();
			}
			GizmoExtensions.DrawString(text, base.transform.position - Vector3.up * 0.5f, Color.green);
		}
		Gizmos.DrawWireSphere(bowAimTarget, 0.1f);
	}
}
