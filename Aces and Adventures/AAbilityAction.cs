using ProtoBuf;

[ProtoContract]
[UIField("Ability Action", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
[ProtoInclude(10, typeof(ModifyResourceCostAction))]
[ProtoInclude(11, typeof(TargetAbilityAction))]
[ProtoInclude(12, typeof(RemoveAbilityAction))]
[ProtoInclude(13, typeof(CopyAbilityCardAction))]
[ProtoInclude(14, typeof(TransferAbilityCardAction))]
public abstract class AAbilityAction : AAction
{
	public const string CAT_ABILITY = "Ability Card";

	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Open, tooltip = "Determines which Ability Cards can be targeted by this action.")]
	[UIDeepValueChange]
	[UIHeader("Ability Card Action")]
	[UIMargin(24f, false)]
	protected Target.AAbility _target;

	public override Target target => _target;

	protected sealed override bool _ShouldAct(ActionContext context)
	{
		if (context.target is Ability ability)
		{
			return _ShouldActUnique(context, ability);
		}
		return false;
	}

	protected sealed override void _Tick(ActionContext context)
	{
		_Tick(context, context.target as Ability);
	}

	public sealed override void Apply(ActionContext context)
	{
		_Apply(context, context.target as Ability);
	}

	public sealed override void Unapply(ActionContext context)
	{
		_Unapply(context, context.target as Ability);
	}

	protected virtual bool _ShouldActUnique(ActionContext context, Ability ability)
	{
		return true;
	}

	protected virtual void _Tick(ActionContext context, Ability ability)
	{
	}

	protected virtual void _Apply(ActionContext context, Ability ability)
	{
	}

	protected virtual void _Unapply(ActionContext context, Ability ability)
	{
	}
}
