using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;
using ProtoBuf.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using VisualDesignCafe.Rendering.Nature;

[ProtoContract]
[UIField]
[UIDeepValidate]
[Localize(reflectedUI = true)]
public class ProfileOptions
{
	[ProtoContract]
	[UIField]
	public class GameOptions
	{
		[ProtoContract]
		[UIField]
		public class Preferences
		{
			[ProtoMember(1)]
			[UIField(tooltip = "Determines when the targeting step of an ability with only one valid target is skipped.")]
			[DefaultValue(AutoSelectSingleTargetType.Reactions)]
			private AutoSelectSingleTargetType _autoSelectSingleTarget = AutoSelectSingleTargetType.Reactions;

			[ProtoMember(2)]
			[UIField(tooltip = "Skip fill and pour animations for mana vials, and skip confirmation clicks.\nHold click to continue adding additional levels.")]
			private bool _quickManaVialAnimations;

			[ProtoMember(3)]
			[UIField(tooltip = "Localize", onValueChangedMethod = "_OnShowGearIconChange")]
			[DefaultValue(true)]
			private bool _showGearIcon = true;

			[ProtoMember(4)]
			[UIField(tooltip = "Localize")]
			[DefaultValue(true)]
			private bool _leaderboardEnabled = true;

			public AutoSelectSingleTargetType autoSelectSingleTarget => _autoSelectSingleTarget;

			public bool quickLevelUp => _quickManaVialAnimations;

			public bool showGearIcon => _showGearIcon;

			public bool leaderboard => _leaderboardEnabled;

			private void _OnShowGearIconChange()
			{
				PauseMenu.Instance?.onShowGearIconChange?.Invoke(showGearIcon);
			}
		}

		[ProtoContract]
		[UIField]
		public class UIOptions
		{
			private const float ABILITY_TOOLTIP_TIME = 2f;

			[ProtoMember(4)]
			[UIField(tooltip = "Increases the size of cards when they are in your hand.", min = 1f, max = 1.5f, onValueChangedMethod = "_OnMainHandScaleChange", view = "UI/Slider Advanced", defaultValue = 1f)]
			[DefaultValue(1f)]
			private float _mainHandScale = 1f;

			[ProtoMember(1)]
			[UIField(tooltip = "Determines if tutorial cards and tutorial idle messages are shown.")]
			[DefaultValue(true)]
			private bool _tutorialMessagesEnabled = true;

			[ProtoMember(2)]
			[UIField("3d Tooltips Enabled", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Determines if floating text tooltips for 3d objects are shown.")]
			[DefaultValue(true)]
			private bool _3dTooltipsEnabled = true;

			[ProtoMember(3)]
			[UIField(tooltip = "Determines if tooltips are shown for map nodes.")]
			[DefaultValue(true)]
			private bool _mapTooltipsEnabled = true;

			[ProtoMember(5)]
			[UIField(tooltip = "Have narrative text typing synchronized with the voice over associated with it.")]
			[DefaultValue(true)]
			private bool _syncNarrativeTextWithVoiceOver = true;

			[ProtoMember(7)]
			[UIField(validateOnChange = true, tooltip = "Localize")]
			[DefaultValue(true)]
			private bool _abilityHoverOverTooltipsEnabled = true;

			[ProtoMember(6)]
			[UIField(min = 0.5f, max = 5, tooltip = "How long you must hover over an ability in your hand before additional information is shown.")]
			[DefaultValue(2f)]
			[UIHideIf("_hideAbilityHoverOverTooltipTime")]
			private float _abilityHoverOverTooltipTime = 2f;

			[ProtoMember(9)]
			[UIField(validateOnChange = true, tooltip = "Localize")]
			[DefaultValue(true)]
			private bool _enemyHoverOverTooltipsEnabled = true;

			[ProtoMember(10)]
			[UIField(min = 0.5f, max = 5f, tooltip = "Localize")]
			[DefaultValue(2f)]
			[UIHideIf("_hideEnemyHoverOverTooltipTime")]
			private float _enemyHoverOverTooltipTime = 2f;

			[ProtoMember(8)]
			[UIField(tooltip = "Localize", onValueChangedMethod = "_OnShowPotentialDamageChange")]
			[DefaultValue(true)]
			private bool _showPotentialCombatDamage = true;

			public bool tutorialEnabled => _tutorialMessagesEnabled;

			public bool cancelButtonEnabled => true;

			public bool skipConfirmationForOpenWebPageRequests => false;

			public bool cancelButtonOnLeft => true;

			public float mainHandScale => _mainHandScale;

			public bool syncNarrativeText => _syncNarrativeTextWithVoiceOver;

			public float abilityHoverOverTooltipTime => _abilityHoverOverTooltipTime;

			public bool abilityHoverTips => _abilityHoverOverTooltipsEnabled;

			public float enemyHoverOverTooltipTime => _enemyHoverOverTooltipTime;

			public bool enemyHoverTips => _enemyHoverOverTooltipsEnabled;

			public bool potentialCombatDamage => _showPotentialCombatDamage;

			public bool this[TooltipOptionType option] => option switch
			{
				TooltipOptionType.ThreeDimensional => _3dTooltipsEnabled, 
				TooltipOptionType.Map => _mapTooltipsEnabled, 
				_ => false, 
			};

			private bool _hideAbilityHoverOverTooltipTime => !_abilityHoverOverTooltipsEnabled;

			private bool _hideEnemyHoverOverTooltipTime => !_enemyHoverOverTooltipsEnabled;

			public static event Action<float> OnMainHandScaleChange;

			public static event Action<bool> OnShowPotentialDamageChange;

			private void _OnMainHandScaleChange()
			{
				UIOptions.OnMainHandScaleChange?.Invoke(_mainHandScale);
			}

			private void _OnShowPotentialDamageChange()
			{
				UIOptions.OnShowPotentialDamageChange?.Invoke(_showPotentialCombatDamage);
			}
		}

		[ProtoContract]
		[UIField]
		public class UgcOptions
		{
			[ProtoContract]
			[UIField]
			public class AdvancedOptions
			{
				private const float PREVIEW_TIMEOUT = 5f;

				[ProtoMember(1)]
				[UIField(min = 2, max = 30, tooltip = "Determines how long a web request to retrieve a workshop preview image must be inactive before it is aborted.\n<i>If you are having troubles with workshop preview images not showing up in searches, increasing this may help.</i>", view = "UI/Slider Advanced", defaultValue = 5f)]
				[DefaultValue(5f)]
				private float _previewImageRequestTimeout = 5f;

				public float previewTimeout => _previewImageRequestTimeout;
			}

			[ProtoMember(6)]
			[UIField(collapse = UICollapseType.Hide)]
			private ParentalLock _parentalLock;

			[ProtoMember(5)]
			[UIField("Enable User Generated Content", 0u, null, null, null, null, null, null, false, null, 5, false, null, validateOnChange = true, tooltip = "Would you like to enable all Steam Workshop UGC features?", dynamicInitMethod = "_InitCommon")]
			[DefaultValue(true)]
			private bool _enabled = true;

			[ProtoMember(1)]
			[UIField(maxCount = 0, tooltip = "Which types of mature content would you like to include in UGC searches?", dynamicInitMethod = "_InitCommon")]
			[UIHideIf("_hideCommon")]
			private MatureContentFlags _enableMatureContent;

			[ProtoMember(2)]
			[UIField(tooltip = "Would you like to include content with overwhelmingly negative reviews in UGC searches?", dynamicInitMethod = "_InitCommon")]
			[UIHideIf("_hideCommon")]
			private bool _enableDownVotedContent;

			[ProtoMember(3)]
			[UIField(tooltip = "Would you like to include content from creators with overwhelmingly negative reviews in UGC searches?", dynamicInitMethod = "_InitCommon")]
			[UIHideIf("_hideCommon")]
			private bool _enableDownVotedCreators;

			[ProtoMember(4)]
			[UIField(tooltip = "Would you like to include content which has not yet been reviewed by the community in UGC searches?", dynamicInitMethod = "_InitCommon")]
			[DefaultValue(true)]
			[UIHideIf("_hideCommon")]
			private bool _enableContentPendingReview = true;

			[ProtoMember(7)]
			[UIField]
			private AdvancedOptions _advanced;

			public bool enabled => _enabled;

			public bool enableDownVotedCreators => _enableDownVotedCreators;

			public AdvancedOptions advanced => _advanced ?? (_advanced = new AdvancedOptions());

			private bool _hideCommon => !_enabled;

			public bool Visible(MatureContentFlags mature, bool downVoted, bool pendingReview)
			{
				if (EnumUtil.HasFlags(_enableMatureContent, mature) && (_enableDownVotedContent || !downVoted))
				{
					if (!_enableContentPendingReview)
					{
						return !pendingReview;
					}
					return true;
				}
				return false;
			}

			private void _InitCommon(UIFieldAttribute uiField)
			{
				uiField.readOnly = _parentalLock;
			}
		}

		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Hide, order = 3u)]
		private Preferences _preferences;

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Hide, order = 2u)]
		private UIOptions _ui;

		private UgcOptions _userGeneratedContent;

		public Preferences preferences => _preferences ?? (_preferences = new Preferences());

		public UIOptions ui => _ui ?? (_ui = new UIOptions());

		public UgcOptions ugc => _userGeneratedContent ?? (_userGeneratedContent = new UgcOptions());

		[UIField(order = 1u, onValueChangedMethod = "_OnLanguageChange")]
		public LocaleIdentifier language
		{
			get
			{
				return ProfileManager.prefs.locale.Identifier;
			}
			set
			{
				ProfileManager.prefs.localeOverride = LocalizationSettings.AvailableLocales.GetLocale(value);
			}
		}

		private void _OnLanguageChange()
		{
			PauseMenu.Instance?.RefreshOptionsMenu();
			ProfileManager.Profile.SavePreferences();
		}
	}

	[ProtoContract]
	[UIField]
	public class VideoOptions
	{
		[ProtoContract]
		[UIField]
		public class DisplayOptions
		{
			[UIField]
			[ProtoContract(EnumPassthru = true)]
			public enum DynamicResolutionFilter
			{
				None,
				[UIField("Fidelity FX Super Resolution", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
				FidelityFXSuperResolution
			}

			[ProtoMember(1)]
			private Resolution _resolution;

			[UIField("Resolution", 0u, null, null, null, null, null, null, false, null, 5, false, null, validateOnChange = true)]
			private Enum _resolutionEnum;

			[ProtoMember(2)]
			[UIField(validateOnChange = true)]
			[DefaultValue(true)]
			private bool _fullscreen = true;

			[ProtoMember(3)]
			[UIField(validateOnChange = true)]
			[DefaultValue(true)]
			[UIHorizontalLayout("FrameRate", flexibleWidth = 0f)]
			private bool _vsync = true;

			[ProtoMember(4)]
			[UIField(min = 30, max = 300, dynamicInitMethod = "_InitTargetFrameRate", onValueChangedMethod = "_OnTargetFrameRateChange")]
			[DefaultValue(60)]
			[UIHideIf("_hideTargetFrameRate")]
			[UIHorizontalLayout("FrameRate", flexibleWidth = 999f)]
			private ushort _targetFrameRate = 60;

			[ProtoMember(6)]
			[UIField(validateOnChange = true)]
			[DefaultValue(DynamicResolutionFilter.FidelityFXSuperResolution)]
			[UIHorizontalLayout("Scale", expandHeight = false)]
			private DynamicResolutionFilter _resolutionScaling = DynamicResolutionFilter.FidelityFXSuperResolution;

			[ProtoMember(5)]
			[UIField(min = 50, max = 100)]
			[DefaultValue(100)]
			[UIHideIf("_hideResolutionScale")]
			[UIHorizontalLayout("Scale", expandHeight = false)]
			private byte _resolutionScale = 100;

			[ProtoMember(7)]
			[UIField("VSync Count", 0u, null, null, null, null, null, null, false, null, 5, false, null, min = 1, max = 4, onValueChangedMethod = "_OnVSyncCountChange", tooltip = "Localized")]
			[DefaultValue(1)]
			[UIHideIf("_hideVSyncCount")]
			[UIHorizontalLayout("FrameRate", flexibleWidth = 999f)]
			private int _vSyncCount = 1;

			protected Enum resolutionEnum => EnumGenerator.InsureValid(EnumGenerator.Types.DisplayResolution, ref _resolutionEnum, _GetFallbackResolutionString);

			public int targetFrameRate
			{
				get
				{
					if (_hideTargetFrameRate)
					{
						return -1;
					}
					return _targetFrameRate;
				}
			}

			public byte resolutionScale
			{
				get
				{
					return _resolutionScale;
				}
				set
				{
					_resolutionScale = Math.Max((byte)50, Math.Min(value, (byte)100));
				}
			}

			private bool _hideTargetFrameRate => _vsync;

			private bool _hideVSyncCount => !_vsync;

			private bool _hideResolutionScale => _resolutionScaling != DynamicResolutionFilter.FidelityFXSuperResolution;

			static DisplayOptions()
			{
				DynamicResolutionHandler.SetDynamicResScaler(GetDynamicResolution, DynamicResScalePolicyType.ReturnsPercentage);
			}

			private static float GetDynamicResolution()
			{
				return (int)ProfileManager.options.video.display._resolutionScale;
			}

			private string _GetFallbackResolutionString()
			{
				return (_resolution.IsValidResolution() ? _resolution : GraphicsUtil.GetNativeResolution()).ToString();
			}

			public void ApplyChanges()
			{
				OnValidateUI();
			}

			public void SetPerformanceOptions()
			{
			}

			public void SetSafeModeOptions()
			{
				SetPerformanceOptions();
				_resolutionScale = 50;
			}

			private void _InitTargetFrameRate(UIFieldAttribute uiField)
			{
				if (Application.isEditor)
				{
					uiField.min = 5;
				}
			}

			private void _OnTargetFrameRateChange()
			{
				Application.targetFrameRate = targetFrameRate;
			}

			private void _OnVSyncCountChange()
			{
				QualitySettings.vSyncCount = _vSyncCount;
			}

			private void OnValidateUI()
			{
				_resolution = default(Resolution).FromString(resolutionEnum.ToString());
				Screen.SetResolution(_resolution.width, _resolution.height, _fullscreen, _resolution.refreshRate);
				QualitySettings.vSyncCount = (_vsync ? _vSyncCount : 0);
				_OnTargetFrameRateChange();
				_OnResolutionScalingChange();
			}

			private void _OnResolutionScalingChange()
			{
				Camera main = Camera.main;
				if ((object)main != null)
				{
					HDAdditionalCameraData component = main.GetComponent<HDAdditionalCameraData>();
					component.allowDynamicResolution = _resolutionScaling != DynamicResolutionFilter.None;
					component.allowDeepLearningSuperSampling = false;
				}
			}
		}

		[ProtoContract]
		[UIField]
		public class QualityOptions
		{
			[ProtoContract(EnumPassthru = true)]
			[UISortEnum(UISortEnumType.Value)]
			public enum ParticleQuality
			{
				Minimum = -2,
				ExtremelyLow,
				VeryLow,
				Low,
				Medium,
				High,
				VeryHigh
			}

			[ProtoContract(EnumPassthru = true)]
			public enum FoliageQuality
			{
				Off,
				VeryLow,
				Low,
				Medium,
				High
			}

			[ProtoMember(1)]
			private string _qualitySetting;

			[UIField("Quality Preset", 0u, null, null, null, null, null, null, false, null, 5, false, null, onValueChangedMethod = "OnValueChanged", order = 1u, validateOnChange = true)]
			private Enum _qualitySettingEnum;

			[ProtoMember(2)]
			[UIField]
			[DefaultValue(ParticleQuality.Medium)]
			private ParticleQuality _particleQuality = ParticleQuality.Medium;

			[ProtoMember(3)]
			[UIField(onValueChangedMethod = "_OnFoliageQualityChange")]
			[DefaultValue(FoliageQuality.Medium)]
			private FoliageQuality _foliageQuality = FoliageQuality.Medium;

			public FoliageQuality foliageQuality => _foliageQuality;

			public float foliageDensity => _foliageQuality.GetDensity();

			public int maxProjectileBurstLightCount => (int)(_particleQuality + 1);

			public static event Action<FoliageQuality> OnFoliageQualityChange;

			public QualityOptions()
			{
				_Init();
			}

			private void _Init()
			{
				_qualitySettingEnum = EnumGenerator.TryParse(EnumGenerator.Types.QualitySettings, _qualitySetting, GraphicsUtil.GetQualitySettingName(GraphicsUtil.GetRecommendedQualitySettingIndex()));
			}

			public void ApplyChanges()
			{
				OnValueChanged();
				ProjectileMediaView.EmissionMultiplier = _particleQuality.GetEmissionMultiplier();
				ProjectileMediaView.LaunchMultiplier = _particleQuality.GetLaunchMultiplier();
			}

			public void SetPerformanceOptions()
			{
				_qualitySettingEnum = Enum.GetValues(_qualitySettingEnum.GetType()).GetLastValue<Enum>();
				_particleQuality = ParticleQuality.ExtremelyLow;
				_foliageQuality = FoliageQuality.VeryLow;
				_OnFoliageQualityChange();
			}

			public void SetSafeModeOptions()
			{
				_qualitySettingEnum = Enum.GetValues(_qualitySettingEnum.GetType()).GetLastValue<Enum>();
				_particleQuality = ParticleQuality.Minimum;
				_foliageQuality = FoliageQuality.Off;
				_OnFoliageQualityChange();
			}

			private void OnValidateUI()
			{
				_OnFoliageQualityChange();
			}

			private void OnValueChanged()
			{
				_qualitySetting = _qualitySettingEnum.ToString();
				QualitySettings.SetQualityLevel(GraphicsUtil.GetQualitySettingIndex(_qualitySetting), applyExpensiveChanges: true);
			}

			private void _OnFoliageQualityChange()
			{
				if ((bool)RefreshNatureRenderer.Instance)
				{
					RefreshNatureRenderer.Instance.detailDensity = foliageDensity;
				}
				QualityOptions.OnFoliageQualityChange?.Invoke(_foliageQuality);
			}

			[ProtoAfterDeserialization]
			private void _ProtoAfterDeserialization()
			{
				_Init();
			}
		}

		[ProtoContract]
		[UIField]
		public class PostProcessingOptions
		{
			private const bool MOTION_BLUR = true;

			private const bool DOF = true;

			private const bool AMBIENT_OCCLUSION = true;

			private const bool BLOOM = true;

			private const bool VOLUMETRIC = true;

			[ProtoMember(8)]
			[UIField("MSAA", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Localize", onValueChangedMethod = "_OnValueChange")]
			private MultiSampleType _msaa;

			[ProtoMember(1)]
			[UIField(tooltip = "Blur objects which are moving quickly.", onValueChangedMethod = "_OnValueChange")]
			[DefaultValue(true)]
			private bool _motionBlur = true;

			[ProtoMember(2)]
			[UIField(tooltip = "Blur objects which are not the current focus of the camera.", onValueChangedMethod = "_OnValueChange")]
			[DefaultValue(true)]
			private bool _depthOfField = true;

			[ProtoMember(3)]
			[UIField(tooltip = "Simulate shadowing caused by objects blocking ambient light.", onValueChangedMethod = "_OnValueChange")]
			[DefaultValue(true)]
			private bool _ambientOcclusion = true;

			[ProtoMember(4)]
			[UIField(tooltip = "Make brightly lit regions of a scene glow.", onValueChangedMethod = "_OnValueChange")]
			[DefaultValue(true)]
			private bool _bloom = true;

			[ProtoMember(5)]
			[UIField(tooltip = "Simulate light traveling through particles in the air.", onValueChangedMethod = "_OnVolumetricLightingChange")]
			[DefaultValue(true)]
			private bool _volumetricLighting = true;

			[ProtoMember(6)]
			[UIField(tooltip = "Adventure Table lighting will have color and intensity animated.\n<i>Setting takes effect next time lighting would change.</i>")]
			[DefaultValue(true)]
			private bool _animatedLighting = true;

			[ProtoMember(7)]
			[UIField(tooltip = "Lights will cast shadows.")]
			[DefaultValue(true)]
			private bool _shadows = true;

			public bool volumetricLighting => _volumetricLighting;

			public bool animatedLighting => _animatedLighting;

			public bool shadows => _shadows;

			public static event Action<bool> OnVolumetricLightingChange;

			public void ApplyChanges()
			{
				Camera main = Camera.main;
				if ((object)main != null)
				{
					HDAdditionalCameraData component = main.GetComponent<HDAdditionalCameraData>();
					if ((object)component != null)
					{
						component.customRenderingSettings = true;
						FrameSettingsOverrideMask renderingPathCustomFrameSettingsOverrideMask = component.renderingPathCustomFrameSettingsOverrideMask;
						renderingPathCustomFrameSettingsOverrideMask.mask[82u] = true;
						renderingPathCustomFrameSettingsOverrideMask.mask[81u] = true;
						renderingPathCustomFrameSettingsOverrideMask.mask[24u] = true;
						renderingPathCustomFrameSettingsOverrideMask.mask[84u] = true;
						renderingPathCustomFrameSettingsOverrideMask.mask[27u] = true;
						component.renderingPathCustomFrameSettingsOverrideMask = renderingPathCustomFrameSettingsOverrideMask;
						FrameSettings renderingPathCustomFrameSettings = component.renderingPathCustomFrameSettings;
						renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.MotionBlur, _motionBlur);
						renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.DepthOfField, _depthOfField);
						renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.SSAO, _ambientOcclusion);
						renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Bloom, _bloom);
						renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.StopNaN, _bloom && QualitySettings.GetQualityLevel() == 2);
						renderingPathCustomFrameSettings.msaaMode = _msaa.GetMode();
						component.renderingPathCustomFrameSettings = renderingPathCustomFrameSettings;
						_OnVolumetricLightingChange();
					}
				}
				GameManager instance = GameManager.Instance;
				if ((object)instance != null && (bool)instance)
				{
					instance.mainLight?.GetComponent<HDAdditionalLightData>()?.EnableShadows(shadows);
					if (!shadows)
					{
						instance.spotLight?.GetComponent<HDAdditionalLightData>()?.EnableShadows(enabled: false);
					}
				}
			}

			public void SetPerformanceOptions()
			{
				_motionBlur = (_depthOfField = (_volumetricLighting = false));
			}

			public void SetSafeModeOptions()
			{
				SetPerformanceOptions();
				_ambientOcclusion = (_bloom = (_animatedLighting = (_shadows = false)));
				_msaa = MultiSampleType.Off;
			}

			private void _OnValueChange()
			{
				ApplyChanges();
			}

			private void _OnVolumetricLightingChange()
			{
				PostProcessingOptions.OnVolumetricLightingChange?.Invoke(volumetricLighting);
			}
		}

		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Hide)]
		[UIHeader("Display")]
		private DisplayOptions _display;

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Hide)]
		[UIMargin(12f, false)]
		[UIHeader("Quality")]
		private QualityOptions _qualitySettings;

		[ProtoMember(3)]
		[UIField(collapse = UICollapseType.Hide)]
		[UIMargin(12f, false)]
		[UIHeader("Effects")]
		private PostProcessingOptions _postProcessing;

		public DisplayOptions display => _display ?? (_display = new DisplayOptions());

		public QualityOptions quality => _qualitySettings ?? (_qualitySettings = new QualityOptions());

		public PostProcessingOptions postProcessing => _postProcessing ?? (_postProcessing = new PostProcessingOptions());

		public void ApplyChanges()
		{
			display.ApplyChanges();
			quality.ApplyChanges();
			postProcessing.ApplyChanges();
		}

		public void SetPerformanceOptions(bool applyChanges = true)
		{
			display.SetPerformanceOptions();
			quality.SetPerformanceOptions();
			postProcessing.SetPerformanceOptions();
			if (applyChanges)
			{
				ApplyChanges();
			}
		}

		public void SetSafeModeOptions(bool applyChanges = true)
		{
			display.SetSafeModeOptions();
			quality.SetSafeModeOptions();
			postProcessing.SetSafeModeOptions();
			if (applyChanges)
			{
				ApplyChanges();
			}
		}
	}

	[ProtoContract]
	[UIField]
	public class AudioOptions
	{
		private const float MASTER_VOLUME = 0.75f;

		private const float SOUND_EFFECT_VOLUME = 0.75f;

		private const float MUSIC_VOLUME = 0.75f;

		private const float AMBIENT_VOLUME = 0.75f;

		private const float NARRATION_VOLUME = 0.75f;

		private const float STEP_SIZE = 0.01f;

		[ProtoMember(1)]
		[UIField(min = 0, max = 1, stepSize = 0.01f, view = "UI/Slider Advanced", defaultValue = 0.75f)]
		[DefaultValue(0.75f)]
		private float _masterVolume = 0.75f;

		[ProtoMember(2)]
		[UIField(min = 0, max = 1, stepSize = 0.01f, view = "UI/Slider Advanced", defaultValue = 0.75f)]
		[DefaultValue(0.75f)]
		private float _soundEffectVolume = 0.75f;

		[ProtoMember(3)]
		[UIField(min = 0, max = 1, stepSize = 0.01f, view = "UI/Slider Advanced", defaultValue = 0.75f)]
		[DefaultValue(0.75f)]
		private float _musicVolume = 0.75f;

		[ProtoMember(4)]
		[UIField(min = 0, max = 1, stepSize = 0.01f, view = "UI/Slider Advanced", defaultValue = 0.75f)]
		[DefaultValue(0.75f)]
		private float _ambientVolume = 0.75f;

		[ProtoMember(5)]
		[UIField(min = 0, max = 1, stepSize = 0.01f, view = "UI/Slider Advanced", defaultValue = 0.75f)]
		[DefaultValue(0.75f)]
		private float _narrationVolume = 0.75f;

		public float masterVolume => _masterVolume;

		public float soundEffectVolume => _soundEffectVolume;

		public float musicVolume => _musicVolume;

		public float ambientVolume => _ambientVolume;

		public float uiVolume => 0.75f;

		public float narrationVolume => _narrationVolume;

		public void ApplyChanges()
		{
			_ = MasterMixManager.Instance;
		}
	}

	[ProtoContract]
	[UIField]
	public class ControlOptions
	{
		[ProtoContract]
		[UIField]
		public class KeyBinds
		{
			[ProtoMember(1)]
			[UIField(tooltip = "Takes you to the previous state of most user interfaces.\n<i>I.e. Cancel Attack, Cancel Ability, etc.</i>")]
			private KeyBind _back = new KeyBind(KeyAction.Back, KeyCode.Mouse3, KeyCode.Backspace);

			[ProtoMember(2)]
			[UIField(tooltip = "Takes you to the next state of most user interfaces.\n<i>I.e. End Turn, Confirm Attack, Confirm Defense etc.</i>")]
			private KeyBind _finish = new KeyBind(KeyAction.Finish, KeyCode.Space, KeyCode.Return);

			[ProtoMember(3)]
			private KeyBind _pause = new KeyBind(KeyAction.Pause, KeyCode.Escape);

			public KeyBind this[KeyAction action] => _GetBindings().FirstOrDefault((KeyBind keybind) => keybind.action == action);

			public KeyBind this[KeyCode key] => _GetBindings().FirstOrDefault((KeyBind keybind) => keybind.GetKeyCodes().Any((KeyCode keyCode) => key == keyCode));

			private IEnumerable<KeyBind> _GetBindings()
			{
				yield return _back;
				yield return _finish;
				yield return _pause;
			}

			private IEnumerable<KeyCode> _GetDuplicateKeyCodes()
			{
				return (from key in _GetBindings().SelectMany((KeyBind keybind) => keybind.GetKeyCodes())
					group key by key).SelectMany((IGrouping<KeyCode, KeyCode> g) => g.Take(g.Count() - 1));
			}

			private void _ClearDuplicateKeyCodes()
			{
				_GetDuplicateKeyCodes().EffectAll(UnbindKey);
			}

			public void ApplyChanges()
			{
				InputManager.I.bindings.SetBindings(_GetBindings());
			}

			public void UnbindKey(KeyCode key)
			{
				this[key]?.UnbindKey(key);
			}

			[ProtoAfterDeserialization]
			private void _ProtoAfterDeserialization()
			{
				_ClearDuplicateKeyCodes();
			}

			private void OnValidateUI()
			{
				ApplyChanges();
			}
		}

		[ProtoContract]
		[UIField]
		public class KeyBind
		{
			[ProtoMember(1, OverwriteList = true)]
			[UIField(maxCount = 2)]
			private KeyCode?[] _keyCodes;

			private KeyAction _action;

			public KeyCode?[] keyCodes => CollectionUtil.Resize(ref _keyCodes, 2);

			public KeyAction action => _action;

			public bool isValid => keyCodes.Any((KeyCode? keyCode) => keyCode.HasValue);

			private KeyBind()
			{
			}

			public KeyBind(KeyAction action, params KeyCode[] defaultKeyCodes)
			{
				_action = action;
				for (int i = 0; i < Mathf.Min(2, defaultKeyCodes.Length); i++)
				{
					keyCodes[i] = defaultKeyCodes[i];
				}
			}

			public IEnumerable<KeyCode> GetKeyCodes()
			{
				return from keyCode in keyCodes
					where keyCode.HasValue
					select keyCode.Value;
			}

			public void UnbindKey(KeyCode key)
			{
				for (int i = 0; i < keyCodes.Length; i++)
				{
					if (keyCodes[i] == key)
					{
						keyCodes[i] = null;
					}
				}
			}

			public static implicit operator bool(KeyBind keyBind)
			{
				return keyBind?.isValid ?? false;
			}
		}

		[ProtoContract]
		[UIField]
		public class ClickingAndDragging
		{
			private const float CLICK_TIME = 0.5f;

			private const float DOUBLE_CLICK_TIME = 0.5f;

			private const float SENSITIVITY = 1f;

			private const float DPI_SCALE = 0.5f;

			[ProtoMember(1)]
			[UIField(min = 0.25f, max = 1f, tooltip = "Determines how quickly, in seconds, a button must be pressed in order to be considered a click.\n<i>When a button is down for longer than this threshold it is considered held.</i>", view = "UI/Slider Advanced", defaultValue = 0.5f)]
			[DefaultValue(0.5f)]
			private float _clickTime = 0.5f;

			private float _doubleClickTime = 0.5f;

			[ProtoMember(3)]
			[UIField(min = 0.5f, max = 3f, tooltip = "Drag sensitivity used when interacting with Ability Cards and other UI items that are presented as a hand.\n<i>Higher values make dragging more responsive, but can interfere with clicks.</i>", view = "UI/Slider Advanced", defaultValue = 1f)]
			[DefaultValue(1f)]
			private float _cardDragSensitivity = 1f;

			private float _cameraDragSensitivity = 1f;

			private float clickTime => _clickTime;

			private float doubleClickThreshold => _doubleClickTime;

			public int cardDragThreshold => Mathf.RoundToInt(Screen.dpi * 0.5f / _cardDragSensitivity);

			private float cameraDragThreshold => Screen.dpi * 0.5f / _cameraDragSensitivity;

			public void ApplyChanges()
			{
				InputManager.I.ClickThreshold = clickTime;
				InputManager.I.DoubleClickThreshold = doubleClickThreshold;
				InputManager.I.DragDistanceThreshold = cameraDragThreshold;
			}
		}

		public const int MAX_BINDS = 2;

		public static readonly HashSet<KeyCode> ReservedKeyCodes;

		[ProtoMember(1)]
		[UIField(onValueChangedMethod = "_OnKeybindsChanged", collapse = UICollapseType.Hide)]
		[UIHeader("Key Bindings")]
		private KeyBinds _keyBinds;

		[ProtoMember(3)]
		[UIField(collapse = UICollapseType.Hide)]
		[UIHeader("Mouse Settings")]
		private ClickingAndDragging _clickingAndDragging;

		public KeyBinds keyBinds => _keyBinds ?? (_keyBinds = new KeyBinds());

		public ClickingAndDragging clickingAndDragging => _clickingAndDragging ?? (_clickingAndDragging = new ClickingAndDragging());

		static ControlOptions()
		{
			ReservedKeyCodes = new HashSet<KeyCode>
			{
				KeyCode.Mouse0,
				KeyCode.Mouse1,
				KeyCode.Mouse2,
				KeyCode.Escape
			};
			RuntimeTypeModel.Default.Add(typeof(KeyBind), applyDefaultBehaviour: true)[1].SupportNull = true;
		}

		public void ApplyChanges()
		{
			keyBinds.ApplyChanges();
			clickingAndDragging.ApplyChanges();
		}

		private void _OnKeybindsChanged()
		{
			keyBinds.ApplyChanges();
		}
	}

	[ProtoContract]
	[UIField]
	public class CosmeticOptions
	{
		[ProtoMember(1)]
		[UIField(excludedValuesMethod = "_ExcludePlayingCardDeck", onValueChangedMethod = "_OnPlayingCardDeckChange")]
		private PlayingCardSkinType _playingCardDeck;

		[ProtoMember(2)]
		[UIField(excludedValuesMethod = "_ExcludeTable", onValueChangedMethod = "_OnTableChange")]
		private TableSkinType _table;

		[ProtoMember(3)]
		[UIField(excludedValuesMethod = "_ExcludeToken", onValueChangedMethod = "_OnTokenChange")]
		private TokenSkinType _token;

		public PlayingCardSkinType playingCardDeck => _playingCardDeck;

		public TableSkinType table => _table;

		public TokenSkinType token => _token;

		public static event Action<PlayingCardSkinType> OnPlayingCardDeckChange;

		public static event Action<TableSkinType> OnTableChange;

		public static event Action<TokenSkinType> OnTokenChange;

		public bool HasCosmetics()
		{
			if (!EnumUtil<PlayingCardSkinType>.Values.Any((PlayingCardSkinType s) => s != 0 && !_ExcludePlayingCardDeck(s)) && !EnumUtil<TableSkinType>.Values.Any((TableSkinType s) => s != 0 && !_ExcludeTable(s)))
			{
				return EnumUtil<TokenSkinType>.Values.Any((TokenSkinType s) => s != 0 && !_ExcludeToken(s));
			}
			return true;
		}

		public void SignalPlayingCardDeckChange()
		{
			CosmeticOptions.OnPlayingCardDeckChange?.Invoke(playingCardDeck);
		}

		protected bool _ExcludePlayingCardDeck(PlayingCardSkinType skin)
		{
			if (skin != PlayingCardSkinType.Enemy)
			{
				uint? dlcId = skin.GetDlcId();
				if (dlcId.HasValue)
				{
					uint valueOrDefault = dlcId.GetValueOrDefault();
					return !Steam.HasDLC(valueOrDefault);
				}
				return false;
			}
			return true;
		}

		protected bool _ExcludeTable(TableSkinType skin)
		{
			uint? dlcId = skin.GetDlcId();
			if (dlcId.HasValue)
			{
				uint valueOrDefault = dlcId.GetValueOrDefault();
				return !Steam.HasDLC(valueOrDefault);
			}
			return false;
		}

		protected bool _ExcludeToken(TokenSkinType skin)
		{
			uint? dlcId = skin.GetDlcId();
			if (dlcId.HasValue)
			{
				uint valueOrDefault = dlcId.GetValueOrDefault();
				return !Steam.HasDLC(valueOrDefault);
			}
			return false;
		}

		protected void _OnPlayingCardDeckChange()
		{
			CosmeticOptions.OnPlayingCardDeckChange?.Invoke(_playingCardDeck);
		}

		protected void _OnTableChange()
		{
			CosmeticOptions.OnTableChange?.Invoke(_table);
		}

		protected void _OnTokenChange()
		{
			CosmeticOptions.OnTokenChange?.Invoke(_token);
		}
	}

	[ProtoContract]
	[UIField]
	public class DevOptions
	{
		public static Action<int> OnLevelChange;

		private static DevData _DevData;

		[ProtoMember(1)]
		[UIField(validateOnChange = true, tooltip = "Enable to modify values that will appear below.\n<i>Modify values below when you can see all three tables for consistent results.</i>")]
		private bool _enableDevOptions;

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Hide)]
		[UIHeader("Unlocks")]
		[UIHideIf("_hideCommon")]
		private DevData.UnlockData _unlocks;

		[ProtoMember(3)]
		[UIField(collapse = UICollapseType.Hide)]
		[UIHeader("Overrides")]
		[UIHideIf("_hideCommon")]
		private DevData.OverridesData _overrides;

		[ProtoMember(4)]
		[UIField]
		[UIHideIf("_hideRandomizeDailySeed")]
		private bool _randomizeDailySeed;

		private static DevData DevData => _DevData ?? (_DevData = ScriptableObject.CreateInstance<DevData>());

		public bool randomizeDailySeed
		{
			get
			{
				if (_randomizeDailySeed && _enableDevOptions)
				{
					return !Steam.Enabled;
				}
				return false;
			}
		}

		private bool _hideCommon => !_enableDevOptions;

		private bool _hideClearAchievements
		{
			get
			{
				if (!_hideCommon)
				{
					return !Steam.Enabled;
				}
				return true;
			}
		}

		private bool _hideRandomizeDailySeed
		{
			get
			{
				if (!_hideCommon)
				{
					return Steam.Enabled;
				}
				return true;
			}
		}

		public static implicit operator bool(DevOptions dev)
		{
			return dev?._enableDevOptions ?? false;
		}

		public static implicit operator DevData(DevOptions dev)
		{
			if (!dev)
			{
				return null;
			}
			return DevData.SetData(dev._unlocks, dev._overrides);
		}

		[UIField(tooltip = "Gives player 100 mana per click.")]
		[UIHideIf("_hideCommon")]
		[UIHeader("Functions")]
		private void _LevelUp()
		{
			ProfileManager.progress.experience.write.experience += 100;
		}

		[UIField(tooltip = "View the various hot keys which are available while dev mode is enabled.")]
		[UIHideIf("_hideCommon")]
		private void _ViewDevControls()
		{
			GameObject mainContent = UIUtil.CreateMessageBox("<i><b>ALL</b> the following require <b>[Shift + D] to be held</b> while executing:</i>\n<b>Kill All Enemies</b>: Press [K]\n<b>Draw Card/Ability</b>: Right click draw pile or discard pile\n<b>Discard Card/Ability</b>: Right click card in hand\n<b>Reset Hero Ability</b>: Right click hero ability\n<b>Gain an attack</b>: Right click player\n<b>Kill Enemy</b>: Right click enemy\n<b>Reset Item Uses</b>: Right click usable item\n<b>Remove Enemy Combat Card</b>: Right click enemy combat card", TextAlignmentOptions.Left, 32, 1600, 900, 24f);
			Transform activeUITransform = UIGeneratorType.GetActiveUITransform<ProfileOptions>();
			UIUtil.CreatePopup("Dev Hot Keys", mainContent, null, null, null, null, null, null, true, true, null, null, null, activeUITransform, null, null);
		}

		[UIField(tooltip = "Clear Steam achievements to allow testing unlocks again.")]
		[UIHideIf("_hideClearAchievements")]
		private async void _ClearAchievements()
		{
			await Steam.Stats.ClearAllAchievements();
		}
	}

	[ProtoContract]
	[UIField]
	public class RebirthOptions
	{
		[ProtoMember(1)]
		[UIField("Trait Ruleset", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Allows changing the way level up traits will be presented to characters which have achieved max level in Rebirth 1.", dynamicInitMethod = "_InitCommon", excludedValuesMethod = "_ExcludeTraits")]
		[DefaultValue(TraitRuleset.Unrestricted)]
		private TraitRuleset _traits = TraitRuleset.Unrestricted;

		[ProtoMember(2)]
		[UIField("Character Level Override", 0u, null, null, null, null, null, null, false, null, 5, false, null, min = 0, max = 30, tooltip = "Allows overriding the level of all characters which have achieved max level in Rebirth 2.", onValueChangedMethod = "_OnLevelChange", dynamicInitMethod = "_InitCommon")]
		[DefaultValue(30)]
		[UIHideIf("_hideRebirth2")]
		private int _level = 30;

		private bool _hideRebirth2 => !ProfileManager.progress.experience.read.HasRebirth2Options();

		public static event Action<int> OnLevelChange;

		public TraitRuleset? GetTraitRuleset(DataRef<CharacterData> characterDataRef)
		{
			if (!ProfileManager.progress.experience.read.CanOverrideTraitRuleSet(characterDataRef))
			{
				return null;
			}
			return _traits;
		}

		public int? GetLevelOverride(DataRef<CharacterData> characterDataRef)
		{
			if (!ProfileManager.progress.experience.read.CanOverrideLevel(characterDataRef))
			{
				return DevData.Overrides.levelOverride;
			}
			return _level;
		}

		private void _InitCommon(UIFieldAttribute uiField)
		{
			uiField.readOnly = GameState.Instance?.parameters.adventureBeganInitialize ?? false;
		}

		private bool _ExcludeTraits(TraitRuleset ruleset)
		{
			if (_hideRebirth2)
			{
				return ruleset == TraitRuleset.Unlocked;
			}
			return false;
		}

		private void _OnLevelChange()
		{
			RebirthOptions.OnLevelChange?.Invoke(_level);
		}
	}

	private const string CAT_VIDEO = "Video";

	private const string CAT_AUDIO = "Audio";

	private const string CAT_GAME = "Game";

	private const string CAT_CONTROLS = "Controls";

	private const string CAT_PROFILES = "Profiles";

	private const string CAT_COSMETIC = "Cosmetic";

	private const string CAT_DEV = "DEV";

	private const string CAT_REBIRTH = "Rebirth";

	[ProtoMember(4)]
	[UIField(order = 1u, collapse = UICollapseType.Hide)]
	[UICategory("Video")]
	private VideoOptions _video;

	[ProtoMember(2)]
	[UIField(order = 1000u, collapse = UICollapseType.Hide)]
	[UICategory("Audio")]
	private AudioOptions _audio;

	[ProtoMember(3)]
	[UIField(order = 2000u, collapse = UICollapseType.Hide)]
	[UICategory("Game")]
	private GameOptions _game;

	[ProtoMember(5)]
	[UIField(order = 3000u, collapse = UICollapseType.Hide)]
	[UICategory("Controls")]
	private ControlOptions _controls;

	[ProtoMember(6)]
	[UIField(order = 4000u, collapse = UICollapseType.Hide)]
	[UICategory("Cosmetic")]
	[UIHideIf("_hideCosmetics")]
	private CosmeticOptions _cosmetic;

	[ProtoMember(7)]
	[UIField(order = 5000u, collapse = UICollapseType.Hide)]
	[UICategory("DEV")]
	[UIHideIf("_hideDev")]
	private DevOptions _dev;

	[ProtoMember(8)]
	[UIField(order = 4500u, collapse = UICollapseType.Hide)]
	[UICategory("Rebirth")]
	[UIHideIf("_hideRebirth")]
	private RebirthOptions _rebirth;

	public VideoOptions video => _video ?? (_video = new VideoOptions());

	public AudioOptions audio => _audio ?? (_audio = new AudioOptions());

	public GameOptions game => _game ?? (_game = new GameOptions());

	public ControlOptions controls => _controls ?? (_controls = new ControlOptions());

	public CosmeticOptions cosmetic => _cosmetic ?? (_cosmetic = new CosmeticOptions());

	public DevData devData => _dev;

	public bool devInputEnabled
	{
		get
		{
			if (InputManager.I[KeyModifiers.Shift] && InputManager.I[KeyCode.D][KState.Down] && !CanvasInputFocus.HasActiveComponents)
			{
				return _dev;
			}
			return false;
		}
	}

	public bool devModeEnabled => _dev;

	public int? randomizedDailySeed
	{
		get
		{
			if (IOUtil.IsEditor && !Steam.Enabled)
			{
				DevOptions dev = _dev;
				if (dev != null && dev.randomizeDailySeed)
				{
					return UnityEngine.Random.Range(int.MinValue, int.MaxValue);
				}
			}
			return null;
		}
	}

	public RebirthOptions rebirth => _rebirth ?? (_rebirth = new RebirthOptions());

	private SaveProfileData profiles
	{
		get
		{
			return SaveProfileData.Data;
		}
		set
		{
		}
	}

	private bool _hideCosmetics
	{
		get
		{
			if (Steam.IsMainApp)
			{
				return !cosmetic.HasCosmetics();
			}
			return true;
		}
	}

	private bool _hideDev
	{
		get
		{
			if (!IOUtil.IsDemoBuild)
			{
				return !Steam.HasDLC(2275380u);
			}
			return true;
		}
	}

	private bool _hideRebirth => !ProfileManager.progress.experience.read.HasRebirthOptions();

	private bool _devSpecified => _dev;

	public static event Action<ProfileOptions> OnApplyChanges;

	static ProfileOptions()
	{
		EnumGenerator.CreateEnumeration(EnumUtil.FriendlyName(EnumGenerator.Types.DisplayResolution), typeof(ushort), Screen.resolutions.Select((Resolution resolution) => resolution.ToString()).Reverse().Distinct(ResolutionStringEqualityComparer.Default));
		EnumGenerator.CreateEnumeration(EnumUtil.FriendlyName(EnumGenerator.Types.QualitySettings), typeof(ushort), QualitySettings.names);
	}

	private void _ResetToDefault(Action onReset, string categoryOfData)
	{
		string text = UIUtil.GetUITable().AutoLocalize(categoryOfData);
		string confirm = MessageData.UIPopupButton.ResetOptions.GetButton().SetArguments(text).Localize();
		UIUtil.CreatePopup(MessageData.UIPopupTitle.ResetOptions.GetTitle().SetArguments(text).Localize(), UIUtil.CreateMessageBox(MessageData.UIPopupMessage.ResetOptions.GetMessage().SetArguments(text).Localize(), TextAlignmentOptions.Left, 32, 600, 300, 24f), null, parent: UIGeneratorType.GetActiveUITransform(this), buttons: new string[2] { confirm, "Cancel" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string buttonName)
		{
			if (buttonName == confirm)
			{
				onReset();
				UIGeneratorType.Validate(this);
			}
		});
	}

	public void ApplyChanges()
	{
		ProfileOptions.OnApplyChanges?.Invoke(this);
		video.ApplyChanges();
		audio.ApplyChanges();
		controls.ApplyChanges();
	}

	[UIField(order = 100u)]
	[UICategory("Video")]
	private void _ResetVideoOptionsToDefault()
	{
		_ResetToDefault(delegate
		{
			_video = null;
		}, "Video");
	}

	[UIField(order = 1100u)]
	[UICategory("Audio")]
	private void _ResetAudioOptionsToDefault()
	{
		_ResetToDefault(delegate
		{
			_audio = null;
		}, "Audio");
	}

	[UIField(order = 2100u)]
	[UICategory("Game")]
	private void _ResetGameOptionsToDefault()
	{
		_ResetToDefault(delegate
		{
			_game = null;
		}, "Game");
	}

	[UIField(order = 3100u)]
	[UICategory("Controls")]
	private void _ResetControlOptionsToDefault()
	{
		_ResetToDefault(delegate
		{
			_controls = null;
		}, "Controls");
	}

	[ProtoAfterDeserialization]
	private void _ProtoAfterDeserialization()
	{
		if (_dev != null && _hideDev)
		{
			_dev = null;
		}
	}
}
