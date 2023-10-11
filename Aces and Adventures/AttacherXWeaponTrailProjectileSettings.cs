using System;
using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class AttacherXWeaponTrailProjectileSettings : IAttacherXWeaponTrailSettings
{
	private static readonly RangeF RANGE = new RangeF(0f, 1f, -1f, 2f);

	[ProtoMember(1)]
	[UIField]
	private OptionalTints _tint;

	[ProtoMember(2)]
	[UIField(min = 0.1f, max = 1)]
	[DefaultValue(0.5f)]
	private float _lifetime = 0.5f;

	[ProtoMember(3)]
	[UIField]
	private RangeF _rangeAlongObject = RANGE;

	[ProtoMember(4)]
	[UIField(min = 0, max = 1)]
	[DefaultValue(0.5f)]
	private float _meshComplexity = 0.5f;

	[ProtoMember(5)]
	[UIField]
	private bool _flip;

	private bool _rangeAlongObjectSpecified => _rangeAlongObject != RANGE;

	public void Apply(System.Random random, AttacherXWeaponTrail attacherTrail)
	{
		attacherTrail.color.tintColor = _tint.GetTint(random);
		attacherTrail.trail.SetMaxFrame(Mathf.RoundToInt(_lifetime * 60f));
		attacherTrail.trail.Range = (_flip ? new Vector2(1f - _rangeAlongObject.max, 1f - _rangeAlongObject.min) : ((Vector2)_rangeAlongObject));
		attacherTrail.trail.SetGranularity(Mathf.RoundToInt(Mathf.Lerp(10f, 50f, _meshComplexity)));
		attacherTrail.flipStartAndEnd = _flip;
	}
}
