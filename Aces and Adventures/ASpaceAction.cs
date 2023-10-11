using ProtoBuf;

[ProtoContract]
[UIField]
[ProtoInclude(10, typeof(MoveToSpaceAction))]
[ProtoInclude(11, typeof(TargetSpaceAction))]
public abstract class ASpaceAction : AAction
{
	protected const string CAT_SPACE = "Turn Order Space";

	[ProtoMember(1)]
	[UIField]
	[UIDeepValueChange]
	[UIHeader("Turn Order Space Action")]
	[UIMargin(24f, false)]
	protected Target.TurnOrder _target;

	public override Target target => _target;

	protected sealed override bool _ShouldAct(ActionContext context)
	{
		if (context.target is TurnOrderSpace space)
		{
			return _ShouldActUnique(context, space);
		}
		return false;
	}

	protected sealed override void _Tick(ActionContext context)
	{
		_Tick(context, context.target as TurnOrderSpace);
	}

	public sealed override void Apply(ActionContext context)
	{
		_Apply(context, context.target as TurnOrderSpace);
	}

	public sealed override void Unapply(ActionContext context)
	{
		_Unapply(context, context.target as TurnOrderSpace);
	}

	protected virtual bool _ShouldActUnique(ActionContext context, TurnOrderSpace space)
	{
		return true;
	}

	protected virtual void _Tick(ActionContext context, TurnOrderSpace space)
	{
	}

	protected virtual void _Apply(ActionContext context, TurnOrderSpace space)
	{
	}

	protected virtual void _Unapply(ActionContext context, TurnOrderSpace space)
	{
	}
}
