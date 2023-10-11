using ProtoBuf;

[ProtoContract]
[UIField("Combatant", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
[ProtoInclude(10, typeof(DamageAction))]
[ProtoInclude(11, typeof(CancelCombatAction))]
[ProtoInclude(12, typeof(SuccessfulAttackAction))]
[ProtoInclude(13, typeof(TapEntityAction))]
[ProtoInclude(14, typeof(StatAction))]
[ProtoInclude(15, typeof(ProcessCombatDamageAction))]
[ProtoInclude(16, typeof(ShieldAction))]
[ProtoInclude(17, typeof(KillAction))]
[ProtoInclude(18, typeof(MoveTargetAction))]
[ProtoInclude(19, typeof(MoveToTargetAction))]
[ProtoInclude(20, typeof(ProcessAbilityDamageAction))]
[ProtoInclude(21, typeof(NumberOfAttacksAction))]
[ProtoInclude(22, typeof(HealAction))]
[ProtoInclude(23, typeof(NegateTraitAction))]
[ProtoInclude(24, typeof(AddTraitAction))]
[ProtoInclude(25, typeof(StatusAction))]
[ProtoInclude(26, typeof(CombatPokerHandTypesAction))]
[ProtoInclude(27, typeof(SpecialDefenseRulesAction))]
[ProtoInclude(28, typeof(TieBreakerAction))]
[ProtoInclude(29, typeof(ProcessHealAmountAction))]
[ProtoInclude(30, typeof(SetCombatResultAction))]
[ProtoInclude(31, typeof(TargetCombatantAction))]
[ProtoInclude(32, typeof(CombatValueOffsetAction))]
[ProtoInclude(33, typeof(CaptureValueAction))]
[ProtoInclude(34, typeof(MoveTargetToSelfAction))]
[ProtoInclude(35, typeof(SetHPAction))]
[ProtoInclude(36, typeof(ProcessEnemyCombatHandSize))]
[ProtoInclude(37, typeof(CancelTargetAction))]
[ProtoInclude(38, typeof(RemoveAppliedAbilityAction))]
[ProtoInclude(39, typeof(RestoreTraitAction))]
[ProtoInclude(40, typeof(ProcessIgnoreShieldsAction))]
public abstract class ACombatantAction : AAction
{
	protected const string CAT_COMBAT = "Combatant";

	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Open, tooltip = "Determines which combatants can be targeted by this action.")]
	[UIDeepValueChange]
	[UIHeader("Combatant Action")]
	[UIMargin(24f, false)]
	protected Target.Combatant _target;

	[ProtoMember(2)]
	[UIField(validateOnChange = true, tooltip = "Allows overriding the default targeting logic used for action ticking.")]
	[UIHorizontalLayout("Tick", flexibleWidth = 0f)]
	protected bool _useCustomTickTargeting;

	[ProtoMember(3)]
	[UIField(collapse = UICollapseType.Open, tooltip = "Determines which combatants will be targeted by the ticking of this action.")]
	[UIDeepValueChange]
	[UIHideIf("_hideCustomTickTarget")]
	[UIHorizontalLayout("Tick", flexibleWidth = 999f)]
	protected Target.Combatant _customTickTarget;

	public override Target target => _target;

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

	protected bool _hideCustomTickTarget => !_useCustomTickTargeting;

	private bool _customTickTargetSpecified => _useCustomTickTargeting;

	protected bool _TargetsEnemy(AbilityData abilityData)
	{
		return (((base.isTicking ? (_tickTarget ?? target) : target) as Target.Combatant)?.TargetsEnemy(abilityData, this)).GetValueOrDefault();
	}

	protected sealed override bool _ShouldAct(ActionContext context)
	{
		if (context.target is ACombatant combatant)
		{
			if (context.state != 0 || !base.isTicking)
			{
				return _ShouldActUnique(context, combatant);
			}
			return true;
		}
		return false;
	}

	public override bool ShouldTick(ActionContext context)
	{
		if (base.ShouldTick(context))
		{
			if (context.target is ACombatant aCombatant)
			{
				return aCombatant.alive;
			}
			return false;
		}
		return false;
	}

	protected sealed override void _Tick(ActionContext context)
	{
		_Tick(context, context.target as ACombatant);
	}

	public sealed override void Apply(ActionContext context)
	{
		_Apply(context, context.target as ACombatant);
	}

	public sealed override void Unapply(ActionContext context)
	{
		_Unapply(context, context.target as ACombatant);
	}

	protected virtual bool _ShouldActUnique(ActionContext context, ACombatant combatant)
	{
		return true;
	}

	protected virtual void _Tick(ActionContext context, ACombatant combatant)
	{
	}

	protected virtual void _Apply(ActionContext context, ACombatant combatant)
	{
	}

	protected virtual void _Unapply(ActionContext context, ACombatant combatant)
	{
	}

	protected override string _GetTargetString()
	{
		return base.isTicking.ToText(" " + ((!_hideCustomTickTarget && _customTickTarget != null) ? _customTickTarget.ToString() : "Tick Target") + " <size=66%>applied on</size>") + base._GetTargetString();
	}
}
