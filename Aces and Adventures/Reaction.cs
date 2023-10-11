using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField]
[ProtoInclude(1, typeof(EncounterStart))]
[ProtoInclude(2, typeof(EncounterEnd))]
[ProtoInclude(3, typeof(AbilityUsed))]
[ProtoInclude(4, typeof(BuffReplaced))]
[ProtoInclude(5, typeof(RoundStart))]
[ProtoInclude(6, typeof(CombatantKilled))]
[ProtoInclude(7, typeof(TurnStart))]
[ProtoInclude(8, typeof(TurnEnd))]
[ProtoInclude(9, typeof(AttackLaunched))]
[ProtoInclude(10, typeof(DefenseLaunched))]
[ProtoInclude(11, typeof(CombatVictorDecided))]
[ProtoInclude(12, typeof(ProcessAttackDamage))]
[ProtoInclude(13, typeof(AttackEnd))]
[ProtoInclude(14, typeof(DamageDealt))]
[ProtoInclude(15, typeof(ShieldDamageDealt))]
[ProtoInclude(16, typeof(Overkill))]
[ProtoInclude(17, typeof(Heal))]
[ProtoInclude(18, typeof(OverHeal))]
[ProtoInclude(19, typeof(HealthThreshold))]
[ProtoInclude(20, typeof(SummonReplaced))]
[ProtoInclude(21, typeof(TurnOrder))]
[ProtoInclude(22, typeof(AbilityPileChanged))]
[ProtoInclude(23, typeof(ShieldGain))]
[ProtoInclude(24, typeof(DeathsDoor))]
[ProtoInclude(25, typeof(RoundEnd))]
[ProtoInclude(26, typeof(TopDeckFinishedDrawing))]
[ProtoInclude(27, typeof(ShieldThreshold))]
[ProtoInclude(28, typeof(BeginToUseAbility))]
[ProtoInclude(29, typeof(AbilityTargeting))]
[ProtoInclude(30, typeof(BuffRemoved))]
[ProtoInclude(31, typeof(DynamicNumberRange))]
[ProtoInclude(32, typeof(SummonPlaced))]
[ProtoInclude(33, typeof(SummonRemoved))]
[ProtoInclude(34, typeof(ActionsFinishProcessing))]
[ProtoInclude(35, typeof(BeginDeath))]
[ProtoInclude(36, typeof(BuffPlaced))]
[ProtoInclude(37, typeof(DefensePresent))]
[ProtoInclude(38, typeof(Tap))]
[ProtoInclude(39, typeof(AbilityTick))]
[ProtoInclude(40, typeof(DeckShuffled))]
[ProtoInclude(41, typeof(And))]
[ProtoInclude(42, typeof(TraitRemoved))]
[ProtoInclude(43, typeof(CombatantPileChange))]
public abstract class Reaction
{
	[ProtoContract]
	[UIField(tooltip = "Triggered after an attack hand has been committed against a target, before a defense hand is presented.", category = "Combat")]
	public class AttackLaunched : Reaction
	{
		private void _OnAttackLaunched(ActiveCombat combat)
		{
			_OnAvailable(combat.attacker, combat.defender);
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onAttackLaunched += _OnAttackLaunched;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onAttackLaunched -= _OnAttackLaunched;
		}

		public override string ToString()
		{
			return "Launched Attack Against";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered after attack has been launched, but before a defense hand has been committed.", category = "Combat")]
	public class DefensePresent : Reaction
	{
		private void _OnDefensePresent(ActiveCombat combat)
		{
			_OnAvailable(combat.defender, combat.attacker);
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onDefensePresent += _OnDefensePresent;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onDefensePresent -= _OnDefensePresent;
		}

		public override string ToString()
		{
			return "Can Present Defense Against";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered after a defense hand has been committed against a target, before the victor of the combat is decided.", category = "Combat")]
	public class DefenseLaunched : Reaction
	{
		private void _OnDefenseLaunched(ActiveCombat combat)
		{
			_OnAvailable(combat.defender, combat.attacker);
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onDefenseLaunched += _OnDefenseLaunched;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onDefenseLaunched -= _OnDefenseLaunched;
		}

		public override string ToString()
		{
			return "Launched Defense Against";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered after the victor of combat has been decided, but before the damage has been dealt.", category = "Combat")]
	public class CombatVictorDecided : Reaction
	{
		[ProtoMember(1)]
		[UIField(validateOnChange = true)]
		[UIHorizontalLayout("T", flexibleWidth = 999f)]
		private AttackResultType? _attackResultFilter;

		[ProtoMember(2)]
		[UIField]
		[UIHorizontalLayout("T", flexibleWidth = 0f)]
		[UIHideIf("_hideInvertResult")]
		private bool _invertResult;

		[ProtoMember(3)]
		[UIField(tooltip = "Trigger only when the result of combat can no longer be changed.")]
		private bool _waitForFinalResult;

		private bool _hideInvertResult => !_attackResultFilter.HasValue;

		private bool _IsValidResult(AttackResultType? result)
		{
			return (result == _attackResultFilter) ^ (_invertResult && result.HasValue);
		}

		private void _OnCombatVictorDecided(AttackResultType? previousResult, ActiveCombat combat)
		{
			if (!_attackResultFilter.HasValue || (!_IsValidResult(previousResult) && _IsValidResult(combat.result)))
			{
				_OnAvailable(combat.attacker, combat.defender);
			}
		}

		private void _OnFinalCombatVictorDecided(ActiveCombat combat)
		{
			if (!_attackResultFilter.HasValue || _IsValidResult(combat.result))
			{
				_OnAvailable(combat.attacker, combat.defender);
			}
		}

		public override AbilityPreventedBy? GetAbilityPreventedBy(ReactionEntity triggeredBy, ReactionEntity triggeredOn)
		{
			if (!_attackResultFilter.HasValue)
			{
				return null;
			}
			AttackResultType attackResultType = (_invertResult ? _attackResultFilter.Value.Opposite() : _attackResultFilter.Value);
			return triggeredBy switch
			{
				ReactionEntity.Owner => attackResultType switch
				{
					AttackResultType.Success => AbilityPreventedBy.ReactionAboutToSuccessfullyAttack, 
					AttackResultType.Failure => AbilityPreventedBy.ReactionAboutToFailToAttack, 
					_ => null, 
				}, 
				ReactionEntity.Enemy => attackResultType switch
				{
					AttackResultType.Success => AbilityPreventedBy.ReactionAboutToFailToDefend, 
					AttackResultType.Failure => AbilityPreventedBy.ReactionAboutToSuccessfullyDefend, 
					_ => null, 
				}, 
				_ => null, 
			};
		}

		protected override void _Register(ActionContext context)
		{
			if (_waitForFinalResult)
			{
				context.gameState.onFinalCombatVictorDecided += _OnFinalCombatVictorDecided;
			}
			else
			{
				context.gameState.onCombatVictorDecided += _OnCombatVictorDecided;
			}
		}

		protected override void _Unregister(ActionContext context)
		{
			if (_waitForFinalResult)
			{
				context.gameState.onFinalCombatVictorDecided -= _OnFinalCombatVictorDecided;
			}
			else
			{
				context.gameState.onCombatVictorDecided -= _OnCombatVictorDecided;
			}
		}

		public override string ToString()
		{
			return _waitForFinalResult.ToText("Final ") + "Attack Result" + (_attackResultFilter.HasValue ? ("ed In " + _invertResult.ToText("<b>!</b>") + EnumUtil.FriendlyName(_attackResultFilter)) : " Decided") + " Against";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered after combat victor has been determined, but damage has not yet been committed.", category = "Combat")]
	public class ProcessAttackDamage : Reaction
	{
		[ProtoContract(EnumPassthru = true)]
		public enum Phase
		{
			Processing,
			AboutToProcess,
			AboutToProcessEarly
		}

		[ProtoMember(1)]
		[UIField]
		[UIHorizontalLayout("T")]
		private AttackResultType? _attackResultFilter;

		[ProtoMember(2)]
		[UIField(tooltip = "Should this trigger just before damage is actually processed?\n<i>Useful for reactions which require processing combat damage.</i>")]
		[UIHorizontalLayout("T")]
		private Phase _phase;

		private void _OnProcessCombatDamage(ActiveCombat combat)
		{
			if (combat.context.gameState.reactToProcessDamage && combat.resultIsFinal && (!_attackResultFilter.HasValue || combat.result == _attackResultFilter))
			{
				_OnAvailable(combat.attacker, combat.defender, combat.totalDamage);
			}
		}

		public override AbilityPreventedBy? GetAbilityPreventedBy(ReactionEntity triggeredBy, ReactionEntity triggeredOn)
		{
			return triggeredBy switch
			{
				ReactionEntity.Owner => _attackResultFilter switch
				{
					AttackResultType.Success => AbilityPreventedBy.ReactionProcessingSuccessfulAttackDamage, 
					AttackResultType.Failure => AbilityPreventedBy.ReactionProcessingFailedAttackDamage, 
					_ => null, 
				}, 
				ReactionEntity.Enemy => _attackResultFilter switch
				{
					AttackResultType.Success => AbilityPreventedBy.ReactionProcessingFailedDefenseDamage, 
					AttackResultType.Failure => AbilityPreventedBy.ReactionProcessingSuccessfulDefenseDamage, 
					_ => null, 
				}, 
				_ => null, 
			};
		}

		protected override void _Register(ActionContext context)
		{
			if (_phase == Phase.AboutToProcess)
			{
				context.gameState.onAboutToProcessCombatDamage += _OnProcessCombatDamage;
			}
			else if (_phase == Phase.AboutToProcessEarly)
			{
				context.gameState.onAboutToProcessCombatDamageEarly += _OnProcessCombatDamage;
			}
			else
			{
				context.gameState.onProcessCombatDamage += _OnProcessCombatDamage;
			}
		}

		protected override void _Unregister(ActionContext context)
		{
			if (_phase == Phase.AboutToProcess)
			{
				context.gameState.onAboutToProcessCombatDamage -= _OnProcessCombatDamage;
			}
			else if (_phase == Phase.AboutToProcessEarly)
			{
				context.gameState.onAboutToProcessCombatDamageEarly -= _OnProcessCombatDamage;
			}
			else
			{
				context.gameState.onProcessCombatDamage -= _OnProcessCombatDamage;
			}
		}

		public override string ToString()
		{
			return "Is " + EnumUtil.FriendlyName(_phase) + " " + _attackResultFilter.PastTense().SpaceIfNotEmpty() + "Attack Damage Against";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when combat has just finished being processed in its entirety.", category = "Combat")]
	public class AttackEnd : Reaction
	{
		[ProtoMember(1)]
		[UIField(validateOnChange = true)]
		[UIHorizontalLayout("T", flexibleWidth = 999f)]
		private AttackResultType? _attackResultFilter;

		[ProtoMember(2)]
		[UIField(tooltip = "Ignore successful attacks that were triggered via an attack damage action from an ability.")]
		[UIHorizontalLayout("T", flexibleWidth = 0f, minWidth = 180f)]
		private bool _mustBeRealCombat;

		private void _OnCombatEnd(ActiveCombat combat)
		{
			if ((!_attackResultFilter.HasValue || _attackResultFilter == combat.result) && (!_mustBeRealCombat || !combat.createdByAction))
			{
				_OnAvailable(combat.attacker, combat.defender, combat.totalDamage);
			}
		}

		public override AbilityPreventedBy? GetAbilityPreventedBy(ReactionEntity triggeredBy, ReactionEntity triggeredOn)
		{
			return triggeredBy switch
			{
				ReactionEntity.Owner => _attackResultFilter switch
				{
					AttackResultType.Success => AbilityPreventedBy.ReactionSuccessfulAttack, 
					AttackResultType.Failure => AbilityPreventedBy.ReactionFailedAttack, 
					_ => null, 
				}, 
				ReactionEntity.Enemy => _attackResultFilter switch
				{
					AttackResultType.Success => AbilityPreventedBy.ReactionFailedDefense, 
					AttackResultType.Failure => AbilityPreventedBy.ReactionSuccessfulDefense, 
					_ => AbilityPreventedBy.ReactionFinishDefending, 
				}, 
				_ => null, 
			};
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onCombatEnd += _OnCombatEnd;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onCombatEnd -= _OnCombatEnd;
		}

		public override IEnumerable<AbilityKeyword> GetKeywords()
		{
			if (_mustBeRealCombat)
			{
				yield return AbilityKeyword.StandardAttack;
			}
		}

		public override string ToString()
		{
			return "Ended " + _mustBeRealCombat.ToText("<b>Real</b> ") + "Attack" + (_attackResultFilter.HasValue ? (" In " + EnumUtil.FriendlyName(_attackResultFilter)) : "") + " Against";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a combatant changes piles.", category = "Combat")]
	public class CombatantPileChange : Reaction
	{
		[ProtoMember(1)]
		[UIField]
		private AdventureCard.Piles? _from;

		[ProtoMember(2)]
		[UIField]
		private AdventureCard.Piles? _to;

		public override bool usesTriggeredOn => false;

		private void _OnAdventureTransfer(ATarget card, AdventureCard.Pile? oldPile, AdventureCard.Pile? newPile)
		{
			if (card is ACombatant triggeredBy && (!_from.HasValue || EnumUtil.HasFlagConvert(_from.Value, oldPile)) && (!_to.HasValue || EnumUtil.HasFlagConvert(_to.Value, newPile)))
			{
				_OnAvailable(triggeredBy, null);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.adventureDeck.onTransfer += _OnAdventureTransfer;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.adventureDeck.onTransfer -= _OnAdventureTransfer;
		}

		public override string ToString()
		{
			return "is transferred from " + (_from.HasValue ? EnumUtil.FriendlyName(_from.Value) : "anywhere") + " to " + (_to.HasValue ? EnumUtil.FriendlyName(_to.Value) : "anywhere");
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when damage has been dealt to a combatant, whether it be from combat or an ability.", category = "Damage")]
	public class DamageDealt : Reaction
	{
		[ProtoMember(1)]
		[UIField(validateOnChange = true, tooltip = "The sources of damage that will trigger this event.")]
		[DefaultValue(DamageSources.Attack | DamageSources.Defense | DamageSources.Ability)]
		private DamageSources _sources = DamageSources.Attack | DamageSources.Defense | DamageSources.Ability;

		[ProtoMember(2, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must hold true for ability that was used in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		[UIHideIf("_hideAbilityConditions")]
		private List<AAction.Condition.AAbility> _abilityConditions;

		[ProtoMember(5, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must hold for action that was used in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		[UIHideIf("_hideAbilityConditions")]
		private List<AAction.Filter> _actionConditions;

		[ProtoMember(3)]
		[UIField(tooltip = "Ignore attack damage dealt by abilities.", validateOnChange = true)]
		[UIHideIf("_hideMustBeRealCombat")]
		private bool _mustBeRealCombat;

		[ProtoMember(4)]
		[UIField(tooltip = "Damage dealt to shields will not be counted towards this trigger.")]
		private bool _ignoreShieldDamage;

		private bool _hideAbilityConditions => _mustBeRealCombatSpecified;

		private bool _hideMustBeRealCombat => !EnumUtil.HasFlag(_sources, DamageSources.Attack | DamageSources.Defense);

		private bool _abilityConditionsSpecified
		{
			get
			{
				List<AAction.Condition.AAbility> abilityConditions = _abilityConditions;
				if (abilityConditions != null && abilityConditions.Count > 0)
				{
					return !_hideAbilityConditions;
				}
				return false;
			}
		}

		private bool _mustBeRealCombatSpecified
		{
			get
			{
				if (_mustBeRealCombat)
				{
					return !_hideMustBeRealCombat;
				}
				return false;
			}
		}

		private void _OnDamageDealt(ActionContext context, AAction action, int damage, DamageSource source)
		{
			if ((!_mustBeRealCombat || action == null) && EnumUtil.HasFlagConvert(_sources, source) && (!context.hasAbility || (_abilityConditions.All<AAction.Condition.AAbility>(context.SetTarget(context.ability)) && _actionConditions.All(context, action))))
			{
				_OnAvailable(context.actor, context.target as AEntity, damage);
			}
		}

		public override AbilityPreventedBy? GetAbilityPreventedBy(ReactionEntity triggeredBy, ReactionEntity triggeredOn)
		{
			if (triggeredBy == ReactionEntity.Owner)
			{
				return _sources switch
				{
					DamageSources.Attack => AbilityPreventedBy.ReactionDealAttackDamage, 
					DamageSources.Defense => AbilityPreventedBy.ReactionDealDefenseDamage, 
					DamageSources.Ability => AbilityPreventedBy.ReactionDealAbilityDamage, 
					DamageSources.Attack | DamageSources.Defense | DamageSources.Ability => AbilityPreventedBy.ReactionDealDamage, 
					_ => null, 
				};
			}
			return null;
		}

		protected override void _Register(ActionContext context)
		{
			_abilityConditions.Register(context);
			if (_ignoreShieldDamage)
			{
				context.gameState.onDamageDealt += _OnDamageDealt;
			}
			else
			{
				context.gameState.onTotalDamageDealt += _OnDamageDealt;
			}
		}

		protected override void _Unregister(ActionContext context)
		{
			_abilityConditions.Unregister(context);
			if (_ignoreShieldDamage)
			{
				context.gameState.onDamageDealt -= _OnDamageDealt;
			}
			else
			{
				context.gameState.onTotalDamageDealt -= _OnDamageDealt;
			}
		}

		public override IEnumerable<AbilityKeyword> GetKeywords()
		{
			if (_sources == DamageSources.Attack)
			{
				yield return AbilityKeyword.AttackDamageModifier;
			}
			else if (_sources == DamageSources.Defense)
			{
				yield return AbilityKeyword.DefenseDamageModifier;
			}
			else if (_sources == (DamageSources.Attack | DamageSources.Defense))
			{
				yield return AbilityKeyword.CombatDamageModifier;
			}
			if (_mustBeRealCombatSpecified)
			{
				yield return AbilityKeyword.StandardAttack;
			}
		}

		public override string ToString()
		{
			return "Deals " + (_mustBeRealCombatSpecified ? "<b>Real</b> " : "") + ((!_hideAbilityConditions) ? (_abilityConditions.ToStringSmart(" & ") + _actionConditions.ToStringSmart(" & ").PreSpaceIf(!_abilityConditions.IsNullOrEmpty())) : "").SizeIfNotEmpty().SpaceIfNotEmpty() + ((_sources != EnumUtil<DamageSources>.AllFlags) ? (_sources.GetText() + " ") : "") + "Damage to";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when damage has been dealt to a combatant that has lowered its HP below 0, whether it be from combat or an ability.", category = "Damage")]
	public class Overkill : Reaction
	{
		[ProtoMember(1)]
		[UIField(tooltip = "The sources of damage that will trigger this event.")]
		[DefaultValue(DamageSources.Attack | DamageSources.Defense | DamageSources.Ability)]
		private DamageSources _sources = DamageSources.Attack | DamageSources.Defense | DamageSources.Ability;

		private void _OnOverkill(ActionContext context, AAction action, int overkillDamage, DamageSource source)
		{
			if (EnumUtil.HasFlagConvert(_sources, source))
			{
				_OnAvailable(context.actor, context.target as AEntity, overkillDamage);
			}
		}

		public override AbilityPreventedBy? GetAbilityPreventedBy(ReactionEntity triggeredBy, ReactionEntity triggeredOn)
		{
			if (!triggeredBy.OwnerOrAnyone() || triggeredOn != ReactionEntity.Enemy)
			{
				return null;
			}
			return AbilityPreventedBy.ReactionOverkillEnemy;
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onOverkill += _OnOverkill;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onOverkill -= _OnOverkill;
		}

		public override IEnumerable<AbilityKeyword> GetKeywords()
		{
			yield return AbilityKeyword.Overkill;
		}

		public override string ToString()
		{
			return (_sources != EnumUtil<DamageSources>.AllFlags).ToText("'s " + EnumUtil.FriendlyName(_sources) + " damage ") + "Overkills";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when damage has been dealt that has reduced a combatant's shield amount.", category = "Damage")]
	public class ShieldDamageDealt : Reaction
	{
		private void _OnShieldDamageDealt(ActionContext context, AAction action, int damage, DamageSource source)
		{
			_OnAvailable(context.actor, context.target as AEntity, damage);
		}

		public override AbilityPreventedBy? GetAbilityPreventedBy(ReactionEntity triggeredBy, ReactionEntity triggeredOn)
		{
			if (triggeredOn != 0)
			{
				return null;
			}
			return (triggeredBy == ReactionEntity.Enemy) ? AbilityPreventedBy.ReactionEnemyDamagesShield : AbilityPreventedBy.ReactionLosingShield;
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onShieldDamageDealt += _OnShieldDamageDealt;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onShieldDamageDealt -= _OnShieldDamageDealt;
		}

		public override string ToString()
		{
			return "Deals Shield Damage to";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a combatant is about to be killed.", category = "Damage")]
	public class DeathsDoor : Reaction
	{
		private void _OnDeathsDoor(ActionContext context, AAction action)
		{
			_OnAvailable(context.actor, context.target as AEntity);
		}

		public override AbilityPreventedBy? GetAbilityPreventedBy(ReactionEntity triggeredBy, ReactionEntity triggeredOn)
		{
			if (!triggeredBy.OwnerOrAnyone() || triggeredOn != ReactionEntity.Enemy)
			{
				if (triggeredOn != 0)
				{
					return null;
				}
				return AbilityPreventedBy.ReactionDeathsDoorPlayer;
			}
			return AbilityPreventedBy.ReactionDeathsDoorEnemy;
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onDeathsDoor += _OnDeathsDoor;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onDeathsDoor -= _OnDeathsDoor;
		}

		public override string ToString()
		{
			return "Is about to kill";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a combatant is certain to die, but has not yet died.", category = "Damage")]
	public class BeginDeath : Reaction
	{
		private void _OnBeginDeath(ActionContext context, AAction action)
		{
			_OnAvailable(context.actor, context.target as AEntity);
		}

		public override AbilityPreventedBy? GetAbilityPreventedBy(ReactionEntity triggeredBy, ReactionEntity triggeredOn)
		{
			if (!triggeredBy.OwnerOrAnyone() || triggeredOn != ReactionEntity.Enemy)
			{
				return null;
			}
			return AbilityPreventedBy.ReactionKillEnemy;
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onBeginDeath += _OnBeginDeath;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onBeginDeath -= _OnBeginDeath;
		}

		public override string ToString()
		{
			return "Has just killed";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a combatant has been killed.", category = "Damage")]
	public class CombatantKilled : Reaction
	{
		private void _OnDeath(ActionContext context, AAction action)
		{
			_OnAvailable(context.actor, context.target as AEntity);
		}

		public override AbilityPreventedBy? GetAbilityPreventedBy(ReactionEntity triggeredBy, ReactionEntity triggeredOn)
		{
			if (!triggeredBy.OwnerOrAnyone() || triggeredOn != ReactionEntity.Enemy)
			{
				return null;
			}
			return AbilityPreventedBy.ReactionKillEnemy;
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onDeath += _OnDeath;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onDeath -= _OnDeath;
		}

		public override string ToString()
		{
			return "Kills";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a combatant's HP changes and satisfies threshold settings.", category = "Damage")]
	public class HealthThreshold : Reaction
	{
		private static readonly RangeByte HP_RANGE = new RangeByte(0, 4, 0, 20, 0, 0);

		[ProtoMember(1)]
		[UIField]
		private RangeByte _hpRange = HP_RANGE;

		[ProtoMember(2)]
		[UIField]
		[UIHorizontalLayout("B")]
		[DefaultValue(RangeThreshold.OutsideOf)]
		private RangeThreshold _begins = RangeThreshold.OutsideOf;

		[ProtoMember(3)]
		[UIField]
		[UIHorizontalLayout("B")]
		private RangeThreshold _ends;

		public override bool usesTriggeredBy => false;

		private bool _hpRangeSpecified => _hpRange != HP_RANGE;

		private void _OnHPChange(ACombatant combatant, int previousHP, int currentHP)
		{
			if (_hpRange.Threshold(previousHP, _begins) && _hpRange.Threshold(currentHP, _ends))
			{
				_OnAvailable(null, combatant, currentHP - previousHP);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onHPChange += _OnHPChange;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onHPChange -= _OnHPChange;
		}

		public override string ToString()
		{
			return "HP Transitions from " + EnumUtil.FriendlyName(_begins).SizeIfNotEmpty() + " to " + EnumUtil.FriendlyName(_ends).SizeIfNotEmpty() + " " + _hpRange.ToRangeString(null, "", 50) + " for";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a combatant's Shield changes and satisfies threshold settings.", category = "Damage")]
	public class ShieldThreshold : Reaction
	{
		private static readonly RangeByte SHIELD_RANGE = new RangeByte(0, 0, 0, 20, 0, 0);

		[ProtoMember(1)]
		[UIField]
		private RangeByte _shieldRange = SHIELD_RANGE;

		[ProtoMember(2)]
		[UIField]
		[UIHorizontalLayout("B")]
		[DefaultValue(RangeThreshold.OutsideOf)]
		private RangeThreshold _begins = RangeThreshold.OutsideOf;

		[ProtoMember(3)]
		[UIField]
		[UIHorizontalLayout("B")]
		private RangeThreshold _ends;

		public override bool usesTriggeredBy => false;

		private bool _shieldRangeSpecified => _shieldRange != SHIELD_RANGE;

		private void _OnShieldChange(ACombatant combatant, int previousShield, int currentShield)
		{
			if (_shieldRange.Threshold(previousShield, _begins) && _shieldRange.Threshold(currentShield, _ends))
			{
				_OnAvailable(null, combatant, currentShield - previousShield);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onShieldChange += _OnShieldChange;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onShieldChange -= _OnShieldChange;
		}

		public override string ToString()
		{
			return "Shield Transitions from " + EnumUtil.FriendlyName(_begins).SizeIfNotEmpty() + " to " + EnumUtil.FriendlyName(_ends).SizeIfNotEmpty() + " " + _shieldRange.ToRangeString(null, "", 50).AppendSpace() + "for";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a combatant has been healed.", category = "Heal")]
	public class Heal : Reaction
	{
		private void _OnHeal(ActionContext context, AAction action, int heal)
		{
			_OnAvailable(context.actor, context.target as AEntity, heal);
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onHeal += _OnHeal;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onHeal -= _OnHeal;
		}

		public override string ToString()
		{
			return "Heals";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when healing on a combatant would be taking them over their max HP.", category = "Heal")]
	public class OverHeal : Reaction
	{
		private void _OnOverHeal(ActionContext context, AAction action, int overHeal)
		{
			_OnAvailable(context.actor, context.target as AEntity, overHeal);
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onOverHeal += _OnOverHeal;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onOverHeal -= _OnOverHeal;
		}

		public override string ToString()
		{
			return "Over-Heals";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a combatant has gained shields.", category = "Heal")]
	public class ShieldGain : Reaction
	{
		private void _OnShieldGained(ActionContext context, AAction action, int shieldAmount)
		{
			_OnAvailable(context.actor, context.target as AEntity, shieldAmount);
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onShieldGain += _OnShieldGained;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onShieldGain -= _OnShieldGained;
		}

		public override string ToString()
		{
			return "Shields";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when encounter starts, right before the start of Round 1.", category = "Time")]
	public class EncounterStart : Reaction
	{
		public static readonly RangeByte RANGE = new RangeByte(1, 10, 1, 10, 0, 0);

		[ProtoMember(1)]
		[UIField]
		private RangeByte _encounterRange = RANGE;

		public override bool usesTriggeredBy => false;

		public override bool usesTriggeredOn => false;

		private bool _encounterRangeSpecified => _encounterRange != RANGE;

		private void _OnEncounterStart(int encounterNumber)
		{
			if (_encounterRange.InRangeSmart(encounterNumber))
			{
				_OnAvailable(null, null);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onEncounterStart += _OnEncounterStart;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onEncounterStart -= _OnEncounterStart;
		}

		public override string ToString()
		{
			return "Encounter " + _encounterRange.ToRangeString(RANGE, "", 50).SpaceIfNotEmpty() + "Starts";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a new round starts, before first entity in turn order begins turn.", category = "Time")]
	public class RoundStart : Reaction
	{
		private static readonly RangeByte RANGE = new RangeByte(1, 20, 1, 20, 0, 0);

		[ProtoMember(1)]
		[UIField]
		private RangeByte _roundRange = RANGE;

		public override bool usesTriggeredBy => false;

		public override bool usesTriggeredOn => false;

		private bool _roundRangeSpecified => _roundRange != RANGE;

		private void _OnRoundStart(int round)
		{
			if (_roundRange.InRangeSmart(round))
			{
				_OnAvailable(null, null, round);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onRoundStart += _OnRoundStart;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onRoundStart -= _OnRoundStart;
		}

		public override string ToString()
		{
			return "Round " + _roundRange.ToRangeString(RANGE, "", 50).SpaceIfNotEmpty() + "Starts";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a round ends, right before next round starts.", category = "Time")]
	public class RoundEnd : Reaction
	{
		private static readonly RangeByte RANGE = new RangeByte(1, 20, 1, 20, 0, 0);

		[ProtoMember(1)]
		[UIField]
		private RangeByte _roundRange = RANGE;

		public override bool usesTriggeredBy => false;

		public override bool usesTriggeredOn => false;

		private bool _roundRangeSpecified => _roundRange != RANGE;

		private void _OnRoundEnd(int round)
		{
			if (_roundRange.InRangeSmart(round))
			{
				_OnAvailable(null, null, round);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onRoundEnd += _OnRoundEnd;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onRoundEnd -= _OnRoundEnd;
		}

		public override string ToString()
		{
			return "Round " + _roundRange.ToRangeString(RANGE, "", 50).SpaceIfNotEmpty() + "Ends";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when an entity begins its turn, before taking any non-triggered actions.", category = "Time")]
	public class TurnStart : Reaction
	{
		[ProtoContract(EnumPassthru = true)]
		public enum Phase
		{
			Standard,
			Late,
			Early
		}

		[ProtoMember(1)]
		[UIField]
		private Phase _phase;

		public override bool usesTriggeredOn => false;

		private void _OnTurnStart(AEntity entity)
		{
			_OnAvailable(entity, entity);
		}

		protected override void _Register(ActionContext context)
		{
			switch (_phase)
			{
			case Phase.Standard:
				context.gameState.onTurnStart += _OnTurnStart;
				break;
			case Phase.Late:
				context.gameState.onTurnStartLate += _OnTurnStart;
				break;
			case Phase.Early:
				context.gameState.onTurnStartEarly += _OnTurnStart;
				break;
			}
		}

		protected override void _Unregister(ActionContext context)
		{
			switch (_phase)
			{
			case Phase.Standard:
				context.gameState.onTurnStart -= _OnTurnStart;
				break;
			case Phase.Late:
				context.gameState.onTurnStartLate -= _OnTurnStart;
				break;
			case Phase.Early:
				context.gameState.onTurnStartEarly -= _OnTurnStart;
				break;
			}
		}

		public override string ToString()
		{
			return "Starts Turn" + (_phase != Phase.Standard).ToText(" (" + EnumUtil.FriendlyName(_phase) + ")").SizeIfNotEmpty();
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when an entity ends its turn, and no further non-triggered actions will be taken.", category = "Time")]
	public class TurnEnd : Reaction
	{
		public override bool usesTriggeredOn => false;

		private void _OnTurnEnd(AEntity entity)
		{
			_OnAvailable(entity, entity);
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onTurnEnd += _OnTurnEnd;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onTurnEnd -= _OnTurnEnd;
		}

		public override string ToString()
		{
			return "Ends Turn";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when encounter ends. Right before returning to adventure step.", category = "Time")]
	public class EncounterEnd : Reaction
	{
		public static readonly RangeByte RANGE = new RangeByte(1, 10, 1, 10, 0, 0);

		[ProtoMember(1)]
		[UIField]
		private RangeByte _encounterRange = RANGE;

		public override bool usesTriggeredBy => false;

		public override bool usesTriggeredOn => false;

		private bool _encounterRangeSpecified => _encounterRange != RANGE;

		private void _OnEncounterEnd(int encounterNumber)
		{
			if (_encounterRange.InRangeSmart(encounterNumber))
			{
				_OnAvailable(null, null);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onEncounterEnd += _OnEncounterEnd;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onEncounterEnd -= _OnEncounterEnd;
		}

		public override string ToString()
		{
			return "Encounter " + _encounterRange.ToRangeString(RANGE, "", 50).SpaceIfNotEmpty() + "Ends";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when all steps for a given action have been processed. This includes reactions to the action.", category = "Ability")]
	public class ActionsFinishProcessing : Reaction
	{
		public override bool usesTriggeredBy => false;

		public override bool usesTriggeredOn => false;

		private void _OnControlGained(AEntity entity, ControlGainType controlType)
		{
			if (controlType != ControlGainType.PlayerReaction)
			{
				_OnAvailable(entity, entity);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onControlGained += _OnControlGained;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onControlGained -= _OnControlGained;
		}

		public override string ToString()
		{
			return "Actions Finish Processing";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a combatant has just begun to use an ability.", category = "Ability")]
	public class BeginToUseAbility : Reaction
	{
		private static readonly RangeByte NUMBER_OF_TARGETS = new RangeByte(1, 5, 0, 5, 0, 0);

		[ProtoMember(1, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must hold true for ability that is being used in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition.AAbility> _abilityConditions;

		[ProtoMember(2, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must hold true for targets of the ability that is being used in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition> _targetConditions;

		[ProtoMember(3)]
		[UIField(tooltip = "Number of targets that must be targeted by the ability that is being used in order to trigger.")]
		private RangeByte _numberOfTargets = NUMBER_OF_TARGETS;

		public override bool usesTriggeredOn => false;

		private bool _numberOfTargetsSpecified => _numberOfTargets != NUMBER_OF_TARGETS;

		private void _OnBeginToUseAbility(Ability ability, List<ATarget> targets)
		{
			if (!_abilityConditions.All<AAction.Condition.AAbility>(new ActionContext(ability.owner, ability, ability)))
			{
				return;
			}
			object copyFrom;
			if (!_targetConditions.IsNullOrEmpty())
			{
				copyFrom = targets.Where((ATarget target) => _targetConditions.All(new ActionContext(ability.owner, ability, target)));
			}
			else
			{
				copyFrom = targets;
			}
			using PoolKeepItemHashSetHandle<ATarget> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet((IEnumerable<ATarget>)copyFrom);
			if (_numberOfTargets.InRangeSmart(poolKeepItemHashSetHandle.Count))
			{
				_OnAvailable(ability.owner, null);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onBeginToUseAbility += _OnBeginToUseAbility;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onBeginToUseAbility -= _OnBeginToUseAbility;
		}

		public override string ToString()
		{
			return "Begins to use " + _abilityConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + "Ability" + (_numberOfTargetsSpecified || !_targetConditions.IsNullOrEmpty()).ToText(" on " + _numberOfTargets.ToRangeString(NUMBER_OF_TARGETS, "", 50).SpaceIfNotEmpty() + _targetConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + "<size=66%>" + "target".Pluralize((!_numberOfTargetsSpecified) ? 1 : _numberOfTargets.max) + "</size>");
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when an ability action has set its targets, but before it has executed the action.", category = "Ability")]
	public class AbilityTargeting : Reaction
	{
		private static readonly RangeByte NUMBER_OF_TARGETS = new RangeByte(1, 5, 0, 5, 0, 0);

		[ProtoMember(1, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must hold true for ability that is being used in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition.AAbility> _abilityConditions;

		[ProtoMember(2, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must hold true for action which is currently targeting in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Filter> _actionConditions;

		[ProtoMember(3, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must hold true for targets of the action that is being used in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition> _targetConditions;

		[ProtoMember(4)]
		[UIField(tooltip = "Number of targets that must be targeted by the action that is being used in order to trigger.")]
		private RangeByte _numberOfTargets = NUMBER_OF_TARGETS;

		[ProtoMember(5)]
		[UIField(validateOnChange = true, tooltip = "Should this event trigger multiple times if an action has multiple targets?")]
		[DefaultValue(true)]
		[UIHorizontalLayout("B", flexibleWidth = 0f)]
		private bool _triggerEventPerTarget = true;

		[ProtoMember(6)]
		[UIField(tooltip = "Exclude trait abilities from triggering.")]
		[DefaultValue(true)]
		[UIHorizontalLayout("B", flexibleWidth = 0f)]
		private bool _ignoreTraits = true;

		[ProtoMember(8)]
		[UIField(tooltip = "Exclude summon abilities from triggering.")]
		[DefaultValue(true)]
		[UIHorizontalLayout("B", flexibleWidth = 0f)]
		private bool _ignoreSummons = true;

		[ProtoMember(7)]
		[UIField(tooltip = "Determines if direct actions, or ticking actions count towards triggering.")]
		[DefaultValue(ActionContext.States.Act)]
		[UIHorizontalLayout("B", flexibleWidth = 999f)]
		private ActionContext.States _actionStates = ActionContext.States.Act;

		public override bool usesTriggeredOn => _triggerEventPerTarget;

		private bool _numberOfTargetsSpecified => _numberOfTargets != NUMBER_OF_TARGETS;

		private void _OnAbilityTargeting(ActionContext context, AAction action, List<ATarget> targets)
		{
			if (!action.hasEffectOnTarget)
			{
				Ability ability2 = context.ability;
				if (ability2 == null || !ability2.isBuff || ability2.data.actions.IndexOf(action) != 0)
				{
					return;
				}
			}
			if (!EnumUtil.HasFlagConvert(_actionStates, context.state))
			{
				return;
			}
			Ability ability = context.ability;
			if ((_ignoreTraits && ability.isTrait) || (_ignoreSummons && ability.isSummon) || !_abilityConditions.All<AAction.Condition.AAbility>(new ActionContext(ability.owner, ability, ability)) || !_actionConditions.All(context, action))
			{
				return;
			}
			object copyFrom;
			if (!_targetConditions.IsNullOrEmpty())
			{
				copyFrom = targets.Where((ATarget target) => _targetConditions.All(new ActionContext(ability.owner, ability, target)));
			}
			else
			{
				copyFrom = targets;
			}
			using PoolKeepItemHashSetHandle<ATarget> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet((IEnumerable<ATarget>)copyFrom);
			if (!_numberOfTargets.InRangeSmart(poolKeepItemHashSetHandle.Count) || ability.gameState.stack.activeStep.contextGroup.HasHashTag(this))
			{
				return;
			}
			if (!_triggerEventPerTarget)
			{
				_OnAvailable(ability.owner, null);
				return;
			}
			foreach (ATarget item in poolKeepItemHashSetHandle.value)
			{
				if (item is AEntity triggeredOn)
				{
					_OnAvailable(ability.owner, triggeredOn);
				}
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onAbilityTargeting += _OnAbilityTargeting;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onAbilityTargeting -= _OnAbilityTargeting;
		}

		protected override void _OnValid(ActionContext context)
		{
			context.gameState.stack.activeStep.contextGroup.AddHashTag(this);
		}

		public override string ToString()
		{
			return ((_actionStates != ActionContext.States.Act) ? (EnumUtil.FriendlyName(_actionStates) + " ") : "") + "Targets " + _actionConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + _abilityConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + "Ability at" + (_numberOfTargetsSpecified || !_targetConditions.IsNullOrEmpty()).ToText(" on " + _numberOfTargets.ToRangeString(NUMBER_OF_TARGETS, "", 50).SpaceIfNotEmpty() + _targetConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + "<size=66%>" + "target".Pluralize((!_numberOfTargetsSpecified) ? 1 : _numberOfTargets.max) + "</size>") + _triggerEventPerTarget.ToText("", "<size=66%> (Trigger Per Action)</size>") + _ignoreTraits.ToText("", "<size=66%> (Include Traits)</size>") + _ignoreSummons.ToText("", "<size=66%> (Include Summons)</size>");
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a combatant has just finished using an ability.", category = "Ability")]
	public class AbilityUsed : Reaction
	{
		[ProtoContract(EnumPassthru = true)]
		public enum CapturedValue
		{
			NumberOfActivationCards,
			NumberOfTargets
		}

		[ProtoContract(EnumPassthru = true)]
		public enum ResponseToInterrupt
		{
			DoNotTriggerIfInterrupted,
			TriggerIfInterrupted,
			OnlyTriggerIfInterrupted
		}

		private static readonly RangeByte NUMBER_OF_TARGETS = new RangeByte(1, 5, 0, 5, 0, 0);

		[ProtoMember(1, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must hold true for ability that was used in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition.AAbility> _abilityConditions;

		[ProtoMember(2, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must hold true for targets of the ability that was used in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition> _targetConditions;

		[ProtoMember(3)]
		[UIField(tooltip = "Number of targets that must be affected by the ability that was used in order to trigger.")]
		private RangeByte _numberOfTargets = NUMBER_OF_TARGETS;

		[ProtoMember(4)]
		[UIField(validateOnChange = true, tooltip = "Should this event trigger multiple times if the ability was used on multiple targets?")]
		private bool _triggerEventPerTarget;

		[ProtoMember(5)]
		[UIField(tooltip = "What should be placed into event's captured value.")]
		private CapturedValue _capturedValue;

		[ProtoMember(6)]
		[UIField(tooltip = "Trigger just before ability used end steps occur.")]
		[UIHorizontalLayout("Bool", flexibleWidth = 0f)]
		private bool _isAboutToFinish;

		[ProtoMember(7)]
		[UIField(tooltip = "Determines if abilities that have been interrupted trigger.")]
		[UIHorizontalLayout("Bool", flexibleWidth = 999f)]
		private ResponseToInterrupt _responseToInterrupted;

		public override bool usesTriggeredOn => _triggerEventPerTarget;

		private bool _numberOfTargetsSpecified => _numberOfTargets != NUMBER_OF_TARGETS;

		private void _OnAbilityUsed(Ability ability, List<ATarget> targets, bool interrupted)
		{
			if (_responseToInterrupted switch
			{
				ResponseToInterrupt.DoNotTriggerIfInterrupted => interrupted, 
				ResponseToInterrupt.TriggerIfInterrupted => false, 
				ResponseToInterrupt.OnlyTriggerIfInterrupted => !interrupted, 
				_ => false, 
			} || !_abilityConditions.All<AAction.Condition.AAbility>(new ActionContext(ability.owner, ability, ability)))
			{
				return;
			}
			object copyFrom;
			if (!_targetConditions.IsNullOrEmpty())
			{
				copyFrom = targets.Where((ATarget target) => _targetConditions.All(new ActionContext(ability.owner, ability, target)));
			}
			else
			{
				copyFrom = targets;
			}
			using PoolKeepItemHashSetHandle<ATarget> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet((IEnumerable<ATarget>)copyFrom);
			if (!_numberOfTargets.InRangeSmart(poolKeepItemHashSetHandle.Count))
			{
				return;
			}
			int capturedValue = _capturedValue switch
			{
				CapturedValue.NumberOfTargets => poolKeepItemHashSetHandle.Count, 
				CapturedValue.NumberOfActivationCards => ability.owner.resourceDeck.Count(ResourceCard.Pile.ActivationHand), 
				_ => 0, 
			};
			if (!_triggerEventPerTarget)
			{
				_OnAvailable(ability.owner, null, capturedValue);
				return;
			}
			foreach (AEntity item in poolKeepItemHashSetHandle.value.OfType<AEntity>())
			{
				_OnAvailable(ability.owner, item, capturedValue);
			}
		}

		public override AbilityPreventedBy? GetAbilityPreventedBy(ReactionEntity triggeredBy, ReactionEntity triggeredOn)
		{
			if (triggeredBy != 0)
			{
				return null;
			}
			return AbilityPreventedBy.ReactionUsingAbility;
		}

		protected override void _Register(ActionContext context)
		{
			if (_isAboutToFinish)
			{
				context.gameState.onAbilityAboutToFinish += _OnAbilityUsed;
			}
			else
			{
				context.gameState.onAbilityUsed += _OnAbilityUsed;
			}
			_abilityConditions.Register(context);
			_targetConditions.Register(context);
		}

		protected override void _Unregister(ActionContext context)
		{
			if (_isAboutToFinish)
			{
				context.gameState.onAbilityAboutToFinish -= _OnAbilityUsed;
			}
			else
			{
				context.gameState.onAbilityUsed -= _OnAbilityUsed;
			}
			_abilityConditions.Unregister(context);
			_targetConditions.Unregister(context);
		}

		public override string ToString()
		{
			return (_isAboutToFinish ? "is about to finish using" : "uses") + " " + _abilityConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + "Ability" + (_responseToInterrupted != ResponseToInterrupt.DoNotTriggerIfInterrupted).ToText(" <b>" + EnumUtil.FriendlyName(_responseToInterrupted).SizeIfNotEmpty() + "</b>") + (_numberOfTargetsSpecified || !_targetConditions.IsNullOrEmpty()).ToText(" on " + _numberOfTargets.ToRangeString(NUMBER_OF_TARGETS, "", 50).SpaceIfNotEmpty() + _targetConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + "<size=66%>" + "target".Pluralize((!_numberOfTargetsSpecified) ? 1 : _numberOfTargets.max) + "</size>") + _triggerEventPerTarget.ToText("<size=66%> (Trigger Per Target)</size>") + ((_capturedValue != 0) ? ("<size=66%> (Capture: " + EnumUtil.FriendlyName(_capturedValue) + ")</size>") : "");
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a buff or debuff is placed on an entity.", category = "Ability")]
	public class BuffPlaced : Reaction
	{
		[ProtoMember(1, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must be true for <b>Buff Being Placed</b> in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition.AAbility> _buffBeingPlacedConditions;

		[ProtoMember(2)]
		[UIField(tooltip = "Will not trigger when replacing another buff or debuff.")]
		private bool _ignoreWhenReplacing;

		private Ability _buffThatIsReplacing;

		private void _OnBuffPlaced(ACombatant actor, ACombatant effected, Ability buffBeingPlaced)
		{
			if (_buffBeingPlacedConditions.All<AAction.Condition.AAbility>(new ActionContext(actor, null, buffBeingPlaced)) && buffBeingPlaced != _buffThatIsReplacing)
			{
				_OnAvailable(actor, effected);
			}
			_buffThatIsReplacing = null;
		}

		private void _OnBuffReplaced(ACombatant actor, ACombatant effected, Ability buffBeingReplaced, Ability buffThatIsReplacing)
		{
			_buffThatIsReplacing = buffThatIsReplacing;
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onBuffPlaced += _OnBuffPlaced;
			if (_ignoreWhenReplacing)
			{
				context.gameState.onBuffReplaced += _OnBuffReplaced;
			}
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onBuffPlaced -= _OnBuffPlaced;
			if (_ignoreWhenReplacing)
			{
				context.gameState.onBuffReplaced -= _OnBuffReplaced;
			}
		}

		public override string ToString()
		{
			return "Places " + _buffBeingPlacedConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + "Ability" + _ignoreWhenReplacing.ToText(" (without replacing)".SizeIfNotEmpty()) + " on";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a buff or debuff is being replaced by another.", category = "Ability")]
	public class BuffReplaced : Reaction
	{
		[ProtoMember(1)]
		[UIField(validateOnChange = true, tooltip = "Buff being replaced must be the ability which registered this reaction in order to trigger.")]
		[DefaultValue(true)]
		private bool _respondToOwningAbilityReplaced = true;

		[ProtoMember(2, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must be true for <b>Buff Being Replaced</b> in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		[UIHideIf("_hideBuffBeingReplacedConditions")]
		private List<AAction.Condition.AAbility> _buffBeingReplacedConditions;

		[ProtoMember(3, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must be true for <b>Buff That Is Replacing</b> in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition.AAbility> _buffThatIsReplacingConditions;

		[ProtoMember(15)]
		private Id<Ability> _owningAbilityId;

		private bool _hideBuffBeingReplacedConditions => _respondToOwningAbilityReplaced;

		private bool _owningAbilityIdSpecified => _owningAbilityId.shouldSerialize;

		private void _OnBuffReplaced(ACombatant actor, ACombatant effected, Ability buffBeingReplaced, Ability buffThatIsReplacing)
		{
			if (!_respondToOwningAbilityReplaced || !(buffBeingReplaced != _owningAbilityId))
			{
				ActionContext context = new ActionContext(actor, buffThatIsReplacing, effected);
				if ((_respondToOwningAbilityReplaced || _buffBeingReplacedConditions.IsNullOrEmpty() || _buffBeingReplacedConditions.All((AAction.Condition.AAbility c) => c.IsValid(context.SetTarget(buffBeingReplaced)))) && (_buffThatIsReplacingConditions.IsNullOrEmpty() || _buffThatIsReplacingConditions.All((AAction.Condition.AAbility c) => c.IsValid(context.SetTarget(buffThatIsReplacing)))))
				{
					_OnAvailable(actor, effected);
				}
			}
		}

		protected override void _Register(ActionContext context)
		{
			_owningAbilityId = context.ability;
			context.gameState.onBuffReplaced += _OnBuffReplaced;
		}

		protected override void _Unregister(ActionContext context)
		{
			_owningAbilityId = Id<Ability>.Null;
			context.gameState.onBuffReplaced -= _OnBuffReplaced;
		}

		public override string ToString()
		{
			return "Replaces " + (_respondToOwningAbilityReplaced ? "this" : (_buffBeingReplacedConditions.IsNullOrEmpty() ? "a" : _buffBeingReplacedConditions.ToStringSmart(" & ").SizeIfNotEmpty())) + " buff " + (_buffThatIsReplacingConditions.IsNullOrEmpty() ? "" : ("with " + _buffThatIsReplacingConditions.ToStringSmart(" & ").SizeIfNotEmpty() + " buff ")) + "on";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a buff or debuff is being removed.", category = "Ability")]
	public class BuffRemoved : Reaction
	{
		[ProtoMember(1)]
		[UIField(validateOnChange = true, tooltip = "Buff being removed must be the ability which registered this reaction in order to trigger.")]
		[DefaultValue(true)]
		private bool _respondToOwningAbilityRemoved = true;

		[ProtoMember(2, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must be true for <b>Buff Being Removed</b> in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		[UIHideIf("_hideBuffBeingRemovedConditions")]
		private List<AAction.Condition.AAbility> _buffBeingRemovedConditions;

		[ProtoMember(15)]
		private Id<Ability> _owningAbilityId;

		private bool _hideBuffBeingRemovedConditions => _respondToOwningAbilityRemoved;

		private bool _owningAbilityIdSpecified => _owningAbilityId.shouldSerialize;

		private void _OnBuffRemoved(ACombatant actor, ACombatant effected, Ability buffBeingRemoved)
		{
			if (!effected.deadOrInsuredDeath && (!_respondToOwningAbilityRemoved || !(buffBeingRemoved != _owningAbilityId)) && (_respondToOwningAbilityRemoved || _buffBeingRemovedConditions.All<AAction.Condition.AAbility>(new ActionContext(actor, null, buffBeingRemoved))))
			{
				_OnAvailable(actor, effected);
			}
		}

		protected override void _Register(ActionContext context)
		{
			_owningAbilityId = context.ability;
			context.gameState.onBuffRemoved += _OnBuffRemoved;
		}

		protected override void _Unregister(ActionContext context)
		{
			_owningAbilityId = Id<Ability>.Null;
			context.gameState.onBuffRemoved -= _OnBuffRemoved;
		}

		public override string ToString()
		{
			return "Removes " + (_respondToOwningAbilityRemoved ? "this" : (_buffBeingRemovedConditions.IsNullOrEmpty() ? "a" : _buffBeingRemovedConditions.ToStringSmart(" & ").SizeIfNotEmpty())) + " Ability from";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a trait is removed.", category = "Ability")]
	public class TraitRemoved : Reaction
	{
		[ProtoMember(1)]
		[UIField(tooltip = "Trigger right before trait is actually removed?")]
		[DefaultValue(true)]
		private bool _beginToRemove = true;

		public override bool usesTriggeredBy => false;

		private void _OnTraitRemoved(ACombatant combatant, Ability trait)
		{
			if (combatant != null && combatant.inTurnOrder && combatant.gameState.reactToTraitRemove)
			{
				_OnAvailable(null, combatant);
			}
		}

		protected override void _Register(ActionContext context)
		{
			if (_beginToRemove)
			{
				context.gameState.onTraitBeginRemove += _OnTraitRemoved;
			}
			else
			{
				context.gameState.onTraitRemoved += _OnTraitRemoved;
			}
		}

		protected override void _Unregister(ActionContext context)
		{
			if (_beginToRemove)
			{
				context.gameState.onTraitBeginRemove -= _OnTraitRemoved;
			}
			else
			{
				context.gameState.onTraitRemoved -= _OnTraitRemoved;
			}
		}

		public override string ToString()
		{
			return _beginToRemove.ToText("Trait is about to be removed", "Trait is removed") + " from";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a summon is placed into turn order.", category = "Ability")]
	public class SummonPlaced : Reaction
	{
		[ProtoMember(1, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must be true for <b>Summon Being Placed</b> in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition.AAbility> _summonBeingPlacedConditions;

		public override bool usesTriggeredOn => false;

		private void _OnSummonPlaced(ACombatant actor, Ability summonBeingPlaced)
		{
			if (_summonBeingPlacedConditions.All<AAction.Condition.AAbility>(new ActionContext(actor, summonBeingPlaced, summonBeingPlaced)))
			{
				_OnAvailable(actor, summonBeingPlaced);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onSummonPlaced += _OnSummonPlaced;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onSummonPlaced -= _OnSummonPlaced;
		}

		public override string ToString()
		{
			return "Places " + _summonBeingPlacedConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + "Summon";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a summon is removed from turn order.", category = "Ability")]
	public class SummonRemoved : Reaction
	{
		[ProtoContract(EnumPassthru = true)]
		public enum TriggerPhase
		{
			IsAboutToRemove,
			Removes
		}

		[ProtoMember(1, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must be true for <b>Summon Being Removed</b> in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition.AAbility> _summonBeingRemovedConditions;

		[ProtoMember(2)]
		[UIField]
		private TriggerPhase _triggersWhen;

		[ProtoMember(3)]
		[UIField(tooltip = "Do not trigger if summon is being removed as part of another summon being placed.")]
		private bool _ignoreIfBeingReplaced;

		public override bool usesTriggeredOn => false;

		private void _OnSummonPlaced(ACombatant actor, Ability summonBeingRemoved, bool isFinishedBeingRemoved, bool isBeingReplaced)
		{
			if (!(_ignoreIfBeingReplaced && isBeingReplaced) && _triggersWhen == (TriggerPhase)isFinishedBeingRemoved.ToInt() && _summonBeingRemovedConditions.All<AAction.Condition.AAbility>(new ActionContext(actor, summonBeingRemoved, summonBeingRemoved)))
			{
				_OnAvailable(actor, summonBeingRemoved);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onSummonRemoved += _OnSummonPlaced;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onSummonRemoved -= _OnSummonPlaced;
		}

		public override string ToString()
		{
			return EnumUtil.FriendlyName(_triggersWhen, uppercase: false) + " " + _summonBeingRemovedConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + "Summon" + _ignoreIfBeingReplaced.ToText(" which is not being replaced".SizeIfNotEmpty());
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when a summon is being replaced by another.", category = "Ability")]
	public class SummonReplaced : Reaction
	{
		[ProtoMember(1)]
		[UIField(validateOnChange = true, tooltip = "Summon being replaced must be the ability which registered this reaction in order to trigger.")]
		[DefaultValue(true)]
		private bool _respondToOwningAbilityReplaced = true;

		[ProtoMember(2, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must be true for <b>Summon Being Replaced</b> in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		[UIHideIf("_hideSummonBeingReplacedConditions")]
		private List<AAction.Condition.AAbility> _summonBeingReplacedConditions;

		[ProtoMember(3, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must be true for <b>Summon That Is Replacing</b> in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition.AAbility> _summonThatIsReplacingConditions;

		[ProtoMember(15)]
		private Id<Ability> _owningAbilityId;

		private bool _hideSummonBeingReplacedConditions => _respondToOwningAbilityReplaced;

		private bool _owningAbilityIdSpecified => _owningAbilityId.shouldSerialize;

		private void _OnSummonReplaced(ACombatant actor, Ability summonBeingReplaced, Ability summonThatIsReplacing)
		{
			if (!_respondToOwningAbilityReplaced || !(summonBeingReplaced != _owningAbilityId))
			{
				ActionContext context = new ActionContext(actor, summonThatIsReplacing, summonBeingReplaced);
				if ((_respondToOwningAbilityReplaced || _summonBeingReplacedConditions.IsNullOrEmpty() || _summonBeingReplacedConditions.All((AAction.Condition.AAbility c) => c.IsValid(context.SetTarget(summonBeingReplaced)))) && (_summonThatIsReplacingConditions.IsNullOrEmpty() || _summonThatIsReplacingConditions.All((AAction.Condition.AAbility c) => c.IsValid(context.SetTarget(summonThatIsReplacing)))))
				{
					_OnAvailable(actor, summonBeingReplaced);
				}
			}
		}

		protected override void _Register(ActionContext context)
		{
			_owningAbilityId = context.ability;
			context.gameState.onSummonReplaced += _OnSummonReplaced;
		}

		protected override void _Unregister(ActionContext context)
		{
			_owningAbilityId = Id<Ability>.Null;
			context.gameState.onSummonReplaced -= _OnSummonReplaced;
		}

		public override string ToString()
		{
			return "Replaces " + (_respondToOwningAbilityReplaced ? "this" : (_summonBeingReplacedConditions.IsNullOrEmpty() ? "a" : _summonBeingReplacedConditions.ToStringSmart(" & ").SizeIfNotEmpty())) + " summon " + (_summonThatIsReplacingConditions.IsNullOrEmpty() ? "" : ("with " + _summonThatIsReplacingConditions.ToStringSmart(" & ").SizeIfNotEmpty() + " summon ")) + "on";
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when an ability is transferred from one pile of cards to another.", category = "Ability")]
	public class AbilityPileChanged : Reaction
	{
		[ProtoMember(1)]
		[UIField(tooltip = "Valid piles which ability is transferred from in order to trigger.")]
		[DefaultValue(Ability.Piles.Draw)]
		[UIHorizontalLayout("T")]
		private Ability.Piles _from = Ability.Piles.Draw;

		[ProtoMember(2)]
		[UIField(tooltip = "Valid piles which ability is transferred to in order to trigger.")]
		[DefaultValue(Ability.Piles.Hand)]
		[UIHorizontalLayout("T")]
		private Ability.Piles _to = Ability.Piles.Hand;

		[ProtoMember(3, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must hold true for ability that was transferred in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition.AAbility> _abilityConditions;

		[ProtoMember(4, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must be true for the ability owner in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition.Actor> _ownerConditions;

		public override bool usesTriggeredBy => false;

		public override bool usesTriggeredOn => false;

		private void _OnAbilityTransfer(Ability card, Ability.Pile? oldPile, Ability.Pile? newPile)
		{
			if ((_from == (Ability.Piles)0 || _from.Contains(oldPile)) && (_to == (Ability.Piles)0 || _to.Contains(newPile)) && (_abilityConditions.IsNullOrEmpty() || _abilityConditions.All((AAction.Condition.AAbility c) => c.IsValid(new ActionContext(card.owner, card, card)))) && (_ownerConditions.IsNullOrEmpty() || _ownerConditions.All((AAction.Condition.Actor c) => c.IsValid(new ActionContext(card.owner, card, card.owner)))))
			{
				_OnAvailable(null, null);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.player.abilityDeck.onTransfer += _OnAbilityTransfer;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.player.abilityDeck.onTransfer -= _OnAbilityTransfer;
		}

		public override string ToString()
		{
			return _abilityConditions.ToStringSmart(" & ").SpaceIfNotEmpty().SizeIfNotEmpty() + "Ability is transferred from " + EnumUtil.FriendlyName(_from) + " to " + EnumUtil.FriendlyName(_to) + _ownerConditions.ToStringSmart(" & ").PreSpaceIfNotEmpty().SizeIfNotEmpty();
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggered when an ability action is ticked.", category = "Ability")]
	public class AbilityTick : Reaction
	{
		[ProtoMember(1, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must hold true for the ability that is being used in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition.AAbility> _abilityConditions;

		[ProtoMember(2, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must hold true for the action which is currently ticking in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Filter> _actionConditions;

		[ProtoMember(3, OverwriteList = true)]
		[UIField(tooltip = "Conditions that must hold true for the targets of the action that is ticking in order to trigger.")]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition> _targetConditions;

		private void _OnAbilityTick(ActionContext context, AAction action)
		{
			if (_abilityConditions.All<AAction.Condition.AAbility>(context.SetTarget(context.ability)) && _actionConditions.All(context, action) && _targetConditions.All(context))
			{
				_OnAvailable(context.actor, context.target as AEntity);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onAbilityTick += _OnAbilityTick;
			_abilityConditions.Register(context);
			_targetConditions.Register(context);
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onAbilityTick -= _OnAbilityTick;
			_abilityConditions.Unregister(context);
			_targetConditions.Unregister(context);
		}

		public override string ToString()
		{
			return _abilityConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + "Ability <b>ticks</b>" + (_actionConditions.IsNullOrEmpty() ? "" : (" " + _actionConditions.ToStringSmart(" & ").SizeIfNotEmpty() + " action")) + (_targetConditions.IsNullOrEmpty() ? " on" : (" on " + _targetConditions.ToStringSmart(" & ").SizeIfNotEmpty()));
		}
	}

	[ProtoContract]
	[UIField("Turn Order Changes", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Triggered when turn order queue changes.", category = "Turn Order")]
	public class TurnOrder : Reaction
	{
		public override bool usesTriggeredBy => false;

		public override bool usesTriggeredOn => false;

		private void _OnAdventureReorder(ATarget card, AdventureCard.Pile pile)
		{
			_OnAdventureTransfer(card, pile, pile);
		}

		private void _OnAdventureTransfer(ATarget card, AdventureCard.Pile? oldPile, AdventureCard.Pile? newPile)
		{
			if (oldPile == AdventureCard.Pile.TurnOrder || newPile == AdventureCard.Pile.TurnOrder)
			{
				_OnAvailable(null, null);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.adventureDeck.onTransfer += _OnAdventureTransfer;
			context.gameState.adventureDeck.onReorder += _OnAdventureReorder;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.adventureDeck.onTransfer -= _OnAdventureTransfer;
			context.gameState.adventureDeck.onReorder -= _OnAdventureReorder;
		}

		public override string ToString()
		{
			return "Turn Order Changes";
		}
	}

	[ProtoContract]
	[UIField("Tapped", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Triggered when an entity taps or untaps.", category = "Turn Order")]
	public class Tap : Reaction
	{
		[ProtoMember(1)]
		[UIField(validateOnChange = true)]
		[DefaultValue(true)]
		private bool _triggerOnTap = true;

		[ProtoMember(2)]
		[UIField(excludedValuesMethod = "_ExcludeMustBeActiveTap")]
		[DefaultValue(true)]
		private bool _mustBeActiveTap = true;

		public override bool usesTriggeredBy => _mustBeActiveTap;

		private void _OnEntityTap(AEntity entity, bool tapped, AEntity tappedBy)
		{
			if (_triggerOnTap == tapped && (!tapped || !_mustBeActiveTap || tappedBy != null))
			{
				_OnAvailable(tappedBy, entity);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onEntityTap += _OnEntityTap;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onEntityTap -= _OnEntityTap;
		}

		public override string ToString()
		{
			return ((!_triggerOnTap) ? "untapping" : (_mustBeActiveTap ? "taps" : "tapping")) ?? "";
		}

		private bool _ExcludeMustBeActiveTap(bool mustBeActiveTapped)
		{
			if (mustBeActiveTapped)
			{
				return !_triggerOnTap;
			}
			return false;
		}
	}

	[ProtoContract]
	[UIField(category = "Top Deck")]
	public class TopDeckFinishedDrawing : Reaction
	{
		[ProtoMember(1)]
		[UIField]
		[DefaultValue(TopDeckResult.Failure)]
		private TopDeckResult _result = TopDeckResult.Failure;

		public override bool usesTriggeredOn => false;

		private void _OnTopDeckFinishedDrawing(ActionContext context, TopDeckResult result)
		{
			if (_result == TopDeckResult.None || _result == result)
			{
				_OnAvailable(context.actor, null);
			}
		}

		public override AbilityPreventedBy? GetAbilityPreventedBy(ReactionEntity triggeredBy, ReactionEntity triggeredOn)
		{
			if (_result != TopDeckResult.Failure)
			{
				return base.GetAbilityPreventedBy(triggeredBy, triggeredOn);
			}
			return AbilityPreventedBy.ReactionFailingTopDeck;
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onTopDeckFinishedDrawing += _OnTopDeckFinishedDrawing;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onTopDeckFinishedDrawing -= _OnTopDeckFinishedDrawing;
		}

		public override string ToString()
		{
			return "Top Deck Has Finished Drawing" + ((_result != TopDeckResult.None) ? (" In " + EnumUtil.FriendlyName(_result)) : "");
		}
	}

	[ProtoContract]
	[UIField(tooltip = "Triggers in response to a dynamic number changing value.", category = "Dynamic Number")]
	public class DynamicNumberRange : Reaction
	{
		[ProtoContract(EnumPassthru = true)]
		public enum TransitionType
		{
			Becomes,
			IsNoLonger
		}

		private static readonly RangeByte RANGE = new RangeByte(1, 20, 0, 20, 0, 0);

		[ProtoMember(1)]
		[UIField]
		[UIDeepValueChange]
		private AAction.DynamicNumber _value;

		[ProtoMember(2)]
		[UIField]
		[UIHorizontalLayout("R", expandHeight = false, preferredWidth = 1f, flexibleWidth = 1f)]
		private TransitionType _whenRange;

		[ProtoMember(3)]
		[UIField]
		[UIHorizontalLayout("R", preferredWidth = 1f, flexibleWidth = 3f)]
		private RangeByte _range = RANGE;

		private int _previousValue;

		private AppliedAction _appliedAction;

		private bool _rangeSpecified => _range != RANGE;

		private void _OnValueChange()
		{
			int value = _value.GetValue(_appliedAction.context);
			if (_whenRange switch
			{
				TransitionType.Becomes => !_range.InRangeSmart(_previousValue) && _range.InRangeSmart(value), 
				TransitionType.IsNoLonger => _range.InRangeSmart(_previousValue) && !_range.InRangeSmart(value), 
				_ => throw new ArgumentOutOfRangeException(), 
			})
			{
				_OnAvailable(_value.actor, _value.target as AEntity, value);
			}
			_previousValue = value;
		}

		protected override void _Register(ActionContext context)
		{
			_value.Register(_appliedAction ?? (_appliedAction = new AppliedAction(context, new TargetCombatantAction())));
			_appliedAction.onReapply += _OnValueChange;
			_previousValue = _value.GetValue(context);
		}

		protected override void _Unregister(ActionContext context)
		{
			_value.Unregister();
			_appliedAction.onReapply -= _OnValueChange;
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2}", _value, EnumUtil.FriendlyName(_whenRange, uppercase: false), _range.ToRangeString(RANGE, "", 50, showDefaultValue: true));
		}
	}

	[ProtoContract]
	[UIField]
	[UICategory("Misc")]
	public class DeckShuffled : Reaction
	{
		[ProtoContract(EnumPassthru = true)]
		[Flags]
		public enum DeckFlags
		{
			AbilityDeck = 1,
			ResourceDeck = 2,
			EnemyResourceDeck = 4
		}

		private const DeckFlags DEFAULT = DeckFlags.AbilityDeck | DeckFlags.ResourceDeck;

		[ProtoMember(1)]
		[UIField]
		[DefaultValue(DeckFlags.AbilityDeck | DeckFlags.ResourceDeck)]
		private DeckFlags _decksToReactTo = DeckFlags.AbilityDeck | DeckFlags.ResourceDeck;

		public override bool usesTriggeredBy => false;

		public override bool usesTriggeredOn => false;

		private void _OnDeckShuffled(GameState state, IdDeckBase deck)
		{
			if (deck == state.abilityDeck && EnumUtil.HasFlag(_decksToReactTo, DeckFlags.AbilityDeck))
			{
				_OnAvailable(state.player, state.player);
			}
			else if (deck == state.playerResourceDeck && EnumUtil.HasFlag(_decksToReactTo, DeckFlags.ResourceDeck))
			{
				_OnAvailable(state.player, state.player);
			}
			else if (deck == state.enemyResourceDeck && EnumUtil.HasFlag(_decksToReactTo, DeckFlags.EnemyResourceDeck))
			{
				_OnAvailable(state.player, state.player);
			}
		}

		protected override void _Register(ActionContext context)
		{
			context.gameState.onDeckShuffled += _OnDeckShuffled;
		}

		protected override void _Unregister(ActionContext context)
		{
			context.gameState.onDeckShuffled -= _OnDeckShuffled;
		}

		public override string ToString()
		{
			return EnumUtil.FriendlyName(_decksToReactTo).SizeIfNotEmpty() + " Shuffled";
		}
	}

	[ProtoContract]
	[UIField]
	[UICategory("Misc")]
	public class And : Reaction
	{
		[ProtoMember(1, OverwriteList = true)]
		[UIField]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<Reaction> _reactions;

		[ProtoMember(2)]
		[UIField(tooltip = "Each reaction in list waits for previous reactions in list to trigger before listening to their triggers.")]
		private bool _inOrder;

		[ProtoMember(3)]
		private int _triggeredIndicesBits;

		public override bool usesTriggeredOn => _reactions?.Any((Reaction r) => r.usesTriggeredOn) ?? false;

		public override bool usesTriggeredBy => _reactions?.Any((Reaction r) => r.usesTriggeredBy) ?? false;

		private void _OnReactionTrigger(ReactionContext context)
		{
			int num = _reactions.IndexOf(context.reaction);
			if (!_inOrder || _triggeredIndicesBits == (1 << num) - 1)
			{
				_triggeredIndicesBits |= 1 << num;
			}
			if (_triggeredIndicesBits == (1 << _reactions.Count) - 1)
			{
				_triggeredIndicesBits = 0;
				_OnAvailable(context.triggeredBy, context.triggeredOn, context.capturedValue);
			}
		}

		public override IEnumerable<AbilityKeyword> GetKeywords()
		{
			if (_reactions.IsNullOrEmpty())
			{
				yield break;
			}
			foreach (Reaction reaction in _reactions)
			{
				foreach (AbilityKeyword keyword in reaction.GetKeywords())
				{
					yield return keyword;
				}
			}
		}

		public override AbilityPreventedBy? GetAbilityPreventedBy(ReactionEntity triggeredBy, ReactionEntity triggeredOn)
		{
			if (!_reactions.IsNullOrEmpty())
			{
				foreach (Reaction reaction in _reactions)
				{
					AbilityPreventedBy? abilityPreventedBy = reaction.GetAbilityPreventedBy(triggeredBy, triggeredOn);
					if (abilityPreventedBy.HasValue)
					{
						return abilityPreventedBy.GetValueOrDefault();
					}
				}
			}
			return null;
		}

		protected override void _Register(ActionContext context)
		{
			if (_reactions == null)
			{
				return;
			}
			foreach (Reaction reaction in _reactions)
			{
				reaction._Register(context);
				reaction.onTrigger = (Action<ReactionContext>)Delegate.Combine(reaction.onTrigger, new Action<ReactionContext>(_OnReactionTrigger));
			}
		}

		protected override void _Unregister(ActionContext context)
		{
			if (_reactions == null)
			{
				return;
			}
			foreach (Reaction reaction in _reactions)
			{
				reaction._Unregister(context);
				reaction.onTrigger = (Action<ReactionContext>)Delegate.Remove(reaction.onTrigger, new Action<ReactionContext>(_OnReactionTrigger));
			}
		}

		public override string ToString()
		{
			return _reactions.ToStringSmart(_inOrder.ToText(" <b>then</b> ", " & "));
		}
	}

	public Action<ReactionContext> onTrigger;

	public virtual bool usesTriggeredBy => true;

	public virtual bool usesTriggeredOn => true;

	protected void _OnAvailable(AEntity triggeredBy, AEntity triggeredOn, int capturedValue = 0)
	{
		onTrigger?.Invoke(new ReactionContext(this, triggeredBy, triggeredOn, capturedValue));
	}

	public virtual AbilityPreventedBy? GetAbilityPreventedBy(ReactionEntity triggeredBy, ReactionEntity triggeredOn)
	{
		return null;
	}

	protected abstract void _Register(ActionContext context);

	protected abstract void _Unregister(ActionContext context);

	protected virtual void _OnValid(ActionContext context)
	{
	}

	public virtual IEnumerable<AbilityKeyword> GetKeywords()
	{
		yield break;
	}

	public void Register(ActionContext context)
	{
		_Register(context);
	}

	public void Unregister(ActionContext context)
	{
		_Unregister(context);
	}

	public bool OnValid(ActionContext context)
	{
		_OnValid(context);
		return true;
	}
}
