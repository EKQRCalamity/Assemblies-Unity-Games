using System;
using UnityEngine;

[Serializable]
public class NewGameTypeMaterials
{
	[Header("New Game Materials")]
	public Material spring;

	public Material summer;

	public Material fall;

	public Material winter;

	[Header("Special Materials")]
	public Material procedural;

	public Material challenge;

	public Material spiral;

	public Material this[NewGameType type] => type switch
	{
		NewGameType.Spring => spring, 
		NewGameType.Summer => summer, 
		NewGameType.Fall => fall, 
		NewGameType.Winter => winter, 
		_ => throw new ArgumentOutOfRangeException("type", type, null), 
	};

	public Material this[SpecialGameType? type] => type switch
	{
		SpecialGameType.Procedural => procedural, 
		SpecialGameType.Challenge => challenge, 
		SpecialGameType.Spiral => spiral, 
		_ => null, 
	};
}
