using System;
using UnityEngine;

[ScriptOrder(32766)]
public class AfterImageGenerator : MonoBehaviour
{
	public const float MIN_TIME_BETWEEN_IMAGES = 1f / 120f;

	public const float MAX_TIME_BETWEEN_IMAGE = 0.5f;

	public const float TIME_BETWEEN_IMAGES_DEFAULT = 1f / 120f;

	public const float MIN_IMAGE_LIFETIME = 1f / 120f;

	public const float MAX_IMAGE_LIFETIME = 1f;

	public const float IMAGE_LIFETIME_DEFAULT = 0.25f;

	private static GameObject _Blueprint;

	private static readonly System.Random _Random = new System.Random();

	[Header("Emission")]
	public GameObject generateImagesOf;

	[Range(1f / 120f, 0.5f)]
	public float timeBetweenImages = 1f / 120f;

	[Header("Image Lifetime")]
	[Range(1f / 120f, 1f)]
	public float imageLifetime = 0.25f;

	public Gradient colorOverImageLifetime;

	[Header("Scale")]
	public AnimationCurve scaleOverImageLifetime = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public Vector2 scaleStart = Vector2.one;

	public Vector2 scaleEnd = Vector2.one;

	public bool syncScaleStartAndEnd;

	[Header("Translation")]
	public AnimationCurve translationOverLifetime = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public Vector2 horizontalStart;

	public Vector2 verticalStart;

	public Vector2 forwardStart;

	public Vector2 horizontalEnd;

	public Vector2 verticalEnd;

	public Vector2 forwardEnd;

	[Header("Material Overrides")]
	public Material objectMaterialOverride;

	public Material afterImageMaterialOverride;

	[Header("Generator Lifetime")]
	[Range(0f, 10f)]
	public float lifetime;

	public Gradient tintOverGeneratorLifetime;

	public AnimationCurve timeBetweenImagesOverGeneratorLifetime = AnimationCurve.Constant(0f, 1f, 1f);

	public OnCompleteAction onLifetimeCompleteAction = OnCompleteAction.DisableSelf;

	private float _elapsedLifetime;

	private float _elapsed;

	private float _timeBetweenImagesMultiplier;

	private float _imageLifetimeMultiplier;

	private int _meshCount;

	private bool? _hasTranslation;

	private bool? _hasScale;

	public static GameObject Blueprint
	{
		get
		{
			if (!_Blueprint)
			{
				return _Blueprint = Resources.Load<GameObject>("Graphics/AfterImages/AfterImageGenerator");
			}
			return _Blueprint;
		}
	}

	public float activeImageMax => imageLifetime / timeBetweenImages * (float)_meshCount * timeBetweenImagesOverGeneratorLifetime.ApproximateAverage();

	protected bool hasTranslation
	{
		get
		{
			bool? flag = _hasTranslation;
			if (!flag.HasValue)
			{
				bool? flag2 = (_hasTranslation = horizontalStart.AbsMax() + verticalStart.AbsMax() + forwardStart.AbsMax() + horizontalEnd.AbsMax() + verticalEnd.AbsMax() + forwardEnd.AbsMax() > 0f);
				return flag2.Value;
			}
			return flag.GetValueOrDefault();
		}
	}

	protected bool hasScale
	{
		get
		{
			bool? flag = _hasScale;
			if (!flag.HasValue)
			{
				bool? flag2 = (_hasScale = scaleStart != Vector2.one || scaleEnd != Vector2.one);
				return flag2.Value;
			}
			return flag.GetValueOrDefault();
		}
	}

	private Vector2? _GetScale()
	{
		if (!hasScale)
		{
			return null;
		}
		if (!syncScaleStartAndEnd)
		{
			return new Vector2(_Random.Range(scaleStart), _Random.Range(scaleEnd));
		}
		float t = _Random.Value();
		return new Vector2(scaleStart.Lerp(t), scaleEnd.Lerp(t));
	}

	private void OnEnable()
	{
		_elapsed = 0f;
		_elapsedLifetime = 0f;
		_timeBetweenImagesMultiplier = 1f;
		_imageLifetimeMultiplier = 1f;
		_meshCount = 1;
		_hasTranslation = null;
		_hasScale = null;
	}

	private void LateUpdate()
	{
		float num = ((lifetime > 0f) ? Mathf.Clamp01((_elapsedLifetime += Time.deltaTime) / lifetime) : 0f);
		float num2 = ((lifetime > 0f) ? (timeBetweenImages / timeBetweenImagesOverGeneratorLifetime.Evaluate(num).Max(0.0001f)) : timeBetweenImages) * _timeBetweenImagesMultiplier;
		if ((_elapsed += Time.deltaTime) >= num2)
		{
			_meshCount = AfterImageManager.Instance.CreateAfterImage(generateImagesOf ? generateImagesOf : base.gameObject, imageLifetime * _imageLifetimeMultiplier, colorOverImageLifetime, (lifetime > 0f) ? new Color?(tintOverGeneratorLifetime.Evaluate(num)) : null, objectMaterialOverride, afterImageMaterialOverride, scaleOverImageLifetime, _GetScale(), translationOverLifetime, hasTranslation ? new Vector3?(new Vector3(_Random.Range(horizontalStart), _Random.Range(verticalStart), _Random.Range(forwardStart))) : null, hasTranslation ? new Vector3?(new Vector3(_Random.Range(horizontalEnd), _Random.Range(verticalEnd), _Random.Range(forwardEnd))) : null);
			_elapsed = 0f;
		}
		if (num >= 1f)
		{
			this.DoOnCompleteAction(onLifetimeCompleteAction);
		}
	}

	public AfterImageGenerator SetData(GameObject generateImagesOf, AfterImageGeneratorData data, float lifetime = 0f, OnCompleteAction onCompleteAction = OnCompleteAction.DeactivateGameObject)
	{
		this.generateImagesOf = generateImagesOf;
		timeBetweenImages = data.timeBetweenImages;
		timeBetweenImagesOverGeneratorLifetime = data.timeBetweenImagesOverLifetime;
		imageLifetime = data.imageLifetime;
		this.lifetime = lifetime;
		colorOverImageLifetime = data.colorOverImageLifetime;
		tintOverGeneratorLifetime = data.colorOverLifetime;
		objectMaterialOverride = data.materials.objectMaterialOverride;
		afterImageMaterialOverride = data.materials.afterImageMaterialOverride;
		scaleOverImageLifetime = data.scale.curve;
		scaleStart = data.scale.start;
		scaleEnd = data.scale.end;
		syncScaleStartAndEnd = data.scale.sync;
		translationOverLifetime = data.translation.curve;
		horizontalStart = data.translation.start.horizontal;
		verticalStart = data.translation.start.vertical;
		forwardStart = data.translation.start.forward;
		horizontalEnd = data.translation.end.horizontal;
		verticalEnd = data.translation.end.vertical;
		forwardEnd = data.translation.end.forward;
		onLifetimeCompleteAction = onCompleteAction;
		return this;
	}

	public void SetMultipliers(float timeBetweenImagesMultiplier, float imageLifetimeMultiplier)
	{
		_timeBetweenImagesMultiplier = timeBetweenImagesMultiplier;
		_imageLifetimeMultiplier = imageLifetimeMultiplier;
	}

	public void Finish()
	{
		base.gameObject.SetActive(value: false);
	}
}
