using UnityEngine;

namespace Gameplay.UI.Others.MenuLogic;

public class ImportGamepadClose : MonoBehaviour
{
	public MainMenu menu;

	private void Update()
	{
		if (Input.GetButtonDown("A") || Input.GetKeyDown(KeyCode.Return))
		{
			menu.CloseImportMenu();
		}
	}
}
