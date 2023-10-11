using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField("Statistic", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class StatAction : ACombatantAction
{
	[ProtoContract(EnumPassthru = true)]
	public enum Function
	{
		Add,
		Set,
		Subtract,
		Multiply,
		Divide
	}

	[ProtoMember(1)]
	[UIField]
	[UIHorizontalLayout("Top")]
	private StatType _stat;

	[ProtoMember(2)]
	[UIField]
	[UIDeepValueChange]
	private DynamicNumber _amount;

	[ProtoMember(3)]
	[UIField]
	[UIHorizontalLayout("Top")]
	private Function _function;

	protected override bool _canTick => false;

	protected override IEnumerable<DynamicNumber> _appliedDynamicNumbers
	{
		get
		{
			yield return _amount;
		}
	}

	public override int appliedSortingOrder => _function.SortOrder();

	private StatAction()
	{
	}

	public StatAction(StatType stat, DynamicNumber amount, Function function)
	{
		_stat = stat;
		_amount = amount;
		_function = function;
	}

	public override IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		foreach (AbilityKeyword keyword in base.GetKeywords(abilityData))
		{
			yield return keyword;
		}
		if (_stat == StatType.Offense)
		{
			yield return _TargetsEnemy(abilityData) ? AbilityKeyword.EnemyOffense : AbilityKeyword.PlayerOffense;
		}
		else if (_stat == StatType.Defense)
		{
			yield return _TargetsEnemy(abilityData) ? AbilityKeyword.EnemyDefense : AbilityKeyword.PlayerDefense;
		}
	}

	protected override void _Register(AppliedAction appliedAction)
	{
		appliedAction.context.GetTarget<ACombatant>().stats.Register(_stat, appliedAction);
	}

	protected override void _Apply(ActionContext context, ACombatant combatant)
	{
		int num;
		switch (_function)
		{
		case Function.Add:
		case Function.Subtract:
			num = _amount.GetValue(context) * _function.Sign();
			break;
		case Function.Set:
			num = _amount.GetOffset(context, combatant.stats[_stat]);
			break;
		case Function.Multiply:
			num = _amount.GetMultiplier(context, combatant.stats[_stat]);
			break;
		case Function.Divide:
			num = _amount.GetDivider(context, combatant.stats[_stat]);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		combatant.stats[_stat].value += num;
	}

	protected override void _Unapply(ActionContext context, ACombatant combatant)
	{
		combatant.stats[_stat].value -= _amount.GetValue(context, refreshValue: false) * _function.Sign();
	}

	public override void Reapply(ActionContext context)
	{
		context.GetTarget<ACombatant>().stats.Recalculate(_stat);
	}

	protected override void _Unregister(AppliedAction appliedAction)
	{
		appliedAction.context.GetTarget<ACombatant>().stats.Unregister(_stat, appliedAction);
	}

	protected override string _ToStringUnique()
	{
		if (!_function.IsAdjustment())
		{
			return $"{EnumUtil.FriendlyName(_stat)} = {_amount} for";
		}
		return string.Format("{0}{1}{2} {3} to", (_function == Function.Divide) ? "1" : "", _function.Symbol(), _amount, EnumUtil.FriendlyName(_stat));
	}
}
