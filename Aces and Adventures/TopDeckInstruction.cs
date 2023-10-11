using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField]
[ProtoInclude(10, typeof(Draw))]
[ProtoInclude(11, typeof(Blackjack))]
[ProtoInclude(12, typeof(War))]
public abstract class TopDeckInstruction
{
	[ProtoContract]
	[UIField]
	public class Draw : TopDeckInstruction
	{
		public class Step : GameStepTopDeckInstruction
		{
			private Draw _instruction;

			private int _countRemaining;

			private int _count;

			public override TopDeckInstruction instruction => _instruction;

			protected override List<AAction.Condition.Combatant> _drawFromTargetConditions => _instruction._drawFromConditions;

			public Step(Draw instruction, ActionContext context)
				: base(context)
			{
				_instruction = instruction;
				_countRemaining = (_count = _instruction.count.GetValue(context));
			}

			protected override IEnumerable<ActionContextTarget> _DrawFromTargets()
			{
				yield return _instruction._drawFrom;
			}

			protected override IEnumerator Update()
			{
				while (_countRemaining-- > 0)
				{
					GameStepTopDeck topDeck = new GameStepTopDeck(_context, _instruction._drawFrom, _instruction._while ? (1f / (float)(_count - _countRemaining)) : (1f / (float)(_countRemaining + 1)));
					yield return AppendStep(topDeck);
					if (topDeck.card == null || !_instruction._while.IsValid(topDeck.card))
					{
						break;
					}
				}
			}
		}

		[ProtoMember(1)]
		[UIField]
		private ActionContextTarget _drawFrom;

		[ProtoMember(2)]
		[UIField(min = 1, max = 20)]
		[UIDeepValueChange]
		private AAction.DynamicNumber _count;

		[ProtoMember(3)]
		[UIField]
		[UIDeepValueChange]
		private PlayingCard.Filter _while;

		[ProtoMember(4, OverwriteList = true)]
		[UIField]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Condition.Combatant> _drawFromConditions;

		private AAction.DynamicNumber count => _count ?? (_count = new AAction.DynamicNumber.Constant());

		public override bool ShouldAct(ActionContext context)
		{
			return context.GetTarget<ACombatant>(_drawFrom)?.resourceDeck.CanDraw() ?? false;
		}

		public override GameStepTopDeckInstruction GetGameStep(ActionContext context)
		{
			return new Step(this, context);
		}

		public override IEnumerable<AbilityKeyword> GetKeywords()
		{
			yield return AbilityKeyword.TopDeck;
		}

		public override string ToString()
		{
			string text = string.Format("Draw {0} {1} from {2}{3}'s deck", count, "card".Pluralize(count.constantValue ?? 2), _drawFromConditions.ToStringSmart(" & ").SpaceIfNotEmpty().SizeIfNotEmpty(), _drawFrom);
			bool b = _while;
			PlayingCard.Filter @while = _while;
			return text + b.ToText(" while " + @while.ToString());
		}
	}

	[ProtoContract]
	[UIField]
	public class Blackjack : TopDeckInstruction
	{
		public class Step : GameStepTopDeckInstruction
		{
			private static readonly string[] CHOICES = new string[2] { "Hit", "Stay" };

			private Blackjack _instruction;

			private int _total;

			public override TopDeckInstruction instruction => _instruction;

			public Step(Blackjack instruction, ActionContext context)
				: base(context)
			{
				_instruction = instruction;
			}

			protected override IEnumerable<ActionContextTarget> _DrawFromTargets()
			{
				yield return _instruction._drawFrom;
			}

			protected override IEnumerator Update()
			{
				GameStepStringChoice choice;
				do
				{
					GameStepTopDeck topDeck = new GameStepTopDeck(_context, _instruction._drawFrom);
					yield return AppendStep(topDeck);
					if ((_total += (int)topDeck.card.currentValue.value) < _instruction._target)
					{
						choice = new GameStepStringChoice(CHOICES);
						yield return AppendStep(choice);
						continue;
					}
					break;
				}
				while (!(choice.choice == CHOICES[1]));
			}
		}

		[ProtoMember(1)]
		[UIField]
		private ActionContextTarget _drawFrom;

		[ProtoMember(2)]
		[UIField(min = 1, max = 21)]
		[DefaultValue(21)]
		private int _target = 21;

		public override bool ShouldAct(ActionContext context)
		{
			return true;
		}

		public override GameStepTopDeckInstruction GetGameStep(ActionContext context)
		{
			return new Step(this, context);
		}

		public override string ToString()
		{
			return $"Draw from {_drawFrom}'s deck until {_target}+ or stay";
		}
	}

	[ProtoContract]
	[UIField]
	public class War : TopDeckInstruction
	{
		public class Step : GameStepTopDeckInstruction
		{
			private War _instruction;

			private int _countRemaining;

			private ActionContextTarget _target
			{
				get
				{
					AEntity actor = _context.actor;
					if (actor == null || actor.faction != 0)
					{
						return ActionContextTarget.Player;
					}
					if (!(_context.target is Enemy))
					{
						if (_context.gameState.activeCombat == null)
						{
							return ActionContextTarget.FirstEnemyInTurnOrder;
						}
						return ActionContextTarget.EnemyInActiveCombat;
					}
					return ActionContextTarget.Target;
				}
			}

			public override TopDeckInstruction instruction => _instruction;

			public Step(War instruction, ActionContext context)
				: base(context)
			{
				_instruction = instruction;
				_countRemaining = _instruction._count;
			}

			protected override IEnumerable<ActionContextTarget> _DrawFromTargets()
			{
				yield return _target;
				yield return ActionContextTarget.Owner;
			}

			protected override IEnumerator Update()
			{
				while (_countRemaining-- > 0)
				{
					base.state.player.activeTopDeckResult = null;
					GameStepTopDeck enemyTopDeck = new GameStepTopDeck(_context, _target, 0.5f);
					yield return AppendStep(enemyTopDeck);
					GameStepTopDeck topDeck = new GameStepTopDeck(_context, ActionContextTarget.Owner);
					yield return AppendStep(topDeck);
					if (enemyTopDeck.card == null || topDeck.card == null)
					{
						break;
					}
					if (_instruction._orUntilBeaten && enemyTopDeck.card.currentValue.value > topDeck.card.currentValue.value)
					{
						base.state.player.activeTopDeckResult = TopDeckResult.Failure;
						base.state.SignalTopDeckFinishedDrawing(_context, TopDeckResult.Failure);
						if (base.isActiveStep)
						{
							break;
						}
						yield return null;
						if (enemyTopDeck.card.currentValue.value > topDeck.card.currentValue.value)
						{
							break;
						}
					}
				}
				base.state.player.activeTopDeckResult = null;
			}
		}

		[ProtoMember(1)]
		[UIField(min = 1, max = 20)]
		[DefaultValue(1)]
		[UIHorizontalLayout("A")]
		private int _count = 1;

		[ProtoMember(2)]
		[UIField]
		[UIHorizontalLayout("A")]
		private bool _orUntilBeaten;

		public override bool shouldSignalAboutToFail => !_orUntilBeaten;

		public override bool ShouldAct(ActionContext context)
		{
			if (context.actor is ACombatant aCombatant && aCombatant.resourceDeck.CanDraw())
			{
				ACombatant target = context.GetTarget<ACombatant>();
				if (target != null)
				{
					return target.resourceDeck.CanDraw();
				}
			}
			return false;
		}

		public override GameStepTopDeckInstruction GetGameStep(ActionContext context)
		{
			return new Step(this, context);
		}

		public override IEnumerable<AbilityKeyword> GetKeywords()
		{
			yield return AbilityKeyword.TopDeckAgainst;
		}

		public override string ToString()
		{
			return string.Format("War {0} {1}", _count, "card".Pluralize(_count)) + _orUntilBeaten.ToText(" or until beaten");
		}
	}

	public virtual bool shouldSignalAboutToFail => true;

	public abstract bool ShouldAct(ActionContext context);

	public abstract GameStepTopDeckInstruction GetGameStep(ActionContext context);

	public virtual IEnumerable<AbilityKeyword> GetKeywords()
	{
		yield break;
	}
}
