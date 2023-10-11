public static class SoundPackExtensions
{
	public static bool ShouldSerialize(this SoundPack soundPack)
	{
		if (soundPack != null)
		{
			return soundPack.count > 0;
		}
		return false;
	}
}
