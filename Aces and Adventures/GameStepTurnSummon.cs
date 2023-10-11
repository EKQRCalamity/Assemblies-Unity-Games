public class GameStepTurnSummon : AGameStepTurn
{
	public GameStepTurnSummon(AEntity entity)
		: base(entity)
	{
	}

	protected override void OnDestroy()
	{
		_entity.hasTakenTurn.value = true;
	}
}
