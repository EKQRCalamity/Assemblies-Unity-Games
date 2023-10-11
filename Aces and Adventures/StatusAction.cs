using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField("Status", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class StatusAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField]
	private StatusType _status;

	[ProtoMember(2, OverwriteList = true)]
	[UIField(tooltip = "If a condition is added, status will only be active while all the following conditions are true.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.Combatant> _dynamicConditions;

	[ProtoMember(15)]
	private bool _active;

	private AppliedAction _appliedAction;

	public StatusType status => _status;

	private bool active
	{
		set
		{
			if (!_dynamicConditions.IsNullOrEmpty())
			{
				_active = value;
			}
		}
	}

	private bool _shouldUnapply
	{
		get
		{
			if (!_dynamicConditions.IsNullOrEmpty())
			{
				return _active;
			}
			return true;
		}
	}

	protected override bool _canTick => false;

	private bool _ShouldApply(ActionContext context)
	{
		if (!_dynamicConditions.IsNullOrEmpty())
		{
			if (!_active)
			{
				return _dynamicConditions.All(context);
			}
			return false;
		}
		return true;
	}

	private void _OnConditionDirty(ATarget targetOfCondition)
	{
		Reapply(_appliedAction.context);
	}

	protected override void _Register(AppliedAction appliedAction)
	{
		if (_dynamicConditions.IsNullOrEmpty())
		{
			return;
		}
		foreach (Condition.Combatant dynamicCondition in _dynamicConditions)
		{
			dynamicCondition.Register(appliedAction.context);
			dynamicCondition.onDirty = (Action<ATarget>)Delegate.Combine(dynamicCondition.onDirty, new Action<ATarget>(_OnConditionDirty));
		}
		_appliedAction = appliedAction;
	}

	protected override void _Unregister(AppliedAction appliedAction)
	{
		if (_dynamicConditions.IsNullOrEmpty())
		{
			return;
		}
		foreach (Condition.Combatant dynamicCondition in _dynamicConditions)
		{
			dynamicCondition.Unregister(appliedAction.context);
			dynamicCondition.onDirty = (Action<ATarget>)Delegate.Remove(dynamicCondition.onDirty, new Action<ATarget>(_OnConditionDirty));
		}
		_appliedAction = null;
	}

	protected override void _Apply(ActionContext context, ACombatant combatant)
	{
		if (_ShouldApply(context) && (active = true))
		{
			Statuses statuses = combatant.statuses;
			StatusType statusType = _status;
			int value = statuses[statusType] + 1;
			statuses[statusType] = value;
		}
	}

	protected override void _Unapply(ActionContext context, ACombatant combatant)
	{
		if (_shouldUnapply && !(active = false))
		{
			Statuses statuses = combatant.statuses;
			StatusType statusType = _status;
			int value = statuses[statusType] - 1;
			statuses[statusType] = value;
		}
	}

	public override IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		foreach (AbilityKeyword keyword in base.GetKeywords(abilityData))
		{
			yield return keyword;
		}
		if (status == StatusType.CanBeReducedToZeroDefense || status == StatusType.CanReduceEnemyDefenseToZero)
		{
			yield return AbilityKeyword.CanReduceDefenseToZero;
		}
		else if (status == StatusType.RedrawAttack)
		{
			yield return AbilityKeyword.Redraw;
		}
		else if (status == StatusType.SafeAttack)
		{
			yield return AbilityKeyword.SafeAttack;
		}
		foreach (AbilityKeyword keyword2 in _dynamicConditions.GetKeywords())
		{
			yield return keyword2;
		}
	}

	protected override string _ToStringUnique()
	{
		return "Apply <b>" + EnumUtil.FriendlyName(_status) + "</b> Status" + ((!_dynamicConditions.IsNullOrEmpty()) ? (" while " + _dynamicConditions.ToStringSmart(" & ")).SizeIfNotEmpty() : "") + " To";
	}
}
