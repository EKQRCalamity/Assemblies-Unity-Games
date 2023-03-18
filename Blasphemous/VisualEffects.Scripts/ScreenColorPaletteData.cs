using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VisualEffects.Scripts;

[CreateAssetMenu(fileName = "Screen Color Palette", menuName = "Blasphemous/Screen Color Palette", order = 0)]
public class ScreenColorPaletteData : ScriptableObject
{
	[SerializeField]
	public List<Color> colors;

	[BoxGroup("Generate from Gradient", true, false, 0)]
	public Texture2D paletteTexture;

	[BoxGroup("Generate from Gradient", true, false, 0)]
	[Range(4f, 32f)]
	public int paletteSize = 8;

	private const int MAX_PALETTE_SIZE = 32;

	private readonly Color[] injectingColors = new Color[32];

	[BoxGroup("Generate from Gradient", true, false, 0)]
	[ShowIf("ShouldShowGenerateButton", true)]
	[Button("Generate", ButtonSizes.Medium)]
	private void Generate()
	{
		int width = paletteTexture.width;
		paletteSize = Mathf.Min(width, paletteSize);
		int num = width / (paletteSize - 1);
		colors.Clear();
		for (int i = 0; i < paletteSize; i++)
		{
			colors.Add(paletteTexture.GetPixel(num * i + num / 2, 0));
		}
	}

	private bool ShouldShowGenerateButton()
	{
		return paletteTexture != null;
	}

	public void AdjustColorList()
	{
		while (colors.Count > paletteSize)
		{
			colors.RemoveAt(colors.Count - 1);
		}
	}

	public void Inject(Material mat, bool dithering)
	{
		colors.CopyTo(injectingColors);
		mat.SetColorArray("_ColorPalette", injectingColors);
		mat.SetInt("_PaletteSize", colors.Count);
		if (dithering)
		{
			mat.EnableKeyword("DITHERING");
		}
		else
		{
			mat.DisableKeyword("DITHERING");
		}
	}
}
