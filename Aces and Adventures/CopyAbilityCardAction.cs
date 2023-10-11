using ProtoBuf;

[ProtoContract]
[UIField("Copy Ability Card", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Ability Card")]
public class CopyAbilityCardAction : AAbilityAction
{
	[ProtoMember(1)]
	[UIField(tooltip = "Defines which ability pile the card will end up in.\n<i>This overrides the default of having the copied card end up in same pile as the card that was copied.</i>")]
	private Ability.Pile? _pileToTransferInto;

	[ProtoMember(2)]
	[UIField(tooltip = "Permanent copies will count toward max hand size, and will not automatically be exiled upon use.")]
	private bool _permanent;

	[ProtoMember(15)]
	private Id<Ability> _createdCard;

	protected override bool _canTick => false;

	private bool _createdCardSpecified => _createdCard.shouldSerialize;

	protected override void _Apply(ActionContext context, Ability ability)
	{
		if (!_createdCard)
		{
			_createdCard = ProtoUtil.CloneTarget(ability);
			_createdCard.value.ephemeral.value = !_permanent;
			IdDeck<Ability.Pile, Ability> abilityDeck = context.gameState.player.abilityDeck;
			Ability.Pile pile = _pileToTransferInto ?? ability.abilityPile;
			Ability.Pile pile2 = ((pile == Ability.Pile.Draw) ? Ability.Pile.ActivationHandWaiting : Ability.Pile.Draw);
			int num = abilityDeck.Count(Ability.Pile.Draw);
			abilityDeck.Transfer(_createdCard, pile2, (pile2 == Ability.Pile.Draw) ? new int?(num) : null);
			abilityDeck.layout.TransferWithSpecialTransitions(_createdCard, pile, (pile == Ability.Pile.Draw) ? new int?(context.gameState.random.RangeInt(0, num)) : null);
		}
	}

	protected override void _Unapply(ActionContext context, Ability ability)
	{
		if ((bool)_createdCard)
		{
			context.gameState.exileDeck.Transfer((Ability)_createdCard, ExilePile.PlayerResource);
		}
		_createdCard = Id<Ability>.Null;
	}

	protected override string _ToStringUnique()
	{
		return _permanent.ToText("Permanently ") + "Copy";
	}

	protected override string _ToStringAfterTarget()
	{
		if (!_pileToTransferInto.HasValue)
		{
			return base._ToStringAfterTarget();
		}
		return " into " + EnumUtil.FriendlyName(_pileToTransferInto);
	}
}
