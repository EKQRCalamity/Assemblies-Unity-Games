using System.Collections;
using System.Linq;
using UnityEngine.Localization;

public class GameStepViewMap : AGameStepMap
{
	private LocalizedString _activeMessage;

	private ProceduralMapView mapView => ProceduralMapView.Instance;

	private ProceduralMap map => mapView.map;

	public override bool canSafelyCancelStack => true;

	protected override bool shouldBeCanceled => base.state.mapDeck.Any(ProceduralMap.Pile.Hidden);

	private void _OnButtonClick(ButtonCard.Pile pile, ButtonCard card)
	{
		if (pile == ButtonCard.Pile.Active && (ButtonCardType)card == ButtonCardType.Back)
		{
			_OnBack();
		}
	}

	private void _OnBack()
	{
		base.finished = true;
	}

	private void _OnHideRequested()
	{
		if (mapView.atRestInLayout)
		{
			_OnBack();
		}
	}

	protected override void OnCanceled()
	{
		GameStepHideProceduralMap gameStepHideProceduralMap = base.state.stack.GetSteps().OfType<GameStepHideProceduralMap>().FirstOrDefault();
		if (gameStepHideProceduralMap != null)
		{
			gameStepHideProceduralMap.finished = true;
		}
	}

	public override void Start()
	{
		_OffsetLayouts();
		if (!GameStepProceduralMap.Instance.hasFocusedMap)
		{
			GameStepProceduralMap.Instance.FocusMap();
		}
		_activeMessage = base.view.GetActiveMessage();
		if (_activeMessage != null)
		{
			base.view.ClearMessage();
		}
	}

	protected override void OnEnable()
	{
		base.state.mapDeck.Transfer(map, ProceduralMap.Pile.Active);
		base.view.stoneDeckLayout.SetLayout(Stone.Pile.Cancel, base.view.stoneDeckLayout.cancelFloating);
		base.view.buttonDeckLayout.SetActive(ButtonCardType.Back, setActive: true);
		base.view.buttonDeckLayout.onPointerClick += _OnButtonClick;
		base.view.onBackPressed += _OnBack;
		mapView.onHideRequested += _OnHideRequested;
		mapView.inputBlockerEnabled = true;
		base.view.dofShifter.targetOverrides.Add(mapView.transform);
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
		base.state.mapDeck.Transfer(map, ProceduralMap.Pile.Closed);
		base.view.buttonDeckLayout.SetActive(ButtonCardType.Back, setActive: false, forceUpdateCancelStone: true);
		base.view.stoneDeckLayout.RestoreLayoutToDefault(Stone.Pile.Cancel);
		base.view.buttonDeckLayout.onPointerClick -= _OnButtonClick;
		base.view.onBackPressed -= _OnBack;
		mapView.onHideRequested -= _OnHideRequested;
		mapView.inputBlockerEnabled = false;
		base.view.dofShifter.targetOverrides.Remove(mapView.transform);
	}

	protected override void End()
	{
		_ClearLayoutOffsets();
		if (_activeMessage != null)
		{
			base.view.LogMessage(_activeMessage);
		}
	}
}
