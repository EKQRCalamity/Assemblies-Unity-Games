using UnityEngine;

namespace HutongGames.PlayMaker.Actions;

[ActionCategory(ActionCategory.GameObject)]
[Tooltip("Keep searching for a game object until it is not null.")]
public class WaitForGameObject : FsmStateAction
{
	[Tooltip("The name of the GameObject to find. You can leave this empty if you specify a Tag.")]
	public FsmString objectName;

	[UIHint(UIHint.Tag)]
	[Tooltip("Find a GameObject with this tag. If Object Name is specified then both name and Tag must match.")]
	public FsmString withTag;

	[RequiredField]
	[UIHint(UIHint.Variable)]
	[Tooltip("Store the result in a GameObject variable.")]
	public FsmGameObject store;

	public override void Reset()
	{
		objectName = string.Empty;
		withTag = "Untagged";
		store = null;
	}

	public override void OnUpdate()
	{
		Find();
		if (store.Value != null)
		{
			Finish();
		}
	}

	private void Find()
	{
		if (withTag.Value != "Untagged")
		{
			if (!string.IsNullOrEmpty(objectName.Value))
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag(withTag.Value);
				GameObject[] array2 = array;
				foreach (GameObject gameObject in array2)
				{
					if (gameObject.name == objectName.Value)
					{
						store.Value = gameObject;
						return;
					}
				}
				store.Value = null;
			}
			else
			{
				store.Value = GameObject.FindGameObjectWithTag(withTag.Value);
			}
		}
		else
		{
			store.Value = GameObject.Find(objectName.Value);
		}
	}

	public override string ErrorCheck()
	{
		if (string.IsNullOrEmpty(objectName.Value) && string.IsNullOrEmpty(withTag.Value))
		{
			return "Specify Name, Tag, or both.";
		}
		return null;
	}
}
