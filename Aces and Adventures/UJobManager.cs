using System;
using System.Collections.Generic;
using UnityEngine;

public class UJobManager : MonoBehaviour
{
	private static UJobManager _instance;

	private List<Job> _jobs;

	public Dictionary<Department, List<Job>> _jobsByDepartment;

	private Dictionary<Department, int> _maxRunLevel;

	private Dictionary<Department, int> _maxBlockLevel;

	public Dictionary<object, Dictionary<string, Job>> _trackedJobs;

	private int _currentConcurrentGroupId;

	private Dictionary<int, List<Job>> _concurrentGroups;

	private Action _onApplicationQuit;

	public static UJobManager Instance => ManagerUtil.GetSingletonInstance(ref _instance, createSeparateGameObject: false, delegate(UJobManager i)
	{
		i.Initialize();
	});

	public static bool IsQuitting { get; private set; }

	static UJobManager()
	{
		Application.quitting += _OnApplicationQuitting;
	}

	private static void _OnApplicationQuitting()
	{
		FileLock.WaitForLocks();
		IsQuitting = true;
	}

	private void Initialize()
	{
		_jobs = new List<Job>();
		_currentConcurrentGroupId = 0;
		_concurrentGroups = new Dictionary<int, List<Job>>();
		InitializeDepartments();
	}

	private void InitializeDepartments()
	{
		_jobsByDepartment = new Dictionary<Department, List<Job>>();
		_maxRunLevel = new Dictionary<Department, int>();
		_maxBlockLevel = new Dictionary<Department, int>();
		_trackedJobs = new Dictionary<object, Dictionary<string, Job>>();
		Department[] values = EnumUtil<Department>.Values;
		foreach (Department key in values)
		{
			_jobsByDepartment.Add(key, new List<Job>());
			_maxRunLevel.Add(key, 0);
			_maxBlockLevel.Add(key, 0);
		}
	}

	private void Update()
	{
		AsyncTask._Update();
	}

	private void LateUpdate()
	{
		foreach (List<Job> value in _concurrentGroups.Values)
		{
			for (int num = value.Count - 1; num >= 0; num--)
			{
				if (value[num].hasCompleted)
				{
					value.RemoveAt(num);
				}
			}
		}
		for (int i = 0; i < _jobs.Count; i++)
		{
			Job job = _jobs[i];
			job.blocked = job.runLevel < _maxBlockLevel[job.department];
			job._Update();
		}
		for (int num2 = _jobs.Count - 1; num2 >= 0; num2--)
		{
			if (_jobs[num2].finalized)
			{
				_RemoveJob(num2);
			}
		}
	}

	public void _AddJob(Job job)
	{
		_maxRunLevel[job.department] = Math.Max(_maxRunLevel[job.department], job.runLevel);
		_maxBlockLevel[job.department] = Math.Max(_maxBlockLevel[job.department], job.blockLevel);
		_jobs.Add(job);
		_jobsByDepartment[job.department].Add(job);
	}

	private void _RemoveJob(int index)
	{
		Job job = _jobs[index];
		List<Job> list = _jobsByDepartment[job.department];
		_jobs.RemoveAt(index);
		list.Remove(job);
		bool flag = job.runLevel == _maxRunLevel[job.department];
		bool flag2 = job.blockLevel == _maxBlockLevel[job.department];
		if (!flag && !flag2)
		{
			return;
		}
		int num = ((list.Count > 0) ? int.MinValue : 0);
		int num2 = num;
		for (int i = 0; i < list.Count; i++)
		{
			if (flag)
			{
				num = Math.Max(num, list[i].runLevel);
			}
			if (flag2)
			{
				num2 = Math.Max(num2, list[i].blockLevel);
			}
		}
		if (flag)
		{
			_maxRunLevel[job.department] = num;
		}
		if (flag2)
		{
			_maxBlockLevel[job.department] = num2;
		}
	}

	public int HighestRunLevel(Department department)
	{
		return _maxRunLevel[department];
	}

	public int HighestBlockLevel(Department department)
	{
		return _maxBlockLevel[department];
	}

	public bool ConcurrentGroupComplete(int id)
	{
		if (_concurrentGroups.ContainsKey(id))
		{
			if (_concurrentGroups[id].Count == 0)
			{
				return _concurrentGroups.Remove(id);
			}
			return false;
		}
		return true;
	}

	public int _GetNextConcurrentGroupId()
	{
		return _currentConcurrentGroupId++;
	}

	public void _AddToConcurrentGroup(int id, Job job)
	{
		if (!_concurrentGroups.ContainsKey(id))
		{
			_concurrentGroups.Add(id, new List<Job>());
		}
		_concurrentGroups[id].AddUnique(job);
	}

	public void _DeleteConcurrentGroup(int id)
	{
		_concurrentGroups.Remove(id);
	}

	public void _AddOnApplicationQuit(Action action)
	{
		_onApplicationQuit = (Action)Delegate.Combine(_onApplicationQuit, action);
	}

	private void OnApplicationQuit()
	{
		if (_onApplicationQuit != null)
		{
			_onApplicationQuit();
		}
	}
}
