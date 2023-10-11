using ProtoBuf;

[ProtoContract]
[UIField]
[ProtoInclude(1000, typeof(NodeDataWarpEnter))]
[ProtoInclude(1001, typeof(NodeDataWarpExit))]
[ProtoInclude(1002, typeof(NodeDataEventWarpExit))]
public abstract class ANodeDataWarp : NodeData
{
	public virtual NodeDataTags identifiers => null;

	public override string name
	{
		get
		{
			return identifiers.name;
		}
		set
		{
		}
	}

	protected override bool _hideNameInInspector => true;

	public override string contextPath => "Advanced/Warp/";

	public override NodeGraph graph
	{
		get
		{
			return base.graph;
		}
		set
		{
			base.graph = value;
			if (identifiers != null)
			{
				value.SetGraph(identifiers);
			}
		}
	}

	public override Type activeOutputType => Type.False;
}
