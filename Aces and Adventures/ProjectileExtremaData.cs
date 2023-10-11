using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class ProjectileExtremaData
{
	[ProtoContract(EnumPassthru = true)]
	public enum Subject : byte
	{
		Target,
		Activator
	}

	[ProtoContract(EnumPassthru = true)]
	public enum Location : byte
	{
		EntityBodyParts,
		Tile
	}

	private static readonly RangeF RANGE = new RangeF(0f, 0f, -10f, 10f).Scale(0.1f);

	[ProtoMember(1, IsRequired = true)]
	[UIField]
	private Subject _subject;

	[ProtoMember(2, IsRequired = true)]
	[UIField(min = CardTargets.Cost, maxCount = 0)]
	[DefaultValue(CardTargets.Center)]
	private CardTargets _cardTargets = CardTargets.Center;

	[ProtoMember(3)]
	[UIField]
	[UIDeepValueChange]
	private OffsetRanges _positionOffset = new OffsetRanges(RANGE);

	public Subject subject => _subject;

	public bool subjectIsActivator => _subject == Subject.Activator;

	public OffsetRanges positionOffset => _positionOffset ?? (_positionOffset = new OffsetRanges(RANGE));

	public CardTargets cardTargets => _cardTargets;

	private bool _positionOffsetSpecified => _positionOffset;

	public ProjectileExtremaData()
	{
	}

	public ProjectileExtremaData(Subject subject, CardTargets cardTargets)
	{
		_subject = subject;
		_cardTargets = cardTargets;
	}

	public PoolKeepItemListHandle<Transform> GetTransforms(IProjectileExtrema activator, IProjectileExtrema target)
	{
		PoolKeepItemListHandle<Transform> poolKeepItemListHandle = Pools.UseKeepItemList<Transform>();
		foreach (CardTarget item in EnumUtil.FlagsConverted<CardTargets, CardTarget>(_cardTargets))
		{
			poolKeepItemListHandle.Add(((_subject == Subject.Target) ? target : activator).GetTargetForProjectile(item));
		}
		return poolKeepItemListHandle;
	}

	public override string ToString()
	{
		return $"{_subject}'s {EnumUtil.FriendlyName(_cardTargets)}" + (_positionOffset?.hasOffset ?? false).ToText(_positionOffset.ToString());
	}
}
