using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProtoBuf;

[ProtoContract]
[UIField]
public class ProceduralNodePack
{
	public struct Selection
	{
		public readonly DataRef<ProceduralNodeData> node;

		public readonly double weight;

		public readonly ProceduralNodeType? typeOverride;

		public ProceduralNodeType type => typeOverride ?? node.data.type;

		public Selection(DataRef<ProceduralNodeData> node, double weight, ProceduralNodeType? typeOverride)
		{
			this.node = node;
			this.weight = weight;
			this.typeOverride = typeOverride;
		}

		public override string ToString()
		{
			return node.GetFriendlyName();
		}
	}

	[ProtoMember(1)]
	[UIField(tooltip = "Use to override node type of contained nodes and packs.")]
	private ProceduralNodeType? _typeOverride;

	[ProtoMember(2, OverwriteList = true)]
	[UIField(maxCount = 0)]
	[UIFieldCollectionItem]
	[UIFieldValue(defaultValue = 1, min = 0.1f, max = 100)]
	[UIDeepValueChange]
	private Dictionary<DataRef<ProceduralNodeData>, float> _nodes;

	[ProtoMember(3, OverwriteList = true)]
	[UIField(maxCount = 0)]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private Dictionary<DataRef<ProceduralNodePackData>, ProceduralWeight> _packs;

	public ProceduralNodeType? type => _typeOverride;

	public ProceduralNodePack()
	{
	}

	public ProceduralNodePack(ProceduralNodeType? typeOverride = null, Dictionary<DataRef<ProceduralNodeData>, float> nodes = null, Dictionary<DataRef<ProceduralNodePackData>, ProceduralWeight> packs = null)
	{
		_typeOverride = typeOverride;
		_nodes = nodes;
		_packs = packs;
	}

	private int _GetCount(PoolKeepItemHashSetHandle<DataRef<ProceduralNodePackData>> traversedPacks)
	{
		int num = _nodes?.Count ?? 0;
		if (!_packs.IsNullOrEmpty())
		{
			foreach (DataRef<ProceduralNodePackData> key in _packs.Keys)
			{
				if (traversedPacks.Add(key))
				{
					num += key.data.pack._GetCount(traversedPacks);
				}
			}
			return num;
		}
		return num;
	}

	private void _GetWeightedSelections(PoolWRandomDHandle<Selection> selections, PoolKeepItemHashSetHandle<DataRef<ProceduralNodePackData>> traversedPacks, double weightMultiplier = 1.0, ProceduralNodeType? typeOverride = null, ProceduralGraph graph = null)
	{
		ProceduralNodeType? proceduralNodeType = typeOverride;
		if (!proceduralNodeType.HasValue)
		{
			typeOverride = _typeOverride;
		}
		if (!_nodes.IsNullOrEmpty())
		{
			foreach (KeyValuePair<DataRef<ProceduralNodeData>, float> node in _nodes)
			{
				double weight = (graph == null || !graph.PreventRepeatedNodeDataContains(node.Key)).ToDouble((double)node.Value * weightMultiplier);
				selections.value.Add(weight, new Selection(node.Key, weight, typeOverride));
			}
		}
		if (_packs.IsNullOrEmpty())
		{
			return;
		}
		foreach (KeyValuePair<DataRef<ProceduralNodePackData>, ProceduralWeight> pack in _packs)
		{
			if (traversedPacks.Add(pack.Key))
			{
				pack.Key.data.pack._GetWeightedSelections(selections, traversedPacks, weightMultiplier * (double)((pack.Value.type == ProceduralWeightType.PerItem) ? pack.Value.weight : (pack.Value.weight / (float)pack.Key.data.pack.GetCount().InsureNonZero())), typeOverride, graph);
			}
		}
	}

	private IEnumerable<ProceduralNodeData> _GetNodeData(PoolKeepItemHashSetHandle<DataRef<ProceduralNodePackData>> traversedPacks)
	{
		if (!_nodes.IsNullOrEmpty())
		{
			foreach (DataRef<ProceduralNodeData> key in _nodes.Keys)
			{
				yield return key.data;
			}
		}
		if (_packs.IsNullOrEmpty())
		{
			yield break;
		}
		foreach (DataRef<ProceduralNodePackData> key2 in _packs.Keys)
		{
			if (!traversedPacks.Add(key2))
			{
				continue;
			}
			foreach (ProceduralNodeData item in key2.data.pack._GetNodeData(traversedPacks))
			{
				yield return item;
			}
		}
	}

	private async Task _ProcessNodeMappings(HashSet<DataRef<ProceduralNodePackData>> traversedPacks, Func<DataRef<ProceduralNodeData>, Task<DataRef<ProceduralNodeData>>> processNodeKey, Func<DataRef<ProceduralNodePackData>, Task<DataRef<ProceduralNodePackData>>> processPackKey)
	{
		if (!_nodes.IsNullOrEmpty())
		{
			using PoolKeepItemDictionaryHandle<DataRef<ProceduralNodeData>, float> nodes = Pools.UseKeepItemDictionary(_nodes);
			_nodes.Clear();
			foreach (KeyValuePair<DataRef<ProceduralNodeData>, float> node in nodes.value)
			{
				Dictionary<DataRef<ProceduralNodeData>, float> nodes2 = _nodes;
				nodes2[await processNodeKey(node.Key)] = node.Value;
			}
		}
		if (_packs.IsNullOrEmpty())
		{
			return;
		}
		using PoolKeepItemDictionaryHandle<DataRef<ProceduralNodePackData>, ProceduralWeight> packs = Pools.UseKeepItemDictionary(_packs);
		_packs.Clear();
		foreach (KeyValuePair<DataRef<ProceduralNodePackData>, ProceduralWeight> pack in packs.value)
		{
			if (traversedPacks.Add(pack.Key))
			{
				Dictionary<DataRef<ProceduralNodePackData>, ProceduralWeight> packs2 = _packs;
				packs2[await processPackKey(pack.Key)] = ProtoUtil.Clone(pack.Value);
			}
			else
			{
				_packs[pack.Key] = ProtoUtil.Clone(pack.Value);
			}
		}
	}

	public void PrepareDataForSave()
	{
		_nodes?.RemoveKeys((DataRef<ProceduralNodeData> d) => !d);
		_packs?.RemoveKeys((DataRef<ProceduralNodePackData> d) => !d);
	}

	public int GetCount()
	{
		using PoolKeepItemHashSetHandle<DataRef<ProceduralNodePackData>> traversedPacks = Pools.UseKeepItemHashSet<DataRef<ProceduralNodePackData>>();
		return _GetCount(traversedPacks);
	}

	public Selection GetSelection(Random random, ProceduralGraph graph = null)
	{
		using PoolWRandomDHandle<Selection> poolWRandomDHandle = GetWeightedSelections(graph);
		return poolWRandomDHandle.value.Random(random.NextDouble());
	}

	public PoolWRandomDHandle<Selection> GetWeightedSelections(ProceduralGraph graph = null)
	{
		PoolWRandomDHandle<Selection> poolWRandomDHandle = Pools.UseWRandomD<Selection>();
		using PoolKeepItemHashSetHandle<DataRef<ProceduralNodePackData>> traversedPacks = Pools.UseKeepItemHashSet<DataRef<ProceduralNodePackData>>();
		_GetWeightedSelections(poolWRandomDHandle, traversedPacks, 1.0, null, graph);
		return poolWRandomDHandle;
	}

	public IEnumerable<ProceduralNodeData> GetNodeData()
	{
		using PoolKeepItemHashSetHandle<DataRef<ProceduralNodePackData>> traversedPacks = Pools.UseKeepItemHashSet<DataRef<ProceduralNodePackData>>();
		foreach (ProceduralNodeData item in _GetNodeData(traversedPacks))
		{
			yield return item;
		}
	}

	public async Task ProcessKeyMappings(Func<DataRef<ProceduralNodeData>, Task<DataRef<ProceduralNodeData>>> processNodeKey, Func<DataRef<ProceduralNodePackData>, Task<DataRef<ProceduralNodePackData>>> processPackKey, HashSet<DataRef<ProceduralNodePackData>> traversedPacks = null)
	{
		if (traversedPacks == null)
		{
			traversedPacks = new HashSet<DataRef<ProceduralNodePackData>>();
		}
		await _ProcessNodeMappings(traversedPacks, processNodeKey, processPackKey);
	}

	public override string ToString()
	{
		if (!_nodes.IsNullOrEmpty() || !_packs.IsNullOrEmpty())
		{
			return ((!_nodes.IsNullOrEmpty()) ? ("<b>Nodes:</b> " + _nodes.Keys.ToStringSmart() + " ") : "") + ((!_packs.IsNullOrEmpty()) ? ("<b>Packs:</b> " + _packs.Keys.ToStringSmart()) : "");
		}
		return "EMPTY";
	}
}
