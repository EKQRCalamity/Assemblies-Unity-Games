using System.Collections;
using UnityEngine;

public class GameStepTopDeck : GameStep
{
	private ActionContext _context;

	private ActionContextTarget _drawFrom;

	private ResourceCard _topDeck;

	private float _wait;

	public ResourceCard card => _topDeck;

	public GameStepTopDeck(ActionContext context, ActionContextTarget drawFrom, float wait = 1f)
	{
		_context = context;
		_drawFrom = drawFrom;
		_wait = wait;
	}

	protected override IEnumerator Update()
	{
		IdDeck<ResourceCard.Pile, ResourceCard> resourceDeck = _context.GetTarget<ACombatant>(_drawFrom).resourceDeck;
		ResourceCard.Pile? drawTo = ResourceCard.Pile.TopDeckHand;
		IdDeck<ResourceCard.Pile, ResourceCard>.GameStepDraw drawStep = resourceDeck.DrawStep(1, null, drawTo, null, 0f);
		yield return AppendStep(drawStep);
		_topDeck = drawStep.lastDrawnValue;
		if (_topDeck == null)
		{
			yield break;
		}
		ATargetView aTargetView = _context.ability?.root.view ?? base.view.playerAbilityDeckLayout.activationHand.GetComponentInChildren<AdventureTargetView>() ?? _context.actor.view;
		if ((bool)aTargetView)
		{
			TargetLineView.AddUnique(aTargetView, Colors.USED, aTargetView[CardTarget.Name], _topDeck.view[CardTarget.Name], Quaternion.AngleAxis(45f, Vector3.right), Quaternion.AngleAxis(45f, Vector3.right), TargetLineTags.TopDeck | TargetLineTags.Persistent, 0.5f, 1f, null, new Vector3(0f, 0f, 0.008f));
		}
		foreach (float item in Wait(_wait))
		{
			_ = item;
			yield return null;
		}
	}

	public void Discard()
	{
		card?.deck.Discard(card);
	}
}
