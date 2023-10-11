using System.Collections;
using UnityEngine.EventSystems;

public class GameStepHideProceduralMap : GameStep
{
	public static GameStepHideProceduralMap ActiveStep { get; private set; }

	private ProceduralMapView mapView => ProceduralMapView.Instance;

	public override bool canSafelyCancelStack => true;

	private void _OnMapClick(ProceduralMap.Pile pile, ProceduralMap card)
	{
		_OnBack();
	}

	private void _OnBack()
	{
		base.finished = true;
	}

	private void _OnDragEnd(PointerEventData eventData, CardLayoutElement card)
	{
		if (base.finished)
		{
			GameStepProceduralMap.Instance?.SignalDraggedBackIntoView();
		}
	}

	protected override void OnFirstEnabled()
	{
		ActiveStep = this;
	}

	protected override void OnEnable()
	{
		base.state.mapDeck.Transfer(ProceduralMapView.Instance.map, ProceduralMap.Pile.Hidden);
		base.state.mapDeck.layout.onPointerClick += _OnMapClick;
		base.view.onBackPressed += _OnBack;
		base.view.mapDeckLayout.hidden.onDragEnd += _OnDragEnd;
		EnumUtil.Subtract(ref mapView.bubbleDrag.buttonsToBubble, PointerInputButtonFlags.Left);
		base.view.wildPiles = ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand;
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
		base.state.mapDeck.layout.onPointerClick -= _OnMapClick;
		base.view.onBackPressed -= _OnBack;
		base.view.mapDeckLayout.hidden.onDragEnd -= _OnDragEnd;
		EnumUtil.Add(ref mapView.bubbleDrag.buttonsToBubble, PointerInputButtonFlags.Left);
	}

	protected override void OnDestroy()
	{
		ActiveStep = ((ActiveStep == this) ? null : ActiveStep);
	}

	public void ShowMap()
	{
		_OnBack();
	}
}
