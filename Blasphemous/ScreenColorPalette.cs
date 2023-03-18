using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualEffects.Scripts;

[ExecuteInEditMode]
public class ScreenColorPalette : MonoBehaviour
{
	[BoxGroup("Effects", true, false, 0)]
	public bool dithering;

	[BoxGroup("Palettes", true, false, 0)]
	public List<ScreenColorPaletteData> availablePalettes;

	[BoxGroup("Palettes", true, false, 0)]
	[ValueDropdown("availablePalettes")]
	[ShowInInspector]
	private ScreenColorPaletteData currentPalette;

	private ScreenColorPaletteData lastInjectedPalette;

	private Material material;

	private void OnEnable()
	{
		if (material == null)
		{
			material = Resources.Load<Material>("Materials/Effects_Nostalgia");
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (material == null || currentPalette == null)
		{
			Graphics.Blit(src, dest);
			return;
		}
		if (lastInjectedPalette == null || lastInjectedPalette != currentPalette)
		{
			currentPalette.Inject(material, dithering);
			lastInjectedPalette = currentPalette;
		}
		Graphics.Blit(src, dest, material);
	}
}
