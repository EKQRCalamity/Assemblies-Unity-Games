using ProtoBuf;

[ProtoContract]
public class ExperienceVial : ATarget
{
	[ProtoMember(1)]
	private BInt _experience;

	[ProtoMember(2)]
	private BBool _shouldSimulateLiquid;

	public BInt experience => _experience ?? (_experience = new BInt());

	public BBool shouldSimulateLiquid => _shouldSimulateLiquid ?? (_shouldSimulateLiquid = new BBool(value: true));

	public ExperienceVialView vialview => base.view as ExperienceVialView;

	public ExperienceVial()
	{
	}

	public ExperienceVial(int experience)
	{
		_experience = new BInt(experience);
	}

	public ExperienceVial(int experience, bool shouldSimulateLiquid)
		: this(experience)
	{
		_shouldSimulateLiquid = new BBool(shouldSimulateLiquid);
	}

	public void SetExperience(int experienceToSet)
	{
		experience.value = experienceToSet;
	}
}
