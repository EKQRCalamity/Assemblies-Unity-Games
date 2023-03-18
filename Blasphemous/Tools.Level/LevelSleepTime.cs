using System;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;

namespace Tools.Level;

[Serializable]
public class LevelSleepTime : PersistentInterface
{
	protected float Normal;

	protected float Heavy;

	protected float Critical;

	public LevelSleepTime(float normal, float heavy, float critical)
	{
		Normal = normal;
		Heavy = heavy;
		Critical = critical;
	}

	public string GetPersistenID()
	{
		throw new NotImplementedException();
	}

	public PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		throw new NotImplementedException();
	}

	public void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		throw new NotImplementedException();
	}

	public void ResetPersistence()
	{
	}

	public int GetOrder()
	{
		return 0;
	}

	public float GetHitSleepTime(Hit hit)
	{
		float result = 0f;
		switch (hit.DamageType)
		{
		case DamageArea.DamageType.Normal:
			result = Normal;
			break;
		case DamageArea.DamageType.Heavy:
			result = Heavy;
			break;
		case DamageArea.DamageType.Critical:
			result = Critical;
			break;
		case DamageArea.DamageType.Simple:
			result = Normal;
			break;
		case DamageArea.DamageType.Stunt:
			result = Heavy;
			break;
		case DamageArea.DamageType.OptionalStunt:
			result = Heavy;
			break;
		}
		return result;
	}
}
