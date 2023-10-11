using System;
using UnityEngine;

public class ExileDeckLayout : ADeckLayout<ExilePile, ATarget>
{
	public ACardLayout playerResource;

	public ACardLayout enemyResource;

	public ACardLayout character;

	public ACardLayout clearGameState;

	[Header("Layouts not backed by a pile")]
	public ACardLayout playerResourceGenerate;

	public ACardLayout enemyResourceGenerate;

	protected override ACardLayout this[ExilePile? pile]
	{
		get
		{
			return pile switch
			{
				ExilePile.PlayerResource => playerResource, 
				ExilePile.EnemyResource => enemyResource, 
				ExilePile.Character => character, 
				ExilePile.ClearGameState => clearGameState, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case ExilePile.PlayerResource:
					playerResource = value;
					break;
				case ExilePile.EnemyResource:
					enemyResource = value;
					break;
				case ExilePile.Character:
					character = value;
					break;
				case ExilePile.ClearGameState:
					clearGameState = value;
					break;
				default:
					throw new ArgumentOutOfRangeException("pile", pile, null);
				}
			}
		}
	}

	protected override CardLayoutElement _CreateView(ATarget value)
	{
		if (value is IAdventureCard)
		{
			return AdventureTargetView.Create(value);
		}
		if (value is ResourceCard card)
		{
			return ResourceCardView.Create(card);
		}
		if (value is LevelUpReward reward)
		{
			return LevelUpRewardView.Create(reward);
		}
		if (value is Leaderboard leaderboard)
		{
			return LeaderboardView.Create(leaderboard);
		}
		return null;
	}
}
