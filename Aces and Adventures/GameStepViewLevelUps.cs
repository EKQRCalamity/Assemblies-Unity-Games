using System;
using System.Collections;
using UnityEngine.EventSystems;

public class GameStepViewLevelUps : GameStep
{
	private void _OnPointerClick(LevelUpPile pile, ATarget card)
	{
		if (pile != LevelUpPile.LevelUpsView)
		{
			_OnBackClick();
		}
	}

	private void _OnBackClick()
	{
		base.finished = true;
	}

	private void _OnBackClick(PointerEventData eventData)
	{
		_OnBackClick();
	}

	protected override void OnFirstEnabled()
	{
		base.state.levelUp.main.TransferPile(LevelUpPile.LevelUps, LevelUpPile.LevelUpsView);
	}

	protected override void OnEnable()
	{
		base.view.levelUp.SetCloseupFocalDistance();
		base.state.view.levelUp.backColliderEnabled = true;
		base.state.levelUp.main.layout.onPointerClick += _OnPointerClick;
		LevelUpStateView levelUp = base.state.view.levelUp;
		levelUp.onBackColliderClick = (Action)Delegate.Combine(levelUp.onBackColliderClick, new Action(_OnBackClick));
		LevelUpStateView levelUp2 = base.state.view.levelUp;
		levelUp2.onVialStandClick = (Action)Delegate.Combine(levelUp2.onVialStandClick, new Action(_OnBackClick));
		base.view.onBackPressed += _OnBackClick;
		base.manager.levelUpState.pointerClick.OnClick.AddListener(_OnBackClick);
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
		base.view.levelUp.ResetFocalDistance();
		base.state.view.levelUp.backColliderEnabled = false;
		base.state.levelUp.main.layout.onPointerClick -= _OnPointerClick;
		LevelUpStateView levelUp = base.state.view.levelUp;
		levelUp.onBackColliderClick = (Action)Delegate.Remove(levelUp.onBackColliderClick, new Action(_OnBackClick));
		LevelUpStateView levelUp2 = base.state.view.levelUp;
		levelUp2.onVialStandClick = (Action)Delegate.Remove(levelUp2.onVialStandClick, new Action(_OnBackClick));
		base.view.onBackPressed -= _OnBackClick;
		base.manager.levelUpState.pointerClick.OnClick.RemoveListener(_OnBackClick);
	}

	protected override void OnDestroy()
	{
		base.state.levelUp.main.TransferPile(LevelUpPile.LevelUpsView, LevelUpPile.LevelUps);
	}
}
