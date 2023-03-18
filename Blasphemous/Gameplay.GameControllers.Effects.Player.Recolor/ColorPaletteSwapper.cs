using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Recolor;

public class ColorPaletteSwapper : MonoBehaviour
{
	private const string ColorPaletteResource = "";

	public Material extraMaterial;

	private void Start()
	{
		SetMaterial();
	}

	private void SetMaterial()
	{
		Sprite currentColorPaletteSprite = Core.ColorPaletteManager.GetCurrentColorPaletteSprite();
		if (!(currentColorPaletteSprite != null))
		{
			return;
		}
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		if (component != null)
		{
			component.material.SetTexture("_PaletteTex", currentColorPaletteSprite.texture);
			if ((bool)extraMaterial)
			{
				extraMaterial.SetTexture("_PaletteTex", currentColorPaletteSprite.texture);
			}
		}
		else
		{
			Debug.LogError("There is no sprite renderer attached");
		}
	}
}
