using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class ProceduralCard : AdventureTarget
{
	[ProtoMember(1)]
	private DataRef<ProceduralGraphData> _graph;

	public override IEnumerable<GameStep> selectedGameSteps
	{
		get
		{
			yield return new GameStepProceduralMap(_graph);
		}
	}

	private bool _graphSpecified => _graph.ShouldSerialize();

	private ProceduralCard()
	{
	}

	public ProceduralCard(DataRef<ProceduralGraphData> graph)
	{
		_graph = graph;
	}
}
