using System;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RendererFitter : MonoBehaviour
{
	public RectTransform fitInto;

	public AxisType rendererForwardAxis = AxisType.Z;

	public float offsetAlongRenderAxis = 0.01f;

	public bool preserveRendererAspect;

	public Vector2 pivot = new Vector2(0.5f, 0.5f);

	private Rect? _lastRect;

	private Bounds? _rendererBounds;

	private float _rendererForwardAxisScale;

	private Quaternion _initialLocalRotation;

	private void Start()
	{
		_initialLocalRotation = base.transform.localRotation;
		fitInto = base.transform.parent as RectTransform;
		_rendererBounds = GetComponent<Renderer>().bounds;
		if (_rendererBounds.Value == default(Bounds))
		{
			_rendererBounds = null;
		}
		_rendererForwardAxisScale = base.transform.GetWorldScale()[(int)rendererForwardAxis];
	}

	private void _Fit()
	{
		_lastRect = fitInto.rect;
		Vector2 v = _rendererBounds.Value.size.Project(rendererForwardAxis);
		Vector2 vector = _rendererBounds.Value.min.Project(rendererForwardAxis);
		Vector2 vector2 = _rendererBounds.Value.max.Project(rendererForwardAxis);
		Vector2 vector3 = ((vector + vector2) * 0.5f).Multiply(v.Inverse());
		Vector2 v2 = pivot - new Vector2(0.5f, 0.5f) + vector3;
		Vector2 v3 = _lastRect.Value.size.Multiply(fitInto.GetWorldScale().Abs().Project(AxisType.Z));
		Vector2 multiplier = Vector2.one;
		if (preserveRendererAspect)
		{
			multiplier = ((v.x > v.y) ? new Vector2(1f, v.y / v.x) : new Vector2(v.x / v.y, 1f));
		}
		v3 = v3.Multiply(multiplier);
		base.transform.rotation = fitInto.rotation * _initialLocalRotation;
		base.transform.position = fitInto.position + base.transform.GetAxis(rendererForwardAxis) * offsetAlongRenderAxis * Math.Sign(base.transform.localScale[(int)rendererForwardAxis]) - v2.Multiply(v3).Unproject(rendererForwardAxis);
		base.transform.SetWorldScale(v3.Multiply(v.Inverse()).Unproject(rendererForwardAxis, _rendererForwardAxisScale));
	}

	private void LateUpdate()
	{
		if (_rendererBounds.HasValue && (!_lastRect.HasValue || fitInto.rect != _lastRect.Value))
		{
			_Fit();
		}
	}

	public void SetFitIntoRect(object rt)
	{
		fitInto = rt as RectTransform;
		_lastRect = null;
	}

	public void SetRenderer(object renderer)
	{
		_rendererBounds = (renderer as Renderer).bounds;
		_rendererForwardAxisScale = base.transform.GetWorldScale()[(int)rendererForwardAxis];
		_lastRect = null;
	}

	public void SetMesh(Mesh mesh)
	{
		_rendererBounds = mesh.bounds;
		_rendererForwardAxisScale = base.transform.GetWorldScale()[(int)rendererForwardAxis];
		_lastRect = null;
	}

	public void SetRendererForwardAxis(object axis)
	{
		rendererForwardAxis = (AxisType)axis;
		_rendererForwardAxisScale = base.transform.GetWorldScale()[(int)rendererForwardAxis];
		_lastRect = null;
	}
}
