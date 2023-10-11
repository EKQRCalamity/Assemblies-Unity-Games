using System;
using System.Collections;

public class GameStepDealDamage : GameStep
{
	private ActionContext _context;

	private int _damage;

	private DamageSource _source;

	private AAction _action;

	private bool? _ignoreShields;

	public GameStepDealDamage(ActionContext context, int damage, DamageSource source, AAction action = null, bool? ignoreShields = null)
	{
		_context = context;
		_damage = damage;
		_source = source;
		_action = action;
		_ignoreShields = ignoreShields;
	}

	protected override IEnumerator Update()
	{
		if (_damage <= 0)
		{
			yield break;
		}
		int totalDamage = _damage;
		ACombatant damaged = _context.GetTarget<ACombatant>();
		if ((int)damaged.shield > 0)
		{
			base.state.SignalShouldIgnoreShields(_context, _action, _damage, _source, ref _ignoreShields);
			if (!base.isActiveStep)
			{
				yield return null;
			}
			if (_ignoreShields != true)
			{
				int damagedShield = Math.Min(_damage, damaged.shield);
				damaged.shield.value -= damagedShield;
				base.state.SignalShieldDamageDealt(_context, _action, damagedShield, _source);
				if (!base.isActiveStep)
				{
					yield return null;
				}
				_damage -= damagedShield;
			}
		}
		int value = damaged.HP.value;
		int overkill = _damage - (int)damaged.HP;
		bool causedLethal = value > 0 && _damage >= value;
		if (_damage > 0)
		{
			damaged.HP.value -= _damage;
			base.state.SignalDamageDealt(_context, _action, _damage, _source);
			if (!base.isActiveStep)
			{
				yield return null;
			}
		}
		base.state.SignalTotalDamageDealt(_context, _action, totalDamage, _source);
		if (!base.isActiveStep)
		{
			yield return null;
		}
		if (overkill > 0)
		{
			base.state.SignalOverkill(_context, _action, overkill, _source);
			if (!base.isActiveStep)
			{
				yield return null;
			}
		}
		if (causedLethal)
		{
			base.state.stack.Queue(new GameStepDeath(_context, _action));
		}
	}
}
