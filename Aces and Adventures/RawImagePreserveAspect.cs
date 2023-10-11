using UnityEngine;
using UnityEngine.UI;

public class RawImagePreserveAspect : RawImage
{
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		Texture texture = mainTexture;
		if ((bool)texture)
		{
			Rect pixelAdjustedRect = GetPixelAdjustedRect();
			Rect rect = base.uvRect;
			Vector2 pivot = base.rectTransform.pivot;
			pivot.y *= -1f;
			pixelAdjustedRect = pixelAdjustedRect.GetOptimalInscirbedAspectRatioRect(this.UVPixelAspectRatio(), pivot);
			Vector4 vector = new Vector4(pixelAdjustedRect.x, pixelAdjustedRect.y, pixelAdjustedRect.x + pixelAdjustedRect.width, pixelAdjustedRect.y + pixelAdjustedRect.height);
			Vector2 vector2 = new Vector2((float)texture.width * texture.texelSize.x, (float)texture.height * texture.texelSize.y);
			Color32 color = this.color;
			vh.AddVert(new Vector3(vector.x, vector.y), color, new Vector2(rect.xMin * vector2.x, rect.yMin * vector2.y));
			vh.AddVert(new Vector3(vector.x, vector.w), color, new Vector2(rect.xMin * vector2.x, rect.yMax * vector2.y));
			vh.AddVert(new Vector3(vector.z, vector.w), color, new Vector2(rect.xMax * vector2.x, rect.yMax * vector2.y));
			vh.AddVert(new Vector3(vector.z, vector.y), color, new Vector2(rect.xMax * vector2.x, rect.yMin * vector2.y));
			vh.AddTriangle(0, 1, 2);
			vh.AddTriangle(2, 3, 0);
		}
	}
}
