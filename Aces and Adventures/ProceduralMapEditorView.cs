using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ProceduralMapEditorView : ProceduralMapView
{
	public class ClipboardData
	{
		private List<ProceduralNode> _nodes;

		public ClipboardData(IEnumerable<ProceduralNode> nodes)
		{
			_nodes = new List<ProceduralNode>(nodes.Select(ProtoUtil.Clone));
		}

		public IEnumerable<ProceduralNode> Paste(ProceduralGraph graph, Vector2 position)
		{
			using PoolKeepItemDictionaryHandle<uint, uint> idMap = Pools.UseKeepItemDictionary<uint, uint>();
			Vector2 offset = position - _nodes.Select((ProceduralNode node) => node.position).Centroid();
			foreach (ProceduralNode node2 in _nodes.Select(ProtoUtil.Clone))
			{
				uint oldId = node2.id;
				yield return graph.AddNode(node2.ClearDataForCopy(), node2.position + offset);
				idMap[oldId] = node2.id;
			}
			foreach (ProceduralNode node3 in _nodes)
			{
				foreach (uint connection in node3.connections)
				{
					if (idMap.ContainsKey(connection))
					{
						graph.CreateConnection(idMap[node3.id], idMap[connection]);
					}
				}
			}
		}

		public static implicit operator bool(ClipboardData clipboardData)
		{
			if (clipboardData == null)
			{
				return false;
			}
			return clipboardData._nodes?.Count > 0;
		}
	}

	public RectTransform marquee;

	public UnityEvent onDirty;

	private bool _isDirty;

	private Vector3 _beginDragPosition;

	private ClipboardData _clipboardData;

	protected override bool _isEditor => true;

	public event Action<List<uint>> onSelectedNodesChange;

	private void _OnNodeCreated(ProceduralNode node)
	{
		_CreateNodeView(node);
		base.map.graph.MarkAsSelected(node);
		SetDirty();
	}

	private void _OnNodeDeleted(ProceduralNode node)
	{
		base.nodeViews.Remove(node.id);
		SetDirty();
	}

	private void _OnConnectionCreated(ProceduralNode startNode, ProceduralNode endNode)
	{
		_CreateConnectionView(startNode, endNode);
		SetDirty();
	}

	private void _OnConnectionDeleted(ProceduralNode startNode, ProceduralNode endNode)
	{
		SetDirty();
	}

	private void _OnPointerClick(PointerEventData eventData)
	{
		if (!eventData.dragging)
		{
			if (InputManager.I[KeyModifiers.Control])
			{
				base.map.graph.CreateNode(SnapNodePositionToGrid(ToLocalPositionV2(eventData.pointerPressRaycast.worldPosition)));
			}
			else
			{
				base.map.graph.ClearSelected();
			}
		}
	}

	private void _OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			_beginDragPosition = eventData.GetWorldPositionOnPlane(nodeContainer);
			marquee.gameObject.SetActive(value: true);
		}
	}

	private void _OnDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			marquee.SetWorldCornersPreserveScale(new Rect3D(-nodeContainer.forward, nodeContainer.up, _beginDragPosition, eventData.GetWorldPositionOnPlane(nodeContainer)));
		}
	}

	private void _OnEndDrag(PointerEventData eventData)
	{
		if (eventData.button != 0)
		{
			return;
		}
		Rect rect = new Rect3D(marquee).WorldToViewportRect(eventData.pressEventCamera).Project2D();
		if (!InputManager.I[KeyModifiers.Control] && !InputManager.I[KeyModifiers.Shift])
		{
			base.map.graph.ClearSelected();
		}
		foreach (ProceduralNodeView item in nodeContainer.gameObject.GetComponentsInChildrenPooled<ProceduralNodeView>())
		{
			if (rect.Overlaps(new Rect3D(item.transform as RectTransform).WorldToViewportRect(eventData.pressEventCamera).Project2D()))
			{
				if (InputManager.I[KeyModifiers.Control])
				{
					base.map.graph.ToggleSelected(item.node);
				}
				else
				{
					base.map.graph.MarkAsSelected(item.node, clearSelected: false);
				}
			}
		}
		marquee.gameObject.SetActive(value: false);
	}

	private void _CopySelection()
	{
		_clipboardData = new ClipboardData(base.graph.GetSelectedNodes());
	}

	private void _PasteSelection()
	{
		if (!_clipboardData)
		{
			return;
		}
		base.graph.ClearSelected();
		foreach (ProceduralNode item in Pools.UseKeepItemList(_clipboardData.Paste(base.graph, ToLocalPositionV2(nodeContainer.GetPlane(PlaneAxes.XY).Raycast(Camera.main.ScreenPointToRay(Input.mousePosition)).GetValueOrDefault()))))
		{
			base.graph.MarkAsSelected(item, clearSelected: false);
		}
	}

	private void _DuplicateSelection()
	{
		_CopySelection();
		_PasteSelection();
	}

	protected override void Start()
	{
		base.Start();
		base.pointerClick.OnClick.AddListener(_OnPointerClick);
		base.pointerDrag.OnBegin.AddListener(_OnBeginDrag);
		base.pointerDrag.OnDragged.AddListener(_OnDrag);
		base.pointerDrag.OnEnd.AddListener(_OnEndDrag);
	}

	private void Update()
	{
		CanvasInputFocus componentInParent = GetComponentInParent<CanvasInputFocus>();
		if ((object)componentInParent != null && !componentInParent.hasFocus)
		{
			return;
		}
		if (InputManager.I[KeyCode.Delete][KState.JustPressed] && base.graph.GetSelectedNodes().Any())
		{
			foreach (ProceduralNode item in Pools.UseKeepItemList(base.graph.GetSelectedNodes()))
			{
				base.graph.DeleteNode(item.id);
			}
		}
		if (InputManager.I[KeyModifiers.Control])
		{
			if (InputManager.I[KeyCode.C][KState.JustPressed] && base.graph.GetSelectedNodes().Any())
			{
				_CopySelection();
			}
			else if (InputManager.I[KeyCode.V][KState.JustPressed])
			{
				_PasteSelection();
			}
			else if (InputManager.I[KeyCode.D][KState.JustPressed])
			{
				_DuplicateSelection();
			}
		}
	}

	private void LateUpdate()
	{
		if (_isDirty && !(_isDirty = false))
		{
			onDirty?.Invoke();
		}
	}

	public override void SetDirty()
	{
		_isDirty = true;
	}

	protected override void _OnGraphChanged(ProceduralMap oldMap, ProceduralMap newMap)
	{
		base._OnGraphChanged(oldMap, newMap);
		ProceduralGraph proceduralGraph = oldMap?.graph;
		if (proceduralGraph != null)
		{
			proceduralGraph.onNodeCreated -= _OnNodeCreated;
			proceduralGraph.onNodeDeleted -= _OnNodeDeleted;
			proceduralGraph.onConnectionCreated -= _OnConnectionCreated;
			proceduralGraph.onConnectionDeleted -= _OnConnectionDeleted;
			proceduralGraph.onSelectedNodesChange -= this.onSelectedNodesChange;
			this.onSelectedNodesChange?.Invoke(null);
		}
		ProceduralGraph proceduralGraph2 = newMap?.graph;
		if (proceduralGraph2 != null)
		{
			proceduralGraph2.onNodeCreated += _OnNodeCreated;
			proceduralGraph2.onNodeDeleted += _OnNodeDeleted;
			proceduralGraph2.onConnectionCreated += _OnConnectionCreated;
			proceduralGraph2.onConnectionDeleted += _OnConnectionDeleted;
			proceduralGraph2.onSelectedNodesChange += this.onSelectedNodesChange;
		}
	}

	public override void SignalNodeClick(ProceduralNode node, PointerEventData eventData)
	{
		base.SignalNodeClick(node, eventData);
		if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
		{
			if (InputManager.I[KeyModifiers.Control])
			{
				base.map.graph.ToggleSelected(node);
			}
			else
			{
				base.map.graph.MarkAsSelected(node, !InputManager.I[KeyModifiers.Shift]);
			}
		}
	}
}
