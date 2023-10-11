using UnityEngine;
using UnityEngine.UI;

public class UIContentContainer : VerticalLayoutGroup, IUIContentContainer
{
	public Transform uiContentParent => base.transform;
}
