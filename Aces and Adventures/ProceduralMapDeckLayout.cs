using UnityEngine.EventSystems;

public class ProceduralMapDeckLayout : ADeckLayout<ProceduralMap.Pile, ProceduralMap>
{
	public ACardLayout closed;

	public ACardLayout active;

	public ACardLayout hidden;

	private DragPlane3D _dragPlane;

	public DragPlane3D dragPlane => active.CacheComponentInParent(ref _dragPlane);

	protected override ACardLayout this[ProceduralMap.Pile? pile]
	{
		get
		{
			return pile switch
			{
				ProceduralMap.Pile.Closed => closed, 
				ProceduralMap.Pile.Active => active, 
				ProceduralMap.Pile.Hidden => hidden, 
				_ => null, 
			};
		}
		set
		{
			if (pile.HasValue)
			{
				switch (pile.GetValueOrDefault())
				{
				case ProceduralMap.Pile.Closed:
					closed = value;
					break;
				case ProceduralMap.Pile.Active:
					active = value;
					break;
				case ProceduralMap.Pile.Hidden:
					hidden = value;
					break;
				}
			}
		}
	}

	private void _OnDragPlaneBeginDrag(PointerEventData eventData)
	{
		ProceduralMapView.Instance?.SignalPointerExit();
	}

	private void _OnDragPlaneEndDrag(PointerEventData eventData)
	{
		ProceduralMapView.Instance?.RefreshPointerOver();
	}

	private void Start()
	{
		PointerDrag3D component = dragPlane.GetComponent<PointerDrag3D>();
		component.OnBegin.AddListener(_OnDragPlaneBeginDrag);
		component.OnEnd.AddListener(_OnDragPlaneEndDrag);
	}

	protected override CardLayoutElement _CreateView(ProceduralMap value)
	{
		return ProceduralMapView.Create(value);
	}
}
