using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField("Wild Card", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Resource Card")]
public class WildCardAction : AResourceAction
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIDeepValueChange]
	private AWild _wild;

	protected override bool _canTick => false;

	protected override void _Apply(ActionContext context, ResourceCard resourceCard)
	{
		resourceCard.AddWildModification(_wild);
	}

	protected override void _Unapply(ActionContext context, ResourceCard resourceCard)
	{
		resourceCard.RemoveWildModification(_wild);
	}

	public override IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		foreach (AbilityKeyword keyword in base.GetKeywords(abilityData))
		{
			yield return keyword;
		}
		foreach (AbilityKeyword keyword2 in _wild.GetKeywords())
		{
			yield return keyword2;
		}
	}

	protected override string _ToStringUnique()
	{
		return _wild?.ToString() ?? "";
	}
}
