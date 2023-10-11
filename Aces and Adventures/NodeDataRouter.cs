using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Router", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1101u)]
[YouTubeVideo("Router Node Tutorial", "WPKGTYyAtOM", -1, -1)]
public class NodeDataRouter : NodeData
{
	public override string name
	{
		get
		{
			return "";
		}
		set
		{
		}
	}

	public override string contextPath => "Advanced/";

	public override HotKey? contextHotKey => new HotKey((KeyModifiers)0, KeyCode.F);

	public override NodeDataIconType iconType => NodeDataIconType.Router;

	protected override bool _hideNameInInspector => true;
}
