using System.Collections.Generic;
using UnityEngine;

namespace Framework.FrameworkCore;

[CreateAssetMenu(fileName = "skill", menuName = "Blasphemous/Unlockable CONFIG")]
public class UnlockableSkillConfiguration : ScriptableObject
{
	[SerializeField]
	private List<int> TierConfiguration = new List<int> { 0, 1, 3, 5, 7, 9, 14 };

	public int GetMaxTier()
	{
		return TierConfiguration.Count;
	}

	public int GetUnlockNeeded(int tier)
	{
		int result = -1;
		if (tier < TierConfiguration.Count)
		{
			result = TierConfiguration[tier];
		}
		return result;
	}
}
