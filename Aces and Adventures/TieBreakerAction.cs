using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField("Combat Tie Breaker", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class TieBreakerAction : ACombatantAction
{
	[ProtoContract(EnumPassthru = true)]
	public enum Result
	{
		Win,
		Lose
	}

	[ProtoMember(1)]
	[UIField(tooltip = "Types of combat which will have ties affected.")]
	[DefaultValue(CombatTypes.Attack | CombatTypes.Defense)]
	private CombatTypes _combatTypes = CombatTypes.Attack | CombatTypes.Defense;

	[ProtoMember(2)]
	[UIField(tooltip = "Tie breaker result for selected combat types.")]
	private Result _result;

	protected override bool _canTick => false;

	protected override void _Apply(ActionContext context, ACombatant combatant)
	{
		foreach (CombatType item in EnumUtil.FlagsConverted<CombatTypes, CombatType>(_combatTypes))
		{
			combatant.combat[item] += ((_result == Result.Win) ? 1 : (-1));
		}
	}

	protected override void _Unapply(ActionContext context, ACombatant combatant)
	{
		foreach (CombatType item in EnumUtil.FlagsConverted<CombatTypes, CombatType>(_combatTypes))
		{
			combatant.combat[item] -= ((_result == Result.Win) ? 1 : (-1));
		}
	}

	protected override string _ToStringUnique()
	{
		return EnumUtil.FriendlyName(_result) + " " + EnumUtil.FriendlyName(_combatTypes) + " Ties for";
	}
}
