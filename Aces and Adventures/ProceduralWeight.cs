using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
public class ProceduralWeight
{
	[ProtoMember(1)]
	[UIField(min = 0.1f, max = 100)]
	[DefaultValue(1)]
	private float _weight = 1f;

	[ProtoMember(2)]
	[UIField]
	private ProceduralWeightType _type;

	public float weight => _weight;

	public ProceduralWeightType type => _type;

	public ProceduralWeight()
	{
	}

	public ProceduralWeight(float weight, ProceduralWeightType type = ProceduralWeightType.PerItem)
	{
		_weight = weight;
		_type = type;
	}

	public override string ToString()
	{
		return $"{EnumUtil.FriendlyName(_type)}: {_weight}";
	}
}
