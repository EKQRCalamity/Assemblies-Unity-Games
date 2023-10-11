using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameStepGroupTutorial : GameStepGroup
{
	public class TargetStep : GameStepActionTarget
	{
		private static TargetCombatantAction _Action;

		private static TargetCombatantAction Action => _Action ?? (_Action = new TargetCombatantAction());

		public TargetStep(ATarget target)
			: base(Action, default(ActionContext).SetTarget(target), null)
		{
		}

		public override void Start()
		{
			_targets = new List<ATarget> { base.context.target ?? base.state.player };
		}
	}

	public class TutorialStep : GameStep
	{
		private static SoundPack.SoundData _SoundData = new SoundPack.SoundData();

		public TutorialCard tutorial;

		private VoiceSource _activeNarration;

		private TextMeshTyper _activeTyper;

		private PoolKeepItemListHandle<AGameStepTurn> _displayEffectiveStatSteps;

		public TutorialStep(TutorialCard tutorial)
		{
			this.tutorial = tutorial;
		}

		private void _OnTutorialClick(TutorialCard.Pile pile, TutorialCard card)
		{
			if (card == tutorial && (bool)_activeTyper)
			{
				if (!_activeTyper.isFinished)
				{
					_activeTyper.Finish();
				}
				else
				{
					base.finished = true;
				}
			}
		}

		private void _OnConfirmPressed()
		{
			_OnTutorialClick(TutorialCard.Pile.TopLeft, tutorial);
		}

		private void _OnClick()
		{
			base.finished = base.finished || tutorial.view.hasGlow;
		}

		private void _CreateTargetLines()
		{
			bool flag = false;
			foreach (TutorialData.TargetLineData targetLine in tutorial.data.targetLines)
			{
				if (!flag)
				{
					ActiveCombat activeCombat = base.state.activeCombat;
					if (activeCombat != null && !activeCombat.defenseHasBeenLaunched && targetLine.targetingAction.target.targetType == typeof(ACombatant))
					{
						flag = targetLine.endAt == CardTarget.Defense || targetLine.endAt == CardTarget.Offense;
					}
				}
				foreach (GameStep actGameStep in targetLine.targetingAction.GetActGameSteps(tutorial.context))
				{
					AppendStep(actGameStep);
				}
				AppendStep(new TargetLineStep(tutorial, targetLine));
			}
			if (flag)
			{
				_displayEffectiveStatSteps = Pools.UseKeepItemList(base.state.stack.GetSteps().OfType<AGameStepTurn>().Process(delegate(AGameStepTurn step)
				{
					step.StopDisplayingEffectiveStats();
				}));
			}
		}

		protected override void OnFirstEnabled()
		{
			if (tutorial.pile != 0)
			{
				CancelGroup();
			}
			else
			{
				base.OnFirstEnabled();
			}
		}

		protected override void OnEnable()
		{
			base.state.tutorialDeck.layout.onPointerClick += _OnTutorialClick;
			base.view.onConfirmPressed += _OnConfirmPressed;
			base.view.onClick += _OnClick;
		}

		public override void Start()
		{
			tutorial.MarkAsFinished();
			base.state.tutorialDeck.Transfer(tutorial, TutorialCard.Pile.TopLeft);
		}

		protected override IEnumerator Update()
		{
			float alpha = tutorial.tutorialCard.descriptionText.alpha;
			tutorial.tutorialCard.descriptionText.alpha = 0f;
			_activeTyper = tutorial.tutorialCard.descriptionText.GetOrAddComponent<TextMeshTyper>().Stop(freezeFontSize: false);
			while (!tutorial.view.atRestInLayout)
			{
				yield return null;
			}
			_CreateTargetLines();
			yield return null;
			if ((bool)tutorial.data.narration)
			{
				_activeNarration = VoiceManager.Instance.Play(_SoundData.SetAudioRef(tutorial.data.narration), interrupt: true, 0f, isGlobal: false);
			}
			tutorial.tutorialCard.descriptionText.alpha = alpha;
			_activeTyper.StartTyping(_activeNarration ? ((Func<AudioSource>)(() => _activeNarration.source)) : null, 0f, 0f, ProfileManager.options.game.ui.syncNarrativeText);
			while (!_activeTyper.isFinished)
			{
				yield return null;
			}
			tutorial.view.RequestGlow(this, Colors.TARGET);
			while (!base.finished)
			{
				yield return null;
			}
		}

		protected override void OnDisable()
		{
			base.state.tutorialDeck.layout.onPointerClick -= _OnTutorialClick;
			base.view.onConfirmPressed -= _OnConfirmPressed;
			base.view.onClick -= _OnClick;
		}

		protected override void OnDestroy()
		{
			TargetLineView.RemoveOwnedBy(tutorial);
			tutorial.view.ReleaseOwnedGlowRequests(GlowTags.Persistent);
			_activeNarration.StopAndClear(ref _activeNarration);
			_activeTyper?.DisableAll();
			if (base.ended)
			{
				base.state.tutorialDeck.Discard(tutorial);
			}
			if (!_displayEffectiveStatSteps)
			{
				return;
			}
			foreach (AGameStepTurn item in _displayEffectiveStatSteps.value)
			{
				item.DisplayEffectiveStats();
			}
			Pools.Repool(ref _displayEffectiveStatSteps);
		}
	}

	public class TargetLineStep : GameStep
	{
		public TutorialCard owner;

		public TutorialData.TargetLineData data;

		public TargetLineStep(TutorialCard owner, TutorialData.TargetLineData data)
		{
			this.owner = owner;
			this.data = data;
		}

		public override void Start()
		{
			foreach (ATarget target in GetPreviousSteps().OfType<GameStepActionTarget>().First().targets)
			{
				TargetLineView.AddUnique(owner, Colors.USED, owner.view[data.beginFrom], target.view[data.endAt], null, data.endRotation, TargetLineTags.Persistent, 1f, 1f, null, data.endOffset);
				target.view.RequestGlow(owner.view, Colors.USED, GlowTags.Persistent);
			}
		}
	}

	public TutorialCard tutorial;

	public AEntity triggerTarget;

	public GameStepGroupTutorial(TutorialCard tutorial, AEntity triggerTarget)
	{
		this.tutorial = tutorial;
		this.triggerTarget = triggerTarget;
	}

	protected override IEnumerable<GameStep> _GetSteps()
	{
		yield return new TargetStep(triggerTarget);
		yield return new TutorialStep(tutorial);
	}
}
