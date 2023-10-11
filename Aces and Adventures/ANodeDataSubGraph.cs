using ProtoBuf;

[ProtoContract]
[UIField("Abstract Sub Graph", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
[ProtoInclude(1000, typeof(NodeDataSubGraph))]
[ProtoInclude(1001, typeof(NodeDataRef))]
public abstract class ANodeDataSubGraph : NodeData
{
	public override string contextPath => "Advanced/";

	public override string contextCategory => "Sub Graph";

	public override NodeDataSpriteType spriteType => NodeDataSpriteType.SubGraph;
}
