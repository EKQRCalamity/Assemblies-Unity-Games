using ProtoBuf;

[ProtoContract]
[UIField("Discard Resource Card", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Player")]
public class DiscardResourceCardAction : APlayerAction
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIDeepValueChange]
	private DynamicNumber _discardCount;

	[ProtoMember(2)]
	[UIField]
	private bool _mulligan;

	[ProtoMember(3)]
	[UIField]
	private bool _removeDiscardedFromDeck;

	protected override bool _requiresUserInputAfterTargeting => true;

	protected override void _Tick(ActionContext context, Player player)
	{
		context.gameState.stack.Append(new GameStepDiscardResourceChoice(_discardCount.GetValue(context), _mulligan ? DiscardReason.Mulligan : ((!context.hasAbility) ? DiscardReason.PayingForItem : ((context.actor == player) ? DiscardReason.PlayerEffect : DiscardReason.EnemyEffect)), _removeDiscardedFromDeck));
	}

	protected override string _ToStringUnique()
	{
		return string.Format("{0}{1} {2} {3} for", _mulligan.ToText("Mulligan", "Discard"), _removeDiscardedFromDeck.ToText(" and Remove"), _discardCount, "Card".Pluralize(_discardCount?.constantValue ?? 2));
	}
}
