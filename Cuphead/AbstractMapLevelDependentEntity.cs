using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractMapLevelDependentEntity : AbstractMonoBehaviour
{
	public enum State
	{
		Incomplete,
		Transitioning,
		Complete
	}

	[SerializeField]
	protected bool anyLevelPassesCheck;

	[SerializeField]
	protected Levels[] _levels;

	[SerializeField]
	private Vector2 _cameraPosition = Vector2.zero;

	public bool panCamera;

	protected bool firstTimeWon;

	protected bool gradeChanged;

	protected bool difficultyChanged;

	protected Level.Mode difficulty;

	protected LevelScoringData.Grade grade;

	public static List<AbstractMapLevelDependentEntity> RegisteredEntities { get; private set; }

	protected virtual bool ReactToGradeChange => false;

	protected virtual bool ReactToDifficultyChange => false;

	public Vector2 CameraPosition => (Vector2)base.baseTransform.position + _cameraPosition;

	public State CurrentState { get; protected set; }

	public abstract void OnConditionNotMet();

	public abstract void OnConditionMet();

	public abstract void OnConditionAlreadyMet();

	public abstract void DoTransition();

	protected override void Awake()
	{
		base.Awake();
		if (RegisteredEntities == null)
		{
			RegisteredEntities = new List<AbstractMapLevelDependentEntity>();
		}
	}

	protected virtual void Start()
	{
		Check();
	}

	private void OnDestroy()
	{
		RegisteredEntities = null;
	}

	private void Check()
	{
		if (ValidateSucess())
		{
			bool flag = false;
			Levels[] levels = _levels;
			foreach (Levels levels2 in levels)
			{
				if (anyLevelPassesCheck)
				{
					if ((Level.PreviousLevel != levels2 || Level.PreviouslyWon) && PlayerData.Data.GetLevelData(levels2).completed)
					{
						flag = false;
						break;
					}
					if (Level.PreviousLevel == levels2 && Level.Won && !Level.PreviouslyWon)
					{
						flag = true;
					}
				}
				else
				{
					flag = ValidateCondition(levels2);
				}
			}
			if (flag)
			{
				CallOnConditionMet();
			}
			else
			{
				CallOnConditionAlreadyMet();
			}
		}
		else
		{
			CallOnConditionNotMet();
		}
	}

	protected virtual bool ValidateCondition(Levels level)
	{
		if (!Level.Won)
		{
			return false;
		}
		if (Level.PreviousLevel != level)
		{
			return false;
		}
		bool result = false;
		if (!Level.PreviouslyWon && Level.Won)
		{
			firstTimeWon = true;
			result = true;
		}
		if (ReactToGradeChange && Level.Grade > Level.PreviousGrade)
		{
			gradeChanged = true;
			result = true;
		}
		if (ReactToDifficultyChange && Level.Difficulty > Level.PreviousDifficulty)
		{
			difficultyChanged = true;
			result = true;
		}
		return result;
	}

	protected virtual bool ValidateSucess()
	{
		bool result = true;
		Levels[] levels = _levels;
		foreach (Levels levelID in levels)
		{
			PlayerData.PlayerLevelDataObject levelData = PlayerData.Data.GetLevelData(levelID);
			if (!levelData.completed)
			{
				result = false;
				if (!anyLevelPassesCheck)
				{
					break;
				}
				continue;
			}
			difficulty = levelData.difficultyBeaten;
			grade = levelData.grade;
			if (anyLevelPassesCheck)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private void CallOnConditionNotMet()
	{
		CurrentState = State.Incomplete;
		OnConditionNotMet();
	}

	private void CallOnConditionAlreadyMet()
	{
		CurrentState = State.Complete;
		OnConditionAlreadyMet();
	}

	private void CallOnConditionMet()
	{
		CurrentState = State.Incomplete;
		OnConditionMet();
		RegisteredEntities.Add(this);
	}

	public void MapMeetCondition()
	{
		DoTransition();
	}

	private void OnValidate()
	{
		if (_levels == null)
		{
			_levels = new Levels[1];
		}
		if (_levels.Length < 1)
		{
			Array.Resize(ref _levels, 1);
		}
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		Vector2 vector = (Vector2)base.baseTransform.position + _cameraPosition;
		Gizmos.color = Color.black;
		Gizmos.DrawWireSphere(vector, 0.19f);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(vector, 0.2f);
	}
}
