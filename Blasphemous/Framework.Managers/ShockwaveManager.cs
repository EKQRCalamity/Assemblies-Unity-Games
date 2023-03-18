using System.Collections;
using UnityEngine;

namespace Framework.Managers;

public class ShockwaveManager : MonoBehaviour
{
	public float maxRadius = 1f;

	public AnimationCurve curve;

	public Material shockWaveMaterial;

	private Vector3 lastWorldPos;

	private void Start()
	{
		shockWaveMaterial.SetFloat("_Radius", -0.2f);
	}

	public void Shockwave(Vector3 worldpos, float maxSeconds, float minScreenRadius, float maxScreenRadius)
	{
		SetCenter(worldpos);
		StopAllCoroutines();
		StartCoroutine(ShockWaveEffect(maxSeconds, minScreenRadius, maxScreenRadius));
	}

	private void SetCenter(Vector3 worldpos)
	{
		Camera gameCamera = Core.Logic.CameraManager.ProCamera2D.GameCamera;
		lastWorldPos = worldpos;
		Vector2 vector = gameCamera.WorldToViewportPoint(worldpos);
		shockWaveMaterial.SetFloat("_CenterX", vector.x);
		shockWaveMaterial.SetFloat("_CenterY", vector.y);
	}

	private IEnumerator ShockWaveEffect(float seconds, float minRadius, float radius)
	{
		float counter = 0f;
		while (counter < seconds)
		{
			counter += Time.deltaTime;
			float v = curve.Evaluate(counter / seconds);
			float waveRadius = Mathf.Lerp(minRadius, radius, v);
			shockWaveMaterial.SetFloat("_Radius", waveRadius);
			SetCenter(lastWorldPos);
			yield return null;
		}
		shockWaveMaterial.SetFloat("_Radius", -0.2f);
	}
}
