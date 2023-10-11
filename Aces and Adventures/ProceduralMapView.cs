using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BubblePointerDrag3D))]
public class ProceduralMapView : ATargetView
{
	public static ResourceBlueprint<GameObject> Blueprint = "GameState/Procedural/ProceduralMapView";

	public static ResourceBlueprint<GameObject> EditorBlueprint = "GameState/Procedural/ProceduralMapEditorView";

	[Header("Procedural Map View")]
	public Transform nodeContainer;

	public BoolEvent onInputBlockerEnabled;

	public MaterialEvent onMaterialChange;

	private BubblePointerDrag3D _bubbleDrag;

	private Dictionary<uint, ProceduralNodeView> _nodeViews;

	private bool _inputBlockerEnabled;

	public static ProceduralMapView Instance { get; private set; }

	public ProceduralMap map
	{
		get
		{
			return base.target as ProceduralMap;
		}
		set
		{
			base.target = value;
		}
	}

	public ProceduralGraph graph => map?.graph;

	protected virtual bool _isEditor => false;

	public BubblePointerDrag3D bubbleDrag => this.CacheComponent(ref _bubbleDrag);

	protected Dictionary<uint, ProceduralNodeView> nodeViews => _nodeViews ?? (_nodeViews = new Dictionary<uint, ProceduralNodeView>());

	public bool inputBlockerEnabled
	{
		get
		{
			return _inputBlockerEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _inputBlockerEnabled, value))
			{
				onInputBlockerEnabled?.Invoke(value);
			}
		}
	}

	public ProceduralNodeView this[uint id] => nodeViews.GetValueOrDefault(id);

	public event Action<ProceduralNode, PointerEventData> onNodeClicked;

	public event Action onHideRequested;

	public static ProceduralMapView Create(ProceduralMap map, bool isEditor = false, Transform parent = null)
	{
		return Pools.Unpool(isEditor ? EditorBlueprint : Blueprint, parent).GetComponent<ProceduralMapView>()._SetData(map);
	}

	private ProceduralMapView _SetData(ProceduralMap mapTarget)
	{
		map = mapTarget;
		return this;
	}

	protected virtual ProceduralNodeView _CreateNodeView(ProceduralNode node)
	{
		return nodeViews[node.id] = ProceduralNodeView.Create(this, node, nodeContainer, _isEditor);
	}

	protected virtual ProceduralConnectionView _CreateConnectionView(ProceduralNode node, ProceduralNode connectedNode)
	{
		ProceduralConnectionView proceduralConnectionView = ProceduralConnectionView.Create(this, node, connectedNode, nodeContainer, _isEditor);
		proceduralConnectionView.transform.SetSiblingIndex(0);
		return proceduralConnectionView;
	}

	protected virtual void _OnGraphChanged(ProceduralMap oldMap, ProceduralMap newMap)
	{
		if (oldMap?.graph != null)
		{
			nodeContainer.SetChildrenActive(active: false);
		}
		ProceduralGraph proceduralGraph = newMap?.graph;
		if (proceduralGraph == null || newMap.generateOverTime)
		{
			return;
		}
		foreach (ProceduralNode node in proceduralGraph.GetNodes())
		{
			_CreateNodeView(node);
		}
		foreach (ProceduralNode node2 in proceduralGraph.GetNodes())
		{
			foreach (ProceduralNode connectedNode in proceduralGraph.GetConnectedNodes(node2))
			{
				_CreateConnectionView(node2, connectedNode);
			}
		}
		UpdateMaterial(newMap.data.mapMaterial);
	}

	protected virtual void OnEnable()
	{
		Instance = this;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Instance = ((Instance == this) ? null : Instance);
		map = null;
	}

	protected sealed override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		_OnGraphChanged(oldTarget as ProceduralMap, newTarget as ProceduralMap);
	}

	public IEnumerator GenerateOverTime(ProceduralMap newMap)
	{
		foreach (IGrouping<ProceduralNodeType, ProceduralNode> item in from node in newMap.graph.GetNodes()
			group node by node.type)
		{
			foreach (ProceduralNode item2 in item)
			{
				_CreateNodeView(item2);
			}
			yield return null;
		}
		foreach (ProceduralNode node in newMap.graph.GetNodes())
		{
			foreach (ProceduralNode connectedNode in newMap.graph.GetConnectedNodes(node))
			{
				_CreateConnectionView(node, connectedNode);
			}
		}
		yield return null;
		UpdateMaterial(newMap.data.mapMaterial);
	}

	public virtual void UpdateNodePositions()
	{
		foreach (ProceduralNodeView item in nodeContainer.gameObject.GetComponentsInChildrenPooled<ProceduralNodeView>())
		{
			item.UpdatePosition();
		}
	}

	public virtual void SetDirty()
	{
	}

	public virtual void SignalNodeClick(ProceduralNode node, PointerEventData eventData)
	{
		this.onNodeClicked?.Invoke(node, eventData);
	}

	public Vector3 ToVector3Position(Vector2 localPosition)
	{
		return localPosition.Unproject(AxisType.Z);
	}

	public Vector2 ToVector2Position(Vector3 localPosition)
	{
		return localPosition.Project(AxisType.Z);
	}

	public Vector3 ToLocalPosition(Vector3 worldPosition)
	{
		return nodeContainer.InverseTransformPoint(worldPosition).SetAxis(AxisType.Z);
	}

	public Vector2 ToLocalPositionV2(Vector3 worldPosition)
	{
		return ToVector2Position(ToLocalPosition(worldPosition));
	}

	public Vector2 SnapNodePositionToGrid(Vector2 nodePosition)
	{
		return nodePosition.RoundToNearestMultipleOf(new Vector2(20f, 20f));
	}

	public Vector3 GetSelectableCentroid()
	{
		using PoolKeepItemListHandle<ProceduralNode> poolKeepItemListHandle = Pools.UseKeepItemList<ProceduralNode>();
		foreach (ProceduralNode selectableNode in graph.GetSelectableNodes())
		{
			if (selectableNode.isSelectable)
			{
				poolKeepItemListHandle.Add(selectableNode);
			}
		}
		if (poolKeepItemListHandle.Count == 0)
		{
			ProceduralNode lastSelectedNode = graph.lastSelectedNode;
			if (lastSelectedNode != null)
			{
				poolKeepItemListHandle.Add(lastSelectedNode);
			}
		}
		return poolKeepItemListHandle.value.Select((ProceduralNode node) => nodeContainer.TransformPoint(ToVector3Position(node.position))).Centroid();
	}

	public void RequestHide()
	{
		this.onHideRequested?.Invoke();
	}

	public void HighlightNodesOfType(ProceduralNodeType type, bool highlighted)
	{
		foreach (ProceduralNodeView value in _nodeViews.Values)
		{
			if (value.node.type.IsLikeNode(type))
			{
				value.node.highlighted = highlighted;
			}
		}
	}

	public void CustomHighlightNodesOfType(ProceduralNodeType type, bool customHighlight)
	{
		foreach (ProceduralNodeView value in _nodeViews.Values)
		{
			if (value.node.type == type)
			{
				value.node.customHighlighted = customHighlight;
			}
		}
	}

	public void RefreshPointerOver()
	{
		foreach (ProceduralNodeView value in nodeViews.Values)
		{
			if (value.node.isPointerOver)
			{
				value.SignalPointerOver();
			}
		}
	}

	public void SignalPointerExit()
	{
		foreach (ProceduralNodeView value in nodeViews.Values)
		{
			if (value.node.isPointerOver)
			{
				value.SignalPointerExit();
			}
		}
	}

	public void UpdateMaterial(MapMaterialType mapMaterial)
	{
		onMaterialChange?.Invoke(EnumUtil.GetResourceBlueprint(mapMaterial).GetComponent<MeshRenderer>().sharedMaterial);
	}

	public void UpdateSkin(ProceduralGraphData.Skin skin)
	{
		foreach (ProceduralNodeView value in nodeViews.Values)
		{
			value.onSkinChange?.Invoke((int)skin);
		}
		foreach (ProceduralConnectionView item in nodeContainer.gameObject.GetComponentsInChildrenPooled<ProceduralConnectionView>())
		{
			item.onSkinChange?.Invoke((int)skin);
		}
	}
}
