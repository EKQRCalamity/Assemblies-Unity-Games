public struct ProbabilityRanges
{
	public ProbabilityRange reliable;

	public ProbabilityRange critical;

	public ProbabilityRange miss;

	public ProbabilityRanges(ProbabilityRange reliable, ProbabilityRange critical, float missChance)
	{
		this.reliable = reliable;
		this.critical = critical;
		miss = new ProbabilityRange(missChance, Int2.zero);
	}
}
