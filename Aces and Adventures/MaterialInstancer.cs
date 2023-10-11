using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialInstancer : MonoBehaviour
{
	private static Dictionary<Material, Dictionary<Color, Material>> _MaterialsByColor = new Dictionary<Material, Dictionary<Color, Material>>();

	private static Dictionary<Material, Dictionary<Texture, Material>> _MaterialsByTexture = new Dictionary<Material, Dictionary<Texture, Material>>();

	private Renderer _renderer;

	private Material _material;

	public Renderer renderer
	{
		get
		{
			if (!_renderer)
			{
				return _renderer = GetComponent<Renderer>();
			}
			return _renderer;
		}
	}

	public Material material
	{
		get
		{
			if (!_material)
			{
				return _material = renderer.sharedMaterial;
			}
			return _material;
		}
	}

	private static Material _GetMaterial(Material sharedMaterial, Color color, string colorName = null)
	{
		if (!_MaterialsByColor.ContainsKey(sharedMaterial))
		{
			_MaterialsByColor.Add(sharedMaterial, new Dictionary<Color, Material>());
		}
		if (!_MaterialsByColor[sharedMaterial].ContainsKey(color))
		{
			Material material = Object.Instantiate(sharedMaterial);
			if (colorName == null)
			{
				material.color = color;
			}
			else
			{
				material.SetColor(colorName, color);
			}
			_MaterialsByColor[sharedMaterial].Add(color, material);
		}
		return _MaterialsByColor[sharedMaterial][color];
	}

	private static Material _GetMaterial(Material sharedMaterial, Texture texture)
	{
		if (!_MaterialsByTexture.ContainsKey(sharedMaterial))
		{
			_MaterialsByTexture.Add(sharedMaterial, new Dictionary<Texture, Material>());
		}
		if (!_MaterialsByTexture[sharedMaterial].ContainsKey(texture))
		{
			Material material = Object.Instantiate(sharedMaterial);
			material.mainTexture = texture;
			_MaterialsByTexture[sharedMaterial].Add(texture, material);
		}
		return _MaterialsByTexture[sharedMaterial][texture];
	}

	public static void Clear()
	{
		_MaterialsByColor.Clear();
		_MaterialsByTexture.Clear();
	}

	public void SetMainColor(Color color)
	{
		renderer.sharedMaterial = _GetMaterial(material, color);
	}

	public void SetParticleTint(Color color)
	{
		renderer.sharedMaterial = _GetMaterial(material, color, "_TintColor");
	}

	public void SetMainTexture(Texture texture)
	{
		renderer.sharedMaterial = _GetMaterial(material, texture);
	}
}
