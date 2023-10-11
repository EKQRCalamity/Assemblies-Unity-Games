using UnityEngine;
using UnityEngine.UI;

public class SteamWorkshopImageView : SteamWorkshopItemView
{
	public static readonly ResourceBlueprint<GameObject> Blueprint = "UI/Content/SteamWorkshopImageView";

	public Texture2DEvent onImageChange;

	public MaterialEvent onMaterialChange;

	public RectEvent onUVRectChange;

	public Int2Event onMaxDimensionChange;

	public StringEvent onNameChange;

	public BoolEvent onHasImageChange;

	private Texture2D _texture;

	public ImageRef imageRef => _contentRef as ImageRef;

	public override bool layoutIsReady => _texture;

	private void _OnTexture(Texture2D texture)
	{
		if (this.IsActiveAndEnabled())
		{
			onHasImageChange.Invoke(_texture = texture);
			onImageChange.Invoke(texture);
			onMaterialChange.Invoke(imageRef.category.NeedsColorCorrectionForWorkshopImageSearch() ? UIUtil.LinearToGammaMaterial : null);
			onUVRectChange.Invoke(imageRef.category.PreferredSize().HasValue ? UVCoords.FromPreferredAndActualSize(imageRef.category.PreferredSize().Value, texture.PixelSize()) : UVCoords.Default);
			onMaxDimensionChange.Invoke(imageRef.category.MaxSizeInImageSearch());
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform.parent as RectTransform);
		}
	}

	protected override void _OnResultSetUnique()
	{
		onNameChange.Invoke(base.result.name);
		if (!base.result.previewImageUrl.IsNullOrEmpty())
		{
			Job.Process(WebRequestTextureCache.RequestAsync(this, base.result.previewImageUrl).AsEnumerator()).Immediately().ResultAction<Texture2D>(_OnTexture);
		}
	}
}
