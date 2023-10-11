using System;
using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
[ProtoInclude(5, typeof(RemoveNode))]
public abstract class PostProcessGraphInstruction
{
	[ProtoContract]
	[UIField]
	public class RemoveNode : PostProcessGraphInstruction
	{
		[ProtoMember(1)]
		[UIField(min = 0, max = 100)]
		[DefaultValue(100)]
		private byte _removeChance = 100;

		[ProtoMember(2)]
		[UIField(min = 0, max = 100)]
		[DefaultValue(50)]
		private byte _patchConnectionsChance = 50;

		public override void PostProcess(Random random, ProceduralGraph graph, ProceduralNode node)
		{
			if (random.Chance((float)(int)_removeChance * 0.01f))
			{
				graph.DeleteNode(node.id, random, (float)(int)_patchConnectionsChance * 0.01f);
			}
		}

		public override string ToString()
		{
			return $"{_removeChance}% to Remove Node" + (_patchConnectionsChance > 0).ToText($" and {_patchConnectionsChance}% Patch Connections");
		}
	}

	public abstract void PostProcess(Random random, ProceduralGraph graph, ProceduralNode node);
}
