using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

[ProtoContract]
public struct ActionContext
{
	[ProtoContract(EnumPassthru = true)]
	public enum State
	{
		Act,
		Tick
	}

	[Flags]
	[ProtoContract(EnumPassthru = true)]
	public enum States
	{
		Act = 1,
		Tick = 2
	}

	[ProtoMember(1)]
	private Id<AEntity> _actor;

	[ProtoMember(2)]
	private Id<Ability> _ability;

	[ProtoMember(3)]
	private Id<ATarget> _target;

	[ProtoMember(15)]
	private int _capturedValue;

	private State _state;

	public AEntity actor => _actor;

	public bool hasAbility => _ability;

	public Ability ability => _ability;

	public ATarget target => _target;

	private IEnumerable<ATarget> _targets
	{
		get
		{
			yield return target;
		}
	}

	public IEnumerable<ATarget> targets => gameState.stack.activeStep.GetPreviousSteps().OfType<GameStepActionTarget>().FirstOrDefault()?.targets ?? _targets;

	public GameState gameState => GameState.Instance;

	public int capturedValue => _capturedValue;

	public State state => _state;

	private bool _actorSpecified => _actor.shouldSerialize;

	private bool _abilitySpecified => _ability.shouldSerialize;

	private bool _targetSpecified => _target.shouldSerialize;

	public ActionContext(AEntity actor, Ability ability, ATarget target = null)
	{
		_actor = actor;
		_ability = ability;
		_target = target;
		_capturedValue = 0;
		_state = State.Act;
	}

	private T _FindTarget<T>() where T : ATarget
	{
		foreach (GameStep previousStep in gameState.stack.activeStep.GetPreviousSteps(GameStep.GroupType.Context))
		{
			if (!(previousStep is GameStepActionTarget gameStepActionTarget) || !gameStepActionTarget.hasTargets)
			{
				continue;
			}
			foreach (ATarget target in gameStepActionTarget.targets)
			{
				if (target is T result)
				{
					return result;
				}
			}
		}
		return null;
	}

	private T _FindTickTarget<T>() where T : ATarget
	{
		foreach (GameStep previousStep in gameState.stack.activeStep.GetPreviousSteps(GameStep.GroupType.Context))
		{
			if (!(previousStep is GameStepActionTarget.Tick tick) || !tick.hasTargets)
			{
				continue;
			}
			foreach (ATarget target in tick.targets)
			{
				if (target is T result)
				{
					return result;
				}
			}
		}
		return null;
	}

	public ActionContext SetActor(AEntity newActor)
	{
		ActionContext result = this;
		result._actor = newActor ?? ((AEntity)result._actor);
		return result;
	}

	public ActionContext SetTarget(ATarget newTarget)
	{
		ActionContext result = this;
		result._target = newTarget ?? ((ATarget)result._target);
		return result;
	}

	public ActionContext SetActorAndTarget(AEntity actor, ATarget target)
	{
		ActionContext result = this;
		result._actor = actor;
		result._target = target;
		return result;
	}

	public ActionContext SetCapturedValue(int value)
	{
		ActionContext result = this;
		result._capturedValue = value;
		return result;
	}

	public ActionContext SetAbility(Ability abilityToSet)
	{
		ActionContext result = this;
		result._ability = abilityToSet;
		return result;
	}

	public ActionContext SetState(State newState)
	{
		ActionContext result = this;
		result._state = newState;
		return result;
	}

	public T GetTarget<T>(ActionContextTarget contextTarget = ActionContextTarget.Target) where T : ATarget
	{
		return contextTarget switch
		{
			ActionContextTarget.Owner => actor as T, 
			ActionContextTarget.Target => (target as T) ?? _FindTarget<T>(), 
			ActionContextTarget.Attacker => gameState.activeCombat?.attacker as T, 
			ActionContextTarget.Defender => gameState.activeCombat?.defender as T, 
			ActionContextTarget.Player => gameState.player as T, 
			ActionContextTarget.EnemyInActiveCombat => gameState.activeCombat?.enemyCombatant as T, 
			ActionContextTarget.FirstEnemyInTurnOrder => gameState.GetEntities(Faction.Enemy).First() as T, 
			ActionContextTarget.TickTarget => _FindTickTarget<T>() ?? GetTarget<T>(), 
			_ => throw new ArgumentOutOfRangeException("contextTarget", contextTarget, string.Format("ActionContext.GetTarget<{0}> does not handle {1} {2}", "T", contextTarget, "ActionContextTarget")), 
		};
	}

	public static implicit operator bool(ActionContext context)
	{
		if (!context._actorSpecified && !context._abilitySpecified && !context._targetSpecified)
		{
			return context._capturedValue != 0;
		}
		return true;
	}

	public override string ToString()
	{
		return $"Actor: [{_actor}], Ability: [{_ability}], Target: [{_target}]";
	}
}
