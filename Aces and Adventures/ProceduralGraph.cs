using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class ProceduralGraph
{
	[ProtoMember(1, OverwriteList = true)]
	private Dictionary<uint, ProceduralNode> _nodes;

	[ProtoMember(2, OverwriteList = true)]
	private List<uint> _completedNodes;

	[ProtoMember(3)]
	private List<uint> _selectedNodes;

	[ProtoMember(4, OverwriteList = true)]
	private HashSet<uint> _preventRepeatedNodeData;

	private uint _lastNodeId;

	private uint _lastSelectedNode
	{
		get
		{
			List<uint> list = _selectedNodes;
			if (list == null || list.Count <= 0)
			{
				return 0u;
			}
			return _selectedNodes[_selectedNodes.Count - 1];
		}
	}

	public ProceduralNode lastSelectedNode => nodes.GetValueOrDefault(_lastSelectedNode);

	public ProceduralNode lastCompleted
	{
		get
		{
			if (completedNodes.Count != 0)
			{
				return nodes[completedNodes.Last()];
			}
			return null;
		}
	}

	private Dictionary<uint, ProceduralNode> nodes => _nodes ?? (_nodes = new Dictionary<uint, ProceduralNode>());

	private List<uint> completedNodes => _completedNodes ?? (_completedNodes = new List<uint>());

	private List<uint> selectedNodes => _selectedNodes ?? (_selectedNodes = new List<uint>());

	private HashSet<uint> preventRepeatedNodeData => _preventRepeatedNodeData ?? (_preventRepeatedNodeData = new HashSet<uint>());

	public ProceduralNode this[uint id] => nodes.GetValueOrDefault(id);

	public event Action<List<uint>> onSelectedNodesChange;

	public event Action<ProceduralNode> onNodeCreated;

	public event Action<ProceduralNode> onNodeDeleted;

	public event Action<ProceduralNode, ProceduralNode> onConnectionCreated;

	public event Action<ProceduralNode, ProceduralNode> onConnectionDeleted;

	private PoolKeepItemHashSetHandle<uint> _GetAllNodesInFuturePath(ProceduralNode node, PoolKeepItemHashSetHandle<uint> nodesInPath = null)
	{
		if (nodesInPath == null)
		{
			nodesInPath = Pools.UseKeepItemHashSet<uint>();
		}
		foreach (ProceduralNode connectedNode in GetConnectedNodes(node))
		{
			if (nodesInPath.Add(connectedNode.id))
			{
				_GetAllNodesInFuturePath(connectedNode, nodesInPath);
			}
		}
		return nodesInPath;
	}

	private void _SignalCompletedChange(ProceduralNode completedNode)
	{
		completedNode.isCompleted = true;
		foreach (ProceduralNode selectableNode in GetSelectableNodes())
		{
			selectableNode.isSelectable = true;
		}
		using PoolKeepItemHashSetHandle<uint> poolKeepItemHashSetHandle = _GetAllNodesInFuturePath(completedNode);
		foreach (ProceduralNode value in nodes.Values)
		{
			if (!value.isCompleted && !poolKeepItemHashSetHandle.Contains(value.id))
			{
				value.isInactive = true;
			}
		}
	}

	private uint _GetNewNodeId()
	{
		while (nodes.ContainsKey(++_lastNodeId))
		{
		}
		return _lastNodeId;
	}

	public void PrepareDataForSave()
	{
		foreach (ProceduralNode value in nodes.Values)
		{
			value.PrepareDataForSave();
		}
		_completedNodes = null;
		ClearSelected();
		_preventRepeatedNodeData = null;
	}

	public ProceduralGraph GenerateGraph(GameState state)
	{
		System.Random random = state.random;
		ProceduralGraph proceduralGraph2 = (state.graph = ProtoUtil.Clone(this));
		foreach (ProceduralNode item in random.Items(proceduralGraph2.nodes.EnumerateValuesSafe().AsEnumerable()))
		{
			item.Generate(random, proceduralGraph2, state);
		}
		foreach (ProceduralNode item2 in Pools.UseKeepItemList(proceduralGraph2.GetNodes()))
		{
			item2.PostProcessGraph(random, proceduralGraph2);
		}
		proceduralGraph2._preventRepeatedNodeData = null;
		return proceduralGraph2;
	}

	public IEnumerable<ProceduralNode> GetNodes()
	{
		return nodes.Values;
	}

	public IEnumerable<ProceduralNode> GetStartNodes()
	{
		using PoolKeepItemHashSetHandle<uint> allConnections = Pools.UseKeepItemHashSet<uint>();
		foreach (ProceduralNode value in nodes.Values)
		{
			foreach (uint connection in value.connections)
			{
				allConnections.Add(connection);
			}
		}
		foreach (ProceduralNode value2 in nodes.Values)
		{
			if (!allConnections.Contains(value2.id))
			{
				yield return value2;
			}
		}
	}

	public IEnumerable<ProceduralNode> GetCompletedNodes()
	{
		foreach (uint completedNode in completedNodes)
		{
			ProceduralNode valueOrDefault = nodes.GetValueOrDefault(completedNode);
			if (valueOrDefault != null)
			{
				yield return valueOrDefault;
			}
		}
	}

	public IEnumerable<ProceduralNode> GetSelectedNodes()
	{
		foreach (uint selectedNode in selectedNodes)
		{
			ProceduralNode valueOrDefault = nodes.GetValueOrDefault(selectedNode);
			if (valueOrDefault != null)
			{
				yield return valueOrDefault;
			}
		}
	}

	public bool IsSelectedNode(ProceduralNode node)
	{
		return selectedNodes.Contains(node.id);
	}

	public bool ConnectionSelected(ProceduralNode start, ProceduralNode end)
	{
		for (int i = 0; i < selectedNodes.Count - 1; i++)
		{
			if (selectedNodes[i] == start.id && selectedNodes[i + 1] == end.id)
			{
				return true;
			}
		}
		return false;
	}

	public ProceduralNode GetPreviousCompleted(ProceduralNode completedNode)
	{
		int num = completedNodes.IndexOf(completedNode.id);
		if (num <= 0)
		{
			return lastCompleted;
		}
		return this[completedNodes[num - 1]];
	}

	public ProceduralNode GetPreviousSelected(ProceduralNode node)
	{
		int num = selectedNodes.IndexOf(node.id);
		if (num <= 0)
		{
			return lastSelectedNode;
		}
		return this[selectedNodes[num - 1]];
	}

	public IEnumerable<ProceduralNode> GetSelectableNodes()
	{
		if (_lastSelectedNode != 0 && completedNodes.LastValue() != _lastSelectedNode)
		{
			yield break;
		}
		if (completedNodes.IsNullOrEmpty())
		{
			foreach (ProceduralNode startNode in GetStartNodes())
			{
				yield return startNode;
			}
			yield break;
		}
		foreach (ProceduralNode connectedNode in GetConnectedNodes(nodes[completedNodes.Last()]))
		{
			if (!completedNodes.Contains(connectedNode.id))
			{
				yield return connectedNode;
			}
		}
	}

	public bool IsSelectableNode(ProceduralNode node)
	{
		using PoolKeepItemHashSetHandle<ProceduralNode> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet(GetSelectableNodes());
		return poolKeepItemHashSetHandle.Contains(node);
	}

	public IEnumerable<ProceduralNode> GetConnectedNodes(ProceduralNode node)
	{
		foreach (uint connection in node.connections)
		{
			ProceduralNode valueOrDefault = nodes.GetValueOrDefault(connection);
			if (valueOrDefault != null)
			{
				yield return valueOrDefault;
			}
		}
	}

	public bool MarkAsSelected(ProceduralNode node, bool clearSelected = true)
	{
		if (clearSelected)
		{
			ClearSelected();
		}
		if (node == null || selectedNodes.Contains(node.id))
		{
			return false;
		}
		if (!clearSelected)
		{
			foreach (ProceduralNode selectableNode in GetSelectableNodes())
			{
				selectableNode.isSelectable = false;
			}
		}
		ProceduralNode proceduralNode = lastSelectedNode;
		if (proceduralNode != null)
		{
			proceduralNode.isLastSelected = false;
		}
		selectedNodes.Add(node.id);
		node.isSelected = true;
		node.isLastSelected = true;
		this.onSelectedNodesChange?.Invoke(selectedNodes);
		return true;
	}

	public bool RemoveFromSelected(ProceduralNode node)
	{
		if (node != null)
		{
			int num = selectedNodes.IndexOf(node.id);
			if (num >= 0)
			{
				selectedNodes.RemoveAt(num);
				node.isSelected = false;
				this.onSelectedNodesChange?.Invoke(selectedNodes);
				return true;
			}
		}
		return false;
	}

	public void ToggleSelected(ProceduralNode node)
	{
		if (selectedNodes.Contains(node.id))
		{
			RemoveFromSelected(node);
		}
		else
		{
			MarkAsSelected(node, clearSelected: false);
		}
	}

	public void ClearSelected()
	{
		foreach (uint selectedNode in selectedNodes)
		{
			this[selectedNode].isSelected = false;
		}
		selectedNodes.Clear();
		this.onSelectedNodesChange?.Invoke(selectedNodes);
	}

	public bool MarkAsCompleted(ProceduralNode node)
	{
		if (completedNodes.Contains(node.id))
		{
			return false;
		}
		completedNodes.Add(node.id);
		_SignalCompletedChange(node);
		return true;
	}

	public ProceduralNode CreateNode(Vector2 position)
	{
		return AddNode(new ProceduralNode(_GetNewNodeId())
		{
			position = position
		});
	}

	public ProceduralNode AddNode(ProceduralNode newNode, Vector2? position = null)
	{
		if (position.HasValue)
		{
			newNode.position = position.Value;
		}
		if (!newNode.hasValidId)
		{
			newNode.SetId(_GetNewNodeId());
		}
		nodes.Add(newNode.id, newNode);
		this.onNodeCreated?.Invoke(newNode);
		return newNode;
	}

	public void DeleteNode(uint nodeId, System.Random random = null, float patchConnectionChance = -1f)
	{
		ProceduralNode proceduralNode = this[nodeId];
		if (proceduralNode == null)
		{
			return;
		}
		RemoveFromSelected(proceduralNode);
		foreach (ProceduralNode value in nodes.Values)
		{
			if (patchConnectionChance > 0f && value.connections.Contains(nodeId))
			{
				foreach (uint connection in proceduralNode.connections)
				{
					if (random.Chance(patchConnectionChance))
					{
						CreateConnection(value.id, connection);
					}
				}
			}
			DeleteConnection(value.id, nodeId);
		}
		foreach (uint item in proceduralNode.connections.EnumerateSafe())
		{
			DeleteConnection(nodeId, item);
		}
		nodes.Remove(nodeId);
		this.onNodeDeleted?.Invoke(proceduralNode);
	}

	public void CreateConnection(uint startNodeId, uint endNodeId)
	{
		if (startNodeId == endNodeId)
		{
			return;
		}
		ProceduralNode proceduralNode = this[startNodeId];
		if (proceduralNode != null)
		{
			ProceduralNode proceduralNode2 = this[endNodeId];
			if (proceduralNode2 != null && proceduralNode.connections.Add(endNodeId))
			{
				this.onConnectionCreated?.Invoke(proceduralNode, proceduralNode2);
			}
		}
	}

	public void DeleteConnection(uint startNodeId, uint endNodeId)
	{
		if (startNodeId == endNodeId)
		{
			return;
		}
		ProceduralNode proceduralNode = this[startNodeId];
		if (proceduralNode != null)
		{
			ProceduralNode proceduralNode2 = this[endNodeId];
			if (proceduralNode2 != null && proceduralNode.connections.Remove(endNodeId))
			{
				this.onConnectionDeleted?.Invoke(proceduralNode, proceduralNode2);
			}
		}
	}

	public bool PreventRepeatedNodeDataContains(DataRef<ProceduralNodeData> node)
	{
		return preventRepeatedNodeData.Contains(node);
	}

	public bool PreventRepeatedNodeDataAdd(DataRef<ProceduralNodeData> node)
	{
		return preventRepeatedNodeData.Add(node);
	}

	public bool PreventRepeatedNodeDataAddTrue(DataRef<ProceduralNodeData> node)
	{
		return preventRepeatedNodeData.AddTrue(node);
	}
}
