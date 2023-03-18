using System.Collections;
using Framework.Managers;
using Gameplay.UI.Widgets;
using UnityEngine;

namespace Framework.Util;

public class FinalDoor : MonoBehaviour
{
	private bool used;

	private bool inside;

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (col.gameObject.layer == LayerMask.NameToLayer("Penitent"))
		{
			inside = true;
		}
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		if (col.gameObject.layer == LayerMask.NameToLayer("Penitent"))
		{
			inside = false;
		}
	}

	private void Update()
	{
		if (inside && Core.Logic.Penitent.PlatformCharacterInput.isJoystickUp && !used)
		{
			StartCoroutine(action());
			used = true;
		}
	}

	private IEnumerator action()
	{
		FadeWidget.instance.Fade(toBlack: false);
		yield return new WaitForSeconds(1f);
		Core.Logic.LoadAttrackScene();
	}
}
