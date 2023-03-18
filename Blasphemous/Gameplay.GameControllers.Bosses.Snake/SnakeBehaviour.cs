using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Enemies.HomingTurret.Attack;
using Gameplay.GameControllers.Enemies.Projectiles;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Environment.AreaEffects;
using Gameplay.GameControllers.Environment.Traps.FireTrap;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Attack;
using Gameplay.UI.Widgets;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using Rewired;
using Sirenix.OdinInspector;
using Tools.Level.Interactables;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Snake;

public class SnakeBehaviour : EnemyBehaviour
{
	public enum SNAKE_ATTACKS
	{
		INTRO = 0,
		CHARGED_BITE = 1,
		SCALES_SPIKES = 2,
		CHAINED_LIGHTNING = 4,
		GO_UP = 6,
		DEATH = 7,
		TAIL_ORBS = 8,
		TAIL_BEAM = 9,
		SEEKING_CHAINED_LIGHTNING = 10,
		BIG_CHAINED_LIGHTNING = 11,
		SCALES_SPIKES_OBSTACLES = 12,
		DUMMY = 999
	}

	public enum SNAKE_WEAPONS
	{
		CHARGING_OPEN_MOUTH,
		OPEN_TO_CLOSED,
		CASTING_OPEN_MOUTH,
		CHARGED_BITE
	}

	public enum SNAKE_PHASE
	{
		FIRST,
		SECOND,
		THIRD
	}

	[Serializable]
	public struct SnakeFightParameters
	{
		[EnumToggleButtons]
		public SNAKE_PHASE Phase;

		[ProgressBar(0.0, 1.0, 0.8f, 0f, 0.1f)]
		[SuffixLabel("%", false)]
		public float HpPercentageBeforeApply;

		[MinMaxSlider(0f, 5f, true)]
		public Vector2 MinMaxWaitingTimeBetweenActions;

		[SuffixLabel("health", true)]
		public int MaxHealthLostBeforeCounter;
	}

	[Serializable]
	public struct OrbsSettings
	{
		public int NumOrbs;

		[EnumToggleButtons]
		public EntityOrientation TailLocation;

		public float WaitTimeBetweenSpawns;

		public float WaitTimeBeforeFirstLaunch;

		public float WaitTimeBetweenLaunchs;
	}

	public class Intro_EnemyAction : EnemyAction
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
			SnakeBehaviour o = owner as SnakeBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			ACT_WAIT.StartAction(o, 0.2f);
			yield return ACT_WAIT.waitForCompletion;
			o.Snake.SnakeAnimatorInyector.BackgroundAnimationSetSpeed(1f, 1f);
			o.Snake.ShadowMaskSprites.ForEach(delegate(SpriteRenderer x)
			{
				x.gameObject.SetActive(value: false);
			});
			Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeThunder(0.3f);
			o.Snake.Audio.PlaySnakeThunder();
			ACT_WAIT.StartAction(o, 0.3f);
			yield return ACT_WAIT.waitForCompletion;
			ACT_WAIT.StartAction(o, 1f);
			yield return ACT_WAIT.waitForCompletion;
			o.Snake.SnakeSegmentsMovementController.MoveToNextStage();
			o.Snake.Audio.PlaySnakePhaseMovement();
			ACT_WAIT.StartAction(o, 0.1f);
			yield return ACT_WAIT.waitForCompletion;
			Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeThunder(0.5f);
			o.Snake.Audio.PlaySnakeThunder();
			ACT_WAIT.StartAction(o, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			ACT_WAIT.StartAction(o, 0.2f);
			yield return ACT_WAIT.waitForCompletion;
			Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeThunder(0.5f);
			o.Snake.Audio.PlaySnakeThunder();
			ACT_WAIT.StartAction(o, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			ACT_WAIT.StartAction(o, 2f);
			yield return ACT_WAIT.waitForCompletion;
			GameObject leftHead = o.Snake.HeadLeft;
			Vector2 outOfCameraPos = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
			Vector2 startPos = new Vector2(o.BattleBounds.xMin + 2f, o.BattleBounds.yMax - 1f);
			leftHead.transform.position = outOfCameraPos;
			o.Snake.Audio.PlaySnakeVanishIn();
			ACT_MOVE.StartAction(o, startPos, 1.5f, Ease.InOutQuad, leftHead.transform);
			ACT_WAIT.StartAction(o, 0.1f);
			yield return ACT_WAIT.waitForCompletion;
			Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeThunder(0.5f);
			o.Snake.Audio.PlaySnakeThunder();
			ACT_WAIT.StartAction(o, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			ACT_WAIT.StartAction(o, 0.2f);
			yield return ACT_WAIT.waitForCompletion;
			Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeThunder(0.5f);
			o.Snake.Audio.PlaySnakeThunder();
			ACT_WAIT.StartAction(o, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			o.Snake.ShadowMaskSprites.ForEach(delegate(SpriteRenderer x)
			{
				x.gameObject.SetActive(value: true);
			});
			ACT_WAIT.StartAction(o, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
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
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			SnakeBehaviour o = owner as SnakeBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			GameObject leftHead = o.Snake.HeadLeft;
			GameObject rightHead = o.Snake.HeadRight;
			SpriteRenderer leftHeadSprite = o.Snake.HeadLeftSprite;
			SpriteRenderer rightHeadSprite = o.Snake.HeadRightSprite;
			p.Status.Invulnerable = true;
			Core.Audio.Ambient.SetSceneParam("BossDeath", 1f);
			o.DoActivateCollisionsIdle(b: false);
			o.DoActivateCollisionsOpenMouth(b: false);
			o.WindAtTheTopActivated = false;
			o.WindAtTheTop.IsDisabled = true;
			Core.Logic.ScreenFreeze.Freeze(0.05f, 0.2f);
			ACT_WAIT.StartAction(o, 0.1f);
			yield return ACT_WAIT.waitForCompletion;
			string blockerName = "TPOReposition";
			Core.Input.SetBlocker(blockerName, blocking: true);
			bool rightHeadShowing = o.Snake.IsRightHeadVisible;
			GameObject headToUse = ((!rightHeadShowing) ? leftHead : rightHead);
			Vector2 snakeTargetPos = ((!rightHeadShowing) ? o.SnakeDeathWaypointLeftHead.position : o.SnakeDeathWaypointRightHead.position);
			Vector2 tpoTargetPos = ((!rightHeadShowing) ? o.TpoWaypointLeftHead.position : o.TpoWaypointRightHead.position);
			EntityOrientation targetOrientation = ((!rightHeadShowing) ? EntityOrientation.Left : EntityOrientation.Right);
			p.DrivePlayer.MoveToPosition(tpoTargetPos, targetOrientation);
			float timeForSnakeMove = Vector2.Distance(snakeTargetPos, headToUse.transform.position) * 0.1f;
			ACT_MOVE.StartAction(o, snakeTargetPos, timeForSnakeMove, Ease.InOutQuad, headToUse.transform);
			o.Snake.ShadowMaskSprites.ForEach(delegate(SpriteRenderer x)
			{
				x.gameObject.SetActive(value: false);
			});
			Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeThunder(0.3f);
			o.Snake.Audio.PlaySnakeThunder();
			ACT_WAIT.StartAction(o, 0.15f);
			yield return ACT_WAIT.waitForCompletion;
			o.ClearAllAttacks();
			ACT_WAIT.StartAction(o, 0.15f);
			yield return ACT_WAIT.waitForCompletion;
			ACT_WAIT.StartAction(o, 0.05f);
			yield return ACT_WAIT.waitForCompletion;
			Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeThunder(0.15f);
			o.Snake.Audio.PlaySnakeThunder();
			ACT_WAIT.StartAction(o, 0.15f);
			yield return ACT_WAIT.waitForCompletion;
			o.Snake.ShadowMaskSprites.ForEach(delegate(SpriteRenderer x)
			{
				x.gameObject.SetActive(value: true);
			});
			yield return ACT_MOVE.waitForCompletion;
			o.Snake.SnakeAnimatorInyector.PlayDeath();
			ACT_WAIT.StartAction(o, 2f);
			yield return ACT_WAIT.waitForCompletion;
			o.Snake.SnakeAnimatorInyector.PlayDeathBite();
			ACT_WAIT.StartAction(o, 0.2f);
			yield return ACT_WAIT.waitForCompletion;
			float moveTime = 1.2f;
			ACT_MOVE.StartAction(o, tpoTargetPos, 1.2f, Ease.InBack, headToUse.transform);
			ACT_WAIT.StartAction(o, moveTime * 0.85f);
			yield return ACT_WAIT.waitForCompletion;
			o.Snake.SnakeAnimatorInyector.StopDeathBite();
			ACT_WAIT.StartAction(o, moveTime * 0.1f);
			yield return ACT_WAIT.waitForCompletion;
			FadeWidget.instance.StartEasyFade(Color.clear, Color.black, 0f, toBlack: true);
			o.Snake.SnakeAnimatorInyector.BackgroundAnimationSetActive(active: false);
			o.Snake.Audio.PlaySnakeDeath();
			ACT_WAIT.StartAction(o, 2f);
			yield return ACT_WAIT.waitForCompletion;
			o.Snake.Audio.PlaySnakeGrunt1();
			ACT_WAIT.StartAction(o, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			o.Snake.Audio.PlaySnakeGrunt2();
			ACT_WAIT.StartAction(o, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			ACT_WAIT.StartAction(o, 2f);
			yield return ACT_WAIT.waitForCompletion;
			o.Snake.HeadLeft.SetActive(value: false);
			o.Snake.HeadRight.SetActive(value: false);
			o.Snake.Tail.SetActive(value: false);
			o.Snake.ChainLeft.SetActive(value: false);
			o.Snake.ChainRight.SetActive(value: false);
			o.Snake.SnakeSegments.ForEach(delegate(SnakeSegmentVisualController x)
			{
				x.gameObject.SetActive(value: false);
			});
			p.Physics.EnablePhysics(enable: false);
			p.Teleport(o.TpoPositionForExecution.position);
			p.SetOrientation(EntityOrientation.Right);
			p.Shadow.ManuallyControllingAlpha = true;
			p.Shadow.SetShadowAlpha(0f);
			o.Snake.SnakeSegmentsMovementController.InstantSetCamAsStart();
			UnityEngine.Object.Instantiate(o.DeadSnakePrefab, o.SnakePositionForExecution.position, Quaternion.identity);
			GameObject executionGO = UnityEngine.Object.Instantiate(o.ExecutionPrefab, o.SnakePositionForExecution.position, Quaternion.identity);
			FadeWidget.instance.StartEasyFade(Color.black, Color.clear, 2f, toBlack: false);
			ACT_WAIT.StartAction(o, 2f);
			yield return ACT_WAIT.waitForCompletion;
			Player player = p.PlatformCharacterInput.Rewired;
			yield return new WaitUntil(() => player.GetButtonDown(8) || Core.GameModeManager.IsCurrentMode(GameModeManager.GAME_MODES.BOSS_RUSH));
			p.Physics.EnablePhysics();
			executionGO.GetComponent<FakeExecution>().UseEvenIfInputBlocked();
			ACT_WAIT.StartAction(owner, 2.5f);
			yield return ACT_WAIT.waitForCompletion;
			Tween t = DOTween.To(() => Core.Logic.Penitent.Shadow.GetShadowAlpha(), delegate(float x)
			{
				Core.Logic.Penitent.Shadow.SetShadowAlpha(x);
			}, 1f, 0.2f);
			t.OnComplete(delegate
			{
				Core.Logic.Penitent.Shadow.ManuallyControllingAlpha = false;
			});
			ACT_WAIT.StartAction(owner, 1f);
			yield return ACT_WAIT.waitForCompletion;
			Core.Input.SetBlocker(blockerName, blocking: false);
			PlayMakerFSM.BroadcastEvent("BOSS DEAD");
			p.Status.Invulnerable = false;
			FinishAction();
			UnityEngine.Object.Destroy(o.gameObject);
		}
	}

	public class ChargedBiteLeftHead_EnemyAction : EnemyAction
	{
		private float waitTime;

		private float moveTime;

		private bool changesHead;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, float waitTime, float moveTime, bool changesHead)
		{
			this.waitTime = waitTime;
			this.moveTime = moveTime;
			this.changesHead = changesHead;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			SnakeBehaviour o = owner as SnakeBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			SnakeAnimatorInyector anim = o.Snake.SnakeAnimatorInyector;
			Vector2 startPos = new Vector2(o.BattleBounds.xMin + 2f, o.BattleBounds.yMax - 1f);
			Vector2 endPos = new Vector2(o.BattleBounds.xMax - 6f, o.BattleBounds.yMin + 0.6f);
			GameObject leftHead = o.Snake.HeadLeft;
			GameObject rightHead = o.Snake.HeadRight;
			SpriteRenderer leftHeadSprite = o.Snake.HeadLeftSprite;
			SpriteRenderer rightHeadSprite = o.Snake.HeadRightSprite;
			if (o.Snake.IsRightHeadVisible)
			{
				Vector2 outOfCameraPos2 = new Vector2(o.BattleBounds.xMax + 8f, o.BattleBounds.yMin + 1f);
				o.Snake.Audio.PlaySnakeVanishOut();
				ACT_MOVE.StartAction(o, outOfCameraPos2, 1.5f, Ease.InOutQuad, rightHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			if (!o.Snake.IsLeftHeadVisible)
			{
				Vector2 outOfCameraPos = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
				leftHead.transform.position = outOfCameraPos;
				o.Snake.Audio.PlaySnakeVanishIn();
				ACT_MOVE.StartAction(o, startPos, 1.5f, Ease.InOutQuad, leftHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			o.Snake.SetOrientation(EntityOrientation.Right, allowFlipRenderer: false);
			anim.StopCloseMouth();
			anim.PlayOpenMouth(SnakeAnimatorInyector.OPEN_MOUTH_INTENTIONS.TO_BITE);
			ACT_WAIT.StartAction(o, waitTime * 0.6f);
			yield return ACT_WAIT.waitForCompletion;
			Vector3 shockwaveStartPoint = leftHead.transform.position + new Vector3(1f, -2.25f, 0f);
			Vector3 shockwaveEndPoint = leftHead.transform.position + new Vector3(2f, -2.25f, 0f);
			GizmoExtensions.DrawDebugCross(shockwaveStartPoint, Color.yellow, 0.9f);
			GizmoExtensions.DrawDebugCross(shockwaveEndPoint, Color.red, 1f);
			o.ShoutShockwave(shockwaveStartPoint, shockwaveEndPoint);
			if (!o.WindAtTheTopActivated)
			{
				o.Snake.Audio.PlaySnakeWind();
				Sequence sequence = DOTween.Sequence();
				sequence.OnStart(delegate
				{
					o.WindToTheRight.IsDisabled = false;
				});
				sequence.AppendInterval(1f);
				sequence.OnComplete(delegate
				{
					o.WindToTheRight.IsDisabled = true;
				});
				sequence.Play();
			}
			ACT_WAIT.StartAction(o, waitTime * 0.4f);
			yield return ACT_WAIT.waitForCompletion;
			anim.BackgroundAnimationSetSpeed(3f);
			ACT_MOVE.StartAction(o, endPos, moveTime, Ease.InBack, leftHead.transform);
			yield return ACT_MOVE.waitForCompletion;
			anim.BackgroundAnimationSetSpeed(1f);
			anim.StopOpenMouth();
			anim.PlayCloseMouth();
			ACT_WAIT.StartAction(o, waitTime);
			yield return ACT_WAIT.waitForCompletion;
			anim.StopCloseMouth();
			anim.BackgroundAnimationSetSpeed(-1f);
			ACT_MOVE.StartAction(o, startPos, moveTime * 4f, Ease.InOutQuad, leftHead.transform);
			yield return ACT_MOVE.waitForCompletion;
			anim.BackgroundAnimationSetSpeed(1f);
			anim.PlayCloseMouth();
			if (changesHead)
			{
				Vector2 leftOutPos = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
				o.Snake.Audio.PlaySnakeVanishOut();
				ACT_MOVE.StartAction(o, leftOutPos, 1.5f, Ease.InOutQuad, leftHead.transform);
				yield return ACT_MOVE.waitForCompletion;
				if (!o.queuedAttacks.Contains(SNAKE_ATTACKS.GO_UP))
				{
					Vector2 rightOutPos = new Vector2(o.BattleBounds.xMax + 8f, o.BattleBounds.yMin + 1f);
					rightHead.transform.position = rightOutPos;
					Vector2 rightInPos = new Vector2(o.BattleBounds.xMax - 2f, o.BattleBounds.yMin + 2f);
					o.Snake.Audio.PlaySnakeVanishIn();
					ACT_MOVE.StartAction(o, rightInPos, 1.5f, Ease.InOutQuad, rightHead.transform);
					yield return ACT_MOVE.waitForCompletion;
				}
			}
			FinishAction();
		}
	}

	public class ChargedBiteRightHead_EnemyAction : EnemyAction
	{
		private float waitTime;

		private float moveTime;

		private bool changesHead;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, float waitTime, float moveTime, bool changesHead)
		{
			this.waitTime = waitTime;
			this.moveTime = moveTime;
			this.changesHead = changesHead;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			SnakeBehaviour o = owner as SnakeBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			SnakeAnimatorInyector anim = o.Snake.SnakeAnimatorInyector;
			Vector2 startPos = new Vector2(o.BattleBounds.xMax - 2f, o.BattleBounds.yMin + 2f);
			Vector2 pointA = new Vector2(o.SnakeRightCorner.transform.position.x, o.SnakeLeftCorner.transform.position.y);
			Vector2 pointB = new Vector2(o.SnakeLeftCorner.transform.position.x, o.SnakeRightCorner.transform.position.y);
			Vector2 dir = pointB - pointA;
			Vector2 endPos = startPos + dir * 0.3f;
			GameObject leftHead = o.Snake.HeadLeft;
			GameObject rightHead = o.Snake.HeadRight;
			SpriteRenderer leftHeadSprite = o.Snake.HeadLeftSprite;
			SpriteRenderer rightHeadSprite = o.Snake.HeadRightSprite;
			if (o.Snake.IsLeftHeadVisible)
			{
				Vector2 outOfCameraPos2 = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
				o.Snake.Audio.PlaySnakeVanishOut();
				ACT_MOVE.StartAction(o, outOfCameraPos2, 1.5f, Ease.InOutQuad, leftHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			if (!o.Snake.IsRightHeadVisible)
			{
				Vector2 outOfCameraPos = new Vector2(o.BattleBounds.xMax + 8f, o.BattleBounds.yMin + 1f);
				rightHead.transform.position = outOfCameraPos;
				o.Snake.Audio.PlaySnakeVanishIn();
				ACT_MOVE.StartAction(o, startPos, 1.5f, Ease.InOutQuad, rightHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			o.Snake.SetOrientation(EntityOrientation.Left, allowFlipRenderer: false);
			anim.StopCloseMouth();
			anim.PlayOpenMouth(SnakeAnimatorInyector.OPEN_MOUTH_INTENTIONS.TO_BITE);
			ACT_WAIT.StartAction(o, waitTime * 0.6f);
			yield return ACT_WAIT.waitForCompletion;
			Vector3 shockwaveStartPoint = rightHead.transform.position + new Vector3(-1f, -2.25f, 0f);
			Vector3 shockwaveEndPoint = rightHead.transform.position + new Vector3(-2f, -2.25f, 0f);
			GizmoExtensions.DrawDebugCross(shockwaveStartPoint, Color.yellow, 0.9f);
			GizmoExtensions.DrawDebugCross(shockwaveEndPoint, Color.red, 1f);
			o.ShoutShockwave(shockwaveStartPoint, shockwaveEndPoint);
			if (!o.WindAtTheTopActivated)
			{
				o.Snake.Audio.PlaySnakeWind();
				Sequence sequence = DOTween.Sequence();
				sequence.OnStart(delegate
				{
					o.WindToTheLeft.IsDisabled = false;
				});
				sequence.AppendInterval(1f);
				sequence.OnComplete(delegate
				{
					o.WindToTheLeft.IsDisabled = true;
				});
				sequence.Play();
			}
			ACT_WAIT.StartAction(o, waitTime * 0.4f);
			yield return ACT_WAIT.waitForCompletion;
			ACT_MOVE.StartAction(o, endPos, moveTime, Ease.InBack, rightHead.transform);
			yield return ACT_MOVE.waitForCompletion;
			anim.StopOpenMouth();
			anim.PlayCloseMouth();
			ACT_WAIT.StartAction(o, waitTime);
			yield return ACT_WAIT.waitForCompletion;
			anim.StopCloseMouth();
			ACT_MOVE.StartAction(o, startPos, moveTime * 4f, Ease.InOutQuad, rightHead.transform);
			yield return ACT_MOVE.waitForCompletion;
			anim.PlayCloseMouth();
			if (changesHead)
			{
				Vector2 rightOutPos = new Vector2(o.BattleBounds.xMax + 8f, o.BattleBounds.yMin + 1f);
				o.Snake.Audio.PlaySnakeVanishOut();
				ACT_MOVE.StartAction(o, rightOutPos, 1.5f, Ease.InOutQuad, rightHead.transform);
				yield return ACT_MOVE.waitForCompletion;
				if (!o.queuedAttacks.Contains(SNAKE_ATTACKS.GO_UP))
				{
					Vector2 leftOutPos = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
					leftHead.transform.position = leftOutPos;
					Vector2 leftInPos = new Vector2(o.BattleBounds.xMin + 2f, o.BattleBounds.yMax - 1f);
					o.Snake.Audio.PlaySnakeVanishIn();
					ACT_MOVE.StartAction(o, leftInPos, 1.5f, Ease.InOutQuad, leftHead.transform);
					yield return ACT_MOVE.waitForCompletion;
				}
			}
			FinishAction();
		}
	}

	public class ScalesSpikesLeftHead_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private TailSpikes_EnemyAction ACT_TAILSPIKES = new TailSpikes_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_TAILSPIKES.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			SnakeBehaviour o = owner as SnakeBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			SnakeAnimatorInyector anim = o.Snake.SnakeAnimatorInyector;
			GameObject leftHead = o.Snake.HeadLeft;
			GameObject rightHead = o.Snake.HeadRight;
			SpriteRenderer leftHeadSprite = o.Snake.HeadLeftSprite;
			SpriteRenderer rightHeadSprite = o.Snake.HeadRightSprite;
			if (o.Snake.IsRightHeadVisible)
			{
				Vector2 outOfCameraPos2 = new Vector2(o.BattleBounds.xMax + 8f, o.BattleBounds.yMin + 1f);
				o.Snake.Audio.PlaySnakeVanishOut();
				ACT_MOVE.StartAction(o, outOfCameraPos2, 1.5f, Ease.InOutQuad, rightHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			Vector2 startPos = new Vector2(o.BattleBounds.xMin + 2f, o.BattleBounds.yMax - 1f);
			if (!o.Snake.IsLeftHeadVisible)
			{
				Vector2 outOfCameraPos = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
				leftHead.transform.position = outOfCameraPos;
				o.Snake.Audio.PlaySnakeVanishIn();
				ACT_MOVE.StartAction(o, startPos, 1.5f, Ease.InOutQuad, leftHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			o.Snake.SetOrientation(EntityOrientation.Right, allowFlipRenderer: false);
			anim.StopCloseMouth();
			anim.PlayOpenMouth(SnakeAnimatorInyector.OPEN_MOUTH_INTENTIONS.TO_SUMMON_SPIKES);
			ACT_WAIT.StartAction(o, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			Vector3 scalesSpikesStartPos = o.SnakeRightCorner.position + Vector3.down * 0.5f;
			Vector3 scalesSpikesDir = (o.SnakeLeftCorner.position - o.SnakeRightCorner.position).normalized;
			o.ScalesSpikesFast.SummonAreas(scalesSpikesStartPos, scalesSpikesDir);
			ACT_WAIT.StartAction(o, 3f);
			yield return ACT_WAIT.waitForCompletion;
			anim.StopOpenMouth();
			anim.PlayCloseMouth();
			FinishAction();
		}
	}

	public class ScalesSpikesRightHead_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private TailSpikes_EnemyAction ACT_TAILSPIKES = new TailSpikes_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_TAILSPIKES.StopAction();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			SnakeBehaviour o = owner as SnakeBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			SnakeAnimatorInyector anim = o.Snake.SnakeAnimatorInyector;
			GameObject leftHead = o.Snake.HeadLeft;
			GameObject rightHead = o.Snake.HeadRight;
			SpriteRenderer leftHeadSprite = o.Snake.HeadLeftSprite;
			SpriteRenderer rightHeadSprite = o.Snake.HeadRightSprite;
			if (o.Snake.IsLeftHeadVisible)
			{
				Vector2 outOfCameraPos2 = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
				o.Snake.Audio.PlaySnakeVanishOut();
				ACT_MOVE.StartAction(o, outOfCameraPos2, 1.5f, Ease.InOutQuad, leftHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			Vector2 startPos = new Vector2(o.BattleBounds.xMax - 2f, o.BattleBounds.yMin + 1f);
			if (!o.Snake.IsRightHeadVisible)
			{
				Vector2 outOfCameraPos = new Vector2(o.BattleBounds.xMax + 8f, o.BattleBounds.yMin + 3f);
				rightHead.transform.position = outOfCameraPos;
				o.Snake.Audio.PlaySnakeVanishIn();
				ACT_MOVE.StartAction(o, startPos, 1.5f, Ease.InOutQuad, rightHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			o.Snake.SetOrientation(EntityOrientation.Left, allowFlipRenderer: false);
			anim.StopCloseMouth();
			anim.PlayOpenMouth(SnakeAnimatorInyector.OPEN_MOUTH_INTENTIONS.TO_SUMMON_SPIKES);
			ACT_WAIT.StartAction(o, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			Vector3 scalesSpikesStartPos = o.SnakeLeftCorner.position + Vector3.down * 0.2f;
			Vector3 scalesSpikesDir = (o.SnakeRightCorner.position - o.SnakeLeftCorner.position).normalized;
			o.ScalesSpikesSlow.SummonAreas(scalesSpikesStartPos, scalesSpikesDir);
			ACT_WAIT.StartAction(o, 3f);
			yield return ACT_WAIT.waitForCompletion;
			anim.StopOpenMouth();
			anim.PlayCloseMouth();
			FinishAction();
		}
	}

	public class ScalesSpikesRightHeadObstacles_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private TailSpikes_EnemyAction ACT_TAILSPIKES = new TailSpikes_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			ACT_TAILSPIKES.StopAction();
			SnakeBehaviour snakeBehaviour = owner as SnakeBehaviour;
			snakeBehaviour.ScalesSpikesObstacles.StopAllCoroutines();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			SnakeBehaviour o = owner as SnakeBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			SnakeAnimatorInyector anim = o.Snake.SnakeAnimatorInyector;
			GameObject leftHead = o.Snake.HeadLeft;
			GameObject rightHead = o.Snake.HeadRight;
			SpriteRenderer leftHeadSprite = o.Snake.HeadLeftSprite;
			SpriteRenderer rightHeadSprite = o.Snake.HeadRightSprite;
			if (o.Snake.IsLeftHeadVisible)
			{
				Vector2 outOfCameraPos2 = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
				o.Snake.Audio.PlaySnakeVanishOut();
				ACT_MOVE.StartAction(o, outOfCameraPos2, 1.5f, Ease.InOutQuad, leftHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			Vector2 startPos = new Vector2(o.BattleBounds.xMax - 2f, o.BattleBounds.yMin + 1f);
			if (!o.Snake.IsRightHeadVisible)
			{
				Vector2 outOfCameraPos = new Vector2(o.BattleBounds.xMax + 8f, o.BattleBounds.yMin + 3f);
				rightHead.transform.position = outOfCameraPos;
				o.Snake.Audio.PlaySnakeVanishIn();
				ACT_MOVE.StartAction(o, startPos, 1.5f, Ease.InOutQuad, rightHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			o.Snake.SetOrientation(EntityOrientation.Left, allowFlipRenderer: false);
			anim.StopCloseMouth();
			anim.PlayOpenMouth(SnakeAnimatorInyector.OPEN_MOUTH_INTENTIONS.TO_SUMMON_SPIKES);
			ACT_WAIT.StartAction(o, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			Vector3 scalesSpikesStartPos = o.SnakeLeftCorner.position + Vector3.down * 0.2f;
			Vector3 scalesSpikesDir = (o.SnakeRightCorner.position - o.SnakeLeftCorner.position).normalized;
			scalesSpikesStartPos += scalesSpikesDir * 2f;
			o.ScalesSpikesObstacles.SummonAreas(scalesSpikesStartPos, scalesSpikesDir);
			ACT_WAIT.StartAction(o, 3f);
			yield return ACT_WAIT.waitForCompletion;
			anim.StopOpenMouth();
			anim.PlayCloseMouth();
			FinishAction();
		}
	}

	public class ChainedLightningLeftHead_EnemyAction : EnemyAction
	{
		private ElmFireTrapManager elmFireLoopManager;

		private float waitTimeToShowEachTrap;

		private float lightningChargeLapse;

		private int numSteps;

		private bool seeking;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			elmFireLoopManager.InstantHideElmFireTraps();
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, ElmFireTrapManager elmFireLoopManager, float waitTimeToShowEachTrap, float lightningChargeLapse, int numSteps, bool seeking = false)
		{
			this.elmFireLoopManager = elmFireLoopManager;
			this.waitTimeToShowEachTrap = waitTimeToShowEachTrap;
			this.lightningChargeLapse = lightningChargeLapse;
			this.numSteps = numSteps;
			this.seeking = seeking;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			SnakeBehaviour o = owner as SnakeBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			SnakeAnimatorInyector anim = o.Snake.SnakeAnimatorInyector;
			GameObject leftHead = o.Snake.HeadLeft;
			GameObject rightHead = o.Snake.HeadRight;
			SpriteRenderer leftHeadSprite = o.Snake.HeadLeftSprite;
			SpriteRenderer rightHeadSprite = o.Snake.HeadRightSprite;
			if (o.Snake.IsRightHeadVisible)
			{
				Vector2 outOfCameraPos = new Vector2(o.BattleBounds.xMax + 8f, o.BattleBounds.yMin + 1f);
				o.Snake.Audio.PlaySnakeVanishOut();
				ACT_MOVE.StartAction(o, outOfCameraPos, 1.5f, Ease.InOutQuad, rightHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			Vector2 startPos = new Vector2(o.BattleBounds.xMin + 2f, o.BattleBounds.yMax - 1f);
			if (!o.Snake.IsLeftHeadVisible)
			{
				Vector2 outOfCameraPos2 = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
				leftHead.transform.position = outOfCameraPos2;
				o.Snake.Audio.PlaySnakeVanishIn();
				ACT_MOVE.StartAction(o, startPos, 1.5f, Ease.InOutQuad, leftHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			o.Snake.SetOrientation(EntityOrientation.Right, allowFlipRenderer: false);
			anim.StopCloseMouth();
			anim.PlayOpenMouth(SnakeAnimatorInyector.OPEN_MOUTH_INTENTIONS.TO_CAST);
			if (seeking)
			{
				Vector3 position = p.GetPosition();
				float t = (position.x - o.SnakeLeftCorner.position.x) / (o.SnakeRightCorner.position.x - o.SnakeLeftCorner.position.x);
				float y = Mathf.Lerp(o.SnakeLeftCorner.position.y, o.SnakeRightCorner.position.y, t);
				elmFireLoopManager.transform.parent.position = new Vector2(position.x, y);
			}
			elmFireLoopManager.ShowElmFireTrapRecursively(elmFireLoopManager.elmFireTrapNodes[0], waitTimeToShowEachTrap, lightningChargeLapse, applyChargingTimeToAll: false);
			yield return new WaitUntil(() => elmFireLoopManager.ElmFireLoopEndReached);
			ACT_WAIT.StartAction(o, waitTimeToShowEachTrap * (float)numSteps);
			yield return ACT_WAIT.waitForCompletion;
			elmFireLoopManager.elmFireTrapNodes[0].SetCurrentCycleCooldownToMax();
			elmFireLoopManager.EnableTraps();
			ACT_WAIT.StartAction(o, lightningChargeLapse * (float)numSteps);
			yield return ACT_WAIT.waitForCompletion;
			anim.StopOpenMouth();
			anim.PlayCloseMouth();
			elmFireLoopManager.DisableTraps();
			elmFireLoopManager.HideElmFireTrapRecursively(elmFireLoopManager.elmFireTrapNodes[0], 0.2f);
			yield return new WaitUntil(() => elmFireLoopManager.ElmFireLoopEndReached);
			FinishAction();
		}
	}

	public class ChainedLightningRightHead_EnemyAction : EnemyAction
	{
		private ElmFireTrapManager elmFireLoopManager;

		private float waitTimeToShowEachTrap;

		private float lightningChargeLapse;

		private int numSteps;

		private bool seeking;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			elmFireLoopManager.InstantHideElmFireTraps();
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, ElmFireTrapManager elmFireLoopManager, float waitTimeToShowEachTrap, float lightningChargeLapse, int numSteps, bool seeking = false)
		{
			this.elmFireLoopManager = elmFireLoopManager;
			this.waitTimeToShowEachTrap = waitTimeToShowEachTrap;
			this.lightningChargeLapse = lightningChargeLapse;
			this.numSteps = numSteps;
			this.seeking = seeking;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			SnakeBehaviour o = owner as SnakeBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			SnakeAnimatorInyector anim = o.Snake.SnakeAnimatorInyector;
			GameObject leftHead = o.Snake.HeadLeft;
			GameObject rightHead = o.Snake.HeadRight;
			SpriteRenderer leftHeadSprite = o.Snake.HeadLeftSprite;
			SpriteRenderer rightHeadSprite = o.Snake.HeadRightSprite;
			if (o.Snake.IsLeftHeadVisible)
			{
				Vector2 outOfCameraPos = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
				o.Snake.Audio.PlaySnakeVanishOut();
				ACT_MOVE.StartAction(o, outOfCameraPos, 1.5f, Ease.InOutQuad, leftHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			Vector2 startPos = new Vector2(o.BattleBounds.xMax - 2f, o.BattleBounds.yMin + 1f);
			if (!o.Snake.IsRightHeadVisible)
			{
				Vector2 outOfCameraPos2 = new Vector2(o.BattleBounds.xMax + 8f, o.BattleBounds.yMin + 3f);
				rightHead.transform.position = outOfCameraPos2;
				o.Snake.Audio.PlaySnakeVanishIn();
				ACT_MOVE.StartAction(o, startPos, 1.5f, Ease.InOutQuad, rightHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			o.Snake.SetOrientation(EntityOrientation.Left, allowFlipRenderer: false);
			anim.StopCloseMouth();
			anim.PlayOpenMouth(SnakeAnimatorInyector.OPEN_MOUTH_INTENTIONS.TO_CAST);
			if (seeking)
			{
				Vector3 position = p.GetPosition();
				float t = (position.x - o.SnakeLeftCorner.position.x) / (o.SnakeRightCorner.position.x - o.SnakeLeftCorner.position.x);
				float y = Mathf.Lerp(o.SnakeLeftCorner.position.y, o.SnakeRightCorner.position.y, t);
				elmFireLoopManager.transform.parent.position = new Vector2(position.x, y);
			}
			elmFireLoopManager.ShowElmFireTrapRecursively(elmFireLoopManager.elmFireTrapNodes[0], waitTimeToShowEachTrap, lightningChargeLapse, applyChargingTimeToAll: false);
			yield return new WaitUntil(() => elmFireLoopManager.ElmFireLoopEndReached);
			ACT_WAIT.StartAction(o, waitTimeToShowEachTrap * (float)numSteps);
			yield return ACT_WAIT.waitForCompletion;
			elmFireLoopManager.elmFireTrapNodes[0].SetCurrentCycleCooldownToMax();
			elmFireLoopManager.EnableTraps();
			ACT_WAIT.StartAction(o, lightningChargeLapse * (float)numSteps);
			yield return ACT_WAIT.waitForCompletion;
			anim.StopOpenMouth();
			anim.PlayCloseMouth();
			elmFireLoopManager.DisableTraps();
			elmFireLoopManager.HideElmFireTrapRecursively(elmFireLoopManager.elmFireTrapNodes[0], 0.2f);
			yield return new WaitUntil(() => elmFireLoopManager.ElmFireLoopEndReached);
			FinishAction();
		}
	}

	public class BigChainedLightningLeftHead_EnemyAction : EnemyAction
	{
		private ElmFireTrapManager elmFireLoopManager1;

		private ElmFireTrapManager elmFireLoopManager2;

		private float waitTimeToShowEachTrap1;

		private float waitTimeToShowEachTrap2;

		private float lightningChargeLapse1;

		private float lightningChargeLapse2;

		private int numSteps1;

		private int numSteps2;

		private float timeBetweenBothLoop;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			elmFireLoopManager1.InstantHideElmFireTraps();
			elmFireLoopManager2.InstantHideElmFireTraps();
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, ElmFireTrapManager elmFireLoopManager1, float waitTimeToShowEachTrap1, float lightningChargeLapse1, int numSteps1, ElmFireTrapManager elmFireLoopManager2, float waitTimeToShowEachTrap2, float lightningChargeLapse2, int numSteps2, float timeBetweenBothLoop)
		{
			this.elmFireLoopManager1 = elmFireLoopManager1;
			this.elmFireLoopManager2 = elmFireLoopManager2;
			this.waitTimeToShowEachTrap1 = waitTimeToShowEachTrap1;
			this.waitTimeToShowEachTrap2 = waitTimeToShowEachTrap2;
			this.lightningChargeLapse1 = lightningChargeLapse1;
			this.lightningChargeLapse2 = lightningChargeLapse2;
			this.numSteps1 = numSteps1;
			this.numSteps2 = numSteps2;
			this.timeBetweenBothLoop = timeBetweenBothLoop;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			SnakeBehaviour o = owner as SnakeBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			SnakeAnimatorInyector anim = o.Snake.SnakeAnimatorInyector;
			GameObject leftHead = o.Snake.HeadLeft;
			GameObject rightHead = o.Snake.HeadRight;
			SpriteRenderer leftHeadSprite = o.Snake.HeadLeftSprite;
			SpriteRenderer rightHeadSprite = o.Snake.HeadRightSprite;
			if (o.Snake.IsRightHeadVisible)
			{
				Vector2 outOfCameraPos2 = new Vector2(o.BattleBounds.xMax + 8f, o.BattleBounds.yMin + 1f);
				o.Snake.Audio.PlaySnakeVanishOut();
				ACT_MOVE.StartAction(o, outOfCameraPos2, 1.5f, Ease.InOutQuad, rightHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			Vector2 startPos = new Vector2(o.BattleBounds.xMin + 2f, o.BattleBounds.yMax - 1f);
			if (!o.Snake.IsLeftHeadVisible)
			{
				Vector2 outOfCameraPos = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
				leftHead.transform.position = outOfCameraPos;
				o.Snake.Audio.PlaySnakeVanishIn();
				ACT_MOVE.StartAction(o, startPos, 1.5f, Ease.InOutQuad, leftHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			o.Snake.SetOrientation(EntityOrientation.Right, allowFlipRenderer: false);
			anim.StopCloseMouth();
			anim.PlayOpenMouth(SnakeAnimatorInyector.OPEN_MOUTH_INTENTIONS.TO_CAST);
			elmFireLoopManager1.ShowElmFireTrapRecursively(elmFireLoopManager1.elmFireTrapNodes[0], waitTimeToShowEachTrap1, lightningChargeLapse1, applyChargingTimeToAll: false);
			yield return new WaitUntil(() => elmFireLoopManager1.ElmFireLoopEndReached);
			ACT_WAIT.StartAction(o, waitTimeToShowEachTrap1 * (float)numSteps1);
			yield return ACT_WAIT.waitForCompletion;
			elmFireLoopManager1.elmFireTrapNodes[0].SetCurrentCycleCooldownToMax();
			elmFireLoopManager1.EnableTraps();
			ACT_WAIT.StartAction(o, lightningChargeLapse1 * (float)numSteps1);
			yield return ACT_WAIT.waitForCompletion;
			elmFireLoopManager1.DisableTraps();
			elmFireLoopManager1.HideElmFireTrapRecursively(elmFireLoopManager1.elmFireTrapNodes[0], 0.2f);
			yield return new WaitUntil(() => elmFireLoopManager1.ElmFireLoopEndReached);
			ACT_WAIT.StartAction(o, timeBetweenBothLoop);
			yield return ACT_WAIT.waitForCompletion;
			elmFireLoopManager2.ShowElmFireTrapRecursively(elmFireLoopManager2.elmFireTrapNodes[0], waitTimeToShowEachTrap2, lightningChargeLapse2, applyChargingTimeToAll: false);
			yield return new WaitUntil(() => elmFireLoopManager2.ElmFireLoopEndReached);
			ACT_WAIT.StartAction(o, waitTimeToShowEachTrap2 * (float)numSteps2);
			yield return ACT_WAIT.waitForCompletion;
			elmFireLoopManager2.elmFireTrapNodes[0].SetCurrentCycleCooldownToMax();
			elmFireLoopManager2.EnableTraps();
			ACT_WAIT.StartAction(o, lightningChargeLapse2 * (float)numSteps2);
			yield return ACT_WAIT.waitForCompletion;
			elmFireLoopManager2.DisableTraps();
			elmFireLoopManager2.HideElmFireTrapRecursively(elmFireLoopManager2.elmFireTrapNodes[0], 0.2f);
			yield return new WaitUntil(() => elmFireLoopManager2.ElmFireLoopEndReached);
			anim.StopOpenMouth();
			anim.PlayCloseMouth();
			FinishAction();
		}
	}

	public class BigChainedLightningRightHead_EnemyAction : EnemyAction
	{
		private ElmFireTrapManager elmFireLoopManager1;

		private ElmFireTrapManager elmFireLoopManager2;

		private float waitTimeToShowEachTrap1;

		private float waitTimeToShowEachTrap2;

		private float lightningChargeLapse1;

		private float lightningChargeLapse2;

		private int numSteps1;

		private int numSteps2;

		private float timeBetweenBothLoop;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			elmFireLoopManager1.InstantHideElmFireTraps();
			elmFireLoopManager2.InstantHideElmFireTraps();
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, ElmFireTrapManager elmFireLoopManager1, float waitTimeToShowEachTrap1, float lightningChargeLapse1, int numSteps1, ElmFireTrapManager elmFireLoopManager2, float waitTimeToShowEachTrap2, float lightningChargeLapse2, int numSteps2, float timeBetweenBothLoop)
		{
			this.elmFireLoopManager1 = elmFireLoopManager1;
			this.elmFireLoopManager2 = elmFireLoopManager2;
			this.waitTimeToShowEachTrap1 = waitTimeToShowEachTrap1;
			this.waitTimeToShowEachTrap2 = waitTimeToShowEachTrap2;
			this.lightningChargeLapse1 = lightningChargeLapse1;
			this.lightningChargeLapse2 = lightningChargeLapse2;
			this.numSteps1 = numSteps1;
			this.numSteps2 = numSteps2;
			this.timeBetweenBothLoop = timeBetweenBothLoop;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			SnakeBehaviour o = owner as SnakeBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			SnakeAnimatorInyector anim = o.Snake.SnakeAnimatorInyector;
			GameObject leftHead = o.Snake.HeadLeft;
			GameObject rightHead = o.Snake.HeadRight;
			SpriteRenderer leftHeadSprite = o.Snake.HeadLeftSprite;
			SpriteRenderer rightHeadSprite = o.Snake.HeadRightSprite;
			if (o.Snake.IsLeftHeadVisible)
			{
				Vector2 outOfCameraPos2 = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
				o.Snake.Audio.PlaySnakeVanishOut();
				ACT_MOVE.StartAction(o, outOfCameraPos2, 1.5f, Ease.InOutQuad, leftHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			Vector2 startPos = new Vector2(o.BattleBounds.xMax - 2f, o.BattleBounds.yMin + 1f);
			if (!o.Snake.IsRightHeadVisible)
			{
				Vector2 outOfCameraPos = new Vector2(o.BattleBounds.xMax + 8f, o.BattleBounds.yMin + 3f);
				rightHead.transform.position = outOfCameraPos;
				o.Snake.Audio.PlaySnakeVanishIn();
				ACT_MOVE.StartAction(o, startPos, 1.5f, Ease.InOutQuad, rightHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			o.Snake.SetOrientation(EntityOrientation.Left, allowFlipRenderer: false);
			anim.StopCloseMouth();
			anim.PlayOpenMouth(SnakeAnimatorInyector.OPEN_MOUTH_INTENTIONS.TO_CAST);
			elmFireLoopManager1.ShowElmFireTrapRecursively(elmFireLoopManager1.elmFireTrapNodes[0], waitTimeToShowEachTrap1, lightningChargeLapse1, applyChargingTimeToAll: false);
			yield return new WaitUntil(() => elmFireLoopManager1.ElmFireLoopEndReached);
			ACT_WAIT.StartAction(o, waitTimeToShowEachTrap1 * (float)numSteps1);
			yield return ACT_WAIT.waitForCompletion;
			elmFireLoopManager1.elmFireTrapNodes[0].SetCurrentCycleCooldownToMax();
			elmFireLoopManager1.EnableTraps();
			ACT_WAIT.StartAction(o, lightningChargeLapse1 * (float)numSteps1);
			yield return ACT_WAIT.waitForCompletion;
			elmFireLoopManager1.DisableTraps();
			elmFireLoopManager1.HideElmFireTrapRecursively(elmFireLoopManager1.elmFireTrapNodes[0], 0.2f);
			yield return new WaitUntil(() => elmFireLoopManager1.ElmFireLoopEndReached);
			ACT_WAIT.StartAction(o, timeBetweenBothLoop);
			yield return ACT_WAIT.waitForCompletion;
			elmFireLoopManager2.ShowElmFireTrapRecursively(elmFireLoopManager2.elmFireTrapNodes[0], waitTimeToShowEachTrap2, lightningChargeLapse2, applyChargingTimeToAll: false);
			yield return new WaitUntil(() => elmFireLoopManager2.ElmFireLoopEndReached);
			ACT_WAIT.StartAction(o, waitTimeToShowEachTrap2 * (float)numSteps2);
			yield return ACT_WAIT.waitForCompletion;
			elmFireLoopManager2.elmFireTrapNodes[0].SetCurrentCycleCooldownToMax();
			elmFireLoopManager2.EnableTraps();
			ACT_WAIT.StartAction(o, lightningChargeLapse2 * (float)numSteps2);
			yield return ACT_WAIT.waitForCompletion;
			elmFireLoopManager2.DisableTraps();
			elmFireLoopManager2.HideElmFireTrapRecursively(elmFireLoopManager2.elmFireTrapNodes[0], 0.2f);
			yield return new WaitUntil(() => elmFireLoopManager2.ElmFireLoopEndReached);
			anim.StopOpenMouth();
			anim.PlayCloseMouth();
			FinishAction();
		}
	}

	public class TailSpikes_EnemyAction : EnemyAction
	{
	}

	public class TailOrbs_EnemyAction : EnemyAction
	{
		private int numOrbs;

		private bool shootFromRightSide;

		private bool careForHeadAnims;

		private List<HomingProjectile> projectiles = new List<HomingProjectile>();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			projectiles.ForEach(delegate(HomingProjectile x)
			{
				x.SetTTL(0f);
			});
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, int numOrbs, bool shootFromRightSide, bool careForHeadAnims)
		{
			this.numOrbs = numOrbs;
			this.shootFromRightSide = shootFromRightSide;
			this.careForHeadAnims = careForHeadAnims;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			SnakeBehaviour o = owner as SnakeBehaviour;
			SnakeAnimatorInyector anim = o.Snake.SnakeAnimatorInyector;
			GameObject leftHead = o.Snake.HeadLeft;
			GameObject rightHead = o.Snake.HeadRight;
			SpriteRenderer leftHeadSprite = o.Snake.HeadLeftSprite;
			SpriteRenderer rightHeadSprite = o.Snake.HeadRightSprite;
			GameObject tail = o.Snake.Tail;
			Animator tailAnimator = o.Snake.TailAnimator;
			if (careForHeadAnims)
			{
				if (!o.Snake.IsRightHeadVisible && !o.Snake.IsLeftHeadVisible)
				{
					Vector2 outOfCameraPos = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
					leftHead.transform.position = outOfCameraPos;
					o.Snake.Audio.PlaySnakeVanishIn();
					Vector2 startPos = new Vector2(o.BattleBounds.xMin + 2f, o.BattleBounds.yMax - 1f);
					ACT_MOVE.StartAction(o, startPos, 1.5f, Ease.InOutQuad, leftHead.transform);
					yield return ACT_MOVE.waitForCompletion;
					shootFromRightSide = false;
				}
				anim.StopCloseMouth();
				anim.PlayOpenMouth(SnakeAnimatorInyector.OPEN_MOUTH_INTENTIONS.TO_CAST);
			}
			string triggerName = ((!shootFromRightSide) ? "RIGHT_TAIL" : "LEFT_TAIL");
			EntityOrientation tailLocation = (shootFromRightSide ? EntityOrientation.Left : EntityOrientation.Right);
			tailAnimator.SetBool(triggerName, value: true);
			o.Snake.Audio.PlaySnakeTail();
			ACT_WAIT.StartAction(o, 0.35f);
			yield return ACT_WAIT.waitForCompletion;
			o.Snake.Audio.PlaySnakeElectricTail();
			ACT_WAIT.StartAction(o, 0.35f);
			yield return ACT_WAIT.waitForCompletion;
			OrbsSettings settings = o.AllOrbsAttacksSettings.Find((OrbsSettings x) => x.NumOrbs == numOrbs && x.TailLocation == tailLocation);
			projectiles.Clear();
			for (int i = 0; i < numOrbs; i++)
			{
				o.HomingProjectileAttack.UseEntityPosition = false;
				o.HomingProjectileAttack.UseEntityOrientation = false;
				o.HomingProjectileAttack.ShootingPoint = o.TailOrbsCenterPoint;
				o.HomingProjectileAttack.OffsetPosition = o.OrbLeftPoint.localPosition;
				HomingProjectile p = o.HomingProjectileAttack.FireProjectileToTarget(o.OrbRightPoint.position);
				p.timeToLive = 30f;
				p.ResetTTL();
				p.ResetSpeed();
				p.ResetRotateSpeed();
				p.ChangesRotatesSpeedInFlight = true;
				p.currentDirection = Vector2.left;
				p.TargetOffset = Vector2.zero;
				p.DestroyedOnReachingTarget = false;
				p.ChangeTargetToAlternative(o.TailOrbsCenterPoint, 0.75f, 1f, 1f);
				p.GetComponentInChildren<GhostTrailGenerator>().EnableGhostTrail = false;
				projectiles.Add(p);
				ACT_WAIT.StartAction(o, settings.WaitTimeBetweenSpawns);
				yield return ACT_WAIT.waitForCompletion;
			}
			ACT_WAIT.StartAction(o, settings.WaitTimeBeforeFirstLaunch);
			yield return ACT_WAIT.waitForCompletion;
			foreach (HomingProjectile p2 in projectiles)
			{
				p2.timeToLive = 3f;
				p2.ResetTTL();
				p2.ChangeTargetToPenitent(changeTargetOnlyIfInactive: false);
				p2.TargetOffset = Vector3.up + UnityEngine.Random.insideUnitSphere * 0.5f;
				p2.GetComponentInChildren<GhostTrailGenerator>().EnableGhostTrail = true;
				p2.OnDisableEvent += DeactivateGhostTrail;
				p2.ChangesRotatesSpeedInFlight = false;
				p2.ResetRotateSpeed();
				ACT_WAIT.StartAction(o, settings.WaitTimeBetweenLaunchs);
				yield return ACT_WAIT.waitForCompletion;
			}
			tailAnimator.SetBool(triggerName, value: false);
			o.Snake.Audio.StopSnakeElectricTail();
			if (careForHeadAnims)
			{
				anim.StopOpenMouth();
				anim.PlayCloseMouth();
			}
			yield return new WaitUntil(() => tailAnimator.GetCurrentAnimatorStateInfo(0).IsName("IDLE"));
			FinishAction();
		}

		private void DeactivateGhostTrail(Projectile obj)
		{
			GhostTrailGenerator componentInChildren = obj.GetComponentInChildren<GhostTrailGenerator>();
			componentInChildren.EnableGhostTrail = false;
		}
	}

	public class TailBeam_EnemyAction : EnemyAction
	{
		private bool shootFromRightSide;

		private bool careForHeadAnims;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_MOVE.StopAction();
			SnakeBehaviour snakeBehaviour = owner as SnakeBehaviour;
			snakeBehaviour.HomingLaserAttack.StopBeam();
			base.DoOnStop();
		}

		public EnemyAction StartAction(EnemyBehaviour e, bool shootFromRightSide, bool careForHeadAnims)
		{
			this.shootFromRightSide = shootFromRightSide;
			this.careForHeadAnims = careForHeadAnims;
			return StartAction(e);
		}

		protected override IEnumerator BaseCoroutine()
		{
			SnakeBehaviour o = owner as SnakeBehaviour;
			SnakeAnimatorInyector anim = o.Snake.SnakeAnimatorInyector;
			GameObject leftHead = o.Snake.HeadLeft;
			GameObject rightHead = o.Snake.HeadRight;
			SpriteRenderer leftHeadSprite = o.Snake.HeadLeftSprite;
			SpriteRenderer rightHeadSprite = o.Snake.HeadRightSprite;
			GameObject tail = o.Snake.Tail;
			Animator tailAnimator = o.Snake.TailAnimator;
			if (careForHeadAnims)
			{
				if (!o.Snake.IsRightHeadVisible && !o.Snake.IsLeftHeadVisible)
				{
					Vector2 outOfCameraPos = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
					leftHead.transform.position = outOfCameraPos;
					o.Snake.Audio.PlaySnakeVanishIn();
					Vector2 startPos = new Vector2(o.BattleBounds.xMin + 2f, o.BattleBounds.yMax - 1f);
					ACT_MOVE.StartAction(o, startPos, 1.5f, Ease.InOutQuad, leftHead.transform);
					yield return ACT_MOVE.waitForCompletion;
					shootFromRightSide = false;
				}
				anim.StopCloseMouth();
				anim.PlayOpenMouth(SnakeAnimatorInyector.OPEN_MOUTH_INTENTIONS.TO_CAST);
			}
			string triggerName = ((!shootFromRightSide) ? "RIGHT_TAIL" : "LEFT_TAIL");
			tailAnimator.SetBool(triggerName, value: true);
			o.Snake.Audio.PlaySnakeTail();
			ACT_WAIT.StartAction(o, 0.35f);
			yield return ACT_WAIT.waitForCompletion;
			o.Snake.Audio.PlaySnakeElectricTail();
			ACT_WAIT.StartAction(o, 0.35f);
			yield return ACT_WAIT.waitForCompletion;
			float warningTime = 1f;
			float beamTime = 3f;
			o.HomingLaserAttack.transform.position = o.TailBeamShootingPoint.position;
			o.HomingLaserAttack.DelayedTargetedBeam(Core.Logic.Penitent.transform, warningTime, beamTime);
			float beamSfxSyncTime = 0.2f;
			ACT_WAIT.StartAction(o, warningTime - beamSfxSyncTime);
			yield return ACT_WAIT.waitForCompletion;
			o.Snake.Audio.PlaySnakeElectricShot();
			ACT_WAIT.StartAction(o, beamSfxSyncTime);
			yield return ACT_WAIT.waitForCompletion;
			Core.Logic.CameraManager.ProCamera2DShake.Shake(beamTime - warningTime, Vector2.down, 50, 0.2f, 0.01f);
			ACT_WAIT.StartAction(o, beamTime - warningTime);
			yield return ACT_WAIT.waitForCompletion;
			tailAnimator.SetBool(triggerName, value: false);
			o.Snake.Audio.StopSnakeElectricTail();
			o.Snake.Audio.StopSnakeElectricShot();
			if (careForHeadAnims)
			{
				anim.StopOpenMouth();
				anim.PlayCloseMouth();
			}
			yield return new WaitUntil(() => tailAnimator.GetCurrentAnimatorStateInfo(0).IsName("IDLE"));
			FinishAction();
		}
	}

	public class TailSweep_EnemyAction : EnemyAction
	{
	}

	public class GoUp_EnemyAction : EnemyAction
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
			SnakeBehaviour o = owner as SnakeBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			SnakeAnimatorInyector anim = o.Snake.SnakeAnimatorInyector;
			GameObject leftHead = o.Snake.HeadLeft;
			GameObject rightHead = o.Snake.HeadRight;
			SpriteRenderer leftHeadSprite = o.Snake.HeadLeftSprite;
			SpriteRenderer rightHeadSprite = o.Snake.HeadRightSprite;
			GameObject tail = o.Snake.Tail;
			o.Snake.ShadowMaskSprites.ForEach(delegate(SpriteRenderer x)
			{
				x.gameObject.SetActive(value: false);
			});
			Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeThunder(0.3f);
			o.Snake.Audio.PlaySnakeThunder();
			ACT_WAIT.StartAction(o, 0.15f);
			yield return ACT_WAIT.waitForCompletion;
			o.ScalesSpikesObstacles.ClearAll();
			ACT_WAIT.StartAction(o, 0.15f);
			yield return ACT_WAIT.waitForCompletion;
			ACT_WAIT.StartAction(o, 0.05f);
			yield return ACT_WAIT.waitForCompletion;
			Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeThunder(0.15f);
			o.Snake.Audio.PlaySnakeThunder();
			ACT_WAIT.StartAction(o, 0.15f);
			yield return ACT_WAIT.waitForCompletion;
			o.Snake.ShadowMaskSprites.ForEach(delegate(SpriteRenderer x)
			{
				x.gameObject.SetActive(value: true);
			});
			if (o.Snake.IsRightHeadVisible)
			{
				Vector2 outOfCameraPos2 = new Vector2(o.BattleBounds.xMax + 8f, o.BattleBounds.yMin + 1f);
				o.Snake.Audio.PlaySnakeVanishOut();
				ACT_MOVE.StartAction(o, outOfCameraPos2, 1.5f, Ease.InOutQuad, rightHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			else if (o.Snake.IsLeftHeadVisible)
			{
				Vector2 outOfCameraPos = new Vector2(o.BattleBounds.xMin - 8f, o.BattleBounds.yMax + 1f);
				o.Snake.Audio.PlaySnakeVanishOut();
				ACT_MOVE.StartAction(o, outOfCameraPos, 1.5f, Ease.InOutQuad, leftHead.transform);
				yield return ACT_MOVE.waitForCompletion;
			}
			o.Snake.Audio.IncreaseSnakeRainState();
			o.Snake.Audio.PlaySnakePhaseMovement();
			tail.transform.position += Vector3.up * 15f;
			SnakeSegmentsMovementController.STAGES curStage = o.Snake.SnakeSegmentsMovementController.CurrentStage;
			o.Snake.SnakeSegmentsMovementController.MoveToNextStage();
			float thunderSeconds = 1f;
			ACT_WAIT.StartAction(o, thunderSeconds);
			yield return ACT_WAIT.waitForCompletion;
			Core.Logic.CameraManager.ScreenEffectsManager.ScreenModeThunder(0.3f);
			o.Snake.Audio.PlaySnakeThunder();
			yield return new WaitUntil(() => curStage != o.Snake.SnakeSegmentsMovementController.CurrentStage);
			if (o.Snake.SnakeSegmentsMovementController.CurrentStage == SnakeSegmentsMovementController.STAGES.STAGE_TWO)
			{
				o.WindAtTheTopActivated = true;
				o.WindAtTheTop.IsDisabled = false;
				o.Snake.Audio.PlaySnakeWind();
			}
			FinishAction();
		}
	}

	[FoldoutGroup("Character settings", 0)]
	public AnimationCurve TimeSlowCurve;

	[FoldoutGroup("Battle area", 0)]
	public Rect BattleBounds;

	[FoldoutGroup("Battle area", 0)]
	public Transform SnakeLeftCorner;

	[FoldoutGroup("Battle area", 0)]
	public Transform SnakeRightCorner;

	[FoldoutGroup("Battle config", 0)]
	public List<SnakeFightParameters> AllFightParameters;

	[FoldoutGroup("Attacks config", 0)]
	public SnakeScriptableConfig AttackConfigData;

	[FoldoutGroup("Debug", 0)]
	public bool DebugDrawCurrentAction;

	[FoldoutGroup("Attack references", 0)]
	public SnakeMeleeAttack OpenedMouthAttackLeft;

	[FoldoutGroup("Attack references", 0)]
	public SnakeMeleeAttack OpenedMouthAttackRight;

	[FoldoutGroup("Attack references", 0)]
	public SnakeMeleeAttack CastingOpenedMouthAttackLeft;

	[FoldoutGroup("Attack references", 0)]
	public SnakeMeleeAttack CastingOpenedMouthAttackRight;

	[FoldoutGroup("Attack references", 0)]
	public SnakeMeleeAttack OpenedToClosedAttackLeft;

	[FoldoutGroup("Attack references", 0)]
	public SnakeMeleeAttack OpenedToClosedAttackRight;

	[FoldoutGroup("Attack references", 0)]
	public SnakeMeleeAttack ChargedBiteAttackLeft;

	[FoldoutGroup("Attack references", 0)]
	public SnakeMeleeAttack ChargedBiteAttackRight;

	[FoldoutGroup("Attack references", 0)]
	public BossAreaSummonAttack ScalesSpikesFast;

	[FoldoutGroup("Attack references", 0)]
	public BossAreaSummonAttack ScalesSpikesSlow;

	[FoldoutGroup("Attack references", 0)]
	public BossAreaSummonAttack ScalesSpikesObstacles;

	[FoldoutGroup("Attack references", 0)]
	public ElmFireTrapManager ElmFireLoopLeftHead1;

	[FoldoutGroup("Attack references", 0)]
	public ElmFireTrapManager ElmFireLoopLeftHead2;

	[FoldoutGroup("Attack references", 0)]
	public ElmFireTrapManager ElmFireLoopLeftHead3;

	[FoldoutGroup("Attack references", 0)]
	public ElmFireTrapManager ElmFireLoopRightHead1;

	[FoldoutGroup("Attack references", 0)]
	public ElmFireTrapManager ElmFireLoopRightHead2;

	[FoldoutGroup("Attack references", 0)]
	public ElmFireTrapManager ElmFireLoopRightHead3;

	[FoldoutGroup("Attack references", 0)]
	public ElmFireTrapManager ElmFireLoopLeftHeadSeeking;

	[FoldoutGroup("Attack references", 0)]
	public ElmFireTrapManager ElmFireLoopRightHeadSeeking;

	[FoldoutGroup("Attack references", 0)]
	public ElmFireTrapManager ElmFireLoopLeftHeadBig1;

	[FoldoutGroup("Attack references", 0)]
	public ElmFireTrapManager ElmFireLoopLeftHeadBig2;

	[FoldoutGroup("Attack references", 0)]
	public ElmFireTrapManager ElmFireLoopRightHeadBig1;

	[FoldoutGroup("Attack references", 0)]
	public ElmFireTrapManager ElmFireLoopRightHeadBig2;

	[FoldoutGroup("Attack references", 0)]
	public BossStraightProjectileAttack TailSpikesAttack;

	[FoldoutGroup("Attack references", 0)]
	public Transform TailBeamShootingPoint;

	[FoldoutGroup("Attack references", 0)]
	public Transform TailOrbsCenterPoint;

	[FoldoutGroup("Attack references", 0)]
	public HomingTurretAttack HomingProjectileAttack;

	[FoldoutGroup("Attack references", 0)]
	public Transform OrbLeftPoint;

	[FoldoutGroup("Attack references", 0)]
	public Transform OrbRightPoint;

	[FoldoutGroup("Attack references", 0)]
	public BossHomingLaserAttack HomingLaserAttack;

	[FoldoutGroup("Attack references", 0)]
	public WindAreaEffect WindToTheRight;

	[FoldoutGroup("Attack references", 0)]
	public WindAreaEffect WindToTheLeft;

	[FoldoutGroup("Attack references", 0)]
	public WindAreaEffect WindAtTheTop;

	[FoldoutGroup("Death references", 0)]
	public Transform TpoWaypointLeftHead;

	[FoldoutGroup("Death references", 0)]
	public Transform TpoWaypointRightHead;

	[FoldoutGroup("Death references", 0)]
	public Transform TpoPositionForExecution;

	[FoldoutGroup("Death references", 0)]
	public Transform SnakePositionForExecution;

	[FoldoutGroup("Death references", 0)]
	public Transform SnakeDeathWaypointLeftHead;

	[FoldoutGroup("Death references", 0)]
	public Transform SnakeDeathWaypointRightHead;

	[FoldoutGroup("Death references", 0)]
	public GameObject ExecutionPrefab;

	[FoldoutGroup("Death references", 0)]
	public GameObject DeadSnakePrefab;

	[FoldoutGroup("Orbs attack additional settings", 0)]
	public List<OrbsSettings> AllOrbsAttacksSettings;

	[HideInInspector]
	public SnakeMeleeAttack CurrentMeleeAttackLeftHead;

	[HideInInspector]
	public SnakeMeleeAttack CurrentMeleeAttackRightHead;

	[HideInInspector]
	public bool WindAtTheTopActivated;

	private List<SNAKE_ATTACKS> availableAttacks = new List<SNAKE_ATTACKS>();

	[ShowInInspector]
	private List<SNAKE_ATTACKS> queuedAttacks = new List<SNAKE_ATTACKS>();

	private SnakeFightParameters currentFightParameters;

	private EnemyAction currentAction;

	private SNAKE_ATTACKS lastAttack = SNAKE_ATTACKS.DUMMY;

	private SNAKE_ATTACKS prevToLastAttack = SNAKE_ATTACKS.DUMMY;

	private Dictionary<SNAKE_ATTACKS, Func<EnemyAction>> actionsDictionary = new Dictionary<SNAKE_ATTACKS, Func<EnemyAction>>();

	private float extraRecoverySeconds;

	private WaitSeconds_EnemyAction waitBetweenActions_EA;

	private Intro_EnemyAction intro_EA;

	private GoUp_EnemyAction goUp_EA;

	private Death_EnemyAction death_EA;

	private ChargedBiteLeftHead_EnemyAction chargedBiteLeftHead_EA;

	private ChargedBiteRightHead_EnemyAction chargedBiteRightHead_EA;

	private ScalesSpikesLeftHead_EnemyAction scalesSpikesLeftHead_EA;

	private ScalesSpikesRightHead_EnemyAction scalesSpikesRightHead_EA;

	private ChainedLightningLeftHead_EnemyAction chainedLightningLeftHead_EA;

	private ChainedLightningRightHead_EnemyAction chainedLightningRightHead_EA;

	private BigChainedLightningLeftHead_EnemyAction bigChainedLightningLeftHead_EA;

	private BigChainedLightningRightHead_EnemyAction bigChainedLightningRightHead_EA;

	private ScalesSpikesRightHeadObstacles_EnemyAction scalesSpikesRightHeadObstacles_EA;

	private TailOrbs_EnemyAction tailOrbs_EA;

	private TailBeam_EnemyAction tailBeam_EA;

	private StateMachine<SnakeBehaviour> _fsm;

	private State<SnakeBehaviour> stIdle;

	private State<SnakeBehaviour> stAction;

	private State<SnakeBehaviour> stDeath;

	public Snake Snake { get; set; }

	private void InitAI()
	{
		stIdle = new SnakeBehaviour_StIdle();
		stAction = new Snake_StAction();
		stDeath = new Snake_StDeath();
		_fsm = new StateMachine<SnakeBehaviour>(this, stIdle);
	}

	private void InitActionDictionary()
	{
		waitBetweenActions_EA = new WaitSeconds_EnemyAction();
		intro_EA = new Intro_EnemyAction();
		goUp_EA = new GoUp_EnemyAction();
		death_EA = new Death_EnemyAction();
		chargedBiteLeftHead_EA = new ChargedBiteLeftHead_EnemyAction();
		chargedBiteRightHead_EA = new ChargedBiteRightHead_EnemyAction();
		scalesSpikesLeftHead_EA = new ScalesSpikesLeftHead_EnemyAction();
		scalesSpikesRightHead_EA = new ScalesSpikesRightHead_EnemyAction();
		chainedLightningLeftHead_EA = new ChainedLightningLeftHead_EnemyAction();
		chainedLightningRightHead_EA = new ChainedLightningRightHead_EnemyAction();
		bigChainedLightningLeftHead_EA = new BigChainedLightningLeftHead_EnemyAction();
		bigChainedLightningRightHead_EA = new BigChainedLightningRightHead_EnemyAction();
		tailOrbs_EA = new TailOrbs_EnemyAction();
		tailBeam_EA = new TailBeam_EnemyAction();
		scalesSpikesRightHeadObstacles_EA = new ScalesSpikesRightHeadObstacles_EnemyAction();
		actionsDictionary.Add(SNAKE_ATTACKS.INTRO, LaunchAction_Intro);
		actionsDictionary.Add(SNAKE_ATTACKS.CHARGED_BITE, LaunchAction_ChargedBite);
		actionsDictionary.Add(SNAKE_ATTACKS.SCALES_SPIKES, LaunchAction_ScalesSpikes);
		actionsDictionary.Add(SNAKE_ATTACKS.CHAINED_LIGHTNING, LaunchAction_ChainedLightning);
		actionsDictionary.Add(SNAKE_ATTACKS.TAIL_ORBS, LaunchAction_TailOrbs);
		actionsDictionary.Add(SNAKE_ATTACKS.TAIL_BEAM, LaunchAction_TailBeam);
		actionsDictionary.Add(SNAKE_ATTACKS.GO_UP, LaunchAction_GoUp);
		actionsDictionary.Add(SNAKE_ATTACKS.DEATH, LaunchAction_Death);
		actionsDictionary.Add(SNAKE_ATTACKS.SEEKING_CHAINED_LIGHTNING, LaunchAction_SeekingChainedLightning);
		actionsDictionary.Add(SNAKE_ATTACKS.BIG_CHAINED_LIGHTNING, LaunchAction_BigChainedLightning);
		actionsDictionary.Add(SNAKE_ATTACKS.SCALES_SPIKES_OBSTACLES, LaunchAction_ScalesSpikesObstacles);
		availableAttacks = AttackConfigData.GetAttackIds(onlyActive: true, useHP: true, 1f);
	}

	private IEnumerator Start()
	{
		Snake = (Snake)Entity;
		InitAI();
		InitActionDictionary();
		currentFightParameters = AllFightParameters[0];
		HideElmFireTraps(ElmFireLoopLeftHead1);
		HideElmFireTraps(ElmFireLoopLeftHead2);
		HideElmFireTraps(ElmFireLoopLeftHead3);
		HideElmFireTraps(ElmFireLoopRightHead1);
		HideElmFireTraps(ElmFireLoopRightHead2);
		HideElmFireTraps(ElmFireLoopRightHead3);
		HideElmFireTraps(ElmFireLoopLeftHeadSeeking);
		HideElmFireTraps(ElmFireLoopRightHeadSeeking);
		HideElmFireTraps(ElmFireLoopLeftHeadBig1);
		HideElmFireTraps(ElmFireLoopLeftHeadBig2);
		HideElmFireTraps(ElmFireLoopRightHeadBig1);
		HideElmFireTraps(ElmFireLoopRightHeadBig2);
		Snake.SnakeAnimatorInyector.PlayCloseMouth();
		Snake.TongueLeftIdleMouth.DamageAreaCollider.enabled = false;
		Snake.TongueLeftOpenMouth.DamageAreaCollider.enabled = false;
		Snake.TongueRightOpenMouth.DamageAreaCollider.enabled = false;
		Snake.TongueRightIdleMouth.DamageAreaCollider.enabled = false;
		Snake.Audio.PlaySnakeRain();
		WindToTheRight.SetMaxForce();
		WindToTheLeft.SetMaxForce();
		LevelManager.OnBeforeLevelLoad += StopRainOnLevelLoad;
		yield return new WaitUntil(() => Core.Logic.Penitent);
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, new Core.SimpleEvent(OnPenitentDead));
	}

	private void OnPenitentDead()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Remove(penitent.OnDead, new Core.SimpleEvent(OnPenitentDead));
		Snake.Audio.StopAll();
		Snake.Audio.StopSnakeRain();
	}

	private void StopRainOnLevelLoad(Level a, Level b)
	{
		LevelManager.OnBeforeLevelLoad -= StopRainOnLevelLoad;
		Snake.Audio.StopAll();
		Snake.Audio.StopSnakeRain();
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Remove(penitent.OnDead, new Core.SimpleEvent(ResetRain));
	}

	private void HideElmFireTraps(ElmFireTrapManager elmFireLoopManager)
	{
		elmFireLoopManager.transform.parent.gameObject.SetActive(value: true);
		elmFireLoopManager.InstantHideElmFireTraps();
	}

	private void Update()
	{
		_fsm.DoUpdate();
	}

	private void OnGUI()
	{
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(BattleBounds.center, new Vector3(BattleBounds.width, BattleBounds.height, 0f));
		Gizmos.DrawWireCube(ArenaGetBotLeftCorner(), Vector3.one * 0.1f);
		Gizmos.DrawWireCube(ArenaGetBotRightCorner(), Vector3.one * 0.1f);
	}

	public void StartIntro()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Combine(penitent.OnDead, new Core.SimpleEvent(ResetRain));
		LaunchAction(SNAKE_ATTACKS.INTRO);
		QueueAttack(SNAKE_ATTACKS.CHARGED_BITE);
	}

	private List<SNAKE_ATTACKS> GetFilteredAttacks(List<SNAKE_ATTACKS> originalList)
	{
		List<SNAKE_ATTACKS> list = new List<SNAKE_ATTACKS>(originalList);
		SnakeScriptableConfig.SnakeAttackConfig atkConfig = AttackConfigData.GetAttackConfig(lastAttack);
		if (atkConfig.cantBeFollowedBy != null && atkConfig.cantBeFollowedBy.Count > 0)
		{
			list.RemoveAll((SNAKE_ATTACKS x) => atkConfig.cantBeFollowedBy.Contains(x));
		}
		if (atkConfig.alwaysFollowedBy != null && atkConfig.alwaysFollowedBy.Count > 0)
		{
			list.RemoveAll((SNAKE_ATTACKS x) => !atkConfig.alwaysFollowedBy.Contains(x));
		}
		if (lastAttack == SNAKE_ATTACKS.BIG_CHAINED_LIGHTNING || lastAttack == SNAKE_ATTACKS.CHAINED_LIGHTNING || lastAttack == SNAKE_ATTACKS.SEEKING_CHAINED_LIGHTNING)
		{
			list.Remove(SNAKE_ATTACKS.BIG_CHAINED_LIGHTNING);
			list.Remove(SNAKE_ATTACKS.CHAINED_LIGHTNING);
			list.Remove(SNAKE_ATTACKS.SEEKING_CHAINED_LIGHTNING);
		}
		if (lastAttack == SNAKE_ATTACKS.GO_UP)
		{
			list.Remove(SNAKE_ATTACKS.SEEKING_CHAINED_LIGHTNING);
			list.Remove(SNAKE_ATTACKS.SCALES_SPIKES_OBSTACLES);
		}
		if (Snake.IsRightHeadVisible)
		{
			list.Remove(SNAKE_ATTACKS.TAIL_BEAM);
		}
		else
		{
			list.Remove(SNAKE_ATTACKS.SCALES_SPIKES_OBSTACLES);
		}
		if (ScalesSpikesObstacles.instantiations.Exists((GameObject x) => x.activeInHierarchy))
		{
			list.Remove(SNAKE_ATTACKS.SCALES_SPIKES_OBSTACLES);
			list.Remove(SNAKE_ATTACKS.CHAINED_LIGHTNING);
			list.Remove(SNAKE_ATTACKS.BIG_CHAINED_LIGHTNING);
			list.Remove(SNAKE_ATTACKS.TAIL_BEAM);
			if (Snake.IsRightHeadVisible)
			{
				list.Remove(SNAKE_ATTACKS.SCALES_SPIKES);
			}
		}
		if (list.Count > 1)
		{
			list.Remove(lastAttack);
		}
		if (list.Count > 2)
		{
			list.Remove(prevToLastAttack);
		}
		return list;
	}

	private int RandomizeUsingWeights(List<SNAKE_ATTACKS> filteredAtks)
	{
		float hpPercentage = Snake.GetHpPercentage();
		List<float> filteredAttacksWeights = AttackConfigData.GetFilteredAttacksWeights(filteredAtks, useHP: true, hpPercentage);
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

	private void QueueAttack(SNAKE_ATTACKS atk)
	{
		queuedAttacks.Add(atk);
	}

	private SNAKE_ATTACKS PopAttackFromQueue()
	{
		SNAKE_ATTACKS result = SNAKE_ATTACKS.CHARGED_BITE;
		int num = queuedAttacks.Count - 1;
		if (num >= 0)
		{
			result = queuedAttacks[num];
			queuedAttacks.RemoveAt(num);
		}
		return result;
	}

	private bool SwapFightParametersIfNeeded()
	{
		bool result = false;
		float hpPercentage = Snake.GetHpPercentage();
		availableAttacks = AttackConfigData.GetAttackIds(onlyActive: true, useHP: true, hpPercentage);
		for (int i = 0; i < AllFightParameters.Count; i++)
		{
			if (AllFightParameters[i].HpPercentageBeforeApply < currentFightParameters.HpPercentageBeforeApply && AllFightParameters[i].HpPercentageBeforeApply > hpPercentage && !currentFightParameters.Equals(AllFightParameters[i]))
			{
				currentFightParameters = AllFightParameters[i];
				result = true;
				break;
			}
		}
		return result;
	}

	private void LaunchAutomaticAction()
	{
		SNAKE_ATTACKS sNAKE_ATTACKS = SNAKE_ATTACKS.DUMMY;
		List<SNAKE_ATTACKS> filteredAttacks = GetFilteredAttacks(availableAttacks);
		if (queuedAttacks.Count > 0)
		{
			sNAKE_ATTACKS = PopAttackFromQueue();
		}
		else
		{
			int index = RandomizeUsingWeights(filteredAttacks);
			sNAKE_ATTACKS = filteredAttacks[index];
		}
		LaunchAction(sNAKE_ATTACKS);
		prevToLastAttack = lastAttack;
		lastAttack = sNAKE_ATTACKS;
	}

	protected void LaunchAction(SNAKE_ATTACKS action)
	{
		StopCurrentAction();
		_fsm.ChangeState(stAction);
		currentAction = actionsDictionary[action]();
		currentAction.OnActionEnds += CurrentAction_OnActionEnds;
		currentAction.OnActionIsStopped += CurrentAction_OnActionStops;
	}

	protected EnemyAction LaunchAction_Death()
	{
		return death_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_Intro()
	{
		return intro_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_ChargedBite()
	{
		float hpPercentage = Snake.GetHpPercentage();
		bool changesHead = hpPercentage < 0.66f;
		float num = 1f + hpPercentage * 0.2f;
		if (Snake.IsRightHeadVisible)
		{
			return chargedBiteRightHead_EA.StartAction(this, 1f * num, 0.7f * num, changesHead);
		}
		return chargedBiteLeftHead_EA.StartAction(this, 1f * num, 1.2f * num, changesHead);
	}

	protected EnemyAction LaunchAction_ScalesSpikes()
	{
		if (Snake.IsLeftHeadVisible)
		{
			return scalesSpikesLeftHead_EA.StartAction(this);
		}
		return scalesSpikesRightHead_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_ChainedLightning()
	{
		ElmFireTrapManager elmFireLoopManager;
		int numSteps;
		if (Snake.IsLeftHeadVisible)
		{
			if (currentFightParameters.Phase == SNAKE_PHASE.FIRST)
			{
				elmFireLoopManager = ElmFireLoopLeftHead1;
				numSteps = 6;
			}
			else if (currentFightParameters.Phase == SNAKE_PHASE.SECOND)
			{
				elmFireLoopManager = ElmFireLoopLeftHead2;
				numSteps = 4;
			}
			else
			{
				elmFireLoopManager = ElmFireLoopLeftHead3;
				numSteps = 5;
			}
			return chainedLightningLeftHead_EA.StartAction(this, elmFireLoopManager, 0.2f, 0.2f, numSteps);
		}
		if (currentFightParameters.Phase == SNAKE_PHASE.FIRST)
		{
			elmFireLoopManager = ElmFireLoopRightHead1;
			numSteps = 6;
		}
		else if (currentFightParameters.Phase == SNAKE_PHASE.SECOND)
		{
			elmFireLoopManager = ElmFireLoopRightHead2;
			numSteps = 4;
		}
		else
		{
			elmFireLoopManager = ElmFireLoopRightHead3;
			numSteps = 5;
		}
		return chainedLightningRightHead_EA.StartAction(this, elmFireLoopManager, 0.2f, 0.2f, numSteps);
	}

	protected EnemyAction LaunchAction_SeekingChainedLightning()
	{
		int numSteps = 4;
		if (Snake.IsLeftHeadVisible)
		{
			ElmFireTrapManager elmFireLoopLeftHeadSeeking = ElmFireLoopLeftHeadSeeking;
			return chainedLightningLeftHead_EA.StartAction(this, elmFireLoopLeftHeadSeeking, 0.1f, 0.2f, numSteps, seeking: true);
		}
		ElmFireTrapManager elmFireLoopRightHeadSeeking = ElmFireLoopRightHeadSeeking;
		return chainedLightningRightHead_EA.StartAction(this, elmFireLoopRightHeadSeeking, 0.1f, 0.2f, numSteps, seeking: true);
	}

	protected EnemyAction LaunchAction_BigChainedLightning()
	{
		int num = 4;
		if (Snake.IsLeftHeadVisible)
		{
			ElmFireTrapManager elmFireLoopLeftHeadBig = ElmFireLoopLeftHeadBig1;
			ElmFireTrapManager elmFireLoopLeftHeadBig2 = ElmFireLoopLeftHeadBig2;
			return bigChainedLightningLeftHead_EA.StartAction(this, elmFireLoopLeftHeadBig, 0.2f, 0.2f, num, elmFireLoopLeftHeadBig2, 0.2f, 0.2f, num, 0.2f);
		}
		ElmFireTrapManager elmFireLoopRightHeadBig = ElmFireLoopRightHeadBig1;
		ElmFireTrapManager elmFireLoopRightHeadBig2 = ElmFireLoopRightHeadBig2;
		return bigChainedLightningRightHead_EA.StartAction(this, elmFireLoopRightHeadBig, 0.2f, 0.2f, num, elmFireLoopRightHeadBig2, 0.2f, 0.2f, num, 0.2f);
	}

	protected EnemyAction LaunchAction_ScalesSpikesObstacles()
	{
		return scalesSpikesRightHeadObstacles_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_TailSpikes()
	{
		throw new NotImplementedException("The Tail Spikes attack has been removed.");
	}

	protected EnemyAction LaunchAction_TailOrbs()
	{
		float hpPercentage = Snake.GetHpPercentage();
		int attackRepetitions = AttackConfigData.GetAttackRepetitions(SNAKE_ATTACKS.TAIL_ORBS, useHP: true, hpPercentage);
		bool isRightHeadVisible = Snake.IsRightHeadVisible;
		return tailOrbs_EA.StartAction(this, attackRepetitions, isRightHeadVisible, careForHeadAnims: true);
	}

	protected EnemyAction LaunchAction_TailSweep()
	{
		throw new NotImplementedException("The Tail Sweep attack has been removed.");
	}

	protected EnemyAction LaunchAction_TailBeam()
	{
		bool isRightHeadVisible = Snake.IsRightHeadVisible;
		return tailBeam_EA.StartAction(this, isRightHeadVisible, careForHeadAnims: true);
	}

	protected EnemyAction LaunchAction_GoUp()
	{
		return goUp_EA.StartAction(this);
	}

	private void CurrentAction_OnActionStops(EnemyAction e)
	{
	}

	private void CurrentAction_OnActionEnds(EnemyAction e)
	{
		e.OnActionEnds -= CurrentAction_OnActionEnds;
		e.OnActionIsStopped -= CurrentAction_OnActionStops;
		if (!_fsm.IsInState(stDeath))
		{
			if (e != waitBetweenActions_EA)
			{
				WaitBetweenActions();
			}
			else
			{
				LaunchAutomaticAction();
			}
		}
	}

	private void WaitBetweenActions()
	{
		_fsm.ChangeState(stIdle);
		StartWait(extraRecoverySeconds + currentFightParameters.MinMaxWaitingTimeBetweenActions.x, extraRecoverySeconds + currentFightParameters.MinMaxWaitingTimeBetweenActions.y);
		extraRecoverySeconds = 0f;
	}

	private void StartWait(float min, float max)
	{
		StopCurrentAction();
		currentAction = waitBetweenActions_EA.StartAction(this, min, max);
		currentAction.OnActionEnds += CurrentAction_OnActionEnds;
	}

	private void CheckDebugActions()
	{
		Dictionary<KeyCode, SNAKE_ATTACKS> debugActions = AttackConfigData.debugActions;
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

	public void SetWeapon(SNAKE_WEAPONS weapon)
	{
		if ((bool)CurrentMeleeAttackLeftHead || (bool)CurrentMeleeAttackRightHead)
		{
			OnMeleeAttackFinished();
		}
		switch (weapon)
		{
		case SNAKE_WEAPONS.CHARGING_OPEN_MOUTH:
			CurrentMeleeAttackLeftHead = OpenedMouthAttackLeft;
			CurrentMeleeAttackRightHead = OpenedMouthAttackRight;
			break;
		case SNAKE_WEAPONS.OPEN_TO_CLOSED:
			CurrentMeleeAttackLeftHead = OpenedToClosedAttackLeft;
			CurrentMeleeAttackRightHead = OpenedToClosedAttackRight;
			break;
		case SNAKE_WEAPONS.CASTING_OPEN_MOUTH:
			CurrentMeleeAttackLeftHead = CastingOpenedMouthAttackLeft;
			CurrentMeleeAttackRightHead = CastingOpenedMouthAttackRight;
			break;
		case SNAKE_WEAPONS.CHARGED_BITE:
			CurrentMeleeAttackLeftHead = ChargedBiteAttackLeft;
			CurrentMeleeAttackRightHead = ChargedBiteAttackRight;
			break;
		}
	}

	public void ResetRain()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Remove(penitent.OnDead, new Core.SimpleEvent(ResetRain));
		Snake.SnakeSegmentsMovementController.ResetRain();
	}

	private Vector2 GetDirToPenitent()
	{
		return (Vector2)Core.Logic.Penitent.transform.position - (Vector2)base.transform.position;
	}

	public Vector2 ArenaGetBotRightCorner()
	{
		return new Vector2(BattleBounds.xMax, BattleBounds.yMin);
	}

	public Vector2 ArenaGetBotLeftCorner()
	{
		return new Vector2(BattleBounds.xMin, BattleBounds.yMin);
	}

	public Vector2 ArenaGetBotFarRandomPoint()
	{
		Vector2 zero = Vector2.zero;
		zero.y = BattleBounds.yMin;
		if (base.transform.position.x > BattleBounds.center.x)
		{
			zero.x = UnityEngine.Random.Range(BattleBounds.xMin, BattleBounds.center.x);
		}
		else
		{
			zero.x = UnityEngine.Random.Range(BattleBounds.center.x, BattleBounds.xMax);
		}
		return zero;
	}

	public Vector2 ArenaGetBotNearRandomPoint()
	{
		Vector2 zero = Vector2.zero;
		zero.y = BattleBounds.yMin;
		if (base.transform.position.x < BattleBounds.center.x)
		{
			zero.x = UnityEngine.Random.Range(BattleBounds.xMin, BattleBounds.center.x);
		}
		else
		{
			zero.x = UnityEngine.Random.Range(BattleBounds.center.x, BattleBounds.xMax);
		}
		return zero;
	}

	public void OnMeleeAttackStarts()
	{
		if ((bool)CurrentMeleeAttackLeftHead)
		{
			CurrentMeleeAttackLeftHead.DealsDamage = true;
			CurrentMeleeAttackLeftHead.CurrentWeaponAttack();
		}
		if ((bool)CurrentMeleeAttackRightHead)
		{
			CurrentMeleeAttackRightHead.DealsDamage = true;
			CurrentMeleeAttackRightHead.CurrentWeaponAttack();
		}
	}

	public void OnMeleeAttackFinished()
	{
		if ((bool)CurrentMeleeAttackLeftHead)
		{
			CurrentMeleeAttackLeftHead.DealsDamage = false;
		}
		if ((bool)CurrentMeleeAttackRightHead)
		{
			CurrentMeleeAttackRightHead.DealsDamage = false;
		}
	}

	public void DoActivateCollisionsOpenMouth(bool b)
	{
		if (b)
		{
			if (Snake.IsRightHeadVisible)
			{
				Snake.TongueRightOpenMouth.DamageAreaCollider.enabled = b;
			}
			else
			{
				Snake.TongueLeftOpenMouth.DamageAreaCollider.enabled = b;
			}
		}
		else
		{
			Snake.TongueLeftOpenMouth.DamageAreaCollider.enabled = b;
			Snake.TongueRightOpenMouth.DamageAreaCollider.enabled = b;
		}
	}

	public void DoActivateCollisionsIdle(bool b)
	{
		if (b)
		{
			if (Snake.IsRightHeadVisible)
			{
				Snake.TongueRightIdleMouth.DamageAreaCollider.enabled = b;
			}
			else
			{
				Snake.TongueLeftIdleMouth.DamageAreaCollider.enabled = b;
			}
		}
		else
		{
			Snake.TongueLeftIdleMouth.DamageAreaCollider.enabled = b;
			Snake.TongueRightIdleMouth.DamageAreaCollider.enabled = b;
		}
	}

	public void Death()
	{
		Snake.Audio.StopAll();
		Snake.Audio.PlaySnakeRain();
		GameplayUtils.DestroyAllProjectiles();
		StopCurrentAction();
		StopAllCoroutines();
		base.transform.DOKill();
		_fsm.ChangeState(stDeath);
		Core.Logic.Penitent.Status.Invulnerable = true;
		LaunchAction(SNAKE_ATTACKS.DEATH);
	}

	private void ClearAllAttacks()
	{
		HomingLaserAttack.Clear();
		HomingLaserAttack.Clear();
		ScalesSpikesSlow.ClearAll();
		ScalesSpikesFast.ClearAll();
		ScalesSpikesObstacles.ClearAll();
	}

	public void ShoutShockwave(Vector3 startPosition, Vector3 endPosition)
	{
		StartCoroutine(ShoutShockwaveCoroutine(startPosition, endPosition));
	}

	private IEnumerator ShoutShockwaveCoroutine(Vector3 startPosition, Vector3 endPosition)
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.45f, Vector3.up * 2.5f, 25, 0.2f, 0.01f, default(Vector3), 0.01f, ignoreTimeScale: true);
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(startPosition, 0.3f, 0.1f, 0.3f);
		yield return new WaitForSeconds(0.2f);
		Core.Logic.ScreenFreeze.Freeze(0.05f, 0.1f);
		Vector3 position = Vector3.Lerp(startPosition, endPosition, 0.5f);
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(position, 0.3f, 0.1f, 1f);
		yield return new WaitForSeconds(0.1f);
		Core.Logic.ScreenFreeze.Freeze(0.05f, 0.1f);
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(endPosition, 0.4f, 0.2f, 2f);
		yield return new WaitForSeconds(0.3f);
	}

	public void Damage(Hit hit)
	{
		if (hit.AttackingEntity.CompareTag("Penitent"))
		{
			Vector3 centerPos = ((!Snake.IsRightHeadVisible) ? Snake.GetLeftDamagedPosition() : Snake.GetRightDamagedPosition());
			PenitentSword penitentSword = (PenitentSword)Core.Logic.Penitent.PenitentAttack.CurrentPenitentWeapon;
			penitentSword.SpawnSparks(centerPos);
			if (!hit.DontSpawnBlood)
			{
				penitentSword.SpawnBlood(centerPos, hit);
			}
		}
		if (SwapFightParametersIfNeeded())
		{
			QeuePhaseSwitchAttacks();
		}
	}

	private void QeuePhaseSwitchAttacks()
	{
		QueueAttack(SNAKE_ATTACKS.GO_UP);
	}

	public override void Attack()
	{
		throw new NotImplementedException();
	}

	public override void Chase(Transform targetPosition)
	{
		throw new NotImplementedException();
	}

	public override void Damage()
	{
		throw new NotImplementedException();
	}

	public override void Idle()
	{
		throw new NotImplementedException();
	}

	public override void StopMovement()
	{
		throw new NotImplementedException();
	}

	public override void Wander()
	{
		throw new NotImplementedException();
	}
}
