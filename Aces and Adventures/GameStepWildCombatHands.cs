using System.Collections;
using System.Linq;

public class GameStepWildCombatHands : AGameStepTurn
{
	private bool _wildPlayerAttackHand;

	private bool _wildEnemyDefenseHand;

	protected override bool shouldBeCanceled => base.state.combatShouldBeCanceled;

	protected override Stone.Pile? _enabledTurnStonePile => Stone.Pile.PlayerReaction;

	protected override Stone.Pile? _disabledTurnStonePile => Stone.Pile.TurnInactive;

	public GameStepWildCombatHands(Player player, bool wildPlayerAttackHand, bool wildEnemyDefenseHand)
		: base(player)
	{
		_wildPlayerAttackHand = wildPlayerAttackHand;
		_wildEnemyDefenseHand = wildEnemyDefenseHand;
	}

	private void _UpdateGlows()
	{
		base.stoneDeckLayout[StoneType.Turn].view.RequestGlow(this, Colors.TARGET);
	}

	private bool _IsValidTarget(ATarget target)
	{
		if (target is ACombatant aCombatant)
		{
			return aCombatant.activeCombatType.HasValue;
		}
		if (target is ResourceCard resourceCard)
		{
			if (!_wildEnemyDefenseHand || resourceCard.faction != Faction.Enemy || !(ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Contains(resourceCard.pile))
			{
				if (_wildPlayerAttackHand && resourceCard.faction == Faction.Player)
				{
					return resourceCard.pile == ResourceCard.Pile.AttackHand;
				}
				return false;
			}
			return true;
		}
		if (!(target is Stone stone) || stone.type != 0)
		{
			if (target is Chip chip)
			{
				return chip.pile == Chip.Pile.ActiveAttack;
			}
			return false;
		}
		return true;
	}

	private void _OnClick(ADeckLayoutBase deckLayout, int pile, ATarget target)
	{
		if (_IsValidTarget(target))
		{
			base.finished = true;
		}
	}

	private void _OnPointerEnter(ADeckLayoutBase deckLayout, int pile, ATarget target)
	{
		if (_IsValidTarget(target))
		{
			InputManager.I.RequestCursorOverride(this, SpecialCursorImage.CursorAttack);
		}
	}

	private void _OnPointerExit(ADeckLayoutBase deckLayout, int pile, ATarget target)
	{
		if (_IsValidTarget(target))
		{
			InputManager.I.ReleaseCursorOverride(this);
		}
	}

	protected override void _OnStoneClick(Stone.Pile pile, Stone card)
	{
		if (pile == Stone.Pile.PlayerReaction)
		{
			base.finished = true;
		}
	}

	protected override void OnFirstEnabled()
	{
		if (_wildPlayerAttackHand)
		{
			_wildPlayerAttackHand = base.state.playerResourceDeck.GetCards(ResourceCard.Pile.AttackHand).Any((ResourceCard c) => c.hasWild);
		}
		if (_wildEnemyDefenseHand)
		{
			_wildEnemyDefenseHand = base.state.enemyResourceDeck.GetCards(ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand).Any((ResourceCard c) => c.hasWild);
		}
		if (!_wildPlayerAttackHand && !_wildEnemyDefenseHand)
		{
			Cancel();
		}
	}

	protected override void OnEnable()
	{
		ADeckLayoutBase.OnClick += _OnClick;
		ADeckLayoutBase.OnPointerEnter += _OnPointerEnter;
		ADeckLayoutBase.OnPointerExit += _OnPointerExit;
		if (_wildPlayerAttackHand)
		{
			base.view.wildPiles = ResourceCard.Piles.AttackHand;
		}
		if (_wildEnemyDefenseHand)
		{
			base.view.enemyWildPiles = ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand;
		}
		base.view.LogMessage(((_wildPlayerAttackHand && _wildEnemyDefenseHand) ? AbilityPreventedBy.ReactionCanWildCombatHands : (_wildPlayerAttackHand ? AbilityPreventedBy.ReactionCanWildPlayerAttack : AbilityPreventedBy.ReactionCanWildEnemyDefense)).LocalizeReaction());
		_UpdateGlows();
		base.OnEnable();
		base.state.activeCombat?.BeginShowPotentialDamage(base.state);
	}

	protected override IEnumerator Update()
	{
		while (!base.finished)
		{
			yield return null;
		}
	}

	protected override void OnDisable()
	{
		ADeckLayoutBase.OnClick -= _OnClick;
		ADeckLayoutBase.OnPointerEnter -= _OnPointerEnter;
		ADeckLayoutBase.OnPointerExit -= _OnPointerExit;
		base.view.ClearMessage();
		base.OnDisable();
		base.state.activeCombat?.EndShowPotentialDamage(base.state);
	}
}
