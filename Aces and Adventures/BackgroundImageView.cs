using UnityEngine;

public class BackgroundImageView : MonoBehaviour
{
	private ImageRef _backgroundImage;

	private ShowImageAnimatedView _previousBackgroundImageView;

	public ImageRef backgroundImage
	{
		get
		{
			return _backgroundImage;
		}
		set
		{
			if (SetPropertyUtility.SetObjectEQ(ref _backgroundImage, value))
			{
				_OnBackgroundImageChange();
			}
		}
	}

	private void _OnBackgroundImageChange()
	{
		Clear();
		if ((bool)backgroundImage)
		{
			_previousBackgroundImageView = ShowImageAnimatedView.Create(backgroundImage, base.transform, doResolutionScaling: false, 0f);
		}
	}

	public void Clear()
	{
		if ((bool)_previousBackgroundImageView)
		{
			_previousBackgroundImageView.Finish();
			_previousBackgroundImageView = null;
		}
	}

	public void SetBackgroundImageNull()
	{
		_backgroundImage = null;
	}
}
