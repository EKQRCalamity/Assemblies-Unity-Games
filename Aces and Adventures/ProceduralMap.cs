using ProtoBuf;

[ProtoContract]
public class ProceduralMap : ATarget
{
	[ProtoContract(EnumPassthru = true)]
	public enum Pile
	{
		Closed,
		Active,
		Hidden
	}

	[ProtoMember(1)]
	private ProceduralGraph _graph;

	[ProtoMember(2)]
	private DataRef<ProceduralGraphData> _data;

	public ProceduralGraph graph => _graph;

	public ProceduralGraphData data => _data.data;

	public bool generateOverTime { get; set; }

	private bool _dataSpecified => _data.ShouldSerialize();

	private ProceduralMap()
	{
	}

	public ProceduralMap(ProceduralGraph graph, DataRef<ProceduralGraphData> data)
	{
		_graph = graph;
		_data = data;
	}
}
