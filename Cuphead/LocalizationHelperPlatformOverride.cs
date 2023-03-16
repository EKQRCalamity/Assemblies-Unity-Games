using System;
using UnityEngine;

public class LocalizationHelperPlatformOverride : MonoBehaviour
{
	[Serializable]
	public class OverrideInfo
	{
		public RuntimePlatform platform;

		public int id;
	}

	public OverrideInfo[] overrides;

	public bool HasOverrideForCurrentPlatform(out int newID)
	{
		RuntimePlatform platform = Application.platform;
		for (int i = 0; i < overrides.Length; i++)
		{
			OverrideInfo overrideInfo = overrides[i];
			if (overrideInfo.platform == platform)
			{
				newID = overrideInfo.id;
				return true;
			}
		}
		newID = -1;
		return false;
	}
}
