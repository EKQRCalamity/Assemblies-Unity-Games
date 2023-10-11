using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
public class ButtonSensitivity
{
	[ProtoMember(1)]
	[UIField(min = 0.01f, max = 4f, view = "UI/Slider Advanced", defaultValue = 1)]
	[DefaultValue(1f)]
	private float _speed = 1f;

	[ProtoMember(2)]
	[UIField]
	[UIHorizontalLayout("Acceleration", flexibleWidth = 0f)]
	private bool _invert;

	[ProtoMember(3)]
	[UIField(min = 0.1f, max = 2f)]
	[UIHorizontalLayout("Acceleration", flexibleWidth = 999f)]
	private float? _acceleration;

	public float speed => _speed * (float)_invert.ToInt(-1, 1);

	public float? acceleration => _acceleration * 10f;

	public bool invert => _invert;
}
