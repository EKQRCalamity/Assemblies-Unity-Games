using System;
using UnityEngine;

namespace VisualDesignCafe.Rendering.Nature;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(NatureRenderer))]
public class RefreshNatureRenderer : MonoBehaviour
{
	private Terrain _terrain;

	private NatureRenderer _natureRenderer;

	public static RefreshNatureRenderer Instance { get; private set; }

	public Terrain terrain => this.CacheComponent(ref _terrain);

	public NatureRenderer natureRenderer => this.CacheComponent(ref _natureRenderer);

	public float detailDensity
	{
		get
		{
			return terrain.detailObjectDensity;
		}
		set
		{
			if (!(Math.Abs(detailDensity - value) < 0.001f))
			{
				terrain.detailObjectDensity = value;
				_draw = value > 0f && terrain.enabled;
				_shadows = value >= 1f && terrain.enabled;
			}
		}
	}

	private bool _draw
	{
		set
		{
			Camera.main.GetOrAddComponent<NatureRendererCameraSettings>().Render = value;
		}
	}

	private bool _shadows
	{
		set
		{
			Camera.main.GetOrAddComponent<NatureRendererCameraSettings>().RenderShadows = value;
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public void SetDensityAndDraw(float density)
	{
		detailDensity = density;
		base.enabled = true;
	}
}
