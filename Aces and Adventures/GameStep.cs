using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GameStep
{
	[Flags]
	public enum Flags
	{
		Awake = 1,
		HasBeenEnabled = 2,
		Enabled = 4,
		Canceled = 8,
		Finished = 0x10,
		Ended = 0x20,
		Destroyed = 0x40,
		IsBlocking = 0x80,
		IsParallel = 0x100,
		HasBeenAboutToEnable = 0x200
	}

	public enum GroupType
	{
		Owning,
		Context,
		Root
	}

	private const Flags SHOULD_NOT_UPDATE = Flags.Canceled | Flags.Finished | Flags.Ended | Flags.Destroyed;

	private const Flags SHOULD_NOT_END = Flags.Canceled | Flags.Ended | Flags.Destroyed;

	private IEnumerator _update;

	private GameStepGroup _group;

	private Flags _flags;

	public bool shouldStart
	{
		get
		{
			if (shouldUpdate && _update == null)
			{
				return (_update = Update()) != null;
			}
			return false;
		}
	}

	public IEnumerator update => _update ?? (_update = Update());

	public GameStateView view => GameStateView.Instance;

	protected GameState state => GameState.Instance;

	protected GameManager manager => GameManager.Instance;

	public GameStepGroup group
	{
		get
		{
			return _group;
		}
		private set
		{
			_group = value;
		}
	}

	public GameStepGroup rootGroup => _group?.rootGroup;

	public GameStepGroup contextGroup => _group?.contextGroup;

	public GameStepGroup parentContextGroup => _group?.parentContextGroup;

	public bool awake
	{
		get
		{
			return EnumUtil.HasFlag(_flags, Flags.Awake);
		}
		set
		{
			if (value && _flags != EnumUtil.SetFlag(ref _flags, Flags.Awake, isOn: true))
			{
				Awake();
			}
		}
	}

	public bool enabled
	{
		get
		{
			return EnumUtil.HasFlag(_flags, Flags.Enabled);
		}
		set
		{
			if (_flags != EnumUtil.SetFlag(ref _flags, Flags.Enabled, value))
			{
				if (value && !hasBeenEnabled && (hasBeenEnabled = true))
				{
					OnFirstEnabled();
				}
				if (value)
				{
					OnEnable();
				}
				GameStepStack.Active.SignalEnabledChange(this, value);
				if (!value)
				{
					OnDisable();
				}
			}
		}
	}

	public bool hasStarted => _update != null;

	public bool canceled
	{
		get
		{
			return EnumUtil.HasFlag(_flags, Flags.Canceled);
		}
		private set
		{
			EnumUtil.SetFlag(ref _flags, Flags.Canceled, value);
		}
	}

	public bool finished
	{
		get
		{
			return EnumUtil.HasFlag(_flags, Flags.Finished);
		}
		set
		{
			if (value && _flags != EnumUtil.SetFlag(ref _flags, Flags.Finished, isOn: true))
			{
				OnFinish();
			}
		}
	}

	public bool ended
	{
		get
		{
			return EnumUtil.HasFlag(_flags, Flags.Ended);
		}
		private set
		{
			EnumUtil.SetFlag(ref _flags, Flags.Ended, value);
		}
	}

	public bool destroyed
	{
		get
		{
			return EnumUtil.HasFlag(_flags, Flags.Destroyed);
		}
		private set
		{
			EnumUtil.SetFlag(ref _flags, Flags.Destroyed, value);
		}
	}

	public bool isBlocking
	{
		get
		{
			return EnumUtil.HasFlag(_flags, Flags.IsBlocking);
		}
		set
		{
			EnumUtil.SetFlag(ref _flags, Flags.IsBlocking, value);
		}
	}

	public bool isParallel
	{
		get
		{
			return EnumUtil.HasFlag(_flags, Flags.IsParallel);
		}
		set
		{
			EnumUtil.SetFlag(ref _flags, Flags.IsParallel, value);
		}
	}

	public bool hasBeenEnabled
	{
		get
		{
			return EnumUtil.HasFlag(_flags, Flags.HasBeenEnabled);
		}
		private set
		{
			if (value)
			{
				EnumUtil.Add(ref _flags, Flags.HasBeenEnabled);
			}
		}
	}

	public bool hasBeenAboutToEnable
	{
		get
		{
			return EnumUtil.HasFlag(_flags, Flags.HasBeenAboutToEnable);
		}
		private set
		{
			if (value)
			{
				EnumUtil.Add(ref _flags, Flags.HasBeenAboutToEnable);
			}
		}
	}

	public bool isActiveStep
	{
		get
		{
			if (GameStepStack.Active.activeStep == this)
			{
				if (!isBlocking)
				{
					return !GameStepStack.Active.hasPendingSteps;
				}
				return true;
			}
			return false;
		}
	}

	public bool shouldUpdate
	{
		get
		{
			if (shouldBeCanceled)
			{
				Cancel();
			}
			return _shouldUpdate;
		}
	}

	protected bool _shouldUpdate => (_flags & (Flags.Canceled | Flags.Finished | Flags.Ended | Flags.Destroyed)) == 0;

	protected bool _shouldEnd => (_flags & (Flags.Canceled | Flags.Ended | Flags.Destroyed)) == 0;

	public virtual bool canSafelyCancelStack => false;

	public virtual bool canInspect => canSafelyCancelStack;

	public virtual bool countsTowardsStrategyTime => false;

	protected virtual bool shouldBeCanceled => false;

	public GameStepGroup this[GroupType groupType] => groupType switch
	{
		GroupType.Context => contextGroup, 
		GroupType.Root => rootGroup, 
		_ => group, 
	};

	public IEnumerable<float> Wait(float time)
	{
		float num;
		do
		{
			yield return time;
			time = (num = time - Time.deltaTime);
		}
		while (num > 0f);
	}

	public T Group<T>() where T : GameStepGroup
	{
		GameStepGroup gameStepGroup = _group;
		if (gameStepGroup == null)
		{
			return null;
		}
		return gameStepGroup.Group<T>();
	}

	protected void _ClearGlows()
	{
		ATargetView.ClearGlowRequestsFrom<ATarget>(this);
	}

	protected void _ClearGlowsFor<T>() where T : ATarget
	{
		ATargetView.ClearGlowRequestsFrom<T>(this);
	}

	protected virtual void Awake()
	{
	}

	public void AboutToEnable()
	{
		if (!hasBeenAboutToEnable && (hasBeenAboutToEnable = true))
		{
			OnAboutToEnableForFirstTime();
		}
		OnAboutToEnable();
	}

	protected virtual void OnAboutToEnable()
	{
	}

	protected virtual void OnAboutToEnableForFirstTime()
	{
	}

	protected virtual void OnFirstEnabled()
	{
	}

	protected virtual void OnEnable()
	{
	}

	public virtual void Start()
	{
	}

	protected virtual IEnumerator Update()
	{
		yield break;
	}

	protected virtual void LateUpdate()
	{
	}

	protected virtual void OnFinish()
	{
	}

	protected virtual void End()
	{
	}

	public virtual void OnCompletedSuccessfully()
	{
	}

	protected virtual void OnCanceled()
	{
	}

	protected virtual void OnDisable()
	{
	}

	protected virtual void OnDestroy()
	{
	}

	public GameStep SetGroup(GameStepGroup gameStepGroup)
	{
		if (group == null)
		{
			group = gameStepGroup;
		}
		else
		{
			group.group = gameStepGroup;
		}
		if (gameStepGroup.isBlocking)
		{
			isBlocking = true;
		}
		return this;
	}

	public GameStep AppendStep(GameStep step)
	{
		if (group == null)
		{
			return GameStepStack.Active.Push(step);
		}
		return group.AppendStep(step);
	}

	public GameStep ParallelStep(GameStep step)
	{
		return GameStepStack.Active.ParallelProcess(step);
	}

	public GameStep ParallelChain(GameStep stepToChain)
	{
		return GameStepStack.Active.ChainParallelProcess(this, stepToChain);
	}

	public GameStepGroup AppendGroup(GameStepGroup groupToAppend)
	{
		foreach (GameStep item in groupToAppend)
		{
			AppendStep(item);
		}
		return groupToAppend;
	}

	public GameStep TransitionTo(GameStep step)
	{
		finished = true;
		return GameStepStack.Active.Push(step);
	}

	public GameStepGroup TransitionTo(GameStepGroup gameStepGroup)
	{
		finished = true;
		GameStepStack.Active.Push(gameStepGroup);
		return gameStepGroup;
	}

	public IEnumerable<GameStep> GetPreviousSteps(GroupType groupType = GroupType.Owning)
	{
		if (group == null)
		{
			return Enumerable.Empty<GameStep>();
		}
		return this[groupType].GetPreviousSteps(this);
	}

	public IEnumerable<GameStep> GetNextSteps(GroupType groupType = GroupType.Owning)
	{
		if (group == null)
		{
			return Enumerable.Empty<GameStep>();
		}
		return this[groupType].GetNextSteps(this);
	}

	public bool DoLateUpdate()
	{
		LateUpdate();
		return true;
	}

	public void Cancel()
	{
		if (_shouldUpdate && (canceled = true))
		{
			OnCanceled();
		}
	}

	public void CancelGroup(GroupType groupType = GroupType.Owning)
	{
		this[groupType]?.Cancel();
	}

	public void CancelNextSteps(GroupType groupType = GroupType.Owning)
	{
		foreach (GameStep item in Pools.UseKeepItemList(GetNextSteps(groupType)))
		{
			item.Cancel();
		}
	}

	public void FinishGroup(GroupType groupType = GroupType.Owning)
	{
		this[groupType]?.Finish();
	}

	public bool IsEnding()
	{
		finished = true;
		if (!_shouldEnd)
		{
			return false;
		}
		End();
		return ended = true;
	}

	public void Destroy()
	{
		if (!destroyed && (destroyed = true))
		{
			OnDestroy();
		}
	}
}
