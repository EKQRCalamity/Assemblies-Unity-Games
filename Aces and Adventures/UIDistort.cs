using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIDistort : MonoBehaviour
{
	public Material distortMaterialBlueprint;

	private Graphic _graphic;

	private Material _distortMaterial;

	protected Material distortMaterial
	{
		get
		{
			if (!_distortMaterial)
			{
				return _distortMaterial = (distortMaterialBlueprint ? Object.Instantiate(distortMaterialBlueprint) : null);
			}
			return _distortMaterial;
		}
	}

	public Graphic graphic => this.CacheComponent(ref _graphic);

	private void OnEnable()
	{
		if ((bool)graphic)
		{
			graphic.material = distortMaterial;
		}
		if ((bool)distortMaterial)
		{
			distortMaterial.SetFloat(UIGlow.TimeOffset_ID, Random.value * 10000f);
		}
	}

	private void Update()
	{
		if ((bool)distortMaterial)
		{
			distortMaterial.SetFloat(UIGlow.UnscaledTime_ID, Time.unscaledTime);
		}
	}

	private void OnDisable()
	{
		if ((bool)graphic)
		{
			graphic.material = null;
		}
	}
}
