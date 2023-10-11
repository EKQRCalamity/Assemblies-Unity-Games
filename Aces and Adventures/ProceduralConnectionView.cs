using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ProceduralConnectionView : MonoBehaviour
{
	private static readonly ResourceBlueprint<GameObject> Blueprint = "GameState/Procedural/ProceduralConnectionView";

	private static readonly ResourceBlueprint<GameObject> EditorBlueprint = "GameState/Procedural/ProceduralConnectionEditorView";

	[Range(1f, 128f)]
	public int width = 32;

	public BoolEvent onActiveChange;

	public BoolEvent onSelectableChange;

	public BoolEvent onSelectedChange;

	public BoolEvent onCompletedChange;

	public IntEvent onSkinChange;

	protected ProceduralMapView _graphView;

	private bool _isActive = true;

	private readonly List<ProceduralNode> _siblingEndNodes = new List<ProceduralNode>();

	public ProceduralGraph graph => _graphView?.map?.graph;

	public ProceduralNode startNode { get; private set; }

	protected uint _startNodeId => startNode?.id ?? 0;

	public ProceduralNode endNode { get; private set; }

	protected uint _endNodeId => endNode?.id ?? 0;

	public static ProceduralConnectionView Create(ProceduralMapView graphView, ProceduralNode startNode, ProceduralNode endNode, Transform parent, bool isEditor = false)
	{
		return Pools.Unpool(isEditor ? EditorBlueprint : Blueprint, parent).GetComponent<ProceduralConnectionView>().SetData(graphView, startNode, endNode);
	}

	private void _OnStartNodeStateChange(ProceduralNode.State previousState, ProceduralNode.State state)
	{
		if (_isActive)
		{
			onActiveChange?.Invoke(_isActive = startNode.isActive && endNode.isActive);
		}
	}

	private void _OnEndNodeStateChange(ProceduralNode.State previousState, ProceduralNode.State state)
	{
		if (_isActive)
		{
			onActiveChange?.Invoke(_isActive = endNode.isActive);
			if (_isActive && graph.GetPreviousSelected(endNode)?.id == _startNodeId)
			{
				onSelectableChange?.Invoke(endNode.isSelectable);
				onSelectedChange?.Invoke(endNode.isSelected);
				onCompletedChange?.Invoke(endNode.isCompleted);
			}
		}
	}

	private void _OnSiblingEndNodeStateChange(ProceduralNode.State previousState, ProceduralNode.State state)
	{
		if (_isActive && EnumUtil.HasFlag(state, ProceduralNode.State.Selected) && !_graphView.graph.ConnectionSelected(startNode, endNode))
		{
			onActiveChange?.Invoke(_isActive = false);
		}
	}

	protected void _UpdatePosition()
	{
		Vector3 vector = _graphView.nodeContainer.TransformPoint(_graphView.ToVector3Position(startNode.position));
		Vector3 vector2 = _graphView.nodeContainer.TransformPoint(_graphView.ToVector3Position(endNode.position));
		Vector3 normalized = (vector2 - vector).normalized;
		vector += normalized * _graphView[_startNodeId].radius;
		vector2 -= normalized * _graphView[_endNodeId].radius;
		GetComponent<RectTransform>().SetWorldCornersPreserveScale(new Rect3D(vector2, vector, -_graphView.nodeContainer.forward, _graphView.nodeContainer.lossyScale.x * (float)width));
	}

	private void _OnNodeScaleChange(float scale)
	{
		_UpdatePosition();
	}

	protected virtual void OnDisable()
	{
		if (startNode != null)
		{
			startNode.onStateChange -= _OnStartNodeStateChange;
			_graphView[startNode.id]?.onScaleChange.RemoveListener(_OnNodeScaleChange);
		}
		if (endNode != null)
		{
			endNode.onStateChange -= _OnEndNodeStateChange;
			_graphView[endNode.id]?.onScaleChange.RemoveListener(_OnNodeScaleChange);
		}
		foreach (ProceduralNode siblingEndNode in _siblingEndNodes)
		{
			siblingEndNode.onStateChange -= _OnSiblingEndNodeStateChange;
		}
		_graphView = null;
		ProceduralNode proceduralNode2 = (endNode = null);
		startNode = proceduralNode2;
		_siblingEndNodes.Clear();
		_isActive = true;
	}

	public virtual ProceduralConnectionView SetData(ProceduralMapView graphView, ProceduralNode startNodeToSet, ProceduralNode endNodeToSet)
	{
		_graphView = graphView;
		startNode = startNodeToSet;
		endNode = endNodeToSet;
		_siblingEndNodes.Clear();
		foreach (ProceduralNode connectedNode in _graphView.graph.GetConnectedNodes(startNodeToSet))
		{
			if (connectedNode != endNode)
			{
				_siblingEndNodes.Add(connectedNode);
				connectedNode.onStateChange += _OnSiblingEndNodeStateChange;
				_OnSiblingEndNodeStateChange(connectedNode.state, connectedNode.state);
			}
		}
		startNode.onStateChange += _OnStartNodeStateChange;
		_OnStartNodeStateChange(startNode.state, startNode.state);
		endNode.onStateChange += _OnEndNodeStateChange;
		_OnEndNodeStateChange(endNode.state, endNode.state);
		graphView[startNode.id].onScaleChange.AddListener(_OnNodeScaleChange);
		graphView[endNode.id].onScaleChange.AddListener(_OnNodeScaleChange);
		_UpdatePosition();
		onSkinChange?.Invoke((int)graphView.map.data.skin);
		return this;
	}
}
