using System;
using ProtoBuf;

[ProtoContract]
public class AppliedAction : IComparable<AppliedAction>, IRegister
{
	[ProtoMember(1)]
	private ActionContext _context;

	[ProtoMember(2)]
	private AAction _action;

	public ActionContext context => _context;

	public AAction action => _action;

	[ProtoMember(15)]
	public int registerId { get; set; }

	public event Action onReapply;

	public static AppliedAction Apply(ActionContext context, AAction action)
	{
		return new AppliedAction(context, action)._Apply();
	}

	private AppliedAction()
	{
	}

	public AppliedAction(ActionContext context, AAction action)
	{
		_context = context;
		_action = ProtoUtil.Clone(action);
	}

	private AppliedAction _Apply()
	{
		context.gameState.appliedActions.Add(this);
		_action.Apply(_context);
		_action.PostProcessApply(this);
		this.Register();
		return this;
	}

	public void _Register()
	{
		_action.Register(this);
	}

	public void _Unregister()
	{
		_action.Unregister(this);
	}

	public void Unapply(bool clearPendingTicks = false)
	{
		_action.Unapply(_context);
		this.Unregister();
		context.gameState.appliedActions.Remove(this);
		if (!clearPendingTicks)
		{
			return;
		}
		foreach (GameStepGroupActionTick group in context.gameState.stack.GetGroups<GameStepGroupActionTick>())
		{
			if (group.appliedAction == this && !group.hasStarted)
			{
				group.Cancel();
			}
		}
	}

	public void Reapply()
	{
		_action.Reapply(_context);
		this.onReapply?.Invoke();
	}

	public void OnTick(ReactionContext reactionContext, TargetedReactionFilter filter, int capturedValue)
	{
		if (filter.IsValid(reactionContext, _context.SetCapturedValue(capturedValue)))
		{
			_context.gameState.stack.Append(new GameStepGroupActionTick(this, _context.SetCapturedValue(capturedValue).SetTarget(filter.GetTarget(reactionContext, _context)).SetState(ActionContext.State.Tick), reactionContext));
		}
	}

	public void OnDurationComplete(ReactionContext reactionContext, ReactionFilter filter)
	{
		if (filter.IsValid(reactionContext, _context))
		{
			Unapply();
		}
	}

	public void SetAppliedOn(ATarget appliedOn)
	{
		_context = _context.SetTarget(appliedOn);
	}

	public int CompareTo(AppliedAction other)
	{
		return _action.appliedSortingOrder - other._action.appliedSortingOrder;
	}

	public override string ToString()
	{
		return string.Format("Applied Action: Context = [{0}], Action = [{1}]", _context, _action?.ToString().RemoveRichText() ?? "Null");
	}
}
