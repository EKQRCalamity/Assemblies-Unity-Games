using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
public class ActionMedia
{
	[ProtoContract(EnumPassthru = true)]
	public enum Sequencing
	{
		[UITooltip("All Projectile Media will be created and launched simultaneously per target.")]
		AllAtOnce,
		[UITooltip("Projectile Media will be created one after another per target.")]
		OneAtATime,
		[UITooltip("Projectile Media will only be created for first target.")]
		PlayOnce,
		[UITooltip("All Projectile Media will be created and launched simultaneously per target.\n<i>Splitting total emission multiplier amongst all separate targets.</i>")]
		AllAtOnceNormalizeEmission
	}

	[ProtoMember(1)]
	[UIField(tooltip = "Determines how projectile media will be sequenced per target.")]
	private Sequencing _sequencing;

	[ProtoMember(2)]
	[UIField(tooltip = "Defines what will be considered the \"Activator\" location of the projectile media.")]
	[UIHorizontalLayout("T")]
	[DefaultValue(CardTargetType.Ability)]
	private CardTargetType _activateFrom = CardTargetType.Ability;

	[ProtoMember(3)]
	[UIField(tooltip = "Defines what will be considered the \"Target\" for projectile media to be launched towards.")]
	[UIHorizontalLayout("T")]
	private CardTargetType _launchAt;

	[ProtoMember(4)]
	[UIField(collapse = UICollapseType.Hide)]
	private ProjectileMediaPack _projectileMedia;

	public Sequencing sequencing => _sequencing;

	public CardTargetType activator => _activateFrom;

	public CardTargetType target => _launchAt;

	public ProjectileMediaPack projectileMedia => _projectileMedia;

	private bool _projectileMediaSpecified => _projectileMedia;

	public static implicit operator bool(ActionMedia media)
	{
		return media?._projectileMedia;
	}

	public static implicit operator ProjectileMediaData(ActionMedia media)
	{
		if (!media)
		{
			return null;
		}
		return media.projectileMedia.data;
	}

	public override string ToString()
	{
		if (!_projectileMedia)
		{
			return "<i>Null</i>";
		}
		return _projectileMedia;
	}
}
