using UnityEngine;
using UnityEngine.UI;

public class UIGridGraphic : MaskableGraphic
{
	[SerializeField]
	protected bool _useImageSlices;

	[SerializeField]
	protected Sprite _sprite;

	[SerializeField]
	[Range(1f, 1000f)]
	protected float _gridSizeX = 10f;

	[SerializeField]
	[Range(1f, 1000f)]
	protected float _gridSizeY = 10f;

	public bool useImageSlices
	{
		get
		{
			return _useImageSlices;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _useImageSlices, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public Sprite sprite
	{
		get
		{
			return _sprite;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _sprite, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public float gridSizeX
	{
		get
		{
			return _gridSizeX;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _gridSizeX, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public float gridSizeY
	{
		get
		{
			return _gridSizeY;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _gridSizeY, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public Vector2 gridSize
	{
		get
		{
			return new Vector2(gridSizeX, gridSizeY);
		}
		set
		{
			gridSizeX = value.x;
			gridSizeY = value.y;
		}
	}

	public override Texture mainTexture
	{
		get
		{
			if (!sprite)
			{
				if (!material || !material.mainTexture)
				{
					return Graphic.s_WhiteTexture;
				}
				return material.mainTexture;
			}
			return sprite.texture;
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		if ((bool)sprite)
		{
			Rect rect = base.rectTransform.rect;
			Vector2 vector = rect.size.Multiply(gridSize.Inverse()) * 0.5f;
			Vector4 drawingDimensions = this.GetDrawingDimensions(sprite, rect, shouldPreserveAspect: false);
			Vector4 vector2 = new Vector4(0f - vector.x, 0f - vector.y, vector.x, vector.y);
			vh.AddVert(new Vector3(drawingDimensions.x, drawingDimensions.y), color, new Vector2(vector2.x, vector2.y));
			vh.AddVert(new Vector3(drawingDimensions.x, drawingDimensions.w), color, new Vector2(vector2.x, vector2.w));
			vh.AddVert(new Vector3(drawingDimensions.z, drawingDimensions.w), color, new Vector2(vector2.z, vector2.w));
			vh.AddVert(new Vector3(drawingDimensions.z, drawingDimensions.y), color, new Vector2(vector2.z, vector2.y));
			vh.AddTriangle(0, 1, 2);
			vh.AddTriangle(2, 3, 0);
		}
	}
}
