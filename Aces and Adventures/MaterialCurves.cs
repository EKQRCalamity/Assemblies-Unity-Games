using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialCurves : ACurves
{
	public ColorCurve color;

	private Material _material;

	private int _colorId;

	protected Material material
	{
		get
		{
			if (!(_material != null))
			{
				return _InitializeValues();
			}
			return _material;
		}
	}

	private Material _InitializeValues()
	{
		_material = GetComponent<Renderer>().material;
		_colorId = Shader.PropertyToID(_material.FindColor());
		color.initialValue = _material.GetColor(_colorId);
		return _material;
	}

	private void Awake()
	{
		_ = material;
	}

	protected override void _Input(float t)
	{
		if (color.enabled)
		{
			material.SetColor(_colorId, color.GetValue(material.GetColor(_colorId), t));
		}
	}

	public void SetMaterialColor(Color color)
	{
		material.SetColor(_colorId, color);
	}

	public void SetMaterialRGB(Color rgb)
	{
		material.SetColor(_colorId, rgb.SetAlpha(material.GetColor(_colorId).a));
	}
}
