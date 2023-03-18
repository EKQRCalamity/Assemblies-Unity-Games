using UnityEngine;

namespace Gameplay.GameControllers.Camera;

[ExecuteInEditMode]
public class CRT : MonoBehaviour
{
	public Material material;

	private void Start()
	{
		Debug.Log(Shader.Find("Custom/CRT"));
		material = new Material(Shader.Find("Custom/CRT"));
	}

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetTexture("_MainTex", source);
		Graphics.Blit(source, destination, material);
	}
}
