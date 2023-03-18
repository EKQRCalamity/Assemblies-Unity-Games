using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Framework.FrameworkCore;
using Framework.Managers;
using Framework.Util;
using Gameplay.GameControllers.Bosses.Isidora.Audio;
using Gameplay.GameControllers.Bosses.Quirce.Attack;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Maikel.StatelessFSM;
using Plugins.Maikel.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Isidora;

public class IsidoraBehaviour : EnemyBehaviour
{
	[Serializable]
	public struct IsidoraFightParameters
	{
		[ProgressBar(0.0, 1.0, 0.8f, 0f, 0.1f)]
		[SuffixLabel("%", false)]
		public float hpPercentageBeforeApply;

		[MinMaxSlider(0f, 5f, true)]
		public Vector2 minMaxWaitingTimeBetweenActions;

		[SuffixLabel("hits", true)]
		public int maxHitsInHurt;

		[InfoBox("If the boss phase should change after reaching this", InfoMessageType.Info, null)]
		public bool advancePhase;

		[InfoBox("If the boss should wait between actions in a vanished state", InfoMessageType.Info, null)]
		public bool waitsInVanish;
	}

	public enum ISIDORA_PHASES
	{
		FIRST,
		BRIDGE,
		SECOND
	}

	public enum ISIDORA_ATTACKS
	{
		AUDIO_TEST = 0,
		ATTACK_TEST = 1,
		FIRST_COMBO = 2,
		SECOND_COMBO = 3,
		PHASE_SWITCH_ATTACK = 4,
		HOMING_PROJECTILES_ATTACK = 5,
		BONFIRE_PROJECTILES_ATTACK = 6,
		BONFIRE_INFINITE_PROJECTILES_ATTACK = 7,
		HORIZONTAL_DASH = 8,
		BLASTS_ATTACK = 9,
		THIRD_COMBO = 10,
		SYNC_COMBO_1 = 11,
		INVISIBLE_HORIZONTAL_DASH = 12,
		CHARGED_BLASTS_ATTACK = 13,
		DUMMY = 999
	}

	public enum ISIDORA_SLASHES
	{
		SLASH,
		RISING_SLASH
	}

	public enum ISIDORA_WEAPONS
	{
		PRE_SLASH = 4,
		SLASH = 0,
		PRE_RISING_SLASH = 1,
		HOLD_RISING_SLASH = 2,
		RISING_SLASH = 3,
		TWIRL = 5,
		FADE_SLASH = 6
	}

	public class Intro_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			(owner as IsidoraBehaviour).Isidora.AnimatorInyector.ResetAll();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			o.Isidora.DamageEffect.StartColorizeLerp(0f, 0.2f, 1f, null);
			ACT_WAIT.StartAction(o, 1f);
			yield return ACT_WAIT.waitForCompletion;
			o.homingBonfireBehavior.IsSpawningIsidora = false;
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
			(owner as IsidoraBehaviour).Isidora.AnimatorInyector.ResetAll();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			p.Status.Invulnerable = true;
			o.homingBonfireBehavior.BonfireAttack.ClearAll();
			o.homingBonfireBehavior.DeactivateBonfire(changeMask: true, explode: true);
			o.blastAttack.ClearAll();
			if (o.blinkCoroutine != null)
			{
				o.StopCoroutine(o.blinkCoroutine);
			}
			o.absorbSpriteRenderer.enabled = false;
			o.Isidora.AnimatorInyector.PlayDeath();
			Vector3 targetPos = o.transform.position;
			targetPos.y = o.battleBounds.yMin + 0.2f;
			float moveTime = Vector2.Distance(targetPos, o.transform.position) * 0.2f + 0.2f;
			ACT_MOVE.StartAction(o, targetPos, moveTime, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			o.homingBonfireBehavior.enabled = false;
			MasterShaderEffects effects = Core.Logic.Penitent.GetComponentInChildren<MasterShaderEffects>();
			if (effects != null)
			{
				effects.StartColorizeLerp(0f, 0.5f, 4f, null);
			}
			o.Isidora.DamageEffect.StartColorizeLerp(0f, 0.2f, 4f, null);
			ACT_WAIT.StartAction(o, 4f);
			yield return ACT_WAIT.waitForCompletion;
			o.Isidora.AnimatorInyector.StopDeath();
			yield return new WaitUntil(() => o.orbSpawned);
			p.Status.Invulnerable = false;
			FinishAction();
			UnityEngine.Object.Destroy(o.gameObject);
		}
	}

	public class AudioSyncTest_EnemyAction : EnemyAction
	{
		protected override void DoOnStop()
		{
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			isidoraBehaviour.Isidora.AnimatorInyector.ResetAll();
			isidoraBehaviour.Isidora.Audio.SetIsidoraVoice(on: false);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour isidora = owner as IsidoraBehaviour;
			yield return new WaitUntilBarFinishes(isidora.Isidora.Audio);
			Debug.Log($"<color=blue>Bar finished: Anticipation starts</color>");
			isidora.Isidora.Audio.SetIsidoraVoice(on: true);
			yield return null;
			isidora.Isidora.Audio.SetIsidoraVoice(on: false);
			isidora.transform.DOMoveX(isidora.transform.position.x + 3f, isidora.Isidora.Audio.GetSingleBarDuration() * 0.5f);
			yield return new WaitUntilBarFinishes(isidora.Isidora.Audio);
			Debug.Log($"<color=blue>Bar finished: Attack starts!</color>");
			isidora.transform.DOMoveX(isidora.transform.position.x - 6f, 0.2f);
			yield return new WaitUntilBarFinishes(isidora.Isidora.Audio);
			FinishAction();
		}
	}

	public class AttackSyncTest_EnemyAction : EnemyAction
	{
		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		protected override void DoOnStop()
		{
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			isidoraBehaviour.Isidora.AnimatorInyector.ResetAll();
			isidoraBehaviour.Isidora.Audio.SetIsidoraVoice(on: false);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			float dashSlashTime = 0.99f;
			IsidoraBehaviour isidora = owner as IsidoraBehaviour;
			IsidoraAnimatorInyector animatorInyector = isidora.Isidora.AnimatorInyector;
			isidora.LookAtDirUsingOrientation(Vector2.left);
			Tweener tween = ShortcutExtensions.DOMove(endValue: isidora.ArenaGetBotRightCorner(), target: isidora.transform, duration: isidora.Isidora.Audio.GetTimeUntilNextAttackAnticipationPeriod() - 0.1f);
			yield return tween.WaitForCompletion();
			isidora.Isidora.Audio.SetIsidoraVoice(on: true);
			yield return new WaitUntilBarFinishes(isidora.Isidora.Audio);
			ACT_WAIT.StartAction(isidora, 0.01f);
			yield return ACT_WAIT.waitForCompletion;
			isidora.Isidora.Audio.SetIsidoraVoice(on: false);
			Debug.Log($"<color=blue>Bar finished: Anticipation starts!</color>");
			animatorInyector.PlaySlashAttack();
			animatorInyector.SetAttackAnticipation(hold: true);
			float remainingTime = isidora.Isidora.Audio.GetTimeLeftForCurrentBar();
			ACT_WAIT.StartAction(isidora, remainingTime * 0.55f);
			yield return ACT_WAIT.waitForCompletion;
			isidora.transform.DOMoveX(isidora.transform.position.x + 1f, remainingTime * 0.3f).SetEase(Ease.InOutCubic);
			yield return new WaitUntilBarFinishes(isidora.Isidora.Audio);
			Debug.Log($"<color=blue>Bar finished: Attack 1 starts!</color>");
			animatorInyector.SetTwirl(twirl: true);
			animatorInyector.SetAttackAnticipation(hold: false);
			Tweener t = ShortcutExtensions.DOMove(endValue: isidora.ArenaGetBotLeftCorner(), target: isidora.transform, duration: dashSlashTime).SetEase(Ease.InOutCubic);
			yield return t.WaitForCompletion();
			ACT_WAIT.StartAction(isidora, 0.33f);
			yield return ACT_WAIT.waitForCompletion;
			animatorInyector.SetTwirl(twirl: false);
			yield return new WaitUntilBarFinishes(isidora.Isidora.Audio);
			animatorInyector.PlaySlashAttack();
			animatorInyector.SetAttackAnticipation(hold: true);
			isidora.LookAtDirUsingOrientation(Vector2.right);
			Debug.Log($"<color=blue>Bar finished: Dance starts!</color>");
			remainingTime = isidora.Isidora.Audio.GetTimeLeftForCurrentBar();
			Sequence s = DOTween.Sequence();
			s.Append(isidora.transform.DOMoveY(isidora.transform.position.y + 1f, remainingTime * 0.5f).SetEase(Ease.InOutCubic));
			s.Append(isidora.transform.DOMoveX(isidora.transform.position.x - 1f, remainingTime * 0.33f).SetEase(Ease.InOutCubic));
			yield return new WaitUntilBarFinishes(isidora.Isidora.Audio);
			animatorInyector.SetAttackAnticipation(hold: false);
			Debug.Log($"<color=blue>Bar finished: Attack 2 starts!</color>");
			ShortcutExtensions.DOMove(endValue: isidora.ArenaGetBotRightCorner() + Vector2.up, target: isidora.transform, duration: dashSlashTime);
			isidora.Isidora.Audio.SetIsidoraVoice(on: false);
			yield return new WaitUntilBarFinishes(isidora.Isidora.Audio);
			FinishAction();
		}
	}

	public class WaitUntilIdle : WaitUntilAnimationState
	{
		public WaitUntilIdle(IsidoraBehaviour isidora)
			: base(isidora, "IDLE")
		{
		}
	}

	public class WaitUntilOut : WaitUntilAnimationState
	{
		public WaitUntilOut(IsidoraBehaviour isidora)
			: base(isidora, "OUT")
		{
		}
	}

	public class WaitUntilTwirl : WaitUntilAnimationState
	{
		public WaitUntilTwirl(IsidoraBehaviour isidora)
			: base(isidora, "TWIRL")
		{
		}
	}

	public class WaitUntilCasting : WaitUntilAnimationState
	{
		public WaitUntilCasting(IsidoraBehaviour isidora)
			: base(isidora, "CASTING")
		{
		}
	}

	public class WaitUntilAnimationState : CustomYieldInstruction
	{
		private IsidoraBehaviour isidora;

		private string state;

		public override bool keepWaiting => !isidora.IsAnimatorInState(state);

		public WaitUntilAnimationState(IsidoraBehaviour isidora, string state)
		{
			this.isidora = isidora;
			this.state = state;
		}
	}

	public class MeleeAttack_EnemyAction : EnemyAction
	{
		private Vector2 attackDir;

		private ISIDORA_SLASHES slashType;

		private float slashTime;

		private bool hold;

		private float holdTime;

		private bool twirl;

		private float twirlTime;

		private bool endsWithTwirlActive;

		private Core.SimpleEvent onAttackGuardedCallback;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 attackDir, ISIDORA_SLASHES slashType, float slashTime, bool hold, float holdTime, bool twirl, float twirlTime, bool endsWithTwirlActive, Core.SimpleEvent onAttackGuardedCallback = null)
		{
			this.attackDir = attackDir;
			this.slashType = slashType;
			this.slashTime = slashTime;
			this.hold = hold;
			this.holdTime = holdTime;
			this.twirl = twirl;
			this.twirlTime = twirlTime;
			this.endsWithTwirlActive = endsWithTwirlActive;
			this.onAttackGuardedCallback = onAttackGuardedCallback;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			isidoraBehaviour.Isidora.AnimatorInyector.ResetAll();
			if (onAttackGuardedCallback != null)
			{
				IsidoraMeleeAttack preSlashAttack = isidoraBehaviour.preSlashAttack;
				preSlashAttack.OnAttackGuarded = (Core.SimpleEvent)Delegate.Remove(preSlashAttack.OnAttackGuarded, onAttackGuardedCallback);
				IsidoraMeleeAttack slashAttack = isidoraBehaviour.slashAttack;
				slashAttack.OnAttackGuarded = (Core.SimpleEvent)Delegate.Remove(slashAttack.OnAttackGuarded, onAttackGuardedCallback);
				IsidoraMeleeAttack risingAttack = isidoraBehaviour.risingAttack;
				risingAttack.OnAttackGuarded = (Core.SimpleEvent)Delegate.Remove(risingAttack.OnAttackGuarded, onAttackGuardedCallback);
			}
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			o.LookAtDirUsingOrientation(attackDir);
			o.Isidora.AnimatorInyector.SetAttackAnticipation(hold);
			o.Isidora.AnimatorInyector.SetHidden(hidden: false);
			switch (slashType)
			{
			case ISIDORA_SLASHES.SLASH:
				o.Isidora.AnimatorInyector.PlaySlashAttack();
				if (onAttackGuardedCallback != null)
				{
					IsidoraMeleeAttack preSlashAttack = o.preSlashAttack;
					preSlashAttack.OnAttackGuarded = (Core.SimpleEvent)Delegate.Combine(preSlashAttack.OnAttackGuarded, onAttackGuardedCallback);
					IsidoraMeleeAttack slashAttack = o.slashAttack;
					slashAttack.OnAttackGuarded = (Core.SimpleEvent)Delegate.Combine(slashAttack.OnAttackGuarded, onAttackGuardedCallback);
				}
				break;
			case ISIDORA_SLASHES.RISING_SLASH:
				o.Isidora.AnimatorInyector.PlayRisingSlash();
				if (onAttackGuardedCallback != null)
				{
					IsidoraMeleeAttack risingAttack = o.risingAttack;
					risingAttack.OnAttackGuarded = (Core.SimpleEvent)Delegate.Combine(risingAttack.OnAttackGuarded, onAttackGuardedCallback);
				}
				break;
			}
			if (hold)
			{
				ACT_WAIT.StartAction(o, holdTime);
				yield return ACT_WAIT.waitForCompletion;
			}
			o.Isidora.AnimatorInyector.SetTwirl(twirl);
			o.Isidora.AnimatorInyector.SetAttackAnticipation(hold: false);
			ACT_WAIT.StartAction(o, slashTime);
			yield return ACT_WAIT.waitForCompletion;
			if (twirl)
			{
				ACT_WAIT.StartAction(o, twirlTime);
				yield return ACT_WAIT.waitForCompletion;
			}
			if (!endsWithTwirlActive)
			{
				o.Isidora.AnimatorInyector.SetTwirl(twirl: false);
				o.Isidora.AnimatorInyector.SetHidden(hidden: false);
				yield return new WaitUntilIdle(o);
			}
			if (onAttackGuardedCallback != null)
			{
				IsidoraMeleeAttack preSlashAttack2 = o.preSlashAttack;
				preSlashAttack2.OnAttackGuarded = (Core.SimpleEvent)Delegate.Remove(preSlashAttack2.OnAttackGuarded, onAttackGuardedCallback);
				IsidoraMeleeAttack slashAttack2 = o.slashAttack;
				slashAttack2.OnAttackGuarded = (Core.SimpleEvent)Delegate.Remove(slashAttack2.OnAttackGuarded, onAttackGuardedCallback);
				IsidoraMeleeAttack risingAttack2 = o.risingAttack;
				risingAttack2.OnAttackGuarded = (Core.SimpleEvent)Delegate.Remove(risingAttack2.OnAttackGuarded, onAttackGuardedCallback);
			}
			FinishAction();
		}
	}

	public class FirstCombo_EnemyAction : EnemyAction
	{
		private float distanceFromPenitent;

		private float slashDistance;

		private float slashTime;

		private float holdTime1;

		private float holdTime2;

		private float twirlTime1;

		private float twirlDistance1;

		private float twirlTime2;

		private float twirlDistance2;

		private int repetitions;

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MeleeAttack_EnemyAction ACT_MELEE = new MeleeAttack_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, float distanceFromPenitent, float slashDistance, float slashTime, float holdTime1, float holdTime2, float twirlTime1, float twirlDistance1, float twirlTime2, float twirlDistance2, int repetitions = 1)
		{
			this.distanceFromPenitent = distanceFromPenitent;
			this.slashDistance = slashDistance;
			this.slashTime = slashTime;
			this.holdTime1 = holdTime1;
			this.holdTime2 = holdTime2;
			this.twirlTime1 = twirlTime1;
			this.twirlDistance1 = twirlDistance1;
			this.twirlTime2 = twirlTime2;
			this.twirlDistance2 = twirlDistance2;
			this.repetitions = repetitions;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_MOVE.StopAction();
			ACT_WAIT.StopAction();
			ACT_MELEE.StopAction();
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			isidoraBehaviour.Isidora.AnimatorInyector.ResetAll();
			isidoraBehaviour.Isidora.Audio.SetSkullsChoir(on: false);
			isidoraBehaviour.transform.DOKill();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			float vOffset = 0.3f;
			if (o.IsAnimatorInState("OUT"))
			{
				o.transform.position = o.ArenaGetBotFarRandomPoint();
				o.LookAtTarget();
				o.Isidora.AnimatorInyector.SetHidden(hidden: false);
				yield return new WaitUntilIdle(o);
			}
			o.Isidora.Audio.SetSkullsChoir(on: true);
			Vector2 dir2 = o.GetDirToPenitent();
			Vector2 startingPosition = o.transform.position;
			if (Mathf.Abs(dir2.x) > distanceFromPenitent)
			{
				if (dir2.x > 0f)
				{
					startingPosition.x = p.GetPosition().x - distanceFromPenitent;
				}
				else
				{
					startingPosition.x = p.GetPosition().x + distanceFromPenitent;
				}
			}
			startingPosition.y = o.battleBounds.yMin - 0.5f;
			float approachTime = ((Vector2)o.transform.position - startingPosition).magnitude * 0.1f + 0.2f;
			ACT_MOVE.StartAction(o, startingPosition, approachTime, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			ACT_MELEE.StartAction(o, dir2, ISIDORA_SLASHES.SLASH, slashTime, hold: true, holdTime1, twirl: true, twirlTime1, endsWithTwirlActive: true, delegate
			{
				ACT_MOVE.StopAction();
			});
			dir2 = o.GetDirToPenitent();
			float anticipationDistance = 1.5f;
			Vector2 anticipationPoint2 = (Vector2)o.transform.position + Vector2.left * Mathf.Sign(dir2.x) * anticipationDistance;
			anticipationPoint2 = o.ClampInsideBoundaries(anticipationPoint2);
			ACT_MOVE.StartAction(o, anticipationPoint2, holdTime1, Ease.OutQuad);
			yield return ACT_MOVE.waitForCompletion;
			Vector2 afterSlashPosition2 = startingPosition + Vector2.right * slashDistance * Mathf.Sign(dir2.x) + Vector2.down * vOffset;
			afterSlashPosition2 = o.ClampInsideBoundaries(afterSlashPosition2);
			ACT_MOVE.StartAction(o, afterSlashPosition2, slashTime, Ease.OutQuad);
			yield return ACT_MOVE.waitForCompletion;
			Vector2 newDir = o.GetDirToPenitent();
			Vector2 twirlTarget2 = (Vector2)o.transform.position + Vector2.left * Mathf.Sign(newDir.x) * twirlDistance2 + Vector2.up * vOffset;
			twirlTarget2 = o.ClampInsideBoundaries(twirlTarget2);
			ACT_MOVE.StartAction(o, twirlTarget2, twirlTime1, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			yield return ACT_MELEE.waitForCompletion;
			int j = repetitions;
			for (int i = 0; i < j; i++)
			{
				newDir = o.GetDirToPenitent();
				float remainingTime = o.Isidora.Audio.GetTimeLeftForCurrentBar();
				if (remainingTime < holdTime2)
				{
					float singleBarDuration = o.Isidora.Audio.GetSingleBarDuration();
					remainingTime += singleBarDuration;
					Debug.Log("<color=magenta> ENTRANDO EN EL TWIRL DANCE </color>");
					bool isFirstLoop = true;
					Vector2 point = (Vector2)o.transform.position + Vector2.left * Mathf.Sign(newDir.x) * twirlDistance1 * 0.8f;
					point = o.ClampInsideBoundaries(point);
					o.transform.DOMove(point, singleBarDuration * 0.5f).SetEase(Ease.InOutQuad).SetLoops(2, LoopType.Yoyo)
						.OnStepComplete(delegate
						{
							if (isFirstLoop)
							{
								isFirstLoop = false;
								newDir = o.GetDirToPenitent();
								o.LookAtTarget();
							}
						});
				}
				else
				{
					Vector2 point2 = (Vector2)o.transform.position + Vector2.left * Mathf.Sign(newDir.x) * twirlDistance1;
					point2 = o.ClampInsideBoundaries(point2);
					o.transform.DOMove(point2, remainingTime - holdTime2).SetEase(Ease.OutQuad);
				}
				ACT_WAIT.StartAction(o, remainingTime - holdTime2);
				yield return ACT_WAIT.waitForCompletion;
				bool keepTwirl = i < j - 1;
				o.ghostTrail.EnableGhostTrail = true;
				ACT_MELEE.StartAction(o, newDir, ISIDORA_SLASHES.SLASH, slashTime, hold: true, holdTime2, twirl: true, twirlTime2, keepTwirl, delegate
				{
					ACT_MOVE.StopAction();
				});
				ACT_MOVE.StartAction(o, (Vector2)o.transform.position + Vector2.left * Mathf.Sign(newDir.x) * anticipationDistance, holdTime2, Ease.OutQuad);
				yield return ACT_MOVE.waitForCompletion;
				float nSlashDistance = o.GetDirToPenitent().magnitude + 1f;
				Vector2 afterSlashPosition4 = (Vector2)o.transform.position + Vector2.right * Mathf.Sign(newDir.x) * nSlashDistance + Vector2.down * vOffset;
				afterSlashPosition4 = o.ClampInsideBoundaries(afterSlashPosition4);
				ACT_MOVE.StartAction(o, afterSlashPosition4, slashTime, Ease.OutQuad);
				yield return ACT_MOVE.waitForCompletion;
				o.ghostTrail.EnableGhostTrail = false;
				if (!keepTwirl)
				{
					twirlDistance2 += 2f;
					twirlTime2 += 1f;
				}
				Vector2 twirlTarget4 = (Vector2)o.transform.position + Vector2.left * Mathf.Sign(newDir.x) * twirlDistance2 + Vector2.up * vOffset;
				twirlTarget4 = o.ClampInsideBoundaries(twirlTarget4);
				o.Isidora.AnimatorInyector.Decelerate(twirlTime2 * 0.5f);
				ACT_MOVE.StartAction(o, twirlTarget4, twirlTime2, Ease.InOutQuad);
				yield return ACT_MOVE.waitForCompletion;
				yield return ACT_MELEE.waitForCompletion;
			}
			o.Isidora.Audio.SetSkullsChoir(on: false);
			FinishAction();
		}
	}

	public class SecondCombo_EnemyAction : EnemyAction
	{
		private float distanceFromPenitent;

		private float slashDistance;

		private float slashTime;

		private float risingSlashTime;

		private float holdTime1;

		private float holdTime2;

		private float twirlTime1;

		private float twirlDistance1;

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE2 = new MoveEasing_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MeleeAttack_EnemyAction ACT_MELEE = new MeleeAttack_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, float distanceFromPenitent, float slashDistance, float slashTime, float risingSlashTime, float holdTime1, float holdTime2, float twirlTime1, float twirlDistance1)
		{
			this.distanceFromPenitent = distanceFromPenitent;
			this.slashDistance = slashDistance;
			this.slashTime = slashTime;
			this.risingSlashTime = risingSlashTime;
			this.holdTime1 = holdTime1;
			this.holdTime2 = holdTime2;
			this.twirlTime1 = twirlTime1;
			this.twirlDistance1 = twirlDistance1;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_MOVE.StopAction();
			ACT_MOVE2.StopAction();
			ACT_WAIT.StopAction();
			ACT_MELEE.StopAction();
			(owner as IsidoraBehaviour).Isidora.AnimatorInyector.ResetAll();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			float vOffset = 0.3f;
			if (o.IsAnimatorInState("OUT"))
			{
				o.transform.position = o.ArenaGetBotFarRandomPoint();
				o.LookAtTarget();
				o.Isidora.AnimatorInyector.SetHidden(hidden: false);
				yield return new WaitUntilIdle(o);
			}
			o.Isidora.Audio.SetSkullsChoir(on: true);
			Vector2 dir = o.GetDirToPenitent();
			Vector2 startingPosition = o.transform.position;
			if (Mathf.Abs(dir.x) > distanceFromPenitent)
			{
				startingPosition.x = p.GetPosition().x - Mathf.Sign(dir.x) * distanceFromPenitent;
			}
			startingPosition.y = o.battleBounds.yMin - 0.5f;
			float approachTime = ((Vector2)o.transform.position - startingPosition).magnitude * 0.1f + 0.2f;
			ACT_MOVE.StartAction(o, startingPosition, approachTime, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			ACT_MELEE.StartAction(o, dir, ISIDORA_SLASHES.SLASH, slashTime, hold: true, holdTime1, twirl: true, twirlTime1, endsWithTwirlActive: true, delegate
			{
				ACT_MOVE.StopAction();
			});
			dir = o.GetDirToPenitent();
			float anticipationDistance = 1.5f;
			Vector2 anticipationPoint2 = (Vector2)o.transform.position + Vector2.left * Mathf.Sign(dir.x) * anticipationDistance;
			anticipationPoint2 = o.ClampInsideBoundaries(anticipationPoint2);
			ACT_MOVE.StartAction(o, anticipationPoint2, holdTime1, Ease.OutQuad);
			yield return ACT_MOVE.waitForCompletion;
			Vector2 afterSlashPosition2 = startingPosition + Vector2.right * slashDistance * Mathf.Sign(dir.x) + Vector2.down * vOffset;
			afterSlashPosition2 = o.ClampInsideBoundaries(afterSlashPosition2);
			ACT_MOVE.StartAction(o, afterSlashPosition2, slashTime, Ease.OutQuad);
			yield return ACT_MOVE.waitForCompletion;
			Vector2 newDir = o.GetDirToPenitent();
			Vector2 twirlTarget2 = (Vector2)o.transform.position + Vector2.left * Mathf.Sign(newDir.x) * twirlDistance1 + Vector2.up * vOffset;
			twirlTarget2 = o.ClampInsideBoundaries(twirlTarget2);
			ACT_MOVE.StartAction(o, twirlTarget2, twirlTime1, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			yield return ACT_MELEE.waitForCompletion;
			o.ghostTrail.EnableGhostTrail = true;
			ACT_MELEE.StartAction(o, newDir, ISIDORA_SLASHES.RISING_SLASH, risingSlashTime, hold: true, holdTime2, twirl: true, 1.5f, endsWithTwirlActive: true);
			ACT_WAIT.StartAction(o, holdTime2 * 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			Vector2 afterSlashPosition4 = twirlTarget2;
			if (newDir.x > 0f)
			{
				afterSlashPosition4.x += slashDistance;
			}
			else
			{
				afterSlashPosition4.x -= slashDistance;
			}
			float risingSlashHeight = 4f;
			afterSlashPosition4.y += risingSlashHeight;
			afterSlashPosition4 = o.ClampInsideBoundaries(afterSlashPosition4);
			ACT_MOVE.StartAction(o, afterSlashPosition4, risingSlashTime + holdTime2 * 0.5f, Ease.InOutQuad, null, _timeScaled: true, null, _tweenOnX: true, _tweenOnY: false);
			ACT_WAIT.StartAction(o, holdTime2 * 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			ACT_MOVE2.StartAction(o, afterSlashPosition4, risingSlashTime, Ease.OutQuad, null, _timeScaled: true, null, _tweenOnX: false);
			yield return ACT_MOVE2.waitForCompletion;
			o.ghostTrail.EnableGhostTrail = false;
			o.Isidora.AnimatorInyector.Decelerate(risingSlashTime * 2.5f);
			afterSlashPosition4.y += 0f - risingSlashHeight + 0.5f;
			ACT_MOVE2.StartAction(o, afterSlashPosition4, risingSlashTime * 5f, Ease.InOutQuad);
			yield return ACT_MOVE2.waitForCompletion;
			o.Isidora.AnimatorInyector.SetTwirl(twirl: false);
			ACT_WAIT.StartAction(o, 0.5f);
			yield return ACT_WAIT.waitForCompletion;
			yield return ACT_MELEE.waitForCompletion;
			FinishAction();
		}
	}

	public class ThirdCombo_EnemyAction : EnemyAction
	{
		private float distanceFromPenitent;

		private float slashDistance;

		private float slashTime;

		private float holdTime1;

		private float holdTime2;

		private float vanishTime;

		private float vanishDistance;

		private float twirlTime;

		private float twirlDistance;

		private int repetitions;

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MeleeAttack_EnemyAction ACT_MELEE = new MeleeAttack_EnemyAction();

		private Teleport_EnemyAction ACT_TELEPORT = new Teleport_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, float distanceFromPenitent, float slashDistance, float slashTime, float holdTime1, float holdTime2, float vanishTime, float vanishDistance, float twirlTime, float twirlDistance, int repetitions = 1)
		{
			this.distanceFromPenitent = distanceFromPenitent;
			this.slashDistance = slashDistance;
			this.slashTime = slashTime;
			this.holdTime1 = holdTime1;
			this.holdTime2 = holdTime2;
			this.vanishTime = vanishTime;
			this.vanishDistance = vanishDistance;
			this.twirlTime = twirlTime;
			this.twirlDistance = twirlDistance;
			this.repetitions = repetitions;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_MOVE.StopAction();
			ACT_WAIT.StopAction();
			ACT_MELEE.StopAction();
			ACT_TELEPORT.StopAction();
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			isidoraBehaviour.Isidora.AnimatorInyector.ResetAll();
			isidoraBehaviour.Isidora.Audio.SetSkullsChoir(on: false);
			isidoraBehaviour.transform.DOKill();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			float vOffset = 0.3f;
			if (o.IsAnimatorInState("OUT"))
			{
				o.transform.position = o.ArenaGetBotFarRandomPoint();
				o.LookAtTarget();
				o.Isidora.AnimatorInyector.SetHidden(hidden: false);
				yield return new WaitUntilIdle(o);
			}
			o.Isidora.Audio.SetSkullsChoir(on: true);
			Vector2 dir2 = o.GetDirToPenitent();
			Vector2 startingPosition = o.transform.position;
			if (Mathf.Abs(dir2.x) > distanceFromPenitent)
			{
				if (dir2.x > 0f)
				{
					startingPosition.x = p.GetPosition().x - distanceFromPenitent;
				}
				else
				{
					startingPosition.x = p.GetPosition().x + distanceFromPenitent;
				}
			}
			startingPosition.y = o.battleBounds.yMin - 0.5f;
			float approachTime = ((Vector2)o.transform.position - startingPosition).magnitude * 0.1f + 0.2f;
			ACT_MOVE.StartAction(o, startingPosition, approachTime, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			ACT_MELEE.StartAction(o, dir2, ISIDORA_SLASHES.SLASH, slashTime, hold: true, holdTime1, twirl: true, vanishTime, endsWithTwirlActive: true, delegate
			{
				ACT_MOVE.StopAction();
			});
			dir2 = o.GetDirToPenitent();
			float anticipationDistance = 1.5f;
			Vector2 anticipationPoint2 = (Vector2)o.transform.position + Vector2.left * Mathf.Sign(dir2.x) * anticipationDistance;
			anticipationPoint2 = o.ClampInsideBoundaries(anticipationPoint2);
			ACT_MOVE.StartAction(o, anticipationPoint2, holdTime1, Ease.OutQuad);
			yield return ACT_MOVE.waitForCompletion;
			Vector2 afterSlashPosition2 = startingPosition + Vector2.right * slashDistance * Mathf.Sign(dir2.x) + Vector2.down * vOffset;
			afterSlashPosition2 = o.ClampInsideBoundaries(afterSlashPosition2);
			ACT_MOVE.StartAction(o, afterSlashPosition2, slashTime, Ease.OutQuad);
			yield return ACT_MOVE.waitForCompletion;
			Vector2 vanishTarget2 = (Vector2)p.GetPosition() + Vector2.left * Mathf.Sign(dir2.x) * vanishDistance;
			vanishTarget2.y = afterSlashPosition2.y + vOffset;
			vanishTarget2 = o.ClampInsideBoundaries(vanishTarget2);
			ACT_TELEPORT.StartAction(o, vanishTarget2, vanishTime, twirlOnExit: true, castingOnExit: false, o.GetTarget());
			yield return ACT_TELEPORT.waitForCompletion;
			yield return ACT_MELEE.waitForCompletion;
			int j = repetitions;
			Vector2 newDir = o.GetDirToPenitent();
			for (int i = 0; i < j; i++)
			{
				newDir = o.GetDirToPenitent();
				float remainingTime = o.Isidora.Audio.GetTimeLeftForCurrentBar();
				if (remainingTime < holdTime2)
				{
					float singleBarDuration = o.Isidora.Audio.GetSingleBarDuration();
					remainingTime += singleBarDuration;
					bool isFirstLoop = true;
					Debug.Log("<color=magenta> ENTRANDO EN EL TWIRL DANCE </color>");
					Vector2 point = (Vector2)o.transform.position + Vector2.left * Mathf.Sign(newDir.x) * vanishDistance * 0.8f;
					point = o.ClampInsideBoundaries(point);
					o.transform.DOMove(point, singleBarDuration * 0.5f).SetEase(Ease.InOutQuad).SetLoops(2, LoopType.Yoyo)
						.OnStepComplete(delegate
						{
							if (isFirstLoop)
							{
								isFirstLoop = false;
								newDir = o.GetDirToPenitent();
								o.LookAtTarget();
							}
						});
				}
				else
				{
					Vector2 point2 = (Vector2)o.transform.position + Vector2.left * Mathf.Sign(newDir.x) * vanishDistance;
					point2 = o.ClampInsideBoundaries(point2);
					float duration = Mathf.Clamp(remainingTime - holdTime2, 0.75f, remainingTime - holdTime2);
					o.transform.DOMove(point2, duration).SetEase(Ease.OutQuad);
				}
				ACT_WAIT.StartAction(o, remainingTime - holdTime2);
				yield return ACT_WAIT.waitForCompletion;
				bool keepTwirl = i < j - 1;
				o.ghostTrail.EnableGhostTrail = true;
				ACT_MELEE.StartAction(o, newDir, ISIDORA_SLASHES.SLASH, slashTime, hold: true, holdTime2, twirl: true, twirlTime, keepTwirl, delegate
				{
					ACT_MOVE.StopAction();
				});
				Vector2 targetPoint2 = (Vector2)o.transform.position + Vector2.left * Mathf.Sign(newDir.x) * anticipationDistance;
				targetPoint2 = o.ClampInsideBoundaries(targetPoint2);
				ACT_MOVE.StartAction(o, targetPoint2, holdTime2, Ease.OutQuad);
				yield return ACT_MOVE.waitForCompletion;
				float nSlashDistance = o.GetDirToPenitent().magnitude + 1f;
				Vector2 afterSlashPosition4 = (Vector2)o.transform.position + Vector2.right * Mathf.Sign(newDir.x) * nSlashDistance + Vector2.down * vOffset;
				afterSlashPosition4 = o.ClampInsideBoundaries(afterSlashPosition4);
				ACT_MOVE.StartAction(o, afterSlashPosition4, slashTime, Ease.OutQuad);
				yield return ACT_MOVE.waitForCompletion;
				o.ghostTrail.EnableGhostTrail = false;
				if (!keepTwirl)
				{
					twirlDistance += 2f;
					twirlTime += 1f;
				}
				Vector2 twirlTarget3 = (Vector2)o.transform.position + Vector2.left * Mathf.Sign(newDir.x) * twirlDistance + Vector2.up * vOffset;
				twirlTarget3 = o.ClampInsideBoundaries(twirlTarget3);
				o.Isidora.AnimatorInyector.Decelerate(twirlTime * 0.5f);
				ACT_MOVE.StartAction(o, twirlTarget3, twirlTime, Ease.InOutQuad);
				yield return ACT_MOVE.waitForCompletion;
				yield return ACT_MELEE.waitForCompletion;
			}
			o.Isidora.Audio.SetSkullsChoir(on: false);
			FinishAction();
		}
	}

	public class FadeSlashCombo1_EnemyAction : EnemyAction
	{
		private SingleFadeSlash_EnemyAction ACT_FADE_SLASH = new SingleFadeSlash_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_FADE_SLASH.StopAction();
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			IsidoraAnimatorInyector anim = o.Isidora.AnimatorInyector;
			int j = 1;
			for (int i = 0; i < j; i++)
			{
				Vector2 targetPoint = o.ArenaGetBotRightCorner() + Vector2.left;
				Vector2 dir = Vector2.left;
				if (o.IsIsidoraOnTheRightSide())
				{
					dir = Vector2.right;
					targetPoint = o.ArenaGetBotLeftCorner() + Vector2.right;
				}
				targetPoint.y -= 0.75f;
				ACT_FADE_SLASH.StartAction(o, dir, targetPoint);
				yield return ACT_FADE_SLASH.waitForCompletion;
			}
			FinishAction();
		}
	}

	public class SingleFadeSlash_EnemyAction : EnemyAction
	{
		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_AUX = new MoveEasing_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private Teleport_EnemyAction ACT_TELEPORT = new Teleport_EnemyAction();

		private BlastsAttack_EnemyAction ACT_BLASTS = new BlastsAttack_EnemyAction();

		private Vector2 startPosition;

		private Vector2 direction;

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 direction, Vector2 startPosition)
		{
			this.direction = direction;
			this.startPosition = startPosition;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_MOVE.StopAction();
			ACT_MOVE_AUX.StopAction();
			ACT_WAIT.StopAction();
			ACT_TELEPORT.StopAction();
			ACT_BLASTS.StopAction();
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			isidoraBehaviour.Isidora.AnimatorInyector.ResetAll();
			isidoraBehaviour.Isidora.Audio.SetSkullsChoir(on: false);
			isidoraBehaviour.Isidora.Audio.SetIsidoraVoice(on: false);
			isidoraBehaviour.transform.DOKill();
			isidoraBehaviour.Isidora.AnimatorInyector.SetFadeSlash(fade: false);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			IsidoraAnimatorInyector anim = o.Isidora.AnimatorInyector;
			if (!o.IsAnimatorInState("OUT"))
			{
				o.Isidora.AnimatorInyector.SetHidden(hidden: true);
			}
			yield return new WaitUntilOut(o);
			Vector2 targetPoint = startPosition;
			o.transform.position = targetPoint;
			int currentBar = o.Isidora.Audio.bossAudioSync.LastBar;
			int lastAttackMarkerBar = o.Isidora.Audio.lastAttackMarkerBar;
			o.LookAtDirUsingOrientation(direction);
			o.Isidora.AnimatorInyector.SetHidden(hidden: false);
			yield return new WaitUntilIdle(o);
			anim.PlaySlashAttack();
			anim.SetAttackAnticipation(hold: true);
			float untilAnticipation = 0.7f;
			ACT_WAIT.StartAction(o, untilAnticipation);
			yield return ACT_WAIT.waitForCompletion;
			Debug.Log($"<color=blue>Anticipation starts!</color>");
			float remainingTime = o.Isidora.Audio.GetTimeLeftForCurrentBar();
			Debug.Log(string.Format("<color=blue>LETS SEE -> Remaining time for current bar </color>" + remainingTime));
			if (remainingTime < 0.75f)
			{
				Debug.Log($"<color=orange>NOT ENOUGH TIME. Adding a full bar. </color>");
				remainingTime += o.Isidora.Audio.GetSingleBarDuration();
				Debug.Log(string.Format("<color=orange>[0]Remaining time for current bar </color>" + remainingTime));
			}
			ACT_WAIT.StartAction(o, remainingTime * 0.33f);
			yield return ACT_WAIT.waitForCompletion;
			Debug.Log(string.Format("<color=blue>[1]Remaining time for current bar </color>" + o.Isidora.Audio.GetTimeLeftForCurrentBar()));
			Debug.Log(string.Format("<color=blue>[1]Accumulated remainingTime </color>" + (remainingTime - remainingTime * 0.33f)));
			Vector2 anticipationDir = ((!o.IsIsidoraOnTheRightSide()) ? Vector2.left : Vector2.right);
			targetPoint = (Vector2)o.transform.position + anticipationDir;
			ACT_MOVE.StartAction(o, targetPoint, remainingTime * 0.33f, Ease.OutCubic);
			yield return ACT_MOVE.waitForCompletion;
			Debug.Log(string.Format("<color=blue>[2]Remaining time for current bar </color>" + o.Isidora.Audio.GetTimeLeftForCurrentBar()));
			Debug.Log(string.Format("<color=blue>[2]Accumulated remainingTime </color>" + (remainingTime - remainingTime * 0.66f)));
			float fadeSlashAnticipationSeconds = 0.25f;
			float justWait = remainingTime * 0.33f - fadeSlashAnticipationSeconds;
			ACT_MOVE.StartAction(o, targetPoint - anticipationDir, justWait, Ease.InCubic);
			yield return ACT_MOVE.waitForCompletion;
			Debug.Log(string.Format("<color=blue>[3]Remaining time for current bar </color>" + o.Isidora.Audio.GetTimeLeftForCurrentBar()));
			Debug.Log(string.Format("<color=blue>[3]Accumulated remainingTime </color>" + (remainingTime - remainingTime * 0.66f - justWait)));
			int j = 3;
			for (int i = 0; i < j; i++)
			{
				Debug.Log($"<color=blue>Bar finished: Fade Slash starts!</color>");
				targetPoint.x = ((!o.IsIsidoraOnTheRightSide()) ? (o.battleBounds.xMax - 2f) : (o.battleBounds.xMin + 2f));
				anim.SetFadeSlash(fade: true);
				anim.SetHidden(hidden: true);
				anim.SetAttackAnticipation(hold: false);
				o.ghostTrail.EnableGhostTrail = true;
				o.WarningVFX(Vector2.up);
				float waitBeforeMove = ((i != 0) ? fadeSlashAnticipationSeconds : (fadeSlashAnticipationSeconds - 0.1f));
				ACT_WAIT.StartAction(o, waitBeforeMove);
				yield return ACT_WAIT.waitForCompletion;
				Debug.Log(string.Format("<color=blue>[4]Remaining time for current bar </color>" + o.Isidora.Audio.GetTimeLeftForCurrentBar()));
				o.LinearScreenshake();
				o.SlashLineVFX();
				o.Isidora.Audio.PlayFadeDash();
				ACT_MOVE.StartAction(o, targetPoint, 0.1f, Ease.InOutCubic);
				yield return ACT_MOVE.waitForCompletion;
				o.ghostTrail.EnableGhostTrail = false;
				if (i < j - 1)
				{
					float restSeconds = 0.75f;
					ACT_WAIT.StartAction(o, restSeconds);
					yield return ACT_WAIT.waitForCompletion;
					o.LookAtTarget(o.battleBounds.center);
					anim.SetHidden(hidden: false);
					ACT_WAIT.StartAction(o, 0.05f);
					yield return ACT_WAIT.waitForCompletion;
				}
				else
				{
					anim.SetFadeSlash(fade: false);
					anim.SetHidden(hidden: false);
					ACT_WAIT.StartAction(o, 0.1f);
					yield return ACT_WAIT.waitForCompletion;
				}
			}
			Vector2 startingPos = o.transform.position;
			ACT_BLASTS.StartAction(o, startingPos - Vector2.right * 0.75f, Vector2.right, 2f, shouldVanish: false, 1, 0.4f);
			ACT_BLASTS.StartAction(o, startingPos + Vector2.right * 0.75f, Vector2.left, 2f, shouldVanish: false, 1, 0.4f);
			anim.SetFadeSlash(fade: false);
			anim.SetHidden(hidden: true);
			float fadeSlashRecoverSeconds = 1.85f;
			ACT_WAIT.StartAction(o, fadeSlashRecoverSeconds);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class SyncCombo1_EnemyAction : EnemyAction
	{
		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE_AUX = new MoveEasing_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private Teleport_EnemyAction ACT_TELEPORT = new Teleport_EnemyAction();

		private BlastsAttack_EnemyAction ACT_BLASTS = new BlastsAttack_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_MOVE.StopAction();
			ACT_MOVE_AUX.StopAction();
			ACT_WAIT.StopAction();
			ACT_TELEPORT.StopAction();
			ACT_BLASTS.StopAction();
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			isidoraBehaviour.Isidora.AnimatorInyector.ResetAll();
			isidoraBehaviour.Isidora.Audio.SetSkullsChoir(on: false);
			isidoraBehaviour.Isidora.Audio.SetIsidoraVoice(on: false);
			isidoraBehaviour.transform.DOKill();
			isidoraBehaviour.Isidora.AnimatorInyector.SetFadeSlash(fade: false);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			IsidoraAnimatorInyector anim = o.Isidora.AnimatorInyector;
			if (!o.IsAnimatorInState("OUT"))
			{
				o.Isidora.AnimatorInyector.SetHidden(hidden: true);
			}
			yield return new WaitUntilOut(o);
			Vector2 targetPoint = ((!o.IsIsidoraOnTheRightSide()) ? (o.ArenaGetBotRightCorner() + Vector2.left) : (o.ArenaGetBotLeftCorner() + Vector2.right));
			targetPoint.y -= 0.4f;
			o.transform.position = targetPoint;
			int currentBar = o.Isidora.Audio.bossAudioSync.LastBar;
			int lastAttackMarkerBar = o.Isidora.Audio.lastAttackMarkerBar;
			if (currentBar != lastAttackMarkerBar)
			{
			}
			o.Isidora.AnimatorInyector.SetHidden(hidden: false);
			o.LookAtTarget(o.ArenaGetBotFarRandomPoint());
			anim.PlaySlashAttack();
			anim.SetAttackAnticipation(hold: true);
			float untilAnticipation = 0.5f;
			ACT_WAIT.StartAction(o, untilAnticipation);
			yield return ACT_WAIT.waitForCompletion;
			Debug.Log($"<color=blue>Bar finished: Anticipation starts!</color>");
			float remainingTime = o.Isidora.Audio.GetTimeLeftForCurrentBar();
			ACT_WAIT.StartAction(o, remainingTime * 0.3f);
			yield return ACT_WAIT.waitForCompletion;
			Vector2 anticipationDir2 = ((!o.IsIsidoraOnTheRightSide()) ? Vector2.left : Vector2.right);
			targetPoint = (Vector2)o.transform.position + anticipationDir2;
			ACT_MOVE.StartAction(o, targetPoint, remainingTime * 0.3f, Ease.InOutQuad);
			yield return new WaitUntilBarFinishes(o.Isidora.Audio);
			Debug.Log($"<color=blue>Bar finished: Attack 1 starts!</color>");
			anim.SetTwirl(twirl: true);
			targetPoint.x = ((!o.IsIsidoraOnTheRightSide()) ? (o.battleBounds.xMax - 2f) : (o.battleBounds.xMin + 2f));
			anim.SetFadeSlash(fade: true);
			anim.SetAttackAnticipation(hold: false);
			float fadeSlashAnticipationSeconds = 0.25f;
			float fadeSlashRecoverSeconds = 0.7f;
			ACT_WAIT.StartAction(o, fadeSlashAnticipationSeconds);
			yield return ACT_WAIT.waitForCompletion;
			o.SlashLineVFX();
			ACT_MOVE.StartAction(o, targetPoint, 0.1f, Ease.InOutCubic);
			yield return ACT_MOVE.waitForCompletion;
			float attackTime = o.Isidora.Audio.GetSingleBarDuration() * 0.4f;
			Vector2 startingPos = o.transform.position;
			ACT_BLASTS.StartAction(o, startingPos - Vector2.right * 0.5f, Vector2.right, 1.5f, shouldVanish: false, 3, 0.5f);
			ACT_BLASTS.StartAction(o, startingPos + Vector2.right * 0.5f, Vector2.left, 1.5f, shouldVanish: false, 3, 0.5f);
			anim.SetFadeSlash(fade: false);
			ACT_WAIT.StartAction(o, fadeSlashRecoverSeconds);
			yield return ACT_WAIT.waitForCompletion;
			float waitTime = o.Isidora.Audio.GetSingleBarDuration() * 0.5f;
			anim.Decelerate(waitTime * 0.8f);
			ACT_WAIT.StartAction(o, waitTime);
			yield return ACT_WAIT.waitForCompletion;
			yield return new WaitUntilBarFinishes(o.Isidora.Audio);
			Debug.Log($"<color=blue>Bar finished: Dance starts!</color>");
			remainingTime = o.Isidora.Audio.GetSingleBarDuration();
			targetPoint = o.transform.position + Vector3.up * 0.75f;
			ACT_MOVE.StartAction(o, targetPoint, remainingTime * 0.5f, Ease.InOutCubic, null, _timeScaled: true, null, _tweenOnX: false);
			anticipationDir2 = ((!o.IsIsidoraOnTheRightSide()) ? Vector2.left : Vector2.right);
			targetPoint.x = o.transform.position.x + anticipationDir2.x;
			ACT_MOVE_AUX.StartAction(o, targetPoint, remainingTime * 0.3f, Ease.InOutCubic, null, _timeScaled: true, null, _tweenOnX: true, _tweenOnY: false);
			yield return ACT_MOVE.waitForCompletion;
			yield return ACT_MOVE_AUX.waitForCompletion;
			anim.resetAnimationSpeedFlag = true;
			anim.PlaySlashAttack();
			anim.SetAttackAnticipation(hold: true);
			o.LookAtTarget(o.ArenaGetBotFarRandomPoint());
			yield return new WaitUntilBarFinishes(o.Isidora.Audio);
			Debug.Log($"<color=blue>Bar finished: Attack 2 starts!</color>");
			targetPoint.x = ((!o.IsIsidoraOnTheRightSide()) ? (o.ArenaGetBotRightCorner().x - 2f) : (o.ArenaGetBotLeftCorner().x + 2f));
			ACT_MOVE.StartAction(o, targetPoint, attackTime, Ease.InOutCubic);
			ACT_WAIT.StartAction(o, attackTime * 0.1f);
			yield return ACT_WAIT.waitForCompletion;
			o.Isidora.Audio.SetIsidoraVoice(on: false);
			anim.SetAttackAnticipation(hold: false);
			yield return ACT_MOVE.waitForCompletion;
			float twirlTime = o.Isidora.Audio.GetSingleBarDuration() * 0.5f;
			anim.Decelerate(twirlTime * 0.3f);
			targetPoint.y -= 0.75f;
			ACT_MOVE.StartAction(o, targetPoint, twirlTime, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			anim.SetTwirl(twirl: false);
			FinishAction();
		}
	}

	public class HorizontalDash_EnemyAction : EnemyAction
	{
		private float anticipationTime;

		private float dashDuration;

		private float shoryukenDuration;

		private float floatDownDuration;

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE2 = new MoveEasing_EnemyAction();

		private Teleport_EnemyAction ACT_TELEPORT = new Teleport_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MeleeAttack_EnemyAction ACT_MELEE = new MeleeAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST = new SingleBlastAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST2 = new SingleBlastAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST3 = new SingleBlastAttack_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, float anticipationTime, float dashDuration, float shoryukenDuration, float floatDownDuration)
		{
			this.anticipationTime = anticipationTime;
			this.dashDuration = dashDuration;
			this.shoryukenDuration = shoryukenDuration;
			this.floatDownDuration = floatDownDuration;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_MOVE.StopAction();
			ACT_MOVE2.StopAction();
			ACT_TELEPORT.StopAction();
			ACT_WAIT.StopAction();
			ACT_MELEE.StopAction();
			ACT_SINGLEBLAST.StopAction();
			ACT_SINGLEBLAST2.StopAction();
			ACT_SINGLEBLAST3.StopAction();
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			IsidoraAnimatorInyector animatorInyector = isidoraBehaviour.Isidora.AnimatorInyector;
			animatorInyector.SetFireScythe(on: false);
			animatorInyector.ResetAll();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			IsidoraAnimatorInyector anim = o.Isidora.AnimatorInyector;
			float distanceFromWall = 2f;
			Vector2 leftCorner = o.ArenaGetBotLeftCorner() + Vector2.right * distanceFromWall;
			Vector2 rightCorner = o.ArenaGetBotRightCorner() + Vector2.left * distanceFromWall;
			Vector2 dir = ((!(p.GetPosition().x > o.battleBounds.center.x)) ? Vector2.left : Vector2.right);
			Vector2 startPosition = ((!(dir.x > 0f)) ? rightCorner : leftCorner);
			Vector2 endPosition = ((!(dir.x > 0f)) ? leftCorner : rightCorner);
			startPosition.y += 0.5f;
			endPosition.y += 0.5f;
			endPosition -= dir * 2f;
			if (o.IsAnimatorInState("OUT"))
			{
				o.transform.position = startPosition;
				anim.SetHidden(hidden: false);
				anim.SetTwirl(twirl: true);
				ACT_WAIT.StartAction(o, 0.1f);
				yield return ACT_WAIT.waitForCompletion;
			}
			else
			{
				ACT_TELEPORT.StartAction(o, startPosition, 0.1f, twirlOnExit: true, castingOnExit: false, p.transform);
				yield return ACT_TELEPORT.waitForCompletion;
			}
			o.LookAtDirUsingOrientation(dir);
			anim.SetFireScythe(on: true);
			anim.PlayRisingSlash();
			anim.SetAttackAnticipation(hold: true);
			ACT_SINGLEBLAST.StartAction(o, o.transform.position - (Vector3)dir * 1.5f, 0f, 0.25f, 0.5f, 0.1f);
			ACT_WAIT.StartAction(o, anticipationTime);
			yield return ACT_WAIT.waitForCompletion;
			ACT_MOVE.StartAction(o, endPosition, dashDuration, Ease.InCubic, null, _timeScaled: true, null, _tweenOnX: true, _tweenOnY: false);
			ACT_WAIT.StartAction(o, dashDuration * 0.9f);
			yield return ACT_WAIT.waitForCompletion;
			anim.SetAttackAnticipation(hold: false);
			float risingHeight = 4f;
			float extraDelay = shoryukenDuration * 0.5f;
			Vector2 blastOffset = dir * 1.25f;
			ACT_SINGLEBLAST.StartAction(o, endPosition + blastOffset, 0f, 0.2f, 0.3f, extraDelay);
			ACT_SINGLEBLAST2.StartAction(o, endPosition + blastOffset * 2f, 0f, 0.2f, 0.3f, extraDelay + 0.2f);
			ACT_SINGLEBLAST3.StartAction(o, endPosition + blastOffset * 3f, 0f, 0.2f, 0.3f, extraDelay + 0.4f);
			ACT_MOVE2.StartAction(o, endPosition + Vector2.up * risingHeight, shoryukenDuration, Ease.OutCubic, null, _timeScaled: true, null, _tweenOnX: false);
			yield return ACT_MOVE2.waitForCompletion;
			anim.Decelerate(floatDownDuration * 0.7f);
			ACT_MOVE2.StartAction(o, endPosition, floatDownDuration, Ease.InOutQuad);
			yield return ACT_MOVE2.waitForCompletion;
			anim.SetTwirl(twirl: false);
			anim.SetFireScythe(on: false);
			ACT_WAIT.StartAction(o, 0.3f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class InvisibleHorizontalDash_EnemyAction : EnemyAction
	{
		private float anticipationTime;

		private float dashDuration;

		private float shoryukenDuration;

		private float floatDownDuration;

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE2 = new MoveEasing_EnemyAction();

		private Teleport_EnemyAction ACT_TELEPORT = new Teleport_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private MeleeAttack_EnemyAction ACT_MELEE = new MeleeAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST = new SingleBlastAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST2 = new SingleBlastAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST3 = new SingleBlastAttack_EnemyAction();

		private Vector2 endPosition;

		public EnemyAction StartAction(EnemyBehaviour e, float anticipationTime, float dashDuration, float shoryukenDuration, float floatDownDuration)
		{
			this.anticipationTime = anticipationTime;
			this.dashDuration = dashDuration;
			this.shoryukenDuration = shoryukenDuration;
			this.floatDownDuration = floatDownDuration;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_MOVE.StopAction();
			ACT_MOVE2.StopAction();
			ACT_TELEPORT.StopAction();
			ACT_WAIT.StopAction();
			ACT_MELEE.StopAction();
			ACT_SINGLEBLAST.StopAction();
			ACT_SINGLEBLAST2.StopAction();
			ACT_SINGLEBLAST3.StopAction();
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			IsidoraAnimatorInyector animatorInyector = isidoraBehaviour.Isidora.AnimatorInyector;
			animatorInyector.SetFireScythe(on: false);
			animatorInyector.ResetAll();
			isidoraBehaviour.floorSparksParticlesToRight.Stop();
			isidoraBehaviour.floorSparksParticlesToLeft.Stop();
			isidoraBehaviour.floorSparksMaskToRight.transform.DOKill(complete: true);
			isidoraBehaviour.floorSparksMaskToLeft.transform.DOKill(complete: true);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			IsidoraAnimatorInyector anim = o.Isidora.AnimatorInyector;
			float distanceFromWall = 2f;
			Vector2 leftCorner = o.ArenaGetBotLeftCorner() + Vector2.right * distanceFromWall;
			Vector2 rightCorner = o.ArenaGetBotRightCorner() + Vector2.left * distanceFromWall;
			Vector2 dir = ((!(p.GetPosition().x > o.battleBounds.center.x)) ? Vector2.left : Vector2.right);
			Vector2 startPosition = ((!(dir.x > 0f)) ? rightCorner : leftCorner);
			endPosition = ((!(dir.x > 0f)) ? leftCorner : rightCorner);
			startPosition.y += 0.5f;
			endPosition.y += 0.5f;
			endPosition -= dir * 2f;
			if (!o.IsAnimatorInState("OUT"))
			{
				o.Isidora.AnimatorInyector.SetHidden(hidden: true);
			}
			yield return new WaitUntilOut(o);
			o.transform.position = startPosition;
			o.LookAtDirUsingOrientation(dir);
			anim.SetFireScythe(on: true);
			anim.PlayRisingSlash();
			anim.SetAttackAnticipation(hold: true);
			o.SingleSparkVFX();
			ACT_WAIT.StartAction(o, anticipationTime);
			yield return ACT_WAIT.waitForCompletion;
			o.Isidora.Audio.PlayInvisibleDash();
			if (dir.x > 0f)
			{
				o.floorSparksParticlesToLeft.Play();
				o.floorSparksMaskToLeft.gameObject.SetActive(value: true);
				Vector3 prevScale2 = o.floorSparksMaskToLeft.transform.localScale;
				o.floorSparksMaskToLeft.transform.localScale = Vector3.zero;
				o.floorSparksMaskToLeft.transform.DOScale(prevScale2, dashDuration).OnComplete(delegate
				{
					o.floorSparksMaskToLeft.gameObject.SetActive(value: false);
					o.floorSparksMaskToLeft.transform.localScale = prevScale2;
				});
			}
			else
			{
				o.floorSparksParticlesToRight.Play();
				o.floorSparksMaskToRight.gameObject.SetActive(value: true);
				Vector3 prevScale = o.floorSparksMaskToRight.transform.localScale;
				o.floorSparksMaskToRight.transform.localScale = Vector3.zero;
				o.floorSparksMaskToRight.transform.DOScale(prevScale, dashDuration).OnComplete(delegate
				{
					o.floorSparksMaskToRight.gameObject.SetActive(value: false);
					o.floorSparksMaskToRight.transform.localScale = prevScale;
				});
			}
			ACT_MOVE.StartAction(o, endPosition, dashDuration, Ease.InQuart, null, _timeScaled: true, CheckIfNearPenitentToAppear, _tweenOnX: true, _tweenOnY: false);
			yield return ACT_MOVE.waitForCompletion;
			anim.SetTwirl(twirl: true);
			anim.SetAttackAnticipation(hold: false);
			o.floorSparksParticlesToRight.Stop();
			o.floorSparksParticlesToLeft.Stop();
			o.floorSparksMaskToRight.transform.DOKill(complete: true);
			o.floorSparksMaskToLeft.transform.DOKill(complete: true);
			endPosition = (Vector2)o.transform.position + dir;
			ACT_MOVE.StartAction(o, endPosition, 0.3f, Ease.InQuad);
			yield return ACT_MOVE.waitForCompletion;
			float extraDelay = shoryukenDuration * 0.5f;
			Vector2 blastOffset = dir * 1.25f;
			ACT_SINGLEBLAST.StartAction(o, endPosition + blastOffset, 0f, 0.2f, 0.3f, extraDelay);
			ACT_SINGLEBLAST2.StartAction(o, endPosition + blastOffset * 2f, 0f, 0.2f, 0.3f, extraDelay + 0.2f);
			ACT_SINGLEBLAST3.StartAction(o, endPosition + blastOffset * 3f, 0f, 0.2f, 0.3f, extraDelay + 0.4f);
			o.ghostTrail.EnableGhostTrail = true;
			float risingHeight = 4f;
			ACT_MOVE2.StartAction(o, endPosition + Vector2.up * risingHeight, shoryukenDuration, Ease.OutCubic, null, _timeScaled: true, null, _tweenOnX: false);
			yield return ACT_MOVE2.waitForCompletion;
			o.ghostTrail.EnableGhostTrail = false;
			anim.Decelerate(floatDownDuration * 0.7f);
			ACT_MOVE2.StartAction(o, endPosition, floatDownDuration, Ease.InOutQuad);
			yield return ACT_MOVE2.waitForCompletion;
			anim.SetTwirl(twirl: false);
			anim.SetFireScythe(on: false);
			ACT_WAIT.StartAction(o, 0.3f);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}

		private void CheckIfNearPenitentToAppear()
		{
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
			IsidoraAnimatorInyector animatorInyector = isidoraBehaviour.Isidora.AnimatorInyector;
			if (Vector2.Distance(isidoraBehaviour.transform.position, penitent.GetPosition()) < 2f || Vector2.Distance(isidoraBehaviour.transform.position, endPosition) < 2f)
			{
				animatorInyector.SetHidden(hidden: false);
			}
			if (!isidoraBehaviour.IsAnimatorInState("OUT"))
			{
				ACT_MOVE.StopAction();
			}
		}
	}

	public class Teleport_EnemyAction : EnemyAction
	{
		private Vector2 targetPosition;

		private float timeInvisible;

		private bool twirlOnExit;

		private bool castingOnExit;

		private Transform lookTarget;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 targetPosition, float timeInvisible, bool twirlOnExit, bool castingOnExit, Transform lookTarget)
		{
			this.targetPosition = targetPosition;
			this.timeInvisible = timeInvisible;
			this.twirlOnExit = twirlOnExit;
			this.castingOnExit = castingOnExit;
			this.lookTarget = lookTarget;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			(owner as IsidoraBehaviour).Isidora.AnimatorInyector.ResetAll();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			IsidoraAnimatorInyector anim = o.Isidora.AnimatorInyector;
			anim.SetHidden(hidden: true);
			timeInvisible = Mathf.Clamp(timeInvisible, anim.GetVanishAnimationDuration(), timeInvisible);
			ACT_WAIT.StartAction(o, timeInvisible);
			yield return ACT_WAIT.waitForCompletion;
			o.transform.position = targetPosition;
			if (lookTarget != null)
			{
				o.LookAtTarget(lookTarget.position);
			}
			anim.SetTwirl(twirlOnExit);
			anim.SetCasting(castingOnExit);
			anim.SetHidden(hidden: false);
			if (twirlOnExit)
			{
				yield return new WaitUntilTwirl(o);
			}
			else if (castingOnExit)
			{
				yield return new WaitUntilCasting(o);
			}
			else
			{
				yield return new WaitUntilIdle(o);
			}
			FinishAction();
		}
	}

	public class PhaseSwitchAttack_EnemyAction : EnemyAction
	{
		private BonfireProjectilesAttack_EnemyAction ACT_BONFIRE = new BonfireProjectilesAttack_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private BlastsAttack_EnemyAction ACT_BLASTS = new BlastsAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST = new SingleBlastAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST2 = new SingleBlastAttack_EnemyAction();

		private Teleport_EnemyAction ACT_TELEPORT = new Teleport_EnemyAction();

		protected override void DoOnStop()
		{
			ACT_BONFIRE.StopAction();
			ACT_MOVE.StopAction();
			ACT_WAIT.StopAction();
			ACT_BLASTS.StopAction();
			ACT_TELEPORT.StopAction();
			ACT_SINGLEBLAST.StopAction();
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			isidoraBehaviour.Isidora.AnimatorInyector.ResetAll();
			isidoraBehaviour.Isidora.Audio.SetIsidoraVoice(on: false);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour isidora = owner as IsidoraBehaviour;
			IsidoraAnimatorInyector animatorInyector = isidora.Isidora.AnimatorInyector;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			IsidoraAudio mAudio = isidora.Isidora.Audio;
			isidora.Isidora.Audio.SetSecondPhase();
			Debug.Log("Audio.SetSecondPhase TIME:  " + mAudio.GetTimeSinceLevelLoad());
			animatorInyector.SetHidden(hidden: true);
			ACT_SINGLEBLAST.StartAction(isidora, isidora.transform.position, 0.5f, 1f, 3f);
			ACT_WAIT.StartAction(isidora, 0.8f);
			yield return ACT_WAIT.waitForCompletion;
			Debug.Log("Audio.WaitUntilNextValidBar - TIME:  " + mAudio.GetTimeSinceLevelLoad());
			yield return new WaitUntilNextValidBar(mAudio);
			Debug.Log("AUDIO: ToPhase3 (2 bars) TIME:  " + mAudio.GetTimeSinceLevelLoad());
			float barTime = mAudio.GetSingleBarDuration();
			Debug.Log("BARTIME: " + barTime);
			Vector2 targetPoint4 = isidora.ArenaGetBotRightCorner() + Vector2.left * 2f;
			Vector2 blastOrientation = Vector2.left;
			if (p.GetPosition().x > isidora.battleBounds.center.x)
			{
				targetPoint4 = isidora.ArenaGetBotLeftCorner() + Vector2.right * 2f;
				blastOrientation = Vector2.right;
			}
			Vector2 startingPos2 = targetPoint4 + blastOrientation * -2.5f;
			ACT_BLASTS.StartAction(isidora, startingPos2, blastOrientation, barTime, shouldVanish: false, 5, 4f);
			yield return ACT_BLASTS.waitForCompletion;
			startingPos2 += blastOrientation * -0.5f;
			ACT_BLASTS.StartAction(isidora, startingPos2, blastOrientation, barTime, shouldVanish: false, 7, 3f);
			yield return ACT_BLASTS.waitForCompletion;
			Debug.Log("AUDIO: Phase3 starts (2 bars) TIME:  " + mAudio.GetTimeSinceLevelLoad());
			isidora.Isidora.Audio.SetIsidoraVoice(on: true);
			targetPoint4 = isidora.battleBounds.center + Vector2.right * 0.3f;
			isidora.transform.position = targetPoint4;
			startingPos2 += blastOrientation * 1.5f;
			ACT_BLASTS.StartAction(isidora, startingPos2, blastOrientation, barTime, shouldVanish: false, 5, 4f);
			yield return ACT_BLASTS.waitForCompletion;
			startingPos2 += blastOrientation * -1.5f;
			ACT_BLASTS.StartAction(isidora, startingPos2, blastOrientation, barTime, shouldVanish: false, 7, 3f);
			yield return ACT_BLASTS.waitForCompletion;
			Debug.Log("AUDIO: Epic voice starts(2 bars) TIME:  " + mAudio.GetTimeSinceLevelLoad());
			animatorInyector.SetHidden(hidden: false);
			animatorInyector.SetCasting(active: true);
			targetPoint4 = isidora.transform.position + Vector3.up * 3f;
			ACT_MOVE.StartAction(isidora, targetPoint4, 2f * barTime, Ease.InOutCubic);
			Vector2 castingPos = isidora.homingBonfireBehavior.gameObject.transform.position;
			ACT_BONFIRE.StartAction(isidora, isidora.Isidora.Audio.GetSingleBarDuration(), 1, useCastingPosition: true, castingPos, 0f, 0f, -1f);
			yield return ACT_BONFIRE.waitForCompletion;
			MasterShaderEffects effects = Core.Logic.Penitent.GetComponentInChildren<MasterShaderEffects>();
			if (effects != null)
			{
				effects.StartColorizeLerp(0.5f, 0f, 5f, null);
			}
			isidora.Isidora.DamageEffect.StartColorizeLerp(0.2f, 0f, 5f, null);
			startingPos2 = isidora.ArenaGetBotLeftCorner();
			Vector2 startingPosRight = isidora.ArenaGetBotRightCorner();
			int j = 6;
			for (int i = 0; i < j; i++)
			{
				float baseOffset = 1.5f;
				Vector2 offset = Vector2.right * i * baseOffset;
				ACT_SINGLEBLAST.StartAction(isidora, startingPos2 + offset, 0.5f, 1f, 0.7f);
				ACT_SINGLEBLAST2.StartAction(isidora, startingPosRight - offset, 0.5f, 1f, 0.7f);
				yield return ACT_SINGLEBLAST.waitForCompletion;
			}
			yield return ACT_MOVE.waitForCompletion;
			animatorInyector.SetCasting(active: false);
			targetPoint4 = isidora.transform.position + Vector3.down * 4f;
			isidora.Isidora.Audio.SetIsidoraVoice(on: false);
			ACT_MOVE.StartAction(isidora, targetPoint4, 4f, Ease.InOutCubic);
			yield return ACT_MOVE.waitForCompletion;
			FinishAction();
		}
	}

	public class HomingProjectilesAttack_EnemyAction : EnemyAction
	{
		private float horizontalSpacingFactor;

		private float verticalSpacingFactor;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT_AUX = new WaitSeconds_EnemyAction();

		private Teleport_EnemyAction ACT_TELEPORT = new Teleport_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST = new SingleBlastAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST2 = new SingleBlastAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST3 = new SingleBlastAttack_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, float horizontalSpacingFactor, float verticalSpacingFactor)
		{
			this.horizontalSpacingFactor = horizontalSpacingFactor;
			this.verticalSpacingFactor = verticalSpacingFactor;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			ACT_WAIT_AUX.StopAction();
			ACT_TELEPORT.StopAction();
			ACT_SINGLEBLAST.StopAction();
			ACT_SINGLEBLAST2.StopAction();
			ACT_SINGLEBLAST3.StopAction();
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			isidoraBehaviour.Isidora.AnimatorInyector.ResetAll();
			isidoraBehaviour.Isidora.Audio.SetIsidoraVoice(on: false);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			if (!o.IsAnimatorInState("OUT"))
			{
				o.Isidora.AnimatorInyector.SetHidden(hidden: true);
			}
			yield return new WaitUntilOut(o);
			Vector2 center = o.battleBounds.center + Vector2.right * 0.3f;
			o.transform.position = center - Vector2.up * 1.5f;
			int currentBar = o.Isidora.Audio.bossAudioSync.LastBar;
			int lastAttackMarkerBar = o.Isidora.Audio.lastAttackMarkerBar;
			if (currentBar != lastAttackMarkerBar)
			{
				o.Isidora.Audio.SetIsidoraVoice(on: true);
				float timeLeft = o.Isidora.Audio.GetTimeLeftForCurrentBar();
				if (timeLeft > 1f)
				{
					ACT_SINGLEBLAST.StartAction(o, center, 0f, timeLeft * 0.4f, timeLeft * 0.4f);
					ACT_SINGLEBLAST2.StartAction(o, center + Vector2.right * 3f, 0f, timeLeft * 0.4f, timeLeft * 0.4f);
					ACT_SINGLEBLAST3.StartAction(o, center + Vector2.left * 3f, 0f, timeLeft * 0.4f, timeLeft * 0.4f);
				}
				ACT_WAIT.StartAction(o, timeLeft * 0.9f);
				yield return ACT_WAIT.waitForCompletion;
			}
			else
			{
				yield return new WaitUntil(() => currentBar != o.Isidora.Audio.bossAudioSync.LastBar);
				center.y = o.battleBounds.yMin;
				ACT_SINGLEBLAST.StartAction(o, center, 0f);
				ACT_SINGLEBLAST2.StartAction(o, center + Vector2.right * 3f, 0f);
				ACT_SINGLEBLAST3.StartAction(o, center + Vector2.left * 3f, 0f);
				o.Isidora.Audio.SetIsidoraVoice(on: true);
				ACT_WAIT.StartAction(o, o.Isidora.Audio.GetSingleBarDuration() * 0.9f);
				yield return ACT_WAIT.waitForCompletion;
			}
			o.Isidora.AnimatorInyector.SetHidden(hidden: false);
			o.Isidora.AnimatorInyector.SetCasting(active: true);
			o.homingBonfireAttack.NumProjectiles = 1;
			o.homingBonfireAttack.HorizontalSpacingFactor = horizontalSpacingFactor;
			o.homingBonfireAttack.VerticalSpacingFactor = verticalSpacingFactor;
			int j = 4;
			float delay = o.Isidora.Audio.GetSingleBarDuration() * 0.4f;
			float intro = o.Isidora.Audio.GetSingleBarDuration() * 0.4f;
			Tweener t2 = ShortcutExtensions.DOMoveY(duration: intro + delay * (float)j + o.Isidora.Audio.GetSingleBarDuration() * 0.3f, target: o.transform, endValue: o.transform.position.y + 2f).SetEase(Ease.InOutCubic);
			ACT_WAIT.StartAction(o, intro);
			yield return ACT_WAIT.waitForCompletion;
			currentBar = o.Isidora.Audio.bossAudioSync.LastBar;
			for (int i = 0; i < j; i++)
			{
				o.homingBonfireAttack.FireProjectile();
				ACT_WAIT.StartAction(o, delay);
				yield return ACT_WAIT.waitForCompletion;
				if (currentBar != o.Isidora.Audio.bossAudioSync.LastBar)
				{
					o.Isidora.Audio.SetIsidoraVoice(on: false);
				}
			}
			o.Isidora.AnimatorInyector.SetCasting(active: false);
			t2 = o.transform.DOMoveY(o.transform.position.y - 3f, 1f).SetEase(Ease.InOutCubic);
			yield return t2.WaitForCompletion();
			FinishAction();
		}
	}

	public class BonfireProjectilesAttack_EnemyAction : EnemyAction
	{
		private float attackCooldown;

		private int numProjectiles;

		private bool useCastingPosition;

		private Vector2 castingPosition;

		private float horizontalSpacingFactor;

		private float verticalSpacingFactor;

		private float activeTime;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, float attackCooldown, int numProjectiles, bool useCastingPosition, Vector2 castingPosition, float horizontalSpacingFactor, float verticalSpacingFactor, float activeTime)
		{
			this.attackCooldown = attackCooldown;
			this.numProjectiles = numProjectiles;
			this.useCastingPosition = useCastingPosition;
			this.castingPosition = castingPosition;
			this.horizontalSpacingFactor = horizontalSpacingFactor;
			this.verticalSpacingFactor = verticalSpacingFactor;
			this.activeTime = activeTime;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			(owner as IsidoraBehaviour).Isidora.AnimatorInyector.ResetAll();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			ACT_WAIT.StartAction(o, o.Isidora.Audio.GetTimeUntilNextValidBar());
			yield return ACT_WAIT.waitForCompletion;
			o.homingBonfireBehavior.SetupAttack(attackCooldown, numProjectiles, useCastingPosition, castingPosition, horizontalSpacingFactor, verticalSpacingFactor);
			o.homingBonfireBehavior.ActivateBonfire(changeMask: true);
			if (activeTime < 0f)
			{
				o.homingBonfireBehavior.EnlargeMask();
				o.bonfireMaskIsEnlarged = true;
			}
			else
			{
				ACT_WAIT.StartAction(o, activeTime);
				yield return ACT_WAIT.waitForCompletion;
				o.homingBonfireBehavior.DeactivateBonfire(changeMask: true);
			}
			FinishAction();
		}
	}

	public class BonfireChargeIsidoraAttack_EnemyAction : EnemyAction
	{
		private Vector2 castingPosition;

		private float timeToMaxRate;

		private float activeTime;

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private BonfireProjectilesAttack_EnemyAction ACT_BONFIRE = new BonfireProjectilesAttack_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 castingPosition, float timeToMaxRate, float activeTime)
		{
			this.castingPosition = castingPosition;
			this.timeToMaxRate = timeToMaxRate;
			this.activeTime = activeTime;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_WAIT.StopAction();
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			isidoraBehaviour.Isidora.AnimatorInyector.ResetAll();
			isidoraBehaviour.homingBonfireBehavior.DeactivateBonfire(changeMask: false);
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			o.homingBonfireBehavior.SetupAttack(2, castingPosition, 0.5f, 0.5f);
			o.homingBonfireBehavior.StartChargingIsidora(timeToMaxRate);
			ACT_WAIT.StartAction(o, activeTime);
			yield return ACT_WAIT.waitForCompletion;
			o.homingBonfireBehavior.DeactivateBonfire(changeMask: false);
			for (int i = 0; i < 30; i++)
			{
				if (!o.homingBonfireBehavior.IsAnyProjectileVisible())
				{
					break;
				}
				ACT_WAIT.StartAction(o, 0.1f);
				yield return ACT_WAIT.waitForCompletion;
			}
			o.homingBonfireBehavior.IsChargingIsidora = false;
			o.homingBonfireBehavior.BonfireAttack.ChargesIsidora = false;
			o.homingBonfireBehavior.ActivateBonfire(changeMask: false);
			o.homingBonfireBehavior.SetupAttack(1, castingPosition, 0f, 0f);
			FinishAction();
		}
	}

	public class BlastsAttack_EnemyAction : EnemyAction
	{
		private Vector2 startingPos;

		private Vector2 direction;

		private float waitTime;

		private bool shouldVanish;

		private int totalAreas;

		private float distanceBetweenAreas;

		private Teleport_EnemyAction ACT_TELEPORT = new Teleport_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 startingPos, Vector2 direction, float waitTime, bool shouldVanish, int totalAreas = -1, float distanceBetweenAreas = -1f)
		{
			this.startingPos = startingPos;
			this.direction = direction;
			this.waitTime = waitTime;
			this.shouldVanish = shouldVanish;
			this.totalAreas = totalAreas;
			this.distanceBetweenAreas = distanceBetweenAreas;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_TELEPORT.StopAction();
			ACT_WAIT.StopAction();
			(owner as IsidoraBehaviour).Isidora.AnimatorInyector.ResetAll();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			if (shouldVanish)
			{
				o.Isidora.AnimatorInyector.SetHidden(hidden: true);
			}
			ACT_WAIT.StartAction(o, waitTime);
			int prevTotalAreas = o.blastAttack.totalAreas;
			float prevDistanceBetweenAreas = o.blastAttack.distanceBetweenAreas;
			o.blastAttack.totalAreas = ((totalAreas != -1) ? totalAreas : o.blastAttack.totalAreas);
			o.blastAttack.distanceBetweenAreas = ((distanceBetweenAreas != -1f) ? distanceBetweenAreas : o.blastAttack.distanceBetweenAreas);
			direction.y = 0f;
			o.blastAttack.SummonAreas(startingPos, direction, (!(direction.x > 0f)) ? EntityOrientation.Left : EntityOrientation.Right);
			o.blastAttack.totalAreas = prevTotalAreas;
			o.blastAttack.distanceBetweenAreas = prevDistanceBetweenAreas;
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	public class ChargedBlastsAttack_EnemyAction : EnemyAction
	{
		private Teleport_EnemyAction ACT_TELEPORT = new Teleport_EnemyAction();

		private MoveEasing_EnemyAction ACT_MOVE = new MoveEasing_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		private BlastsAttack_EnemyAction ACT_BLASTS = new BlastsAttack_EnemyAction();

		private BonfireChargeIsidoraAttack_EnemyAction ACT_BONFIRE = new BonfireChargeIsidoraAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST1 = new SingleBlastAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST2 = new SingleBlastAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST3 = new SingleBlastAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST4 = new SingleBlastAttack_EnemyAction();

		private SingleBlastAttack_EnemyAction ACT_SINGLEBLAST5 = new SingleBlastAttack_EnemyAction();

		private Tween xTween;

		private Tween yTween;

		protected override void DoOnStop()
		{
			ACT_TELEPORT.StopAction();
			ACT_MOVE.StopAction();
			ACT_WAIT.StopAction();
			ACT_BLASTS.StopAction();
			ACT_BONFIRE.StopAction();
			ACT_SINGLEBLAST1.StopAction();
			ACT_SINGLEBLAST2.StopAction();
			ACT_SINGLEBLAST3.StopAction();
			ACT_SINGLEBLAST4.StopAction();
			ACT_SINGLEBLAST5.StopAction();
			IsidoraBehaviour isidoraBehaviour = owner as IsidoraBehaviour;
			isidoraBehaviour.Isidora.AnimatorInyector.ResetAll();
			isidoraBehaviour.StopFlameParticles();
			if (xTween != null)
			{
				xTween.Kill();
				xTween = null;
			}
			if (yTween != null)
			{
				yTween.Kill();
				yTween = null;
			}
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			Gameplay.GameControllers.Penitent.Penitent p = Core.Logic.Penitent;
			IsidoraAnimatorInyector anim = o.Isidora.AnimatorInyector;
			o.numberOfCharges = 0;
			if (!o.IsAnimatorInState("OUT"))
			{
				anim.SetHidden(hidden: true);
			}
			yield return new WaitUntilOut(o);
			Vector2 castingPosBottom = ((!o.IsIsidoraOnTheRightSide()) ? (o.ArenaGetBotRightCorner() + Vector2.left) : (o.ArenaGetBotLeftCorner() + Vector2.right));
			o.transform.position = castingPosBottom;
			Vector2 blastsDir2 = o.battleBounds.center - castingPosBottom;
			blastsDir2.y = 0f;
			blastsDir2 = blastsDir2.normalized;
			o.LookAtDirUsingOrientation(blastsDir2);
			int currentBar = o.Isidora.Audio.bossAudioSync.LastBar;
			int lastAttackMarkerBar = o.Isidora.Audio.lastAttackMarkerBar;
			if (currentBar != lastAttackMarkerBar)
			{
				o.Isidora.Audio.SetIsidoraVoice(on: true);
				float timeLeft = o.Isidora.Audio.GetTimeLeftForCurrentBar();
				if (timeLeft > 2f)
				{
					ACT_SINGLEBLAST1.StartAction(o, castingPosBottom, 0f);
					ACT_SINGLEBLAST2.StartAction(o, castingPosBottom + blastsDir2 * 3f, 0f);
					ACT_SINGLEBLAST3.StartAction(o, castingPosBottom + blastsDir2 * 6f, 0f);
				}
				ACT_WAIT.StartAction(o, timeLeft * 0.9f);
				yield return ACT_WAIT.waitForCompletion;
			}
			else
			{
				yield return new WaitUntil(() => currentBar != o.Isidora.Audio.bossAudioSync.LastBar);
				o.Isidora.Audio.SetIsidoraVoice(on: true);
				ACT_SINGLEBLAST1.StartAction(o, castingPosBottom, 0f);
				ACT_SINGLEBLAST2.StartAction(o, castingPosBottom + blastsDir2 * 3f, 0f);
				ACT_SINGLEBLAST3.StartAction(o, castingPosBottom + blastsDir2 * 6f, 0f);
				ACT_WAIT.StartAction(o, o.Isidora.Audio.GetSingleBarDuration() * 0.9f);
				yield return ACT_WAIT.waitForCompletion;
			}
			anim.SetHidden(hidden: false);
			anim.SetCasting(active: true);
			float singleBar = o.Isidora.Audio.GetSingleBarDuration();
			Vector2 castingPosTop = (Vector2)o.transform.position + Vector2.up * 4f;
			ACT_MOVE.StartAction(o, castingPosTop, singleBar * 0.7f, Ease.InOutQuad);
			ACT_BONFIRE.StartAction(o, o.homingBonfireBehavior.transform.position, singleBar * 0.5f, singleBar * 0.7f);
			yield return ACT_BONFIRE.waitForCompletion;
			yield return ACT_MOVE.waitForCompletion;
			yield return new WaitUntilBarFinishes(o.Isidora.Audio);
			float bar = o.Isidora.Audio.GetSingleBarDuration();
			float totalTime = bar * 4f;
			Vector2 oppositePos = ((!(castingPosTop.x > o.battleBounds.center.x)) ? (castingPosTop + o.battleBounds.width * Vector2.right * 0.8f) : (castingPosTop + o.battleBounds.width * Vector2.left * 0.8f));
			Vector2 startingDir = (oppositePos - castingPosTop).normalized;
			xTween = o.transform.DOMoveX(oppositePos.x, totalTime * 0.4f);
			xTween.SetEase(Ease.InQuad);
			xTween.OnComplete(delegate
			{
				xTween = o.transform.DOMoveX(oppositePos.x + startingDir.x, totalTime * 0.05f);
				xTween.SetEase(Ease.Linear);
				xTween.OnComplete(delegate
				{
					xTween = o.transform.DOMoveX(oppositePos.x, totalTime * 0.05f);
					xTween.SetEase(Ease.Linear);
					xTween.OnComplete(delegate
					{
						xTween = o.transform.DOMoveX(castingPosTop.x, totalTime * 0.4f);
						xTween.SetEase(Ease.InQuad);
						xTween.OnComplete(delegate
						{
							xTween = o.transform.DOMoveX(castingPosTop.x - startingDir.x, totalTime * 0.05f);
							xTween.SetEase(Ease.Linear);
							xTween.OnComplete(delegate
							{
								xTween = o.transform.DOMoveX(castingPosTop.x, totalTime * 0.05f);
								xTween.SetEase(Ease.Linear);
							});
						});
					});
				});
			});
			yTween = o.transform.DOMoveY(castingPosTop.y - 2f, totalTime * 0.4f);
			yTween.SetEase(Ease.InOutQuad);
			yTween.OnComplete(delegate
			{
				yTween = o.transform.DOMoveY(castingPosTop.y, totalTime * 0.1f);
				yTween.SetEase(Ease.InOutQuad);
				yTween.OnComplete(delegate
				{
					yTween = o.transform.DOMoveY(castingPosTop.y - 2f, totalTime * 0.4f);
					yTween.SetEase(Ease.InOutQuad);
					yTween.OnComplete(delegate
					{
						yTween = o.transform.DOMoveY(castingPosTop.y, totalTime * 0.1f);
						yTween.SetEase(Ease.InOutQuad);
					});
				});
			});
			o.StopFlameParticles();
			for (int i = 0; i < 4; i++)
			{
				Vector2 startingPos = p.GetPosition();
				startingPos.y = o.battleBounds.yMin;
				ACT_SINGLEBLAST1.StartAction(o, startingPos, bar, bar * 0.75f, bar * 0.25f);
				if (o.numberOfCharges > 4)
				{
					ACT_SINGLEBLAST2.StartAction(o, startingPos + Vector2.right * 2f, bar, bar * 0.75f, bar * 0.25f, 0f, screenshake: true);
					ACT_SINGLEBLAST3.StartAction(o, startingPos + Vector2.left * 2f, bar, bar * 0.75f, bar * 0.25f);
				}
				if (o.numberOfCharges > 8)
				{
					ACT_SINGLEBLAST4.StartAction(o, startingPos + Vector2.right * 4f, bar, bar * 0.75f, bar * 0.25f, 0f, screenshake: true);
					ACT_SINGLEBLAST5.StartAction(o, startingPos + Vector2.left * 4f, bar, bar * 0.75f, bar * 0.25f);
				}
				yield return ACT_SINGLEBLAST1.waitForCompletion;
				if (i == 2)
				{
					o.Isidora.Audio.SetIsidoraVoice(on: false);
				}
			}
			o.StopFlameParticles();
			anim.SetCasting(active: false);
			ACT_MOVE.StartAction(o, castingPosBottom, 3f, Ease.InOutQuad);
			yield return ACT_MOVE.waitForCompletion;
			FinishAction();
		}
	}

	public class SingleBlastAttack_EnemyAction : EnemyAction
	{
		private Vector2 startingPos;

		private float waitAfterBlast;

		private float activeTime;

		private float anticipationTime;

		private float delay;

		private bool screenshake;

		private Teleport_EnemyAction ACT_TELEPORT = new Teleport_EnemyAction();

		private WaitSeconds_EnemyAction ACT_WAIT = new WaitSeconds_EnemyAction();

		public EnemyAction StartAction(EnemyBehaviour e, Vector2 startingPos, float waitAfterBlast, float anticipationTime = 1f, float activeTime = 1f, float delay = 0f, bool screenshake = false)
		{
			this.startingPos = startingPos;
			this.waitAfterBlast = waitAfterBlast;
			this.activeTime = activeTime;
			this.anticipationTime = anticipationTime;
			this.delay = delay;
			this.screenshake = screenshake;
			return StartAction(e);
		}

		protected override void DoOnStop()
		{
			ACT_TELEPORT.StopAction();
			ACT_WAIT.StopAction();
			(owner as IsidoraBehaviour).Isidora.AnimatorInyector.ResetAll();
			base.DoOnStop();
		}

		protected override IEnumerator BaseCoroutine()
		{
			IsidoraBehaviour o = owner as IsidoraBehaviour;
			if (delay > 0f)
			{
				ACT_WAIT.StartAction(o, delay);
				yield return ACT_WAIT.waitForCompletion;
			}
			GameObject b = o.blastAttack.SummonAreaOnPoint(startingPos);
			BossSpawnedAreaAttack area = b.GetComponent<BossSpawnedAreaAttack>();
			area.SetCustomTimes(anticipationTime, activeTime);
			if (screenshake)
			{
				o.BlastScreenshake();
			}
			ACT_WAIT.StartAction(o, waitAfterBlast);
			yield return ACT_WAIT.waitForCompletion;
			FinishAction();
		}
	}

	private class WaitUntilNextMarker : CustomYieldInstruction
	{
		private bool finished;

		public override bool keepWaiting => !finished;

		public WaitUntilNextMarker(IsidoraAudio isidoraAudio)
		{
			finished = false;
			isidoraAudio.OnNextMarker += IsidoraAudio_OnNextMarker;
		}

		private void IsidoraAudio_OnNextMarker(IsidoraAudio obj)
		{
			obj.OnNextMarker -= IsidoraAudio_OnNextMarker;
			finished = true;
		}
	}

	private class WaitUntilNextAttackMarker : CustomYieldInstruction
	{
		private bool finished;

		public override bool keepWaiting => !finished;

		public WaitUntilNextAttackMarker(IsidoraAudio isidoraAudio)
		{
			finished = false;
			isidoraAudio.OnAttackMarker += IsidoraAudio_OnNextAttackMarker;
		}

		private void IsidoraAudio_OnNextAttackMarker(IsidoraAudio obj)
		{
			obj.OnNextMarker -= IsidoraAudio_OnNextAttackMarker;
			finished = true;
		}
	}

	private class WaitUntilBarFinishes : CustomYieldInstruction
	{
		private bool finished;

		public override bool keepWaiting => !finished;

		public WaitUntilBarFinishes(IsidoraAudio isidoraAudio)
		{
			finished = false;
			isidoraAudio.OnBarBegins += IsidoraAudio_OnBarBegins;
		}

		private void IsidoraAudio_OnBarBegins(IsidoraAudio obj)
		{
			obj.OnBarBegins -= IsidoraAudio_OnBarBegins;
			finished = true;
		}
	}

	private class WaitUntilNextValidBar : CustomYieldInstruction
	{
		private bool finished;

		public override bool keepWaiting => !finished;

		public WaitUntilNextValidBar(IsidoraAudio isidoraAudio)
		{
			finished = false;
			isidoraAudio.OnBarBegins += IsidoraAudio_OnBarBegins;
		}

		private void IsidoraAudio_OnBarBegins(IsidoraAudio obj)
		{
			if (obj.IsLastBarValid())
			{
				obj.OnBarBegins -= IsidoraAudio_OnBarBegins;
				finished = true;
			}
		}
	}

	private class WaitUntilAnticipationPeriod : CustomYieldInstruction
	{
		private bool finished;

		public float targetTime;

		public override bool keepWaiting => !finished;

		public WaitUntilAnticipationPeriod(IsidoraAudio isidoraAudio)
		{
			finished = false;
			targetTime = isidoraAudio.GetTimeSinceLevelLoad() + isidoraAudio.GetTimeUntilNextAttackAnticipationPeriod();
			targetTime -= 0.1f;
			isidoraAudio.OnBarBegins += IsidoraAudio_OnBarBegins;
		}

		private void IsidoraAudio_OnBarBegins(IsidoraAudio obj)
		{
			if (obj.GetTimeSinceLevelLoad() >= targetTime)
			{
				obj.OnBarBegins -= IsidoraAudio_OnBarBegins;
				finished = true;
			}
		}
	}

	[FoldoutGroup("Battle area", 0)]
	public Rect battleBounds;

	[FoldoutGroup("Battle config", 0)]
	public List<IsidoraFightParameters> allFightParameters;

	[FoldoutGroup("Attacks config", 0)]
	public IsidoraScriptableConfig attackConfigData;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public IsidoraMeleeAttack preSlashAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public IsidoraMeleeAttack slashAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public IsidoraMeleeAttack preRisingAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public IsidoraMeleeAttack holdRisingAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public IsidoraMeleeAttack risingAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public IsidoraMeleeAttack twirlAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public IsidoraMeleeAttack fadeSlashAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public HomingBonfireAttack homingBonfireAttack;

	[FoldoutGroup("Attack references", 0)]
	[SerializeField]
	public BossAreaSummonAttack blastAttack;

	[FoldoutGroup("Death references", 0)]
	[SerializeField]
	public GameObject orbCollectible;

	[HideInInspector]
	public HomingBonfireBehaviour homingBonfireBehavior;

	[FoldoutGroup("VFX", 0)]
	public ParticleSystem floorSparksParticlesToRight;

	[FoldoutGroup("VFX", 0)]
	public ParticleSystem floorSparksParticlesToLeft;

	[FoldoutGroup("VFX", 0)]
	public ParticleSystem flameParticles;

	[FoldoutGroup("VFX", 0)]
	public ParticleSystem sparksParticles;

	[FoldoutGroup("VFX", 0)]
	public GameObject floorSparksMaskToRight;

	[FoldoutGroup("VFX", 0)]
	public GameObject floorSparksMaskToLeft;

	[FoldoutGroup("VFX", 0)]
	public GameObject singleSparkSimpleVFX;

	[FoldoutGroup("VFX", 0)]
	public GameObject attackAnticipationWarningSimpleVFX;

	[FoldoutGroup("VFX", 0)]
	public Vector2 singleSparkOffset;

	[FoldoutGroup("VFX", 0)]
	public GameObject slashLineSimpleVFX;

	[FoldoutGroup("VFX", 0)]
	public SpriteRenderer absorbSpriteRenderer;

	[FoldoutGroup("VFX", 0)]
	public GhostTrailGenerator ghostTrail;

	private List<ISIDORA_ATTACKS> availableAttacks = new List<ISIDORA_ATTACKS>();

	[ShowInInspector]
	private List<ISIDORA_ATTACKS> queuedAttacks = new List<ISIDORA_ATTACKS>();

	private IsidoraFightParameters currentFightParameters;

	private EnemyAction currentAction;

	private ISIDORA_ATTACKS lastAttack = ISIDORA_ATTACKS.DUMMY;

	private ISIDORA_ATTACKS secondLastAttack = ISIDORA_ATTACKS.DUMMY;

	private Dictionary<ISIDORA_ATTACKS, Func<EnemyAction>> actionsDictionary = new Dictionary<ISIDORA_ATTACKS, Func<EnemyAction>>();

	private float extraRecoverySeconds;

	private IsidoraMeleeAttack currentMeleeAttack;

	private bool bonfireMaskIsEnlarged;

	private bool orbSpawned;

	private Coroutine blinkCoroutine;

	private bool wasVoiceOn;

	private string debugMessage;

	[HideInInspector]
	public int numberOfCharges;

	private WaitSeconds_EnemyAction waitBetweenActions_EA;

	private AudioSyncTest_EnemyAction audioSync_EA;

	private AttackSyncTest_EnemyAction attackSync_EA;

	private FirstCombo_EnemyAction firstCombo_EA;

	private SecondCombo_EnemyAction secondCombo_EA;

	private ThirdCombo_EnemyAction thirdCombo_EA;

	private SyncCombo1_EnemyAction syncCombo1_EA;

	private FadeSlashCombo1_EnemyAction fadeSlashCombo1_EA;

	private PhaseSwitchAttack_EnemyAction phaseSwitch_EA;

	private HomingProjectilesAttack_EnemyAction homingProjectiles_EA;

	private BonfireProjectilesAttack_EnemyAction bonfireProjectiles_EA;

	private HorizontalDash_EnemyAction horizontalDash_EA;

	private BlastsAttack_EnemyAction blastsAttack_EA;

	private InvisibleHorizontalDash_EnemyAction invisibleHorizontalDash_EA;

	private ChargedBlastsAttack_EnemyAction chargedBlastsAttack_EA;

	private Intro_EnemyAction intro_EA;

	private Death_EnemyAction death_EA;

	private StateMachine<IsidoraBehaviour> _fsm;

	private State<IsidoraBehaviour> stIdle;

	private State<IsidoraBehaviour> stAction;

	public ISIDORA_PHASES currentPhase;

	public Isidora Isidora { get; set; }

	private void Start()
	{
		Isidora = (Isidora)Entity;
		InitAI();
		InitActionDictionary();
		homingBonfireBehavior = UnityEngine.Object.FindObjectOfType<HomingBonfireBehaviour>();
		currentFightParameters = allFightParameters[0];
		PoolManager.Instance.CreatePool(singleSparkSimpleVFX, 2);
		PoolManager.Instance.CreatePool(slashLineSimpleVFX, 2);
		PoolManager.Instance.CreatePool(attackAnticipationWarningSimpleVFX, 2);
	}

	private void OnGUI()
	{
	}

	public void ProjectileAbsortion(Vector2 projectilePosition, Vector2 projectileDirection)
	{
		if (!Isidora.Status.Dead)
		{
			base.transform.DOPunchPosition(projectileDirection.normalized * 0.1f, 0.2f);
			numberOfCharges++;
			PlayFlameParticles();
			if (blinkCoroutine != null)
			{
				StopCoroutine(blinkCoroutine);
			}
			blinkCoroutine = StartCoroutine(BlinkAbsortion(0.1f, 3));
		}
	}

	private IEnumerator BlinkAbsortion(float delay, int blinks)
	{
		absorbSpriteRenderer.flipX = Isidora.SpriteRenderer.flipX;
		for (int i = 0; i < blinks; i++)
		{
			absorbSpriteRenderer.enabled = true;
			yield return new WaitForSeconds(delay);
			absorbSpriteRenderer.enabled = false;
		}
	}

	private void DrawMusicBars()
	{
		int lastBar = Isidora.Audio.bossAudioSync.LastBar;
		string text = "red";
		int lastAttackMarkerBar = Isidora.Audio.lastAttackMarkerBar;
		if (lastAttackMarkerBar == lastBar)
		{
			text = "green";
		}
		string text2 = ((!Isidora.Audio.GetIsidoraVoice()) ? "..............................................." : "♫♪♫♪♫♪♫♪♫♪♫♪♫♪♫♪♫♪♫♪♫♪♫♪♫♪♫♪♫♪♫♪");
		string text3 = "<color=" + text + ">" + lastBar + "</color>" + text2 + (lastBar + 1);
		GUI.Label(new Rect(800f, 30f, 300f, 20f), text3);
		float timeLeftForCurrentBar = Isidora.Audio.GetTimeLeftForCurrentBar();
		float singleBarDuration = Isidora.Audio.GetSingleBarDuration();
		GUI.HorizontalSlider(new Rect(800f, 50f, 200f, 20f), singleBarDuration - timeLeftForCurrentBar, 0f, singleBarDuration);
	}

	private void InitAI()
	{
		stIdle = new Isidora_StIdle();
		stAction = new Isidora_StAction();
		_fsm = new StateMachine<IsidoraBehaviour>(this, stIdle);
	}

	private void InitActionDictionary()
	{
		waitBetweenActions_EA = new WaitSeconds_EnemyAction();
		intro_EA = new Intro_EnemyAction();
		death_EA = new Death_EnemyAction();
		audioSync_EA = new AudioSyncTest_EnemyAction();
		attackSync_EA = new AttackSyncTest_EnemyAction();
		firstCombo_EA = new FirstCombo_EnemyAction();
		secondCombo_EA = new SecondCombo_EnemyAction();
		thirdCombo_EA = new ThirdCombo_EnemyAction();
		syncCombo1_EA = new SyncCombo1_EnemyAction();
		fadeSlashCombo1_EA = new FadeSlashCombo1_EnemyAction();
		phaseSwitch_EA = new PhaseSwitchAttack_EnemyAction();
		homingProjectiles_EA = new HomingProjectilesAttack_EnemyAction();
		bonfireProjectiles_EA = new BonfireProjectilesAttack_EnemyAction();
		horizontalDash_EA = new HorizontalDash_EnemyAction();
		blastsAttack_EA = new BlastsAttack_EnemyAction();
		invisibleHorizontalDash_EA = new InvisibleHorizontalDash_EnemyAction();
		chargedBlastsAttack_EA = new ChargedBlastsAttack_EnemyAction();
		actionsDictionary.Add(ISIDORA_ATTACKS.AUDIO_TEST, LaunchAction_AudioTest);
		actionsDictionary.Add(ISIDORA_ATTACKS.ATTACK_TEST, LaunchAction_AttackTest);
		actionsDictionary.Add(ISIDORA_ATTACKS.FIRST_COMBO, LaunchAction_FirstCombo);
		actionsDictionary.Add(ISIDORA_ATTACKS.SECOND_COMBO, LaunchAction_SecondCombo);
		actionsDictionary.Add(ISIDORA_ATTACKS.THIRD_COMBO, LaunchAction_ThirdCombo);
		actionsDictionary.Add(ISIDORA_ATTACKS.SYNC_COMBO_1, LaunchAction_FadeSlashCombo1);
		actionsDictionary.Add(ISIDORA_ATTACKS.PHASE_SWITCH_ATTACK, LaunchAction_PhaseSwitch);
		actionsDictionary.Add(ISIDORA_ATTACKS.HOMING_PROJECTILES_ATTACK, LaunchAction_HomingProjectiles);
		actionsDictionary.Add(ISIDORA_ATTACKS.BONFIRE_PROJECTILES_ATTACK, LaunchAction_BonfireProjectiles);
		actionsDictionary.Add(ISIDORA_ATTACKS.BONFIRE_INFINITE_PROJECTILES_ATTACK, LaunchAction_BonfireInfiniteProjectiles);
		actionsDictionary.Add(ISIDORA_ATTACKS.HORIZONTAL_DASH, LaunchAction_HorizontalDash);
		actionsDictionary.Add(ISIDORA_ATTACKS.BLASTS_ATTACK, LaunchAction_BlastsAttack);
		actionsDictionary.Add(ISIDORA_ATTACKS.INVISIBLE_HORIZONTAL_DASH, LaunchAction_InvisibleHorizontalDash);
		actionsDictionary.Add(ISIDORA_ATTACKS.CHARGED_BLASTS_ATTACK, LaunchAction_ChargedBlastsAttack);
		availableAttacks = attackConfigData.GetAttackIds(onlyActive: true, useHP: true, 1f);
	}

	public void Damage(Hit hit)
	{
		if (SwapFightParametersIfNeeded())
		{
			CheckCurrentPhase();
		}
	}

	private void CheckCurrentPhase()
	{
		if (queuedAttacks.Contains(ISIDORA_ATTACKS.PHASE_SWITCH_ATTACK) || !currentFightParameters.advancePhase)
		{
			return;
		}
		if (currentPhase == ISIDORA_PHASES.FIRST)
		{
			currentPhase = ISIDORA_PHASES.BRIDGE;
			Isidora.Audio.SetPhaseBridge();
		}
		else
		{
			if (currentPhase != ISIDORA_PHASES.BRIDGE)
			{
				return;
			}
			currentPhase = ISIDORA_PHASES.SECOND;
			QueueAttack(ISIDORA_ATTACKS.PHASE_SWITCH_ATTACK);
			if (lastAttack == ISIDORA_ATTACKS.INVISIBLE_HORIZONTAL_DASH)
			{
				QueueAttack(ISIDORA_ATTACKS.HORIZONTAL_DASH);
			}
			else
			{
				QueueAttack(ISIDORA_ATTACKS.INVISIBLE_HORIZONTAL_DASH);
			}
			if (Isidora.Audio.currentAudioPhase != ISIDORA_PHASES.BRIDGE)
			{
				if (Isidora.Audio.GetTimeUntilNextAttackAnticipationPeriod() > 0.3f && Isidora.Audio.GetTimeUntilNextAttackAnticipationPeriod() < 1.2f)
				{
					QueueAttack(ISIDORA_ATTACKS.HOMING_PROJECTILES_ATTACK);
				}
				else
				{
					QueueAttack(ISIDORA_ATTACKS.SECOND_COMBO);
				}
				QueueAttack(ISIDORA_ATTACKS.FIRST_COMBO);
			}
		}
	}

	private void QueueAttack(ISIDORA_ATTACKS atk)
	{
		queuedAttacks.Add(atk);
	}

	private ISIDORA_ATTACKS PopAttackFromQueue()
	{
		ISIDORA_ATTACKS result = ISIDORA_ATTACKS.ATTACK_TEST;
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
		float hpPercentage = Isidora.GetHpPercentage();
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

	private Vector2 GetDirToPenitent()
	{
		return (Vector2)Core.Logic.Penitent.transform.position - (Vector2)base.transform.position;
	}

	private float GetDirFromOrientation()
	{
		return (Isidora.Status.Orientation != 0) ? (-1f) : 1f;
	}

	private void LookAtDirUsingOrientation(Vector2 v)
	{
		Isidora.SetOrientation((!(v.x > 0f)) ? EntityOrientation.Left : EntityOrientation.Right);
	}

	public void LookAtTarget()
	{
		LookAtDirUsingOrientation(GetDirToPenitent());
	}

	public bool IsIsidoraOnTheRightSide()
	{
		return Isidora.transform.position.x > battleBounds.center.x;
	}

	public Vector2 ArenaGetBotRightCorner()
	{
		return new Vector2(battleBounds.xMax, battleBounds.yMin);
	}

	public Vector2 ArenaGetBotLeftCorner()
	{
		return new Vector2(battleBounds.xMin, battleBounds.yMin);
	}

	public Vector2 ArenaGetBotFarRandomPoint()
	{
		Vector2 zero = Vector2.zero;
		zero.y = battleBounds.yMin;
		if (IsIsidoraOnTheRightSide())
		{
			zero.x = UnityEngine.Random.Range(Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.1f), Mathf.Lerp(battleBounds.xMin, battleBounds.xMax, 0.4f));
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
		if (IsIsidoraOnTheRightSide())
		{
			zero.x = UnityEngine.Random.Range(battleBounds.center.x, battleBounds.xMax);
		}
		else
		{
			zero.x = UnityEngine.Random.Range(battleBounds.xMin, battleBounds.center.x);
		}
		return zero;
	}

	public IEnumerator DelayedVoiceActivation(IsidoraAudio audio, bool active, float delay)
	{
		yield return new WaitForSeconds(delay);
		audio.SetIsidoraVoice(active);
	}

	public void SetWeapon(ISIDORA_WEAPONS weapon)
	{
		if ((bool)currentMeleeAttack)
		{
			OnMeleeAttackFinished();
		}
		switch (weapon)
		{
		case ISIDORA_WEAPONS.PRE_SLASH:
			currentMeleeAttack = preSlashAttack;
			break;
		case ISIDORA_WEAPONS.SLASH:
			currentMeleeAttack = slashAttack;
			break;
		case ISIDORA_WEAPONS.PRE_RISING_SLASH:
			currentMeleeAttack = preRisingAttack;
			break;
		case ISIDORA_WEAPONS.HOLD_RISING_SLASH:
			currentMeleeAttack = holdRisingAttack;
			break;
		case ISIDORA_WEAPONS.RISING_SLASH:
			currentMeleeAttack = risingAttack;
			break;
		case ISIDORA_WEAPONS.TWIRL:
			currentMeleeAttack = twirlAttack;
			break;
		case ISIDORA_WEAPONS.FADE_SLASH:
			currentMeleeAttack = fadeSlashAttack;
			break;
		}
		IsidoraMeleeAttack isidoraMeleeAttack = currentMeleeAttack;
		isidoraMeleeAttack.OnAttackGuarded = (Core.SimpleEvent)Delegate.Remove(isidoraMeleeAttack.OnAttackGuarded, new Core.SimpleEvent(Isidora.Audio.StopMeleeAudios));
		IsidoraMeleeAttack isidoraMeleeAttack2 = currentMeleeAttack;
		isidoraMeleeAttack2.OnAttackGuarded = (Core.SimpleEvent)Delegate.Combine(isidoraMeleeAttack2.OnAttackGuarded, new Core.SimpleEvent(Isidora.Audio.StopMeleeAudios));
	}

	public void FlipCurrentWeaponCollider()
	{
		Vector3 localScale = currentMeleeAttack.transform.localScale;
		localScale.x *= -1f;
		currentMeleeAttack.transform.localScale = localScale;
	}

	public void SpawnOrb()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(orbCollectible, base.transform.position + Vector3.down * 0.7f, base.transform.rotation);
		SpriteRenderer componentInChildren = gameObject.GetComponentInChildren<SpriteRenderer>();
		componentInChildren.flipX = Isidora.SpriteRenderer.flipX;
		orbSpawned = true;
	}

	private void Update()
	{
		_fsm.DoUpdate();
	}

	private void LaunchAutomaticAction()
	{
		ISIDORA_ATTACKS iSIDORA_ATTACKS = ISIDORA_ATTACKS.DUMMY;
		List<ISIDORA_ATTACKS> filteredAttacks = GetFilteredAttacks(availableAttacks);
		if (queuedAttacks.Count > 0)
		{
			iSIDORA_ATTACKS = PopAttackFromQueue();
		}
		else if (filteredAttacks.Count > 0)
		{
			int index = RandomizeUsingWeights(filteredAttacks);
			iSIDORA_ATTACKS = filteredAttacks[index];
		}
		else
		{
			iSIDORA_ATTACKS = ISIDORA_ATTACKS.THIRD_COMBO;
		}
		LaunchAction(iSIDORA_ATTACKS);
		secondLastAttack = lastAttack;
		lastAttack = iSIDORA_ATTACKS;
	}

	private List<ISIDORA_ATTACKS> GetFilteredAttacks(List<ISIDORA_ATTACKS> originalList)
	{
		List<ISIDORA_ATTACKS> list = new List<ISIDORA_ATTACKS>(originalList);
		IsidoraScriptableConfig.IsidoraAttackConfig atkConfig = attackConfigData.GetAttackConfig(lastAttack);
		if (atkConfig.cantBeFollowedBy != null && atkConfig.cantBeFollowedBy.Count > 0)
		{
			list.RemoveAll((ISIDORA_ATTACKS x) => atkConfig.cantBeFollowedBy.Contains(x));
		}
		if (atkConfig.alwaysFollowedBy != null && atkConfig.alwaysFollowedBy.Count > 0)
		{
			list.RemoveAll((ISIDORA_ATTACKS x) => !atkConfig.alwaysFollowedBy.Contains(x));
		}
		if (bonfireMaskIsEnlarged)
		{
			list.Remove(ISIDORA_ATTACKS.BONFIRE_PROJECTILES_ATTACK);
			list.Remove(ISIDORA_ATTACKS.INVISIBLE_HORIZONTAL_DASH);
			list.Remove(ISIDORA_ATTACKS.BLASTS_ATTACK);
			list.Remove(ISIDORA_ATTACKS.SYNC_COMBO_1);
			list.Remove(ISIDORA_ATTACKS.HOMING_PROJECTILES_ATTACK);
		}
		else
		{
			list.Remove(ISIDORA_ATTACKS.CHARGED_BLASTS_ATTACK);
		}
		if (currentPhase != 0)
		{
			int lastBar = Isidora.Audio.bossAudioSync.LastBar;
			int lastAttackMarkerBar = Isidora.Audio.lastAttackMarkerBar;
			if (lastBar != lastAttackMarkerBar && lastAttackMarkerBar != lastBar - 1)
			{
				list.Remove(ISIDORA_ATTACKS.HOMING_PROJECTILES_ATTACK);
				list.Remove(ISIDORA_ATTACKS.CHARGED_BLASTS_ATTACK);
			}
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

	private int RandomizeUsingWeights(List<ISIDORA_ATTACKS> filteredAtks)
	{
		float hpPercentage = Isidora.GetHpPercentage();
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

	protected void LaunchAction(ISIDORA_ATTACKS action)
	{
		StopCurrentAction();
		_fsm.ChangeState(stAction);
		currentAction = actionsDictionary[action]();
		currentAction.OnActionEnds += CurrentAction_OnActionEnds;
		currentAction.OnActionIsStopped += CurrentAction_OnActionStops;
	}

	protected EnemyAction LaunchAction_AudioTest()
	{
		return audioSync_EA.StartAction(this);
	}

	public void StartIntro()
	{
		StopCurrentAction();
		_fsm.ChangeState(stAction);
		currentAction = intro_EA.StartAction(this);
		currentAction.OnActionEnds += CurrentAction_OnActionEnds;
		currentAction.OnActionIsStopped += CurrentAction_OnActionStops;
	}

	protected EnemyAction LaunchAction_Death()
	{
		return death_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_AttackTest()
	{
		return attackSync_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_FirstCombo()
	{
		return firstCombo_EA.StartAction(this, 6f, 6f, 0.6f, 0.75f, 0.75f, 0.4f, 4f, 1.4f, 3f);
	}

	protected EnemyAction LaunchAction_SecondCombo()
	{
		return secondCombo_EA.StartAction(this, 6f, 6f, 0.6f, 0.3f, 0.75f, 1.25f, 0.4f, 2.5f);
	}

	protected EnemyAction LaunchAction_ThirdCombo()
	{
		return thirdCombo_EA.StartAction(this, 6f, 6f, 0.6f, 0.75f, 0.75f, 0.4f, 3f, 0.8f, 2.5f);
	}

	protected EnemyAction LaunchAction_SyncCombo1()
	{
		return syncCombo1_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_FadeSlashCombo1()
	{
		return fadeSlashCombo1_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_PhaseSwitch()
	{
		return phaseSwitch_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_HorizontalDash()
	{
		float anticipationBeforeDash = attackConfigData.horDashConfig.anticipationBeforeDash;
		float dashDuration = attackConfigData.horDashConfig.dashDuration;
		float shoryukenDuration = attackConfigData.horDashConfig.shoryukenDuration;
		float floatingDownDuration = attackConfigData.horDashConfig.floatingDownDuration;
		return horizontalDash_EA.StartAction(this, anticipationBeforeDash, dashDuration, shoryukenDuration, floatingDownDuration);
	}

	protected EnemyAction LaunchAction_InvisibleHorizontalDash()
	{
		float anticipationBeforeDash = attackConfigData.invisibleHorDashConfig.anticipationBeforeDash;
		float dashDuration = attackConfigData.invisibleHorDashConfig.dashDuration;
		float shoryukenDuration = attackConfigData.invisibleHorDashConfig.shoryukenDuration;
		float floatingDownDuration = attackConfigData.invisibleHorDashConfig.floatingDownDuration;
		return invisibleHorizontalDash_EA.StartAction(this, anticipationBeforeDash, dashDuration, shoryukenDuration, floatingDownDuration);
	}

	protected EnemyAction LaunchAction_BlastsAttack()
	{
		Vector2 startingPos = ((!IsIsidoraOnTheRightSide()) ? ArenaGetBotLeftCorner() : ArenaGetBotRightCorner());
		Vector2 direction = ((!IsIsidoraOnTheRightSide()) ? Vector2.right : Vector2.left);
		float waitTime = Mathf.Clamp(Isidora.Audio.GetTimeLeftForCurrentBar(), 2.5f, Isidora.Audio.GetSingleBarDuration());
		int attackRepetitions = attackConfigData.GetAttackRepetitions(ISIDORA_ATTACKS.BLASTS_ATTACK, useHP: true, Isidora.GetHpPercentage());
		float distanceBetweenAreas = Mathf.Lerp(4f, 3f, Mathf.Clamp01(attackRepetitions - 5));
		return blastsAttack_EA.StartAction(this, startingPos, direction, waitTime, shouldVanish: true, attackRepetitions, distanceBetweenAreas);
	}

	protected EnemyAction LaunchAction_ChargedBlastsAttack()
	{
		return chargedBlastsAttack_EA.StartAction(this);
	}

	protected EnemyAction LaunchAction_HomingProjectiles()
	{
		return homingProjectiles_EA.StartAction(this, 1f, 3f);
	}

	protected EnemyAction LaunchAction_BonfireProjectiles()
	{
		return bonfireProjectiles_EA.StartAction(this, Isidora.Audio.GetSingleBarDuration() / 4f, 2, useCastingPosition: false, Vector2.zero, 1f, 1f, 2f);
	}

	protected EnemyAction LaunchAction_BonfireInfiniteProjectiles()
	{
		Vector2 castingPosition = homingBonfireBehavior.gameObject.transform.position;
		return bonfireProjectiles_EA.StartAction(this, Isidora.Audio.GetSingleBarDuration(), 1, useCastingPosition: true, castingPosition, 0f, 0f, -1f);
	}

	private void CurrentAction_OnActionStops(EnemyAction e)
	{
	}

	private void CurrentAction_OnActionEnds(EnemyAction e)
	{
		e.OnActionEnds -= CurrentAction_OnActionEnds;
		e.OnActionIsStopped -= CurrentAction_OnActionStops;
		if (e == intro_EA)
		{
			QueueAttack(ISIDORA_ATTACKS.INVISIBLE_HORIZONTAL_DASH);
			LaunchAutomaticAction();
		}
		else if (e != waitBetweenActions_EA)
		{
			if (currentFightParameters.waitsInVanish)
			{
				Isidora.AnimatorInyector.SetTwirl(twirl: false);
				Isidora.AnimatorInyector.SetHidden(hidden: true);
				extraRecoverySeconds += Isidora.AnimatorInyector.GetVanishAnimationDuration() + 0.2f;
				if (IsAnimatorInState("SLASH") || IsAnimatorInState("CASTING") || IsAnimatorInState("TWIRL") || IsAnimatorInState("OUT"))
				{
					extraRecoverySeconds += 0.5f;
				}
			}
			else
			{
				LookAtTarget();
			}
			WaitBetweenActions();
		}
		else
		{
			if (CheckToAdvancePhase())
			{
				currentPhase = ISIDORA_PHASES.SECOND;
				QueueAttack(ISIDORA_ATTACKS.PHASE_SWITCH_ATTACK);
			}
			LaunchAutomaticAction();
		}
	}

	private bool CheckToAdvancePhase()
	{
		return currentPhase == ISIDORA_PHASES.BRIDGE && Isidora.Audio.currentAudioPhase == ISIDORA_PHASES.BRIDGE && !queuedAttacks.Contains(ISIDORA_ATTACKS.PHASE_SWITCH_ATTACK);
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
		currentAction.OnActionEnds += CurrentAction_OnActionEnds;
	}

	public void LinearScreenshake()
	{
		Vector2 vector = GetDirFromOrientation() * Vector2.right;
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.2f, vector * 1.5f, 10, 0.2f, 0.01f, default(Vector3), 0.01f, ignoreTimeScale: true);
	}

	public void BlastScreenshake()
	{
		Vector2 down = Vector2.down;
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.2f, down * 1.5f, 12, 0.2f, 0.01f, default(Vector3), 0.01f, ignoreTimeScale: true);
	}

	public void VoiceShockwave()
	{
		StartCoroutine(SingShockwave());
	}

	private IEnumerator SingShockwave()
	{
		yield return new WaitForSeconds(0.4f);
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position + Vector3.up * 2.15f, 0.3f, 0.1f, 0.4f);
		yield return new WaitForSeconds(0.4f);
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position + Vector3.up * 2.15f, 0.3f, 0.1f, 0.7f);
		yield return new WaitForSeconds(0.4f);
		Core.Logic.CameraManager.ShockwaveManager.Shockwave(base.transform.position + Vector3.up * 2.15f, 0.7f, 0.1f, 0.9f);
	}

	public void PlayFlameParticles()
	{
		if (flameParticles.isPlaying)
		{
			ParticleSystem.EmissionModule emission = flameParticles.emission;
			emission.rateOverTime = 10 * numberOfCharges;
			ParticleSystem.EmissionModule emission2 = sparksParticles.emission;
			emission2.rateOverTime = 5 * numberOfCharges;
		}
		else
		{
			flameParticles.Play();
			sparksParticles.Play();
		}
	}

	public void StopFlameParticles()
	{
		flameParticles.Stop();
		sparksParticles.Stop();
	}

	private void CheckDebugActions()
	{
		Dictionary<KeyCode, ISIDORA_ATTACKS> debugActions = attackConfigData.debugActions;
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

	public bool IsAnimatorInState(string state)
	{
		return Isidora.AnimatorInyector.EntityAnimator.GetCurrentAnimatorStateInfo(0).IsName(state);
	}

	private Vector2 ClampInsideBoundaries(Vector2 point, bool clampX = true, bool clampY = false)
	{
		if (clampX)
		{
			point.x = Mathf.Clamp(point.x, battleBounds.xMin, battleBounds.xMax);
		}
		if (clampY)
		{
			point.y = Mathf.Clamp(point.y, battleBounds.yMin, battleBounds.yMax);
		}
		return point;
	}

	private void SlashLineVFX()
	{
		Vector2 vector = base.transform.position;
		GameObject gameObject = PoolManager.Instance.ReuseObject(slashLineSimpleVFX, vector, Quaternion.identity).GameObject;
		gameObject.transform.localScale = new Vector3(GetDirFromOrientation(), 1f, 1f);
	}

	private void SingleSparkVFX(float yOffset = 0f)
	{
		Vector2 vector = new Vector2(base.transform.position.x + singleSparkOffset.x * GetDirFromOrientation(), base.transform.position.y + singleSparkOffset.y + yOffset);
		PoolManager.Instance.ReuseObject(singleSparkSimpleVFX, vector, Quaternion.identity);
	}

	private void WarningVFX(Vector2 offset)
	{
		Vector2 vector = (Vector2)base.transform.position + offset;
		PoolManager.Instance.ReuseObject(attackAnticipationWarningSimpleVFX, vector, Quaternion.identity);
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
		Isidora.DamageArea.DamageAreaCollider.enabled = b;
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
		PlayMakerFSM.BroadcastEvent("BOSS DEAD");
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
