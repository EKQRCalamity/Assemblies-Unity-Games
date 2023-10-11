using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class GraphicMaterialAnimator : MonoBehaviour
{
	public string propertyName;

	private Material _material;

	public Graphic graphic => GetComponent<Graphic>();

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

	public void SetPropertyValue(float value)
	{
		material.SetFloat(propertyName, value);
	}
}
