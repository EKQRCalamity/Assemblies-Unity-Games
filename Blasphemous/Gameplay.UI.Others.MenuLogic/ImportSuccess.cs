using UnityEngine;

namespace Gameplay.UI.Others.MenuLogic;

public class ImportSuccess : MonoBehaviour
{
	public MainMenu menu;

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetButtonDown("A") || Input.GetKeyDown(KeyCode.Return))
		{
			menu.ExitSucessMenu();
		}
	}
}
