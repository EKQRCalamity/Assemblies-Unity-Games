using Rewired;
using UnityEngine;

public class MapLevelLoaderMausoleum : MapLevelLoader
{
	[SerializeField]
	private float pressDurationToReEnable = 1f;

	private bool reEnabled;

	private float currentDuration;

	protected override void Update()
	{
		switch (interactor)
		{
		default:
			if (PlayerWithinDistance(0))
			{
				Player actions4 = Map.Current.players[0].input.actions;
				if (actions4.GetButton(11) && actions4.GetButton(12))
				{
					currentDuration += CupheadTime.Delta;
				}
				else
				{
					currentDuration = 0f;
				}
			}
			break;
		case Interactor.Mugman:
			if (PlayerWithinDistance(1))
			{
				Player actions = Map.Current.players[1].input.actions;
				if (actions.GetButton(11) && actions.GetButton(12))
				{
					currentDuration += CupheadTime.Delta;
				}
				else
				{
					currentDuration = 0f;
				}
			}
			break;
		case Interactor.Either:
		{
			bool flag = false;
			if (PlayerWithinDistance(0))
			{
				Player actions2 = Map.Current.players[0].input.actions;
				if (actions2.GetButton(11) && actions2.GetButton(12))
				{
					currentDuration += CupheadTime.Delta;
					flag = true;
				}
			}
			if (PlayerWithinDistance(1))
			{
				Player actions3 = Map.Current.players[1].input.actions;
				if (actions3.GetButton(11) && actions3.GetButton(12))
				{
					currentDuration += CupheadTime.Delta;
					flag = true;
				}
			}
			if (!flag)
			{
				currentDuration = 0f;
			}
			break;
		}
		case Interactor.Both:
			if (Map.Current.players[0] == null || Map.Current.players[1] == null)
			{
				return;
			}
			if (!PlayerWithinDistance(0) || !PlayerWithinDistance(1))
			{
				break;
			}
			if (Map.Current.players[0].input.actions.GetButton(13))
			{
				if (Map.Current.players[1].input.actions.GetButton(13))
				{
					currentDuration += CupheadTime.Delta;
				}
				else
				{
					currentDuration = 0f;
				}
			}
			else
			{
				currentDuration = 0f;
			}
			break;
		}
		if (currentDuration >= pressDurationToReEnable)
		{
			reEnabled = true;
		}
		if (reEnabled)
		{
			base.Update();
		}
	}
}
