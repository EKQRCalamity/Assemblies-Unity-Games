using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField("Draw Ability Card", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Player")]
public class DrawAbilityCardAction : APlayerAction
{
	[ProtoMember(2)]
	[UIField(excludedValuesMethod = "_ExcludeDrawFrom")]
	[UIHorizontalLayout("D")]
	private Ability.Pile _drawFrom;

	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIDeepValueChange]
	private DynamicNumber _drawCount;

	[ProtoMember(3)]
	[UIField(excludedValuesMethod = "_ExcludeCanOverdraw")]
	private bool _canOverdraw;

	[ProtoMember(4, OverwriteList = true)]
	[UIField(tooltip = "Allows drawing cards that meet a certain set of conditions only.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.AAbility> _filters;

	[ProtoMember(5)]
	[UIField(validateOnChange = true, excludedValuesMethod = "_ExcludeDrawTo")]
	[UIHorizontalLayout("D")]
	[DefaultValue(Ability.Pile.Hand)]
	private Ability.Pile _drawTo = Ability.Pile.Hand;

	[ProtoMember(6)]
	[UIField]
	[UIHideIf("_hideShuffleInto")]
	private bool _shuffleInto;

	private bool _hasFilters => !_filters.IsNullOrEmpty();

	private bool _shuffle
	{
		get
		{
			if (_shuffleInto)
			{
				return !_hideShuffleInto;
			}
			return false;
		}
	}

	private bool _hideShuffleInto
	{
		get
		{
			if (_drawTo != Ability.Pile.Hand)
			{
				return !_hasFilters;
			}
			return true;
		}
	}

	private int _DrawCount(ActionContext context)
	{
		int num = Math.Min(_drawCount.GetValue(context), (_canOverdraw || _drawTo != Ability.Pile.Hand) ? int.MaxValue : context.gameState.player.abilityHandSpace);
		if (!_hasFilters)
		{
			return num;
		}
		return Math.Min(context.gameState.player.abilityDeck.GetCards(_drawFrom).Count((Ability a) => _filters.All<Condition.AAbility>(context.SetTarget(a))), num);
	}

	protected override bool _ShouldActUnique(ActionContext context, Player player)
	{
		if (_drawFrom != 0 || (!isApplied && !player.abilityDeck.CanDraw()))
		{
			return player.abilityDeck.Count(_drawFrom) > 0;
		}
		return true;
	}

	public override bool ShouldTick(ActionContext context)
	{
		if (base.ShouldTick(context) && _DrawCount(context) > 0)
		{
			if (_drawFrom != 0)
			{
				return context.gameState.player.abilityDeck.Count(_drawFrom) > 0;
			}
			return context.gameState.player.abilityDeck.CanDraw();
		}
		return false;
	}

	protected override void _Tick(ActionContext context, Player player)
	{
		context.gameState.stack.Push(_hasFilters ? ((IdDeck<Ability.Pile, Ability>.AGameStep)context.gameState.player.abilityDeck.DrawFilteredStep((Ability a) => _filters.All<Condition.AAbility>(context.SetTarget(a)), _DrawCount(context), _drawFrom, _drawTo, _shuffle)) : ((IdDeck<Ability.Pile, Ability>.AGameStep)context.gameState.player.abilityDeck.DrawStep(_DrawCount(context), _drawFrom, _drawTo)));
	}

	protected override string _ToStringUnique()
	{
		return string.Format("{0} {1}{2} Ability {3} {4}{5}for", _canOverdraw.ToText("Overdraw", _shuffle.ToText("Shuffle", "Draw")), _drawCount, _filters.ToStringSmart(" & ").SizeIfNotEmpty().PreSpaceIfNotEmpty(), "Card".Pluralize(_drawCount?.constantValue ?? 2), (_drawFrom != Ability.Pile.Draw).ToText("from " + EnumUtil.FriendlyName(_drawFrom) + " "), (_drawTo != Ability.Pile.Hand).ToText("into " + EnumUtil.FriendlyName(_drawTo) + " "));
	}

	public override IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		foreach (AbilityKeyword keyword in base.GetKeywords(abilityData))
		{
			yield return keyword;
		}
		if (_canOverdraw)
		{
			yield return AbilityKeyword.Overdraw;
		}
	}

	private bool _ExcludeDrawFrom(Ability.Pile pile)
	{
		if (pile != 0)
		{
			return pile != Ability.Pile.Discard;
		}
		return false;
	}

	private bool _ExcludeDrawTo(Ability.Pile pile)
	{
		if (pile != Ability.Pile.Hand && pile != 0)
		{
			return pile != Ability.Pile.Discard;
		}
		return false;
	}

	private bool _ExcludeCanOverdraw(bool canOverdraw)
	{
		return _drawTo != Ability.Pile.Hand && canOverdraw;
	}
}
