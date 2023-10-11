using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField("Modify Resource Cost", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Ability Card")]
public class ModifyResourceCostAction : AAbilityAction
{
	[ProtoMember(1, OverwriteList = true)]
	[UIField(collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<ResourceCostModifier> _resourceCostModifications;

	private List<ResourceCostModifier> resourceCostModifications => _resourceCostModifications ?? (_resourceCostModifications = new List<ResourceCostModifier>());

	protected override bool _canTick => false;

	protected override void _Apply(ActionContext context, Ability ability)
	{
		foreach (ResourceCostModifier resourceCostModification in resourceCostModifications)
		{
			ability.AddResourceCostModifier(resourceCostModification);
		}
		ability.RefreshResourceCost();
	}

	protected override void _Unapply(ActionContext context, Ability ability)
	{
		if (ability == null)
		{
			return;
		}
		foreach (ResourceCostModifier resourceCostModification in resourceCostModifications)
		{
			ability.RemoveResourceCostModifier(resourceCostModification);
		}
		ability.RefreshResourceCost();
	}

	protected override string _ToStringUnique()
	{
		return _resourceCostModifications.ToStringSmart(" & ") + " for";
	}
}
