using ProtoBuf;

[ProtoContract]
[UIField]
[ProtoInclude(7, typeof(ShuffleResourceDeckAction))]
[ProtoInclude(8, typeof(ShuffleAbilityDeckAction))]
[ProtoInclude(9, typeof(DiscardResourceCardAction))]
[ProtoInclude(10, typeof(DrawResourceCardAction))]
[ProtoInclude(11, typeof(DrawAbilityCardAction))]
[ProtoInclude(12, typeof(PlayerStatAction))]
[ProtoInclude(13, typeof(DiscardAbilityAction))]
[ProtoInclude(14, typeof(TargetPlayerAction))]
[ProtoInclude(15, typeof(NumberOfHeroAbilitiesAction))]
public abstract class APlayerAction : AAction
{
	protected const string CAT_PLAYER = "Player";

	private static readonly Target.Player TARGET = new Target.Player();

	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Open, tooltip = "Allows specifying conditions that must be true in order to target player.")]
	[UIDeepValueChange]
	[UIHeader("Player Action")]
	[UIMargin(24f, false)]
	protected Target.Player _target;

	[ProtoMember(2)]
	[UIField(validateOnChange = true, tooltip = "Allows overriding the default targeting logic used for action ticking.")]
	[UIHorizontalLayout("Tick", flexibleWidth = 0f)]
	protected bool _useCustomTickTargeting;

	[ProtoMember(3)]
	[UIField(collapse = UICollapseType.Open, tooltip = "Allows specifying conditions that must be true in order to tick on player.")]
	[UIDeepValueChange]
	[UIHideIf("_hideCustomTickTarget")]
	[UIHorizontalLayout("Tick", flexibleWidth = 999f)]
	protected Target.Player _customTickTarget;

	public override Target target => _target ?? TARGET;

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

	private bool _targetSpecified => _target;

	private bool _customTickTargetSpecified
	{
		get
		{
			if (_useCustomTickTargeting)
			{
				return _customTickTarget;
			}
			return false;
		}
	}

	protected sealed override bool _ShouldAct(ActionContext context)
	{
		if (context.target is Player player)
		{
			return _ShouldActUnique(context, player);
		}
		return false;
	}

	public override bool ShouldTick(ActionContext context)
	{
		if (base.ShouldTick(context))
		{
			if (context.target is Player player)
			{
				return player.alive;
			}
			return false;
		}
		return false;
	}

	protected sealed override void _Tick(ActionContext context)
	{
		_Tick(context, context.target as Player);
	}

	public sealed override void Apply(ActionContext context)
	{
		_Apply(context, context.target as Player);
	}

	public sealed override void Unapply(ActionContext context)
	{
		_Unapply(context, context.target as Player);
	}

	protected virtual bool _ShouldActUnique(ActionContext context, Player player)
	{
		return true;
	}

	protected virtual void _Tick(ActionContext context, Player player)
	{
	}

	protected virtual void _Apply(ActionContext context, Player player)
	{
	}

	protected virtual void _Unapply(ActionContext context, Player player)
	{
	}

	protected override string _GetTargetString()
	{
		return base.isTicking.ToText(" " + ((!_hideCustomTickTarget && _customTickTarget != null) ? _customTickTarget.ToString() : "Tick Target") + " <size=66%>applied on</size>") + base._GetTargetString();
	}
}
