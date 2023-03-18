using UnityEngine;

public class CameraTextureHolder : MonoBehaviour
{
	public RenderTexture _renderTexture;

	public float distanceFactor = 0.5f;

	private void OnEnable()
	{
		if ((bool)_renderTexture && !_renderTexture.IsCreated())
		{
			_renderTexture.Create();
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		int srcY = Mathf.RoundToInt((float)src.height * distanceFactor);
		int srcHeight = Mathf.RoundToInt((float)src.height * (1f - distanceFactor));
		if ((bool)_renderTexture)
		{
			Graphics.CopyTexture(src, 0, 0, 0, srcY, src.width, srcHeight, _renderTexture, 0, 0, 0, 0);
		}
		Graphics.Blit(src, dest);
	}
}
