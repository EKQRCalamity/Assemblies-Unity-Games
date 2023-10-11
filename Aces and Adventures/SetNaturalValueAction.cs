using ProtoBuf;

[ProtoContract]
[UIField("Set Natural Value", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Resource Card")]
public class SetNaturalValueAction : AResourceAction
{
	[ProtoMember(1)]
	[UIField]
	[UIHorizontalLayout("T")]
	private PlayingCardValue? _value;

	[ProtoMember(2)]
	[UIField]
	[UIHorizontalLayout("T")]
	private PlayingCardSuit? _suit;

	[ProtoMember(3)]
	[UIField]
	private bool _addCurrentNaturalValueAsWild;

	[ProtoMember(10)]
	private PlayingCardType? _originalNaturalValue;

	private AWild _wild;

	private AWild wild => _wild ?? (_wild = new SetResourceCardValueAction(_originalNaturalValue?.Value(), _originalNaturalValue?.Suit()).wild);

	protected override bool _canTick => false;

	protected override void _Apply(ActionContext context, ResourceCard resourceCard)
	{
		_originalNaturalValue = resourceCard.naturalValue;
		resourceCard.naturalValue = resourceCard.naturalValue.ChangeSuit(_suit).ChangeValue(_value);
		if (_addCurrentNaturalValueAsWild)
		{
			resourceCard.AddWildModification(wild);
		}
	}

	protected override void _Unapply(ActionContext context, ResourceCard resourceCard)
	{
		PlayingCardType? originalNaturalValue = _originalNaturalValue;
		if (originalNaturalValue.HasValue)
		{
			PlayingCardType valueOrDefault = originalNaturalValue.GetValueOrDefault();
			resourceCard.naturalValue = valueOrDefault;
		}
		_originalNaturalValue = null;
		if (_addCurrentNaturalValueAsWild)
		{
			resourceCard.RemoveWildModification(wild);
		}
	}

	protected override string _ToStringUnique()
	{
		return "Set <b>natural</b> value to<b>" + _value.HasValue.ToText(EnumUtil.FriendlyName(_value)).PreSpaceIfNotEmpty() + _suit.HasValue.ToText(_value.HasValue.ToText("of ") + EnumUtil.FriendlyName(_suit)).PreSpaceIfNotEmpty() + "</b> for";
	}
}
