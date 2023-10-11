using System;
using System.Collections;
using System.Collections.Generic;

public abstract class GameStepGroup : IEnumerable<GameStep>, IEnumerable
{
	private List<GameStep> _steps;

	private GameStepGroup _group;

	public GameState gameState => GameState.Instance;

	public GameStepGroup group
	{
		get
		{
			return _group;
		}
		set
		{
			if (value == null)
			{
				return;
			}
			if (_group != null)
			{
				GameStepGroup gameStepGroup = this;
				while (gameStepGroup._group != null)
				{
					gameStepGroup = gameStepGroup._group;
					if (gameStepGroup == value)
					{
						return;
					}
				}
				gameStepGroup._group = value;
			}
			else
			{
				_group = value;
			}
		}
	}

	public bool hasStarted
	{
		get
		{
			if (!_steps.IsNullOrEmpty())
			{
				return _steps[0].hasStarted;
			}
			return false;
		}
	}

	public bool finished
	{
		get
		{
			if (_steps == null)
			{
				return false;
			}
			for (int num = _steps.Count - 1; num >= 0; num--)
			{
				if (!_steps[num].destroyed)
				{
					return false;
				}
			}
			return true;
		}
	}

	public GameStepGroup rootGroup
	{
		get
		{
			GameStepGroup gameStepGroup = this;
			while (gameStepGroup.group != null)
			{
				gameStepGroup = gameStepGroup.group;
			}
			return gameStepGroup;
		}
	}

	public GameStepGroup contextGroup
	{
		get
		{
			GameStepGroup gameStepGroup = this;
			while (!gameStepGroup._changesContext && gameStepGroup.group != null)
			{
				gameStepGroup = gameStepGroup.group;
			}
			return gameStepGroup;
		}
	}

	public GameStepGroup parentContextGroup => contextGroup.group?.contextGroup;

	public virtual bool isBlocking => false;

	protected virtual bool _changesContext => false;

	static GameStepGroup()
	{
		Pools.CreatePoolList<GameStep>();
	}

	public T Group<T>() where T : GameStepGroup
	{
		GameStepGroup gameStepGroup = this;
		do
		{
			if (gameStepGroup is T result)
			{
				return result;
			}
		}
		while ((gameStepGroup = gameStepGroup.group) != null);
		return null;
	}

	private void _InsertStep(GameStep step)
	{
		int num = 0;
		while (true)
		{
			if (num < _steps.Count)
			{
				if (!_steps[num].hasStarted && _steps[num].awake)
				{
					_steps.Insert(num, step);
					break;
				}
				num++;
				continue;
			}
			_steps.Add(step);
			break;
		}
		group?._InsertStep(step);
	}

	private bool _InContext(GameStep step, GameStepGroup context)
	{
		return step.group.contextGroup == context;
	}

	protected abstract IEnumerable<GameStep> _GetSteps();

	protected virtual void _OnDestroy()
	{
	}

	public IEnumerable<GameStep> GetPreviousSteps(GameStep step)
	{
		GameStepGroup context = contextGroup;
		if (context == null)
		{
			yield break;
		}
		for (int x = _steps.IndexOf(step) - 1; x >= 0; x--)
		{
			if (_InContext(_steps[x], context))
			{
				yield return _steps[x];
			}
		}
	}

	public IEnumerable<T> GetPreviousStepsOfTypeUntilType<T, U>(GameStep fromStep) where T : GameStep where U : GameStep
	{
		GameStepGroup context = contextGroup;
		for (int x = _steps.IndexOf(fromStep) - 1; x >= 0; x--)
		{
			GameStep step = _steps[x];
			if (_InContext(step, context))
			{
				if (step is T val)
				{
					yield return val;
				}
				if (step is U)
				{
					break;
				}
			}
		}
	}

	public IEnumerable<GameStep> GetNextSteps(GameStep step)
	{
		GameStepGroup context = contextGroup;
		if (context == null)
		{
			yield break;
		}
		for (int x = _steps.IndexOf(step) + 1; x < _steps.Count; x++)
		{
			if (_InContext(_steps[x], context))
			{
				yield return _steps[x];
			}
		}
	}

	public void Cancel()
	{
		GameStepGroup gameStepGroup = contextGroup;
		if (gameStepGroup == null)
		{
			return;
		}
		foreach (GameStep step in _steps)
		{
			if (_InContext(step, gameStepGroup))
			{
				step.Cancel();
			}
		}
	}

	public void Cancel(Func<GameStep, bool> shouldCancel)
	{
		GameStepGroup gameStepGroup = contextGroup;
		if (gameStepGroup == null)
		{
			return;
		}
		foreach (GameStep step in _steps)
		{
			if (_InContext(step, gameStepGroup) && shouldCancel(step))
			{
				step.Cancel();
			}
		}
	}

	public void Finish()
	{
		GameStepGroup gameStepGroup = contextGroup;
		if (gameStepGroup == null)
		{
			return;
		}
		foreach (GameStep step in _steps)
		{
			if (_InContext(step, gameStepGroup))
			{
				step.finished = true;
			}
		}
	}

	public GameStep AppendStep(GameStep step)
	{
		_InsertStep(step);
		return GameState.Instance.stack.Push(step.SetGroup(this));
	}

	public void Destroy()
	{
		_OnDestroy();
		Pools.Repool(ref _steps);
		gameState?.stack.tagMap.ClearTags(this);
	}

	public T AddTag<T>(T tag)
	{
		return gameState.stack.tagMap.AddTag(this, tag);
	}

	public T GetTag<T>()
	{
		return gameState.stack.tagMap.GetTag<T>(this);
	}

	public T AddHashTag<T>(T tag)
	{
		return gameState.stack.tagMap.AddHashTag(this, tag);
	}

	public bool HasHashTag<T>(T tag)
	{
		return gameState.stack.tagMap.HasHashTag(this, tag);
	}

	public IEnumerator<GameStep> GetEnumerator()
	{
		if (_steps != null)
		{
			foreach (GameStep step in _steps)
			{
				yield return step;
			}
			yield break;
		}
		_steps = Pools.Unpool<List<GameStep>>();
		foreach (GameStep item in _GetSteps())
		{
			yield return _steps.AddReturn(item.SetGroup(this));
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
