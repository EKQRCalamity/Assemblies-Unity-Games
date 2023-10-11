using System.Collections;
using UnityEngine.EventSystems;

public class GameManagerStep : GameStep
{
	protected GameStepStack stack => base.manager.stack;

	protected virtual void _OnGameClick(PointerEventData eventData)
	{
	}

	protected virtual void _OnDeckClick(PointerEventData eventData)
	{
	}

	protected virtual void _OnLevelClick(PointerEventData eventData)
	{
	}

	protected override void OnEnable()
	{
		base.manager.adventureState.pointerClick.OnClick.AddListener(_OnGameClick);
		base.manager.deckState.pointerClick.OnClick.AddListener(_OnDeckClick);
		base.manager.levelUpState.pointerClick.OnClick.AddListener(_OnLevelClick);
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
		base.finished = true;
		base.manager.adventureState.pointerClick.OnClick.RemoveListener(_OnGameClick);
		base.manager.deckState.pointerClick.OnClick.RemoveListener(_OnDeckClick);
		base.manager.levelUpState.pointerClick.OnClick.RemoveListener(_OnLevelClick);
	}
}
