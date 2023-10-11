using UnityEngine;
using UnityEngine.UI;

public class ImageRefSearchView : MonoBehaviour
{
	private static ResourceBlueprint<GameObject> _Blueprint = "UI/Content/ImageRefSearchView";

	public Texture2DEvent onImageChange;

	public RectEvent onUVRectChange;

	public Int2Event onMaxDimensionChange;

	public StringEvent onNameChange;

	public ImageRef imageRef { get; private set; }

	public static ImageRefSearchView Create(ImageRef imageRef, Transform parent = null)
	{
		return Pools.Unpool(_Blueprint, parent).GetComponent<ImageRefSearchView>()._SetData(imageRef);
	}

	private void _OnTexture(Texture2D texture)
	{
		if ((bool)this && base.isActiveAndEnabled)
		{
			onImageChange.Invoke(texture);
			onUVRectChange.Invoke(imageRef.category.PreferredSize().HasValue ? UVCoords.FromPreferredAndActualSize(imageRef.category.PreferredSize().Value, texture.PixelSize()) : UVCoords.Default);
			onMaxDimensionChange.Invoke(imageRef.category.MaxSizeInImageSearch());
			LayoutRebuilder.MarkLayoutForRebuild(base.transform as RectTransform);
		}
	}

	private ImageRefSearchView _SetData(ImageRef image)
	{
		imageRef = image;
		image.GetTexture2D(_OnTexture);
		onNameChange.Invoke(image.name);
		return this;
	}

	private void OnDisable()
	{
		onImageChange.Invoke(null);
		imageRef = null;
	}

	public void RequestEditTags()
	{
		if ((bool)imageRef && imageRef.belongsToCurrentCreator)
		{
			UIUtil.CreateContentRefTagsPopup(imageRef, base.transform);
		}
	}

	public void RequestRename()
	{
		if ((bool)imageRef && imageRef.belongsToCurrentCreator)
		{
			UIUtil.CreateContentRefRenamePopup(imageRef, base.transform, onNameChange.Invoke);
		}
	}
}
