using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectHook : MonoBehaviour
{
	[SerializeField]
	private BoolEvent _onCanVerticallyScrollChange;

	private ScrollRect _scrollRect;

	public ScrollRect scrollRect => this.CacheComponent(ref _scrollRect);

	public BoolEvent onCanVerticallyScrollChange => _onCanVerticallyScrollChange ?? (_onCanVerticallyScrollChange = new BoolEvent());

	private void Update()
	{
		onCanVerticallyScrollChange.Invoke(scrollRect.content.GetWorldRect3D().height > scrollRect.viewport.GetWorldRect3D().height);
	}
}
