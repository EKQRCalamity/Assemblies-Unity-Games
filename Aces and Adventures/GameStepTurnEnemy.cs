using System.Collections;

public class GameStepTurnEnemy : AGameStepTurn
{
	protected override Stone.Pile? _enabledTurnStonePile
	{
		get
		{
			if (!_entity.canAttack)
			{
				return null;
			}
			return Stone.Pile.EnemyTurn;
		}
	}

	public GameStepTurnEnemy(AEntity entity)
		: base(entity)
	{
	}

	public override void Start()
	{
		base.state.SignalTurnStartLate(_entity);
	}

	protected override void OnEnable()
	{
		base.state.SignalControlGained(_entity, ControlGainType.Enemy);
		base.OnEnable();
	}

	protected override IEnumerator Update()
	{
		while (_entity.canAttack)
		{
			yield return AppendStep(new GameStepTurnEnemyPrepareAttack((Enemy)_entity));
		}
	}

	protected override void OnDestroy()
	{
		_entity.hasTakenTurn.value = true;
	}
}
