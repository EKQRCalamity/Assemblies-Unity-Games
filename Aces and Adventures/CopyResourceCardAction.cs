using ProtoBuf;

[ProtoContract]
[UIField("Copy Resource Card", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Resource Card")]
public class CopyResourceCardAction : AResourceAction
{
	[ProtoMember(1)]
	[UIField(tooltip = "Defines which resource pile the card will end up in.\n<i>This overrides the default of having the copied card end up in same pile as the card that was copied.</i>")]
	private ResourceCard.Pile? _pileToTransferInto;

	[ProtoMember(2)]
	[UIField(tooltip = "Permanent copies will count toward max hand size, and will not automatically be exiled upon use.")]
	private bool _permanent;

	[ProtoMember(3)]
	[UIField(tooltip = "Override the natural value of created card.")]
	[UIHorizontalLayout("Override")]
	private PlayingCardValue? _valueOverride;

	[ProtoMember(4)]
	[UIField(tooltip = "Override the natural suit of created card.")]
	[UIHorizontalLayout("Override")]
	private PlayingCardSuit? _suitOverride;

	[ProtoMember(15)]
	private Id<ResourceCard> _createdCard;

	protected override bool _canTick => false;

	private bool _createdCardSpecified => _createdCard.shouldSerialize;

	protected override void _Apply(ActionContext context, ResourceCard resourceCard)
	{
		if (!_createdCard)
		{
			_createdCard = ProtoUtil.CloneTarget(resourceCard);
			_createdCard.value.skin = ((context.actor.faction == Faction.Enemy) ? PlayingCardSkinType.Enemy : ProfileManager.options.cosmetic.playingCardDeck);
			_createdCard.value.ephemeral.value = !_permanent;
			PlayingCardValue? valueOverride = _valueOverride;
			if (valueOverride.HasValue)
			{
				PlayingCardValue valueOrDefault = valueOverride.GetValueOrDefault();
				_createdCard.value.naturalValue = _createdCard.value.naturalValue.ChangeValue(valueOrDefault);
			}
			PlayingCardSuit? suitOverride = _suitOverride;
			if (suitOverride.HasValue)
			{
				PlayingCardSuit valueOrDefault2 = suitOverride.GetValueOrDefault();
				_createdCard.value.naturalValue = _createdCard.value.naturalValue.ChangeSuit(valueOrDefault2);
			}
			IdDeck<ResourceCard.Pile, ResourceCard> obj = (_pileToTransferInto.HasValue ? _createdCard.value.deck : resourceCard.deck);
			ResourceCard.Pile pile = _pileToTransferInto ?? resourceCard.pile;
			ResourceCard.Pile pile2 = ((pile == ResourceCard.Pile.DrawPile) ? ResourceCard.Pile.ActivationHandWaiting : ResourceCard.Pile.DrawPile);
			int num = resourceCard.deck.Count(ResourceCard.Pile.DrawPile);
			obj.Transfer(_createdCard, pile2, (pile2 == ResourceCard.Pile.DrawPile) ? new int?(num) : null);
			obj.layout.TransferWithSpecialTransitions(_createdCard, pile, (pile == ResourceCard.Pile.DrawPile) ? new int?(context.gameState.random.RangeInt(0, num)) : null);
		}
	}

	public override void PostProcessApply(AppliedAction appliedAction)
	{
		appliedAction.SetAppliedOn(appliedAction.context.actor);
	}

	protected override void _Unapply(ActionContext context, ResourceCard resourceCard)
	{
		if ((bool)_createdCard)
		{
			context.gameState.exileDeck.Transfer((ResourceCard)_createdCard, _createdCard.value.exilePile);
		}
		_createdCard = Id<ResourceCard>.Null;
	}

	protected override string _ToStringUnique()
	{
		return _permanent.ToText("Permanently ") + "Copy" + ((_valueOverride.HasValue || _suitOverride.HasValue) ? (" as a " + EnumUtil.FriendlyName(_valueOverride) + (_suitOverride.HasValue ? ((_valueOverride.HasValue ? " of " : "") + EnumUtil.FriendlyName(_suitOverride)) : "")) : "").SizeIfNotEmpty();
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
