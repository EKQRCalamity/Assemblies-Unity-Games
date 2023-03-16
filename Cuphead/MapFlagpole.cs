using System.Collections;
using UnityEngine;

public class MapFlagpole : AbstractMapLevelDependentEntity
{
	[SerializeField]
	private bool forceNoAppearAnimation;

	protected override bool ReactToDifficultyChange => true;

	protected override bool ReactToGradeChange => true;

	public override void OnConditionMet()
	{
		if (Level.PreviouslyWon || forceNoAppearAnimation)
		{
			Init(difficulty, grade);
		}
	}

	public override void DoTransition()
	{
		if (Level.PreviouslyWon)
		{
			StartCoroutine(shake_cr());
		}
		else
		{
			StartCoroutine(raise_cr());
		}
	}

	public override void OnConditionAlreadyMet()
	{
		Init(difficulty, grade);
	}

	public override void OnConditionNotMet()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Init(Level.Mode difficulty, LevelScoringData.Grade grade)
	{
		if (_levels.Length == 0)
		{
			return;
		}
		string stateName = string.Empty;
		bool flag = false;
		for (int i = 0; i < Level.platformingLevels.Length; i++)
		{
			if (Level.platformingLevels[i] == _levels[0])
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			stateName = ((grade < LevelScoringData.Grade.AMinus) ? "IdleBelowA" : ((grade >= LevelScoringData.Grade.P) ? "IdleP" : "IdleBelowP"));
		}
		else if (difficulty == Level.Mode.Easy)
		{
			stateName = "IdleEasy";
		}
		else if (difficulty == Level.Mode.Normal && grade < LevelScoringData.Grade.AMinus)
		{
			stateName = "IdleNormalBelowA";
		}
		else if (difficulty == Level.Mode.Normal && grade >= LevelScoringData.Grade.AMinus)
		{
			stateName = "IdleNormalA";
		}
		else if (difficulty == Level.Mode.Hard && grade < LevelScoringData.Grade.S)
		{
			stateName = "IdleExpert";
		}
		else if (difficulty == Level.Mode.Hard && grade >= LevelScoringData.Grade.S)
		{
			stateName = "IdleExpertS";
		}
		base.animator.Play(stateName);
		if (forceNoAppearAnimation)
		{
			base.CurrentState = State.Complete;
		}
	}

	private IEnumerator raise_cr()
	{
		if (_levels.Length == 0)
		{
			yield break;
		}
		string trigger = string.Empty;
		bool platformingLevel = false;
		for (int i = 0; i < Level.platformingLevels.Length; i++)
		{
			if (Level.platformingLevels[i] == _levels[0])
			{
				platformingLevel = true;
				break;
			}
		}
		if (platformingLevel)
		{
			trigger = ((grade < LevelScoringData.Grade.AMinus) ? "RaiseBelowA" : ((grade >= LevelScoringData.Grade.P) ? "RaiseP" : "RaiseBelowP"));
		}
		else if (difficulty == Level.Mode.Easy)
		{
			trigger = "RaiseEasy";
		}
		else if (difficulty == Level.Mode.Normal && grade < LevelScoringData.Grade.AMinus)
		{
			trigger = "RaiseNormalBelowA";
		}
		else if (difficulty == Level.Mode.Normal && grade >= LevelScoringData.Grade.AMinus)
		{
			trigger = "RaiseNormalA";
		}
		else if (difficulty == Level.Mode.Hard && grade < LevelScoringData.Grade.S)
		{
			trigger = "RaiseExpert";
		}
		else if (difficulty == Level.Mode.Hard && grade >= LevelScoringData.Grade.S)
		{
			trigger = "RaiseExpertS";
		}
		base.animator.SetTrigger(trigger);
		if (PlayerManager.playerWasChalice[0])
		{
			AudioManager.Play("worldmap_level_raise_flag_chalice");
		}
		else if (PlayerManager.player1IsMugman)
		{
			AudioManager.Play("worldmap_level_raise_flag_mugman");
		}
		else
		{
			AudioManager.Play("world_map_flag_raise");
		}
		yield return base.animator.WaitForAnimationToEnd(this, trigger);
		base.CurrentState = State.Complete;
	}

	private IEnumerator shake_cr()
	{
		base.CurrentState = State.Complete;
		yield return null;
	}
}
