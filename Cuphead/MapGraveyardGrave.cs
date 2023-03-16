using UnityEngine;

public class MapGraveyardGrave : MonoBehaviour
{
	[SerializeField]
	private bool isResetGrave;

	[SerializeField]
	private int index;

	private MapPlayerController player1;

	private MapPlayerController player2;

	private bool p1InTrigger;

	private bool p2InTrigger;

	private bool canInteract;

	[SerializeField]
	private MapGraveyardHandler main;

	[SerializeField]
	private Transform ghostPos;

	private bool hasCharm;

	private void Start()
	{
		hasCharm = HasCharm();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!collision.GetComponent<MapPlayerController>())
		{
			return;
		}
		MapPlayerController component = collision.GetComponent<MapPlayerController>();
		if (component.id == PlayerId.PlayerOne)
		{
			if (player1 == null)
			{
				player1 = component;
			}
			p1InTrigger = true;
		}
		else
		{
			if (player2 == null)
			{
				player2 = component;
			}
			p2InTrigger = true;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if ((bool)collision.GetComponent<MapPlayerController>())
		{
			MapPlayerController component = collision.GetComponent<MapPlayerController>();
			if (component.id == PlayerId.PlayerOne)
			{
				p1InTrigger = false;
			}
			else
			{
				p2InTrigger = false;
			}
		}
	}

	private bool HasCharm()
	{
		return (PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, Charm.charm_curse) && CharmCurse.CalculateLevel(PlayerId.PlayerOne) >= 0) || (PlayerData.Data.IsUnlocked(PlayerId.PlayerTwo, Charm.charm_curse) && CharmCurse.CalculateLevel(PlayerId.PlayerTwo) >= 0);
	}

	private void Update()
	{
		if (SceneLoader.IsInBlurTransition || (hasCharm && !main.canReenter) || (!canInteract && (!hasCharm || !main.canReenter)))
		{
			return;
		}
		if (p1InTrigger && player1.input.actions.GetButtonDown(13))
		{
			if (player1.animationController.facingUpwards)
			{
				InteractWith(0);
			}
		}
		else if (p2InTrigger && player2.input.actions.GetButtonDown(13) && player2.animationController.facingUpwards)
		{
			InteractWith(1);
		}
	}

	private void InteractWith(int playerNum)
	{
		if (!isResetGrave)
		{
			canInteract = false;
		}
		main.ActivatedGrave(index, playerNum, (!isResetGrave) ? ghostPos.transform.position : Vector3.zero);
	}

	public void SetInteractable(bool value)
	{
		canInteract = value;
	}
}
