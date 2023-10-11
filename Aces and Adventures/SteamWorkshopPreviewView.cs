using UnityEngine;
using UnityEngine.UI;

public class SteamWorkshopPreviewView : SteamWorkshopItemView
{
	public static readonly ResourceBlueprint<GameObject> Blueprint = "UI/Content/SteamWorkshopPreviewView";

	public Texture2DEvent onImageChange;

	public BoolEvent onHasImageChange;

	public BoolEvent onHasTrailerChange;

	private Texture2D _previewImage;

	public override bool layoutIsReady => _previewImage;

	private void _OnImageChange(Texture2D image)
	{
		onHasImageChange.Invoke(_previewImage = image);
		if (SteamWorkshopSearcher.TypeBlueprintsThatRequireSpecialSpacing.ContainsKey(_contentRef.GetType()))
		{
			GetComponentInParent<SteamWorkshopSearcher>().UpdateSpacing();
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform.parent as RectTransform);
	}

	protected override void Awake()
	{
		base.Awake();
		onImageChange.AddListener(_OnImageChange);
	}

	public override Vector2 GetPreferredSize()
	{
		return (GetComponentInChildren<RawImage>().mainTexture as Texture2D).PixelSize();
	}

	protected override void _OnResultSetUnique()
	{
		onHasTrailerChange.Invoke(base.result.GetTrailerVideoId().HasVisibleCharacter());
		Job.Process(WebRequestTextureCache.RequestAsync(this, base.result.previewImageUrl).AsEnumerator()).Immediately().ResultAction(delegate(Texture2D t)
		{
			this.InvokeEventIfActive(onImageChange, t);
		});
	}

	public void PlayTrailer()
	{
		Job.Process(YouTubeVideoId.ShowVideo(base.result.GetTrailerVideoId(), base.result.name + " Trailer", base.transform), Department.UI);
	}
}
