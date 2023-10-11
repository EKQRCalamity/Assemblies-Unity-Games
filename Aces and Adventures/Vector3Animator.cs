using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
[ProtoInclude(10, typeof(UniformAnimator))]
[ProtoInclude(11, typeof(PerAxisAnimator))]
public abstract class Vector3Animator
{
	protected static readonly RangeF RANGE = new RangeF(0.5f, 1.5f, 0.25f, 2f);

	protected const float FREQUENCY = 1f;

	protected const float MIN_FREQ = 0.1f;

	protected const float MAX_FREQ = 10f;

	[ProtoMember(1)]
	[UIField(order = 1u)]
	protected Vector3AnimatorType _type;

	public abstract Vector3 GetValue(float elapsedTime);
}
