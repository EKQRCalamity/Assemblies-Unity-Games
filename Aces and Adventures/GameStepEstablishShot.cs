using UnityEngine;
using UnityEngine.EventSystems;

public class GameStepEstablishShot : GameManagerStep
{
	private void _UpdateTalismans()
	{
		foreach (DataRef<CharacterData> item in DataRef<CharacterData>.All)
		{
			base.manager[item.data.characterClass].SetActive(ProfileManager.progress.experience.read.GetLevel(item) >= 30);
		}
	}

	protected override void _OnGameClick(PointerEventData eventData)
	{
		base.finished = true;
		base.stack.Push(new GameStepGroupTransitionToAdventureTable());
		base.stack.Push(new GameStepGeneric
		{
			onStart = delegate
			{
				GameStateView.CreateAdventureView().state = GameState.BeginAdventureSelection();
			}
		});
	}

	protected override void _OnDeckClick(PointerEventData eventData)
	{
		if (ProfileManager.progress.deckCreationEnabled)
		{
			base.finished = true;
			base.manager.establishCamera.SetDollyTrack(base.manager.deckTrack);
			GameStep step = base.stack.ParallelProcesses(new GameStepWait(1f, null, canSkip: false), new GameStepInstantiate(base.manager.deckCreationViewBlueprint, null, GameStateView._DestroyState), new GameStepWaitForTime(Time.time + 1.666f), new GameStepWaitFrame(), new GameStepGenericSimple(delegate
			{
				base.manager.deckState.boxOpen = true;
			}));
			base.stack.Push(new GameStepLighting(ContentRef.Defaults.lighting.deckCreationTransition.data));
			base.stack.Push(new GameStepTimeline(base.manager.establishShotToDeck, base.manager.deckLookAt, 0.75f, base.stack));
			base.stack.Push(new GameStepVirtualCamera(base.manager.deckCamera, 0f));
			base.stack.Push(new GameStepStateChange(base.manager.deckState));
			base.stack.Push(new GameStepUnloadSceneAsync(base.manager.cosmeticScene));
			base.stack.Push(new GameStepSetupAdventureRendering());
			base.stack.Push(new GameStepLighting(ContentRef.Defaults.lighting.deckCreation.data, base.manager.deckLookAt));
			base.stack.Push(new GameStepWaitForParallelStep(step));
			base.stack.Push(new GameStepGeneric
			{
				onStart = delegate
				{
					GameStateView.Instance.state = GameState.BeginDeckCreation();
				}
			});
		}
	}

	protected override void _OnLevelClick(PointerEventData eventData)
	{
		if (base.state.levelUp.enabled)
		{
			base.finished = true;
			base.manager.establishCamera.SetDollyTrack(base.manager.levelUpTrack);
			base.stack.Push(new GameStepTimeline(base.manager.establishShotToLevelUp, base.manager.levelUpLookAt, 0.8f));
			base.stack.Push(new GameStepVirtualCamera(base.manager.levelUpCamera, 0f));
			base.stack.Push(new GameStepStateChange(base.manager.levelUpState));
			base.stack.Push(new GameStepGeneric
			{
				onStart = delegate
				{
					base.state.stack.Register().Push(new GameStepLevelUpBase());
				}
			});
		}
	}

	protected override void Awake()
	{
		if (!GameStateView.HasActiveInstance)
		{
			GameStateView.CreateLevelUpView(base.manager.levelUpState.transform).state = GameState.BeginLevelUp();
		}
		base.manager.onDeckCreationEnabledChange?.Invoke(ProfileManager.progress.deckCreationEnabled);
		_UpdateTalismans();
		base.manager.deckState.gameObject.SetActive(ProfileManager.progress.deckCreationEnabled);
		base.manager.levelUpState.gameObject.SetActive(ProfileManager.progress.experience.read.enabled);
	}
}
