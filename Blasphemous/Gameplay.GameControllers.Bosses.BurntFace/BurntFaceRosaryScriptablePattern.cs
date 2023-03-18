using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BurntFace;

[CreateAssetMenu(fileName = "RosaryPatterns", menuName = "Blasphemous/Bosses/BurntFace/CreateRosaryPatterns")]
public class BurntFaceRosaryScriptablePattern : ScriptableObject
{
	public List<BurntFaceRosaryPattern> patterns;

	public BurntFaceRosaryPattern GetPattern(string ID)
	{
		return patterns.Find((BurntFaceRosaryPattern x) => x.ID == ID);
	}
}
