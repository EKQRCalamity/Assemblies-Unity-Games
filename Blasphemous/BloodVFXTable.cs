using System.Collections.Generic;
using Gameplay.GameControllers.Effects.Player.Sparks;
using UnityEngine;

[CreateAssetMenu(fileName = "New Blood Fx Table", menuName = "VFXTables/Blood")]
public class BloodVFXTable : ScriptableObject
{
	public List<BloodFXTableElement> bloodVFXList;

	public BloodFXTableElement GetRandomElementOfType(BloodSpawner.BLOOD_FX_TYPES type)
	{
		List<BloodFXTableElement> list = bloodVFXList.FindAll((BloodFXTableElement x) => x.type == type);
		return list[Random.Range(0, list.Count)];
	}
}
