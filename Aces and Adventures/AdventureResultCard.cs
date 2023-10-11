using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class AdventureResultCard : ATarget, IAdventureCard
{
	[ProtoMember(1)]
	private int _experience;

	[ProtoMember(2)]
	private int _totalTime;

	[ProtoMember(3)]
	private int _strategyTime;

	[ProtoMember(4)]
	private AdventureCompletionRank _rank;

	[ProtoMember(5)]
	private int? _nextRank;

	public AdventureCompletionRank rank => _rank;

	public ATarget adventureCard => this;

	public AdventureCard.Pile pileToTransferToOnDraw => AdventureCard.Pile.TurnOrder;

	public AdventureCard.Pile pileToTransferToOnSelect => AdventureCard.Pile.Discard;

	public GameStep selectTransferStep => null;

	public AdventureCard.Common adventureCardCommon
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string name => MessageData.Instance.game.adventureResult[AdventureResultType.ResultTitle].SetVariables(("Rank", MessageData.Instance.game.adventureCompletionRank[rank])).Localize();

	public string description => MessageData.Instance.game.adventureResult[AdventureResultType.ResultDescription].SetVariables(("Experience", _experience), ("Time", TimeSpan.FromSeconds(_totalTime).ToStringSimple()), ("StrategyTime", TimeSpan.FromSeconds(_strategyTime).ToStringSimple()), ("HasNextRank", _nextRank.HasValue), ("NextRank", TimeSpan.FromSeconds(_nextRank.GetValueOrDefault()).ToStringSimple())).Localize();

	public CroppedImageRef image => rank.GetImage();

	public IEnumerable<GameStep> selectedGameSteps
	{
		get
		{
			yield break;
		}
	}

	public ResourceBlueprint<GameObject> blueprint => AdventureResultCardView.Blueprint;

	private AdventureResultCard()
	{
	}

	public AdventureResultCard(GameState state)
	{
		_experience = state.experience;
		_totalTime = state.totalTime;
		_strategyTime = state.strategyTime;
		_rank = state.adventure.data.GetCompletionRank(_strategyTime);
		AdventureCompletionRank? nextCompletionRank = base.gameState.adventure.data.GetNextCompletionRank(_strategyTime);
		int? nextRank;
		if (nextCompletionRank.HasValue)
		{
			AdventureCompletionRank valueOrDefault = nextCompletionRank.GetValueOrDefault();
			nextRank = state.adventure.data.GetCompletionTime(valueOrDefault);
		}
		else
		{
			nextRank = null;
		}
		_nextRank = nextRank;
	}
}
