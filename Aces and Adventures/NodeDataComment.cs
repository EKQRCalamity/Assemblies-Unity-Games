using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Comment", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1102u)]
public class NodeDataComment : NodeData
{
	[ProtoMember(1)]
	[UIField(max = 512, view = "UI/Input Field Multiline", collapse = UICollapseType.Open)]
	private string _comment;

	public string comment => _comment ?? "";

	public override string name
	{
		get
		{
			return comment.MaxLengthOf(50);
		}
		set
		{
		}
	}

	public override string contextPath => "Advanced/";

	public override HotKey? contextHotKey => new HotKey((KeyModifiers)0, KeyCode.Slash);

	public override NodeDataIconType iconType => NodeDataIconType.Comment;

	protected override bool _hideNameInInspector => true;

	protected override void _GetSearchText(TextBuilder builder)
	{
		builder.Append(comment).Space();
	}

	public override void OnPreFlatten()
	{
		RemoveNodeAndPatchConnections();
	}
}
