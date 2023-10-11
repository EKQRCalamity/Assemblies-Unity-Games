using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField("Add Enemy", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Transfers an enemy card from encounter active pile into turn order.", category = "Specialized")]
public class AddEnemyAction : AAction
{
	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	private Target.TurnOrderRelative _target;

	[ProtoMember(2, OverwriteList = true)]
	[UIField]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<Condition.Combatant> _enemyToAddConditions;

	[ProtoMember(3)]
	[UIField(tooltip = "Should added enemy enter turn order tapped?")]
	[DefaultValue(true)]
	private bool _tapped = true;

	public override Target target => _target;

	protected override bool _canTick => false;

	protected override bool _canHaveDuration => false;

	public override bool isApplied => false;

	private AEntity _GetEnemyToAdd(ActionContext context)
	{
		return context.gameState.adventureDeck.GetCards(AdventureCard.Pile.ActiveHand).OfType<Enemy>().FirstOrDefault((Enemy enemy) => _enemyToAddConditions.All(context.SetTarget(enemy)));
	}

	protected override bool _ShouldAct(ActionContext context)
	{
		return _GetEnemyToAdd(context) != null;
	}

	protected override void _Tick(ActionContext context)
	{
		AEntity aEntity = _GetEnemyToAdd(context);
		if (_tapped)
		{
			aEntity.tapped.value = true;
		}
		else
		{
			aEntity.OnRoundStart();
		}
		context.gameState.adventureDeck.Transfer(aEntity, AdventureCard.Pile.TurnOrder, (context.target as TurnOrderSpace)?.index);
		context.gameState.stack.activeStep.AppendStep(new GameStepWaitForCardTransition(aEntity.view));
	}

	protected override string _ToStringUnique()
	{
		return "Add " + _tapped.ToText("<i>Tapped</i> ") + _enemyToAddConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + "Enemy";
	}
}
