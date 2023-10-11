using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShowImageAnimatedView : MonoBehaviour
{
	private static readonly ResourceBlueprint<GameObject> _Blueprint = "UI/NodeGraph/NodeData/ShowImageAnimatedView";

	public UnityEvent onImageLoaded;

	public UnityEvent onFinish;

	private AspectRatioFitter _aspectRatioFitter;

	private RawImage _rawImage;

	private AnimateNoise3d[] _animateNoise3ds;

	public AspectRatioFitter aspectRatioFitter => this.CacheComponentInChildren(ref _aspectRatioFitter);

	public RawImage rawImage => this.CacheComponentInChildren(ref _rawImage);

	public bool isFinishedAnimatingIn { get; set; }

	public AnimateNoise3d[] animateNoise3ds => _animateNoise3ds ?? (_animateNoise3ds = base.gameObject.GetComponentsInChildren<AnimateNoise3d>(includeInactive: true));

	public static ShowImageAnimatedView Create(ImageRef imageRef, Transform parent, bool doResolutionScaling = true, float animateNoiseScale = 1f)
	{
		return Pools.Unpool(_Blueprint, parent).GetComponent<ShowImageAnimatedView>()._SetData(imageRef, doResolutionScaling, animateNoiseScale);
	}

	private ShowImageAnimatedView _SetData(ImageRef imageRef, bool doResolutionScaling, float animationNoiseScale)
	{
		if ((bool)imageRef)
		{
			imageRef.GetTexture2D(delegate(Texture2D image)
			{
				aspectRatioFitter.aspectRatio = image.AspectRatio();
				rawImage.texture = image;
				Vector2 vector = new Vector2(Screen.width, Screen.height);
				Vector2 referenceResolution = GetComponentInParent<CanvasScaler>().referenceResolution;
				rawImage.transform.localScale = Vector3.one * (doResolutionScaling ? (Mathf.Min(1f, referenceResolution.Multiply(vector.Inverse()).Max()) * Mathf.Clamp01(image.PixelSize().ToVector2().Multiply(Vector2.Min(referenceResolution, vector).Inverse())
					.Max())) : 1f);
				onImageLoaded.Invoke();
				AnimateNoise3d[] array = animateNoise3ds;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].rangeMultiplier = animationNoiseScale;
				}
			});
		}
		return this;
	}

	public void Finish()
	{
		onFinish.Invoke();
	}
}
