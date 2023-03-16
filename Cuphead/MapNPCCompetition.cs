using UnityEngine;

public class MapNPCCompetition : MonoBehaviour
{
	private int[] dialogueVarIndices = new int[3] { 26, 27, 28 };

	private void Start()
	{
		int[] curseCharmPuzzleOrder = PlayerData.Data.curseCharmPuzzleOrder;
		int[] curseCharmPuzzleOrder2 = PlayerData.Data.curseCharmPuzzleOrder;
		foreach (int num in curseCharmPuzzleOrder2)
		{
		}
		for (int j = 0; j < curseCharmPuzzleOrder.Length; j++)
		{
			Dialoguer.SetGlobalFloat(dialogueVarIndices[j], curseCharmPuzzleOrder[j]);
		}
	}
}
