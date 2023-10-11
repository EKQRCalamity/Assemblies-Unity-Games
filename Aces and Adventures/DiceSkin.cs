using UnityEngine;

public class DiceSkin : MonoBehaviour
{
	[Header("HP")]
	public Material playerHP;

	public Material enemyHP;

	[Header("Shield")]
	public Material playerShield;

	public Material enemyShield;

	public Material this[Faction faction, DiceSkinType skin]
	{
		get
		{
			if (faction != 0)
			{
				if (skin != 0)
				{
					return enemyShield;
				}
				return enemyHP;
			}
			if (skin != 0)
			{
				return playerShield;
			}
			return playerHP;
		}
	}
}
