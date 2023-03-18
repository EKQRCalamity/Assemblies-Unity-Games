using System;
using Framework.Managers;
using Framework.Util;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.FrameworkCore;

public class PersistentObject : MonoBehaviour, PersistentInterface
{
	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	protected bool IgnorePersistence;

	public virtual bool IsIgnoringPersistence()
	{
		return IgnorePersistence;
	}

	public virtual bool IsOpenOrActivated()
	{
		return true;
	}

	public string GetPersistenID()
	{
		UniqueId component = GetComponent<UniqueId>();
		if (!component)
		{
			return string.Empty;
		}
		return component.uniqueId;
	}

	public T CreatePersistentData<T>() where T : PersistentManager.PersistentData
	{
		T val = (T)Activator.CreateInstance(typeof(T), GetPersistenID());
		val.debugName = base.name;
		return val;
	}

	public virtual PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		return null;
	}

	public virtual void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
	}

	public int GetOrder()
	{
		return 0;
	}

	public void ResetPersistence()
	{
	}
}
