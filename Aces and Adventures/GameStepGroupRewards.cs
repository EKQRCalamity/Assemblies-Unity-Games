using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.Localization;

public class GameStepGroupRewards : GameStepGroup
{
	public class GameStepExperienceVial : GameStep
	{
		private const float PACK_THRESHOLD = 0.4999f;

		private readonly int _initialExperience;

		private readonly int _addedExperience;

		private ExperienceVialView _vialView;

		private bool _doneAnimating;

		private float _animationSpeed = 1f;

		private int _totalExperience => _initialExperience + _addedExperience;

		private float _normalizedExperience => (float)_totalExperience / (float)_maxExperience;

		private BarFiller _barFiller => _vialView.barFiller;

		public bool cardPackUnlocked
		{
			get
			{
				if (_initialExperience < 50)
				{
					return _totalExperience >= 50;
				}
				return false;
			}
		}

		private int _maxExperience => 100;

		public GameStepExperienceVial(int initialExperience, int addedExperience)
		{
			_initialExperience = initialExperience;
			_addedExperience = addedExperience;
		}

		private void _OnRewardClick(RewardPile pile, ATarget card)
		{
			base.finished |= pile == RewardPile.Select && _doneAnimating;
		}

		private void _OnConfirmPressed()
		{
			_OnRewardClick(RewardPile.Select, base.state.rewardDeck.FirstInPile(RewardPile.Select));
			if (!_doneAnimating)
			{
				_animationSpeed = 100f;
			}
		}

		private LocalizedString _GetMessage()
		{
			return MessageData.Instance.game.levelUpCombined.SetVariables(("Unlock", cardPackUnlocked), ("CardPack", LevelUpMessages.CardPackUnlocked.Localize()), ("Next", (_totalExperience < 50) ? LevelUpMessages.ManaTillCardPack.Localize().SetArguments(50 - _totalExperience) : ((_totalExperience < 100) ? LevelUpMessages.ManaTillLevel.Localize().SetArguments(100 - _totalExperience) : LevelUpMessages.YouCanLevelUp.Localize())));
		}

		protected override void OnFirstEnabled()
		{
			base.view.rewardDeckLayout.select.soundPack = base.view.rewardDeckLayout.vialSoundPack;
			_vialView = base.state.rewardDeck.Transfer(base.state.rewardDeck.Add(new ExperienceVial(_initialExperience, shouldSimulateLiquid: false)), RewardPile.Select).view as ExperienceVialView;
			_barFiller.max = _maxExperience;
			_barFiller.value = _initialExperience;
			_vialView.offsets.Add(Matrix4x4.TRS(base.view.rewardDeckLayout.select.transform.forward * -0.012f, Quaternion.Euler(7.5f, 0f, 0f), Vector3.one));
		}

		protected override void OnEnable()
		{
			base.view.rewardDeckLayout.onPointerClick += _OnRewardClick;
			base.view.onConfirmPressed += _OnConfirmPressed;
			base.view.onBackPressed += _OnConfirmPressed;
		}

		public override void Start()
		{
			if (ProfileManager.options.game.preferences.quickLevelUp)
			{
				_OnConfirmPressed();
			}
		}

		protected override IEnumerator Update()
		{
			while (!base.state.rewardDeck.layout.GetLayout(RewardPile.Select).IsAtRest())
			{
				yield return null;
			}
			foreach (float item in Wait(0.5f))
			{
				_ = item;
				yield return null;
			}
			_vialView.topCorkEnabled = false;
			foreach (float item2 in Wait(0.25f))
			{
				_ = item2;
				yield return null;
			}
			_vialView.isFilling = true;
			GameStepProjectileMedia gatherManaStep = base.state.stack.ParallelProcess(new GameStepProjectileMedia(ContentRef.Defaults.media.levelUp.vialGatherManaToTop, _vialView.card, _vialView.card)) as GameStepProjectileMedia;
			while (gatherManaStep.shouldUpdate)
			{
				yield return null;
			}
			while (_barFiller.normalizedValue < _normalizedExperience - 0.001f)
			{
				float normalizedValue = _barFiller.normalizedValue;
				_barFiller.normalizedValue = Math.Min(_normalizedExperience, _barFiller.normalizedValue + Time.deltaTime * _animationSpeed / GameVariables.Values.levelUp.fillVialTime);
				if (normalizedValue < 0.4999f && _barFiller.normalizedValue >= 0.4999f)
				{
					base.state.stack.ParallelProcess(new GameStepProjectileMedia(ContentRef.Defaults.media.levelUp.vialFillThresholdHalfway, _vialView.card, _vialView.card));
				}
				yield return null;
			}
			gatherManaStep.Stop();
			_vialView.isFilling = false;
			if (_totalExperience >= _maxExperience)
			{
				base.state.stack.ParallelProcess(new GameStepProjectileMedia(ContentRef.Defaults.media.levelUp.vialFillComplete, _vialView.card, _vialView.card));
				foreach (float item3 in Wait(0.25f))
				{
					_ = item3;
					yield return null;
				}
			}
			foreach (float item4 in Wait(0.25f))
			{
				_ = item4;
				yield return null;
			}
			_vialView.topCorkEnabled = true;
			foreach (float item5 in Wait(0.25f))
			{
				_ = item5;
				yield return null;
			}
			_doneAnimating = true;
			if (!ProfileManager.options.game.preferences.quickLevelUp)
			{
				if (ProfileManager.options.game.ui.tutorialEnabled)
				{
					base.view.LogMessage(_GetMessage());
				}
				while (!base.finished)
				{
					yield return null;
				}
			}
		}

		protected override void OnDisable()
		{
			base.view.rewardDeckLayout.onPointerClick -= _OnRewardClick;
			base.view.onConfirmPressed -= _OnConfirmPressed;
			base.view.onBackPressed -= _OnConfirmPressed;
			base.view.ClearMessage();
			_vialView?.offsets.Clear();
		}

		protected override void OnDestroy()
		{
			base.state.rewardDeck.TransferPile(RewardPile.Select, RewardPile.Discard);
		}
	}

	public class GameStepCardPack : GameStep
	{
		private CardPack _pack;

		private PoolKeepItemListHandle<DataRef<AbilityData>> _unlockedAbilities;

		public GameStepCardPack()
		{
			_unlockedAbilities = ProfileManager.progress.abilities.write.UnlockRandomAbilities(base.state.random, 5, base.state.player.characterDataRef, base.state.unlockAbilityForCurrentClassChance, base.state.abilityDeck.GetCards().ConcatIfNotNull(base.state.player?.appliedAbilities.GetCards()).ToHash(), base.state.keepPreferredAbilityChance);
		}

		private void _OnRewardClick(RewardPile pile, ATarget card)
		{
			if (pile == RewardPile.Select && !_pack.packView.opening)
			{
				_pack.packView.BeginOpen();
			}
		}

		private void _OnConfirmPressed()
		{
			_OnRewardClick(RewardPile.Select, _pack);
		}

		protected override void OnFirstEnabled()
		{
			base.view.rewardDeckLayout.select.soundPack = base.view.rewardDeckLayout.cardPackSoundPack;
			base.state.rewardDeck.Transfer(base.state.rewardDeck.Add(_pack = new CardPack()), RewardPile.Select);
			_pack.view.offsets.Add(Matrix4x4.Translate(base.view.rewardDeckLayout.GetLayout(RewardPile.Select).transform.up * -0.2f));
		}

		protected override void OnEnable()
		{
			base.view.rewardDeckLayout.onPointerClick += _OnRewardClick;
			base.view.onConfirmPressed += _OnConfirmPressed;
			base.view.onBackPressed += _OnConfirmPressed;
		}

		public override void Start()
		{
			if ((bool)_unlockedAbilities)
			{
				_pack.abilities.Add(_unlockedAbilities.value.Select((DataRef<AbilityData> d) => new Ability(d)));
			}
			if (ProfileManager.options.game.preferences.quickLevelUp)
			{
				_OnConfirmPressed();
			}
		}

		protected override IEnumerator Update()
		{
			while (!_pack.packView.readyToOpen)
			{
				yield return null;
			}
		}

		protected override void End()
		{
			if ((bool)_unlockedAbilities)
			{
				AppendStep(new GameStepOpenPack(_pack));
			}
		}

		protected override void OnDisable()
		{
			base.view.rewardDeckLayout.onPointerClick -= _OnRewardClick;
			base.view.onConfirmPressed -= _OnConfirmPressed;
			base.view.onBackPressed -= _OnConfirmPressed;
		}

		protected override void OnDestroy()
		{
			_pack?.view.offsets.Clear();
			if (_pack != null)
			{
				base.state.rewardDeck.Transfer(_pack, RewardPile.Discard);
			}
			Pools.Repool(ref _unlockedAbilities);
		}
	}

	public class GameStepOpenPack : GameStep
	{
		private CardPack _pack;

		private HashSet<ATarget> _clickedTargets = new HashSet<ATarget>();

		public GameStepOpenPack(CardPack pack)
		{
			_pack = pack;
		}

		private void _OnRewardPointerEnter(RewardPile pile, ATarget card)
		{
			if (pile == RewardPile.CardPackSelect && card is Ability ability)
			{
				ability.view.RequestGlow(this, ability.data.rank.GetColor());
			}
		}

		private void _OnRewardPointerExit(RewardPile pile, ATarget card)
		{
			if (pile == RewardPile.CardPackSelect && card is Ability ability)
			{
				ability.view.ReleaseGlow(this);
			}
		}

		private void _OnRewardPointerClick(RewardPile pile, ATarget card)
		{
			if (pile != RewardPile.CardPackSelect)
			{
				return;
			}
			Ability ability = card as Ability;
			if (ability == null)
			{
				return;
			}
			if (ability.view.hasOffset)
			{
				if (_clickedTargets.Add(ability))
				{
					base.state.stack.ParallelProcess(new GameStepProjectileMedia(ability.data.rank.GetClickMedia(), new ActionContext(base.state.player, ability, ability))).ParallelChain(new GameStepGenericSimple(delegate
					{
						ability.view.offsets.Clear();
					}));
				}
			}
			else if (base.state.rewardDeck.GetCards(RewardPile.CardPackSelect).None((ATarget c) => c.view.hasOffset))
			{
				base.finished = true;
			}
		}

		private void _OnConfirmPressed()
		{
			foreach (ATarget card in base.state.rewardDeck.GetCards(RewardPile.CardPackSelect))
			{
				_OnRewardPointerClick(RewardPile.CardPackSelect, card);
			}
		}

		private void _OnStoneClick(Stone.Pile pile, Stone card)
		{
			if (pile == Stone.Pile.Cancel)
			{
				base.group?.Cancel((GameStep step) => step is GameStepExperienceVial || step is GameStepCardPack || step is GameStepOpenPack);
			}
		}

		private void _EnableCancelButton()
		{
			if (!ProfileManager.options.game.preferences.quickLevelUp)
			{
				return;
			}
			GameStepGroup gameStepGroup = base.group;
			if (gameStepGroup != null && gameStepGroup.GetNextSteps(this).Any((GameStep step) => step is GameStepExperienceVial))
			{
				ATargetView aTargetView = base.view.stoneDeckLayout[StoneType.Cancel]?.view;
				if ((object)aTargetView != null && !aTargetView.isActiveAndEnabled)
				{
					aTargetView.DestroyCard();
				}
				base.view.stoneDeckLayout.onPointerClick += _OnStoneClick;
				base.view.stoneDeckLayout.SetLayout(Stone.Pile.Cancel, base.view.stoneDeckLayout.cancelFloating);
				base.state.buttonDeck.Layout<ButtonDeckLayout>().Activate(ButtonCardType.Finish);
			}
		}

		private void _DisableCancelButton()
		{
			if (!(base.view.stoneDeckLayout.GetLayout(Stone.Pile.Cancel) != base.view.stoneDeckLayout.cancelFloating))
			{
				base.view.stoneDeckLayout.onPointerClick -= _OnStoneClick;
				base.state.buttonDeck.Layout<ButtonDeckLayout>().Deactivate(ButtonCardType.Finish);
				base.view.stoneDeckLayout.RestoreLayoutToDefault(Stone.Pile.Cancel);
			}
		}

		protected override void OnFirstEnabled()
		{
			_pack.packView.Open();
			_pack.packView.noiseAnimationEnabled = false;
			base.state.stack.ParallelProcess(new GameStepProjectileMedia(ContentRef.Defaults.media.levelUp.cardPackOpen, _pack, _pack));
			base.state.rewardDeck.Transfer(_pack.abilities.GetCardsSafe().AsEnumerable(), RewardPile.CardPackSelect);
			foreach (ATarget card in base.state.rewardDeck.GetCards(RewardPile.CardPackSelect))
			{
				card.view.offsets.Add(Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 180f)));
			}
			foreach (ATarget card2 in base.state.rewardDeck.GetCards(RewardPile.Select))
			{
				base.view.dofShifter.RemoveTarget(card2.view.transform);
			}
			AppendStep(new GameStepWaitForLayoutRest(base.view.rewardDeckLayout.GetLayout(RewardPile.CardPackSelect)));
		}

		protected override void OnEnable()
		{
			base.view.rewardDeckLayout.onPointerEnter += _OnRewardPointerEnter;
			base.view.rewardDeckLayout.onPointerExit += _OnRewardPointerExit;
			base.view.rewardDeckLayout.onPointerClick += _OnRewardPointerClick;
			base.view.onConfirmPressed += _OnConfirmPressed;
			base.view.onBackPressed += _OnConfirmPressed;
			_EnableCancelButton();
		}

		public override void Start()
		{
			if (ProfileManager.options.game.preferences.quickLevelUp)
			{
				_OnConfirmPressed();
			}
		}

		protected override IEnumerator Update()
		{
			while (!base.finished)
			{
				yield return null;
			}
		}

		protected override void OnDisable()
		{
			base.view.rewardDeckLayout.onPointerEnter -= _OnRewardPointerEnter;
			base.view.rewardDeckLayout.onPointerExit -= _OnRewardPointerExit;
			base.view.rewardDeckLayout.onPointerClick -= _OnRewardPointerClick;
			base.view.onConfirmPressed -= _OnConfirmPressed;
			base.view.onBackPressed -= _OnConfirmPressed;
			_DisableCancelButton();
		}

		protected override void OnDestroy()
		{
			base.state.rewardDeck.TransferPile(RewardPile.CardPackSelect, RewardPile.Discard);
		}
	}

	public abstract class AGameStepUnlock : GameStep
	{
		protected ATarget _target;

		protected abstract CardLayoutSoundPack _soundPack { get; }

		protected virtual Matrix4x4? _offset => null;

		protected abstract ATarget _CreateTarget();

		protected abstract LocalizedString _GetMessage();

		public abstract void _Unlock();

		protected virtual void _OnRewardPointerClick(RewardPile pile, ATarget card)
		{
			if (pile == RewardPile.Select)
			{
				base.finished = true;
			}
		}

		protected virtual void _OnRewardPointerEnter(RewardPile pile, ATarget card)
		{
		}

		protected virtual void _OnRewardPointerExit(RewardPile pile, ATarget card)
		{
		}

		protected override void Awake()
		{
			_Unlock();
		}

		protected override void OnEnable()
		{
			base.state.rewardDeck.layout.onPointerClick += _OnRewardPointerClick;
			base.state.rewardDeck.layout.onPointerEnter += _OnRewardPointerEnter;
			base.state.rewardDeck.layout.onPointerExit += _OnRewardPointerExit;
		}

		public override void Start()
		{
			base.view.rewardDeckLayout.select.soundPack = _soundPack;
			ATarget aTarget = base.state.rewardDeck.Transfer(base.state.rewardDeck.Add(_target = _CreateTarget(), RewardPile.Draw), RewardPile.Select);
			LocalizedString localizedString = _GetMessage();
			if (localizedString != null)
			{
				base.view.LogMessage(localizedString);
			}
			Matrix4x4? offset = _offset;
			if (offset.HasValue)
			{
				Matrix4x4 valueOrDefault = offset.GetValueOrDefault();
				aTarget.view.offsets.Add(valueOrDefault);
			}
		}

		protected override IEnumerator Update()
		{
			while (!base.finished)
			{
				yield return null;
			}
		}

		protected override void OnDisable()
		{
			base.state.rewardDeck.layout.onPointerClick -= _OnRewardPointerClick;
			base.state.rewardDeck.layout.onPointerEnter -= _OnRewardPointerEnter;
			base.state.rewardDeck.layout.onPointerExit -= _OnRewardPointerExit;
		}

		protected override void OnFinish()
		{
			base.state.rewardDeck.TransferPile(RewardPile.Select, RewardPile.Discard);
		}

		protected override void OnDestroy()
		{
			base.view.ClearMessage();
		}
	}

	public class GameStepUnlockClass : AGameStepUnlock
	{
		private DataRef<CharacterData> _characterToUnlock;

		protected override CardLayoutSoundPack _soundPack => base.view.rewardDeckLayout.classSealSoundPack;

		protected override Matrix4x4? _offset => Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(2.5f, 0f, 0f), Vector3.one);

		public GameStepUnlockClass(DataRef<CharacterData> characterToUnlock)
		{
			_characterToUnlock = characterToUnlock;
		}

		protected override void OnFirstEnabled()
		{
			DataRef<ProceduralNodeData> characterStory = ContentRef.Defaults.data.GetCharacterStory(_characterToUnlock);
			if (characterStory != null)
			{
				base.state.ProcessProceduralNodeData(characterStory.data, useRewardLayouts: true);
			}
		}

		protected override ATarget _CreateTarget()
		{
			return new ClassSeal(_characterToUnlock);
		}

		protected override LocalizedString _GetMessage()
		{
			return _characterToUnlock.data.characterClass.LocalizeUnlockMessage();
		}

		public override void _Unlock()
		{
			ProfileManager.progress.characters.write.Unlock(_characterToUnlock);
		}
	}

	public class GameStepUnlockGame : AGameStepUnlock
	{
		private DataRef<GameData> _gameToUnlock;

		private bool _selectGame;

		protected override CardLayoutSoundPack _soundPack => base.view.rewardDeckLayout.stoneSoundPack;

		public GameStepUnlockGame(DataRef<GameData> gameToUnlock, bool selectGame)
		{
			_gameToUnlock = gameToUnlock;
			_selectGame = selectGame;
		}

		protected override ATarget _CreateTarget()
		{
			return new GameStone(_gameToUnlock);
		}

		protected override LocalizedString _GetMessage()
		{
			return MessageData.GameTooltips.GameUnlocked.Localize();
		}

		public override void _Unlock()
		{
			ProfileManager.progress.games.write.UnlockGame(_gameToUnlock);
			if (_selectGame)
			{
				ProfileManager.prefs.selectedGame = _gameToUnlock;
			}
		}

		protected override void _OnRewardPointerEnter(RewardPile pile, ATarget card)
		{
			if (card == _target)
			{
				ProjectedTooltipFitter.Create(_gameToUnlock.data.GetTitle(), card.view.gameObject, base.view.tooltipCanvas);
			}
		}

		protected override void _OnRewardPointerExit(RewardPile pile, ATarget card)
		{
			if (card == _target)
			{
				ProjectedTooltipFitter.Finish(card.view.gameObject);
			}
		}
	}

	public class GameStepUnlockAdventure : AGameStepUnlock
	{
		private DataRef<GameData> _game;

		private DataRef<AdventureData> _adventureToUnlock;

		protected override CardLayoutSoundPack _soundPack => base.view.rewardDeckLayout.deckSoundPack;

		public GameStepUnlockAdventure(DataRef<GameData> game, DataRef<AdventureData> adventureToUnlock)
		{
			_game = game;
			_adventureToUnlock = adventureToUnlock;
		}

		protected override ATarget _CreateTarget()
		{
			return new AdventureDeck(_adventureToUnlock);
		}

		protected override LocalizedString _GetMessage()
		{
			return MessageData.GameTooltips.AdventureUnlocked.Localize().SetVariables((LocalizedVariableName.Name, _game.data.GetTitle()));
		}

		public override void _Unlock()
		{
			if (!ProfileManager.progress.games.read.IsUnlocked(_game))
			{
				ProfileManager.progress.games.write.UnlockGame(_game);
			}
			if (!ProfileManager.progress.games.read.IsUnlocked(_game, _adventureToUnlock))
			{
				ProfileManager.progress.games.write.UnlockAdventure(_game, _adventureToUnlock);
			}
		}
	}

	public class GameStepSubmitLeaderboard : GameStep
	{
		private DataRef<AdventureData> _adventure;

		private int _mana;

		private int _time;

		private PlayerClass _playerClass;

		private int _reloadCount;

		private int _dailyLeaderboard;

		private bool _completed;

		private bool _leaderboardIsValid;

		private bool _showLeaderboard;

		private LeaderboardScoreUploaded_t? _upload;

		private GameStepSubmitLeaderboard(DataRef<AdventureData> adventure, int mana, int time, PlayerClass playerClass, int reloadCount, int dailyLeaderboard, bool completed, bool leaderboardIsValid, bool showLeaderboard = true, int? maxMana = null)
		{
			_adventure = adventure;
			_mana = mana;
			_time = time;
			_playerClass = playerClass;
			_reloadCount = reloadCount;
			_dailyLeaderboard = dailyLeaderboard;
			_completed = completed;
			_leaderboardIsValid = leaderboardIsValid;
			_showLeaderboard = showLeaderboard;
			if (completed)
			{
				ProfileManager.progress.games.write.leaderboardProgress.MarkAsComplete(adventure);
			}
			if (maxMana.HasValue)
			{
				_mana = Math.Min(_mana, maxMana.Value);
			}
		}

		public GameStepSubmitLeaderboard(GameState state, bool completed, bool showLeaderboard = true)
			: this(state.adventure, state.experience, state.strategyTime, state.player.characterClass, state.reloadCount, state.dailyLeaderboard.GetValueOrDefault(), completed, state.LeaderboardIsValid(), showLeaderboard, state.LeaderboardManaMax())
		{
		}

		private async void _UploadScore()
		{
			int[] data = ProfileManager.progress.games.read.leaderboardProgress.GetData();
			_upload = await Steam.Stats.UploadLeaderboardScore(LeaderboardProgress.GetDailyName(_dailyLeaderboard), ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, LeaderboardProgress.GetData(_adventure, data).score, data);
		}

		public override void Start()
		{
			if (ProfileManager.options.game.preferences.leaderboard && _leaderboardIsValid && Steam.Enabled)
			{
				DataRef<AdventureData> adventure = _adventure;
				if (adventure != null && adventure.data.dailyLeaderboardEnabled && _dailyLeaderboard != 0 && !base.state.devCommandUsed && !(LeaderboardProgress.GetDay() - _dailyLeaderboard >= 7))
				{
					return;
				}
			}
			Cancel();
		}

		protected override IEnumerator Update()
		{
			LeaderboardProgress.Data? currentData = ProfileManager.progress.games.read.leaderboardProgress.GetData(_adventure);
			if (ProfileManager.progress.games.write.leaderboardProgress.SetData(_dailyLeaderboard, _adventure, _mana, _time, _completed, _reloadCount, _playerClass, base.state.modifierNode))
			{
				_UploadScore();
				while (!_upload.HasValue)
				{
					yield return null;
				}
				if (_upload.Value.m_bSuccess == 0)
				{
					ProfileManager.progress.games.write.leaderboardProgress.OverrideData(_adventure, currentData);
					Cancel();
				}
			}
		}

		public override void OnCompletedSuccessfully()
		{
			if (_showLeaderboard)
			{
				AppendStep(new GameStepViewLeaderboard(_adventure, _dailyLeaderboard));
			}
		}
	}

	public class GameStepViewLeaderboard : GameStep
	{
		private DataRef<AdventureData> _adventure;

		private int? _dailyLeaderboard;

		private LeaderboardView _leaderboardView;

		public GameStepViewLeaderboard(DataRef<AdventureData> adventure, int? dailyLeaderboard = null)
		{
			_adventure = adventure;
			_dailyLeaderboard = dailyLeaderboard;
		}

		private void _OnCloseRequested()
		{
			base.finished = true;
		}

		private void _OnBackPressed()
		{
			_leaderboardView?.Close();
		}

		protected override void OnFirstEnabled()
		{
			_leaderboardView = (LeaderboardView)base.state.rewardDeck.Add(new Leaderboard(_adventure, _dailyLeaderboard.GetValueOrDefault()), RewardPile.Draw).view;
			base.view.rewardDeckLayout.leaderboard.Add(_leaderboardView);
		}

		protected override void OnEnable()
		{
			_leaderboardView.onCloseRequested.AddListener(_OnCloseRequested);
			base.view.onBackPressed += _OnBackPressed;
		}

		protected override IEnumerator Update()
		{
			while (!base.finished)
			{
				yield return null;
			}
		}

		protected override void OnDisable()
		{
			_leaderboardView.onCloseRequested.RemoveListener(_OnCloseRequested);
			base.view.onBackPressed -= _OnBackPressed;
		}

		protected override void End()
		{
			base.state.exileDeck.Transfer(_leaderboardView.leaderboard, ExilePile.ClearGameState);
		}
	}

	private AdventureEndType _endType;

	public GameStepGroupRewards(AdventureEndType endType)
	{
		_endType = endType;
	}

	protected override IEnumerable<GameStep> _GetSteps()
	{
		yield return new GameStepAnimateGameStateClear();
		if (base.gameState.dailyLeaderboard.HasValue && base.gameState.experience > 0 && _endType != 0)
		{
			yield return new GameStepSubmitLeaderboard(base.gameState, _endType == AdventureEndType.Victory);
		}
		if (DevData.Overrides.adventureExperienceOverride.HasValue)
		{
			base.gameState.experience = DevData.Overrides.adventureExperienceOverride.Value;
		}
		if (base.gameState.experience > 0)
		{
			int experience = base.gameState.experience;
			while (experience > 0)
			{
				int addedExperience = Math.Min(ProfileManager.progress.experience.read.toNextLevel, experience);
				GameStepExperienceVial experienceVialStep = new GameStepExperienceVial(ProfileManager.progress.experience.read.levelUpOverflow, addedExperience);
				yield return experienceVialStep;
				if (experienceVialStep.cardPackUnlocked && ProfileManager.progress.abilities.read.HasMissingAbility())
				{
					yield return new GameStepCardPack();
				}
				ProfileManager.progress.experience.write.experience += addedExperience;
				experience -= addedExperience;
			}
		}
		if (_endType == AdventureEndType.Victory)
		{
			foreach (AReward item in base.gameState.adventure.data.GetRewardsThatShouldUnlock())
			{
				yield return item.GetUnlockStep();
			}
		}
		yield return new GameStepGeneric
		{
			onUpdate = () => Job.WaitForCondition(() => !base.gameState.rewardDeck.Any())
		};
		ProfileManager.progress.DeleteActiveRun();
		ProfileManager.Profile.SaveProgress();
	}
}
