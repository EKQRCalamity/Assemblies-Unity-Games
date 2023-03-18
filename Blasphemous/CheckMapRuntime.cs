using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

public class CheckMapRuntime : MonoBehaviour
{
	public int slot = 4;

	public int mapReveals = 10;

	public int elementPerScene = 10;

	[HideInEditorMode]
	[Button(ButtonSizes.Small)]
	private void TestMap()
	{
		Core.Persistence.DEBUG_SaveAllData(slot, mapReveals, elementPerScene);
	}
}
