using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialHook : MonoBehaviour
{
	private static int? _ALPHA_ID;

	private Material _material;

	private static int ALPHA_ID
	{
		get
		{
			int valueOrDefault = _ALPHA_ID.GetValueOrDefault();
			if (!_ALPHA_ID.HasValue)
			{
				valueOrDefault = Shader.PropertyToID("_Alpha");
				_ALPHA_ID = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public Material material => _material ?? (_material = GetComponent<Renderer>().material);

	private string _name => (material?.name ?? base.gameObject?.name ?? "N/A") + " (Material)";

	public void SetMainTexture(Texture2D texture)
	{
		material.mainTexture = texture;
	}

	public void SetMainColor(Color color)
	{
		material.color = color;
	}

	public void SetEmissiveColor(Color color)
	{
		material.SetColor("_EmissionColor", color);
	}

	public void SetEmissiveColorHDRP(Color color)
	{
		material.SetColor("_EmissiveColor", color);
	}

	public void SetAlpha(float alpha)
	{
		material.SetFloat(ALPHA_ID, Mathf.Clamp01(alpha));
	}

	public void SetAlphaSquared(float alpha)
	{
		material.SetFloat(ALPHA_ID, Mathf.Clamp01(alpha * alpha));
	}

	public void SetAlphaCubed(float alpha)
	{
		material.SetFloat(ALPHA_ID, Mathf.Clamp01(alpha * alpha * alpha));
	}

	public void SetProperty(string propertyName, float value)
	{
		if (material.HasFloat(propertyName))
		{
			material.SetFloat(propertyName, value);
		}
	}

	public void SetProperty(string propertyName, Color color)
	{
		if (material.HasColor(propertyName))
		{
			material.SetColor(propertyName, color);
		}
	}

	public void Destroy()
	{
		Object.DestroyImmediate(this);
	}
}
