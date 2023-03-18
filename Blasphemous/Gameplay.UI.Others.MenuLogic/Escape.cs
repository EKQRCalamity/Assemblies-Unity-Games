using System.Collections;
using Framework.Managers;
using Gameplay.UI.Widgets;
using UnityEngine;

namespace Gameplay.UI.Others.MenuLogic;

public class Escape : MonoBehaviour
{
	private bool pressed;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && !pressed)
		{
			StartCoroutine(Action());
		}
	}

	private IEnumerator Action()
	{
		pressed = true;
		FadeWidget.instance.Fade(toBlack: true);
		yield return new WaitForSeconds(1f);
		Core.Logic.LoadMenuScene();
		pressed = false;
	}
}
