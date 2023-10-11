using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
[UIField("Draw Resource Card", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Player")]
public class DrawResourceCardAction : APlayerAction
{
	[ProtoMember(2)]
	[UIField(excludedValuesMethod = "_ExcludeDrawFrom")]
	private ResourceCard.Pile _drawFrom;

	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIDeepValueChange]
	private DynamicNumber _drawCount;

	[ProtoMember(3)]
	[UIField]
	private bool _canOverdraw;

	protected override bool _ShouldActUnique(ActionContext context, Player player)
	{
		if (_drawFrom != 0 || (!isApplied && !player.resourceDeck.CanDraw()))
		{
			return player.resourceDeck.Count(_drawFrom) > 0;
		}
		return true;
	}

	public override bool ShouldTick(ActionContext context)
	{
		if (base.ShouldTick(context) && context.gameState.player.GetDrawCount(_drawCount.GetValue(context), _canOverdraw) > 0)
		{
			if (_drawFrom != 0)
			{
				return context.gameState.player.resourceDeck.Count(_drawFrom) > 0;
			}
			return context.gameState.player.resourceDeck.CanDraw();
		}
		return false;
	}

	protected override void _Tick(ActionContext context, Player player)
	{
		context.gameState.stack.Push(context.gameState.player.DrawStep(_drawCount.GetValue(context), _drawFrom, _canOverdraw));
	}

	protected override string _ToStringUnique()
	{
		return string.Format("{0} {1} Resource {2} {3}for", _canOverdraw.ToText("Overdraw", "Draw"), _drawCount, "Card".Pluralize(_drawCount?.constantValue ?? 2), (_drawFrom != ResourceCard.Pile.DrawPile).ToText("From " + EnumUtil.FriendlyName(_drawFrom) + " "));
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

	private bool _ExcludeDrawFrom(ResourceCard.Pile pile)
	{
		if (pile != 0)
		{
			return pile != ResourceCard.Pile.DiscardPile;
		}
		return false;
	}
}
