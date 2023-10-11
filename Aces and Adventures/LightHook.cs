using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightHook : MonoBehaviour
{
	private Light _light;

	private Light light => this.CacheComponent(ref _light);

	public void SetIntensity(float intensity)
	{
		light.intensity = intensity;
		light.enabled = intensity > 0f;
	}

	public void SetRange(float range)
	{
		light.range = range;
		light.enabled = range > 0f;
	}
}
