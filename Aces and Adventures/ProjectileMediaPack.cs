using ProtoBuf;

[ProtoContract]
[UIField]
public class ProjectileMediaPack
{
	[ProtoMember(1)]
	[UIField("Projectile Media", 0u, null, null, null, null, null, null, false, null, 5, false, null, filter = DataRefControlFilter.Simple, collapse = UICollapseType.Open, validateOnChange = true)]
	private DataRef<ProjectileMediaData> _projectile = new DataRef<ProjectileMediaData>();

	[ProtoMember(2)]
	[UIField(validateOnChange = true)]
	[UIHorizontalLayout("Start", flexibleWidth = 0f)]
	private bool _overrideStartLocation;

	[ProtoMember(3)]
	[UIField]
	[UIHideIf("_hideStartLocation")]
	[UIHorizontalLayout("Start", flexibleWidth = 999f)]
	[UIDeepValueChange]
	private ProjectileExtremaData _startLocation = new ProjectileExtremaData(ProjectileExtremaData.Subject.Activator, CardTargets.Center);

	[ProtoMember(4)]
	[UIField(validateOnChange = true)]
	[UIHorizontalLayout("End", flexibleWidth = 0f)]
	private bool _overrideEndLocation;

	[ProtoMember(5)]
	[UIField]
	[UIHideIf("_hideEndLocation")]
	[UIHorizontalLayout("End", flexibleWidth = 999f)]
	[UIDeepValueChange]
	private ProjectileExtremaData _endLocation;

	[ProtoMember(6)]
	[UIField]
	private ProjectilesFinishedAt? _finishedAtOverride;

	public ProjectileMediaData data => _projectile.DataOrDefault();

	protected ProjectileExtremaData startLocation => _startLocation ?? (_startLocation = new ProjectileExtremaData(ProjectileExtremaData.Subject.Activator, CardTargets.Center));

	public ProjectileExtremaData startDataOverride
	{
		get
		{
			if (!_overrideStartLocation)
			{
				return null;
			}
			return startLocation;
		}
	}

	protected ProjectileExtremaData endLocation => _endLocation ?? (_endLocation = new ProjectileExtremaData());

	public ProjectileExtremaData endDataOverride
	{
		get
		{
			if (!_overrideEndLocation)
			{
				return null;
			}
			return endLocation;
		}
	}

	public ProjectilesFinishedAt? finishedAtOverride => _finishedAtOverride;

	public bool shouldRotate => (startDataOverride ?? data.main.startLocation).subject != (endDataOverride ?? data.main.endLocation).subject;

	private bool _projectileSpecified => _projectile.ShouldSerialize();

	private bool _hideStartLocation => !_overrideStartLocation;

	private bool _hideEndLocation => !_overrideEndLocation;

	public static implicit operator bool(ProjectileMediaPack pack)
	{
		if (pack != null)
		{
			return pack._projectile;
		}
		return false;
	}

	public static implicit operator string(ProjectileMediaPack pack)
	{
		if (!pack)
		{
			return "<i>Invalid Projectile Effect</i>";
		}
		return pack.ToString();
	}

	public override string ToString()
	{
		return _projectile.GetFriendlyName();
	}
}
