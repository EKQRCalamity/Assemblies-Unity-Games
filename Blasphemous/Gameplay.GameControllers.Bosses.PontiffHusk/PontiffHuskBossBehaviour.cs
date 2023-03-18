using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core.Surrogates;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Bosses.Generic.Attacks;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Environment.MovingPlatforms;
using Gameplay.GameControllers.Penitent;
using Gameplay.UI;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using Sirenix.OdinInspector;
using Tools.Audio;
using Tools.Level.Interactables;
using UnityEngine;
using UnityEngine.Playables;

namespace Gameplay.GameControllers.Bosses.PontiffHusk;

public class PontiffHuskBossBehaviour : EnemyBehaviour
{
	[Serializable]
	public struct PontiffHuskBossFightParameters
	{
		[SuffixLabel("%", false)]
		public float hpPercentageBeforeApply;

		[MinMaxSlider(0f, 5f, true)]
		public Vector2 minMaxWaitingTimeBetweenActions;
	}

	public enum PH_ATTACKS
	{
		BLASTS_ATTACK = 0,
		SIMPLE_PROJECTILES = 1,
		WIND_ATTACK = 2,
		CHARGED_BLAST = 3,
		SPIRAL_PROJECTILES = 4,
		LASERS_ATTACK = 5,
		MACHINEGUN_ATTACK = 6,
		BULLET_HELL_ATTACK = 7,
		ALT_SPIRAL_PROJECTILES = 8,
		DUMMY = 999
	}

	public class Intro_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			PontiffHuskBossBehaviour pontiffHuskBossBehaviour = owner as PontiffHuskBossBehaviour;
			pontiffHuskBossBehaviour.PontiffHuskBoss.AnimatorInyector.ResetAll();
			pontiffHuskBossBehaviour.transform.parent = null;
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			PontiffHuskBossBehaviour o = owner as PontiffHuskBossBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			o.PontiffHuskBoss.AnimatorInyector.StopHide();
			o.transform.DOKill();
			o.InstantLookAtDir(Vector3.left);
			o.transform.position = o.bossStartPos;
			o.DeathTrapLeft.transform.position = o.deathTrapLeftStartPos;
			o.DeathTrapRight.transform.position = o.deathTrapRightStartPos;
			o.DeathTrapLeft.SetActive(value: true);
			o.DeathTrapRight.SetActive(value: true);
			o.currentFightParameters = o.allFightParameters[0];
			ACT_MOVE.StartAction(o, o.deathTrapLeftStartPos + Vector2.right, 1f, Ease.InOutQuad, o.DeathTrapLeft.transform);
			ACT_MOVE.StartAction(o, o.deathTrapRightStartPos + Vector2.left, 1f, Ease.InOutQuad, o.DeathTrapRight.transform);
			ACT_MOVE.StartAction(o, o.bossStartPos, 2f, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			if (!o.firstTimeDialogDone)
			{
				o.firstTimeDialogDone = true;
				Core.Input.SetBlocker("HW_DIALOG_TIME", blocking: true);
				Core.Dialog.StartConversation("DLG_BS203_01", modal: false, useOnlyLast: false, hideWidget: false);
				yield return new WaitUntil(() => !UIController.instance.GetDialog().IsShowingDialog());
				o.PontiffHuskBoss.Audio.StartCombatMusic();
				Core.Dialog.StartConversation("DLG_BS203_06", modal: false, useOnlyLast: false);
				yield return new WaitUntil(() => !UIController.instance.GetDialog().IsShowingDialog());
				Core.Input.SetBlocker("HW_DIALOG_TIME", blocking: false);
			}
			else
			{
				o.PontiffHuskBoss.Audio.StartCombatMusic();
			}
			ACT_MOVE.StartAction(o, o.bossStartPos, 1f, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			Core.Dialog.StartConversation("DLG_BS203_02", modal: false, useOnlyLast: false);
			o.PontiffHuskBossScrollManager.ActivateModules();
			ACT_MOVE.StartAction(o, o.bossStartPos, 1f, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			FinishAction();
		}
	}

	public class Death_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			PontiffHuskBossBehaviour pontiffHuskBossBehaviour = owner as PontiffHuskBossBehaviour;
			pontiffHuskBossBehaviour.PontiffHuskBoss.AnimatorInyector.ResetAll();
			if (pontiffHuskBossBehaviour.executionInstance != null)
			{
				pontiffHuskBossBehaviour.executionInstance.SetActive(value: false);
				pontiffHuskBossBehaviour.executionInstance = null;
				pontiffHuskBossBehaviour.PontiffHuskBoss.AnimatorInyector.EntityAnimator.Play("HIDE");
				pontiffHuskBossBehaviour.PontiffHuskBoss.AnimatorInyector.PlayHide();
			}
			pontiffHuskBossBehaviour.transform.parent = null;
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			PontiffHuskBossBehaviour o = owner as PontiffHuskBossBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			Core.Audio.Ambient.SetSceneParam("Ending", 1f);
			o.PontiffHuskBoss.Audio.PlayAmbientPostCombat();
			o.PontiffHuskBoss.AnimatorInyector.StopHide();
			o.PontiffHuskBoss.AnimatorInyector.PlayDeath();
			o.ChargedBlastAttack.ClearAll();
			o.PontiffHuskBossScrollManager.Stop();
			WaypointsMovingPlatform platform = o.GetPlatformInDirection(Vector2.down, o.transform.position);
			if (platform == null)
			{
				for (int i = 1; i < 21; i++)
				{
					platform = o.GetPlatformInDirection(Vector2.down + Vector2.right * 0.1f * i, o.transform.position);
					if (platform != null)
					{
						break;
					}
				}
			}
			if (platform == null)
			{
				for (int j = 1; j < 21; j++)
				{
					platform = o.GetPlatformInDirection(Vector2.down + Vector2.left * 0.1f * j, o.transform.position);
					if (platform != null)
					{
						break;
					}
				}
			}
			Vector2 baseExecutionPos;
			if (platform != null)
			{
				baseExecutionPos = ((!(platform.GetDestination().y > platform.GetOrigin().y)) ? platform.GetOrigin() : platform.GetDestination());
				Vector2 executionPos = baseExecutionPos + Vector2.up * 6.5f + Vector2.right * 4f;
				Vector2 moveDir = executionPos - (Vector2)o.transform.position;
				ACT_MOVE.StartAction(o, executionPos, 3f, Ease.InOutQuad);
				yield return ACT_MOVE.waitForCompletion;
			}
			else
			{
				baseExecutionPos = o.transform.position;
				ACT_WAIT.StartAction(o, 3f);
				yield return ACT_WAIT.waitForCompletion;
			}
			yield return new WaitUntil(() => !UIController.instance.GetDialog().IsShowingDialog());
			yield return new WaitUntil(() => p.Status.IsGrounded);
			Core.Dialog.StartConversation("DLG_BS203_05", modal: false, useOnlyLast: false);
			o.PontiffHuskBoss.AnimatorInyector.StopDeath();
			o.executionInstance = PoolManager.Instance.ReuseObject(o.ExecutionPrefab, o.transform.position, o.transform.rotation, createPoolIfNeeded: true).GameObject;
			o.PontiffHuskBoss.DamageArea.enabled = false;
			FakeExecution execution = o.executionInstance.GetComponent<FakeExecution>();
			yield return new WaitUntil(() => execution.BeingUsed);
			o.PontiffHuskBoss.Audio.PlayExecution();
			Core.UI.ShowGamePlayUI = false;
			o.PontiffHuskBossScrollManager.SetExecutionCamBounds();
			ACT_MOVE.StartAction(o, o.DeathTrapLeft.transform.position + Vector3.left * 100f, 3f, Ease.InOutQuad, o.DeathTrapLeft.transform);
			ACT_MOVE.StartAction(o, o.DeathTrapRight.transform.position + Vector3.right * 100f, 3f, Ease.InOutQuad, o.DeathTrapRight.transform);
			p.Shadow.ManuallyControllingAlpha = true;
			Tween t = DOTween.To(() => p.Shadow.GetShadowAlpha(), delegate(float x)
			{
				p.Shadow.SetShadowAlpha(x);
			}, 0f, 0.2f);
			yield return new WaitUntil(() => !execution.BeingUsed);
			Core.Input.SetBlocker("END_BOSS_DEFEATED", blocking: true);
			p.SetOrientation(EntityOrientation.Right);
			Tween t2 = DOTween.To(() => p.Shadow.GetShadowAlpha(), delegate(float x)
			{
				p.Shadow.SetShadowAlpha(x);
			}, 1f, 0.2f);
			t2.OnComplete(delegate
			{
				p.Shadow.ManuallyControllingAlpha = false;
			});
			ACT_WAIT.StartAction(o, 1.75f);
			yield return ACT_WAIT.waitForCompletion;
			Core.Audio.PlaySfx(o.SummaBlasphemiaSfx);
			yield return UIController.instance.ShowFullMessageCourrutine(UIController.FullMensages.EndBossDefeated, 3f, 3f, 2f);
			Core.Input.SetBlocker("END_BOSS_DEFEATED", blocking: false);
			PlayMakerFSM.BroadcastEvent("BOSS DEAD");
			o.HWDialoguePlatform.transform.position = baseExecutionPos + Vector2.down * 7f + Vector2.right * 11.5f;
			Vector3 hwDialoguePlatformTargetPos = o.HWDialoguePlatform.transform.position + Vector3.up * 9f;
			float platformWait = 3f;
			Tween t3 = DOTween.To(() => o.HWDialoguePlatform.transform.position, delegate(Vector3Wrapper x)
			{
				o.HWDialoguePlatform.transform.position = x;
			}, hwDialoguePlatformTargetPos, platformWait);
			t3.SetEase(Ease.InOutQuad);
			Core.Input.SetBlocker("HW_FINAL_PLATFORM_RISING", blocking: true);
			Sequence s = DOTween.Sequence();
			s.AppendInterval(platformWait);
			s.OnComplete(delegate
			{
				Core.Input.SetBlocker("HW_FINAL_PLATFORM_RISING", blocking: false);
			});
			TweenExtensions.Play(s);
			Vector2 hwnpcPos = new Vector2(p.GetPosition().x + o.battleBounds.width * 2f, o.HwNpc.transform.position.y);
			o.HwNpc.transform.position = hwnpcPos + Vector2.right * o.battleBounds.width * 1.1f;
			o.DeathTrapLeft.SetActive(value: false);
			o.DeathTrapRight.SetActive(value: false);
			FinishAction();
			o.gameObject.SetActive(value: false);
		}
	}

	public class BlastsAttack_EnemyAction : EnemyAction
	{
		private Vector2 startingPos;

		private float waitTime;

		private int totalAreas;

		private float distanceBetweenAreas;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 startingPos, float waitTime, int totalAreas = -1, float distanceBetweenAreas = -1f)
		{
			this.startingPos = startingPos;
			this.waitTime = waitTime;
			this.totalAreas = totalAreas;
			this.distanceBetweenAreas = distanceBetweenAreas;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			PontiffHuskBossBehaviour pontiffHuskBossBehaviour = owner as PontiffHuskBossBehaviour;
			pontiffHuskBossBehaviour.PontiffHuskBoss.AnimatorInyector.ResetAll();
			pontiffHuskBossBehaviour.transform.parent = null;
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			PontiffHuskBossBehaviour o = owner as PontiffHuskBossBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			float moveTime = 2f;
			Vector2 target = ((!(Vector2.Distance(p.GetPosition(), o.transform.position) < o.battleBounds.width / 2f)) ? o.ArenaGetTopNearRandomPoint() : o.ArenaGetTopFarRandomPoint());
			ACT_MOVE.StartAction(o, target, moveTime, Ease.InOutQuad);
			ACT_WAIT.StartAction(o, moveTime * 0.9f);
			yield return ACT_WAIT.waitForCompletion;
			o.LookAtTarget();
			yield return ACT_MOVE.waitForCompletion;
			Core.Dialog.StartConversation("DLG_HW_PLACEHOLDER", modal: false, useOnlyLast: false);
			int numSteps = 2;
			for (int i = 0; i < numSteps; i++)
			{
				Vector2 dir = ((i % 2 != 0) ? Vector2.down : Vector2.up);
				o.LookAtTarget();
				ACT_MOVE.StartAction(o, target + dir, waitTime / (float)numSteps, Ease.InOutQuad);
				yield return ACT_MOVE.waitForCompletion;
			}
			FinishAction();
		}
	}

	public class LasersAttack_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			PontiffHuskBossBehaviour pontiffHuskBossBehaviour = owner as PontiffHuskBossBehaviour;
			pontiffHuskBossBehaviour.PontiffHuskBoss.AnimatorInyector.ResetAll();
			pontiffHuskBossBehaviour.LaserAttacks.ForEach(delegate(BossHomingLaserAttack x)
			{
				x.StopBeam();
			});
			pontiffHuskBossBehaviour.transform.parent = null;
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			PontiffHuskBossBehaviour o = owner as PontiffHuskBossBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			o.PontiffHuskBoss.AnimatorInyector.PlayHide();
			for (int i = 0; i < o.LaserAttacks.Count; i++)
			{
				Transform target2 = o.TransformsForLasers[i];
				o.LaserAttacks[i].DelayedTargetedBeam(target2, 1f, 3f);
			}
			ACT_WAIT.StartAction(o, 2f);
			yield return ACT_WAIT.waitForCompletion;
			Vector2 target = o.ArenaGetTopFarRandomPoint();
			o.transform.position = target;
			ACT_WAIT.StartAction(o, 2f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class MachinegunAttack_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			PontiffHuskBossBehaviour pontiffHuskBossBehaviour = owner as PontiffHuskBossBehaviour;
			pontiffHuskBossBehaviour.PontiffHuskBoss.AnimatorInyector.ResetAll();
			pontiffHuskBossBehaviour.MachinegunShooter.StopMachinegun();
			pontiffHuskBossBehaviour.ClearAll();
			pontiffHuskBossBehaviour.transform.parent = null;
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			PontiffHuskBossBehaviour o = owner as PontiffHuskBossBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			o.PontiffHuskBoss.AnimatorInyector.PlayHide();
			ACT_WAIT.StartAction(o, 1f);
			yield return ACT_WAIT.waitForCompletion;
			Vector2 target = o.ArenaGetTopLeftCorner() + Vector2.right;
			o.transform.position = target;
			o.InstantLookAtDir(Vector2.right);
			o.PontiffHuskBoss.AnimatorInyector.StopHide();
			o.PontiffHuskBoss.AnimatorInyector.PlayAltShoot();
			float waitTime2 = 1.75f;
			o.transform.parent = o.CameraAnchor.transform;
			ACT_WAIT.StartAction(o, waitTime2);
			yield return ACT_WAIT.waitForCompletion;
			o.MachinegunShooter.StartAttack(Core.Logic.Penitent.transform);
			o.MachinegunShooter.transform.SetParent(o.transform);
			float attackTime = 6.5f;
			ACT_MOVE.StartAction(o, o.transform.position + Vector3.up, attackTime * 0.5f, Ease.InOutQuad, null, _timeScaled: true, null, _tweenOnX: false);
			yield return ACT_MOVE.waitForCompletion;
			ACT_MOVE.StartAction(o, o.transform.position + Vector3.down, attackTime * 0.5f, Ease.InOutQuad, null, _timeScaled: true, null, _tweenOnX: false);
			yield return ACT_MOVE.waitForCompletion;
			o.PontiffHuskBoss.AnimatorInyector.StopAltShoot();
			waitTime2 = 1f;
			ACT_WAIT.StartAction(o, waitTime2);
			yield return ACT_WAIT.waitForCompletion;
			o.transform.parent = null;
			FinishAction();
		}
	}

	public class BulletHellAttack_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private WaitSeconds_EnemyAction ACT_LONGWAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_LONGWAIT.StopAction();
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			PontiffHuskBossBehaviour pontiffHuskBossBehaviour = owner as PontiffHuskBossBehaviour;
			pontiffHuskBossBehaviour.PontiffHuskBoss.AnimatorInyector.ResetAll();
			pontiffHuskBossBehaviour.ClearAll();
			pontiffHuskBossBehaviour.transform.parent = null;
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			PontiffHuskBossBehaviour o = owner as PontiffHuskBossBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			bool goingToHide = o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("IDLE TO HIDE") || o.PontiffHuskBoss.AnimatorInyector.GetHide();
			bool isHiding = o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("HIDE");
			bool wasHiding = o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("HIDE TO IDLE");
			if (goingToHide || isHiding || wasHiding)
			{
				if (goingToHide)
				{
					yield return new WaitUntil(() => o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("HIDE"));
				}
				if (goingToHide || isHiding)
				{
					o.transform.position = o.ArenaGetTopRightCorner();
					o.InstantLookAtDir(Vector2.left);
				}
				o.PontiffHuskBoss.AnimatorInyector.StopHide();
				yield return new WaitUntil(() => o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("IDLE"));
			}
			o.PontiffHuskBoss.AnimatorInyector.PlayCast();
			Vector2 target = new Vector2(o.battleBounds.center.x, o.battleBounds.yMax);
			float moveTime = Vector2.Distance(target, o.transform.position) * 0.1f + 0.2f;
			target.x += moveTime;
			ACT_MOVE.StartAction(o, target, moveTime, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			int numLoops = 3;
			int numWavesPerLoop = 5;
			float waitTime2 = 0.6f;
			float shootTime = waitTime2 * (float)numLoops * (float)numWavesPerLoop + waitTime2 * 2f * (float)(numLoops + 1);
			ACT_LONGWAIT.StartAction(o, shootTime);
			o.transform.parent = o.CameraAnchor.transform;
			ACT_WAIT.StartAction(o, waitTime2);
			yield return ACT_WAIT.waitForCompletion;
			Vector3 orientationOffset = ((o.PontiffHuskBoss.Status.Orientation != EntityOrientation.Left) ? (Vector3.right * 1.4f) : Vector3.zero);
			float acc = 4f;
			List<AcceleratedProjectile> lastProjectiles = new List<AcceleratedProjectile>();
			Vector3 rotation = Vector3.zero;
			Transform config = o.BulletHellConfigs[0];
			float angleIncrement = 5f;
			config.rotation = Quaternion.Euler(Vector3.zero);
			for (int i = 0; i < 3; i++)
			{
				float rotationDir = ((i % 2 != 0) ? (-1f) : 1f);
				for (int j = 1; j < 6; j++)
				{
					rotation.z += angleIncrement * (float)j * rotationDir;
					config.DORotate(rotation, waitTime2);
					Vector3 origin = config.position + orientationOffset;
					for (int k = 0; k < config.childCount; k++)
					{
						Vector3 normalized = (config.GetChild(k).position - config.position).normalized;
						StraightProjectile straightProjectile = o.AccProjectileAttack.Shoot((Vector2)normalized, (Vector2)origin, (Vector2)normalized, 1f);
						AcceleratedProjectile component = straightProjectile.GetComponent<AcceleratedProjectile>();
						lastProjectiles.Add(component);
						straightProjectile.transform.parent = o.combatAreaParent;
					}
					ACT_WAIT.StartAction(o, waitTime2);
					yield return ACT_WAIT.waitForCompletion;
					origin.x += waitTime2;
					lastProjectiles.ForEach(delegate(AcceleratedProjectile x)
					{
						x.SetAcceleration((x.transform.position - origin).normalized * acc);
					});
					lastProjectiles.Clear();
				}
				ACT_WAIT.StartAction(o, waitTime2 * 2f);
				yield return ACT_WAIT.waitForCompletion;
			}
			yield return ACT_LONGWAIT.waitForCompletion;
			waitTime2 = 1f;
			ACT_WAIT.StartAction(o, waitTime2);
			yield return ACT_WAIT.waitForCompletion;
			o.PontiffHuskBoss.AnimatorInyector.StopCast();
			ACT_WAIT.StartAction(o, waitTime2);
			yield return ACT_WAIT.waitForCompletion;
			o.transform.parent = null;
			FinishAction();
		}
	}

	public class SimpleProjectilesAttack_EnemyAction : EnemyAction
	{
		private int numProjectiles;

		private float chargeTime;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, float chargeTime, int numProjectiles)
		{
			this.chargeTime = chargeTime;
			this.numProjectiles = numProjectiles;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			PontiffHuskBossBehaviour pontiffHuskBossBehaviour = owner as PontiffHuskBossBehaviour;
			pontiffHuskBossBehaviour.PontiffHuskBoss.AnimatorInyector.ResetAll();
			pontiffHuskBossBehaviour.transform.parent = null;
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			PontiffHuskBossBehaviour o = owner as PontiffHuskBossBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			float moveTime = 2f;
			Vector2 target = ((!(Vector2.Distance(p.GetPosition(), o.transform.position) < o.battleBounds.width / 2f)) ? o.ArenaGetTopNearRandomPoint() : o.ArenaGetTopFarRandomPoint());
			target.y += 1.5f;
			ACT_MOVE.StartAction(o, target, moveTime, Ease.InOutQuad);
			ACT_WAIT.StartAction(o, moveTime * 0.9f);
			yield return ACT_WAIT.waitForCompletion;
			o.LookAtTarget();
			yield return ACT_MOVE.waitForCompletion;
			o.PontiffHuskBoss.AnimatorInyector.PlayCharge();
			ACT_WAIT.StartAction(o, chargeTime);
			yield return ACT_WAIT.waitForCompletion;
			o.PontiffHuskBoss.AnimatorInyector.PlayShoot();
			float timeBetweenProjectiles = 0.5f;
			o.transform.parent = o.CameraAnchor.transform;
			ACT_WAIT.StartAction(o, timeBetweenProjectiles);
			yield return ACT_WAIT.waitForCompletion;
			for (int i = 0; i < numProjectiles; i++)
			{
				Vector3 shootPointOffset = Vector3.down * 4f;
				shootPointOffset.x = ((o.PontiffHuskBoss.Status.Orientation != 0) ? (-3f) : 3f);
				Vector2 dir = (Core.Logic.Penitent.GetPosition() - (o.transform.position + shootPointOffset)).normalized;
				dir.x = ((o.PontiffHuskBoss.Status.Orientation != 0) ? Mathf.Min(-0.3f, dir.x) : Mathf.Max(0.3f, dir.x));
				dir.y += (float)i * 0.1f;
				StraightProjectile projectile = o.AccProjectileAttack.Shoot(dir, shootPointOffset);
				AcceleratedProjectile accelProjectile = projectile.GetComponent<AcceleratedProjectile>();
				accelProjectile.SetAcceleration(dir * 10f);
				ACT_WAIT.StartAction(o, timeBetweenProjectiles);
				yield return ACT_WAIT.waitForCompletion;
			}
			o.PontiffHuskBoss.AnimatorInyector.StopShoot();
			o.PontiffHuskBoss.AnimatorInyector.StopCharge();
			ACT_WAIT.StartAction(o, 2f);
			yield return ACT_WAIT.waitForCompletion;
			o.transform.parent = null;
			FinishAction();
		}
	}

	public class WindAttack_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			PontiffHuskBossBehaviour pontiffHuskBossBehaviour = owner as PontiffHuskBossBehaviour;
			pontiffHuskBossBehaviour.PontiffHuskBoss.AnimatorInyector.ResetAll();
			pontiffHuskBossBehaviour.ClearAll();
			pontiffHuskBossBehaviour.transform.parent = null;
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			PontiffHuskBossBehaviour o = owner as PontiffHuskBossBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			o.PontiffHuskBoss.AnimatorInyector.StopHide();
			Vector2 target = ((o.PontiffHuskBoss.Status.Orientation != EntityOrientation.Left) ? (o.ArenaGetTopLeftCorner() + Vector2.right * Vector2.Distance(o.transform.position, o.ArenaGetTopLeftCorner()) * 0.2f) : (o.ArenaGetTopRightCorner() + Vector2.right * Vector2.Distance(o.transform.position, o.ArenaGetTopRightCorner()) * 0.2f));
			float moveTime = Vector2.Distance(target, o.transform.position) * 0.2f + 0.5f;
			ACT_MOVE.StartAction(o, target, moveTime, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			o.PontiffHuskBoss.AnimatorInyector.PlayCharge();
			float chargeTime = 0.3f;
			float windTime = 3f;
			float extensionTime = 1f;
			o.WindSpiralProjectilesAttack.ActivateAttack(8, windTime, extensionTime);
			o.transform.parent = o.CameraAnchor.transform;
			ACT_WAIT.StartAction(o, chargeTime + windTime + extensionTime);
			yield return ACT_WAIT.waitForCompletion;
			o.transform.parent = null;
			o.PontiffHuskBoss.AnimatorInyector.StopCharge();
			FinishAction();
		}
	}

	public class AltSpiralProjectilesAttack_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			PontiffHuskBossBehaviour pontiffHuskBossBehaviour = owner as PontiffHuskBossBehaviour;
			pontiffHuskBossBehaviour.PontiffHuskBoss.AnimatorInyector.ResetAll();
			pontiffHuskBossBehaviour.ClearAll();
			pontiffHuskBossBehaviour.transform.parent = null;
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			PontiffHuskBossBehaviour o = owner as PontiffHuskBossBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			bool goingToHide = o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("IDLE TO HIDE") || o.PontiffHuskBoss.AnimatorInyector.GetHide();
			bool isHiding = o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("HIDE");
			bool wasHiding = o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("HIDE TO IDLE");
			if (goingToHide || isHiding || wasHiding)
			{
				if (goingToHide)
				{
					yield return new WaitUntil(() => o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("HIDE"));
				}
				if (goingToHide || isHiding)
				{
					o.transform.position = o.ArenaGetTopLeftCorner();
					o.InstantLookAtDir(Vector2.right);
				}
				o.PontiffHuskBoss.AnimatorInyector.StopHide();
				yield return new WaitUntil(() => o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("IDLE"));
			}
			Vector2 target = ((o.PontiffHuskBoss.Status.Orientation != EntityOrientation.Left) ? (o.ArenaGetTopLeftCorner() + Vector2.right) : (o.ArenaGetTopRightCorner() + Vector2.left));
			float moveTime = Vector2.Distance(target, o.transform.position) * 0.1f + 0.2f;
			ACT_MOVE.StartAction(o, target, moveTime, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			o.transform.parent = o.CameraAnchor.transform;
			o.PontiffHuskBoss.AnimatorInyector.PlayCharge();
			float chargeTime = 0.3f;
			ACT_WAIT.StartAction(o, chargeTime);
			yield return ACT_WAIT.waitForCompletion;
			float attackTime = 3f;
			float extensionTime = 1f;
			o.WindSpiralProjectilesAttack.transform.localScale = ((o.PontiffHuskBoss.Status.Orientation != 0) ? new Vector3(1f, 1f, 1f) : new Vector3(-1f, 1f, 1f));
			o.WindSpiralProjectilesAttack.ActivateAttack(8, attackTime, extensionTime);
			Transform refTransform = o.WindSpiralProjectilesAttack.spinningTransform;
			refTransform.position = o.WindSpiralProjectilesAttack.transform.position;
			Vector3 refTransformPos = refTransform.position;
			refTransformPos.x = ((o.PontiffHuskBoss.Status.Orientation != EntityOrientation.Left) ? o.ArenaGetTopRightCorner().x : o.ArenaGetTopLeftCorner().x);
			ACT_MOVE.StartAction(o, refTransformPos, attackTime + extensionTime, Ease.InOutQuad, refTransform);
			yield return ACT_MOVE.waitForCompletion;
			o.PontiffHuskBoss.AnimatorInyector.StopCharge();
			o.transform.parent = null;
			FinishAction();
		}
	}

	public class ChargedBlastAttack_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private bool doneBefore;

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			PontiffHuskBossBehaviour pontiffHuskBossBehaviour = owner as PontiffHuskBossBehaviour;
			pontiffHuskBossBehaviour.PontiffHuskBoss.AnimatorInyector.ResetAll();
			pontiffHuskBossBehaviour.transform.parent = null;
			if (pontiffHuskBossBehaviour.CrisantaProtectorInstance != null)
			{
				pontiffHuskBossBehaviour.CrisantaProtectorInstance.SetActive(value: false);
				pontiffHuskBossBehaviour.CrisantaProtectorInstance = null;
			}
			Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
			penitent.Status.Invulnerable = false;
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			PontiffHuskBossBehaviour o = owner as PontiffHuskBossBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			o.PontiffHuskBoss.AnimatorInyector.PlayHide();
			ACT_WAIT.StartAction(o, 1f);
			yield return ACT_WAIT.waitForCompletion;
			Vector2 target = o.ArenaGetBotRightCorner() + Vector2.up * 7.5f;
			o.transform.position = target;
			o.transform.parent = o.CameraAnchor.transform;
			o.InstantLookAtDir(Vector2.left);
			o.PontiffHuskBoss.AnimatorInyector.StopHide();
			o.PontiffHuskBoss.AnimatorInyector.PlayBeam();
			o.PontiffHuskBoss.AnimatorInyector.PlayCharge();
			WaypointsMovingPlatform crisantaPlatform = o.GetPlatformInDirection(Vector2.down, o.battleBounds.center + Vector2.right * 3f);
			if (crisantaPlatform == null)
			{
				for (int k = 1; k < 21; k++)
				{
					crisantaPlatform = o.GetPlatformInDirection(Vector2.down + Vector2.right * 0.1f * k, o.battleBounds.center);
					if (crisantaPlatform != null)
					{
						break;
					}
				}
			}
			if (crisantaPlatform == null)
			{
				for (int l = 1; l < 21; l++)
				{
					crisantaPlatform = o.GetPlatformInDirection(Vector2.down + Vector2.left * 0.1f * l, o.battleBounds.center);
					if (crisantaPlatform != null)
					{
						break;
					}
				}
			}
			if (crisantaPlatform == null)
			{
				Debug.LogError("No platform for Crisanta!");
			}
			Vector2 crisantaPos = ((!(crisantaPlatform.GetDestination().y > crisantaPlatform.GetOrigin().y)) ? crisantaPlatform.GetOrigin() : crisantaPlatform.GetDestination());
			crisantaPos += Vector2.right * 4f;
			for (int j = 0; j < 10; j++)
			{
				if (crisantaPos.y <= crisantaPlatform.transform.position.y)
				{
					break;
				}
				if (Mathf.Approximately(crisantaPos.y, crisantaPlatform.transform.position.y))
				{
					break;
				}
				ACT_WAIT.StartAction(o, 0.1f);
				yield return ACT_WAIT.waitForCompletion;
			}
			GameObject prefab = ((!doneBefore) ? o.CrisantaProtectorPrefabFT : o.CrisantaProtectorPrefab);
			o.CrisantaProtectorInstance = PoolManager.Instance.ReuseObject(prefab, crisantaPos, Quaternion.identity, createPoolIfNeeded: true).GameObject;
			PlayableDirector playableDirector = o.CrisantaProtectorInstance.GetComponent<PlayableDirector>();
			playableDirector.Play();
			if (!doneBefore)
			{
				o.PontiffHuskBoss.Audio.PlayChargedBlastNoVoice();
			}
			else
			{
				o.PontiffHuskBoss.Audio.PlayChargedBlast();
			}
			float chargeTime = 2f;
			ACT_WAIT.StartAction(o, chargeTime * 0.2f);
			yield return ACT_WAIT.waitForCompletion;
			if (!doneBefore)
			{
				doneBefore = true;
				Core.Dialog.StartConversation("DLG_12210", modal: false, useOnlyLast: false);
				ACT_WAIT.StartAction(o, 2f);
				yield return ACT_WAIT.waitForCompletion;
			}
			ACT_WAIT.StartAction(o, chargeTime * 0.8f);
			yield return ACT_WAIT.waitForCompletion;
			o.PontiffHuskBoss.AnimatorInyector.PlayShoot();
			Vector2 shootingPoint = o.transform.position + Vector3.down * 4.2f + Vector3.left * 2f;
			GameObject areago = o.ChargedBlastAttack.SummonAreaOnPoint(shootingPoint);
			areago.transform.parent = o.combatAreaParent;
			ACT_WAIT.StartAction(o, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			PoolManager.Instance.ReuseObject(o.ChargedBlastExplosion, shootingPoint + Vector2.right * 2f, Quaternion.identity, createPoolIfNeeded: true);
			float beamTime = 1.5f;
			int numSteps = 10;
			for (int i = 0; i < numSteps; i++)
			{
				ACT_WAIT.StartAction(o, beamTime / (float)numSteps);
				yield return ACT_WAIT.waitForCompletion;
				if (Vector2.Distance(o.CrisantaProtectorInstance.transform.position, p.GetPosition()) < 4f && p.GetPosition().x < o.CrisantaProtectorInstance.transform.position.x && !p.IsJumping)
				{
					p.Status.Invulnerable = true;
				}
				else
				{
					p.Status.Invulnerable = false;
				}
			}
			o.transform.parent = null;
			o.PontiffHuskBoss.AnimatorInyector.StopShoot();
			o.PontiffHuskBoss.AnimatorInyector.StopCharge();
			ACT_WAIT.StartAction(o, 2.5f);
			yield return ACT_WAIT.waitForCompletion;
			p.Status.Invulnerable = false;
			o.PontiffHuskBoss.AnimatorInyector.PlayHide();
			ACT_WAIT.StartAction(o, 1.5f);
			yield return ACT_WAIT.waitForCompletion;
			o.CrisantaProtectorInstance = null;
			FinishAction();
		}
	}

	public class SpiralProjectilesAttack_EnemyAction : EnemyAction
	{
		private int numBalls;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			PontiffHuskBossBehaviour pontiffHuskBossBehaviour = owner as PontiffHuskBossBehaviour;
			pontiffHuskBossBehaviour.PontiffHuskBoss.AnimatorInyector.ResetAll();
			pontiffHuskBossBehaviour.ClearAll();
			pontiffHuskBossBehaviour.transform.parent = null;
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, int numBalls)
		{
			this.numBalls = numBalls;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			PontiffHuskBossBehaviour o = owner as PontiffHuskBossBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			bool goingToHide = o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("IDLE TO HIDE") || o.PontiffHuskBoss.AnimatorInyector.GetHide();
			bool isHiding = o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("HIDE");
			bool wasHiding = o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("HIDE TO IDLE");
			if (goingToHide || isHiding || wasHiding)
			{
				if (goingToHide)
				{
					yield return new WaitUntil(() => o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("HIDE"));
				}
				if (goingToHide || isHiding)
				{
					o.transform.position = o.ArenaGetTopRightCorner();
					o.InstantLookAtDir(Vector2.left);
				}
				o.PontiffHuskBoss.AnimatorInyector.StopHide();
				yield return new WaitUntil(() => o.PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("IDLE"));
			}
			Vector2 target2 = ((!o.IsBossOnTheRightSide()) ? (o.ArenaGetTopLeftCorner() + Vector2.right * 2f) : (o.ArenaGetTopRightCorner() + Vector2.left * 1f));
			target2.y -= 1f;
			float moveTime2 = Vector2.Distance(target2, o.transform.position) * 0.2f;
			o.LookAtDir(o.battleBounds.center - target2);
			bool turnQeued = o.PontiffHuskBoss.AnimatorInyector.IsTurnQeued();
			moveTime2 += ((!turnQeued) ? 0.5f : 0f);
			ACT_MOVE.StartAction(o, target2, moveTime2, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			Vector2 attackTarget = ((!o.IsBossOnTheRightSide()) ? (o.ArenaGetTopRightCorner() + Vector2.right * 3f) : (o.ArenaGetTopLeftCorner() + Vector2.right * 6f));
			attackTarget.y -= 1f;
			moveTime2 = Vector2.Distance(attackTarget, o.transform.position) * 0.2f + 1f;
			o.PontiffHuskBoss.AnimatorInyector.PlayCharge();
			o.SpiralProjectilesAttack.transform.localScale = ((o.PontiffHuskBoss.Status.Orientation != 0) ? new Vector3(-1f, 1f, 1f) : new Vector3(1f, 1f, 1f));
			float extensionTime = 1f;
			o.SpiralProjectilesAttack.ActivateAttack(numBalls, moveTime2, extensionTime);
			float waitTime = ((!turnQeued) ? 0.5f : 1.5f);
			o.transform.parent = o.CameraAnchor.transform;
			ACT_WAIT.StartAction(o, waitTime);
			yield return ACT_WAIT.waitForCompletion;
			o.transform.parent = null;
			target2 = attackTarget;
			ACT_MOVE.StartAction(o, target2, moveTime2, Ease.InQuad);
			ACT_WAIT.StartAction(o, moveTime2 - 1f);
			yield return ACT_WAIT.waitForCompletion;
			o.LookAtDir(o.battleBounds.center - target2);
			yield return ACT_MOVE.waitForCompletion;
			o.PontiffHuskBoss.AnimatorInyector.StopCharge();
			o.transform.parent = o.CameraAnchor.transform;
			ACT_WAIT.StartAction(o, extensionTime);
			yield return ACT_WAIT.waitForCompletion;
			o.transform.parent = null;
			FinishAction();
		}
	}

	[FoldoutGroup("Character settings", 0)]
	public AnimationCurve timeSlowCurve;

	[FoldoutGroup("Battle area", 0)]
	public Rect battleBounds;

	[FoldoutGroup("Battle area", 0)]
	public Transform combatAreaParent;

	[FoldoutGroup("Battle area", 0)]
	public PontiffHuskBossAnchor CameraAnchor;

	[FoldoutGroup("Battle config", 0)]
	public List<PontiffHuskBossFightParameters> allFightParameters;

	[FoldoutGroup("Attacks config", 0)]
	public PontiffHuskBossScriptableConfig attackConfigData;

	[FoldoutGroup("Debug", 0)]
	public bool debugDrawCurrentAction;

	[FoldoutGroup("Attack references", 0)]
	public BossStraightProjectileAttack AccProjectileAttack;

	[FoldoutGroup("Attack references", 0)]
	public BossAreaSummonAttack ChargedBlastAttack;

	[FoldoutGroup("Attack references", 0)]
	public BossSpiralProjectiles SpiralProjectilesAttack;

	[FoldoutGroup("Attack references", 0)]
	public BossSpiralProjectiles WindSpiralProjectilesAttack;

	[FoldoutGroup("Attack references", 0)]
	public BossMachinegunShooter MachinegunShooter;

	[FoldoutGroup("Attack references", 0)]
	public GameObject CrisantaProtectorPrefabFT;

	[FoldoutGroup("Attack references", 0)]
	public GameObject CrisantaProtectorPrefab;

	[FoldoutGroup("Attack references", 0)]
	public List<BossHomingLaserAttack> LaserAttacks;

	[FoldoutGroup("Attack references", 0)]
	public List<Transform> TransformsForLasers;

	[FoldoutGroup("Attack references", 0)]
	public List<Transform> BulletHellConfigs;

	[FoldoutGroup("Attack references", 0)]
	public GameObject ChargedBlastExplosion;

	[FoldoutGroup("Ending references", 0)]
	public GameObject ExecutionPrefab;

	[FoldoutGroup("Ending references", 0)]
	public GameObject HwNpc;

	[FoldoutGroup("Ending references", 0)]
	public PontiffHuskBossScrollManager PontiffHuskBossScrollManager;

	[FoldoutGroup("Ending references", 0)]
	public LayerMask FloorMask;

	[FoldoutGroup("Ending references", 0)]
	public GameObject DeathTrapLeft;

	[FoldoutGroup("Ending references", 0)]
	public GameObject DeathTrapRight;

	[FoldoutGroup("Ending references", 0)]
	public GameObject HWDialoguePlatform;

	[FoldoutGroup("Ending references", 0)]
	[EventRef]
	public string SummaBlasphemiaSfx = "event:/Key Event/BossBattleEnd";

	private List<PH_ATTACKS> availableAttacks = new List<PH_ATTACKS>();

	[ShowInInspector]
	private List<PH_ATTACKS> queuedAttacks = new List<PH_ATTACKS>();

	private PontiffHuskBossFightParameters currentFightParameters;

	private EnemyAction currentAction;

	private PH_ATTACKS lastAttack = PH_ATTACKS.DUMMY;

	private PH_ATTACKS secondLastAttack = PH_ATTACKS.DUMMY;

	private Dictionary<PH_ATTACKS, Func<EnemyAction>> actionsDictionary = new Dictionary<PH_ATTACKS, Func<EnemyAction>>();

	private float extraRecoverySeconds;

	private PontiffHuskBossMeleeAttack currentMeleeAttack;

	private RaycastHit2D[] results = new RaycastHit2D[1];

	private Vector2 bossStartPos;

	private Vector2 deathTrapLeftStartPos;

	private Vector2 deathTrapRightStartPos;

	private GameObject executionInstance;

	[HideInInspector]
	public GameObject CrisantaProtectorInstance;

	private bool firstTimeDialogDone;

	private SceneAudio sceneAudio;

	private WaitSeconds_EnemyAction waitBetweenActions_EA;

	private BlastsAttack_EnemyAction blastsAttack_EA;

	private LasersAttack_EnemyAction lasersAttack_EA;

	private MachinegunAttack_EnemyAction machinegunAttack_EA;

	private BulletHellAttack_EnemyAction bulletHellAttack_EA;

	private SimpleProjectilesAttack_EnemyAction simpleProjectileAttack_EA;

	private WindAttack_EnemyAction windAttack_EA;

	private ChargedBlastAttack_EnemyAction chargedBlastAttack_EA;

	private SpiralProjectilesAttack_EnemyAction spiralProjectilesAttack_EA;

	private AltSpiralProjectilesAttack_EnemyAction altSpiralProjectilesAttack_EA;

	private Intro_EnemyAction intro_EA;

	private Death_EnemyAction death_EA;

	private StateMachine<PontiffHuskBossBehaviour> _fsm;

	private State<PontiffHuskBossBehaviour> stIdle;

	private State<PontiffHuskBossBehaviour> stAction;

	public PontiffHuskBoss PontiffHuskBoss { get; set; }

	private void Start()
	{
		PontiffHuskBoss = (PontiffHuskBoss)Entity;
		InitAI();
		InitActionDictionary();
		InitCombatArea();
		currentFightParameters = allFightParameters[0];
		bossStartPos = base.transform.position;
		deathTrapLeftStartPos = DeathTrapLeft.transform.position;
		deathTrapRightStartPos = DeathTrapRight.transform.position;
		PoolManager.Instance.CreatePool(ExecutionPrefab, 1);
	}

	private void InitCombatArea()
	{
		combatAreaParent.SetParent(null);
		combatAreaParent.transform.position = battleBounds.center;
		CameraAnchor.transform.SetParent(null);
		CameraAnchor.ReferenceTransform = UnityEngine.Camera.main.transform;
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(0.2f);
		sequence.OnStepComplete(delegate
		{
			battleBounds.center = combatAreaParent.transform.position;
		});
		sequence.SetLoops(-1);
		TweenExtensions.Play(sequence);
	}

	private void OnGUI()
	{
	}

	private void InitAI()
	{
		stIdle = new PontiffHuskBoss_StIdle();
		stAction = new PontiffHuskBoss_StAction();
		_fsm = new StateMachine<PontiffHuskBossBehaviour>(this, stIdle);
	}

	private void InitActionDictionary()
	{
		waitBetweenActions_EA = new WaitSeconds_EnemyAction();
		intro_EA = new Intro_EnemyAction();
		death_EA = new Death_EnemyAction();
		blastsAttack_EA = new BlastsAttack_EnemyAction();
		lasersAttack_EA = new LasersAttack_EnemyAction();
		machinegunAttack_EA = new MachinegunAttack_EnemyAction();
		bulletHellAttack_EA = new BulletHellAttack_EnemyAction();
		simpleProjectileAttack_EA = new SimpleProjectilesAttack_EnemyAction();
		windAttack_EA = new WindAttack_EnemyAction();
		chargedBlastAttack_EA = new ChargedBlastAttack_EnemyAction();
		spiralProjectilesAttack_EA = new SpiralProjectilesAttack_EnemyAction();
		altSpiralProjectilesAttack_EA = new AltSpiralProjectilesAttack_EnemyAction();
		actionsDictionary.Add(PH_ATTACKS.BLASTS_ATTACK, LaunchAction_BlastsAttack);
		actionsDictionary.Add(PH_ATTACKS.SIMPLE_PROJECTILES, LaunchAction_SimpleProjectileAttack);
		actionsDictionary.Add(PH_ATTACKS.WIND_ATTACK, LaunchAction_WindAttack);
		actionsDictionary.Add(PH_ATTACKS.CHARGED_BLAST, LaunchAction_ChargedBlastAttack);
		actionsDictionary.Add(PH_ATTACKS.SPIRAL_PROJECTILES, LaunchAction_SpiralProjectilesAttack);
		actionsDictionary.Add(PH_ATTACKS.LASERS_ATTACK, LaunchAction_LasersAttack);
		actionsDictionary.Add(PH_ATTACKS.MACHINEGUN_ATTACK, LaunchAction_MachinegunAttack);
		actionsDictionary.Add(PH_ATTACKS.BULLET_HELL_ATTACK, LaunchAction_BulletHellAttack);
		actionsDictionary.Add(PH_ATTACKS.ALT_SPIRAL_PROJECTILES, LaunchAction_AltSpiralAttack);
		availableAttacks = attackConfigData.GetAttackIds(onlyActive: true, useHP: true, 1f);
	}

	public void Damage(Hit hit)
	{
		if (SwapFightParametersIfNeeded())
		{
			extraRecoverySeconds = 2f;
			float hpPercentage = PontiffHuskBoss.GetHpPercentage();
			if (hpPercentage < 0.33f)
			{
				Core.Dialog.StartConversation("DLG_BS203_04", modal: false, useOnlyLast: false);
			}
			else if (hpPercentage < 0.66f)
			{
				Core.Dialog.StartConversation("DLG_BS203_03", modal: false, useOnlyLast: false);
			}
			QueueAttack(PH_ATTACKS.CHARGED_BLAST);
			if (UIController.instance.GetDialog().IsShowingDialog() && secondLastAttack != PH_ATTACKS.DUMMY)
			{
				QueueAttack(secondLastAttack);
			}
		}
	}

	private void QueueAttack(PH_ATTACKS atk)
	{
		queuedAttacks.Add(atk);
	}

	private PH_ATTACKS PopAttackFromQueue()
	{
		PH_ATTACKS pH_ATTACKS = PH_ATTACKS.DUMMY;
		int num = queuedAttacks.Count - 1;
		if (num >= 0)
		{
			pH_ATTACKS = queuedAttacks[num];
			queuedAttacks.RemoveAt(num);
		}
		if (pH_ATTACKS == PH_ATTACKS.DUMMY)
		{
			pH_ATTACKS = lastAttack;
		}
		return pH_ATTACKS;
	}

	private bool SwapFightParametersIfNeeded()
	{
		bool result = false;
		float hpPercentage = PontiffHuskBoss.GetHpPercentage();
		availableAttacks = attackConfigData.GetAttackIds(onlyActive: true, useHP: true, hpPercentage);
		for (int i = 0; i < allFightParameters.Count; i++)
		{
			if (allFightParameters[i].hpPercentageBeforeApply < currentFightParameters.hpPercentageBeforeApply && allFightParameters[i].hpPercentageBeforeApply > hpPercentage && !currentFightParameters.Equals(allFightParameters[i]))
			{
				currentFightParameters = allFightParameters[i];
				result = true;
				break;
			}
		}
		return result;
	}

	public void FlipOrientation()
	{
		EntityOrientation orientation = PontiffHuskBoss.Status.Orientation;
		EntityOrientation orientation2 = ((orientation != EntityOrientation.Left) ? EntityOrientation.Left : EntityOrientation.Right);
		PontiffHuskBoss.SetOrientation(orientation2);
	}

	private Vector2 GetDirToPenitent()
	{
		return (Vector2)Core.Logic.Penitent.transform.position - (Vector2)base.transform.position;
	}

	private void InstantLookAtDir(Vector2 v)
	{
		PontiffHuskBoss.SetOrientation((!(v.x > 0f)) ? EntityOrientation.Left : EntityOrientation.Right);
	}

	public void LookAtTarget()
	{
		LookAtDir(GetDirToPenitent());
	}

	private void LookAtDir(Vector2 v)
	{
		if (ShouldTurn(v))
		{
			PontiffHuskBoss.AnimatorInyector.PlayTurn();
		}
	}

	public bool ShouldTurnToLookAtTarget()
	{
		return ShouldTurn(GetDirToPenitent());
	}

	public bool ShouldTurn(Vector2 lookAtDir)
	{
		bool result = false;
		if (!PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("TURN"))
		{
			bool flag = PontiffHuskBoss.Status.Orientation == EntityOrientation.Right;
			bool flag2 = lookAtDir.x > 0f;
			result = flag != flag2;
		}
		return result;
	}

	public bool IsBossOnTheRightSide()
	{
		return PontiffHuskBoss.transform.position.x > battleBounds.center.x;
	}

	public Vector2 ArenaGetBotRightCorner()
	{
		return new Vector2(battleBounds.xMax, battleBounds.yMin);
	}

	public Vector2 ArenaGetBotLeftCorner()
	{
		return new Vector2(battleBounds.xMin, battleBounds.yMin);
	}

	public Vector2 ArenaGetTopRightCorner()
	{
		return new Vector2(battleBounds.xMax, battleBounds.yMax);
	}

	public Vector2 ArenaGetTopLeftCorner()
	{
		return new Vector2(battleBounds.xMin, battleBounds.yMax);
	}

	public Vector2 ArenaGetBotFarRandomPoint()
	{
		Vector2 zero = Vector2.zero;
		zero.y = battleBounds.yMin;
		if (IsBossOnTheRightSide())
		{
			zero.x = UnityEngine.Random.Range(Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.3f), Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.5f));
		}
		else
		{
			zero.x = UnityEngine.Random.Range(Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.6f), Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.9f));
		}
		return zero;
	}

	public Vector2 ArenaGetBotNearRandomPoint()
	{
		Vector2 zero = Vector2.zero;
		zero.y = battleBounds.yMin;
		if (IsBossOnTheRightSide())
		{
			zero.x = UnityEngine.Random.Range(Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.5f), Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 1f));
		}
		else
		{
			zero.x = UnityEngine.Random.Range(Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.2f), Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.5f));
		}
		return zero;
	}

	public Vector2 ArenaGetTopFarRandomPoint()
	{
		Vector2 zero = Vector2.zero;
		zero.y = battleBounds.yMax;
		if (IsBossOnTheRightSide())
		{
			zero.x = UnityEngine.Random.Range(Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.3f), Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.5f));
		}
		else
		{
			zero.x = UnityEngine.Random.Range(Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.6f), Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.9f));
		}
		return zero;
	}

	public Vector2 ArenaGetTopNearRandomPoint()
	{
		Vector2 zero = Vector2.zero;
		zero.y = battleBounds.yMax;
		if (IsBossOnTheRightSide())
		{
			zero.x = UnityEngine.Random.Range(Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.5f), Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 1f));
		}
		else
		{
			zero.x = UnityEngine.Random.Range(Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.2f), Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.5f));
		}
		return zero;
	}

	private WaypointsMovingPlatform GetPlatformInDirection(Vector2 direction, Vector2 position)
	{
		WaypointsMovingPlatform waypointsMovingPlatform = null;
		float num = 20f;
		float num2 = 0.5f;
		float num3 = 3f;
		if (Physics2D.RaycastNonAlloc(position, direction, results, num, FloorMask) > 0)
		{
			GizmoExtensions.DrawDebugCross(results[0].transform.position, Color.yellow, 10f);
			if (results[0].transform.position.x + num3 < battleBounds.xMax && results[0].transform.position.x - num2 > battleBounds.xMin && results[0].transform.position.y > battleBounds.yMin)
			{
				waypointsMovingPlatform = results[0].transform.gameObject.GetComponent<WaypointsMovingPlatform>();
				GizmoExtensions.DrawDebugCross(waypointsMovingPlatform.transform.position, Color.green, 10f);
			}
		}
		else
		{
			GizmoExtensions.DrawDebugCross(position + direction * num, Color.red, 10f);
		}
		return waypointsMovingPlatform;
	}

	private void Update()
	{
		_fsm.DoUpdate();
	}

	private void LaunchAutomaticAction()
	{
		PH_ATTACKS pH_ATTACKS = PH_ATTACKS.DUMMY;
		List<PH_ATTACKS> filteredAttacks = GetFilteredAttacks(availableAttacks);
		if (queuedAttacks.Count > 0)
		{
			pH_ATTACKS = PopAttackFromQueue();
		}
		else if (filteredAttacks.Count > 0)
		{
			int index = RandomizeUsingWeights(filteredAttacks);
			pH_ATTACKS = filteredAttacks[index];
		}
		else
		{
			pH_ATTACKS = PH_ATTACKS.LASERS_ATTACK;
		}
		LaunchAction(pH_ATTACKS);
		secondLastAttack = lastAttack;
		lastAttack = pH_ATTACKS;
	}

	private List<PH_ATTACKS> GetFilteredAttacks(List<PH_ATTACKS> originalList)
	{
		List<PH_ATTACKS> list = new List<PH_ATTACKS>(originalList);
		PontiffHuskBossScriptableConfig.PontiffHuskBossAttackConfig atkConfig = attackConfigData.GetAttackConfig(lastAttack);
		if (atkConfig.cantBeFollowedBy != null && atkConfig.cantBeFollowedBy.Count > 0)
		{
			list.RemoveAll((PH_ATTACKS x) => atkConfig.cantBeFollowedBy.Contains(x));
		}
		if (atkConfig.alwaysFollowedBy != null && atkConfig.alwaysFollowedBy.Count > 0)
		{
			list.RemoveAll((PH_ATTACKS x) => !atkConfig.alwaysFollowedBy.Contains(x));
		}
		if (base.transform.position.x < battleBounds.center.x)
		{
			list.Remove(PH_ATTACKS.MACHINEGUN_ATTACK);
		}
		if (list.Count > 2)
		{
			list.Remove(secondLastAttack);
		}
		if (list.Count > 1)
		{
			list.Remove(lastAttack);
		}
		return list;
	}

	private int RandomizeUsingWeights(List<PH_ATTACKS> filteredAtks)
	{
		float hpPercentage = PontiffHuskBoss.GetHpPercentage();
		List<float> filteredAttacksWeights = attackConfigData.GetFilteredAttacksWeights(filteredAtks, useHP: true, hpPercentage);
		float max = filteredAttacksWeights.Sum();
		float num = UnityEngine.Random.Range(0f, max);
		float num2 = 0f;
		for (int i = 0; i < filteredAtks.Count; i++)
		{
			num2 += filteredAttacksWeights[i];
			if (num2 > num)
			{
				return i;
			}
		}
		return 0;
	}

	private void StopCurrentAction()
	{
		if (currentAction != null)
		{
			currentAction.StopAction();
		}
	}

	protected void LaunchAction(PH_ATTACKS action)
	{
		StopCurrentAction();
		_fsm.ChangeState(stAction);
		currentAction = actionsDictionary[action]();
		currentAction.OnActionEnds -= CurrentAction_OnActionEnds;
		currentAction.OnActionIsStopped -= CurrentAction_OnActionStops;
		currentAction.OnActionEnds += CurrentAction_OnActionEnds;
		currentAction.OnActionIsStopped += CurrentAction_OnActionStops;
	}

	public void StartIntro()
	{
		StopCurrentAction();
		_fsm.ChangeState(stAction);
		currentAction = intro_EA.StartAction(this);
		QueueAttack(PH_ATTACKS.SPIRAL_PROJECTILES);
		currentAction.OnActionEnds -= CurrentAction_OnActionEnds;
		currentAction.OnActionIsStopped -= CurrentAction_OnActionStops;
		currentAction.OnActionEnds += CurrentAction_OnActionEnds;
		currentAction.OnActionIsStopped += CurrentAction_OnActionStops;
	}

	public void ResetCombat()
	{
		StopCurrentAction();
		if (sceneAudio == null)
		{
			sceneAudio = UnityEngine.Object.FindObjectOfType<SceneAudio>();
		}
		if (PontiffHuskBoss.Status.Dead)
		{
			sceneAudio.RestartSceneAudio();
		}
		PontiffHuskBoss.Stats.Life.SetToCurrentMax();
		PontiffHuskBoss.Status.Dead = false;
		availableAttacks = attackConfigData.GetAttackIds(onlyActive: true, useHP: true, 1f);
		currentFightParameters = allFightParameters[0];
		queuedAttacks.Clear();
		QueueAttack(PH_ATTACKS.SPIRAL_PROJECTILES);
		PontiffHuskBoss.AnimatorInyector.PlayHide();
		Core.Audio.Ambient.SetSceneParam("Ending", 0f);
		if (firstTimeDialogDone)
		{
			Core.Audio.Ambient.SetSceneParam("Combat", 1f);
		}
		PontiffHuskBoss.Audio.StopAmbientPostCombat();
		ChargedBlastAttack.ClearAll();
		if (CrisantaProtectorInstance != null)
		{
			CrisantaProtectorInstance.SetActive(value: false);
			CrisantaProtectorInstance = null;
		}
		if (executionInstance != null)
		{
			executionInstance.SetActive(value: false);
			executionInstance = null;
		}
		InstantLookAtDir(Vector2.left);
	}

	private int GetAttackNumberOfRepetitions(PontiffHuskBossScriptableConfig.PontiffHuskBossAttackConfig config)
	{
		float hpPercentage = PontiffHuskBoss.GetHpPercentage();
		if (hpPercentage > 0.66f)
		{
			return config.repetitions1;
		}
		if (hpPercentage > 0.33f)
		{
			return config.repetitions2;
		}
		return config.repetitions3;
	}

	protected void LaunchAction_Death()
	{
		currentAction = death_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_BlastsAttack()
	{
		Vector3 position = Core.Logic.Penitent.GetPosition();
		position.y = battleBounds.yMax;
		return blastsAttack_EA.StartAction(this, position, 2f, 2, 2f);
	}

	protected EnemyAction LaunchAction_LasersAttack()
	{
		return lasersAttack_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_MachinegunAttack()
	{
		return machinegunAttack_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_BulletHellAttack()
	{
		return bulletHellAttack_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_SimpleProjectileAttack()
	{
		PontiffHuskBossScriptableConfig.PontiffHuskBossAttackConfig attackConfig = attackConfigData.GetAttackConfig(PH_ATTACKS.SIMPLE_PROJECTILES);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return simpleProjectileAttack_EA.StartAction(this, 1f, attackNumberOfRepetitions);
	}

	protected EnemyAction LaunchAction_WindAttack()
	{
		return windAttack_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_AltSpiralAttack()
	{
		return altSpiralProjectilesAttack_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_ChargedBlastAttack()
	{
		return chargedBlastAttack_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_SpiralProjectilesAttack()
	{
		PontiffHuskBossScriptableConfig.PontiffHuskBossAttackConfig attackConfig = attackConfigData.GetAttackConfig(PH_ATTACKS.SPIRAL_PROJECTILES);
		int attackNumberOfRepetitions = GetAttackNumberOfRepetitions(attackConfig);
		return spiralProjectilesAttack_EA.StartAction(this, attackNumberOfRepetitions);
	}

	private void CurrentAction_OnActionStops(EnemyAction e)
	{
	}

	private void CurrentAction_OnActionEnds(EnemyAction e)
	{
		e.OnActionEnds -= CurrentAction_OnActionEnds;
		e.OnActionIsStopped -= CurrentAction_OnActionStops;
		if (e != waitBetweenActions_EA)
		{
			if (!PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("HIDE") && !PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("IDLE TO HIDE") && !PontiffHuskBoss.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName("HIDE TO IDLE") && ShouldTurnToLookAtTarget())
			{
				PontiffHuskBoss.AnimatorInyector.PlayHide();
			}
			WaitBetweenActions();
		}
		else if (Core.Logic.Penitent.GetPosition().y < battleBounds.yMin || Core.Logic.Penitent.GetPosition().x < battleBounds.xMin || Core.Logic.Penitent.GetPosition().x > battleBounds.xMax)
		{
			PontiffHuskBoss.AnimatorInyector.PlayHide();
			InstantLookAtDir(Vector2.left);
			WaitBetweenActions();
		}
		else
		{
			LaunchAutomaticAction();
		}
	}

	private void WaitBetweenActions()
	{
		_fsm.ChangeState(stIdle);
		StartWait(extraRecoverySeconds + currentFightParameters.minMaxWaitingTimeBetweenActions.x, extraRecoverySeconds + currentFightParameters.minMaxWaitingTimeBetweenActions.y);
		extraRecoverySeconds = 0f;
	}

	private void StartWait(float min, float max)
	{
		StopCurrentAction();
		currentAction = waitBetweenActions_EA.StartAction(this, min, max);
		currentAction.OnActionEnds -= CurrentAction_OnActionEnds;
		currentAction.OnActionEnds += CurrentAction_OnActionEnds;
		base.transform.DOMoveX(base.transform.position.x + min, min).SetEase(Ease.InOutQuad);
		PontiffHuskBoss.AnimatorInyector.ResetAll();
	}

	private void CheckDebugActions()
	{
		Dictionary<KeyCode, PH_ATTACKS> debugActions = attackConfigData.debugActions;
		if (debugActions == null)
		{
			return;
		}
		foreach (KeyCode key in debugActions.Keys)
		{
			if (Input.GetKeyDown(key))
			{
				QueueAttack(debugActions[key]);
			}
		}
	}

	public void OnMeleeAttackStarts()
	{
		currentMeleeAttack.dealsDamage = true;
		currentMeleeAttack.CurrentWeaponAttack();
	}

	public void OnMeleeAttackFinished()
	{
		if ((bool)currentMeleeAttack)
		{
			currentMeleeAttack.dealsDamage = false;
		}
	}

	public void DoActivateCollisions(bool b)
	{
		PontiffHuskBoss.DamageArea.DamageAreaCollider.enabled = b;
	}

	public void DoActivateGuard(bool b)
	{
		PontiffHuskBoss.IsGuarding = b;
	}

	public override void Damage()
	{
		throw new NotImplementedException();
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}

	public void Death()
	{
		StopCurrentAction();
		StopAllCoroutines();
		base.transform.DOKill();
		ClearAll();
		LaunchAction_Death();
	}

	private void ClearAll()
	{
		GameplayUtils.DestroyAllProjectiles();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(battleBounds.center, new Vector3(battleBounds.width, battleBounds.height, 0f));
		Gizmos.DrawWireCube(ArenaGetBotLeftCorner(), Vector3.one * 0.1f);
		Gizmos.DrawWireCube(ArenaGetBotRightCorner(), Vector3.one * 0.1f);
	}
}
