using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField("Action", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
[ProtoInclude(10, typeof(ACombatantAction))]
[ProtoInclude(11, typeof(APlayerAction))]
[ProtoInclude(13, typeof(AResourceAction))]
[ProtoInclude(14, typeof(AAbilityAction))]
[ProtoInclude(15, typeof(TopDeckAction))]
[ProtoInclude(16, typeof(RemoveOwningAbilityAction))]
[ProtoInclude(17, typeof(TickAction))]
[ProtoInclude(18, typeof(ASpaceAction))]
[ProtoInclude(19, typeof(RemoveSummonAction))]
[ProtoInclude(20, typeof(TargetStoneAction))]
[ProtoInclude(21, typeof(CancelAbilityAction))]
[ProtoInclude(22, typeof(AddEnemyAction))]
[ProtoInclude(23, typeof(WaitAction))]
public abstract class AAction
{
	[ProtoContract]
	[UIField]
	[ProtoInclude(10, typeof(Actor))]
	[ProtoInclude(11, typeof(AResource))]
	[ProtoInclude(12, typeof(AAbility))]
	public abstract class Condition
	{
		[ProtoContract]
		[UIField]
		[ProtoInclude(10, typeof(Combatant))]
		[ProtoInclude(11, typeof(APlayer))]
		public abstract class Actor : Condition
		{
		}

		[ProtoContract]
		[UIField]
		[ProtoInclude(4, typeof(Self))]
		[ProtoInclude(5, typeof(Positional))]
		[ProtoInclude(6, typeof(DynamicNumberCompareCombatant))]
		[ProtoInclude(7, typeof(DynamicNumberCheckCombatant))]
		[ProtoInclude(8, typeof(HP))]
		[ProtoInclude(9, typeof(HPCompare))]
		[ProtoInclude(10, typeof(HPMissing))]
		[ProtoInclude(11, typeof(Shield))]
		[ProtoInclude(12, typeof(Tapped))]
		[ProtoInclude(13, typeof(Or))]
		[ProtoInclude(14, typeof(Targets))]
		[ProtoInclude(15, typeof(Combat))]
		[ProtoInclude(16, typeof(HasTrait))]
		[ProtoInclude(17, typeof(ActiveTopDeckResult))]
		[ProtoInclude(18, typeof(Invert))]
		[ProtoInclude(19, typeof(EnemyDataReference))]
		[ProtoInclude(20, typeof(FactionCondition))]
		[ProtoInclude(21, typeof(CombatHandCondition))]
		[ProtoInclude(22, typeof(BuffCondition))]
		[ProtoInclude(23, typeof(PositionRelativeToSummon))]
		[ProtoInclude(24, typeof(MissingTrait))]
		[ProtoInclude(25, typeof(HalfHealth))]
		[ProtoInclude(26, typeof(ShouldTick))]
		[ProtoInclude(27, typeof(CanTopDeck))]
		[ProtoInclude(28, typeof(Encounter))]
		[ProtoInclude(29, typeof(Alive))]
		public abstract class Combatant : Actor
		{
			[ProtoContract]
			[UIField]
			public class HP : Combatant
			{
				private static readonly RangeByte DEFAULT = new RangeByte(0, 3, 0, 20, 0, 0);

				[ProtoMember(1)]
				[UIField]
				private RangeByte _range = DEFAULT;

				private bool _rangeSpecified => _range != DEFAULT;

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					return _range.InRangeSmart((int)target.HP);
				}

				public override string ToString()
				{
					return "If HP " + _range.ToRangeString(null, "", 50);
				}

				private void _OnHPChange(ACombatant combatant, int previousHP, int HP)
				{
					if (onDirty != null && (_range.InRangeSmart(previousHP) ^ _IsValidTarget(null, combatant)))
					{
						onDirty(combatant);
					}
				}

				public override void Register(ActionContext context)
				{
					context.gameState.onHPChange += _OnHPChange;
				}

				public override void Unregister(ActionContext context)
				{
					context.gameState.onHPChange -= _OnHPChange;
				}
			}

			[ProtoContract]
			[UIField("HP Compare", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
			public class HPCompare : Combatant
			{
				[ProtoMember(1)]
				[UIField]
				private FlagCheckType _comparison;

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					if (actor is ACombatant aCombatant)
					{
						return _comparison.Check(aCombatant.HP, target.HP);
					}
					return false;
				}

				public override string ToString()
				{
					return "If owner's HP is " + EnumUtil.FriendlyName(_comparison) + " target's HP";
				}

				private void _OnHPChange(ACombatant combatant, int previousHP, int HP)
				{
					onDirty?.Invoke(combatant);
				}

				public override void Register(ActionContext context)
				{
					context.gameState.onHPChange += _OnHPChange;
				}

				public override void Unregister(ActionContext context)
				{
					context.gameState.onHPChange -= _OnHPChange;
				}
			}

			[ProtoContract]
			[UIField("HP Missing", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
			public class HPMissing : Combatant
			{
				private static readonly RangeByte DEFAULT = new RangeByte(0, 0, 0, 20, 0, 0);

				[ProtoMember(1)]
				[UIField]
				private RangeByte _range = DEFAULT;

				private bool _rangeSpecified => _range != DEFAULT;

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					return _range.InRangeSmart(target.HPMissing);
				}

				public override string ToString()
				{
					return "If missing " + _range.ToRangeString(null, "", 50).Trim() + " HP";
				}

				private void _OnHPChange(ACombatant combatant, int previousHP, int HP)
				{
					if (onDirty != null && (_range.InRangeSmart(Math.Max(0, (int)combatant.stats.health - previousHP)) ^ _IsValidTarget(null, combatant)))
					{
						onDirty(combatant);
					}
				}

				public override void Register(ActionContext context)
				{
					context.gameState.onHPChange += _OnHPChange;
				}

				public override void Unregister(ActionContext context)
				{
					context.gameState.onHPChange -= _OnHPChange;
				}
			}

			[ProtoContract]
			[UIField]
			public class HalfHealth : Combatant
			{
				[ProtoMember(1)]
				[UIField]
				private FlagCheckType _comparison;

				private void _OnHPChange(ACombatant combatant, int previousHP, int hp)
				{
					if (onDirty != null && (_IsValidHP(combatant, previousHP) ^ _IsValidHP(combatant, hp)))
					{
						onDirty(combatant);
					}
				}

				private bool _IsValidHP(ACombatant combatant, int hp)
				{
					return _comparison.Check(hp, ((int)combatant.stats.health + 1) / 2);
				}

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					return _IsValidHP(target, target.HP);
				}

				public override void Register(ActionContext context)
				{
					context.gameState.onHPChange += _OnHPChange;
				}

				public override void Unregister(ActionContext context)
				{
					context.gameState.onHPChange -= _OnHPChange;
				}

				public override IEnumerable<AbilityKeyword> GetKeywords()
				{
					yield return AbilityKeyword.HalfHealth;
				}

				public override string ToString()
				{
					return "If " + _comparison.GetText() + " Half Health";
				}
			}

			[ProtoContract]
			[UIField]
			public class Shield : Combatant
			{
				private static readonly RangeByte DEFAULT = new RangeByte(1, 20, 0, 20, 0, 0);

				[ProtoMember(1)]
				[UIField]
				private RangeByte _range = DEFAULT;

				private bool _rangeSpecified => _range != DEFAULT;

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					return _range.InRangeSmart((int)target.shield);
				}

				public override string ToString()
				{
					return "If " + _range.ToRangeString(null, "", 50).AppendSpace() + "Shield";
				}

				private void _OnShieldChange(ACombatant combatant, int previousShield, int shield)
				{
					if (onDirty != null && (_range.InRangeSmart(previousShield) ^ _IsValidTarget(null, combatant)))
					{
						onDirty(combatant);
					}
				}

				public override void Register(ActionContext context)
				{
					context.gameState.onShieldChange += _OnShieldChange;
				}

				public override void Unregister(ActionContext context)
				{
					context.gameState.onShieldChange -= _OnShieldChange;
				}
			}

			[ProtoContract]
			[UIField]
			public class Tapped : Combatant
			{
				[ProtoMember(1)]
				[UIField]
				[DefaultValue(true)]
				private bool _isTapped = true;

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					return (bool)target.tapped == _isTapped;
				}

				public override string ToString()
				{
					if (!_isTapped)
					{
						return "If not tapped";
					}
					return "If tapped";
				}

				private void _OnEntityTap(AEntity entity, bool tapped, AEntity tappedBy)
				{
					onDirty?.Invoke(entity);
				}

				public override void Register(ActionContext context)
				{
					context.gameState.onEntityTap += _OnEntityTap;
				}

				public override void Unregister(ActionContext context)
				{
					context.gameState.onEntityTap -= _OnEntityTap;
				}

				public override IEnumerable<AbilityKeyword> GetKeywords()
				{
					yield return AbilityKeyword.IsTapped;
				}
			}

			[ProtoContract]
			[UIField]
			public class Positional : Combatant
			{
				[ProtoContract(EnumPassthru = true)]
				public enum Position
				{
					AdjacentTo,
					NonAdjacentTo,
					InFrontOf,
					Behind
				}

				[ProtoMember(1)]
				[UIField]
				private Position _position;

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					int num = target.gameState.GetTurnOrder(target) - actor.gameState.GetTurnOrder(actor);
					return _position switch
					{
						Position.AdjacentTo => Math.Abs(num) == 1, 
						Position.NonAdjacentTo => Math.Abs(num) != 1, 
						Position.InFrontOf => num > 0, 
						Position.Behind => num < 0, 
						_ => false, 
					};
				}

				public override string ToString()
				{
					return "If Owner is <b>" + EnumUtil.FriendlyName(_position) + "</b>";
				}

				private void _OnAdventureTransfer(ATarget card, AdventureCard.Pile? oldPile, AdventureCard.Pile? newPile)
				{
					if (onDirty != null && (oldPile == AdventureCard.Pile.TurnOrder || newPile == AdventureCard.Pile.TurnOrder))
					{
						onDirty(card);
					}
				}

				private void _OnAdventureReordered(ATarget card, AdventureCard.Pile pile)
				{
					if (pile == AdventureCard.Pile.TurnOrder)
					{
						onDirty?.Invoke(card);
					}
				}

				public override void Register(ActionContext context)
				{
					context.gameState.adventureDeck.onTransfer += _OnAdventureTransfer;
					context.gameState.adventureDeck.onReorder += _OnAdventureReordered;
				}

				public override void Unregister(ActionContext context)
				{
					context.gameState.adventureDeck.onTransfer -= _OnAdventureTransfer;
					context.gameState.adventureDeck.onReorder -= _OnAdventureReordered;
				}
			}

			[ProtoContract]
			[UIField]
			public class PositionRelativeToSummon : Combatant
			{
				[ProtoContract(EnumPassthru = true)]
				public enum Position
				{
					AdjacentTo,
					LeftOf,
					RightOf,
					AcrossFrom
				}

				[ProtoMember(1)]
				[UIField]
				private Position _position;

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					GameState gameState = actor.gameState;
					Ability activeSummon = gameState.activeSummon;
					if (activeSummon == null)
					{
						return false;
					}
					int turnOrder = gameState.GetTurnOrder(activeSummon);
					int turnOrder2 = gameState.GetTurnOrder(target);
					return _position switch
					{
						Position.AdjacentTo => Math.Abs(turnOrder2 - turnOrder) == 1, 
						Position.LeftOf => turnOrder2 < turnOrder, 
						Position.RightOf => turnOrder2 > turnOrder, 
						Position.AcrossFrom => Math.Sign(turnOrder - gameState.GetTurnOrder(actor)) == Math.Sign(turnOrder2 - turnOrder), 
						_ => throw new ArgumentOutOfRangeException(), 
					};
				}

				public override string ToString()
				{
					return "If " + EnumUtil.FriendlyName(_position, uppercase: false) + " summon";
				}

				private void _OnAdventureTransfer(ATarget card, AdventureCard.Pile? oldPile, AdventureCard.Pile? newPile)
				{
					if (onDirty != null && (oldPile == AdventureCard.Pile.TurnOrder || newPile == AdventureCard.Pile.TurnOrder))
					{
						onDirty(card);
					}
				}

				private void _OnAdventureReordered(ATarget card, AdventureCard.Pile pile)
				{
					if (pile == AdventureCard.Pile.TurnOrder)
					{
						onDirty?.Invoke(card);
					}
				}

				public override void Register(ActionContext context)
				{
					context.gameState.adventureDeck.onTransfer += _OnAdventureTransfer;
					context.gameState.adventureDeck.onReorder += _OnAdventureReordered;
				}

				public override void Unregister(ActionContext context)
				{
					context.gameState.adventureDeck.onTransfer -= _OnAdventureTransfer;
					context.gameState.adventureDeck.onReorder -= _OnAdventureReordered;
				}
			}

			[ProtoContract]
			[UIField("Or (Combatant)", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
			public new class Or : Combatant
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Hide)]
				private Or<Combatant> _conditions;

				public override bool IsValid(ActionContext context)
				{
					return _conditions.IsValid(context);
				}

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					return true;
				}

				public override string ToString()
				{
					return "[" + _conditions?.ToString() + "]";
				}

				private void _OnDirty(ATarget target)
				{
					onDirty?.Invoke(target);
				}

				public override void Register(ActionContext context)
				{
					_conditions.conditions.Register(context, _OnDirty);
				}

				public override void Unregister(ActionContext context)
				{
					_conditions.conditions.Unregister(context, _OnDirty);
				}
			}

			[ProtoContract]
			[UIField("Number of Targets (Combatant)", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
			public new class Targets : Combatant
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Open)]
				[UIDeepValueChange]
				private Targets<Combatant> _targets;

				public override bool IsValid(ActionContext context)
				{
					return _targets.IsValid(context);
				}

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					return true;
				}

				public override string ToString()
				{
					return _targets?.ToString() ?? "";
				}
			}

			[ProtoContract]
			[UIField("Dynamic Number Check (Combatant)", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
			public class DynamicNumberCheckCombatant : Combatant
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Hide)]
				private DynamicNumberCheck _dynamicNumberCheck;

				[ProtoMember(2)]
				[UIField]
				private bool _allowSummon;

				protected override AbilityPreventedBy _abilityPreventedBy => _dynamicNumberCheck?.GetPreventedBy() ?? base._abilityPreventedBy;

				public override bool IsValid(ActionContext context)
				{
					if (context.target is ACombatant || (_allowSummon && context.target is Ability ability && ability.isSummon))
					{
						return _dynamicNumberCheck.IsValid(context);
					}
					return false;
				}

				public override string ToString()
				{
					return _dynamicNumberCheck?.ToString() + _allowSummon.ToText("(Allow Summon)".SizeIfNotEmpty());
				}
			}

			[ProtoContract]
			[UIField("Dynamic Number Compare (Combatant)", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
			public class DynamicNumberCompareCombatant : Combatant
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Hide)]
				private DynamicNumberCompare _dynamicNumberCompare;

				public override bool IsValid(ActionContext context)
				{
					if (context.target is ACombatant)
					{
						return _dynamicNumberCompare.IsValid(context);
					}
					return false;
				}

				public override string ToString()
				{
					return _dynamicNumberCompare?.ToString() ?? "";
				}
			}

			[ProtoContract]
			[UIField("In Combat", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Allows filtering targets down to those who are in combat.")]
			public class Combat : Combatant
			{
				[ProtoMember(1)]
				[UIField]
				private CombatTypeFilter _combatType;

				[ProtoMember(2)]
				[UIField(validateOnChange = true)]
				private AttackResultTypes? _validCombatResults;

				[ProtoMember(3)]
				[UIField]
				[UIHideIf("_hideValidPhase")]
				private ActiveCombat.Phase? _validPhase;

				[ProtoMember(4)]
				[UIField(tooltip = "Return false if active combat has been canceled.")]
				[UIHideIf("_hideValidPhase")]
				private bool _ignoreCanceledCombat;

				private bool _hideValidPhase => !_validCombatResults.HasValue;

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					ActiveCombat activeCombat = target.gameState.activeCombat;
					if (activeCombat != null && activeCombat.attackHasBeenLaunched)
					{
						CombatType? activeCombatType = target.activeCombatType;
						if (activeCombatType.HasValue)
						{
							CombatType valueOrDefault = activeCombatType.GetValueOrDefault();
							if ((!_ignoreCanceledCombat || !activeCombat.canceled) && (_combatType == CombatTypeFilter.Any || valueOrDefault == (CombatType)_combatType))
							{
								if (!_validCombatResults.HasValue)
								{
									goto IL_0094;
								}
								AttackResultType? resultFor = activeCombat.GetResultFor(target);
								if (resultFor.HasValue)
								{
									AttackResultType valueOrDefault2 = resultFor.GetValueOrDefault();
									if (EnumUtil.HasFlagConvert(_validCombatResults.Value, valueOrDefault2))
									{
										goto IL_0094;
									}
								}
							}
						}
					}
					return false;
					IL_0094:
					if (_validPhase.HasValue)
					{
						return _validPhase == activeCombat.phase;
					}
					return true;
				}

				public override string ToString()
				{
					return ((_combatType == CombatTypeFilter.Any) ? "If In Combat" : ((_combatType == CombatTypeFilter.Attack) ? "If Attacking" : "If Defending")) + (_validCombatResults.HasValue ? (" In " + _ignoreCanceledCombat.ToText("Uncanceled ") + EnumUtil.FriendlyNameSpacedAfter(_validPhase) + "(" + EnumUtil.FriendlyName(_validCombatResults) + ")") : "");
				}
			}

			[ProtoContract]
			[UIField]
			public class Self : Combatant
			{
				[ProtoMember(1)]
				[UIField]
				private bool _self;

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					return _self ^ (actor != target);
				}

				public override string ToString()
				{
					if (!_self)
					{
						return "If not self";
					}
					return "If self";
				}
			}

			[ProtoContract]
			[UIField]
			public class HasTrait : Combatant
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Open, excludedValuesMethod = "_ExcludeTraits")]
				private DataRef<AbilityData> _trait;

				private bool _traitSpecified => _trait.ShouldSerialize();

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					return target.HasTrait(_trait);
				}

				public override string ToString()
				{
					return "If has " + (_trait ? _trait.friendlyName : "").BoldIfNotEmpty().SpaceIfNotEmpty() + "trait";
				}

				private void _OnTraitChanged(ACombatant combatant, Ability trait)
				{
					if (onDirty != null && (!_trait || ContentRef.Equal(_trait, trait.dataRef)))
					{
						onDirty(combatant);
					}
				}

				public override void Register(ActionContext context)
				{
					context.gameState.onTraitAdded += _OnTraitChanged;
					context.gameState.onTraitRemoved += _OnTraitChanged;
				}

				public override void Unregister(ActionContext context)
				{
					context.gameState.onTraitAdded -= _OnTraitChanged;
					context.gameState.onTraitRemoved -= _OnTraitChanged;
				}

				protected bool _ExcludeTraits(DataRef<AbilityData> abilityRef)
				{
					return !abilityRef.data.type.IsTrait();
				}
			}

			[ProtoContract]
			[UIField]
			public class MissingTrait : Combatant
			{
				[ProtoMember(1)]
				[UIField(tooltip = "Whether or not missing permanent traits should trigger condition.")]
				private bool _includePermanents;

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					return target.IsMissingBaseTrait(_includePermanents);
				}

				public override string ToString()
				{
					return "If missing trait" + _includePermanents.ToText(" (include permanent)".SizeIfNotEmpty());
				}

				private void _OnTraitChanged(ACombatant combatant, Ability trait)
				{
					onDirty?.Invoke(combatant);
				}

				public override void Register(ActionContext context)
				{
					context.gameState.onTraitAdded += _OnTraitChanged;
					context.gameState.onTraitRemoved += _OnTraitChanged;
				}

				public override void Unregister(ActionContext context)
				{
					context.gameState.onTraitAdded -= _OnTraitChanged;
					context.gameState.onTraitRemoved -= _OnTraitChanged;
				}
			}

			[ProtoContract]
			[UIField]
			public class ActiveTopDeckResult : Combatant
			{
				[ProtoMember(1)]
				[UIField]
				[DefaultValue(TopDeckResult.Failure)]
				private TopDeckResult _result = TopDeckResult.Failure;

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					if (_result == TopDeckResult.None)
					{
						return !target.activeTopDeckResult.HasValue;
					}
					return _result == target.activeTopDeckResult;
				}

				public override string ToString()
				{
					if (_result == TopDeckResult.None)
					{
						return "If Not Top Decking";
					}
					return "If Top Decking In " + EnumUtil.FriendlyName(_result);
				}
			}

			[ProtoContract]
			[UIField("Invert (Combatant)", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Allows inverting any other combatant condition.")]
			public class Invert : Combatant
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Hide)]
				[UIDeepValueChange]
				private Combatant _condition;

				public override Action<ATarget> onDirty
				{
					get
					{
						return _condition.onDirty;
					}
					set
					{
						_condition.onDirty = value;
					}
				}

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					return !_condition._IsValidTarget(actor, target);
				}

				public override string ToString()
				{
					return "!" + _condition;
				}

				public override void Register(ActionContext context)
				{
					_condition.Register(context);
				}

				public override void Unregister(ActionContext context)
				{
					_condition.Unregister(context);
				}
			}

			[ProtoContract]
			[UIField(tooltip = "Allows filtering by enemy data references.")]
			public class EnemyDataReference : Combatant
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Open)]
				private DataRef<EnemyData> _enemyDataReference;

				private bool _enemyDataReferenceSpecified => _enemyDataReference.ShouldSerialize();

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					if (target is Enemy enemy)
					{
						if ((bool)_enemyDataReference)
						{
							return ContentRef.Equal(_enemyDataReference, enemy.enemyDataRef);
						}
						return true;
					}
					return false;
				}

				public override string ToString()
				{
					return "If is " + (_enemyDataReference ? (_enemyDataReference.friendlyName + " ") : "") + "enemy";
				}
			}

			[ProtoContract]
			[UIField("Faction", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
			public class FactionCondition : Combatant
			{
				[ProtoMember(1)]
				[UIField]
				[DefaultValue(Faction.Enemy)]
				private Faction _faction = Faction.Enemy;

				public override bool IsValid(ActionContext context)
				{
					if (context.target is AEntity aEntity)
					{
						return aEntity.faction == _faction;
					}
					return false;
				}

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					return true;
				}

				public override string ToString()
				{
					return "If is " + EnumUtil.FriendlyName(_faction) + " faction";
				}
			}

			[ProtoContract]
			[UIField("Combat Hand Type", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Checks if a certain type of poker hand is being used by an active combatant.")]
			public class CombatHandCondition : Combatant
			{
				[ProtoMember(1)]
				[UIField]
				[DefaultValue(CombatTypes.Attack)]
				private CombatTypes _combatTypes = CombatTypes.Attack;

				[ProtoMember(2)]
				[UIField]
				private PokerHandTypes _hands;

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					CombatType? combatType = actor.gameState.activeCombat?.GetCombatType(target);
					if (combatType.HasValue)
					{
						CombatType valueOrDefault = combatType.GetValueOrDefault();
						if (EnumUtil.HasFlagConvert(_combatTypes, valueOrDefault))
						{
							(PoolKeepItemListHandle<ResourceCard>, PokerHandType) combatHand = actor.gameState.activeCombat.GetCombatHand(target);
							using (combatHand.Item1)
							{
								return EnumUtil.HasFlagConvert(_hands, combatHand.Item2);
							}
						}
					}
					return false;
				}

				public override string ToString()
				{
					return "If " + _combatTypes.GetVerb() + " with " + EnumUtil.FriendlyName(_hands);
				}
			}

			[ProtoContract]
			[UIField("Has Buff", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Check if target has a certain buff or debuff applied on them.")]
			public class BuffCondition : Combatant
			{
				[ProtoMember(3)]
				[UIField(validateOnChange = true, tooltip = "Checks if buff is THIS ability.")]
				[UIHorizontalLayout("A", flexibleWidth = 0f)]
				private bool _this;

				[ProtoMember(1)]
				[UIField]
				[DefaultValue(AppliedPile.Debuff)]
				[UIHideIf("_hideAbilityConditions")]
				[UIHorizontalLayout("A", flexibleWidth = 999f)]
				private AppliedPile _type = AppliedPile.Debuff;

				[ProtoMember(2, OverwriteList = true)]
				[UIField(tooltip = "Conditions that must be true of buff.")]
				[UIFieldCollectionItem]
				[UIDeepValueChange]
				[UIHideIf("_hideAbilityConditions")]
				private List<AAbility> _abilityConditions;

				private Id<Ability> _thisAbilityId;

				private bool _hideAbilityConditions => _this;

				private bool _abilityConditionsSpecified => !_hideAbilityConditions;

				public override void Register(ActionContext context)
				{
					_thisAbilityId = (_this ? context.ability.ToId<Ability>() : Id<Ability>.Null);
				}

				public override void Unregister(ActionContext context)
				{
					_thisAbilityId = Id<Ability>.Null;
				}

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					Ability ability = target[_this ? _thisAbilityId.value.data.type.GetAppliedPile() : _type];
					if (ability != null)
					{
						if (!_this)
						{
							return _abilityConditions.All<AAbility>(new ActionContext(actor, ability, ability));
						}
						return ability == _thisAbilityId;
					}
					return false;
				}

				public override string ToString()
				{
					if (!_this)
					{
						return "If has " + _abilityConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + EnumUtil.FriendlyName(_type, uppercase: false);
					}
					return "If has this buff";
				}
			}

			[ProtoContract]
			[UIField(tooltip = "Uses underlying action's ShouldTick logic.\n<b>Should only be used for custom tick targeting conditions.</b>")]
			public class ShouldTick : Combatant
			{
				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					if (target.gameState.stack.activeStep is GameStepActionTarget gameStepActionTarget)
					{
						return gameStepActionTarget.action?.ShouldTick(new ActionContext(actor, null, target)) ?? false;
					}
					return false;
				}

				public override string ToString()
				{
					return "If Action Should Tick";
				}
			}

			[ProtoContract]
			[UIField(tooltip = "Check if combatant has enough cards in deck in order to top deck.")]
			public class CanTopDeck : Combatant
			{
				public override bool IsValid(ActionContext context)
				{
					if (context.target is ACombatant)
					{
						Ability ability = context.ability;
						if (ability != null)
						{
							return ability.data.actions.OfType<TopDeckAction>().Any((TopDeckAction topDeckAction) => topDeckAction.topDeck.ShouldAct(context));
						}
					}
					return false;
				}

				public override string ToString()
				{
					return "If Can Top Deck";
				}
			}

			[ProtoContract]
			[UIField(tooltip = "Check if encounter is active.")]
			public class Encounter : Combatant
			{
				[ProtoMember(1)]
				[UIField]
				private EncounterState _state;

				public override bool IsValid(ActionContext context)
				{
					return context.gameState.GetEncounterState() == _state;
				}

				public override string ToString()
				{
					return "If Encounter " + EnumUtil.FriendlyName(_state);
				}
			}

			[ProtoContract]
			[UIField(tooltip = "Check if combatant is alive.")]
			public class Alive : Combatant
			{
				[ProtoMember(1)]
				[UIField]
				private bool _invert;

				protected override bool _IsValidTarget(AEntity actor, ACombatant target)
				{
					return target.alive ^ _invert;
				}

				public override string ToString()
				{
					return "If " + _invert.ToText("dead", "alive");
				}
			}

			public override bool IsValid(ActionContext context)
			{
				if (context.target is ACombatant target)
				{
					return _IsValidTarget(context.actor, target);
				}
				return false;
			}

			protected virtual bool _IsValidTarget(AEntity actor, ACombatant target)
			{
				return true;
			}
		}

		[ProtoContract]
		[UIField]
		[ProtoInclude(10, typeof(AbilityCount))]
		[ProtoInclude(11, typeof(CharacterClass))]
		[ProtoInclude(12, typeof(ProceduralPhase))]
		[ProtoInclude(13, typeof(CanLevelUp))]
		public abstract class APlayer : Actor
		{
			[ProtoContract]
			[UIField]
			public class AbilityCount : APlayer
			{
				private static readonly RangeByte DEFAULT = new RangeByte(5, 10, 0, 10, 0, 0);

				[ProtoMember(1)]
				[UIField]
				private RangeByte _range = DEFAULT;

				[ProtoMember(2, OverwriteList = true)]
				[UIField(tooltip = "Conditions that must be true for abilities to count towards ability count.")]
				[UIFieldCollectionItem]
				[UIDeepValueChange]
				private List<AAbility> _conditions;

				protected override AbilityPreventedBy _abilityPreventedBy
				{
					get
					{
						if (_range.max != _range.maxRange)
						{
							return base._abilityPreventedBy;
						}
						return AbilityPreventedBy.NotEnoughAbilities;
					}
				}

				private bool _rangeSpecified => _range != DEFAULT;

				public override bool IsValidTarget(AEntity actor, Player target)
				{
					return true;
				}

				public override bool IsValid(ActionContext context)
				{
					if (context.target is Player player)
					{
						return _range.InRangeSmart(player.abilityDeck.GetCards(Ability.Pile.Hand).Count((Ability a) => _conditions.All<AAbility>(context.SetTarget(a))));
					}
					return false;
				}

				public override string ToString()
				{
					return "If has " + _range.ToRangeString(null, "", 50) + " " + _conditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + "abilities";
				}
			}

			[ProtoContract]
			[UIField]
			public class CharacterClass : APlayer
			{
				[ProtoMember(1)]
				[UIField]
				private PlayerClass _class;

				public override bool IsValidTarget(AEntity actor, Player target)
				{
					return target.characterClass == _class;
				}

				public override string ToString()
				{
					return "Is " + EnumUtil.FriendlyName(_class);
				}
			}

			[ProtoContract]
			[UIField]
			public class ProceduralPhase : APlayer
			{
				[ProtoMember(1)]
				[UIField]
				private ProceduralPhaseFlags _phases;

				public override bool IsValidTarget(AEntity actor, Player target)
				{
					ProceduralPhaseType? proceduralPhase = target.gameState.proceduralPhase;
					if (proceduralPhase.HasValue)
					{
						ProceduralPhaseType valueOrDefault = proceduralPhase.GetValueOrDefault();
						return EnumUtil.HasFlagConvert(_phases, valueOrDefault);
					}
					return false;
				}

				public override string ToString()
				{
					return "Is " + EnumUtil.FriendlyName(_phases) + " <b>Phase</b>";
				}
			}

			[ProtoContract]
			[UIField]
			public class CanLevelUp : APlayer
			{
				[ProtoMember(1)]
				[UIField]
				private bool _invert;

				public override bool IsValidTarget(AEntity actor, Player target)
				{
					return (target.heroDeck.Count(HeroDeckPile.Draw) > 0) ^ _invert;
				}

				public override string ToString()
				{
					return _invert.ToText("Can't Level Up", "Can Level Up");
				}
			}

			public override bool IsValid(ActionContext context)
			{
				if (context.target is Player target)
				{
					return IsValidTarget(context.actor, target);
				}
				return false;
			}

			public abstract bool IsValidTarget(AEntity actor, Player target);
		}

		[ProtoContract]
		[UIField]
		[ProtoInclude(10, typeof(Filter))]
		[ProtoInclude(11, typeof(WildSuit))]
		[ProtoInclude(12, typeof(WildValue))]
		[ProtoInclude(13, typeof(Wilded))]
		[ProtoInclude(14, typeof(TopDeck))]
		public abstract class AResource : Condition
		{
			[ProtoContract]
			[UIField]
			public class Filter : AResource
			{
				[ProtoContract(EnumPassthru = true)]
				public enum FilterType
				{
					[UITooltip("Current value of card passes filter.")]
					Is,
					[UITooltip("Card can be wilded to pass the filter.")]
					CanBe,
					[UITooltip("Card's natural value passes the filter.")]
					Naturally
				}

				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Hide)]
				private PlayingCard.Filter _filter;

				[ProtoMember(2)]
				[UIField(tooltip = "Determines how the filter is applied to card.")]
				private FilterType _filterType;

				[ProtoMember(3)]
				[UIField]
				private bool _invert;

				public override bool IsValidTarget(AEntity actor, ResourceCard target)
				{
					return (_filterType switch
					{
						FilterType.Is => _filter.IsValid(target), 
						FilterType.CanBe => _filter.AreValid(target), 
						FilterType.Naturally => _filter.IsValid(target.naturalValue.type), 
						_ => true, 
					}) ^ _invert;
				}

				public override string ToString()
				{
					return _invert.ToText("!") + $"If {EnumUtil.FriendlyName(_filterType, uppercase: false)} {_filter}";
				}
			}

			[ProtoContract]
			[UIField]
			public class WildSuit : AResource
			{
				[ProtoMember(1)]
				[UIField]
				private bool _invert;

				private void _OnWildsChanged(ResourceCard card)
				{
					onDirty?.Invoke(card);
				}

				public override bool IsValidTarget(AEntity actor, ResourceCard target)
				{
					return _invert ^ EnumUtil.HasAllFlags(target.suits);
				}

				public override void Register(ActionContext context)
				{
					context.gameState.onWildsChanged += _OnWildsChanged;
				}

				public override void Unregister(ActionContext context)
				{
					context.gameState.onWildsChanged -= _OnWildsChanged;
				}

				public override string ToString()
				{
					return "If is " + _invert.ToText("not ") + "Wild Suit";
				}
			}

			[ProtoContract]
			[UIField]
			public class WildValue : AResource
			{
				[ProtoMember(1)]
				[UIField]
				private bool _invert;

				private void _OnWildsChanged(ResourceCard card)
				{
					onDirty?.Invoke(card);
				}

				public override bool IsValidTarget(AEntity actor, ResourceCard target)
				{
					return _invert ^ EnumUtil.HasAllFlags(target.values);
				}

				public override void Register(ActionContext context)
				{
					context.gameState.onWildsChanged += _OnWildsChanged;
				}

				public override void Unregister(ActionContext context)
				{
					context.gameState.onWildsChanged -= _OnWildsChanged;
				}

				public override string ToString()
				{
					return "If is " + _invert.ToText("not ") + "Wild Value";
				}
			}

			[ProtoContract]
			[UIField]
			public class Wilded : AResource
			{
				[ProtoMember(1)]
				[UIField]
				private bool _invert;

				private void _OnWildValueChanged(ResourceCard card, ResourceCard.WildContext wildContext)
				{
					onDirty?.Invoke(card);
				}

				public override bool IsValidTarget(AEntity actor, ResourceCard target)
				{
					return _invert ^ target.isWild;
				}

				public override void Register(ActionContext context)
				{
					context.gameState.onWildValueChanged += _OnWildValueChanged;
				}

				public override void Unregister(ActionContext context)
				{
					context.gameState.onWildValueChanged -= _OnWildValueChanged;
				}

				public override string ToString()
				{
					return "If is " + _invert.ToText("not ") + "Wilded";
				}
			}

			[ProtoContract]
			[UIField(tooltip = "Allows only targeting last drawn top deck in the case of <b>War</b> top decks.\n<i>Otherwise always returns true.</i>")]
			public class TopDeck : AResource
			{
				public override bool IsValidTarget(AEntity actor, ResourceCard target)
				{
					if (target.gameState.stack.GetSteps().OfType<GameStepTopDeckInstruction>().FirstOrDefault()?.instruction is TopDeckInstruction.War)
					{
						return target.deck.NextInPile(target.pile) == target;
					}
					return true;
				}

				public override string ToString()
				{
					return "If top deck";
				}
			}

			public sealed override bool IsValid(ActionContext context)
			{
				if (context.target is ResourceCard target)
				{
					return IsValidTarget(context.actor, target);
				}
				return false;
			}

			public abstract bool IsValidTarget(AEntity actor, ResourceCard target);
		}

		[ProtoContract]
		[UIField]
		[ProtoInclude(3, typeof(Active))]
		[ProtoInclude(4, typeof(Passive))]
		[ProtoInclude(5, typeof(Trait))]
		[ProtoInclude(6, typeof(StandardAbility))]
		[ProtoInclude(7, typeof(This))]
		[ProtoInclude(8, typeof(AbilityCategory))]
		[ProtoInclude(9, typeof(Invert))]
		[ProtoInclude(10, typeof(ActionFilter))]
		[ProtoInclude(11, typeof(ResourceCost))]
		[ProtoInclude(12, typeof(Or))]
		[ProtoInclude(13, typeof(Reference))]
		[ProtoInclude(14, typeof(AbilityType))]
		[ProtoInclude(15, typeof(Item))]
		[ProtoInclude(16, typeof(Damaging))]
		public abstract class AAbility : Condition
		{
			[ProtoContract]
			[UIField(tooltip = "Player ability which is not in trait hand.")]
			public class Active : AAbility
			{
				protected override bool _IsValidAbility(ActionContext context, Ability targetAbility)
				{
					if (targetAbility.owner is Player)
					{
						return targetAbility.nullablePile != Ability.Pile.HeroPassive;
					}
					return false;
				}

				public override string ToString()
				{
					return "Active";
				}
			}

			[ProtoContract]
			[UIField("Character Ability", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Ability is part of player's ability deck.")]
			public class StandardAbility : AAbility
			{
				protected override bool _IsValidAbility(ActionContext context, Ability targetAbility)
				{
					if (targetAbility.data.category == AbilityData.Category.Ability)
					{
						return targetAbility.data.characterClass.HasValue;
					}
					return false;
				}

				public override string ToString()
				{
					return "Deck";
				}
			}

			[ProtoContract]
			[UIField(tooltip = "Ability is Trait or Triggered Trait.")]
			public class Trait : AAbility
			{
				[ProtoMember(1)]
				[UIField]
				private bool _invert;

				protected override bool _IsValidAbility(ActionContext context, Ability targetAbility)
				{
					return targetAbility.isTrait ^ _invert;
				}

				public override string ToString()
				{
					return _invert.ToText("!") + "Trait";
				}
			}

			[ProtoContract]
			[UIField(tooltip = "Ability is Hero Ability, Level Up Trait, Trump card etc.")]
			public class AbilityCategory : AAbility
			{
				[ProtoMember(1)]
				[UIField]
				private AbilityData.Category _category;

				protected override bool _IsValidAbility(ActionContext context, Ability targetAbility)
				{
					return targetAbility.data.category == _category;
				}

				public override string ToString()
				{
					return EnumUtil.FriendlyName(_category);
				}
			}

			[ProtoContract]
			[UIField(tooltip = "Ability is Buff, Debuff, Summon etc.")]
			public class AbilityType : AAbility
			{
				[ProtoMember(1)]
				[UIField]
				private AbilityData.Type _type;

				protected override bool _IsValidAbility(ActionContext context, Ability targetAbility)
				{
					return targetAbility.data.type == _type;
				}

				public override string ToString()
				{
					return EnumUtil.FriendlyName(_type);
				}
			}

			[ProtoContract]
			[UIField(tooltip = "An action, or all actions, of an ability pass a certain set of conditions.")]
			public class ActionFilter : AAbility
			{
				[ProtoMember(2)]
				[UIField]
				private bool _allActionsMustPassFilter;

				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Hide)]
				private Filter _filter;

				protected override bool _IsValidAbility(ActionContext context, Ability targetAbility)
				{
					if (!_allActionsMustPassFilter)
					{
						return targetAbility.data.actions.Any((AAction action) => _filter.IsValid(context, action));
					}
					return targetAbility.data.actions.All((AAction action) => _filter.IsValid(context, action));
				}

				public override string ToString()
				{
					return string.Format("If {0} <b>{1}</b> action", _allActionsMustPassFilter ? "all" : "contains", _filter) + _allActionsMustPassFilter.ToText("s");
				}
			}

			[ProtoContract]
			[UIField(tooltip = "The cost of an ability satisfies a set of conditions.")]
			public class ResourceCost : AAbility
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Hide)]
				private ResourceCostFilter _filter;

				protected override bool _IsValidAbility(ActionContext context, Ability targetAbility)
				{
					return _filter.IsValid(context, targetAbility);
				}

				public override string ToString()
				{
					return $"If {_filter}";
				}
			}

			[ProtoContract]
			[UIField("Ability Reference", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Ability is exactly matches an ability data reference, or belongs to its upgrade hierarchy.")]
			public class Reference : AAbility
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Open)]
				private DataRef<AbilityData> _abilityReference;

				[ProtoMember(2)]
				[UIField(tooltip = "Check if ability reference is in upgrade hierarchy of given ability.")]
				private bool _checkUpgradeHierarchy;

				protected override bool _IsValidAbility(ActionContext context, Ability targetAbility)
				{
					if (!ContentRef.Equal(_abilityReference, targetAbility.dataRef))
					{
						if (_checkUpgradeHierarchy)
						{
							return ContentRef.Equal(_abilityReference.BaseAbilityRef(), targetAbility.dataRef.BaseAbilityRef());
						}
						return false;
					}
					return true;
				}

				public override string ToString()
				{
					return "Is \"" + _abilityReference.GetFriendlyName() + "\"" + _checkUpgradeHierarchy.ToText(" Hierarchy");
				}
			}

			[ProtoContract]
			[UIField("Item", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Ability is an item or piece of equipment.")]
			public class Item : AAbility
			{
				protected override bool _IsValidAbility(ActionContext context, Ability targetAbility)
				{
					return targetAbility is ItemCard;
				}

				public override string ToString()
				{
					return "Is Item";
				}
			}

			[ProtoContract]
			[UIField("Or (Ability)", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Allows returning true if any of the nested conditions return true for ability.")]
			public new class Or : AAbility
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Hide)]
				private Or<AAbility> _conditions;

				protected override bool _IsValidAbility(ActionContext context, Ability targetAbility)
				{
					return _conditions.IsValid(context);
				}

				public override string ToString()
				{
					return "[" + _conditions?.ToString() + "]";
				}

				private void _OnDirty(ATarget target)
				{
					onDirty?.Invoke(target);
				}

				public override void Register(ActionContext context)
				{
					_conditions.conditions.Register(context, _OnDirty);
				}

				public override void Unregister(ActionContext context)
				{
					_conditions.conditions.Unregister(context, _OnDirty);
				}
			}

			[ProtoContract]
			[UIField("Invert (Ability)", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Inverts the output of the nested condition.")]
			public class Invert : AAbility
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Hide)]
				[UIDeepValueChange]
				private AAbility _condition;

				public override Action<ATarget> onDirty
				{
					get
					{
						return _condition.onDirty;
					}
					set
					{
						_condition.onDirty = value;
					}
				}

				protected override bool _IsValidAbility(ActionContext context, Ability targetAbility)
				{
					return !_condition.IsValid(context);
				}

				public override string ToString()
				{
					return "!" + _condition;
				}

				public override void Register(ActionContext context)
				{
					_condition.Register(context);
				}

				public override void Unregister(ActionContext context)
				{
					_condition.Unregister(context);
				}
			}

			[ProtoContract]
			[UIField("This Ability", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Only returns true for this <b>Instance</b> of exactly this ability.")]
			public class This : AAbility
			{
				[ProtoMember(1)]
				[UIField]
				private bool _invert;

				private Ability _ability;

				public override void Register(ActionContext context)
				{
					_ability = context.ability;
				}

				public override void Unregister(ActionContext context)
				{
					_ability = null;
				}

				protected override bool _IsValidAbility(ActionContext context, Ability targetAbility)
				{
					return ((_ability ?? context.ability) == targetAbility) ^ _invert;
				}

				public override string ToString()
				{
					return "Is " + _invert.ToText("not ") + "this ability";
				}
			}

			[ProtoContract]
			[UIField("Passive", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Applies to traits, and ticking effects of buffs.")]
			public class Passive : AAbility
			{
				[ProtoMember(1)]
				[UIField]
				private bool _invert;

				protected override bool _IsValidAbility(ActionContext context, Ability targetAbility)
				{
					return (targetAbility.isTrait || (context.state == ActionContext.State.Tick && targetAbility.isBuff)) ^ _invert;
				}

				public override string ToString()
				{
					return _invert.ToText("!") + "Passive";
				}
			}

			[ProtoContract]
			[UIField("Damaging", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Does ability deal damage?")]
			public class Damaging : AAbility
			{
				[ProtoMember(1)]
				[UIField]
				private bool _invert;

				protected override bool _IsValidAbility(ActionContext context, Ability targetAbility)
				{
					return targetAbility.dealsDamage ^ _invert;
				}

				public override string ToString()
				{
					return _invert.ToText("!") + "Damaging";
				}
			}

			public override bool IsValid(ActionContext context)
			{
				if (context.target is Ability targetAbility)
				{
					return _IsValidAbility(context, targetAbility);
				}
				return false;
			}

			protected abstract bool _IsValidAbility(ActionContext context, Ability targetAbility);
		}

		[ProtoContract]
		[UIField]
		protected class Or<T> where T : Condition
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open)]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<T> _conditions;

			public List<T> conditions => _conditions;

			public bool IsValid(ActionContext context)
			{
				if (_conditions == null)
				{
					return false;
				}
				foreach (T condition in _conditions)
				{
					if (condition.IsValid(context))
					{
						return true;
					}
				}
				return false;
			}

			public override string ToString()
			{
				if (_conditions.IsNullOrEmpty())
				{
					return "";
				}
				return _conditions.ToStringSmart(" <b>or</b> ");
			}
		}

		[ProtoContract]
		[UIField]
		protected class Targets<T> where T : Condition
		{
			private static readonly RangeByte DEFAULT_COUNT = new RangeByte(1, 5, 0, 5, 0, 0);

			[ProtoMember(1)]
			[UIField]
			private RangeByte _count = DEFAULT_COUNT;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Open)]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<T> _conditions;

			private bool _countSpecified => _count != DEFAULT_COUNT;

			public bool IsValid(ActionContext context)
			{
				return _count.InRangeSmart(context.targets.Count((ATarget target) => _conditions.All(context.SetTarget(target))));
			}

			public override string ToString()
			{
				return "If " + ((!_conditions.IsNullOrEmpty()) ? ("[" + _conditions.ToStringSmart(" & ") + "] true for ") : "") + _count.ToRangeString(null, "target".Pluralize(_count.max), 100);
			}
		}

		[ProtoContract]
		[UIField]
		protected class DynamicNumberCheck
		{
			private static readonly RangeInt DEFAULT_RANGE = new RangeInt(1, 20, -20, 20);

			[ProtoMember(1)]
			[UIField(tooltip = "The Value to check against Range.")]
			[UIDeepValueChange]
			private DynamicNumber _value;

			[ProtoMember(2)]
			[UIField(tooltip = "The Range in which Value must fall in order to be valid.")]
			private RangeInt _range = DEFAULT_RANGE;

			private bool _rangeSpecified => _range != DEFAULT_RANGE;

			public bool IsValid(ActionContext context)
			{
				return _range.InRangeSmart(_value.GetValue(context));
			}

			public override string ToString()
			{
				return string.Format("If {0} is {1}", _value, _range.ToRangeString(null, "", 50));
			}

			public AbilityPreventedBy? GetPreventedBy()
			{
				return _value?.GetPreventedBy(_range);
			}
		}

		[ProtoContract]
		[UIField]
		protected class DynamicNumberCompare
		{
			[ProtoMember(1)]
			[UIField]
			[UIDeepValueChange]
			private DynamicNumber _a;

			[ProtoMember(2)]
			[UIField]
			[UIDeepValueChange]
			private DynamicNumber _b;

			[ProtoMember(3)]
			[UIField]
			private FlagCheckType _comparison;

			public bool IsValid(ActionContext context)
			{
				return _comparison.Check(_a.GetValue(context), _b.GetValue(context));
			}

			public override string ToString()
			{
				return $"If {_a} {_comparison.GetText()} {_b}";
			}
		}

		protected virtual AbilityPreventedBy _abilityPreventedBy => AbilityPreventedBy.ConditionNotMet;

		public virtual Action<ATarget> onDirty { get; set; }

		public virtual bool IsValid(ActionContext context)
		{
			return true;
		}

		public AbilityPreventedBy? GetAbilityPreventedBy(ActionContext context)
		{
			if (!IsValid(context))
			{
				return _abilityPreventedBy;
			}
			return null;
		}

		public virtual IEnumerable<AbilityKeyword> GetKeywords()
		{
			yield break;
		}

		public virtual void Register(ActionContext context)
		{
		}

		public virtual void Unregister(ActionContext context)
		{
		}
	}

	[ProtoContract]
	[UIField]
	[ProtoInclude(10, typeof(TargetCount))]
	[ProtoInclude(11, typeof(Type))]
	[ProtoInclude(12, typeof(TargetType))]
	public abstract class Filter
	{
		[ProtoContract]
		[UIField]
		public class TargetCount : Filter
		{
			[ProtoMember(1)]
			[UIField]
			private TargetCountType _type;

			[ProtoMember(2)]
			[UIField]
			private TargetToCheck _targetToCheck;

			public override bool IsValid(ActionContext context, AAction action)
			{
				return _type == action[_targetToCheck, context.state].targetCountType;
			}

			public override string ToString()
			{
				return EnumUtil.FriendlyName(_type) + ((_targetToCheck == TargetToCheck.Active) ? "" : (" " + EnumUtil.FriendlyName(_targetToCheck)));
			}
		}

		[ProtoContract]
		[UIField("Action Type", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		public class Type : Filter
		{
			[ProtoMember(1)]
			[UIField(filter = typeof(AAction), collapse = UICollapseType.Hide)]
			private SpecificType _actionType;

			public override bool IsValid(ActionContext context, AAction action)
			{
				return action.GetType().Equals(_actionType);
			}

			public override string ToString()
			{
				return _actionType;
			}
		}

		[ProtoContract]
		[UIField]
		public class TargetType : Filter
		{
			[ProtoContract(EnumPassthru = true)]
			private enum Targets
			{
				Combatant,
				ResourceCard,
				Ability
			}

			[ProtoMember(1)]
			[UIField]
			private Targets _targets;

			[ProtoMember(2)]
			[UIField]
			private TargetToCheck _targetToCheck;

			public override bool IsValid(ActionContext context, AAction action)
			{
				System.Type type = action?[_targetToCheck, context.state]?.GetType();
				bool flag = (object)type != null;
				if (flag)
				{
					flag = _targets switch
					{
						Targets.Combatant => type.IsSubclassOf(typeof(Target.Combatant)), 
						Targets.ResourceCard => type.IsSubclassOf(typeof(Target.Resource)), 
						Targets.Ability => type.IsSubclassOf(typeof(Target.AAbility)), 
						_ => false, 
					};
				}
				return flag;
			}

			public override string ToString()
			{
				return "Targets " + EnumUtil.FriendlyName(_targets) + ((_targetToCheck == TargetToCheck.Active) ? "" : (" " + EnumUtil.FriendlyName(_targetToCheck)));
			}
		}

		public abstract bool IsValid(ActionContext context, AAction action);
	}

	[ProtoContract]
	[UIField]
	public class Duration
	{
		[ProtoMember(1)]
		[UIField(excludedValuesMethod = "_ExcludeTriggeredBy")]
		[UIHorizontalLayout("A")]
		public ReactionEntity triggeredBy;

		[ProtoMember(2)]
		[UIField(excludedValuesMethod = "_ExcludeTriggeredOn")]
		[UIHorizontalLayout("A")]
		[DefaultValue(ReactionEntity.Anyone)]
		public ReactionEntity triggeredOn = ReactionEntity.Anyone;

		[ProtoMember(3)]
		[UIField(collapse = UICollapseType.Hide)]
		protected Reaction _reaction;

		private ReactionFilter _reactionFilter
		{
			get
			{
				ReactionFilter result = default(ReactionFilter);
				result.triggeredBy = triggeredBy;
				result.triggeredOn = triggeredOn;
				return result;
			}
		}

		protected bool _hideTriggeredBy => !(_reaction?.usesTriggeredBy ?? true);

		protected bool _hideTriggeredOn => !(_reaction?.usesTriggeredOn ?? true);

		public event Action<ReactionContext, ReactionFilter> onDurationComplete;

		private void _OnDurationComplete(ReactionContext context)
		{
			this.onDurationComplete?.Invoke(context, _reactionFilter);
		}

		public void Register(ActionContext context)
		{
			_reaction.Register(context);
			Reaction reaction = _reaction;
			reaction.onTrigger = (Action<ReactionContext>)Delegate.Combine(reaction.onTrigger, new Action<ReactionContext>(_OnDurationComplete));
		}

		public void Unregister(ActionContext context)
		{
			_reaction.Unregister(context);
			Reaction reaction = _reaction;
			reaction.onTrigger = (Action<ReactionContext>)Delegate.Remove(reaction.onTrigger, new Action<ReactionContext>(_OnDurationComplete));
		}

		public override string ToString()
		{
			return string.Format("Until{0}{1}{2}", (!_hideTriggeredBy) ? (" <b>" + EnumUtil.FriendlyName(triggeredBy) + "</b> ") : " ", _reaction, (!_hideTriggeredOn) ? (" <b>" + EnumUtil.FriendlyName(triggeredOn) + "</b>") : "");
		}

		protected bool _ExcludeTriggeredBy(ReactionEntity entity)
		{
			if (_hideTriggeredBy)
			{
				return entity != ReactionEntity.Anyone;
			}
			return false;
		}

		protected bool _ExcludeTriggeredOn(ReactionEntity entity)
		{
			if (_hideTriggeredOn)
			{
				return entity != ReactionEntity.Anyone;
			}
			return false;
		}
	}

	[ProtoContract]
	[UIField]
	[ProtoInclude(2, typeof(Constant))]
	[ProtoInclude(3, typeof(ResourceDeckValue))]
	[ProtoInclude(4, typeof(AbilityHand))]
	[ProtoInclude(5, typeof(HP))]
	[ProtoInclude(6, typeof(Shield))]
	[ProtoInclude(7, typeof(Statistic))]
	[ProtoInclude(8, typeof(PlayerStatistic))]
	[ProtoInclude(9, typeof(CapturedEventValue))]
	[ProtoInclude(10, typeof(ProcessedValue))]
	[ProtoInclude(11, typeof(ProcessedValueComplex))]
	[ProtoInclude(12, typeof(Min))]
	[ProtoInclude(13, typeof(Max))]
	[ProtoInclude(14, typeof(Distance))]
	[ProtoInclude(15, typeof(SnapshotValue))]
	[ProtoInclude(16, typeof(Round))]
	[ProtoInclude(17, typeof(Encounter))]
	[ProtoInclude(18, typeof(CombatantStatistic))]
	[ProtoInclude(19, typeof(Divide))]
	[ProtoInclude(20, typeof(EntityCount))]
	[ProtoInclude(21, typeof(Random))]
	[ProtoInclude(22, typeof(HealthDice))]
	[ProtoInclude(23, typeof(CombatDamage))]
	[ProtoInclude(24, typeof(NumberOfAttacksRemaining))]
	[ProtoInclude(25, typeof(NumberOfHeroAbilitiesRemaining))]
	[ProtoInclude(26, typeof(Conditional))]
	[ProtoInclude(27, typeof(Power))]
	public abstract class DynamicNumber
	{
		[ProtoContract]
		[UIField(tooltip = "Value is equal to a constant of your choosing.", category = "Constant")]
		public class Constant : DynamicNumber
		{
			public const int MAX = 25;

			[ProtoMember(1)]
			[DefaultValue(1)]
			[UIField(min = -25, max = 25)]
			private int _value = 1;

			public override int? constantValue => _value;

			public Constant()
			{
			}

			public Constant(int value)
			{
				_value = value;
			}

			protected override int _GetValue(ActionContext context)
			{
				return _value;
			}

			public override string ToString()
			{
				return _value.ToString();
			}
		}

		[ProtoContract]
		[UIField("Resource Deck", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Value is equal to the number of cards that pass a given filter that are in specified piles of a combatant's resource deck.", category = "Hands")]
		public class ResourceDeckValue : DynamicNumber
		{
			[ProtoContract(EnumPassthru = true)]
			public enum Function
			{
				Number,
				Sum,
				SuitCount
			}

			[ProtoMember(1)]
			[UIField]
			private ResourceCard.Piles _piles;

			[ProtoMember(2)]
			[UIField]
			[UIHorizontalLayout("A")]
			private ActionContextTarget _target;

			[ProtoMember(3)]
			[UIField(tooltip = "Determines how value is derived from cards that pass filter.")]
			[UIHorizontalLayout("A")]
			private Function _function;

			[ProtoMember(4)]
			[UIField(collapse = UICollapseType.Hide)]
			private PlayingCard.Filter _filter;

			[ProtoMember(5, OverwriteList = true)]
			[UIField(tooltip = "Conditions that must hold true for resource cards to count towards value.")]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<Condition.AResource> _conditions;

			private bool _trackWildChanges
			{
				get
				{
					if (!_filter)
					{
						return _function == Function.SuitCount;
					}
					return true;
				}
			}

			protected override ActionContextTarget _contextTarget => _target;

			private ResourceDeckValue()
			{
			}

			public ResourceDeckValue(ResourceCard.Piles piles)
			{
				_piles = piles;
			}

			private void _OnTransfer(ResourceCard value, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
			{
				if (_piles.Contains(oldPile) ^ _piles.Contains(newPile))
				{
					_Reapply(_trackWildChanges);
				}
			}

			private void _OnTransferDynamic(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
			{
				if ((_piles.Contains(oldPile) ^ _piles.Contains(newPile)) && base._appliedContext.GetTarget<ACombatant>(_target)?.resourceDeck == card.deck)
				{
					_Reapply(_trackWildChanges);
				}
			}

			private void _OnCombatIsActiveChanged(bool combatIsActive)
			{
				_Reapply(_trackWildChanges);
			}

			private void _OnWildValueChange(ResourceCard card, ResourceCard.WildContext wildContext)
			{
				if (_piles.Contains(card.pile) && base._appliedContext.GetTarget<ACombatant>(_target)?.resourceDeck == card.deck)
				{
					_Reapply(checkIfValueChanged: true);
				}
			}

			private void _OnConditionDirty(ATarget conditionTarget)
			{
				if (conditionTarget is ResourceCard resourceCard && _piles.Contains(resourceCard.pile) && base._appliedContext.GetTarget<ACombatant>(_target)?.resourceDeck == resourceCard.deck)
				{
					_Reapply(checkIfValueChanged: true);
				}
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				if (!_target.IsDynamic())
				{
					appliedAction.context.GetTarget<ACombatant>(_target).resourceDeck.onTransfer += _OnTransfer;
				}
				else
				{
					appliedAction.context.gameState.player.resourceDeck.onTransfer += _OnTransferDynamic;
					appliedAction.context.gameState.enemyResourceDeck.onTransfer += _OnTransferDynamic;
					if (_target.IsCombatCentric())
					{
						appliedAction.context.gameState.onCombatIsActiveChanged += _OnCombatIsActiveChanged;
					}
				}
				if (_trackWildChanges)
				{
					appliedAction.context.gameState.onWildValueChanged += _OnWildValueChange;
				}
				if (!_conditions.IsNullOrEmpty())
				{
					_conditions.Register(appliedAction.context, _OnConditionDirty);
				}
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				if (!_target.IsDynamic())
				{
					appliedAction.context.GetTarget<ACombatant>(_target).resourceDeck.onTransfer -= _OnTransfer;
				}
				else
				{
					appliedAction.context.gameState.player.resourceDeck.onTransfer -= _OnTransferDynamic;
					appliedAction.context.gameState.enemyResourceDeck.onTransfer -= _OnTransferDynamic;
					if (_target.IsCombatCentric())
					{
						appliedAction.context.gameState.onCombatIsActiveChanged -= _OnCombatIsActiveChanged;
					}
				}
				if (_trackWildChanges)
				{
					appliedAction.context.gameState.onWildValueChanged -= _OnWildValueChange;
				}
				if (!_conditions.IsNullOrEmpty())
				{
					_conditions.Unregister(appliedAction.context, _OnConditionDirty);
				}
			}

			public override AbilityPreventedBy? GetPreventedBy(RangeInt range)
			{
				if (range.max != range.maxRange)
				{
					return null;
				}
				return _conditions.IsNullOrEmpty() ? AbilityPreventedBy.ResourceCard : AbilityPreventedBy.NoValidTargets;
			}

			protected override int _GetValue(ActionContext context)
			{
				IEnumerable<ResourceCard> enumerable = (from card in context.GetTarget<ACombatant>(_target)?.resourceDeck.GetCards(_piles)
					where _filter.IsValid(card) && _conditions.All(context.SetTarget(card))
					select card);
				if (enumerable == null)
				{
					return 0;
				}
				if (_function != Function.SuitCount)
				{
					return enumerable.Sum((ResourceCard card) => (int)((_function != Function.Sum) ? ((PlayingCardValue)1) : card.currentValue.value));
				}
				return enumerable.Select((ResourceCard c) => c.suit).Distinct().Count();
			}

			public override string ToString()
			{
				return string.Format("({0} of {1}{2}cards in {3}'s {4})", (_function == Function.Number) ? "#" : ((_function == Function.Sum) ? "Sum" : "#Suits"), _conditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty(), _filter.adjective.SpaceIfNotEmpty(), _target, _piles.ToText());
			}
		}

		[ProtoContract]
		[UIField("Ability Deck", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Value is equal to the number of cards that pass a given filter that are in player's ability deck piles.", category = "Hands")]
		public class AbilityHand : DynamicNumber
		{
			[ProtoMember(2)]
			[UIField]
			[DefaultValue(Ability.Piles.Hand)]
			private Ability.Piles _piles = Ability.Piles.Hand;

			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open, tooltip = "Conditions that must be met by an ability in order to count towards value.")]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<Condition.AAbility> _conditions;

			public override AbilityPreventedBy? GetPreventedBy(RangeInt range)
			{
				if (range.max != range.maxRange)
				{
					return null;
				}
				return _conditions.IsNullOrEmpty() ? AbilityPreventedBy.NotEnoughAbilities : AbilityPreventedBy.NoValidTargets;
			}

			private void _OnTransfer(Ability value, Ability.Pile? oldPile, Ability.Pile? newPile)
			{
				if (EnumUtil.HasFlagConvert(_piles, oldPile) || EnumUtil.HasFlagConvert(_piles, newPile))
				{
					_Reapply(!_conditions.IsNullOrEmpty());
				}
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.player.abilityDeck.onTransfer += _OnTransfer;
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.player.abilityDeck.onTransfer -= _OnTransfer;
			}

			protected override int _GetValue(ActionContext context)
			{
				return context.gameState.player.abilityDeck.GetCards(_piles).Count((Ability card) => _conditions.All<Condition.AAbility>(context.SetTarget(card)));
			}

			public override string ToString()
			{
				return "(# of <size=66%>" + _conditions.ToStringSmart(" & ").SpaceIfNotEmpty() + "</size>cards in Ability " + EnumUtil.FriendlyName(_piles) + ")";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Value is equal to the current Hit Points of actor or target.", category = "Attributes")]
		public class HP : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField]
			[UIHorizontalLayout("A")]
			private ActionContextTarget _target;

			[ProtoMember(2)]
			[UIField]
			[UIHorizontalLayout("A")]
			private bool _useMissingHP;

			protected override ActionContextTarget _contextTarget => _target;

			private void _OnHPChanged(int previousHP, int hp)
			{
				_Reapply();
			}

			private void _OnHPChangedDynamic(ACombatant combatant, int previousHP, int hp)
			{
				if (combatant == base._appliedContext.GetTarget<ACombatant>(_target))
				{
					_Reapply();
				}
			}

			private void _OnCombatIsActiveChange(bool combatIsActive)
			{
				_Reapply();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				if (!_target.IsDynamic())
				{
					appliedAction.context.GetTarget<ACombatant>(_target).HP.onValueChanged += _OnHPChanged;
					return;
				}
				appliedAction.context.gameState.onHPChange += _OnHPChangedDynamic;
				if (_target.IsCombatCentric())
				{
					appliedAction.context.gameState.onCombatIsActiveChanged += _OnCombatIsActiveChange;
				}
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				if (!_target.IsDynamic())
				{
					appliedAction.context.GetTarget<ACombatant>(_target).HP.onValueChanged -= _OnHPChanged;
					return;
				}
				appliedAction.context.gameState.onHPChange -= _OnHPChangedDynamic;
				if (_target.IsCombatCentric())
				{
					appliedAction.context.gameState.onCombatIsActiveChanged -= _OnCombatIsActiveChange;
				}
			}

			protected override int _GetValue(ActionContext context)
			{
				ACombatant aCombatant = context.GetTarget<ACombatant>(_target);
				if (aCombatant == null)
				{
					return 0;
				}
				if (!_useMissingHP)
				{
					return aCombatant.HP;
				}
				return aCombatant.HPMissing;
			}

			public override string ToString()
			{
				return string.Format("({0} {1}HP)", _target, _useMissingHP.ToText("Missing "));
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Value is equal to the current shield amount of the actor.", category = "Attributes")]
		public class Shield : DynamicNumber
		{
			protected override ActionContextTarget _contextTarget => ActionContextTarget.Owner;

			private void _OnShieldChanged(int previousShield, int shield)
			{
				_Reapply();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				appliedAction.context.GetTarget<ACombatant>(ActionContextTarget.Owner).shield.onValueChanged += _OnShieldChanged;
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				appliedAction.context.GetTarget<ACombatant>(ActionContextTarget.Owner).shield.onValueChanged -= _OnShieldChanged;
			}

			protected override int _GetValue(ActionContext context)
			{
				CappedBInt cappedBInt = context.GetTarget<ACombatant>(ActionContextTarget.Owner)?.shield;
				if ((object)cappedBInt == null)
				{
					return 0;
				}
				return cappedBInt;
			}

			public override string ToString()
			{
				return "(Shield)";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Uses the value of a desired statistic from actor or target.", category = "Attributes")]
		public class Statistic : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField]
			[UIHorizontalLayout("A")]
			private ActionContextTarget _target;

			[ProtoMember(2)]
			[UIField]
			[UIHorizontalLayout("A")]
			private StatType _stat;

			protected override ActionContextTarget _contextTarget => _target;

			private void _OnStatChanged(int previousStat, int stat)
			{
				_Reapply();
			}

			private void _OnStatChangeDynamic(ACombatant combatant, StatType stat, int oldStat, int newStat)
			{
				if (stat == _stat && combatant == base._appliedContext.GetTarget<ACombatant>(_target))
				{
					_Reapply();
				}
			}

			private void _OnCombatIsActiveChanged(bool combatIsActive)
			{
				_Reapply();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				if (!_target.IsDynamic())
				{
					appliedAction.context.GetTarget<ACombatant>(_target).stats[_stat].onValueChanged += _OnStatChanged;
					return;
				}
				appliedAction.context.gameState.onStatChange += _OnStatChangeDynamic;
				if (_target.IsCombatCentric())
				{
					appliedAction.context.gameState.onCombatIsActiveChanged += _OnCombatIsActiveChanged;
				}
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				if (!_target.IsDynamic())
				{
					appliedAction.context.GetTarget<ACombatant>(_target).stats[_stat].onValueChanged -= _OnStatChanged;
					return;
				}
				appliedAction.context.gameState.onStatChange -= _OnStatChangeDynamic;
				if (_target.IsCombatCentric())
				{
					appliedAction.context.gameState.onCombatIsActiveChanged -= _OnCombatIsActiveChanged;
				}
			}

			protected override int _GetValue(ActionContext context)
			{
				BInt bInt = context.GetTarget<ACombatant>(_target)?.stats[_stat];
				if ((object)bInt == null)
				{
					return 0;
				}
				return bInt;
			}

			public override string ToString()
			{
				return "(" + EnumUtil.FriendlyName(_target) + " " + EnumUtil.FriendlyName(_stat) + ")";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Uses the value of a desired player statistic.\n<i>Player statistics are unique values which players have but enemies do not.</i>", category = "Attributes")]
		public class PlayerStatistic : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField(tooltip = "The statistic to derive value from.")]
			private PlayerStatType _stat;

			private void _OnStatChanged(int previousStat, int stat)
			{
				_Reapply();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.player.playerStats[_stat].onValueChanged += _OnStatChanged;
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.player.playerStats[_stat].onValueChanged -= _OnStatChanged;
			}

			protected override int _GetValue(ActionContext context)
			{
				return context.gameState.player.playerStats[_stat];
			}

			public override string ToString()
			{
				return "(" + EnumUtil.FriendlyName(_stat) + ")";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Uses the effective offense, or defense of an active combatant.\n<i>This means the stat after being fully processed against the opposing stat.</i>", category = "Attributes")]
		public class CombatantStatistic : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField]
			private CombatType _combatant;

			protected override int _GetValue(ActionContext context)
			{
				if (context.gameState.activeCombat != null)
				{
					if (_combatant != 0)
					{
						return context.gameState.activeCombat.GetEffectiveDefense();
					}
					return context.gameState.activeCombat.GetEffectiveOffense();
				}
				return 0;
			}

			public override string ToString()
			{
				if (_combatant != 0)
				{
					return "Effective Defense of Defender";
				}
				return "Effective Offense of Attacker";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Uses the turn order distance between owner and target.\n<i>An adjacent target would be considered 1 distance away.</i>", category = "Attributes")]
		public class Distance : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField(tooltip = "Allows distance to become negative.\n<i>A negative distance means owner is later in turn order than target.</i>")]
			private bool _useSignedDistance;

			private void _OnAdventureTransfer(ATarget card, AdventureCard.Pile? oldPile, AdventureCard.Pile? newPile)
			{
				if (oldPile == AdventureCard.Pile.TurnOrder || newPile == AdventureCard.Pile.TurnOrder)
				{
					_Reapply(checkIfValueChanged: true);
				}
			}

			private void _OnAdventureReordered(ATarget card, AdventureCard.Pile pile)
			{
				if (pile == AdventureCard.Pile.TurnOrder)
				{
					_Reapply(checkIfValueChanged: true);
				}
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.adventureDeck.onTransfer += _OnAdventureTransfer;
				appliedAction.context.gameState.adventureDeck.onReorder += _OnAdventureReordered;
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.adventureDeck.onTransfer -= _OnAdventureTransfer;
				appliedAction.context.gameState.adventureDeck.onReorder -= _OnAdventureReordered;
			}

			protected override int _GetValue(ActionContext context)
			{
				int num = context.gameState.GetTurnOrder(context.GetTarget<AEntity>()) - context.gameState.GetTurnOrder(context.actor);
				if (!_useSignedDistance)
				{
					return Math.Abs(num);
				}
				return num;
			}

			public override string ToString()
			{
				return _useSignedDistance.ToText("Signed ") + "Distance To";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Uses the maximum value of Health Dice of actor or target.", category = "Attributes")]
		public class HealthDice : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField]
			private ActionContextTarget _target;

			protected override ActionContextTarget _contextTarget => _target;

			private void _OnStatChanged(int previousStat, int stat)
			{
				_Reapply();
			}

			private void _OnStatChangeDynamic(ACombatant combatant, StatType stat, int oldStat, int newStat)
			{
				if (stat == StatType.Health && combatant == base._appliedContext.GetTarget<ACombatant>(_target))
				{
					_Reapply();
				}
			}

			private void _OnCombatIsActiveChanged(bool combatIsActive)
			{
				_Reapply();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				if (!_target.IsDynamic())
				{
					appliedAction.context.GetTarget<ACombatant>(_target).stats[StatType.Health].onValueChanged += _OnStatChanged;
					return;
				}
				appliedAction.context.gameState.onStatChange += _OnStatChangeDynamic;
				if (_target.IsCombatCentric())
				{
					appliedAction.context.gameState.onCombatIsActiveChanged += _OnCombatIsActiveChanged;
				}
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				if (!_target.IsDynamic())
				{
					appliedAction.context.GetTarget<ACombatant>(_target).stats[StatType.Health].onValueChanged -= _OnStatChanged;
					return;
				}
				appliedAction.context.gameState.onStatChange -= _OnStatChangeDynamic;
				if (_target.IsCombatCentric())
				{
					appliedAction.context.gameState.onCombatIsActiveChanged -= _OnCombatIsActiveChanged;
				}
			}

			protected override int _GetValue(ActionContext context)
			{
				ACombatant aCombatant = context.GetTarget<ACombatant>(_target);
				if (aCombatant == null)
				{
					return 6;
				}
				return (int)(aCombatant.combatantCard?.hpDiceType ?? EnumUtil<DiceType>.Min.GetDiceTypeFromMax(aCombatant.stats[StatType.Health]));
			}

			public override string ToString()
			{
				return $"({_target} HP Dice Max)";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Uses the number of entities which pass certain criteria that are found in certain piles.", category = "Hands")]
		public class EntityCount : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField]
			[DefaultValue(AdventureCard.Piles.TurnOrder)]
			private AdventureCard.Piles _piles = AdventureCard.Piles.TurnOrder;

			[ProtoMember(2, OverwriteList = true)]
			[UIField]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<Condition.Combatant> _entityConditions;

			private void _OnConditionDirty(ATarget target)
			{
				if (target is AEntity aEntity && EnumUtil.HasFlagConvert(_piles, aEntity.pile))
				{
					_Reapply(checkIfValueChanged: true);
				}
			}

			private void _OnAdventureDeckTransfer(ATarget card, AdventureCard.Pile? oldPile, AdventureCard.Pile? newPile)
			{
				if (card is AEntity && (EnumUtil.HasFlagConvert(_piles, oldPile.GetValueOrDefault()) ^ EnumUtil.HasFlagConvert(_piles, newPile.GetValueOrDefault())))
				{
					_Reapply(checkIfValueChanged: true);
				}
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.adventureDeck.onTransfer += _OnAdventureDeckTransfer;
				_entityConditions.Register(appliedAction.context, _OnConditionDirty);
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.adventureDeck.onTransfer -= _OnAdventureDeckTransfer;
				_entityConditions.Unregister(appliedAction.context, _OnConditionDirty);
			}

			protected override int _GetValue(ActionContext context)
			{
				int num = 0;
				foreach (ATarget card in context.gameState.adventureDeck.GetCards(_piles))
				{
					if (card is AEntity aEntity && _entityConditions.All(context.SetTarget(aEntity)))
					{
						num++;
					}
				}
				return num;
			}

			public override string ToString()
			{
				return "(# of " + _entityConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + "entities in " + EnumUtil.FriendlyName(_piles) + ")";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Uses the value captured from the event which triggered an action to take affect.", category = "Advanced")]
		public class CapturedEventValue : DynamicNumber
		{
			protected override int _GetValue(ActionContext context)
			{
				return context.capturedValue;
			}

			public override string ToString()
			{
				return "(Captured Event Value)";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Allows taking a snapshot of another dynamic number that will remain constant after first retrieval.", category = "Advanced")]
		public class SnapshotValue : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField("Snapshot Value", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open, tooltip = "Value which will be retrieved and then remain constant.")]
			[UIDeepValueChange]
			private DynamicNumber _value;

			[ProtoMember(2)]
			private int? _snapshot;

			protected override ActionContextTarget _contextTarget => _value._contextTarget;

			protected override int _GetValue(ActionContext context)
			{
				int valueOrDefault = _snapshot.GetValueOrDefault();
				if (!_snapshot.HasValue)
				{
					valueOrDefault = _value._GetValue(context);
					_snapshot = valueOrDefault;
					return valueOrDefault;
				}
				return valueOrDefault;
			}

			public override string ToString()
			{
				return $"<size=66%>Snapshot</size>[{_value}]";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Takes a dynamic number and allows multiplying and adding to it.", category = "Math")]
		public class ProcessedValue : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField("Processed Value", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open, tooltip = "Value to be <i>Multiplied</i> and/or <i>Offset</i>.")]
			[UIDeepValueChange]
			private DynamicNumber _value;

			[ProtoMember(2)]
			[UIField(min = -10, max = 10, tooltip = "Multiplies <i>Processed Value</i>.")]
			[DefaultValue(1)]
			[UIHorizontalLayout("A")]
			private int _multiplier = 1;

			[ProtoMember(3)]
			[UIField(min = -25, max = 25, tooltip = "Is added to <i>Processed Value</i>.")]
			[UIHorizontalLayout("A")]
			private int _offset;

			protected override ActionContextTarget _contextTarget => _value._contextTarget;

			protected override void _ClearCachedValue()
			{
				base._ClearCachedValue();
				_value._ClearCachedValue();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				_value.Register(appliedAction);
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				_value.Unregister();
			}

			protected override int _GetValue(ActionContext context)
			{
				return _value._GetValue(context) * _multiplier + _offset;
			}

			public override string ToString()
			{
				return "(" + (_multiplier == -1).ToText("-") + _value?.ToString() + ((_multiplier != 1 && _multiplier != -1) ? (" * " + _multiplier) : "") + ((_offset != 0) ? (" " + ((_offset > 0) ? "+" : "-") + " " + Math.Abs(_offset)) : "") + ")";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Takes a dynamic number and allows multiplying and adding to it using other dynamic numbers.", category = "Math")]
		public class ProcessedValueComplex : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField("Processed Value", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open, tooltip = "Value to be <i>Multiplied</i> and/or <i>Offset</i>.")]
			[UIDeepValueChange]
			private DynamicNumber _value;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Open, tooltip = "Multiplies <i>Processed Value</i>.")]
			[UIDeepValueChange]
			private DynamicNumber _multiplier;

			[ProtoMember(3)]
			[UIField(collapse = UICollapseType.Open, tooltip = "Is added to <i>Processed Value</i>.")]
			[UIDeepValueChange]
			private DynamicNumber _offset;

			protected override ActionContextTarget _contextTarget => _value._contextTarget;

			protected override void _ClearCachedValue()
			{
				base._ClearCachedValue();
				_value._ClearCachedValue();
				_multiplier._ClearCachedValue();
				_offset._ClearCachedValue();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				_value.Register(appliedAction);
				_multiplier.Register(appliedAction);
				_offset.Register(appliedAction);
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				_value.Unregister();
				_multiplier.Unregister();
				_offset.Unregister();
			}

			protected override int _GetValue(ActionContext context)
			{
				return _value._GetValue(context) * _multiplier._GetValue(context) + _offset._GetValue(context);
			}

			public override string ToString()
			{
				object[] array = new object[4];
				DynamicNumber multiplier = _multiplier;
				array[0] = (multiplier != null && multiplier.constantValue == -1).ToText("-");
				array[1] = _value;
				DynamicNumber multiplier2 = _multiplier;
				object obj;
				if (multiplier2 == null || multiplier2.constantValue != 1)
				{
					DynamicNumber multiplier3 = _multiplier;
					if (multiplier3 == null || multiplier3.constantValue != -1)
					{
						obj = $" * {_multiplier}";
						goto IL_00be;
					}
				}
				obj = "";
				goto IL_00be;
				IL_00be:
				array[2] = obj;
				DynamicNumber offset = _offset;
				array[3] = ((offset == null || offset.constantValue != 0) ? $" + {_offset}" : "");
				return string.Format("({0}{1}{2}{3})", array);
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Returns the minimum value between two Dynamic Numbers.", category = "Math")]
		public class Min : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open, tooltip = "One of the two numbers that the minimum value will be taken from.")]
			[UIDeepValueChange]
			private DynamicNumber _a;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Open, tooltip = "One of the two numbers that the minimum value will be taken from.")]
			[UIDeepValueChange]
			private DynamicNumber _b;

			protected override ActionContextTarget _contextTarget => _a._contextTarget;

			protected override void _ClearCachedValue()
			{
				base._ClearCachedValue();
				_a._ClearCachedValue();
				_b._ClearCachedValue();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				_a.Register(appliedAction);
				_b.Register(appliedAction);
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				_a.Unregister();
				_b.Unregister();
			}

			protected override int _GetValue(ActionContext context)
			{
				return Math.Min(_a._GetValue(context), _b._GetValue(context));
			}

			public override string ToString()
			{
				return $"Min({_a}, {_b})";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Returns the maximum value between two Dynamic Numbers.", category = "Math")]
		public class Max : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open, tooltip = "One of the two numbers that the maximum value will be taken from.")]
			[UIDeepValueChange]
			private DynamicNumber _a;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Open, tooltip = "One of the two numbers that the maximum value will be taken from.")]
			[UIDeepValueChange]
			private DynamicNumber _b;

			protected override ActionContextTarget _contextTarget => _a._contextTarget;

			protected override void _ClearCachedValue()
			{
				base._ClearCachedValue();
				_a._ClearCachedValue();
				_b._ClearCachedValue();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				_a.Register(appliedAction);
				_b.Register(appliedAction);
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				_a.Unregister();
				_b.Unregister();
			}

			protected override int _GetValue(ActionContext context)
			{
				return Math.Max(_a._GetValue(context), _b._GetValue(context));
			}

			public override string ToString()
			{
				return $"Max({_a}, {_b})";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Returns integer division of two dynamic numbers.", category = "Math")]
		public class Divide : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open, tooltip = "Top portion of the fraction")]
			[UIDeepValueChange]
			private DynamicNumber _numerator;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Open, tooltip = "Bottom portion of the fraction.")]
			[UIDeepValueChange]
			private DynamicNumber _denominator;

			protected override ActionContextTarget _contextTarget => _numerator._contextTarget;

			protected override void _ClearCachedValue()
			{
				base._ClearCachedValue();
				_numerator._ClearCachedValue();
				_denominator._ClearCachedValue();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				_numerator.Register(appliedAction);
				_denominator.Register(appliedAction);
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				_numerator.Unregister();
				_denominator.Unregister();
			}

			protected override int _GetValue(ActionContext context)
			{
				return _numerator._GetValue(context) / _denominator._GetValue(context).InsureNonZero();
			}

			public override string ToString()
			{
				return $"({_numerator} / {_denominator})";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Returns a dynamic number to the power of another dynamic number.", category = "Math")]
		public class Power : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open, tooltip = "Value which will be taken to <b>exponent</b> power.")]
			[UIDeepValueChange]
			private DynamicNumber _value;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Open, tooltip = "The power to take <b>value</b> to.")]
			[UIDeepValueChange]
			private DynamicNumber _exponent;

			protected override ActionContextTarget _contextTarget => _value._contextTarget;

			protected override void _ClearCachedValue()
			{
				base._ClearCachedValue();
				_value._ClearCachedValue();
				_exponent._ClearCachedValue();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				_value.Register(appliedAction);
				_exponent.Register(appliedAction);
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				_value.Unregister();
				_exponent.Unregister();
			}

			protected override int _GetValue(ActionContext context)
			{
				return Mathf.RoundToInt(Mathf.Pow(_value._GetValue(context), _exponent._GetValue(context)));
			}

			public override string ToString()
			{
				return $"({_value} ^ {_exponent})";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Returns a random number between two dynamic numbers. (Inclusive)", category = "Math")]
		public class Random : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open, tooltip = "The minimum value that can be returned.")]
			[UIDeepValueChange]
			private DynamicNumber _min;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Open, tooltip = "The maximum value that can be returned.")]
			[UIDeepValueChange]
			private DynamicNumber _max;

			protected override ActionContextTarget _contextTarget => _min._contextTarget;

			protected override void _ClearCachedValue()
			{
				base._ClearCachedValue();
				_min._ClearCachedValue();
				_max._ClearCachedValue();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				_min.Register(appliedAction);
				_max.Register(appliedAction);
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				_min.Unregister();
				_max.Unregister();
			}

			protected override int _GetValue(ActionContext context)
			{
				return context.gameState.random.Next(_min._GetValue(context), _max._GetValue(context) + 1);
			}

			public override string ToString()
			{
				return $"Random[{_min}, {_max}]";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Returns the current round number of an encounter.", category = "Time")]
		public class Round : DynamicNumber
		{
			private void _OnRoundChange(int roundNumber)
			{
				_Reapply();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.onRoundStart += _OnRoundChange;
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.onRoundStart -= _OnRoundChange;
			}

			protected override int _GetValue(ActionContext context)
			{
				return context.gameState.roundNumber;
			}

			public override string ToString()
			{
				return "(Round #)";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Returns the current encounter's number.", category = "Time")]
		public class Encounter : DynamicNumber
		{
			private void _OnEncounterChange(int roundNumber)
			{
				_Reapply();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.onEncounterStart += _OnEncounterChange;
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.onEncounterStart -= _OnEncounterChange;
			}

			protected override int _GetValue(ActionContext context)
			{
				return context.gameState.encounterNumber;
			}

			public override string ToString()
			{
				return "(Encounter #)";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Returns active combat damage data.", category = "Attributes")]
		public class CombatDamage : DynamicNumber
		{
			[ProtoMember(1)]
			[UIField]
			private ActiveCombat.DamageDataType _data;

			protected override int _GetValue(ActionContext context)
			{
				return context.gameState.activeCombat?[_data] ?? 0;
			}

			public override string ToString()
			{
				return "(<size=66%>Combat " + EnumUtil.FriendlyName(_data) + "</size>)";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Returns the number of attacks the player has remaining.", category = "Attributes")]
		public class NumberOfAttacksRemaining : DynamicNumber
		{
			private void _OnNumberOfAttacksRemainingChanged(int previous, int current)
			{
				_Reapply();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.player.numberOfAttacks.onValueChanged += _OnNumberOfAttacksRemainingChanged;
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.player.numberOfAttacks.onValueChanged -= _OnNumberOfAttacksRemainingChanged;
			}

			protected override int _GetValue(ActionContext context)
			{
				return context.gameState.player.numberOfAttacks;
			}

			public override string ToString()
			{
				return "(# of Attacks Remaining)";
			}
		}

		[ProtoContract]
		[UIField(tooltip = "Returns the number of hero abilities the player has remaining.", category = "Attributes")]
		public class NumberOfHeroAbilitiesRemaining : DynamicNumber
		{
			private void _OnNumberOfHeroAbilitiesRemainingChanged(int previous, int current)
			{
				_Reapply();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.player.numberOfHeroAbilities.onValueChanged += _OnNumberOfHeroAbilitiesRemainingChanged;
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				appliedAction.context.gameState.player.numberOfHeroAbilities.onValueChanged -= _OnNumberOfHeroAbilitiesRemainingChanged;
			}

			protected override int _GetValue(ActionContext context)
			{
				return context.gameState.player.numberOfHeroAbilities;
			}

			public override string ToString()
			{
				return "(# of Hero Abilities Remaining)";
			}
		}

		[ProtoContract]
		[UIField(category = "Advanced")]
		public class Conditional : DynamicNumber
		{
			[ProtoMember(1, OverwriteList = true)]
			[UIField]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<Condition> _conditions;

			[ProtoMember(2)]
			[UIField]
			[UIDeepValueChange]
			private DynamicNumber _trueValue;

			[ProtoMember(3)]
			[UIField]
			[UIDeepValueChange]
			private DynamicNumber _falseValue;

			private void _OnConditionDirty(ATarget conditionTarget)
			{
				_Reapply(checkIfValueChanged: true);
			}

			protected override void _ClearCachedValue()
			{
				base._ClearCachedValue();
				_trueValue._ClearCachedValue();
				_falseValue._ClearCachedValue();
			}

			protected override void _Register(AppliedAction appliedAction)
			{
				if (!_conditions.IsNullOrEmpty())
				{
					foreach (Condition condition in _conditions)
					{
						condition.Register(appliedAction.context);
						condition.onDirty = (Action<ATarget>)Delegate.Combine(condition.onDirty, new Action<ATarget>(_OnConditionDirty));
					}
				}
				_trueValue.Register(appliedAction);
				_falseValue.Register(appliedAction);
			}

			protected override void _Unregister(AppliedAction appliedAction)
			{
				if (!_conditions.IsNullOrEmpty())
				{
					foreach (Condition condition in _conditions)
					{
						condition.Unregister(appliedAction.context);
						condition.onDirty = (Action<ATarget>)Delegate.Remove(condition.onDirty, new Action<ATarget>(_OnConditionDirty));
					}
				}
				_trueValue.Unregister();
				_falseValue.Unregister();
			}

			protected override int _GetValue(ActionContext context)
			{
				if (!_conditions.All(context))
				{
					return _falseValue._GetValue(context);
				}
				return _trueValue._GetValue(context);
			}

			public override string ToString()
			{
				return string.Format("({0} ? {1} : {2})", _conditions.ToStringSmart(" & ").SizeIfNotEmpty(), _trueValue, _falseValue);
			}
		}

		[ProtoMember(1)]
		private int? _cachedValue;

		private AppliedAction _appliedAction;

		protected ActionContext _appliedContext => _appliedAction.context;

		public AEntity actor => _appliedContext.actor;

		public ATarget target => _appliedContext.GetTarget<ATarget>(_contextTarget);

		public virtual int? constantValue => null;

		protected virtual ActionContextTarget _contextTarget => ActionContextTarget.Target;

		private void _Reapply(bool checkIfValueChanged = false)
		{
			if (_appliedAction != null && (!checkIfValueChanged || _GetValue(_appliedAction.context) != _cachedValue))
			{
				_appliedAction.Reapply();
			}
		}

		protected abstract int _GetValue(ActionContext context);

		protected virtual void _ClearCachedValue()
		{
			_cachedValue = null;
		}

		protected virtual void _Register(AppliedAction appliedAction)
		{
		}

		protected virtual void _Unregister(AppliedAction appliedAction)
		{
		}

		public virtual AbilityPreventedBy? GetPreventedBy(RangeInt range)
		{
			return null;
		}

		public int GetValue(ActionContext context, bool refreshValue = true)
		{
			if (refreshValue)
			{
				_ClearCachedValue();
			}
			int valueOrDefault = _cachedValue.GetValueOrDefault();
			if (!_cachedValue.HasValue)
			{
				valueOrDefault = _GetValue(context);
				_cachedValue = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}

		public int GetOffset(ActionContext context, int currentValue)
		{
			int? num = (_cachedValue = GetValue(context) - currentValue);
			return num.Value;
		}

		public int GetMultiplier(ActionContext context, int currentValue)
		{
			int? num = (_cachedValue = currentValue * GetValue(context) - currentValue);
			return num.Value;
		}

		public int GetDivider(ActionContext context, int currentValue)
		{
			int value = GetValue(context);
			int? num = (_cachedValue = ((value < 2) ? currentValue : ((currentValue + value - 1) / value)) - currentValue);
			return num.Value;
		}

		public void Register(AppliedAction appliedAction)
		{
			_Register(_appliedAction = appliedAction);
		}

		public void Unregister()
		{
			_Unregister(_appliedAction);
			_appliedAction = null;
		}
	}

	[ProtoContract]
	[UIField]
	[ProtoInclude(5, typeof(Combatant))]
	[ProtoInclude(6, typeof(Resource))]
	[ProtoInclude(7, typeof(AAbility))]
	[ProtoInclude(8, typeof(Player))]
	[ProtoInclude(9, typeof(OwningAbility))]
	[ProtoInclude(10, typeof(TurnOrder))]
	[ProtoInclude(11, typeof(Summon))]
	[ProtoInclude(12, typeof(Stone))]
	[ProtoInclude(13, typeof(ActivatingAbility))]
	[ProtoInclude(14, typeof(TurnOrderRelative))]
	public class Target
	{
		[ProtoContract]
		[UIField]
		[ProtoInclude(8, typeof(TargetPlayer))]
		[ProtoInclude(9, typeof(InCombat))]
		[ProtoInclude(10, typeof(Select))]
		[ProtoInclude(11, typeof(Self))]
		[ProtoInclude(12, typeof(All))]
		[ProtoInclude(13, typeof(Inherit))]
		[ProtoInclude(14, typeof(RelativeToSummon))]
		[ProtoInclude(15, typeof(RandomCombatant))]
		public abstract class Combatant : Target
		{
			[ProtoContract]
			[UIField("Select Combatants", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Allows picking a desired number of combatants one at a time.")]
			public class Select : Combatant
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Hide, tooltip = "Max number of targets that can be selected.")]
				[UIDeepValueChange]
				private DynamicNumber _count;

				[ProtoMember(2, OverwriteList = true)]
				[UIField(tooltip = "Conditions that must hold true for targets.")]
				[UIFieldCollectionItem]
				[UIDeepValueChange]
				private List<Condition.Combatant> _conditions;

				[ProtoMember(3)]
				[UIField(tooltip = "Allegiance relative to owner that combatants must be in order to be targetable.")]
				[UIHorizontalLayout("B", flexibleWidth = 999f)]
				private Allegiance _allegiance;

				[ProtoMember(4)]
				[UIField(tooltip = "Determines if adjacent targets should be included.", validateOnChange = true)]
				[UIHorizontalLayout("B", flexibleWidth = 999f)]
				private Spread _spread;

				[ProtoMember(5)]
				[UIField(tooltip = "Allow targeting the same combatant multiple times.")]
				[UIHorizontalLayout("B", flexibleWidth = 0f)]
				private bool _allowRepeatTargeting;

				[ProtoMember(6)]
				[UIField]
				[UIHideIf("_hideCheckConditionsOn")]
				private CheckConditionsOn _checkConditionsOn;

				public override DynamicNumber count => _count;

				public override bool allowRepeats => _allowRepeatTargeting;

				public override bool canBeRestrictedByReaction => true;

				public override bool requiresUserInput => true;

				public override TargetCountType targetCountType
				{
					get
					{
						TargetCountType? targetCountType = _spread.TargetCount();
						if (!targetCountType.HasValue)
						{
							DynamicNumber dynamicNumber = _count;
							if ((dynamicNumber == null || dynamicNumber.constantValue != 1) && !_allowRepeatTargeting)
							{
								return TargetCountType.MultiTarget;
							}
							return TargetCountType.SingleTarget;
						}
						return targetCountType.GetValueOrDefault();
					}
				}

				public override SelectableTargetType selectableTargetType => SelectableTargetType.Enemy;

				private bool _checkConditionsOnSpecified => !_hideCheckConditionsOn;

				private bool _hideCheckConditionsOn
				{
					get
					{
						if (_spread != 0)
						{
							return _conditions.IsNullOrEmpty();
						}
						return true;
					}
				}

				protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
				{
					PoolKeepItemListHandle<ATarget> poolKeepItemListHandle = Pools.UseKeepItemList<ATarget>();
					foreach (AEntity entity in context.gameState.GetEntities(context.actor, _allegiance.GetAllegiance()))
					{
						if (_conditions.All(context.SetTarget(entity)))
						{
							poolKeepItemListHandle.Add(entity);
						}
					}
					if (context.actor.faction == Faction.Player)
					{
						bool flag = context.gameState.EntityWithStatusExists(StatusType.AbilityGuard, Faction.Enemy);
						bool? flag2 = null;
						for (int num = poolKeepItemListHandle.Count - 1; num >= 0; num--)
						{
							if (!(poolKeepItemListHandle[num] is AEntity aEntity))
							{
								continue;
							}
							if (aEntity.HasStatus(StatusType.AbilityStealth))
							{
								bool valueOrDefault = flag2.GetValueOrDefault();
								bool num2;
								if (!flag2.HasValue)
								{
									valueOrDefault = context.gameState.EntityWithoutStatusExists(StatusType.AbilityStealth, Faction.Enemy);
									flag2 = valueOrDefault;
									num2 = valueOrDefault;
								}
								else
								{
									num2 = valueOrDefault;
								}
								if (num2)
								{
									goto IL_00f8;
								}
							}
							if (!flag || aEntity.HasStatus(StatusType.AbilityGuard))
							{
								continue;
							}
							goto IL_00f8;
							IL_00f8:
							Ability ability = context.ability;
							if (ability == null || !ability.IsTargetOfReaction(aEntity))
							{
								poolKeepItemListHandle.value.RemoveAt(num);
							}
						}
					}
					return poolKeepItemListHandle.AsEnumerable();
				}

				public override void OnInvalidTargetClicked(ATarget target)
				{
					if (!(target is ACombatant aCombatant) || !aCombatant.inTurnOrder)
					{
						return;
					}
					AbilityPreventedBy? abilityPreventedBy = null;
					if (aCombatant.HasStatus(StatusType.AbilityStealth) && aCombatant.gameState.EntityWithoutStatusExists(StatusType.AbilityStealth, aCombatant.faction))
					{
						abilityPreventedBy = AbilityPreventedBy.Stealth;
					}
					else if (!aCombatant.HasStatus(StatusType.AbilityGuard) && aCombatant.gameState.EntityWithStatusExists(StatusType.AbilityGuard, aCombatant.faction))
					{
						abilityPreventedBy = AbilityPreventedBy.Guard;
					}
					if (!abilityPreventedBy.HasValue)
					{
						return;
					}
					AbilityPreventedBy valueOrDefault = abilityPreventedBy.GetValueOrDefault();
					aCombatant.gameState.view.LogError(valueOrDefault.LocalizeError(), aCombatant.gameState.player.audio.character.error[valueOrDefault]);
					switch (valueOrDefault)
					{
					case AbilityPreventedBy.Stealth:
						aCombatant.HighlightStatusTrait(StatusType.AbilityStealth, 3f);
						break;
					case AbilityPreventedBy.Guard:
					{
						foreach (ACombatant item in aCombatant.gameState.GetEntities(aCombatant.faction).OfType<ACombatant>())
						{
							item.HighlightStatusTrait(StatusType.AbilityGuard, 5f);
						}
						break;
					}
					}
				}

				public override GameStepActionTarget GetGameStep(ActionContext context, AAction action)
				{
					return new GameStepActionTargetSelect(action, context, this);
				}

				public override List<ATarget> PostProcessTargets(ActionContext context, List<ATarget> targets)
				{
					return _CheckConditionsOnPostProcess(context, _spread.PostProcessTargets(context, targets, _allegiance), _conditions, _checkConditionsOn);
				}

				public override bool PostProcessesTargets(TargetingContext context)
				{
					return _spread != Spread.None;
				}

				protected override IEnumerable<Condition> _TargetConditions()
				{
					if (_conditions == null)
					{
						yield break;
					}
					foreach (Condition.Combatant condition in _conditions)
					{
						yield return condition;
					}
				}

				protected override IEnumerable<Condition> _PrimaryTargetConditions()
				{
					if (_checkConditionsOn != 0)
					{
						return Enumerable.Empty<Condition>();
					}
					return _TargetConditions();
				}

				public override bool? TargetsEnemy(AbilityData abilityData, AAction action)
				{
					return _allegiance == Allegiance.Enemy;
				}

				public override string ToString()
				{
					string[] array = new string[8];
					DynamicNumber dynamicNumber = _count;
					array[0] = ((dynamicNumber != null && dynamicNumber.constantValue.HasValue) ? _count.ToString() : $"<size=66%>{_count}</size>");
					array[1] = " <size=66%>";
					array[2] = _conditions.ToStringSmart(" & ").SpaceIfNotEmpty();
					array[3] = _checkConditionsOnSpecified.ToText("[" + EnumUtil.FriendlyName(_checkConditionsOn) + "] ");
					array[4] = "</size>";
					array[5] = _allegiance.GetText(_count?.constantValue ?? 2);
					array[6] = ((_spread != 0) ? ("<size=66%> (" + EnumUtil.FriendlyName(_spread) + ")</size>") : "");
					array[7] = _allowRepeatTargeting.ToText("<size=66%> (Repeats)</size>");
					return string.Concat(array);
				}
			}

			[ProtoContract]
			[UIField("Target Self", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Targets the combatant currently acting.")]
			public class Self : Combatant
			{
				public override TargetCountType targetCountType => TargetCountType.SingleTarget;

				protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
				{
					yield return context.actor;
				}

				public override string ToString()
				{
					return "Yourself";
				}
			}

			[ProtoContract]
			[UIField("All Combatants", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Targets all combatants.")]
			public class All : Combatant
			{
				[ProtoMember(1, OverwriteList = true)]
				[UIField(tooltip = "Conditions that must hold true for targets.")]
				[UIFieldCollectionItem]
				[UIDeepValueChange]
				private List<Condition.Combatant> _conditions;

				[ProtoMember(2)]
				[UIField(tooltip = "Allegiance relative to owner that combatants must be in order to be targetable.")]
				[UIHorizontalLayout("B")]
				private Allegiance _allegiance;

				[ProtoMember(3)]
				[UIField(tooltip = "Which piles combatants can be targeted from.", maxCount = 0)]
				[DefaultValue(AdventureCard.Piles.TurnOrder)]
				[UIHorizontalLayout("B")]
				private AdventureCard.Piles _piles = AdventureCard.Piles.TurnOrder;

				private AAction _owningAction;

				private Ability _owningAbility;

				public override bool registers => EnumUtil.HasAllFlags(_piles);

				public override Action<AAction, ATarget> onTargetAdded { get; set; }

				public override TargetCountType targetCountType => TargetCountType.MultiTarget;

				private void _OnCombatantAdded(ACombatant combatant)
				{
					ActionContext context = new ActionContext(_owningAbility.owner, _owningAbility, combatant);
					if ((_allegiance == Allegiance.Any || context.actor.GetAllegiance(combatant) == _allegiance.GetAllegiance()) && _conditions.All(context))
					{
						onTargetAdded?.Invoke(_owningAction, combatant);
					}
				}

				public override void Register(ActionContext context, AAction action, Ability ability)
				{
					_owningAction = action;
					_owningAbility = ability;
					context.gameState.onCombatantAdded += _OnCombatantAdded;
				}

				public override void Unregister(ActionContext context)
				{
					context.gameState.onCombatantAdded -= _OnCombatantAdded;
					_owningAction = null;
					_owningAbility = null;
				}

				protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
				{
					foreach (ATarget card in context.gameState.adventureDeck.GetCards(_piles))
					{
						if (card is ACombatant otherEntity && (_allegiance == Allegiance.Any || context.actor.GetAllegiance(otherEntity) == _allegiance.GetAllegiance()) && _conditions.All(context.SetTarget(card)))
						{
							yield return card;
						}
					}
				}

				protected override IEnumerable<Condition> _TargetConditions()
				{
					if (_conditions == null)
					{
						yield break;
					}
					foreach (Condition.Combatant condition in _conditions)
					{
						yield return condition;
					}
				}

				public override bool? TargetsEnemy(AbilityData abilityData, AAction action)
				{
					return _allegiance == Allegiance.Enemy;
				}

				public override string ToString()
				{
					return "All <size=66%>" + _conditions.ToStringSmart(" & ").SpaceIfNotEmpty() + "</size>" + _allegiance.GetText(2) + (_piles != AdventureCard.Piles.TurnOrder).ToText("<size=66%> In " + _piles.GetText() + "</size>");
				}
			}

			[ProtoContract]
			[UIField("Inherit From Previous Combatant Action", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Uses targets from most recently executed combatant action belonging to the same ability activation.\n<i>This is usually the action directly above this one in the action list.</i>")]
			public class Inherit : Combatant
			{
				[ProtoMember(1)]
				[UIField(tooltip = "Determines how inherited combatant targets will be post processed into final target list.", validateOnChange = true)]
				[UIHorizontalLayout("B")]
				private Spread _spread;

				[ProtoMember(2)]
				[UIField(tooltip = "Allegiance relative to owner that combatants must be in order to be targetable by <i>Spread</i>.")]
				[UIHorizontalLayout("B")]
				[UIHideIf("_hideAllegiance")]
				private Allegiance _allegiance;

				[ProtoMember(3, OverwriteList = true)]
				[UIField(tooltip = "Conditions that must hold true for targets.")]
				[UIFieldCollectionItem]
				[UIDeepValueChange]
				private List<Condition.Combatant> _conditions;

				[ProtoMember(4)]
				[UIField]
				[UIHideIf("_hideCheckConditionsOn")]
				private CheckConditionsOn _checkConditionsOn;

				private TargetCountType _inheritedTargetCountType;

				public Allegiance allegiance
				{
					get
					{
						return _allegiance;
					}
					set
					{
						_allegiance = value;
					}
				}

				public override bool inheritsTargets => true;

				public override TargetCountType targetCountType
				{
					get
					{
						return _spread.TargetCount() ?? _inheritedTargetCountType;
					}
					set
					{
						_inheritedTargetCountType = value;
					}
				}

				private bool _hideAllegiance => !_spread.UsesAllegiance();

				private bool _checkConditionsOnSpecified => !_hideCheckConditionsOn;

				private bool _hideCheckConditionsOn
				{
					get
					{
						if (_spread != 0)
						{
							return _conditions.IsNullOrEmpty();
						}
						return true;
					}
				}

				public override GameStepActionTarget GetGameStep(ActionContext context, AAction action)
				{
					return new GameStepActionTargetInherit<Combatant>(action, context, this);
				}

				public override List<ATarget> PostProcessTargets(ActionContext context, List<ATarget> targets)
				{
					return _CheckConditionsOnPostProcess(context, _spread.PostProcessTargets(context, targets, _allegiance), _conditions, _checkConditionsOn);
				}

				public override bool PostProcessesTargets(TargetingContext context)
				{
					return _spread != Spread.None;
				}

				protected override IEnumerable<Condition> _TargetConditions()
				{
					if (_conditions == null)
					{
						yield break;
					}
					foreach (Condition.Combatant condition in _conditions)
					{
						yield return condition;
					}
				}

				protected override IEnumerable<Condition> _PrimaryTargetConditions()
				{
					if (_checkConditionsOn != 0)
					{
						return Enumerable.Empty<Condition>();
					}
					return _TargetConditions();
				}

				public override bool? TargetsEnemy(AbilityData abilityData, AAction action)
				{
					if (!_hideAllegiance)
					{
						return _allegiance == Allegiance.Enemy;
					}
					for (int num = abilityData.actions.IndexOf(action) - 1; num >= 0; num--)
					{
						AAction aAction = abilityData.actions[num];
						if (aAction != null)
						{
							Target target = aAction.target;
							if (target is Combatant combatant && !target.inheritsTargets)
							{
								return combatant.TargetsEnemy(abilityData, aAction);
							}
						}
					}
					return abilityData.type == AbilityData.Type.Debuff;
				}

				public override string ToString()
				{
					return "Recently Targeted " + _conditions.ToStringSmart(" & ").AppendIfNotEmpty(_checkConditionsOnSpecified.ToText(" [" + EnumUtil.FriendlyName(_checkConditionsOn) + "]")).SizeIfNotEmpty()
						.SpaceIfNotEmpty() + "Combatants" + ((_spread != 0) ? ("<size=66%> (" + EnumUtil.FriendlyName(_spread) + ((!_hideAllegiance) ? _allegiance.GetText(2) : "").PreSpaceIfNotEmpty() + ")</size>") : "");
				}
			}

			[ProtoContract]
			[UIField("Combatants Relative To Summon", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Targets combatants relative to player's active summon position.")]
			public class RelativeToSummon : Combatant
			{
				[ProtoMember(1)]
				[UIField(tooltip = "Determines which targets relative to active summon position will be targeted", excludedValuesMethod = "_ExcludeSpread")]
				[DefaultValue(Spread.AdjacentToOnly)]
				private Spread _spread = Spread.AdjacentToOnly;

				[ProtoMember(2)]
				[UIField(tooltip = "Allegiance relative to owner that combatants must be in order to be targetable.")]
				[UIHorizontalLayout("B")]
				private Allegiance _allegiance;

				[ProtoMember(3, OverwriteList = true)]
				[UIField(tooltip = "Conditions that must hold true for targets.")]
				[UIFieldCollectionItem]
				[UIDeepValueChange]
				private List<Condition.Combatant> _conditions;

				public override TargetCountType targetCountType => TargetCountType.MultiTarget;

				protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
				{
					Ability activeSummon = context.gameState.activeSummon;
					if (activeSummon == null)
					{
						yield break;
					}
					using PoolKeepItemListHandle<ATarget> summonTargets = Pools.UseKeepItemList((ATarget)activeSummon);
					_spread.PostProcessTargets(context, summonTargets, _allegiance);
					foreach (ATarget item in summonTargets.value)
					{
						if (item is ACombatant && (_conditions.IsNullOrEmpty() || _conditions.All(context.SetTarget(item))))
						{
							yield return item;
						}
					}
				}

				protected override IEnumerable<Condition> _TargetConditions()
				{
					if (_conditions == null)
					{
						yield break;
					}
					foreach (Condition.Combatant condition in _conditions)
					{
						yield return condition;
					}
				}

				public override bool? TargetsEnemy(AbilityData abilityData, AAction action)
				{
					return _allegiance == Allegiance.Enemy;
				}

				public override string ToString()
				{
					return _conditions?.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + EnumUtil.FriendlyName(_allegiance) + " Combatants that are " + EnumUtil.FriendlyName(_spread, uppercase: false).RemoveFromEnd(5) + " Summon";
				}

				private bool _ExcludeSpread(Spread spread)
				{
					if (spread != Spread.AdjacentToOnly && spread != Spread.LeftOfOnly)
					{
						return spread != Spread.RightOfOnly;
					}
					return false;
				}
			}

			[ProtoContract]
			[UIField("Random Combatant", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Randomly selects a number of combatants.")]
			public class RandomCombatant : Combatant
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Hide, tooltip = "Max number of targets that can be selected.")]
				[UIDeepValueChange]
				private DynamicNumber _count;

				[ProtoMember(2, OverwriteList = true)]
				[UIField(tooltip = "Conditions that must hold true for targets.")]
				[UIFieldCollectionItem]
				[UIDeepValueChange]
				private List<Condition.Combatant> _conditions;

				[ProtoMember(3)]
				[UIField(tooltip = "Allegiance relative to owner that combatants must be in order to be targetable.")]
				[UIHorizontalLayout("B")]
				private Allegiance _allegiance;

				[ProtoMember(4)]
				[UIField(tooltip = "Which piles combatants can be targeted from.", maxCount = 0)]
				[DefaultValue(AdventureCard.Piles.TurnOrder)]
				[UIHorizontalLayout("B")]
				private AdventureCard.Piles _piles = AdventureCard.Piles.TurnOrder;

				[ProtoMember(5)]
				[UIField]
				private bool _allowRepeatTargeting;

				public override bool allowRepeats => _allowRepeatTargeting;

				public override bool isRandom => true;

				public override TargetCountType targetCountType
				{
					get
					{
						DynamicNumber dynamicNumber = _count;
						if ((dynamicNumber == null || dynamicNumber.constantValue != 1) && !_allowRepeatTargeting)
						{
							return TargetCountType.MultiTarget;
						}
						return TargetCountType.SingleTarget;
					}
				}

				protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
				{
					foreach (ATarget card in context.gameState.adventureDeck.GetCards(_piles))
					{
						if (card is ACombatant otherEntity && (_allegiance == Allegiance.Any || context.actor.GetAllegiance(otherEntity) == _allegiance.GetAllegiance()) && _conditions.All(context.SetTarget(card)))
						{
							yield return card;
						}
					}
				}

				protected override IEnumerable<Condition> _TargetConditions()
				{
					if (_conditions == null)
					{
						yield break;
					}
					foreach (Condition.Combatant condition in _conditions)
					{
						yield return condition;
					}
				}

				public override bool? TargetsEnemy(AbilityData abilityData, AAction action)
				{
					return _allegiance == Allegiance.Enemy;
				}

				public override bool PostProcessesTargets(TargetingContext context)
				{
					return context != TargetingContext.ShowAvailableTargets;
				}

				public override List<ATarget> PostProcessTargets(ActionContext context, List<ATarget> targets)
				{
					if (_allowRepeatTargeting)
					{
						using (PoolKeepItemListHandle<ATarget> poolKeepItemListHandle = Pools.UseKeepItemList(targets))
						{
							targets.Clear();
							for (int num = _count.GetValue(context); num > 0; num--)
							{
								targets.Add(context.gameState.random.Item(poolKeepItemListHandle.value));
							}
							return targets;
						}
					}
					for (int num2 = targets.Count - _count.GetValue(context); num2 > 0; num2--)
					{
						targets.RemoveAt(context.gameState.random.Next(targets.Count));
					}
					return targets;
				}

				public override string ToString()
				{
					return string.Format("{0} Random <size=66%>{1}</size>{2}", _count, _conditions.ToStringSmart(" & ").SpaceIfNotEmpty(), _allegiance.GetText(_count?.constantValue ?? 2)) + (_piles != AdventureCard.Piles.TurnOrder).ToText("<size=66%> In " + EnumUtil.FriendlyName(_piles) + "</size>") + _allowRepeatTargeting.ToText(" (Repeats)").SizeIfNotEmpty();
				}
			}

			[ProtoContract]
			[UIField("Enemy Which Is In Active Combat", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Targets enemy which is currently attacking or defending.")]
			public class InCombat : Combatant
			{
				[ProtoMember(1, OverwriteList = true)]
				[UIField(tooltip = "Conditions that must hold true for enemy combatant.")]
				[UIFieldCollectionItem]
				[UIDeepValueChange]
				private List<Condition.Combatant> _conditions;

				public override TargetCountType targetCountType => TargetCountType.SingleTarget;

				protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
				{
					ACombatant aCombatant = context.gameState.activeCombat?.enemyCombatant;
					if (aCombatant != null && _conditions.All(context.SetTarget(aCombatant)))
					{
						yield return aCombatant;
					}
				}

				public override string ToString()
				{
					return _conditions.ToStringSmart(" & ").SpaceIfNotEmpty().SizeIfNotEmpty() + "Enemy In Active Combat";
				}
			}

			[ProtoContract]
			[UIField("Player", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Targets player.")]
			public class TargetPlayer : Combatant
			{
				public override TargetCountType targetCountType => TargetCountType.SingleTarget;

				protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
				{
					yield return context.gameState.player;
				}

				public override string ToString()
				{
					return "Player";
				}
			}

			[ProtoContract(EnumPassthru = true)]
			public enum Spread
			{
				[UITooltip("Affect selected target only.")]
				None,
				[UITooltip("Affect selected target & adjacent combatants.")]
				IncludeAdjacentTo,
				[UITooltip("Affect combatants adjacent to selected target only.")]
				AdjacentToOnly,
				[UITooltip("Affect selected target & all combatants left of it.")]
				IncludeLeftOf,
				[UITooltip("Affect combatants that are left of the target only.")]
				LeftOfOnly,
				[UITooltip("Affect selected target & all combatants right of it.")]
				IncludeRightOf,
				[UITooltip("Affect combatants that are right of the target only.")]
				RightOfOnly,
				[UITooltip("Affect selected target & actor.")]
				IncludeSelf,
				[UITooltip("Affect actor only.")]
				Self,
				[UITooltip("Affect all combatants except the target.")]
				AllButTarget
			}

			[ProtoContract(EnumPassthru = true)]
			public enum Allegiance
			{
				Enemy,
				Ally,
				Any
			}

			public override Type targetType => typeof(ACombatant);

			public virtual bool? TargetsEnemy(AbilityData abilityData, AAction action)
			{
				return false;
			}
		}

		[ProtoContract]
		[UIField]
		[ProtoInclude(10, typeof(Common))]
		[ProtoInclude(11, typeof(Inherit))]
		public abstract class Resource : Target
		{
			[ProtoContract]
			[UIField]
			[ProtoInclude(10, typeof(Select))]
			[ProtoInclude(11, typeof(All))]
			public abstract class Common : Resource
			{
				[ProtoContract]
				[UIField("Select Resource Cards", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Allows picking a desired number of Resource Cards on at a time.", order = 1u)]
				public class Select : Common
				{
					[ProtoMember(1)]
					[UIField(collapse = UICollapseType.Open, tooltip = "The number of cards that can be targeted.")]
					[UIDeepValueChange]
					private DynamicNumber _count;

					[ProtoMember(2)]
					[UIField(tooltip = "Allow targeting the same card multiple times.")]
					[UIHorizontalLayout("Bool", expandWidth = false)]
					private bool _allowRepeatTargeting;

					public override DynamicNumber count => _count;

					public override bool allowRepeats => _allowRepeatTargeting;

					public override bool requiresUserInput => true;

					public override SelectableTargetType selectableTargetType
					{
						get
						{
							if (_piles == (ResourceCard.Piles)0)
							{
								return SelectableTargetType.EnemyResourceCard;
							}
							return SelectableTargetType.ResourceCard;
						}
					}

					public override GameStepActionTarget GetGameStep(ActionContext context, AAction action)
					{
						return new GameStepActionTargetSelect(action, context, this);
					}

					public override string ToString()
					{
						DynamicNumber dynamicNumber = _count;
						return ((dynamicNumber != null && dynamicNumber.constantValue.HasValue) ? _count.ToString() : $"<size=66%>{_count}</size>") + " " + _ToString(_count?.constantValue) + _allowRepeatTargeting.ToText("<size=66%> (Repeats)</size>");
					}

					public override void OnEnable(GameStepActionTarget step)
					{
						if (!step.context.gameState.encounterActive)
						{
							step.context.gameState.view.wildPiles = _piles;
						}
						if (((ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand) & _opponentPiles) == 0)
						{
							return;
						}
						foreach (CombatantCardView item in step.view.adventureDeckLayout.turnOrder.gameObject.GetComponentsInChildrenPooled<CombatantCardView>())
						{
							foreach (AbilityCardView item2 in item.gameObject.GetComponentsInChildrenPooled<AbilityCardView>())
							{
								item2.pointerOver.enabled = false;
							}
						}
					}

					public override void OnDisable(GameStepActionTarget step)
					{
						if (((ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand) & _opponentPiles) == 0)
						{
							return;
						}
						foreach (CombatantCardView item in step.view.adventureDeckLayout.turnOrder.gameObject.GetComponentsInChildrenPooled<CombatantCardView>())
						{
							foreach (AbilityCardView item2 in item.gameObject.GetComponentsInChildrenPooled<AbilityCardView>())
							{
								item2.pointerOver.enabled = true;
							}
						}
					}
				}

				[ProtoContract]
				[UIField("All Resource Cards", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Targets all Resource Cards in specified card piles.", order = 2u)]
				public class All : Common
				{
					public override string ToString()
					{
						return "All " + _ToString();
					}
				}

				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Hide, order = 1u, validateOnChange = true)]
				protected PlayingCard.Filter _filter;

				[ProtoMember(2)]
				[UIField(tooltip = "Which card piles belonging to the actor to allow targeting cards from.", order = 2u, maxCount = 0)]
				[DefaultValue(ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand)]
				[UIHorizontalLayout("Pile")]
				protected ResourceCard.Piles _piles = ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand;

				[ProtoMember(3)]
				[UIField(tooltip = "Which card piles belonging to the opponent of this action to allow targeting cards from.", order = 3u, maxCount = 0)]
				[UIHorizontalLayout("Pile")]
				protected ResourceCard.Piles _opponentPiles;

				[ProtoMember(4)]
				[UIField(tooltip = "Filter is applied to the natural value of a card, instead of all possible values it can become.")]
				[UIHideIf("_hideNaturalValuesOnly")]
				[UIHorizontalLayout("Bool")]
				protected bool _naturalValuesOnly;

				[ProtoMember(5, OverwriteList = true)]
				[UIField(tooltip = "Conditions that must be true for card in order to be targeted.")]
				[UIFieldCollectionItem]
				[UIDeepValueChange]
				protected List<Condition.AResource> _conditions;

				protected bool _hideNaturalValuesOnly => !_filter;

				protected string _ToString(int? count = null)
				{
					return _naturalValuesOnly.ToText("(Natural) ".SizeIfNotEmpty()) + _conditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + (_filter ? _filter.ToString() : "card").Pluralize(count ?? 2) + " in" + (_piles != (ResourceCard.Piles)0).ToText("<size=66%> " + _piles.ToText() + "</size>") + (_opponentPiles != (ResourceCard.Piles)0).ToText("<size=66%> " + ((_piles != 0) ? "& " : "") + "opponent's " + _opponentPiles.ToText() + "</size>");
				}

				private bool _IsValid(ActionContext context, ResourceCard card)
				{
					bool num;
					if (!_naturalValuesOnly)
					{
						num = _filter.AreValid(card);
					}
					else
					{
						if (!_filter.IsValid(card.naturalValue.type))
						{
							goto IL_0055;
						}
						num = !card.isJoker;
					}
					if (num)
					{
						return _conditions.All(context.SetTarget(card));
					}
					goto IL_0055;
					IL_0055:
					return false;
				}

				protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
				{
					foreach (ResourceCard card in context.gameState.player.resourceDeck.GetCards(_piles))
					{
						if (_IsValid(context, card))
						{
							yield return card;
						}
					}
					foreach (ResourceCard item in (EnumUtil.HasFlags(ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand, _opponentPiles) && EnumUtil.HasFlags(_opponentPiles, ResourceCard.Piles.Hand) && EnumUtil.FlagsIntersect(_opponentPiles, ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand)) ? context.gameState.view.enemyResourceDeckLayout.GetCardsInLayout(ResourceCard.Pile.Hand) : context.gameState.enemyResourceDeck.GetCards(_opponentPiles))
					{
						if (_IsValid(context, item))
						{
							yield return item;
						}
					}
				}
			}

			[ProtoContract]
			[UIField("Inherit From Previous Resource Card Action", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Uses targets from most recently executed resource card action belonging to the same ability activation.\n<i>This is usually the action directly above this one in the action list.</i>", order = 100u)]
			public class Inherit : Resource
			{
				[ProtoMember(1, OverwriteList = true)]
				[UIField(tooltip = "Conditions that must be true for card in order to be targeted.")]
				[UIFieldCollectionItem]
				[UIDeepValueChange]
				protected List<Condition.AResource> _conditions;

				public override bool inheritsTargets => true;

				public override TargetCountType targetCountType { get; set; }

				public override GameStepActionTarget GetGameStep(ActionContext context, AAction action)
				{
					return new GameStepActionTargetInherit<Resource>(action, context, this);
				}

				protected override IEnumerable<Condition> _TargetConditions()
				{
					if (_conditions.IsNullOrEmpty())
					{
						yield break;
					}
					foreach (Condition.AResource condition in _conditions)
					{
						yield return condition;
					}
				}

				public override string ToString()
				{
					return "Recently Targeted " + _conditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + "Cards";
				}
			}

			public override Type targetType => typeof(ResourceCard);
		}

		[ProtoContract]
		[UIField]
		[ProtoInclude(10, typeof(Common))]
		[ProtoInclude(11, typeof(Inherit))]
		[ProtoInclude(12, typeof(This))]
		public abstract class AAbility : Target
		{
			[ProtoContract]
			[UIField]
			[ProtoInclude(10, typeof(Select))]
			[ProtoInclude(11, typeof(All))]
			public abstract class Common : AAbility
			{
				[ProtoContract]
				[UIField("Select Ability Cards", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Allows picking a desired number of Ability Cards from the player's hand.", order = 1u)]
				public class Select : Common
				{
					[ProtoMember(1)]
					[UIField(collapse = UICollapseType.Open, tooltip = "The number of cards that can be targeted.")]
					[UIDeepValueChange]
					private DynamicNumber _count;

					[ProtoMember(2)]
					[UIField(tooltip = "Allow targeting the same card multiple times.")]
					private bool _allowRepeatTargeting;

					public override DynamicNumber count => _count;

					public override bool allowRepeats => _allowRepeatTargeting;

					public override bool requiresUserInput => true;

					public override SelectableTargetType selectableTargetType => SelectableTargetType.Ability;

					public override GameStepActionTarget GetGameStep(ActionContext context, AAction action)
					{
						return new GameStepActionTargetSelect(action, context, this);
					}

					public override string ToString()
					{
						string[] array = new string[9];
						DynamicNumber dynamicNumber = _count;
						array[0] = ((dynamicNumber != null && dynamicNumber.constantValue.HasValue) ? _count.ToString() : $"<size=66%>{_count}</size>");
						array[1] = " <size=66%>";
						array[2] = _conditions.ToStringSmart(" & ").SpaceIfNotEmpty();
						array[3] = "</size>ability ";
						array[4] = "card".Pluralize(_count?.constantValue ?? 2);
						array[5] = _allowRepeatTargeting.ToText("<size=66%> (Repeats)</size>");
						array[6] = "<size=66%> In ";
						array[7] = EnumUtil.FriendlyName(_piles);
						array[8] = "</size>";
						return string.Concat(array);
					}
				}

				[ProtoContract]
				[UIField("All Ability Cards", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Targets all of the Ability Cards in specified piles.", order = 2u)]
				public class All : Common
				{
					[ProtoMember(1)]
					[UIField(maxCount = 0)]
					private AdventureCard.Piles _adventurePiles;

					private AAction _owningAction;

					public override bool registers
					{
						get
						{
							if (EnumUtil.HasAllFlags(_adventurePiles))
							{
								return EnumUtil.HasAllFlags(_piles);
							}
							return false;
						}
					}

					public override Action<AAction, ATarget> onTargetAdded { get; set; }

					private void _OnAbilityAdded(Ability ability)
					{
						if (_conditions.All<Condition.AAbility>(new ActionContext(ability, ability, ability)))
						{
							GameState gameState = ability.gameState;
							if (gameState == null || !gameState.parameters.adventureEnded)
							{
								onTargetAdded?.Invoke(_owningAction, ability);
							}
						}
					}

					public override void Register(ActionContext context, AAction action, Ability ability)
					{
						_owningAction = action;
						context.gameState.onAbilityAdded += _OnAbilityAdded;
					}

					public override void Unregister(ActionContext context)
					{
						context.gameState.onAbilityAdded -= _OnAbilityAdded;
						_owningAction = null;
					}

					protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
					{
						foreach (ATarget item in base._GetTargetable(context))
						{
							yield return item;
						}
						if (EnumUtil.FlagCount(_adventurePiles) != 0)
						{
							foreach (ATarget card in context.gameState.adventureDeck.GetCards(_adventurePiles))
							{
								if (card is Ability ability && _conditions.All<Condition.AAbility>(context.SetTarget(ability)))
								{
									yield return ability;
								}
							}
						}
						if (!EnumUtil.HasFlag(_adventurePiles, AdventureCard.Piles.TurnOrder))
						{
							yield break;
						}
						foreach (AEntity item2 in context.gameState.turnOrderQueue)
						{
							if (!(item2 is ACombatant aCombatant))
							{
								continue;
							}
							foreach (Ability card2 in aCombatant.appliedAbilities.GetCards())
							{
								if (_conditions.All<Condition.AAbility>(context.SetTarget(card2)))
								{
									yield return card2;
								}
							}
						}
					}

					public override string ToString()
					{
						return "All <size=66%>" + _conditions.ToStringSmart(" & ").SpaceIfNotEmpty() + "</size>ability cards<size=66%> In " + EnumUtil.FriendlyName(_piles) + (EnumUtil.FlagCount(_adventurePiles) != 0).ToText(" & Adventure " + EnumUtil.FriendlyName(_adventurePiles)) + "</size>";
					}
				}

				[ProtoMember(1)]
				[UIField(tooltip = "Conditions that must be met for an Ability Card to be targetable.", order = 1u)]
				[UIFieldCollectionItem]
				[UIDeepValueChange]
				protected List<Condition.AAbility> _conditions;

				[ProtoMember(2)]
				[UIField(tooltip = "Which card piles belonging to the actor to allow targeting cards from.", maxCount = 0, order = 2u)]
				[DefaultValue(Ability.Piles.Hand)]
				protected Ability.Piles _piles = Ability.Piles.Hand;

				protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
				{
					foreach (Ability card in context.gameState.player.abilityDeck.GetCards(_piles))
					{
						if (card != context.ability && _conditions.All<Condition.AAbility>(context.SetTarget(card)))
						{
							yield return card;
						}
					}
				}

				protected override IEnumerable<Condition> _TargetConditions()
				{
					if (_conditions == null)
					{
						yield break;
					}
					foreach (Condition.AAbility condition in _conditions)
					{
						yield return condition;
					}
				}
			}

			[ProtoContract]
			[UIField("Inherit From Previous Ability Card Action", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Uses targets from most recently executed ability card action belonging to the same ability activation.\n<i>This is usually the action directly above this one in the action list.</i>", order = 100u)]
			public class Inherit : AAbility
			{
				public override bool inheritsTargets => true;

				public override TargetCountType targetCountType { get; set; }

				public override GameStepActionTarget GetGameStep(ActionContext context, AAction action)
				{
					return new GameStepActionTargetInherit<AAbility>(action, context, this);
				}

				public override string ToString()
				{
					return "Recently Targeted Ability Cards";
				}
			}

			[ProtoContract]
			[UIField("This Ability", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Targets this ability.")]
			public class This : AAbility
			{
				public override TargetCountType targetCountType => TargetCountType.SingleTarget;

				protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
				{
					yield return context.ability;
				}

				public override string ToString()
				{
					return "This Ability";
				}
			}

			public class Set : AAbility
			{
				private readonly List<Ability> _targets;

				public override TargetCountType targetCountType
				{
					get
					{
						List<Ability> targets = _targets;
						if (targets == null || targets.Count <= 1)
						{
							return TargetCountType.SingleTarget;
						}
						return TargetCountType.MultiTarget;
					}
					set
					{
					}
				}

				public Set(params Ability[] targets)
				{
					_targets = new List<Ability>(targets);
				}

				protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
				{
					return _targets;
				}
			}

			public override Type targetType => typeof(Ability);
		}

		[ProtoContract]
		[UIField]
		public class Player : Target
		{
			[ProtoMember(1, OverwriteList = true)]
			[UIField(tooltip = "Conditions that must hold true for player.")]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<Condition.Actor> _conditions;

			protected override IEnumerable<Condition> _TargetConditions()
			{
				if (_conditions == null)
				{
					yield break;
				}
				foreach (Condition.Actor condition in _conditions)
				{
					yield return condition;
				}
			}

			protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
			{
				if (_conditions.All(context.SetTarget(context.gameState.player)))
				{
					yield return context.gameState.player;
				}
			}

			public override string ToString()
			{
				return _conditions.ToStringSmart(" & ").SpaceIfNotEmpty().SizeIfNotEmpty() + "player";
			}

			public static implicit operator bool(Player player)
			{
				if (player != null)
				{
					return !player._conditions.IsNullOrEmpty();
				}
				return false;
			}
		}

		[ProtoContract]
		[UIField]
		public class OwningAbility : Target
		{
			protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
			{
				yield return context.ability;
			}

			public override string ToString()
			{
				return "owning ability";
			}
		}

		[ProtoContract]
		[UIField]
		public class Summon : Target
		{
			protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
			{
				Ability activeSummon = context.gameState.activeSummon;
				if (activeSummon != null)
				{
					yield return activeSummon;
				}
			}

			public override string ToString()
			{
				return "summon";
			}
		}

		[ProtoContract]
		[UIField]
		public class TurnOrder : Target
		{
			private static readonly RangeByte DISTANCE = new RangeByte(0, 10, 0, 10, 0, 0);

			private static readonly DynamicNumber COUNT = new DynamicNumber.Constant();

			[ProtoMember(1)]
			[UIField]
			[UIHideIf("_hideDistance")]
			[UIHorizontalLayout("A", preferredWidth = 1f, flexibleWidth = 999f)]
			private RangeByte _distance = DISTANCE;

			[ProtoMember(2)]
			[UIField(order = 1u, validateOnChange = true, tooltip = "Use recently targeted space.")]
			[UIHorizontalLayout("A", preferredWidth = 1f, flexibleWidth = 1f, minWidth = 130f)]
			private bool _inherit;

			public override DynamicNumber count => COUNT;

			public override bool inheritsTargets => _inherit;

			public override bool requiresUserInput => !inheritsTargets;

			public override SelectableTargetType selectableTargetType => SelectableTargetType.TurnOrderSpace;

			private bool _hideDistance => _inherit;

			private bool _distanceSpecified => _distance != DISTANCE;

			public override GameStepActionTarget GetGameStep(ActionContext context, AAction action)
			{
				if (!_inherit)
				{
					return new GameStepActionTargetSelect(action, context, this);
				}
				return new GameStepActionTargetInherit<TurnOrder>(action, context, this);
			}

			protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
			{
				foreach (TurnOrderSpace card in context.gameState.turnOrderSpaceDeck.GetCards(TurnOrderSpace.Pile.Active))
				{
					if (_distance.InRangeSmart(card.DistanceTo(context.actor)))
					{
						yield return card;
					}
				}
			}

			public override string ToString()
			{
				if (!_inherit)
				{
					return "space" + _distanceSpecified.ToText(" " + _distance.ToRangeString(null, "", 50) + " distance away");
				}
				return "recently targeted space";
			}
		}

		[ProtoContract]
		[UIField]
		public class Stone : Target
		{
			[ProtoMember(1)]
			[UIField("Stone Type", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
			private StoneType _type;

			protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
			{
				foreach (global::Stone card in context.gameState.stoneDeck.GetCards())
				{
					if (card.type == _type)
					{
						yield return card;
					}
				}
			}

			public override string ToString()
			{
				return EnumUtil.FriendlyName(_type) + " stone";
			}
		}

		[ProtoContract]
		[UIField]
		public class ActivatingAbility : Target
		{
			protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
			{
				foreach (Ability card in context.gameState.player.abilityDeck.GetCards(Ability.Pile.ActivationHand))
				{
					yield return card;
				}
			}

			public override string ToString()
			{
				return "activating ability";
			}
		}

		[ProtoContract]
		[UIField]
		public class TurnOrderRelative : Target
		{
			[ProtoContract(EnumPassthru = true)]
			public enum PositionType
			{
				Behind,
				InFrontOf
			}

			[ProtoMember(1)]
			[UIField]
			private PositionType _position;

			[ProtoMember(2)]
			[UIField]
			private ActionContextTarget? _relativeTo;

			protected override IEnumerable<ATarget> _GetTargetable(ActionContext context)
			{
				AEntity aEntity = (_relativeTo.HasValue ? context.GetTarget<AEntity>(_relativeTo.Value) : null);
				if (aEntity == null)
				{
					if (!_relativeTo.HasValue)
					{
						yield return (_position == PositionType.Behind) ? context.gameState.turnOrderSpaceDeck.NextInPile(TurnOrderSpace.Pile.Active) : context.gameState.turnOrderSpaceDeck.FirstInPile(TurnOrderSpace.Pile.Active);
					}
				}
				else
				{
					yield return context.gameState.turnOrderSpaceDeck[TurnOrderSpace.Pile.Active, aEntity.turnOrder + (_position == PositionType.Behind).ToInt()];
				}
			}

			public override string ToString()
			{
				return EnumUtil.FriendlyName(_position) + " " + (_relativeTo.HasValue ? EnumUtil.FriendlyName(_relativeTo) : "Turn Order");
			}
		}

		[ProtoContract(EnumPassthru = true)]
		public enum CheckConditionsOn
		{
			Primary,
			All
		}

		public virtual DynamicNumber count => null;

		public virtual bool allowRepeats => false;

		public virtual bool canBeRestrictedByReaction => false;

		public virtual bool requiresUserInput => false;

		public virtual bool isRandom => false;

		public virtual bool registers => false;

		public virtual Action<AAction, ATarget> onTargetAdded
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public virtual TargetCountType targetCountType
		{
			get
			{
				return TargetCountType.SingleTarget;
			}
			set
			{
			}
		}

		public virtual SelectableTargetType selectableTargetType => SelectableTargetType.Target;

		public virtual Type targetType => typeof(ATarget);

		public virtual bool inheritsTargets => false;

		protected virtual IEnumerable<ATarget> _GetTargetable(ActionContext context)
		{
			yield return null;
		}

		public virtual void Register(ActionContext context, AAction action, Ability ability)
		{
		}

		public virtual void Unregister(ActionContext context)
		{
		}

		public virtual GameStepActionTarget GetGameStep(ActionContext context, AAction action)
		{
			return new GameStepActionTarget(action, context, this);
		}

		public virtual bool PostProcessesTargets(TargetingContext context)
		{
			return false;
		}

		public virtual List<ATarget> PostProcessTargets(ActionContext context, List<ATarget> targets)
		{
			return targets;
		}

		protected virtual IEnumerable<Condition> _TargetConditions()
		{
			yield break;
		}

		protected virtual IEnumerable<Condition> _PrimaryTargetConditions()
		{
			return _TargetConditions();
		}

		public virtual void OnEnable(GameStepActionTarget step)
		{
		}

		public virtual void OnDisable(GameStepActionTarget step)
		{
		}

		public virtual void OnInvalidTargetClicked(ATarget target)
		{
		}

		protected List<ATarget> _CheckConditionsOnPostProcess<C>(ActionContext context, List<ATarget> targets, List<C> conditions, CheckConditionsOn checkConditionsOn) where C : Condition
		{
			if (checkConditionsOn == CheckConditionsOn.All)
			{
				for (int num = targets.Count - 1; num >= 0; num--)
				{
					if (!conditions.All(context.SetTarget(targets[num])))
					{
						targets.RemoveAt(num);
					}
				}
			}
			return targets;
		}

		public IEnumerable<ATarget> GetTargetable(ActionContext context, AAction action)
		{
			if (!action.CanAttemptToAct(context))
			{
				yield break;
			}
			foreach (ATarget item in _GetTargetable(context))
			{
				if (!action._ShouldAct(context.SetTarget(item)))
				{
					continue;
				}
				if (canBeRestrictedByReaction)
				{
					Ability ability = context.ability;
					if (ability != null && ability.TargetRestrictedByReaction(item))
					{
						continue;
					}
				}
				yield return item;
			}
		}

		public IEnumerable<ATarget> FilterTargetsByConditions(ActionContext context, IEnumerable<ATarget> targets)
		{
			using PoolKeepItemListHandle<Condition> conditions = Pools.UseKeepItemList(_PrimaryTargetConditions());
			if (conditions.Count == 0)
			{
				foreach (ATarget target in targets)
				{
					yield return target;
				}
				yield break;
			}
			foreach (ATarget target2 in targets)
			{
				if (conditions.value.All(context.SetTarget(target2)))
				{
					yield return target2;
				}
			}
		}

		public IEnumerable<AbilityKeyword> GetKeywords()
		{
			foreach (Condition item in _TargetConditions())
			{
				foreach (AbilityKeyword keyword in item.GetKeywords())
				{
					yield return keyword;
				}
			}
		}
	}

	[ProtoContract]
	[UIField]
	public class Trigger
	{
		[ProtoMember(1)]
		[UIField(excludedValuesMethod = "_ExcludeTriggeredBy", tooltip = "Entity responsible for taking the action that triggered the specified event.")]
		[UIHorizontalLayout("A")]
		private ReactionEntity _triggeredBy;

		[ProtoMember(2)]
		[UIField(excludedValuesMethod = "_ExcludeTriggeredOn", tooltip = "Entity which was acted upon by <i>Triggered By</i> to trigger the specified event.")]
		[UIHorizontalLayout("A")]
		[DefaultValue(ReactionEntity.Anyone)]
		private ReactionEntity _triggeredOn = ReactionEntity.Anyone;

		[ProtoMember(3)]
		[UIField(excludedValuesMethod = "_ExcludeTarget", tooltip = "Determines which entity is targeted when specified event is triggered by <i>Triggered By</i> on <i>Triggered On</i>.")]
		[UIHorizontalLayout("A")]
		private TargetOfReaction _target;

		[ProtoMember(5, OverwriteList = true)]
		[UIField]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		[UIHideIf("_hideTriggeredBy")]
		private List<Condition.Actor> _triggeredByConditions;

		[ProtoMember(6, OverwriteList = true)]
		[UIField]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		[UIHideIf("_hideTriggeredOn")]
		private List<Condition.Actor> _triggeredOnConditions;

		[ProtoMember(7)]
		[UIField("Prevent Recursive Triggering", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "This trigger will not go off if one of its previous triggers is still processing.")]
		private bool _preventRecursion;

		[ProtoMember(4)]
		[UIField(collapse = UICollapseType.Hide)]
		private Reaction _reaction;

		private TargetedReactionFilter _reactionFilter
		{
			get
			{
				TargetedReactionFilter result = default(TargetedReactionFilter);
				result.filter = new ReactionFilter
				{
					triggeredBy = _triggeredBy,
					triggeredOn = _triggeredOn,
					triggeredByConditions = _triggeredByConditions,
					triggeredOnConditions = _triggeredOnConditions,
					preventRecursion = _preventRecursion
				};
				result.target = _target;
				return result;
			}
		}

		public AbilityPreventedBy? abilityPreventedBy => _reaction.GetAbilityPreventedBy(_triggeredBy, _triggeredOn);

		public Reaction reaction => _reaction;

		private string _targetString
		{
			get
			{
				if (_target != 0 || _triggeredBy == ReactionEntity.Anyone)
				{
					if (_target != TargetOfReaction.TriggeredOn || _triggeredOn == ReactionEntity.Anyone)
					{
						return EnumUtil.FriendlyName(_target);
					}
					return EnumUtil.FriendlyName(_triggeredOn);
				}
				return EnumUtil.FriendlyName(_triggeredBy);
			}
		}

		protected bool _hideTriggeredBy => !(_reaction?.usesTriggeredBy ?? true);

		protected bool _hideTriggeredOn => !(_reaction?.usesTriggeredOn ?? true);

		private bool _triggeredByConditionsSpecified => !_hideTriggeredBy;

		private bool _triggeredOnConditionsSpecified => !_hideTriggeredOn;

		public event Action<ReactionContext, TargetedReactionFilter, int> onTrigger;

		private void _OnReactionTrigger(ReactionContext context)
		{
			this.onTrigger?.Invoke(context, _reactionFilter, context.capturedValue);
		}

		public void Register(ActionContext context)
		{
			_reaction.Register(context);
			_triggeredByConditions.Register(context);
			_triggeredOnConditions.Register(context);
			Reaction obj = _reaction;
			obj.onTrigger = (Action<ReactionContext>)Delegate.Combine(obj.onTrigger, new Action<ReactionContext>(_OnReactionTrigger));
		}

		public void Unregister(ActionContext context)
		{
			_reaction.Unregister(context);
			_triggeredByConditions.Unregister(context);
			_triggeredOnConditions.Unregister(context);
			Reaction obj = _reaction;
			obj.onTrigger = (Action<ReactionContext>)Delegate.Remove(obj.onTrigger, new Action<ReactionContext>(_OnReactionTrigger));
		}

		public IEnumerable<AbilityKeyword> GetKeywords()
		{
			foreach (AbilityKeyword keyword in reaction.GetKeywords())
			{
				yield return keyword;
			}
			foreach (AbilityKeyword keyword2 in _triggeredByConditions.GetKeywords())
			{
				yield return keyword2;
			}
			foreach (AbilityKeyword keyword3 in _triggeredOnConditions.GetKeywords())
			{
				yield return keyword3;
			}
		}

		public override string ToString()
		{
			return string.Format("When{0}{1}{2}target <b>{3}</b>", (!_hideTriggeredBy) ? (" <b>" + _triggeredByConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + EnumUtil.FriendlyName(_triggeredBy) + "</b> ") : " ", _reaction, (!_hideTriggeredOn) ? (" <b>" + _triggeredOnConditions.ToStringSmart(" & ").SizeIfNotEmpty().SpaceIfNotEmpty() + EnumUtil.FriendlyName(_triggeredOn) + "</b> ") : " ", _targetString) + _preventRecursion.ToText(" !recurse").SizeIfNotEmpty();
		}

		protected bool _ExcludeTriggeredBy(ReactionEntity entity)
		{
			if (_hideTriggeredBy)
			{
				return entity != ReactionEntity.Anyone;
			}
			return false;
		}

		protected bool _ExcludeTriggeredOn(ReactionEntity entity)
		{
			if (_hideTriggeredOn)
			{
				return entity != ReactionEntity.Anyone;
			}
			return false;
		}

		protected bool _ExcludeTarget(TargetOfReaction entity)
		{
			if (entity != TargetOfReaction.TriggeredOn || !_hideTriggeredOn)
			{
				if (entity == TargetOfReaction.TriggeredBy)
				{
					return _hideTriggeredBy;
				}
				return false;
			}
			return true;
		}
	}

	[ProtoMember(1, OverwriteList = true)]
	[UIField(tooltip = "Conditions that must hold true for the owner of the ability in order for this action to take affect.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UIHeader("Action")]
	[UIMargin(24f, false)]
	protected List<Condition.Actor> _ownerConditions;

	[ProtoMember(2, OverwriteList = true)]
	[UIField(tooltip = "This action will be considered active until <b>any</b> of the following duration settings are complete.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UIHideIf("_hideDurations")]
	protected List<Duration> _durations;

	[ProtoMember(3, OverwriteList = true)]
	[UIField(tooltip = "This action will act on all tick targets whenever <b>any</b> of the following triggers are signaled.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UIHideIf("_hideTicks")]
	protected List<Trigger> _ticks;

	[ProtoMember(4)]
	[UIField(tooltip = "Projectile Media that will play after targets are selected and this action is about to take affect.")]
	[UIMargin(24f, false)]
	[UIHeader("Media")]
	[UIDeepValueChange]
	protected ActionMedia _actMedia;

	[ProtoMember(5)]
	[UIField(tooltip = "Projectile Media that will play on the target which this action ticks on.")]
	[UIDeepValueChange]
	protected ActionMedia _tickMedia;

	public abstract Target target { get; }

	protected virtual Target _tickTarget => null;

	protected virtual AAction _tickAction => this;

	protected virtual IEnumerable<DynamicNumber> _appliedDynamicNumbers
	{
		get
		{
			yield break;
		}
	}

	public virtual int appliedSortingOrder => 0;

	public virtual bool hasEffectOnTarget => true;

	public virtual bool dealsDamage => false;

	public virtual bool processesDamage => false;

	public virtual bool affectsTurnOrder => false;

	protected virtual bool _canTick => true;

	protected virtual bool _shouldPlayTickMedia => true;

	protected virtual bool _canHaveDuration => true;

	protected virtual bool _requiresUserInputAfterTargeting => false;

	public bool hasDuration
	{
		get
		{
			if (_durations != null)
			{
				return _durations.Count > 0;
			}
			return false;
		}
	}

	public bool isTicking
	{
		get
		{
			if (_ticks != null)
			{
				return _ticks.Count > 0;
			}
			return false;
		}
	}

	public virtual bool isApplied
	{
		get
		{
			if (!hasDuration && !isTicking)
			{
				return !_canTick;
			}
			return true;
		}
	}

	public bool requiresUserInput
	{
		get
		{
			if (!target.requiresUserInput)
			{
				return _requiresUserInputAfterTargeting;
			}
			return true;
		}
	}

	public IEnumerable<Duration> durations
	{
		get
		{
			IEnumerable<Duration> enumerable = _durations;
			return enumerable ?? Enumerable.Empty<Duration>();
		}
	}

	public IEnumerable<Trigger> ticks
	{
		get
		{
			IEnumerable<Trigger> enumerable = _ticks;
			return enumerable ?? Enumerable.Empty<Trigger>();
		}
	}

	public ActionMedia actMedia => _actMedia;

	public ActionMedia tickMedia => _tickMedia;

	public Target this[TargetToCheck type, ActionContext.State state] => (Target)((type switch
	{
		TargetToCheck.Active => (state == ActionContext.State.Tick) ? this[TargetToCheck.Tick, state] : this[TargetToCheck.Main, state], 
		TargetToCheck.Tick => _tickAction._tickTarget, 
		_ => null, 
	}) ?? target);

	protected bool _hideTicks => !_canTick;

	protected bool _hideDurations => !_canHaveDuration;

	private bool _actMediaSpecified => _actMedia;

	private bool _tickMediaSpecified => _tickMedia;

	protected virtual bool _ShouldAct(ActionContext context)
	{
		return true;
	}

	public virtual IEnumerable<AbilityKeyword> GetKeywords(AbilityData abilityData)
	{
		if (!_ownerConditions.IsNullOrEmpty())
		{
			foreach (Condition.Actor ownerCondition in _ownerConditions)
			{
				foreach (AbilityKeyword keyword in ownerCondition.GetKeywords())
				{
					yield return keyword;
				}
			}
		}
		foreach (AbilityKeyword keyword2 in target.GetKeywords())
		{
			yield return keyword2;
		}
		if (_ticks.IsNullOrEmpty())
		{
			yield break;
		}
		foreach (Trigger tick in _ticks)
		{
			foreach (AbilityKeyword keyword3 in tick.GetKeywords())
			{
				yield return keyword3;
			}
		}
	}

	public virtual void PostProcessApply(AppliedAction appliedAction)
	{
	}

	public virtual bool ShouldTick(ActionContext context)
	{
		return true;
	}

	protected virtual void _Tick(ActionContext context)
	{
	}

	public virtual void Apply(ActionContext context)
	{
	}

	protected virtual void _Register(AppliedAction appliedAction)
	{
	}

	public virtual void Unapply(ActionContext context)
	{
	}

	protected virtual void _Unregister(AppliedAction appliedAction)
	{
	}

	public virtual void Reapply(ActionContext context)
	{
		Unapply(context);
		Apply(context);
	}

	public virtual IEnumerable<GameStep> GetActGameSteps(ActionContext context)
	{
		yield return target.GetGameStep(context, this);
		if ((bool)_actMedia)
		{
			yield return new GameStepActionActMedia(this, context);
		}
		yield return new GameStepActionAct(this, context);
	}

	public virtual IEnumerable<GameStep> GetTickGameSteps(ActionContext context)
	{
		AAction tickAction = _tickAction;
		Target tickTarget = tickAction._tickTarget;
		yield return new GameStepActionTarget.Tick(tickAction, context, tickTarget != null);
		if (tickTarget != null)
		{
			yield return tickTarget.GetGameStep(context, tickAction);
		}
		if ((bool)tickAction._tickMedia && tickAction._shouldPlayTickMedia)
		{
			yield return new GameStepActionTickMedia(tickAction, context);
		}
		yield return new GameStepActionTick(tickAction, context);
	}

	public virtual int GetPotentialDamage(ActionContext context)
	{
		return 0;
	}

	protected virtual string _ToStringUnique()
	{
		return GetType().GetUILabel();
	}

	protected virtual string _ToStringAfterTarget()
	{
		return "";
	}

	protected virtual string _GetTargetString()
	{
		return " " + target;
	}

	public IEnumerable<ATarget> GetTargetable(ActionContext context)
	{
		return target.GetTargetable(context, this);
	}

	public IEnumerable<ATarget> GetTargetableWithPostProcessing(ActionContext context)
	{
		IEnumerable<ATarget> targetable = GetTargetable(context);
		if (!target.PostProcessesTargets(TargetingContext.ShowAvailableTargets))
		{
			return targetable;
		}
		PoolKeepItemListHandle<ATarget> poolKeepItemListHandle = Pools.UseKeepItemList(targetable);
		target.PostProcessTargets(context, poolKeepItemListHandle);
		return poolKeepItemListHandle.AsEnumerable();
	}

	public bool CanAttemptToAct(ActionContext context)
	{
		return _ownerConditions.All(context.SetTarget(context.actor));
	}

	public void Act(ActionContext context)
	{
		if (isApplied)
		{
			AppliedAction.Apply(context, this);
		}
		else
		{
			_Tick(context);
		}
	}

	public void Register(AppliedAction appliedAction)
	{
		_Register(appliedAction);
		foreach (Trigger tick in ticks)
		{
			tick.Register(appliedAction.context);
			tick.onTrigger += appliedAction.OnTick;
		}
		foreach (Duration duration in durations)
		{
			duration.Register(appliedAction.context);
			duration.onDurationComplete += appliedAction.OnDurationComplete;
		}
		foreach (DynamicNumber appliedDynamicNumber in _appliedDynamicNumbers)
		{
			appliedDynamicNumber.Register(appliedAction);
		}
	}

	public void Unregister(AppliedAction appliedAction)
	{
		_Unregister(appliedAction);
		foreach (Trigger tick in ticks)
		{
			tick.Unregister(appliedAction.context);
			tick.onTrigger -= appliedAction.OnTick;
		}
		foreach (Duration duration in durations)
		{
			duration.Unregister(appliedAction.context);
			duration.onDurationComplete -= appliedAction.OnDurationComplete;
		}
		foreach (DynamicNumber appliedDynamicNumber in _appliedDynamicNumbers)
		{
			appliedDynamicNumber.Unregister();
		}
	}

	public void Tick(ActionContext context)
	{
		if (CanAttemptToAct(context) && _ShouldAct(context))
		{
			if (_canTick)
			{
				_Tick(context);
			}
			else
			{
				AppliedAction.Apply(context, this);
			}
		}
	}

	public sealed override string ToString()
	{
		return ((!_ticks.IsNullOrEmpty()) ? ("<size=66%>" + _ticks.ToStringSmart(" or ") + "</size> ") : "") + ((!_ownerConditions.IsNullOrEmpty()) ? ("<size=66%>" + _ownerConditions.ToStringSmart() + "</size> ") : "") + _ToStringUnique() + _GetTargetString() + _ToStringAfterTarget() + ((!_durations.IsNullOrEmpty()) ? (" <size=66%>" + _durations.ToStringSmart(" or ") + "</size>") : "");
	}
}
