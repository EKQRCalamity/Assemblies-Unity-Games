using UnityEngine;

public class ImageRefSearcher : MonoBehaviour
{
	public ImageCategoryType category;

	public ImageRefEvent onImageRefSelected;

	public Texture2DEvent onTextureSelected;

	public Color rayCastBlockerColor = new Color(1f, 1f, 1f, 0.31f);

	private void _OnImageRefSelected(ImageRef imageRef)
	{
		imageRef = ProtoUtil.Clone(imageRef);
		onImageRefSelected.Invoke(imageRef);
		if (imageRef.texture != null)
		{
			onTextureSelected.Invoke(imageRef.texture);
		}
	}

	public void OpenSearch()
	{
		UIUtil.CreateImageSearchPopup(category, _OnImageRefSelected, base.transform);
	}
}
