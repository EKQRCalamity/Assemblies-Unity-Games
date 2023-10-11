using System.ComponentModel;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
public class AfterImageGeneratorData
{
	[ProtoContract]
	[UIField]
	public class AfterImageGeneratorTranslation
	{
		[ProtoMember(1)]
		[UIField("Interpolation Over Image Lifetime", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		private SimpleCurve _curve = SimpleCurve.EaseInOut;

		[ProtoMember(2)]
		[UIField]
		private OffsetRanges _startingTranslation;

		[ProtoMember(3)]
		[UIField]
		private OffsetRanges _endTranslation;

		public AnimationCurve curve => _curve;

		public OffsetRanges start => _startingTranslation ?? (_startingTranslation = new OffsetRanges());

		public OffsetRanges end => _endTranslation ?? (_endTranslation = new OffsetRanges());

		private bool _curveSpecified => _curve != SimpleCurve.EaseInOut;
	}

	[ProtoContract]
	[UIField]
	public class AfterImageGeneratorScale
	{
		private static readonly RangeF DEFAULT_SCALE = new RangeF(1f, 1f, 0f, 2f);

		[ProtoMember(1)]
		[UIField("Interpolation Over Image Lifetime", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
		private SimpleCurve _curve = SimpleCurve.EaseInOut;

		[ProtoMember(2)]
		[UIField]
		private RangeF _startingScale = DEFAULT_SCALE;

		[ProtoMember(3)]
		[UIField]
		private RangeF _endScale = DEFAULT_SCALE;

		[ProtoMember(4)]
		[UIField]
		private bool _syncStartAndEndScale;

		public AnimationCurve curve => _curve;

		public RangeF start => _startingScale;

		public RangeF end => _endScale;

		public bool sync => _syncStartAndEndScale;

		private bool _curveSpecified => _curve != SimpleCurve.EaseInOut;

		private bool _startingScaleSpecified => _startingScale != DEFAULT_SCALE;

		private bool _endScaleSpecified => _endScale != DEFAULT_SCALE;
	}

	private const string CAT_ADVANCED = "Advanced Settings";

	[ProtoMember(1)]
	[UIField(min = 1f / 120f, max = 0.5f, tooltip = "How often an after image is generated.")]
	[DefaultValue(1f / 120f)]
	private float _timeBetweenImages = 1f / 120f;

	[ProtoMember(3)]
	[UIField(min = 1f / 120f, max = 1f, tooltip = "How long it takes for an after image to fade out.")]
	[DefaultValue(0.25f)]
	private float _imageLifetime = 0.25f;

	[ProtoMember(4)]
	[UIField(category = "Advanced Settings", tooltip = "The color(s) the after image will take from the moment it’s created until it completely fades out.")]
	private SimpleGradient _colorOverImageLifetime = SimpleGradient.Fade;

	[ProtoMember(9)]
	[UIField(category = "Advanced Settings", tooltip = "The size of the after image from the moment it’s created until it completely fades out.")]
	private AfterImageGeneratorScale _scaleOverImageLifetime;

	[ProtoMember(8)]
	[UIField("Translation Over Image Lifetime", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Advanced Settings", tooltip = "The position of the after image from the moment it’s created until it completely fades out.")]
	private AfterImageGeneratorTranslation _translationOverLifetime;

	[ProtoMember(5)]
	[UIField("Tint Over Lifetime", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Advanced Settings", tooltip = "This can change the starting color of the after images from when the generator begins emitting to when it stops.")]
	private SimpleGradient _colorOverLifetime = SimpleGradient.Default;

	[ProtoMember(2)]
	[UIField("Emission Rate Over Lifetime", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Advanced Settings", tooltip = "Control the amount of after images generated from when the generator begins emitting to when it stops.")]
	private SimpleCurve _timeBetweenImagesOverLifetime = SimpleCurve.Default;

	[ProtoMember(6)]
	[UIField(category = "Advanced Settings", tooltip = "Determines how after images will be rendered. Set to Additive for a glow-like effect.")]
	private AfterImageMaterialsType _visualEffectStyle;

	public float timeBetweenImages => _timeBetweenImages;

	public AnimationCurve timeBetweenImagesOverLifetime => _timeBetweenImagesOverLifetime;

	public AfterImageGeneratorScale scale => _scaleOverImageLifetime ?? (_scaleOverImageLifetime = new AfterImageGeneratorScale());

	public float imageLifetime => _imageLifetime;

	public Gradient colorOverImageLifetime => _colorOverImageLifetime;

	public Gradient colorOverLifetime => _colorOverLifetime;

	public AfterImageMaterials materials => EnumUtil.GetResourceBlueprint(_visualEffectStyle).GetComponent<AfterImageMaterials>();

	public AfterImageGeneratorTranslation translation => _translationOverLifetime ?? (_translationOverLifetime = new AfterImageGeneratorTranslation());

	private bool _timeBetweenImagesOverLifetimeSpecified => _timeBetweenImagesOverLifetime != SimpleCurve.Default;

	private bool _colorOverImageLifetimeSpecified => _colorOverImageLifetime != SimpleGradient.Fade;

	private bool _colorOverLifetimeSpecified => _colorOverLifetime != SimpleGradient.Default;

	public override string ToString()
	{
		return "After Image Generator";
	}
}
