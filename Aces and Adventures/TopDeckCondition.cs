using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
public class TopDeckCondition
{
	[ProtoMember(1)]
	private AAction.DynamicNumber.ResourceDeckValue _topDeckHand;

	[ProtoMember(2)]
	[UIField(order = 2u)]
	[DefaultValue(FlagCheckType.GreaterThanOrEqualTo)]
	private FlagCheckType _comparison = FlagCheckType.GreaterThanOrEqualTo;

	[ProtoMember(3)]
	[UIField(order = 3u, collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	private AAction.DynamicNumber _comparedTo;

	[ProtoMember(4, OverwriteList = true)]
	[UIField]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<AAction.Condition.Combatant> _additionalConditions;

	[UIField(order = 1u, collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	private AAction.DynamicNumber.ResourceDeckValue topDeckHand
	{
		get
		{
			return _topDeckHand ?? (_topDeckHand = new AAction.DynamicNumber.ResourceDeckValue(ResourceCard.Piles.TopDeckHand));
		}
		set
		{
			_topDeckHand = value;
		}
	}

	public bool IsValid(ActionContext context)
	{
		if (_comparison.Check(_topDeckHand.GetValue(context), _comparedTo.GetValue(context)))
		{
			return _additionalConditions.All(context);
		}
		return false;
	}

	public override string ToString()
	{
		return $"If <size=66%>{_topDeckHand}</size> {_comparison.GetText()} {_comparedTo}" + (_additionalConditions.IsNullOrEmpty() ? "" : (" & " + _additionalConditions.ToStringSmart(" & ").SizeIfNotEmpty()));
	}
}
