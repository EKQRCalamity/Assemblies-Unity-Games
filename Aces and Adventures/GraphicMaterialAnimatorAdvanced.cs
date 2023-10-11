using UnityEngine;
using UnityEngine.UI;

public class GraphicMaterialAnimatorAdvanced : MonoBehaviour
{
	[SerializeField]
	protected Graphic _graphic;

	public string[] propertyNames;

	private Material _material;

	public Graphic graphic => _graphic;

	public Material material
	{
		get
		{
			if (!_material)
			{
				Material material2 = (graphic.material = Object.Instantiate(graphic.material));
				return _material = material2;
			}
			return _material;
		}
	}

	public void SetAllPropertyValues(float value)
	{
		string[] array = propertyNames;
		foreach (string text in array)
		{
			material.SetFloat(text, value);
		}
	}

	public void SetAllButLastPropertyValues(float value)
	{
		for (int i = 0; i < propertyNames.Length - 1; i++)
		{
			material.SetFloat(propertyNames[i], value);
		}
	}

	public void SetLastPropertyValue(float value)
	{
		material.SetFloat(propertyNames[^1], value);
	}
}
