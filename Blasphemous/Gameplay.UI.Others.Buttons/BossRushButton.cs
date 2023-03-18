using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.Buttons;

public class BossRushButton : MonoBehaviour
{
	[BoxGroup("Controls", true, false, 0)]
	public GameObject EnableImage;

	[BoxGroup("Controls", true, false, 0)]
	public GameObject LockedImage;

	public void SetData(int idx, bool unlocked)
	{
		Button component = GetComponent<Button>();
		if (component != null)
		{
			component.interactable = unlocked;
		}
		if (EnableImage != null)
		{
			EnableImage.SetActive(unlocked);
		}
		if (LockedImage != null)
		{
			LockedImage.SetActive(!unlocked);
		}
		MenuButton component2 = GetComponent<MenuButton>();
		if (component2 != null)
		{
			component2.OnDeselect(null);
		}
	}
}
