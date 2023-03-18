namespace Framework.Achievements;

public class GogAchievementsHelper : IAchievementsHelper
{
	private bool isOnline;

	private bool statsValid;

	public GogAchievementsHelper()
	{
		gogInit();
	}

	private void gogInit()
	{
	}

	public void SetAchievementProgress(string Id, float value)
	{
	}

	public void GetAchievementProgress(string Id, GetAchievementOperationEvent evt)
	{
	}
}
