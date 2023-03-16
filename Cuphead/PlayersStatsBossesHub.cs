public class PlayersStatsBossesHub
{
	public int HP;

	public int BonusHP;

	public float SuperCharge;

	public Weapon basePrimaryWeapon;

	public Weapon baseSecondaryWeapon;

	public Super BaseSuper;

	public Charm BaseCharm;

	public int tokenCount;

	public int healerHP;

	public int healerHPReceived;

	public int healerHPCounter;

	public void LoseBonusHP()
	{
		if (BonusHP > 0)
		{
			BonusHP--;
		}
	}

	public void LoseHealerHP()
	{
		if (healerHP > 0)
		{
			healerHP--;
		}
	}
}
