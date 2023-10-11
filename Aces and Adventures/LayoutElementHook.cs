using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement))]
public class LayoutElementHook : MonoBehaviour
{
	private LayoutElement _layoutElement;

	public LayoutElement layoutElement => this.CacheComponent(ref _layoutElement);

	public Vector2 preferredSize
	{
		get
		{
			return new Vector2(layoutElement.preferredWidth, layoutElement.preferredHeight);
		}
		set
		{
			layoutElement.preferredWidth = value.x;
			layoutElement.preferredHeight = value.y;
		}
	}
}
