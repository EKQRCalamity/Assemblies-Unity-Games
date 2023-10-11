using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class Job
{
	private class RegisterData
	{
		public Action register;

		public Action unregister;

		public bool unregisterOnBlock;

		public bool unregisterOnPause;

		public bool registered;

		public RegisterData(Action register, Action unregister, bool unregisterOnBlock, bool unregisterOnPause)
		{
			this.register = register;
			this.unregister = unregister;
			this.unregisterOnBlock = unregisterOnBlock;
			this.unregisterOnPause = unregisterOnPause;
			registered = false;
		}

		public void UpdateRegistration(bool blocked, bool paused)
		{
			if (registered)
			{
				if ((blocked && unregisterOnBlock) || (paused && unregisterOnPause))
				{
					unregister();
					registered = false;
				}
			}
			else if ((!blocked || !unregisterOnBlock) && (!paused || !unregisterOnPause))
			{
				register();
				registered = true;
			}
		}
	}

	private class ConnectionData
	{
		private Job _thisJob;

		private Job _nextJob;

		private ConnectionType _type;

		private bool _syncPause;

		private bool _syncBlock;

		private ContinueIfType _continueIf;

		private bool _immediate;

		public Job previousJob => _thisJob;

		public bool doStartLogic
		{
			get
			{
				if (_type != 0)
				{
					return _type != ConnectionType.Afterward;
				}
				return false;
			}
		}

		public ConnectionData(Job thisJob, ConnectionType type, bool syncPause = false, bool syncBlock = false, ContinueIfType continueIf = ContinueIfType.Successful, bool immediate = false)
		{
			_thisJob = thisJob;
			_type = type;
			_syncPause = syncPause;
			_syncBlock = syncBlock;
			_continueIf = continueIf;
			_immediate = immediate;
		}

		public void Connect(Job nextJob)
		{
			_nextJob = nextJob;
			if (_syncPause)
			{
				_thisJob.SyncPause(_nextJob);
			}
			if (_syncBlock)
			{
				_thisJob.SyncBlockToPause(_nextJob);
			}
			switch (_type)
			{
			case ConnectionType.Then:
				_nextJob.parent = _thisJob;
				if (!_thisJob._finished)
				{
					Job thisJob = _thisJob;
					thisJob._onComplete = (Action<Job>)Delegate.Combine(thisJob._onComplete, (Action<Job>)delegate
					{
						ThenConnectLogic();
					});
				}
				else
				{
					ThenConnectLogic();
				}
				break;
			case ConnectionType.Afterward:
				_nextJob.parent = _thisJob;
				if (!_thisJob._finalized)
				{
					Job thisJob2 = _thisJob;
					thisJob2._onUnload = (Action)Delegate.Combine(thisJob2._onUnload, new Action(ThenConnectLogic));
				}
				else
				{
					ThenConnectLogic();
				}
				break;
			case ConnectionType.And:
				if (!_thisJob._concurrentGroupId.HasValue)
				{
					_thisJob._concurrentGroupId = UJobManager.Instance._GetNextConcurrentGroupId();
					UJobManager.Instance._AddToConcurrentGroup(_thisJob._concurrentGroupId.Value, _thisJob);
				}
				_nextJob._concurrentGroupId = _thisJob._concurrentGroupId;
				UJobManager.Instance._AddToConcurrentGroup(_thisJob._concurrentGroupId.Value, _nextJob);
				break;
			case ConnectionType.Link:
				_thisJob.SyncCompletion(_nextJob);
				break;
			case ConnectionType.WhileRunning:
				_thisJob.SyncCompletionOneWay(_nextJob);
				break;
			case ConnectionType.AsLongAs:
				_nextJob.SyncCompletionOneWay(_thisJob);
				break;
			}
		}

		private void ThenConnectLogic()
		{
			if (((uint)_continueIf & (uint)((!_thisJob.wasKilled) ? 1 : 2)) == 0)
			{
				_nextJob._SmartStart();
				_nextJob._lastIEnumeratorValue = _thisJob.result;
				_nextJob.Kill();
				return;
			}
			_nextJob._lastIEnumeratorValue = _thisJob.result;
			if (_immediate)
			{
				_nextJob.UpdateOnStart();
			}
			_nextJob._SmartStart();
		}
	}

	private enum StopType : byte
	{
		Stop,
		Kill
	}

	private enum ConnectionType : byte
	{
		Then,
		Afterward,
		And,
		Link,
		WhileRunning,
		AsLongAs
	}

	[Flags]
	public enum ContinueIfType : byte
	{
		Successful = 1,
		NotSuccessful = 2
	}

	private static ConnectionData ActiveConnection;

	private object _lastIEnumeratorValue;

	private Department _department;

	private object _creator;

	private string _name;

	private int _runLevel;

	private int _blockLevel;

	private IEnumerator _ienumerator;

	private IEnumerator _coroutine;

	private bool _started;

	private bool _running;

	private StopType? _stopType;

	private bool _finished;

	private bool _finalized;

	private bool _lastPaused;

	private bool _paused;

	private bool _lastBlocked;

	private bool _blocked;

	private bool _isInBackgroundThread;

	private int _threadInterval;

	private int? _concurrentGroupId;

	private Action<Job> _onStart;

	private Action<Job> _onStopRunning;

	private Action<Job> _onComplete;

	private Action<bool> _onPause;

	private Action<bool> _onBlock;

	private Action _onUnload;

	private Action _onFinalize;

	private static UJobManager UManager => UJobManager.Instance;

	public object result => _lastIEnumeratorValue;

	public Department department => _department;

	public object creator => _creator;

	public string name => _name;

	public int runLevel => _runLevel;

	public int blockLevel => _blockLevel;

	public bool hasStarted => _started;

	public bool isRunning => _running;

	public bool wasStopped => _stopType.HasValue;

	public bool wasKilled
	{
		get
		{
			if (_stopType.HasValue)
			{
				return _stopType.Value == StopType.Kill;
			}
			return false;
		}
	}

	public bool finished => _finished;

	public bool finalized => _finalized;

	public bool paused
	{
		get
		{
			return _paused;
		}
		set
		{
			if (_started && _onPause != null && value != _lastPaused)
			{
				_onPause(value);
				_lastPaused = value;
			}
			_paused = value;
		}
	}

	public bool blocked
	{
		get
		{
			return _blocked;
		}
		set
		{
			if (_started && _onBlock != null && value != _lastBlocked)
			{
				_onBlock(value);
				_lastBlocked = value;
			}
			_blocked = value;
		}
	}

	public bool hasCompleted
	{
		get
		{
			if (_started)
			{
				return !_running;
			}
			return false;
		}
	}

	public Job parent { get; private set; }

	public static IEnumerator Wait(float seconds, bool realtime = false)
	{
		float time = 0f;
		while (time < seconds)
		{
			time += (realtime ? Time.unscaledDeltaTime : Time.deltaTime);
			yield return null;
		}
	}

	public static IEnumerator WaitFrames(int numFrames)
	{
		int startFrame = Time.frameCount;
		while (Time.frameCount - startFrame < numFrames)
		{
			yield return null;
		}
	}

	private static IEnumerator DoAction(Action action)
	{
		action();
		yield break;
	}

	public static IEnumerator WaitForCondition(Func<bool> condition)
	{
		while (!condition())
		{
			yield return null;
		}
	}

	public static IEnumerator WaitForDepartment(Department department)
	{
		while (NumberOfJobsRunningInDepartment(department) > 0)
		{
			yield return null;
		}
	}

	public static IEnumerator WaitForDepartmentEmpty(Department department)
	{
		while (UJobManager.Instance._jobsByDepartment[department].Count > 0)
		{
			yield return null;
		}
	}

	public static void DebugDepartment(Department department)
	{
		using PoolHandle<StringBuilder> poolHandle = Pools.Use<StringBuilder>();
		poolHandle.value.Append("Debugging [").Append(EnumUtil.FriendlyName(department)).Append("] Department: Number of Jobs = [")
			.Append(NumberOfJobsInDepartment(department))
			.Append("]\n");
		foreach (Job item in UJobManager.Instance._jobsByDepartment[department])
		{
			poolHandle.value.Append("• ").Append(item).Append("\n");
		}
		Debug.LogWarning(poolHandle.value.ToString().RemoveFromEnd('\n'));
	}

	public static IEnumerator WaitTillDestroyed(UnityEngine.Object obj)
	{
		while (!obj.IsDestroyed())
		{
			yield return null;
		}
	}

	public static IEnumerator WaitTillDisabled(Behaviour behaviour)
	{
		while (!behaviour.IsDestroyed() && behaviour.isActiveAndEnabled)
		{
			yield return null;
		}
	}

	public static IEnumerator WaitTillDisabled(Renderer renderer)
	{
		while (!renderer.IsDestroyed() && renderer.enabled)
		{
			yield return null;
		}
	}

	public static IEnumerator WaitTillDeactivated(GameObject go)
	{
		bool hasBeenActive = go.activeInHierarchy;
		while ((bool)go)
		{
			if (!hasBeenActive)
			{
				hasBeenActive = go.activeInHierarchy;
				yield return null;
				continue;
			}
			if (go.activeInHierarchy)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	public static IEnumerator WaitTillNotVisible(CanvasGroup canvasGroup)
	{
		while ((bool)canvasGroup && (!canvasGroup.gameObject.activeInHierarchy || canvasGroup.alpha <= 0f))
		{
			yield return null;
		}
		while ((bool)canvasGroup && canvasGroup.gameObject.activeInHierarchy && canvasGroup.alpha > 0f)
		{
			yield return null;
		}
	}

	public static IEnumerator WaitTillVisible(CanvasGroup canvasGroup, float alphaThreshold = 0.99f)
	{
		while ((bool)canvasGroup && (!canvasGroup.gameObject.activeInHierarchy || canvasGroup.alpha < alphaThreshold))
		{
			yield return null;
		}
	}

	public static IEnumerator WaitForNoChildren(Transform transform, bool includeInactive = true)
	{
		while (true)
		{
			IEnumerator enumerator = transform.GetEnumerator();
			try
			{
				Transform transform2;
				do
				{
					if (enumerator.MoveNext())
					{
						transform2 = (Transform)enumerator.Current;
						continue;
					}
					yield break;
				}
				while (!includeInactive && !transform2.gameObject.activeSelf);
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			yield return null;
		}
	}

	public static IEnumerator WaitForJob(Job job)
	{
		while (!job.hasCompleted)
		{
			yield return null;
		}
	}

	public static IEnumerator ReturnResult(Func<object> result)
	{
		yield return result();
	}

	public static IEnumerator WaitForAction(Ptr<Action> action)
	{
		bool actionTriggered = false;
		Action listener = null;
		listener = delegate
		{
			actionTriggered = true;
			Ptr<Action> ptr2 = action;
			ptr2.value = (Action)Delegate.Remove(ptr2.value, listener);
		};
		Ptr<Action> ptr = action;
		ptr.value = (Action)Delegate.Combine(ptr.value, listener);
		return WaitForCondition(() => actionTriggered);
	}

	public static IEnumerator WaitForUnityEvent(UnityEngine.Object eventOwner, UnityEvent unityEvent)
	{
		bool eventRaised = false;
		UnityAction action = null;
		action = delegate
		{
			eventRaised = true;
			unityEvent.RemoveListener(action);
		};
		unityEvent.AddListener(action);
		return WaitForCondition(() => eventRaised || !eventOwner);
	}

	public static IEnumerator WaitForOneFrame()
	{
		yield return null;
	}

	public static IEnumerator InvokeNextFrame(Action action)
	{
		yield return null;
		action();
	}

	public static IEnumerator InvokeEndOfFrame(Action action)
	{
		yield return new WaitForEndOfFrame();
		action();
	}

	public static IEnumerator LoopIndefinite()
	{
		while (true)
		{
			yield return null;
		}
	}

	public static IEnumerator EmptyEnumerator()
	{
		yield break;
	}

	public static IEnumerator ParallelProcesses(params IEnumerator[] processes)
	{
		if (processes.Length > 64)
		{
			throw new ArgumentOutOfRangeException("ParallelProcesses() can only run up to 64 concurrent processes, passed [" + processes.Length + "] processes.");
		}
		long completeProcessFlags = 0L;
		while (true)
		{
			bool flag = false;
			for (int i = 0; i < processes.Length; i++)
			{
				long num = i;
				if ((completeProcessFlags & num) == 0L)
				{
					if (processes[i].MoveNext())
					{
						flag = true;
					}
					else
					{
						completeProcessFlags |= num;
					}
				}
			}
			if (flag)
			{
				yield return null;
				continue;
			}
			break;
		}
	}

	public static IEnumerator ParallelProcesses(PoolKeepItemListHandle<IEnumerator> processes, bool processInOrder = false)
	{
		if (processInOrder)
		{
			processes.value.Reverse();
		}
		using (processes)
		{
			while (processes.Count > 0)
			{
				for (int num = processes.Count - 1; num >= 0; num--)
				{
					if (!processes[num].MoveNext())
					{
						processes.RemoveAt(num);
					}
				}
				if (processes.Count > 0)
				{
					yield return null;
				}
			}
		}
	}

	private static Job _StartJob(Job job, Department? department = null, int? runLevel = null, int? blockLevel = null)
	{
		if (ActiveConnection == null)
		{
			job._SmartStart();
		}
		else
		{
			ActiveConnection.previousJob._Do(job, department, runLevel, blockLevel);
		}
		return job;
	}

	public static Job Process(IEnumerator ienumerator, Department? department = null, int? runLevel = null, int? blockLevel = null)
	{
		return _StartJob(new Job(ienumerator, department ?? Department.Misc, runLevel.GetValueOrDefault(), blockLevel.GetValueOrDefault()), department, runLevel, blockLevel);
	}

	public static Job Action(Action action, Department? department = null, int? runLevel = null, int? blockLevel = null)
	{
		return Process(DoAction(action), department, runLevel, blockLevel);
	}

	public static Job Result(Func<object> result, Department? department = null, int? runLevel = null, int? blockLevel = null)
	{
		return Process(ReturnResult(result), department, runLevel, blockLevel);
	}

	public static Job WaitForGameNodeProcess()
	{
		return Process(WaitForDepartment(Department.GameNodeProcess), Department.GameNodeProcessNonBlocking);
	}

	public static Job GetJob(object creator, string name)
	{
		if (!UManager._trackedJobs.ContainsKey(creator) || !UManager._trackedJobs[creator].ContainsKey(name))
		{
			return null;
		}
		return UManager._trackedJobs[creator][name];
	}

	public static bool IsTrackedJobCreatedByTypeRunning<T>(string name = null)
	{
		foreach (object key in UManager._trackedJobs.Keys)
		{
			if (typeof(T).IsSameOrSubclass(key.GetType()) && (name.IsNullOrEmpty() || UManager._trackedJobs[key].ContainsKey(name)))
			{
				return true;
			}
		}
		return false;
	}

	private static void _StopDepartment(Department department, StopType stopType)
	{
		if ((bool)UJobManager.Instance)
		{
			List<Job> list = UJobManager.Instance._jobsByDepartment[department];
			for (int num = list.Count - 1; num >= 0; num--)
			{
				list[num].Stop(stopType);
			}
		}
	}

	public static void StopDepartment(Department department)
	{
		_StopDepartment(department, StopType.Stop);
	}

	public static void KillDepartment(Department department)
	{
		_StopDepartment(department, StopType.Kill);
	}

	public static int NumberOfJobsRunningInDepartment(Department department)
	{
		int num = 0;
		foreach (Job item in UJobManager.Instance._jobsByDepartment[department])
		{
			if (item.isRunning)
			{
				num++;
			}
		}
		return num;
	}

	public static int NumberOfJobsInDepartment(Department department)
	{
		return UJobManager.Instance._jobsByDepartment[department].Count;
	}

	public static void OnApplicationQuit(Action action)
	{
		UJobManager.Instance._AddOnApplicationQuit(action);
	}

	public static Coroutine StartRoutine(IEnumerator enumerator)
	{
		return UManager.StartCoroutine(enumerator);
	}

	private Job(IEnumerator ienumarator, Department department = Department.Misc, int runLevel = 0, int blockLevel = 0)
	{
		_department = department;
		_ienumerator = ienumarator;
		paused = false;
		_blockLevel = blockLevel;
		_runLevel = runLevel;
	}

	public bool _Update()
	{
		return _coroutine.MoveNext();
	}

	public Job ToBackgroundThread(int threadInterval = 0)
	{
		if (_started)
		{
			_threadInterval = threadInterval;
			_isInBackgroundThread = ThreadPool.QueueUserWorkItem(_DoWorkBackground);
		}
		else
		{
			_onStart = (Action<Job>)Delegate.Combine(_onStart, (Action<Job>)delegate
			{
				ToBackgroundThread(threadInterval);
			});
		}
		return this;
	}

	public Job ForceCompletion()
	{
		Stack<Job> stack = new Stack<Job>();
		for (Job job = this; job != null; job = job.parent)
		{
			stack.Push(job);
		}
		List<Job> list = stack.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i]._concurrentGroupId.HasValue)
			{
				UJobManager.Instance._DeleteConcurrentGroup(list[i]._concurrentGroupId.Value);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Job job = list[j];
			job._SmartStart();
			do
			{
				job._Update();
			}
			while (!job._finalized);
		}
		return this;
	}

	public T ForceResult<T>()
	{
		ForceCompletion();
		return (T)result;
	}

	public Job RegisterDelegate<T>(T delegateToRegister, Action<T> register, Action<T> unregister, bool unregisterOnBlock = true, bool unregisterOnPause = true) where T : class
	{
		if (_finished)
		{
			return this;
		}
		Action register2 = delegate
		{
			register(delegateToRegister);
		};
		Action unregister2 = delegate
		{
			unregister(delegateToRegister);
		};
		RegisterData regData = new RegisterData(register2, unregister2, unregisterOnBlock, unregisterOnPause);
		if (_started)
		{
			regData.UpdateRegistration(_blocked, _paused);
		}
		else
		{
			_onStart = (Action<Job>)Delegate.Combine(_onStart, (Action<Job>)delegate
			{
				regData.UpdateRegistration(_blocked, _paused);
			});
		}
		if (regData.unregisterOnPause)
		{
			_onPause = (Action<bool>)Delegate.Combine(_onPause, (Action<bool>)delegate(bool t)
			{
				regData.UpdateRegistration(_blocked, t);
			});
		}
		if (regData.unregisterOnBlock)
		{
			_onBlock = (Action<bool>)Delegate.Combine(_onBlock, (Action<bool>)delegate(bool t)
			{
				regData.UpdateRegistration(t, _paused);
			});
		}
		_onComplete = (Action<Job>)Delegate.Combine(_onComplete, (Action<Job>)delegate
		{
			regData.unregister();
		});
		return this;
	}

	public IEnumerator WaitForCompletion()
	{
		return WaitForJob(this);
	}

	public void SyncStart(Job otherJob)
	{
		_onStart = (Action<Job>)Delegate.Combine(_onStart, (Action<Job>)delegate
		{
			otherJob._SmartStart();
		});
		Job job = otherJob;
		job._onStart = (Action<Job>)Delegate.Combine(job._onStart, (Action<Job>)delegate
		{
			_SmartStart();
		});
	}

	public void SyncPause(Job otherJob)
	{
		_onPause = (Action<bool>)Delegate.Combine(_onPause, (Action<bool>)delegate(bool p)
		{
			otherJob.paused = p;
		});
		Job job = otherJob;
		job._onPause = (Action<bool>)Delegate.Combine(job._onPause, (Action<bool>)delegate(bool p)
		{
			paused = p;
		});
	}

	public void SyncBlockToPause(Job otherJob)
	{
		_onBlock = (Action<bool>)Delegate.Combine(_onBlock, (Action<bool>)delegate(bool b)
		{
			otherJob.paused = b;
		});
		Job job = otherJob;
		job._onBlock = (Action<bool>)Delegate.Combine(job._onBlock, (Action<bool>)delegate(bool b)
		{
			paused = b;
		});
	}

	public void SyncCompletion(Job otherJob)
	{
		_onComplete = (Action<Job>)Delegate.Combine(_onComplete, (Action<Job>)delegate
		{
			otherJob.Stop(_stopType);
		});
		Job job = otherJob;
		job._onComplete = (Action<Job>)Delegate.Combine(job._onComplete, (Action<Job>)delegate
		{
			Stop(otherJob._stopType);
		});
	}

	public void SyncCompletionOneWay(Job otherJob)
	{
		_onComplete = (Action<Job>)Delegate.Combine(_onComplete, (Action<Job>)delegate
		{
			otherJob.Stop(_stopType);
		});
	}

	public Job Block(Department departmentToBlock, int blockLevel = int.MaxValue)
	{
		WhileRunning().DoProcess(LoopIndefinite(), departmentToBlock, -1, blockLevel);
		return this;
	}

	public Job If<T>(Func<T, bool> valid)
	{
		OnStopRunning(delegate(Job job)
		{
			if (!valid((T)result))
			{
				job.Kill();
			}
		});
		return this;
	}

	public Job Then()
	{
		ActiveConnection = new ConnectionData(this, ConnectionType.Then);
		return this;
	}

	public Job Immediately()
	{
		ActiveConnection = new ConnectionData(this, ConnectionType.Then, syncPause: false, syncBlock: false, ContinueIfType.Successful, immediate: true);
		return this;
	}

	public Job Next()
	{
		ActiveConnection = new ConnectionData(this, ConnectionType.Then, syncPause: false, syncBlock: false, ContinueIfType.Successful | ContinueIfType.NotSuccessful);
		return this;
	}

	public Job NextImmediately()
	{
		ActiveConnection = new ConnectionData(this, ConnectionType.Then, syncPause: false, syncBlock: false, ContinueIfType.Successful | ContinueIfType.NotSuccessful, immediate: true);
		return this;
	}

	public Job Else()
	{
		ActiveConnection = new ConnectionData(this, ConnectionType.Then, syncPause: false, syncBlock: false, ContinueIfType.NotSuccessful);
		return this;
	}

	public Job ElseImmediately()
	{
		ActiveConnection = new ConnectionData(this, ConnectionType.Then, syncPause: false, syncBlock: false, ContinueIfType.NotSuccessful, immediate: true);
		return this;
	}

	public Job Afterward()
	{
		ActiveConnection = new ConnectionData(this, ConnectionType.Afterward);
		return this;
	}

	public Job ElseAfterward()
	{
		ActiveConnection = new ConnectionData(this, ConnectionType.Afterward, syncPause: false, syncBlock: false, ContinueIfType.NotSuccessful);
		return this;
	}

	public Job NextAfterward()
	{
		ActiveConnection = new ConnectionData(this, ConnectionType.Afterward, syncPause: false, syncBlock: false, ContinueIfType.Successful | ContinueIfType.NotSuccessful);
		return this;
	}

	public Job And(bool syncPause = true, bool syncBlock = true)
	{
		ActiveConnection = new ConnectionData(this, ConnectionType.And, syncPause, syncBlock);
		return this;
	}

	public Job Link(bool syncPause = true, bool syncBlock = true)
	{
		ActiveConnection = new ConnectionData(this, ConnectionType.Link, syncPause, syncBlock);
		return this;
	}

	public Job WhileRunning(bool syncPause = false, bool syncBlock = false)
	{
		ActiveConnection = new ConnectionData(this, ConnectionType.WhileRunning, syncPause, syncBlock);
		return this;
	}

	public Job AsLongAs(bool syncPause = false, bool syncBlock = false)
	{
		ActiveConnection = new ConnectionData(this, ConnectionType.AsLongAs, syncPause, syncBlock);
		return this;
	}

	public Job ChainJob(Func<Job> jobCreator)
	{
		return OnStopRunning(delegate(Job job)
		{
			job.And().DoJob(jobCreator()).OnStopRunning(delegate(Job j)
			{
				_lastIEnumeratorValue = j.result;
			});
		});
	}

	public Job ChainJobs(params Func<Job>[] jobCreators)
	{
		if (jobCreators.IsNullOrEmpty())
		{
			return this;
		}
		for (int j = 0; j < jobCreators.Length; j++)
		{
			int i = j;
			Func<Job> currentJobCreator = jobCreators[j];
			jobCreators[j] = delegate
			{
				Job job = currentJobCreator();
				if (i + 1 < jobCreators.Length)
				{
					job.ChainJob(jobCreators[i + 1]);
				}
				return job;
			};
		}
		return ChainJob(jobCreators[0]);
	}

	public Job ChainJobResult<T>(Func<T, Job> jobCreator)
	{
		return OnStopRunning(delegate(Job job)
		{
			job.And().DoJob(jobCreator((T)job.result)).OnStopRunning(delegate(Job j)
			{
				_lastIEnumeratorValue = j.result;
			});
		});
	}

	public Job ChainJobResults(params Func<object, Job>[] jobCreators)
	{
		for (int k = 0; k < jobCreators.Length; k++)
		{
			int i = k;
			Func<object, Job> currentJobCreator = jobCreators[k];
			jobCreators[k] = delegate
			{
				Job job = currentJobCreator(_lastIEnumeratorValue);
				this = job;
				if (i + 1 < jobCreators.Length)
				{
					job.ChainJobResult(jobCreators[i + 1]);
				}
				else
				{
					job.OnStopRunning(delegate(Job j)
					{
						_lastIEnumeratorValue = j.result;
					});
				}
				return job;
			};
		}
		return ChainJobResult(jobCreators[0]);
	}

	private Job _Do(Job job, Department? department = null, int? runLevel = null, int? blockLevel = null)
	{
		bool flag = true;
		job.parent = parent;
		if (ActiveConnection != null)
		{
			ActiveConnection.Connect(job);
			flag = ActiveConnection.doStartLogic;
			ActiveConnection = null;
		}
		_CopyFieldsTo(job, department, runLevel, blockLevel);
		if (flag)
		{
			if (_started)
			{
				job._SmartStart();
			}
			else
			{
				SyncStart(job);
			}
		}
		return job;
	}

	public Job DoJob(Job job)
	{
		return job;
	}

	public Job DoProcess(IEnumerator ienumerator, Department? department = null, int? runLevel = null, int? blockLevel = null)
	{
		return _Do(new Job(ienumerator), department, runLevel, blockLevel);
	}

	public Job Do(Action action, Department? department = null, int? runLevel = null, int? blockLevel = null)
	{
		return DoProcess(DoAction(action), department, runLevel, blockLevel);
	}

	public Job DoResult(Func<object> result, Department? department = null, int? runLevel = null, int? blockLevel = null)
	{
		return DoProcess(ReturnResult(result), department, runLevel, blockLevel);
	}

	public Job ResultProcess<T>(Func<T, IEnumerator> processResult, Department? department = null, int? runLevel = null, int? blockLevel = null)
	{
		return DoProcess(_ResultProcess(processResult), department, runLevel, blockLevel);
	}

	public Job ResultAction<T>(Action<T> processResult, Department? department = null, int? runLevel = null, int? blockLevel = null)
	{
		return Do(delegate
		{
			processResult((T)result);
		}, department, runLevel, blockLevel);
	}

	public Job ResultConvert<T>(Func<T, object> processResult, Department? department = null, int? runLevel = null, int? blockLevel = null)
	{
		return DoResult(() => processResult((T)result), department, runLevel, blockLevel);
	}

	public Job BeginTracking(object creator, string name)
	{
		if (_creator != null)
		{
			return this;
		}
		_creator = creator;
		_name = name;
		if (UJobManager.Instance._trackedJobs.ContainsKey(creator) && UJobManager.Instance._trackedJobs[_creator].ContainsKey(_name))
		{
			UJobManager.Instance._trackedJobs[_creator][_name]._EndTracking();
		}
		if (!UJobManager.Instance._trackedJobs.ContainsKey(creator))
		{
			UJobManager.Instance._trackedJobs.Add(creator, new Dictionary<string, Job>());
		}
		UJobManager.Instance._trackedJobs[_creator].Add(_name, this);
		return this;
	}

	private void _EndTracking()
	{
		if (_creator != null)
		{
			if (UJobManager.Instance._trackedJobs[_creator].Remove(_name) && UJobManager.Instance._trackedJobs[_creator].Count == 0)
			{
				UJobManager.Instance._trackedJobs.Remove(_creator);
			}
			_creator = null;
		}
	}

	private void Stop(StopType? stopType)
	{
		_running = false;
		paused = false;
		_stopType = stopType.GetValueOrDefault();
		_Update();
	}

	public Job Break(ref Action breakWhen)
	{
		breakWhen = (Action)Delegate.Combine(breakWhen, (Action)delegate
		{
			Stop(StopType.Stop);
		});
		return this;
	}

	public void Kill()
	{
		Stop(StopType.Kill);
	}

	public void KillAndStopTracking()
	{
		Kill();
		_EndTracking();
	}

	public Job Cleanup(Action action)
	{
		_onFinalize = (Action)Delegate.Combine(_onFinalize, action);
		return this;
	}

	public Job OnStartRunning(Action<Job> startupAction)
	{
		_onStart = (Action<Job>)Delegate.Combine(_onStart, startupAction);
		return this;
	}

	private Job UpdateOnStart()
	{
		_onStart = (Action<Job>)Delegate.Combine(_onStart, (Action<Job>)delegate(Job j)
		{
			j._Update();
		});
		return this;
	}

	public Job OnStopRunning(Action<Job> action)
	{
		_onStopRunning = (Action<Job>)Delegate.Combine(_onStopRunning, action);
		return this;
	}

	public T GetTag<T>()
	{
		return JobTags<T>.GetTag(this);
	}

	public Job SetTag<T>(T value)
	{
		JobTags<T>.SetTag(this, value);
		return this;
	}

	private void _SmartStart()
	{
		if (!_started)
		{
			int highestRun = UManager.HighestRunLevel(department);
			int highestBlock = UManager.HighestBlockLevel(department);
			Job job = this;
			while (job.parent != null && !job.parent._started)
			{
				job = job.parent;
			}
			job._Start(highestRun, highestBlock);
		}
	}

	private void _Start(int highestRun, int highestBlock)
	{
		_SmartBlock(highestRun);
		_SmartRun(highestBlock);
		_started = true;
		_running = !wasStopped;
		_coroutine = _DoWork();
		UJobManager.Instance._AddJob(this);
		if (_onStart != null)
		{
			_onStart(this);
		}
	}

	private void _SmartBlock(int highestRun)
	{
		if (_blockLevel == int.MaxValue)
		{
			_blockLevel = highestRun + 1;
		}
	}

	private void _SmartRun(int highestBlock)
	{
		if (_runLevel == int.MaxValue)
		{
			_runLevel = highestBlock + 1;
		}
	}

	private IEnumerator _DoWork()
	{
		while (_running)
		{
			if (_blocked || _paused || _isInBackgroundThread)
			{
				yield return null;
			}
			else if (_ienumerator.MoveNext())
			{
				_lastIEnumeratorValue = _ienumerator.Current;
				yield return _lastIEnumeratorValue;
				if (_lastIEnumeratorValue is ToBackgroundThread)
				{
					ToBackgroundThread(((ToBackgroundThread)_lastIEnumeratorValue).interval);
				}
			}
			else
			{
				_running = false;
			}
		}
		if (_onStopRunning != null)
		{
			_onStopRunning(this);
		}
		if (_concurrentGroupId.HasValue)
		{
			while (!UJobManager.Instance.ConcurrentGroupComplete(_concurrentGroupId.Value))
			{
				yield return null;
			}
		}
		if (_onComplete != null)
		{
			_onComplete(this);
		}
		_finished = true;
		yield return null;
		_Unload();
	}

	private void _DoWorkBackground(object state)
	{
		try
		{
			while (_running)
			{
				if (_blocked || _paused)
				{
					Thread.Sleep(10);
					continue;
				}
				if (_ienumerator.MoveNext())
				{
					_lastIEnumeratorValue = _ienumerator.Current;
					if (!(_lastIEnumeratorValue is ToMainThread))
					{
						if (_threadInterval > 0)
						{
							Thread.Sleep(_threadInterval);
						}
						continue;
					}
					break;
				}
				break;
			}
		}
		catch (Exception e)
		{
			Debug.LogError(e.ErrorString());
		}
		_isInBackgroundThread = false;
	}

	private void _Unload()
	{
		_onComplete = null;
		_onBlock = null;
		_onPause = null;
		_onStart = null;
		_ienumerator = null;
		if (_onUnload != null)
		{
			_onUnload();
		}
		_onUnload = null;
		if (_onFinalize != null)
		{
			_onFinalize();
		}
		_onFinalize = null;
		_finalized = true;
		_EndTracking();
	}

	private void _CopyFieldsTo(Job otherJob, Department? department, int? runLevel, int? blockLevel)
	{
		otherJob._department = department ?? this.department;
		otherJob._runLevel = runLevel ?? _runLevel;
		otherJob._blockLevel = blockLevel ?? _blockLevel;
	}

	private IEnumerator _ResultProcess<T>(Func<T, IEnumerator> resultProcess)
	{
		IEnumerator innerEnum = resultProcess((T)result);
		while (innerEnum.MoveNext())
		{
			yield return innerEnum.Current;
		}
	}

	public override string ToString()
	{
		return string.Format("Job: Department = {0}, Creator = {1}, Name = {2}, Started = {3}, Running = {4}, Stopped = {5}, Finished = {6}, Finalized = {7}, Paused = {8}, Blocked = {9}, IEnumerator = {10}", department, (creator != null) ? creator.ToString() : "N/A", name ?? "N/A", _started, _running, _stopType, _finished, _finalized, _paused, _blocked, (_ienumerator != null) ? _ienumerator.ToString() : "N/A");
	}
}
