using ProtoBuf;

[ProtoContract]
[UIField("Resource Action", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
[ProtoInclude(5, typeof(StealCombatCardAction))]
[ProtoInclude(6, typeof(WildCardAction))]
[ProtoInclude(7, typeof(CopyResourceCardAction))]
[ProtoInclude(8, typeof(RedrawResourceCardAction))]
[ProtoInclude(9, typeof(RemoveResourceCardAction))]
[ProtoInclude(10, typeof(SetResourceCardValueAction))]
[ProtoInclude(11, typeof(TargetResourceAction))]
[ProtoInclude(12, typeof(SetNaturalValueAction))]
public abstract class AResourceAction : AAction
{
	protected const string CAT_RESOURCE = "Resource Card";

	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Open, tooltip = "Determines which Resource Cards can be targeted by this action.")]
	[UIDeepValueChange]
	[UIHeader("Resource Card Action")]
	[UIMargin(24f, false)]
	protected Target.Resource _target;

	[ProtoMember(2)]
	[UIField("Tick Targeting", 0u, null, null, null, null, null, null, false, null, 5, false, null, validateOnChange = true, tooltip = "Allows overriding the default targeting logic used for action ticking.")]
	[UIHorizontalLayout("Tick", flexibleWidth = 1f)]
	protected TickTargetType _useCustomTickTargeting;

	[ProtoMember(3)]
	[UIField(collapse = UICollapseType.Open, tooltip = "Determines which cards will be targeted by the ticking of this action.")]
	[UIDeepValueChange]
	[UIHideIf("_hideCustomTickTarget")]
	[UIHorizontalLayout("Tick", flexibleWidth = 8f)]
	protected Target.Resource _customTickTarget;

	public override Target target => _target;

	protected override Target _tickTarget => _useCustomTickTargeting switch
	{
		TickTargetType.Custom => _customTickTarget, 
		TickTargetType.UseActTarget => target, 
		_ => null, 
	};

	protected bool _hideCustomTickTarget => _useCustomTickTargeting != TickTargetType.Custom;

	protected bool _customTickTargetSpecified => _useCustomTickTargeting == TickTargetType.Custom;

	protected sealed override bool _ShouldAct(ActionContext context)
	{
		if (context.target is ResourceCard resourceCard)
		{
			return _ShouldActUnique(context, resourceCard);
		}
		return false;
	}

	protected sealed override void _Tick(ActionContext context)
	{
		_Tick(context, context.target as ResourceCard);
	}

	public sealed override void Apply(ActionContext context)
	{
		_Apply(context, context.target as ResourceCard);
	}

	public sealed override void Unapply(ActionContext context)
	{
		_Unapply(context, context.target as ResourceCard);
	}

	protected virtual bool _ShouldActUnique(ActionContext context, ResourceCard resourceCard)
	{
		return true;
	}

	protected virtual void _Tick(ActionContext context, ResourceCard resourceCard)
	{
	}

	protected virtual void _Apply(ActionContext context, ResourceCard resourceCard)
	{
	}

	protected virtual void _Unapply(ActionContext context, ResourceCard resourceCard)
	{
	}

	protected override string _GetTargetString()
	{
		if (!_customTickTargetSpecified)
		{
			return base._GetTargetString();
		}
		return $" {_customTickTarget} <size=66%>(Custom Tick)</size>";
	}
}
