using System.Collections;
using UnityEngine;

public class GameStepDecideCombatVictor : AGameStepCombat
{
	private AttackResultType? _combatResult;

	private GameStep _CreateAttackMedia(ACombatant actor, ACombatant target, bool createImpactMedia = true)
	{
		int cardCount = ((actor == base.attacker) ? base.attacker.GetAttackHand(base.defender).type.Size() : (base.defender.GetDefenseHand(base.attacker) ?? base.attacker.GetAttackHand(base.defender)).type.Size());
		VoiceManager.Instance.Play(actor.view.transform, actor.audio.Attack(cardCount), actor == base.attacker, 0.1f);
		actor.view.velocities.position += (target.view.transform.position - actor.view.transform.position).Project(AxisType.Y).Unproject(AxisType.Y).NormalizeSafe(-Vector3.forward) * 0.5f;
		return AppendStep(new GameStepProjectileMedia(actor.combatMedia.Attack(cardCount), new ActionContext(actor, null, target), createImpactMedia));
	}

	private void _CalculateCombatResult(bool signal = false)
	{
		if (base.combat.resultIsFinal)
		{
			return;
		}
		PokerHand attackHand = base.attacker.GetAttackHand(base.defender);
		PokerHand defenseHand = base.defender.GetDefenseHand(base.attacker);
		if (!base.combat.resultOverride.HasValue)
		{
			base.combat.result = attackHand.GetAttackResultType(defenseHand);
			if (base.combat.result == AttackResultType.Tie)
			{
				switch (base.defender.GetDefenseTieBreaker(base.attacker))
				{
				case 1:
					base.combat.result = AttackResultType.Failure;
					break;
				case -1:
					base.combat.result = AttackResultType.Success;
					break;
				}
			}
		}
		base.combat.damage = ((base.combat.result == AttackResultType.Success) ? attackHand.type.Size() : (defenseHand ?? attackHand).type.Size());
		if (signal)
		{
			AttackResultType? combatResult = _combatResult;
			if (SetPropertyUtility.SetStruct(ref _combatResult, base.combat.result))
			{
				base.state.SignalCombatVictorDecided(combatResult);
			}
		}
	}

	private void _OnAttackerResourceTransfer(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		if (oldPile == ResourceCard.Pile.AttackHand || newPile == ResourceCard.Pile.AttackHand)
		{
			_CalculateCombatResult();
		}
	}

	private void _OnDefenderResourceTransfer(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		if (oldPile == ResourceCard.Pile.DefenseHand || newPile == ResourceCard.Pile.DefenseHand)
		{
			_CalculateCombatResult();
		}
	}

	private void _OnWildValueChange(ResourceCard card, ResourceCard.WildContext wildContext)
	{
		if (((card.faction == Faction.Enemy) ? (ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand) : (ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand)).Contains(card.pile))
		{
			_CalculateCombatResult();
		}
	}

	private void _ClearCombatResultChangingEvents()
	{
		base.attacker.resourceDeck.onTransfer -= _OnAttackerResourceTransfer;
		base.defender.resourceDeck.onTransfer -= _OnDefenderResourceTransfer;
		base.state.onWildValueChanged -= _OnWildValueChange;
	}

	protected override void OnFirstEnabled()
	{
		base.attacker.resourceDeck.onTransfer += _OnAttackerResourceTransfer;
		base.defender.resourceDeck.onTransfer += _OnDefenderResourceTransfer;
		base.state.onWildValueChanged += _OnWildValueChange;
	}

	protected override void OnEnable()
	{
		_CalculateCombatResult(signal: true);
	}

	public override void Start()
	{
		if (base.combat.playerAttackWildedAfterLaunch || base.combat.enemyDefenseWildedAfterLaunch)
		{
			AppendStep(new GameStepWildCombatHands(base.state.player, base.combat.playerAttackWildedAfterLaunch, base.combat.enemyDefenseWildedAfterLaunch));
		}
	}

	protected override IEnumerator Update()
	{
		_ClearCombatResultChangingEvents();
		base.view.ClearPersistentAttackGlowsAndLines();
		base.combat.resultIsFinal = true;
		base.state.SignalFinalCombatVictorDecided();
		if (!base.isActiveStep)
		{
			yield return null;
		}
		if (!base.combat.canceled)
		{
			yield return _CreateAttackMedia(base.attacker, base.defender, base.combat.result == AttackResultType.Success);
		}
		if (base.combat.result != AttackResultType.Success)
		{
			IEnumerator wait = base.defender.PlayDefenseMedia();
			while (wait.MoveNext())
			{
				yield return null;
			}
		}
		if (base.combat.result == AttackResultType.Failure)
		{
			if ((bool)base.attacker.statuses.safeAttack)
			{
				base.attacker.HighlightStatusTrait(StatusType.SafeAttack);
				yield return AppendStep(new GameStepProjectileMedia(ContentRef.Defaults.media.safeAttack, base.combat.context));
			}
			else
			{
				yield return _CreateAttackMedia(base.defender, base.attacker);
			}
		}
	}

	protected override void OnCanceled()
	{
		_ClearCombatResultChangingEvents();
	}

	public override void OnCompletedSuccessfully()
	{
		AppendStep(new GameStepDealCombatDamage());
	}
}
