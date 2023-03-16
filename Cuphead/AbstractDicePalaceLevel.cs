public abstract class AbstractDicePalaceLevel : Level
{
	public abstract DicePalaceLevels CurrentDicePalaceLevel { get; }

	protected override void Awake()
	{
		base.Awake();
		if (DicePalaceMainLevelGameInfo.GameInfo != null)
		{
			Level.Current.OnLoseEvent += DicePalaceMainLevelGameInfo.GameInfo.CleanUp;
		}
		base.OnLoseEvent += ResetScore;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.OnLoseEvent -= ResetScore;
	}

	private void ResetScore()
	{
		base.OnLoseEvent -= ResetScore;
		CleanUpScore();
	}

	protected override void CheckIfInABossesHub()
	{
		base.CheckIfInABossesHub();
		if (!Level.IsTowerOfPower)
		{
			Level.IsDicePalace = true;
		}
	}
}
