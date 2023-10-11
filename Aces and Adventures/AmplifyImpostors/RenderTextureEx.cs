using UnityEngine;

namespace AmplifyImpostors;

public static class RenderTextureEx
{
	public static RenderTexture GetTemporary(RenderTexture renderTexture)
	{
		return RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, renderTexture.depth, renderTexture.format);
	}
}
