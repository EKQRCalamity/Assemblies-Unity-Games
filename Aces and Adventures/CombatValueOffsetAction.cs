using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField("Combat Card Value Offset", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class CombatValueOffsetAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField(tooltip = "Types of combat which will be affected.")]
	[DefaultValue(CombatTypes.Attack | CombatTypes.Defense)]
	private CombatTypes _combatTypes = CombatTypes.Attack | CombatTypes.Defense;

	[ProtoMember(2)]
	[UIField(tooltip = "The value which will be added to cards in combat.")]
	[UIDeepValueChange]
	private DynamicNumber _offset;

	protected override bool _canTick => false;

	protected override IEnumerable<DynamicNumber> _appliedDynamicNumbers
	{
		get
		{
			yield return _offset;
		}
	}

	protected override void _Apply(ActionContext context, ACombatant combatant)
	{
		int value = _offset.GetValue(context);
		if (EnumUtil.HasFlag(_combatTypes, CombatTypes.Attack))
		{
			combatant.combat.attackOffset += value;
		}
		if (EnumUtil.HasFlag(_combatTypes, CombatTypes.Defense))
		{
			combatant.combat.defenseOffset += value;
		}
	}

	protected override void _Unapply(ActionContext context, ACombatant combatant)
	{
		int value = _offset.GetValue(context, refreshValue: false);
		if (EnumUtil.HasFlag(_combatTypes, CombatTypes.Attack))
		{
			combatant.combat.attackOffset -= value;
		}
		if (EnumUtil.HasFlag(_combatTypes, CombatTypes.Defense))
		{
			combatant.combat.defenseOffset -= value;
		}
	}

	protected override string _ToStringUnique()
	{
		return $"{_offset} Value Offset To {EnumUtil.FriendlyName(_combatTypes)} Cards for";
	}
}
