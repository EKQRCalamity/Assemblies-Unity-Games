using Framework.FrameworkCore;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Tools.Level.Layout;
using UnityEngine;

namespace Framework.Editor;

public class BundleToolset : MonoBehaviour
{
	[Button(ButtonSizes.Small)]
	[UsedImplicitly]
	public void ShowGreybox()
	{
		ShowGraybox(b: true);
	}

	[Button(ButtonSizes.Small)]
	[UsedImplicitly]
	public void HideGreybox()
	{
		ShowGraybox(b: false);
	}

	private static void ShowGraybox(bool b)
	{
		LayoutElement[] array = Object.FindObjectsOfType<LayoutElement>();
		Log.Trace("Decoration", "Modifiying graybox visibility. Visible: " + b + " Afecteds: " + array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			SpriteRenderer[] componentsInChildren = array[i].GetComponentsInChildren<SpriteRenderer>();
			if (componentsInChildren != null && array[i].showInGame)
			{
				componentsInChildren.ForEach(delegate(SpriteRenderer x)
				{
					x.enabled = true;
				});
			}
			else
			{
				componentsInChildren?.ForEach(delegate(SpriteRenderer x)
				{
					x.enabled = b;
				});
			}
		}
	}
}
