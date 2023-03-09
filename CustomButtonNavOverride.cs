using Rewired.UI.ControlMapper;
using UnityEngine;
using UnityEngine.UI;

public class CustomButtonNavOverride : CustomButton
{
	[SerializeField]
	private Selectable upOnSinglePlayer;

	[SerializeField]
	private Selectable downOnSinglePlayer;

	[SerializeField]
	private Selectable upOnMultiPlayer;

	[SerializeField]
	private Selectable downOnMultiPlayer;

	[SerializeField]
	private ControlMapper mapper;

	public override Selectable FindSelectableOnUp()
	{
		if (!PlayerManager.Multiplayer)
		{
			return upOnSinglePlayer;
		}
		return (!upOnMultiPlayer) ? mapper.GetUnselectedPlayerButton() : upOnMultiPlayer;
	}

	public override Selectable FindSelectableOnDown()
	{
		if (!PlayerManager.Multiplayer)
		{
			if (PlatformHelper.IsConsole)
			{
				return this;
			}
			return downOnSinglePlayer;
		}
		return (!downOnMultiPlayer) ? mapper.GetUnselectedPlayerButton() : downOnMultiPlayer;
	}
}
