using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField("Tick Action", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Use to allow ticking of actions that would otherwise be unable to tick.", category = "Specialized")]
public class TickAction : AAction
{
	private static readonly Target.Combatant.Self TARGET_SELF = new Target.Combatant.Self();

	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	private AAction _actionToTick;

	[ProtoMember(2)]
	[UIField(tooltip = "By default, this action uses <b>Action To Tick's</b> target to determine who to be applied on.\n<i>Use this option to apply action only on its owner, allowing for treating <b>Action To Tick's</b> target as a custom tick target.</i>")]
	[UIHorizontalLayout("A", flexibleWidth = 0f)]
	private bool _applyOnSelf;

	[ProtoMember(3)]
	[UIField(tooltip = "Allows using <b>Action To Tick's</b> tick media, as well as leveraging its <b>Should Tick</b> logic.")]
	[UIHorizontalLayout("A", flexibleWidth = 9999f)]
	private bool _useActionToTickDirectly;

	public override Target target
	{
		get
		{
			object obj;
			if (!_applyOnSelf)
			{
				obj = _actionToTick?.target;
				if (obj == null)
				{
					return TopDeckAction.TARGET;
				}
			}
			else
			{
				obj = TARGET_SELF;
			}
			return (Target)obj;
		}
	}

	protected override Target _tickTarget => _actionToTick?.target;

	protected override AAction _tickAction
	{
		get
		{
			if (!_useActionToTickDirectly)
			{
				return this;
			}
			return _actionToTick;
		}
	}

	protected override void _Tick(ActionContext context)
	{
		_actionToTick.Tick(context);
	}

	protected override string _ToStringUnique()
	{
		return _actionToTick?.ToString();
	}

	protected override string _GetTargetString()
	{
		if (!_applyOnSelf)
		{
			return "";
		}
		return " applied on yourself".SizeIfNotEmpty();
	}

	public override IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		foreach (AbilityKeyword keyword in base.GetKeywords(abilityData))
		{
			yield return keyword;
		}
		foreach (AbilityKeyword item in _actionToTick?.GetKeywords(abilityData) ?? Enumerable.Empty<AbilityKeyword>())
		{
			yield return item;
		}
	}
}
