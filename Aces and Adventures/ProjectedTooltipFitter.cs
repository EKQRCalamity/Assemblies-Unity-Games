using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ProjectedTooltipFitter : MonoBehaviour
{
	private static readonly ResourceBlueprint<GameObject> TextBlueprint = "UI/Tooltips/ProjectedTooltipText";

	private static Dictionary<GameObject, ProjectedTooltipFitter> _Active;

	public GameObject tooltipCreator;

	public TooltipAlignment alignment;

	public Canvas canvas;

	public int padding;

	public BoolEvent onShowChange;

	private RectTransform _rect;

	private bool _allowGoingOffScreen;

	private static Dictionary<GameObject, ProjectedTooltipFitter> Active => _Active ?? (_Active = new Dictionary<GameObject, ProjectedTooltipFitter>());

	public RectTransform rect => this.CacheComponent(ref _rect);

	public static void Create(string text, GameObject creator, Canvas canvas, TooltipAlignment alignment = TooltipAlignment.TopCenter, int padding = 12, TooltipOptionType tooltipOption = TooltipOptionType.ThreeDimensional)
	{
		if (text.HasVisibleCharacter() && ProfileManager.options.game.ui[tooltipOption])
		{
			ProjectedTooltipFitter valueOrDefault = Active.GetValueOrDefault(creator);
			if ((bool)valueOrDefault)
			{
				valueOrDefault.GetComponentInChildren<TextMeshProUGUI>().text = text;
			}
			else
			{
				GameObject blueprint = TextBlueprint;
				AspectRatioFitter componentInChildren = canvas.GetComponentInChildren<AspectRatioFitter>();
				Pools.Unpool(blueprint, ((object)componentInChildren != null) ? componentInChildren.transform : canvas.transform).GetComponentInChildren<TextMeshProUGUI>().SetTextReturn(text)
					.GetComponentInParent<ProjectedTooltipFitter>()
					._SetData(creator, canvas, alignment, padding);
			}
			Active[creator].Show();
		}
	}

	public static void Finish(GameObject tooltipCreator, bool allowGoingOffScreen = false)
	{
		Active.GetValueOrDefault(tooltipCreator)?.Finish(allowGoingOffScreen);
	}

	private ProjectedTooltipFitter _SetData(GameObject tooltipCreator, Canvas canvas, TooltipAlignment alignment, int padding)
	{
		_allowGoingOffScreen = false;
		this.tooltipCreator = tooltipCreator;
		this.canvas = canvas;
		this.alignment = alignment;
		this.padding = padding;
		Active[tooltipCreator] = this;
		LateUpdate();
		return this;
	}

	private void LateUpdate()
	{
		if (!tooltipCreator || !canvas)
		{
			return;
		}
		using PoolKeepItemListHandle<Vector3> poolKeepItemListHandle = Pools.UseKeepItemList<Vector3>();
		foreach (MeshFilter item in tooltipCreator.GetComponentsInChildrenPooled<MeshFilter>())
		{
			MeshRenderer component = item.GetComponent<MeshRenderer>();
			if ((object)component == null || component.shadowCastingMode == ShadowCastingMode.Off)
			{
				continue;
			}
			foreach (Vector3 vertex in item.sharedMesh.GetVertices())
			{
				poolKeepItemListHandle.Add(item.transform.TransformPoint(vertex));
			}
		}
		if (poolKeepItemListHandle.Count == 0)
		{
			foreach (SkinnedMeshRenderer item2 in tooltipCreator.GetComponentsInChildrenPooled<SkinnedMeshRenderer>())
			{
				if (item2.shadowCastingMode == ShadowCastingMode.Off)
				{
					continue;
				}
				foreach (Vector3 vertex2 in item2.sharedMesh.GetVertices())
				{
					poolKeepItemListHandle.Add(item2.transform.TransformPoint(vertex2));
				}
			}
		}
		if (poolKeepItemListHandle.Count == 0)
		{
			foreach (Graphic item3 in tooltipCreator.GetComponentsInChildrenPooled<Graphic>())
			{
				if (!item3.raycastTarget)
				{
					continue;
				}
				foreach (Vector3 item4 in item3.rectTransform.GetWorldRect3D().Corners())
				{
					poolKeepItemListHandle.Add(item4);
				}
			}
		}
		Camera main = Camera.main;
		Plane plane = ((canvas.renderMode == RenderMode.WorldSpace) ? canvas.transform.GetPlane(PlaneAxes.XY) : main.GetPlane(main.farClipPlane));
		Vector3 position = main.transform.position;
		for (int num = poolKeepItemListHandle.Count - 1; num >= 0; num--)
		{
			Ray ray = new Ray(position, poolKeepItemListHandle[num] - position);
			if (plane.Raycast(ray, out var enter))
			{
				poolKeepItemListHandle[num] = ray.origin + ray.direction * enter;
			}
			else
			{
				poolKeepItemListHandle.RemoveAt(num);
			}
		}
		if (poolKeepItemListHandle.Count >= 3)
		{
			Rect3D rect3D = new Rect3D(poolKeepItemListHandle[0], -main.transform.forward, main.transform.up, new Vector2(0.0001f, 0.0001f));
			for (int i = 1; i < poolKeepItemListHandle.Count; i++)
			{
				rect3D = rect3D.Encapsulate(poolKeepItemListHandle[i]);
			}
			if (canvas.renderMode != RenderMode.WorldSpace)
			{
				rect3D = rect3D.WorldToScreenSpaceRect(main);
			}
			float num2 = ((canvas.renderMode == RenderMode.WorldSpace) ? canvas.transform.lossyScale.Average() : canvas.scaleFactor);
			float num3 = (float)padding * num2;
			rect3D = rect3D.Pad(new Vector2(num3, num3));
			Rect3D rect3D2 = new Rect3D(canvas.transform as RectTransform).Pad(new Vector2(0f - num3, 0f - num3) * 2f);
			Rect3D rect3D3 = new Rect3D(canvas.transform.position, -canvas.transform.forward, canvas.transform.up, new Vector2(LayoutUtility.GetPreferredWidth(rect), LayoutUtility.GetPreferredHeight(rect)) * num2);
			rect3D3 = rect3D3.Translate(rect3D.Lerp(alignment.GetPivot()) - rect3D3.Lerp(alignment.GetPivot().OneMinus()));
			if (!_allowGoingOffScreen)
			{
				rect3D3 = rect3D3.FitIntoRange(rect3D2.min, rect3D2.max);
			}
			rect.SetWorldCornersPreserveScale(rect3D3);
		}
	}

	private void OnDisable()
	{
		if ((bool)tooltipCreator && Active.GetValueOrDefault(tooltipCreator) == this)
		{
			Active.Remove(tooltipCreator);
		}
	}

	public void Show()
	{
		onShowChange?.Invoke(arg0: true);
	}

	public void Finish()
	{
		onShowChange?.Invoke(arg0: false);
	}

	public void Finish(bool allowGoingOffScreen)
	{
		_allowGoingOffScreen = allowGoingOffScreen;
		Finish();
	}
}
