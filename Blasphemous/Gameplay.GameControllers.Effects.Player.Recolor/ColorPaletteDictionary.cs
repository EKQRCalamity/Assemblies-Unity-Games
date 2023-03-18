using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Recolor;

public class ColorPaletteDictionary : ScriptableObject
{
	public List<PalettesById> PalettesById;

	public Sprite GetPalette(string id)
	{
		PalettesById palettesById = PalettesById.Find((PalettesById x) => x.id == id);
		if (palettesById.id == string.Empty)
		{
			Debug.LogError($"Palette with id {id} not found");
		}
		return palettesById.paletteTex;
	}

	public Sprite GetPreview(string id)
	{
		PalettesById palettesById = PalettesById.Find((PalettesById x) => x.id == id);
		if (palettesById.id == string.Empty)
		{
			Debug.LogError($"Palette with id {id} not found");
		}
		return palettesById.palettePreview;
	}

	public List<string> GetAllIds()
	{
		List<string> list = new List<string>();
		foreach (PalettesById item in PalettesById)
		{
			list.Add(item.id);
		}
		return list;
	}

	public List<Sprite> GetAllPalettes()
	{
		List<Sprite> list = new List<Sprite>();
		foreach (PalettesById item in PalettesById)
		{
			list.Add(item.paletteTex);
		}
		return list;
	}
}
