using System.Collections;

public sealed class GameStepDeath : GameStep
{
	private ActionContext _context;

	private AAction _action;

	protected override bool shouldBeCanceled
	{
		get
		{
			if (!base.hasStarted)
			{
				if (_context.target is ACombatant aCombatant && (int)aCombatant.HP <= 0 && !aCombatant.dieing)
				{
					return aCombatant.dead;
				}
				return true;
			}
			return false;
		}
	}

	public GameStepDeath(ActionContext context, AAction action)
	{
		_context = context;
		_action = action;
		if (!shouldBeCanceled)
		{
			base.state.SignalEntityOnDeathsDoor(_context, _action);
		}
	}

	public override void Start()
	{
		((ACombatant)_context.target).dieing = true;
		base.state.SignalEntityBeginDeath(_context, _action);
	}

	protected override IEnumerator Update()
	{
		if (_context.target is ACombatant aCombatant)
		{
			VoiceManager.Instance.Play(aCombatant.view.transform, aCombatant.audio.death, interrupt: true);
		}
		_context.target.view.offsets.Clear();
		_context.target.view.offsets.Add(AGameStepTurn.OFFSET);
		foreach (float item in Wait(MathUtil.TwoThirds))
		{
			_ = item;
			yield return null;
		}
		base.state.SignalEntityDeath(_context, _action);
		_context.GetTarget<AEntity>().SetLeftOfEntitiesHasTakenTurn();
		yield return AppendStep(base.state.adventureDeck.DiscardStep(_context.target));
		_context.target.view.offsets.Clear();
		_context.GetTarget<ACombatant>().OnDeath();
	}

	protected override void OnDestroy()
	{
		if (base.hasStarted && _context.target is ACombatant aCombatant)
		{
			aCombatant.dieing = false;
		}
	}
}
