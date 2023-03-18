using Framework.Managers;
using UnityEngine;

public class InteractableDialog : MonoBehaviour
{
	public string conversation;

	public bool modal = true;

	public bool onlyLastLine;

	private void OnUsePost()
	{
		Core.Dialog.StartConversation(conversation, modal, onlyLastLine);
	}
}
