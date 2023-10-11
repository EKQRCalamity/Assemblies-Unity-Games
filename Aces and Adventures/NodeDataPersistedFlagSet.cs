using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Persisted Flag Set", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1010u)]
public class NodeDataPersistedFlagSet : ANodeDataFlagSet
{
	public override HotKey? contextHotKey => new HotKey(KeyModifiers.Shift, KeyCode.W);

	public override NodeDataIconType iconType => NodeDataIconType.PersistedFlagSet;

	public override NodeDataFlagType flagType => NodeDataFlagType.Persisted;
}
