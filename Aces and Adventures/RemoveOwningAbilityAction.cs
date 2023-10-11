using ProtoBuf;

[ProtoContract]
[UIField("Remove Owning Ability", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Removes THIS ability from whoever it was applied upon.", category = "Specialized")]
public class RemoveOwningAbilityAction : AAction
{
	protected static readonly Target.Combatant.Inherit TARGET = new Target.Combatant.Inherit
	{
		allegiance = Target.Combatant.Allegiance.Any
	};

	[ProtoMember(1)]
	[UIField(validateOnChange = true)]
	private bool _useCustomTickTargeting;

	[ProtoMember(2)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIHideIf("_hideCustomTickTarget")]
	private Target.Combatant.Inherit _customTickTarget;

	public override Target target => TARGET;

	protected override Target _tickTarget
	{
		get
		{
			if (!_useCustomTickTargeting)
			{
				return null;
			}
			return _customTickTarget;
		}
	}

	private bool _hideCustomTickTarget => !_customTickTargetSpecified;

	private bool _customTickTargetSpecified => _useCustomTickTargeting;

	protected override void _Tick(ActionContext context)
	{
		context.ability.Remove(context);
	}

	protected override string _ToStringUnique()
	{
		return "Remove Ability On";
	}

	protected override string _GetTargetString()
	{
		return base.isTicking.ToText(" " + ((!_hideCustomTickTarget && _customTickTarget != null) ? _customTickTarget.ToString() : "Tick Target") + " <size=66%>applied on</size>") + base._GetTargetString();
	}
}
