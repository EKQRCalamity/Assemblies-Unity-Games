using UnityEngine;
using UnityEngine.UI;

public class RayCastTargetTriangle : RayCastTarget, ICanvasRaycastFilter
{
	[SerializeField]
	protected bool _visualize;

	private Triangle _tri;

	public override bool visualize
	{
		get
		{
			return _visualize;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _visualize, value))
			{
				SetVerticesDirty();
			}
		}
	}

	public override bool raycastTarget
	{
		get
		{
			return base._rayCastTarget;
		}
		set
		{
			base._rayCastTarget = value;
		}
	}

	public Triangle tri
	{
		get
		{
			return _tri;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _tri, value))
			{
				_OnTriangleChange();
			}
		}
	}

	private void _OnTriangleChange()
	{
		base.rectTransform.SetWorldCorners(tri.ToRect3D());
		base.rectTransform.forward = base.transform.parent.forward;
		if (visualize)
		{
			SetVerticesDirty();
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		base.OnPopulateMesh(vh);
		if (visualize)
		{
			Triangle triangle = tri.Transform(base.transform.worldToLocalMatrix).ToClockwiseTriangle();
			vh.AddVert(triangle.a, color, new Vector2(0f, 0f));
			vh.AddVert(triangle.b, color, new Vector2(0f, 1f));
			vh.AddVert(triangle.c, color, new Vector2(1f, 1f));
			vh.AddTriangle(0, 1, 2);
		}
	}

	public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
	{
		if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(base.rectTransform, sp, eventCamera, out var worldPoint))
		{
			return false;
		}
		return tri.Contains(worldPoint);
	}
}
