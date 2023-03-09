using UnityEngine;

public class MapSecretAchievementUnlocker : AbstractMonoBehaviour
{
	[SerializeField]
	private bool updateDialogue = true;

	[SerializeField]
	private int dialoguerVariableID = 7;

	private void OnTriggerEnter2D(Collider2D collider)
	{
		MapPlayerController component = collider.GetComponent<MapPlayerController>();
		OnlineManager.Instance.Interface.UnlockAchievement(component.id, "FoundSecretPassage");
		if (updateDialogue)
		{
			Dialoguer.SetGlobalFloat(dialoguerVariableID, 1f);
			PlayerData.SaveCurrentFile();
		}
	}
}
