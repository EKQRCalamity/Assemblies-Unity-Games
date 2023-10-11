using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField("Negate Trait", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class NegateTraitAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide, tooltip = "The amount of traits which will be negated.")]
	[UIDeepValueChange]
	private DynamicNumber _amount;

	[ProtoMember(2, OverwriteList = true)]
	[UIField("Trait Conditions", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Conditions that must be met in order for trait to be negated.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.AAbility> _conditions;

	[ProtoMember(3)]
	[UIField(tooltip = "Whether or not this negate action can affect permanent traits.")]
	private bool _canNegatePermanents;

	[ProtoMember(15, OverwriteList = true)]
	private List<Id<Ability>> _negatedTraitIds;

	private AppliedAction _appliedAction;

	private List<Id<Ability>> negatedTraitIds => _negatedTraitIds ?? (_negatedTraitIds = new List<Id<Ability>>());

	public bool canNegatePermanents => _canNegatePermanents;

	protected override bool _canTick => false;

	private bool _IsValidTrait(ActionContext context, Id<Ability> trait)
	{
		if (_conditions.All<Condition.AAbility>(context.SetTarget((Ability)trait)))
		{
			if (!_canNegatePermanents)
			{
				return !trait.value.data.permanent;
			}
			return true;
		}
		return false;
	}

	private void _OnTraitAdded(ACombatant combatant, Ability trait)
	{
		if (combatant != _appliedAction.context.target || !_IsValidTrait(_appliedAction.context, trait) || negatedTraitIds.Contains(trait) || !combatant.HasTrait(trait.dataRef))
		{
			return;
		}
		combatant.gameState.stack.Push(new GameStepGenericSimple(delegate
		{
			if (!negatedTraitIds.Contains(trait) && combatant.HasTrait(trait.dataRef))
			{
				negatedTraitIds.Add(combatant.RemoveTrait(trait));
			}
		}));
	}

	private bool _ShouldRegister(AppliedAction appliedAction)
	{
		DynamicNumber amount = _amount;
		if (amount == null)
		{
			return false;
		}
		return amount.constantValue >= 25;
	}

	protected override bool _ShouldActUnique(ActionContext context, ACombatant combatant)
	{
		if (_amount.GetValue(context) > 0)
		{
			return combatant.Traits().AsEnumerable().Any((Id<Ability> trait) => _IsValidTrait(context, trait));
		}
		return false;
	}

	public override bool ShouldTick(ActionContext context)
	{
		return _ShouldAct(context);
	}

	protected override void _Register(AppliedAction appliedAction)
	{
		if (_ShouldRegister(appliedAction))
		{
			(_appliedAction = appliedAction).context.gameState.onTraitAdded += _OnTraitAdded;
		}
	}

	protected override void _Unregister(AppliedAction appliedAction)
	{
		if (_appliedAction != null)
		{
			appliedAction.context.gameState.onTraitAdded -= _OnTraitAdded;
		}
	}

	protected override void _Apply(ActionContext context, ACombatant combatant)
	{
		foreach (Id<Ability> item in (from trait in combatant.Traits().AsEnumerable()
			where _IsValidTrait(context, trait)
			select trait).Take(_amount.GetValue(context)))
		{
			negatedTraitIds.Add(combatant.RemoveTrait(item));
		}
	}

	protected override void _Unapply(ActionContext context, ACombatant combatant)
	{
		foreach (Id<Ability> negatedTraitId in negatedTraitIds)
		{
			if (combatant.IsRegistered())
			{
				combatant.AddTrait(negatedTraitId);
			}
		}
		negatedTraitIds.Clear();
	}

	public override IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		foreach (AbilityKeyword keyword in base.GetKeywords(abilityData))
		{
			yield return keyword;
		}
		yield return AbilityKeyword.NegateTrait;
	}

	protected override string _ToStringUnique()
	{
		return string.Format("Negate {0} {1}{2}{3} on", _amount, _canNegatePermanents.ToText("Permanent "), _conditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty(), "Trait".Pluralize(_amount?.constantValue ?? 2));
	}
}
