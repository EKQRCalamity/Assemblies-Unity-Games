using DG.Tweening;
using UnityEngine;

public class ShowHideUIItem : MonoBehaviour
{
	public delegate void ShowHideEvent(ShowHideUIItem item);

	public CanvasGroup canvasGroup;

	public float showSeconds = 0.5f;

	public float hideSeconds = 0.5f;

	public bool IsShowing;

	public event ShowHideEvent OnShown;

	public event ShowHideEvent OnHidden;

	public virtual void Show()
	{
		if (canvasGroup != null)
		{
			IsShowing = true;
			canvasGroup.DOFade(1f, showSeconds).OnComplete(delegate
			{
				DoOnShown();
			});
		}
	}

	public virtual void Hide()
	{
		if (canvasGroup != null)
		{
			IsShowing = false;
			canvasGroup.DOFade(0f, hideSeconds).OnComplete(delegate
			{
				DoOnHidden();
			});
		}
	}

	private void DoOnShown()
	{
		if (this.OnShown != null)
		{
			this.OnShown(this);
		}
	}

	private void DoOnHidden()
	{
		IsShowing = false;
		if (this.OnHidden != null)
		{
			this.OnHidden(this);
		}
	}
}
