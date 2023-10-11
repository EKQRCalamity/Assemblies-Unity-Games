using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Persisted Flag Check", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1011u)]
public class NodeDataPersistedFlagCheck : ANodeDataFlagCheck
{
	public override HotKey? contextHotKey => new HotKey(KeyModifiers.Shift, KeyCode.D);

	public override NodeDataIconType iconType => NodeDataIconType.PersistedFlagCheck;

	public override NodeDataFlagType flagType => NodeDataFlagType.Persisted;
}
