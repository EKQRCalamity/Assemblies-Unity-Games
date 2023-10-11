using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class RayCastTarget : Graphic
{
	public virtual bool visualize
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public override bool raycastTarget
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	protected bool _rayCastTarget
	{
		get
		{
			return base.raycastTarget;
		}
		set
		{
			base.raycastTarget = value;
		}
	}

	public override void SetMaterialDirty()
	{
		if (visualize)
		{
			base.SetMaterialDirty();
		}
	}

	public override void SetVerticesDirty()
	{
		if (visualize)
		{
			base.SetVerticesDirty();
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
	}
}
