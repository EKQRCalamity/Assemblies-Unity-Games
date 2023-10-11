using System.Collections.Generic;
using System.Linq;

public class GameStepActionTarget : GameStepAAction
{
	public class Tick : GameStepActionTarget
	{
		private readonly bool _hasCustomTickTarget;

		public Tick(AAction action, ActionContext context, bool hasCustomTickTarget)
			: base(action, context, null)
		{
			_hasCustomTickTarget = hasCustomTickTarget;
		}

		public override void Start()
		{
			_targets = ((_hasCustomTickTarget || base.action.ShouldTick(base.context)) ? new List<ATarget> { base.context.target } : new List<ATarget>(0));
			base.state.SignalAbilityTargeting(base.context, base.action, _targets);
		}

		protected override void End()
		{
			_ExcludeTargets();
			if (_targets.IsNullOrEmpty())
			{
				CancelGroup();
			}
		}
	}

	protected List<ATarget> _targets;

	public AAction.Target targeting { get; }

	public IEnumerable<ATarget> targets
	{
		get
		{
			IEnumerable<ATarget> enumerable = _targets;
			return enumerable ?? Enumerable.Empty<ATarget>();
		}
	}

	public bool targetsHaveBeenSelected => _targets != null;

	public bool hasTargets
	{
		get
		{
			List<ATarget> list = _targets;
			if (list == null)
			{
				return false;
			}
			return list.Count > 0;
		}
	}

	public bool effectedTarget
	{
		get
		{
			if (hasTargets)
			{
				return base.action.hasEffectOnTarget;
			}
			return false;
		}
	}

	public GameStepActionTarget(AAction action, ActionContext context, AAction.Target targeting)
		: base(action, context)
	{
		this.targeting = targeting;
	}

	protected void _SetTargets(IEnumerable<ATarget> targetsToSet)
	{
		_targets = targeting.PostProcessTargets(base.context, targetsToSet.ToList());
		base.state.SignalAbilityTargeting(base.context, base.action, _targets);
	}

	private void _ClearPersistentGlow()
	{
		base.context.ability?.view?.ReleaseGlow(base.context.ability, GlowTags.Persistent);
	}

	private void _ExcludeTargets()
	{
		if (_targets.IsNullOrEmpty())
		{
			return;
		}
		PoolKeepItemHashSetHandle<Id<ATarget>> tag = base.contextGroup.GetTag<PoolKeepItemHashSetHandle<Id<ATarget>>>();
		if (tag == null)
		{
			return;
		}
		for (int num = _targets.Count - 1; num >= 0; num--)
		{
			if (tag.Contains(_targets[num]))
			{
				_targets.RemoveAt(num);
			}
		}
	}

	protected override void OnEnable()
	{
		targeting?.OnEnable(this);
	}

	public override void Start()
	{
		_SetTargets(targeting.GetTargetable(base.context, base.action));
	}

	protected override void End()
	{
		_ExcludeTargets();
		_ClearPersistentGlow();
		if (GetPreviousSteps().OfType<GameStepActionTarget>().Any((GameStepActionTarget s) => s.hasTargets) || !(base.group is GameStepGroupAbilityAct) || !hasTargets)
		{
			return;
		}
		AppendStep(new GameStepWaitForCardTransition(base.context.ability?.view));
		AppendStep(new GameStepAbilityCastBegin(base.context.ability, _targets));
		if ((bool)base.context.ability?.data?.cosmetic?.castMedia)
		{
			AppendStep(new GameStepCastMedia(base.action, base.context.SetTarget(targets.FirstOrDefault())));
		}
		Ability ability2 = base.context.ability;
		if (ability2 != null && ability2.isBuff)
		{
			ACombatant buffTarget = targets.OfType<ACombatant>().FirstOrDefault();
			if (buffTarget != null)
			{
				Ability ability = base.context.ability;
				if (ability != null && ability.IsValidBuffTarget(buffTarget))
				{
					AppendStep(new GameStepGeneric
					{
						onStart = delegate
						{
							buffTarget.BeginBuffApply(ability);
						}
					});
					return;
				}
			}
		}
		Ability ability3 = base.context.ability;
		if (ability3 != null && ability3.isSummon)
		{
			AppendStep(new GameStepSummon(base.action, base.context, targets.OfType<TurnOrderSpace>().First()));
		}
	}

	protected override void OnDisable()
	{
		targeting?.OnDisable(this);
	}

	protected override void OnCanceled()
	{
		_ClearPersistentGlow();
	}

	public bool EffectedTarget(ATarget target)
	{
		if (effectedTarget)
		{
			return _targets.Contains(target);
		}
		return false;
	}

	public bool ExcludedTarget(ATarget target)
	{
		using PoolKeepItemHashSetHandle<Id<ATarget>> poolKeepItemHashSetHandle = base.contextGroup.GetTag<PoolKeepItemHashSetHandle<Id<ATarget>>>();
		return poolKeepItemHashSetHandle?.Contains(target) ?? false;
	}
}
