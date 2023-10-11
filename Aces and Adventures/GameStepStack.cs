using System;
using System.Collections.Generic;

public class GameStepStack
{
	private static readonly List<GameStepStack> _Stacks;

	private readonly Stack<GameStep> _stack = new Stack<GameStep>();

	private readonly Stack<GameStep> _pendingSteps = new Stack<GameStep>();

	private readonly Dictionary<GameStepGroup, Queue<GameStep>> _queues = new Dictionary<GameStepGroup, Queue<GameStep>>();

	private readonly List<GameStep> _parallelProcesses = new List<GameStep>();

	private readonly Dictionary<GameStep, Queue<GameStep>> _chainedParallelProcesses = new Dictionary<GameStep, Queue<GameStep>>();

	private readonly TagMap _tagMap = new TagMap();

	private GameStep _previousActiveStep;

	public static GameStepStack Active => _Stacks.LastRef();

	public GameStep activeStep => _stack.FirstOrDefault();

	public bool hasPendingSteps => _pendingSteps.Count > 0;

	public TagMap tagMap => _tagMap;

	public event Action<GameStep, bool> onEnabledChange;

	static GameStepStack()
	{
		_Stacks = new List<GameStepStack>();
		Pools.CreatePoolQueue<GameStep>();
	}

	private static GameStepStack _Register(GameStepStack stack)
	{
		_Unregister(stack);
		return _Stacks.AddReturn(stack);
	}

	private static void _Unregister(GameStepStack stack)
	{
		_Stacks.Remove(stack);
	}

	private void _Pop()
	{
		if (_stack.Count == 0)
		{
			return;
		}
		GameStep gameStep = _stack.Pop();
		if (!gameStep.canceled)
		{
			gameStep.OnCompletedSuccessfully();
		}
		gameStep.enabled = false;
		gameStep.Destroy();
		if (_queues.Count > 0)
		{
			GameStepGroup rootGroup = gameStep.rootGroup;
			if (rootGroup != null && rootGroup.finished && _queues.ContainsKey(rootGroup))
			{
				Queue<GameStep> queue = _queues[rootGroup];
				foreach (GameStep item in queue)
				{
					Push(item);
				}
				Pools.Repool(queue);
				_queues.Remove(rootGroup);
			}
		}
		GameStepGroup group = gameStep.group;
		if (group != null && group.finished)
		{
			gameStep.group.Destroy();
		}
	}

	private void _UpdateParallelProcesses()
	{
		for (int num = _parallelProcesses.Count - 1; num >= 0; num--)
		{
			GameStep gameStep = _parallelProcesses[num];
			if (gameStep.shouldStart)
			{
				gameStep.Start();
			}
			if (!gameStep.shouldUpdate || !gameStep.update.MoveNext() || !gameStep.DoLateUpdate())
			{
				gameStep.IsEnding();
				if (!gameStep.canceled)
				{
					gameStep.OnCompletedSuccessfully();
				}
				gameStep.enabled = false;
				gameStep.Destroy();
				_parallelProcesses.RemoveAt(num);
				Queue<GameStep> valueOrDefault = _chainedParallelProcesses.GetValueOrDefault(gameStep);
				if (valueOrDefault != null)
				{
					foreach (GameStep item in valueOrDefault)
					{
						ParallelProcess(item);
					}
					if (_chainedParallelProcesses.Remove(gameStep))
					{
						Pools.Repool(valueOrDefault);
					}
				}
			}
		}
	}

	public GameStep Push(GameStep step)
	{
		_pendingSteps.Push(step);
		return step;
	}

	public GameStep Append(GameStep step)
	{
		GameStep gameStep = activeStep;
		if (gameStep == null)
		{
			return Push(step);
		}
		return gameStep.AppendStep(step);
	}

	public GameStep Queue(GameStep step)
	{
		GameStepGroup gameStepGroup = activeStep?.rootGroup;
		if (gameStepGroup == null)
		{
			return Push(step);
		}
		(_queues.GetValueOrDefault(gameStepGroup) ?? (_queues[gameStepGroup] = Pools.Unpool<Queue<GameStep>>())).Enqueue(step);
		return step;
	}

	public GameStep ParallelProcess(GameStep step)
	{
		step.isParallel = true;
		step.awake = true;
		step.AboutToEnable();
		step.enabled = true;
		_parallelProcesses.Add(step);
		return step;
	}

	public GameStep ParallelProcesses(params GameStep[] steps)
	{
		if (steps.Length == 0)
		{
			return null;
		}
		ParallelProcess(steps[0]);
		for (int i = 1; i < steps.Length; i++)
		{
			ChainParallelProcess(steps[i - 1], steps[i]);
		}
		return steps[^1];
	}

	public GameStep ChainParallelProcess(GameStep parentGameStep, GameStep gameStepToChain)
	{
		return (_chainedParallelProcesses.GetValueOrDefault(parentGameStep) ?? (_chainedParallelProcesses[parentGameStep] = Pools.Unpool<Queue<GameStep>>())).EnqueueAndReturn(gameStepToChain);
	}

	public void Push(GameStepGroup group)
	{
		foreach (GameStep item in group)
		{
			Push(item);
		}
	}

	public void Append(GameStepGroup group)
	{
		foreach (GameStep item in group)
		{
			Append(item);
		}
	}

	public void Update()
	{
		_UpdateParallelProcesses();
		while (true)
		{
			if (!IsActiveStep())
			{
				if (activeStep != null)
				{
					activeStep.enabled = false;
				}
				foreach (GameStep pendingStep in _pendingSteps)
				{
					_stack.Push(pendingStep);
					pendingStep.awake = true;
				}
				_pendingSteps.Clear();
			}
			GameStep gameStep = activeStep;
			if (gameStep == null)
			{
				break;
			}
			if (gameStep != _previousActiveStep)
			{
				_previousActiveStep = gameStep;
				if (gameStep.shouldUpdate)
				{
					if (!gameStep.enabled)
					{
						gameStep.AboutToEnable();
						if (!IsActiveStep())
						{
							continue;
						}
					}
					if (gameStep.shouldUpdate)
					{
						gameStep.enabled = true;
					}
				}
			}
			if (!IsActiveStep())
			{
				continue;
			}
			if (gameStep.shouldStart)
			{
				gameStep.Start();
			}
			if (IsActiveStep())
			{
				if (gameStep.shouldUpdate && gameStep.update.MoveNext() && IsActiveStep() && gameStep.DoLateUpdate())
				{
					break;
				}
				if (IsActiveStep() && (!gameStep.IsEnding() || IsActiveStep()))
				{
					_Pop();
				}
			}
		}
		bool IsActiveStep()
		{
			if (_pendingSteps.Count != 0)
			{
				return activeStep?.isBlocking ?? false;
			}
			return true;
		}
	}

	public IEnumerable<GameStep> GetSteps()
	{
		return _stack;
	}

	public PoolKeepItemHashSetHandle<T> GetGroups<T>() where T : GameStepGroup
	{
		PoolKeepItemHashSetHandle<T> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<T>();
		foreach (GameStep item in _stack)
		{
			if (item.group is T value)
			{
				poolKeepItemHashSetHandle.Add(value);
			}
		}
		return poolKeepItemHashSetHandle;
	}

	public void Cancel()
	{
		foreach (Queue<GameStep> value in _queues.Values)
		{
			Pools.Repool(value);
		}
		_queues.Clear();
		foreach (Queue<GameStep> value2 in _chainedParallelProcesses.Values)
		{
			Pools.Repool(value2);
		}
		_chainedParallelProcesses.Clear();
		while (_stack.Count > 0)
		{
			foreach (GameStep parallelProcess in _parallelProcesses)
			{
				parallelProcess.Cancel();
			}
			foreach (GameStep item in _stack)
			{
				item.Cancel();
			}
			Update();
		}
	}

	public void SignalEnabledChange(GameStep step, bool enabled)
	{
		this.onEnabledChange?.Invoke(step, enabled);
	}

	public GameStepStack Register()
	{
		return _Register(this);
	}

	public void Unregister()
	{
		_Unregister(this);
	}
}
