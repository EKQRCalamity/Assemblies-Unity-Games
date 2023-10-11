using System;
using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class AttacherTrailProjectileSettings : IAttacherTrailSettings
{
	private static readonly RangeF WIDTH = ProjectileMediaData.MULTIPLIER;

	private static readonly RangeF END_WIDTH = new RangeF(1f, 1f, 0f, 3f);

	private static readonly RangeF LIFETIME = ProjectileMediaData.MULTIPLIER;

	[ProtoMember(1)]
	[UIField(min = 0, max = 3f)]
	[DefaultValue(1f)]
	private float _referenceSpeed = 1f;

	[ProtoMember(2)]
	[UIField]
	private RangeF _widthMultiplier = WIDTH;

	[ProtoMember(3)]
	[UIField]
	private RangeF _endWidthMultiplier = END_WIDTH;

	[ProtoMember(4)]
	[UIField]
	private RangeF _lifetimeMultiplier = LIFETIME;

	[ProtoMember(5)]
	[UIField]
	private OptionalTints _startTint;

	[ProtoMember(6)]
	[UIField]
	private OptionalTints _endTint;

	[ProtoMember(7)]
	[UIField(category = "Advanced")]
	private TrailAlignment _trailAlignment;

	[ProtoMember(8)]
	[UIField(min = 0.001f, max = 0.010000001f, stepSize = 0.0005f, category = "Advanced")]
	[DefaultValue(0.0050000004f)]
	private float _minVertexDistance = 0.0050000004f;

	[ProtoMember(9)]
	[UIField(min = 0.05f, max = 1, category = "Advanced")]
	[DefaultValue(0.5f)]
	private float _fadeTime = 0.5f;

	[ProtoMember(10)]
	[UIField(category = "Advanced")]
	private OffsetRanges _positionOffset;

	private OffsetRanges positionOffset => _positionOffset ?? (_positionOffset = new OffsetRanges());

	private bool _widthMultiplierSpecified => _widthMultiplier != WIDTH;

	private bool _endWidthMultiplierSpecified => _endWidthMultiplier != END_WIDTH;

	private bool _lifetimeMultiplierSpecified => _lifetimeMultiplier != LIFETIME;

	private bool _positionOffsetSpecified => _positionOffset;

	public void Apply(System.Random random, AttacherTrail attacherTrail)
	{
		attacherTrail.speedRange.max = _referenceSpeed;
		attacherTrail.width.multiplier = random.Range(_widthMultiplier);
		attacherTrail.trail.endWidth = random.Range(_endWidthMultiplier) * attacherTrail.initialEndWidth;
		attacherTrail.lifetime.multiplier = random.Range(_lifetimeMultiplier);
		attacherTrail.startColor.tintColor = _startTint.GetTint(random);
		attacherTrail.endColor.tintColor = _endTint.GetTint(random) ?? attacherTrail.startColor.tintColor;
		attacherTrail.trail.alignment = ((_trailAlignment != 0) ? LineAlignment.Local : LineAlignment.View);
		attacherTrail.trail.minVertexDistance = _minVertexDistance;
		attacherTrail.life.detachTime = _fadeTime;
		attacherTrail.attach.axisOffsetDistances = positionOffset.GetOffset(random);
	}
}
