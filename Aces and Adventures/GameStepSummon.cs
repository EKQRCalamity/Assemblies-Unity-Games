using System;
using System.Collections;

public class GameStepSummon : GameStepAAction
{
	private TurnOrderSpace _targetSpace;

	private PoolKeepItemListHandle<AEntity> _entitiesToLeftOfSpace;

	private PoolKeepItemListHandle<AEntity> _entitiesToRightOfSpace;

	public GameStepSummon(AAction action, ActionContext context, TurnOrderSpace targetSpace)
		: base(action, context)
	{
		_targetSpace = targetSpace;
	}

	public override void Start()
	{
		_entitiesToLeftOfSpace = _targetSpace.GetEntitiesToLeftOf();
		_entitiesToRightOfSpace = _targetSpace.GetEntitiesToRightOf();
		Ability activeSummon = base.state.activeSummon;
		if (activeSummon != null)
		{
			base.state.SignalSummonReplaced(activeSummon, base.context.ability);
		}
	}

	protected override IEnumerator Update()
	{
		Ability activeSummon = base.state.activeSummon;
		if (activeSummon != null)
		{
			yield return AppendStep(new GameStepRemoveSummon(base.action, base.context, activeSummon, isBeingReplaced: true));
		}
	}

	protected override void End()
	{
		using PoolKeepItemHashSetHandle<AEntity> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet(base.state.turnOrderQueue);
		int value = base.context.gameState.adventureDeck.Count(AdventureCard.Pile.TurnOrder);
		for (int i = 0; i < Math.Max(_entitiesToLeftOfSpace.Count, _entitiesToRightOfSpace.Count); i++)
		{
			if (i < _entitiesToRightOfSpace.Count && poolKeepItemHashSetHandle.Contains(_entitiesToRightOfSpace[i]))
			{
				value = base.state.GetTurnOrder(_entitiesToRightOfSpace[i]);
				break;
			}
			if (i < _entitiesToLeftOfSpace.Count && poolKeepItemHashSetHandle.Contains(_entitiesToLeftOfSpace[i]))
			{
				value = base.state.GetTurnOrder(_entitiesToLeftOfSpace[i]) + 1;
				break;
			}
		}
		base.state.adventureDeck.Transfer(base.context.ability, AdventureCard.Pile.TurnOrder, value);
		base.context.ability.hasTakenTurn.value = false;
		AppendStep(new GameStepWaitForCardTransition(base.context.ability.view));
		base.state.SignalSummonPlaced(base.context.ability);
	}

	protected override void OnDestroy()
	{
		Pools.Repool(ref _entitiesToLeftOfSpace);
		Pools.Repool(ref _entitiesToRightOfSpace);
	}
}
