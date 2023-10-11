using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

public static class UIUtil
{
	public const int DESCRIPTION_LENGTH_HUGE = 2048;

	public const int DESCRIPTION_LENGTH_LONG = 1024;

	public const int DESCRIPTION_LENGTH = 512;

	public const int DESCRIPTION_LENGTH_MEDIUM = 256;

	public const int DESCRIPTION_LENGTH_SHORT = 128;

	public const int DEFAULT_MARGIN_FI = 12;

	public const int INDENT_PIXELS = 24;

	public const float FLOAT_STEP_SIZE = 0.01f;

	public const char CATEGORY_SEPARATOR_CHAR = '|';

	public const string CATEGORY_SEPERATOR = "|";

	public const string SLIDER_SIMPLE = "UI/Slider Simple";

	public const string SLIDER_STANDARD = "UI/Slider Standard";

	public const string SLIDER_ADVANCED = "UI/Slider Advanced";

	public const string RANGE_SLIDER_HORIZ = "UI/Range Slider";

	public const string RANGE_SLIDER_VERT = "UI/Range Slider Vertical";

	public const string RANGE_SLIDER_ADVANCED = "UI/Reflection/Range Slider Advanced";

	public const string UILIST_STANDARD = "UI/UIList Standard";

	public const string COMBO_BOX_STANDARD = "UI/Combo Box Standard";

	public const string COMBO_BOX_MULTI_STANDARD = "UI/Combo Box Multi Standard";

	public const string COMBO_BOX_LOCALE = "UI/Combo Box Locale";

	public const string TOGGLE_STANDARD = "UI/Toggle Standard";

	public const string INPUT_FIELD_STANDARD = "UI/Input Field Standard";

	public const string INPUT_FIELD_MULTILINE = "UI/Input Field Multiline";

	public const string MESSAGE_BOX_STANDARD = "UI/Message Box Standard";

	public const string LOG_BOX_STANDARD = "UI/Log Box Standard";

	public const string COLLAPSE_STANDARD = "UI/Collapse Standard";

	public const string ILIST_STANDARD = "UI/IList Standard";

	public const string ILIST_ADD_SUFFIX = " Add";

	public const string ILIST_REMOVE_SUFFIX = " Remove";

	public const string UILIST_REMOVE_CONTAINER = "UI/UIList Remove Container";

	public const string HEADER_STANDARD = "UI/Header Standard";

	public const string BUTTON_STANDARD = "UI/Button Standard";

	public const string BUTTON_FITTED = "UI/Button Fitted";

	public const string BUTTON_CATEGORY_TAB = "UI/Button CategoryTab";

	public const string POPUP_STANDARD = "UI/Popup Standard";

	public const string PROCESSING_POPUP_STANDARD = "UI/Processing Popup Standard";

	public const string SCROLL_TEXT_STANDARD = "UI/ScrollText Standard";

	public const string AUDIO_EDITOR_STANDARD = "UI/Audio Editor Standard";

	public const string IMAGE_ALPHASHAPE_EDITOR_STANDARD = "UI/Image AlphaShape Editor Standard";

	public const string CATEGORY_TAB_CONTAINER = "UI/CategoryTab Container";

	public const string CATEGORY_DATA_CONTAINER = "UI/Category Data Container";

	public const string UI_TABS = "UI/UI Tabs";

	public const string UI_REFLECTED_OBJECT = "UI/Reflection/UIReflectedObject";

	public const string ABILITY_ADD_LOGIC_BUTTON = "UI/Ability/Add Logic Button";

	public const string ABILITY_DATA_ACTION = "UI/Ability/AbilityData Action";

	public const string ABILITY_DATA_CONDITION = "UI/Ability/AbilityData Condition";

	public const string ABILITY_DATA_REACTIVE = "UI/Ability/AbilityData Reactive";

	public const string VALIDATE_METHOD_NAME = "OnValidateUI";

	public const string DEFAULT_CATEGORY_NAME = "Misc";

	public const string CANCEL = "Cancel";

	public const string DISCARD_CHANGES = "Discard Changes";

	public static readonly Vector2 ReferenceResolution;

	public static readonly Dictionary<Type, Func<TreeNode<ReflectTreeData<UIFieldAttribute>>, Transform, Action, GameObject>> TypeViews;

	private static readonly Dictionary<Type, Action<TreeNode<ReflectTreeData<UIFieldAttribute>>>> TypeSpecialInitializations;

	private static readonly Dictionary<Type, Action<TreeNode<ReflectTreeData<UIFieldAttribute>>, TreeNode<ReflectTreeData<UIFieldAttribute>>>> MemberSpecialInitializations;

	private static readonly Dictionary<Type, Action<TreeNode<ReflectTreeData<UIFieldAttribute>>, Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, GameObject>>> TypePostProcesses;

	private static Dictionary<string, GameObject> _Blueprints;

	public static readonly ResourceBlueprint<Material> LinearToGammaMaterial;

	public static readonly ResourceBlueprint<TMP_FontAsset> MirzaFont;

	public static bool TriggerStaticConstructor;

	private const string COLOR_PICKER_STANDARD = "UI/Color Picker Standard";

	private const string COLOR_PICKER_HDR = "UI/Color Picker HDR";

	private const string AUDIO_REF_STANDARD = "UI/AudioRef Standard";

	private const string IMAGE_REF_STANDARD = "UI/ImageRef Standard";

	private const string DATA_REF_STANDARD = "UI/DataRef Standard";

	private const string KEY_VALUE_PAIR_STANDARD = "UI/KeyValuePair Standard";

	public const string KEY_VALUE_PAIR_MIN_KEY_WIDTH = "UI/KeyValuePair MinKeyWidth";

	private const string NULLABLE_STANDARD = "UI/Nullable Standard";

	public const string RANGE_SLIDER_STANDARD = "UI/Range Slider Standard";

	public const string IMAGEREF_POSITION_ANGLE_STANDARD = "UI/Reflection/ImageRef PositionAngle Standard";

	public const string IMAGEREF_POSITION_STANDARD = "UI/Reflection/ImageRef Position Standard";

	private const string IMAGEREF_UVCOORDS_STANDARD = "UI/Reflection/ImageRef UVCoords RawImage";

	public const string KEYBIND_VIEW = "UI/Reflection/KeybindView";

	public const string NODE_GRAPH_LINK_VIEW = "UI/Reflection/NodeGraphLinkView";

	private const float CONTENT_SEARCH_CLOSE_TIMEOUT = 3f;

	private static GameObject _ActiveProcessPopup;

	private static int _ActiveProcesses;

	public static TreeNode<ReflectTreeData<UIFieldAttribute>> InvokingMethodNode { get; private set; }

	public static Transform WebBrowserRequestTransform { get; private set; }

	public static TreeNode<ReflectTreeData<UIFieldAttribute>> ActiveCollectionNode { get; private set; }

	private static MemberInfo ActiveCollectionMemberInfo
	{
		get
		{
			if (ActiveCollectionNode == null)
			{
				return null;
			}
			return ActiveCollectionNode.value.memberInfo;
		}
	}

	public static StringTable UITable { get; private set; }

	static UIUtil()
	{
		ReferenceResolution = new Vector2(2560f, 1440f);
		_Blueprints = new Dictionary<string, GameObject>();
		LinearToGammaMaterial = "UI/Materials/UILinearToGamma";
		MirzaFont = "Fonts & Materials/Mirza SDF";
		TypeViews = new Dictionary<Type, Func<TreeNode<ReflectTreeData<UIFieldAttribute>>, Transform, Action, GameObject>>();
		RegisterTypeView<Color32>(CreateColorPicker);
		RegisterTypeView<Color32HDR>(CreateColorPickerHDR);
		RegisterTypeView<AudioRef>(CreateAudioRefView);
		RegisterTypeView<ImageRef>(CreateImageRefView);
		RegisterTypeView(typeof(DataRef<>), CreateDataRefView);
		RegisterTypeView(typeof(KeyValuePair<, >), CreateKeyValuePairView);
		RegisterTypeView(typeof(Nullable<>), CreateNullableView);
		RegisterTypeView<RangeF>(CreateRangeView);
		RegisterTypeView<RangeInt>(CreateRangeView);
		RegisterTypeView<RangeByte>(CreateRangeView);
		RegisterTypeView<ImageRefPositionAngle>(CreateImageRefPositionAngleView);
		RegisterTypeView<ImageRefPosition>(CreateImageRefPositionView);
		RegisterTypeView<ImageRefUVCoords>(CreateImageRefUVCoordsView);
		RegisterTypeView<ProfileOptions.ControlOptions.KeyBind>(CreateKeybindView);
		Type[] concreteClassesInEntireInheritanceHierarchy = typeof(NodeGraph).GetConcreteClassesInEntireInheritanceHierarchy();
		for (int i = 0; i < concreteClassesInEntireInheritanceHierarchy.Length; i++)
		{
			RegisterTypeView(concreteClassesInEntireInheritanceHierarchy[i], CreateNodeGraphLinkView);
		}
		RegisterTypeView<NodeDataFlag>(CreateNodeDataFlagView);
		RegisterTypeView<NodeDataTag>(CreateNodeDataTagView);
		RegisterTypeView<Type>(CreateTypeView);
		RegisterTypeView<Steam.Friends.Group>(_CreateSteamGroupView);
		RegisterTypeView<Steam.Ugc.Query.Result>(CreateQueryResultView);
		RegisterTypeView<LocalizedStringData.TableEntryId>(CreateLocalizedTableEntryView);
		RegisterTypeView<LocaleIdentifier>(CreateLocaleIdentifierView);
		TypeSpecialInitializations = new Dictionary<Type, Action<TreeNode<ReflectTreeData<UIFieldAttribute>>>>();
		MemberSpecialInitializations = new Dictionary<Type, Action<TreeNode<ReflectTreeData<UIFieldAttribute>>, TreeNode<ReflectTreeData<UIFieldAttribute>>>>();
		RegisterMemberSpecialInitialization(typeof(IListUIControl.IListAddWrapper<>), InitializeIListAddWrapper);
		TypePostProcesses = new Dictionary<Type, Action<TreeNode<ReflectTreeData<UIFieldAttribute>>, Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, GameObject>>>();
		RegisterTypePostProcess<CroppedImageRef>(PostProcessCroppedImageRef);
		RegisterTypePostProcess<LocalizedStringData>(PostProcessLocalizedStringData);
	}

	public static void RegisterTypeView<T>(Func<TreeNode<ReflectTreeData<UIFieldAttribute>>, Transform, Action, GameObject> generateView)
	{
		RegisterTypeView(typeof(T), generateView);
	}

	public static void RegisterTypeView(Type type, Func<TreeNode<ReflectTreeData<UIFieldAttribute>>, Transform, Action, GameObject> generateView)
	{
		TypeViews[type] = generateView;
	}

	public static void RegisterTypeSpecialInitialization(Type type, Action<TreeNode<ReflectTreeData<UIFieldAttribute>>> initialization)
	{
		TypeSpecialInitializations[type] = initialization;
	}

	public static void RegisterTypeSpecialInitialization<T>(Action<TreeNode<ReflectTreeData<UIFieldAttribute>>> initialization)
	{
		RegisterTypeSpecialInitialization(typeof(T), initialization);
	}

	public static void RegisterMemberSpecialInitialization(Type type, Action<TreeNode<ReflectTreeData<UIFieldAttribute>>, TreeNode<ReflectTreeData<UIFieldAttribute>>> initialization)
	{
		MemberSpecialInitializations[type] = initialization;
	}

	public static void RegisterMemberSpecialInitialization<T>(Action<TreeNode<ReflectTreeData<UIFieldAttribute>>, TreeNode<ReflectTreeData<UIFieldAttribute>>> initialization)
	{
		RegisterMemberSpecialInitialization(typeof(T), initialization);
	}

	public static void RegisterTypePostProcess(Type type, Action<TreeNode<ReflectTreeData<UIFieldAttribute>>, Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, GameObject>> postProcess)
	{
		TypePostProcesses[type] = postProcess;
	}

	public static void RegisterTypePostProcess<T>(Action<TreeNode<ReflectTreeData<UIFieldAttribute>>, Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, GameObject>> postProcess)
	{
		RegisterTypePostProcess(typeof(T), postProcess);
	}

	private static bool _HasTypeView(ReflectTreeData<UIFieldAttribute> rData)
	{
		return _GetTypeViewFunc(rData) != null;
	}

	private static Func<TreeNode<ReflectTreeData<UIFieldAttribute>>, Transform, Action, GameObject> _GetTypeViewFunc(ReflectTreeData<UIFieldAttribute> rData)
	{
		return _GetTypeViewFunc(rData.GetUnderlyingType(), rData.GetUnderlyingType().IsGenericType ? rData.GetUnderlyingType().GetGenericTypeDefinition() : null, rData.data != null && rData.data.searchInterfaceViews);
	}

	private static Func<TreeNode<ReflectTreeData<UIFieldAttribute>>, Transform, Action, GameObject> _GetTypeViewFunc(Type type, Type genericTypeDefinition, bool searchInterfaceViews)
	{
		Func<TreeNode<ReflectTreeData<UIFieldAttribute>>, Transform, Action, GameObject> ifExists = TypeViews.GetIfExists(type);
		if (ifExists == null && genericTypeDefinition != null)
		{
			ifExists = TypeViews.GetIfExists(genericTypeDefinition);
		}
		if (ifExists == null && searchInterfaceViews)
		{
			Type[] interfaces = type.GetInterfaces();
			foreach (Type key in interfaces)
			{
				if ((ifExists = TypeViews.GetIfExists(key)) != null)
				{
					break;
				}
			}
		}
		return ifExists;
	}

	private static GameObject CreateColorPicker(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		ReflectTreeData<UIFieldAttribute> r = node.value;
		UIFieldAttribute data = r.data;
		GameObject gameObject = GetGameObject(data.view ?? "UI/Color Picker Standard", parent).SetUILabel(data.label);
		gameObject.SetActive(value: true);
		ColorPicker componentInChildren = gameObject.GetComponentInChildren<ColorPicker>();
		Color32 color2 = (Color32)r.GetValue();
		componentInChildren.setStartColor = color2.EqualTo(default(Color32));
		componentInChildren.SetColor(color2);
		componentInChildren.OnColor32Change.AddListener(delegate(Color32 color)
		{
			r.SetValue(node, color);
		});
		_DefaultValueLogic(gameObject, (Color)((data.defaultValue != null) ? Colors.FromString32((string)data.defaultValue) : color2));
		return gameObject;
	}

	private static GameObject CreateColorPickerHDR(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		ReflectTreeData<UIFieldAttribute> r = node.value;
		UIFieldAttribute data = r.data;
		GameObject gameObject = GetGameObject(data.view ?? "UI/Color Picker HDR", parent).SetUILabel(data.label);
		gameObject.SetActive(value: true);
		ColorPicker componentInChildren = gameObject.GetComponentInChildren<ColorPicker>();
		Color32HDR color32HDR = (Color32HDR)r.GetValue();
		componentInChildren.setStartColor = color32HDR.color32.EqualTo(default(Color32));
		componentInChildren.SetColor(color32HDR.color32);
		componentInChildren.OnColor32Change.AddListener(delegate(Color32 color)
		{
			r.SetValue(node, new Color32HDR(color, ((Color32HDR)r.GetValue()).intensity));
		});
		componentInChildren.intensitySlider.value = color32HDR.intensity;
		componentInChildren.intensitySlider.onValueChanged.AddListener(delegate(float intensity)
		{
			r.SetValue(node, new Color32HDR(((Color32HDR)r.GetValue()).color32, intensity));
		});
		_DefaultValueLogic(gameObject, (Color)((data.defaultValue != null) ? Colors.FromString32((string)data.defaultValue) : ((Color32)color32HDR)));
		return gameObject;
	}

	private static GameObject CreateAudioRefView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		ReflectTreeData<UIFieldAttribute> rData = node.value;
		UIFieldAttribute data = rData.data;
		GameObject gameObject = GetGameObject(data.view ?? "UI/AudioRef Standard", parent);
		AudioRefControl audioRefControl = gameObject.GetComponentInChildren<AudioRefControl>(includeInactive: true);
		AudioCategoryType? audioCategoryType = (data.filter as AudioCategoryType?) ?? ((node.parent != null) ? (node.parent.value.data.filter as AudioCategoryType?) : null);
		AudioCategoryTypeFlags? audioCategoryTypeFlags = (data.stepSize as AudioCategoryTypeFlags?) ?? ((node.parent != null && node.parent.value.data != null) ? (node.parent.value.data.stepSize as AudioCategoryTypeFlags?) : null);
		if (audioCategoryType.HasValue)
		{
			audioRefControl.defaultCategory = (audioRefControl.category = audioCategoryType.Value);
		}
		if ((data.defaultValue as AudioCategoryType?).HasValue)
		{
			audioRefControl.defaultCategory = data.defaultValue as AudioCategoryType?;
		}
		AudioRef audioRef = rData.GetValue() as AudioRef;
		if ((bool)audioRef)
		{
			if (!audioCategoryType.HasValue || audioCategoryTypeFlags.HasValue)
			{
				audioRefControl.category = audioRef.category;
			}
			audioRefControl.SetAudioRef(audioRef, requestPlay: false);
		}
		audioRefControl.onAudioRefChanged.AddListener(delegate(object obj)
		{
			rData.SetValue(node, obj);
		});
		if (audioCategoryTypeFlags.HasValue)
		{
			Transform comboBox = CreateEnumComboBox(typeof(AudioCategoryType).Name, "Library", delegate(object value)
			{
				audioRefControl.SetCategoryAndPickFirstAudio((AudioCategoryType)value);
			}, audioRefControl.category.ToString(), null, null, (from c in EnumUtil<AudioCategoryType>.FlagsConverted(~(audioCategoryTypeFlags.Value | EnumUtil<AudioCategoryType>.ConvertToFlag<AudioCategoryTypeFlags>(audioCategoryType ?? audioRefControl.category)))
				select c.ToString()).ToHash(), 0).transform.SetParentAndReturn(audioRefControl.transform, worldPositionStays: false).SetAsFirstSiblingAndReturn();
			audioRefControl.onValidateImportCategory = delegate
			{
				comboBox.GetComponentInChildren<ComboBox>().SelectValue((rData.GetValue() as AudioRef).category, toggleList: false, clearSearch: true);
			};
		}
		return gameObject;
	}

	private static GameObject CreateImageRefView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		ReflectTreeData<UIFieldAttribute> rData = node.value;
		UIFieldAttribute data = rData.data;
		GameObject gameObject = GetGameObject(data.view ?? "UI/ImageRef Standard", parent);
		ImageRefControl imageRefControl = gameObject.GetComponentInChildren<ImageRefControl>(includeInactive: true);
		gameObject.SetUILabel(data.label);
		if (data.filter != null)
		{
			ImageRefControl imageRefControl2 = imageRefControl;
			ImageRefControl imageRefControl3 = imageRefControl;
			ImageCategoryType num = (ImageCategoryType)data.filter;
			ImageCategoryType value2 = num;
			imageRefControl3.category = num;
			imageRefControl2.defaultCategory = value2;
		}
		if ((data.defaultValue as ImageCategoryType?).HasValue)
		{
			imageRefControl.defaultCategory = data.defaultValue as ImageCategoryType?;
		}
		if (rData.GetValue() is ImageRef imageRef && imageRef.isTracked)
		{
			imageRefControl.category = imageRef.category;
			imageRefControl.SetImageRef(imageRef);
		}
		imageRefControl.onImageRefChanged.AddListener(delegate(object obj)
		{
			rData.SetValue(node, obj);
		});
		if (data.stepSize is ImageCategoryFlags)
		{
			Transform comboBox = CreateEnumComboBox(typeof(ImageCategoryType).Name, ">Library", delegate(object value)
			{
				imageRefControl.SetCategoryAndPickFirstImage((ImageCategoryType)value);
			}, imageRefControl.category.ToString(), null, null, (from c in EnumUtil<ImageCategoryType>.FlagsConverted((ImageCategoryFlags)(~(uint)((ImageCategoryFlags)data.stepSize | EnumUtil<ImageCategoryType>.ConvertToFlag<ImageCategoryFlags>((data.filter as ImageCategoryType?) ?? imageRefControl.category))))
				select c.ToString()).ToHash(), 0).transform.SetParentAndReturn(imageRefControl.GetComponentInChildren<CollapseFitter>().gameObject.GetComponentInChildrenOnly<VerticalLayoutGroup>().transform, worldPositionStays: false).SetAsFirstSiblingAndReturn();
			imageRefControl.onValidateImportCategory = delegate
			{
				comboBox.GetComponentInChildren<ComboBox>().SelectValue((rData.GetValue() as ImageRef).category, toggleList: false, clearSearch: true);
			};
		}
		RawImageLayoutElement componentInChildren = gameObject.GetComponentInChildren<RawImageLayoutElement>();
		if (data.min != null && (bool)componentInChildren)
		{
			componentInChildren.widthMin = (componentInChildren.heightMin = float.Parse(data.min.ToString()));
		}
		if (data.max != null && (bool)componentInChildren)
		{
			componentInChildren.maxWidth = (componentInChildren.maxHeight = float.Parse(data.max.ToString()));
		}
		return gameObject;
	}

	private static GameObject CreateDataRefView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		ReflectTreeData<UIFieldAttribute> rData = node.value;
		UIFieldAttribute data = rData.data;
		GameObject gameObject = GetGameObject(data.view ?? "UI/DataRef Standard", parent);
		DataRefControl componentInChildren = gameObject.GetComponentInChildren<DataRefControl>(includeInactive: true);
		gameObject.SetUILabel(data.label);
		componentInChildren.SetFriendlyNamePrefix(data.label);
		componentInChildren.dataType = rData.GetUnderlyingType().GetGenericArguments()[0];
		componentInChildren.ownerObject = rData.ownerObject;
		componentInChildren.memberInfo = rData.memberInfo;
		componentInChildren.filterMethodName = data.filterMethod;
		componentInChildren.excludedValuesMethodName = data.excludedValuesMethod;
		componentInChildren.excludedValues = node.value.excludedValues;
		componentInChildren.filter = (DataRefControlFilter)(((int?)(DataRefControlFilter?)data.filter) ?? ((node.parent == null || !(node.parent.value.GetUnderlyingType().GetGenericTypeDefinitionSafe() == typeof(Ref<>))) ? 1 : 0));
		componentInChildren.additionalWorkshopTags = data.defaultValue as IEnumerable<KeyValuePair<string, string>>;
		if (componentInChildren.filter.ShowData())
		{
			componentInChildren.GetComponentsInChildren<CollapseFitter>(includeInactive: true).EffectAll(delegate(CollapseFitter c)
			{
				c.ForceOpen();
			});
		}
		if (componentInChildren.filter.ShowData())
		{
			componentInChildren.SetAsActiveEdit();
		}
		if (rData.GetValue() is ContentRef contentRef && contentRef.isTracked)
		{
			componentInChildren.SetDataRef(contentRef, componentInChildren.filter.ShowData());
		}
		else if (componentInChildren.filter.ShowData())
		{
			componentInChildren.SetDataRef(ContentRef.SearchData(componentInChildren.dataType).MaxBy((ContentRef c) => c.lastUpdateTime), restoreLiveEditData: true);
		}
		componentInChildren.onDataRefChanged.AddListener(delegate(object obj)
		{
			rData.SetValue(node, obj);
		});
		if (componentInChildren.filter.ShowData() && DataRefControl.UseLiveEditMode)
		{
			componentInChildren.ChangeToLiveEditUI();
		}
		return gameObject;
	}

	private static GameObject CreateKeyValuePairView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		UIFieldAttribute data = node.value.data;
		MemberInfo memberInfo = ActiveCollectionMemberInfo ?? node.GetMemberInfo();
		GameObject gameObject = GetGameObject(data.view ?? "UI/KeyValuePair Standard", parent);
		UIGeneratorType[] componentsInChildren = gameObject.GetComponentsInChildren<UIGeneratorType>();
		UIGeneratorType uIGeneratorType = componentsInChildren[0];
		uIGeneratorType.overrideNode = node.children[0];
		ReflectTreeData<UIFieldAttribute> value = uIGeneratorType.overrideNode.value;
		value.data.label = ">" + value.data.label + node.value.data.label;
		UIFieldKeyAttribute uIFieldKeyAttribute = ((memberInfo != null) ? memberInfo.GetAttribute<UIFieldKeyAttribute>() : null);
		if (uIFieldKeyAttribute != null)
		{
			uIFieldKeyAttribute.DynamicInitialize(ActiveCollectionNode ?? node);
			uIFieldKeyAttribute.TransferKeyValuePairData(value.data);
			uIGeneratorType.overrideNode.CacheExcludeValuesMethod(ActiveCollectionNode);
			LayoutElement component = uIGeneratorType.GetComponent<LayoutElement>();
			component.flexibleWidth = uIFieldKeyAttribute.flexibleWidth;
			component.preferredWidth = 1f;
			if (ActiveCollectionMemberInfo != null)
			{
				value.data.readOnly = false;
			}
			else if (uIFieldKeyAttribute.inCollection == UIElementType.Hide)
			{
				node.children[0].value.shouldHide = true;
			}
			else if (uIFieldKeyAttribute.inCollection == UIElementType.ReadOnly)
			{
				value.data.readOnly = true;
			}
		}
		Type underlyingType = value.GetUnderlyingType();
		if (ActiveCollectionMemberInfo != null && value.data.defaultValue == null && underlyingType.IsReferenceType())
		{
			value.data.defaultValue = ((underlyingType != typeof(string)) ? ReflectionUtil.CreateInstanceSmart(underlyingType) : "New Key");
		}
		if (ActiveCollectionMemberInfo != null && value.data.defaultValue != null)
		{
			value.SetValue(node, (underlyingType != typeof(string)) ? value.data.defaultValue : (value.data.defaultValue as string).GetUniqueKey(value.excludedValues));
		}
		if (ActiveCollectionNode != null && underlyingType == typeof(string) && ActiveCollectionNode.value.originalCollection.GetType().IsCollectionThatShouldShowAddData())
		{
			value.excludedValues = (object obj) => false;
		}
		UIGeneratorType uIGeneratorType2 = componentsInChildren[1];
		uIGeneratorType2.overrideNode = node.children[1];
		ReflectTreeData<UIFieldAttribute> value2 = uIGeneratorType2.overrideNode.value;
		value2.data.label = ">" + value2.data.label + node.value.data.label;
		UIFieldValueAttribute uIFieldValueAttribute = ((memberInfo != null) ? memberInfo.GetAttribute<UIFieldValueAttribute>() : null);
		if (uIFieldValueAttribute != null)
		{
			uIFieldValueAttribute.DynamicInitialize(ActiveCollectionNode ?? node);
			uIFieldValueAttribute.TransferKeyValuePairData(value2.data);
			uIGeneratorType2.overrideNode.CacheExcludeValuesMethod(ActiveCollectionNode);
			LayoutElement component2 = uIGeneratorType2.GetComponent<LayoutElement>();
			component2.flexibleWidth = uIFieldValueAttribute.flexibleWidth;
			component2.preferredWidth = 1f;
		}
		Type underlyingType2 = value2.GetUnderlyingType();
		if (ActiveCollectionMemberInfo != null && value2.data.defaultValue == null && underlyingType2.IsReferenceType())
		{
			value2.data.defaultValue = ReflectionUtil.CreateInstanceSmart(underlyingType2);
		}
		if (ActiveCollectionMemberInfo != null && value2.data.defaultValue != null)
		{
			value2.SetValue(node, value2.data.defaultValue);
		}
		object value3 = value.GetValue();
		object value4 = value2.GetValue();
		if (ActiveCollectionNode != null)
		{
			node.value.SetValue(node.parent, ReflectionUtil.CreateInstanceSmart(typeof(KeyValuePair<, >).MakeGenericType(underlyingType, underlyingType2), new object[2] { value3, value4 }));
		}
		if (value3 != null)
		{
			if (value3.GetType().ShouldKeepLabelInKeyValuePair())
			{
				value.data.label = "<b>" + node.value.data.label.Split(':')[0] + "</b>: " + value3;
			}
			uIGeneratorType.GenerateFromObject(value3);
		}
		if (value4 != null)
		{
			if (value4.GetType().ShouldKeepLabelInKeyValuePair())
			{
				value2.data.label = node.value.data.label.Split(':')[0] + ": " + value4;
			}
			uIGeneratorType2.GenerateFromObject(value4);
			uIGeneratorType2.OnGenerate.AddListener(delegate
			{
				onValidate?.Invoke();
			});
		}
		if (node.children[0].value.shouldHide)
		{
			uIGeneratorType.gameObject.SetActive(value: false);
		}
		if (node.children[1].value.shouldHide)
		{
			uIGeneratorType2.gameObject.SetActive(value: false);
		}
		uIGeneratorType.OnValueChanged.AddListener(delegate
		{
			node.value.OnValueChanged();
		});
		uIGeneratorType2.OnValueChanged.AddListener(delegate
		{
			node.value.OnValueChanged();
		});
		return gameObject;
	}

	private static GameObject CreateNullableView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		UIFieldAttribute data = node.value.data;
		GameObject gameObject = GetGameObject(data.view ?? "UI/Nullable Standard", parent).SetUILabel(data.label);
		UIGeneratorType dataUIGenerator = gameObject.GetComponentInChildren<UIGeneratorType>(includeInactive: true);
		Toggle componentInChildren = gameObject.GetComponentInChildren<Toggle>();
		object value = node.value.GetValue();
		Type nullableUnderlyingType = Nullable.GetUnderlyingType(node.value.GetUnderlyingType());
		UIFieldAttribute uIFieldAttribute = new UIFieldAttribute(">" + data.label);
		(((node.GetMemberInfo() != null) ? node.GetMemberInfo().GetAttribute<UIFieldValueAttribute>() : null) ?? data).TransferKeyValuePairData(uIFieldAttribute);
		ReflectTreeData<UIFieldAttribute> reflectTreeData = new ReflectTreeData<UIFieldAttribute>(node.value.order + 1, value, node.value.self, node.value.memberInfo, uIFieldAttribute);
		object backingData = value ?? ReflectionUtil.CreateInstanceSmart(nullableUnderlyingType);
		reflectTreeData.underlyingType = nullableUnderlyingType;
		reflectTreeData.skipSetValueBubbling = true;
		reflectTreeData.getValueLogic = () => node.value.GetValue() ?? backingData;
		reflectTreeData.setValueLogic = delegate(object obj)
		{
			if (obj.GetType() != nullableUnderlyingType)
			{
				obj = Convert.ChangeType(obj, nullableUnderlyingType);
			}
			backingData = obj;
			node.value.SetValue(node, obj);
		};
		TreeNode<ReflectTreeData<UIFieldAttribute>> overrideNode = node.AddChild(reflectTreeData);
		dataUIGenerator.overrideNode = overrideNode;
		if (value != null)
		{
			componentInChildren.isOn = true;
			dataUIGenerator.GenerateFromObject(value);
			dataUIGenerator.gameObject.SetCollapserIndentRatio(0f);
		}
		else
		{
			componentInChildren.isOn = false;
		}
		UIFieldKeyAttribute uIFieldKeyAttribute = ((node.GetMemberInfo() != null) ? node.GetMemberInfo().GetAttribute<UIFieldKeyAttribute>() : null);
		bool validateOnToggleChange = uIFieldKeyAttribute?.validateOnChange ?? false;
		Action onValueChanged = null;
		if (uIFieldKeyAttribute != null && !uIFieldKeyAttribute.onValueChangedMethod.IsNullOrEmpty())
		{
			MethodInfo methodInfo;
			TreeNode<ReflectTreeData<UIFieldAttribute>> methodHolder = node.GetNodeWithMethod(uIFieldKeyAttribute.onValueChangedMethod, out methodInfo);
			if (methodHolder != null)
			{
				onValueChanged = delegate
				{
					methodInfo.Invoke(methodHolder.value.self, new object[0]);
				};
			}
		}
		componentInChildren.onValueChanged.AddListener(delegate(bool isOn)
		{
			if (isOn)
			{
				node.value.SetValue(node, backingData);
				dataUIGenerator.GenerateFromObject(backingData);
				dataUIGenerator.gameObject.SetCollapserIndentRatio(0f);
			}
			else
			{
				node.value.SetValue(node, null);
				dataUIGenerator.GenerateFromObject(null);
			}
			if (onValueChanged != null)
			{
				onValueChanged();
			}
			if (validateOnToggleChange)
			{
				onValidate();
			}
		});
		return gameObject.SetActiveAndReturn(active: true);
	}

	private static GameObject CreateRangeView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		ReflectTreeData<UIFieldAttribute> rData = node.value;
		UIFieldAttribute data = rData.data;
		GameObject gameObject = GetGameObject(data.view ?? "UI/Reflection/Range Slider Advanced", parent).SetUILabel(data.label);
		gameObject.SetActive(value: true);
		RangeSlider componentInChildren = gameObject.GetComponentInChildren<RangeSlider>(includeInactive: true);
		Type underlyingType = rData.GetUnderlyingType();
		object value = rData.GetValue();
		if (underlyingType == typeof(RangeF))
		{
			componentInChildren.stepSize = ((data.stepSize != null) ? float.Parse(data.stepSize.ToString()) : _GetStepSize(componentInChildren.rangeF.boundrayRange));
			componentInChildren.rangeF = ((RangeF)value).CopyNonSerializedFieldsFrom(data.defaultValue);
			componentInChildren.onRangeFChanged.AddListener(delegate(RangeF range)
			{
				rData.SetValue(node, range);
			});
		}
		else if (underlyingType == typeof(RangeInt))
		{
			componentInChildren.rangeInt = ((RangeInt)value).CopyNonSerializedFieldsFrom(data.defaultValue);
			componentInChildren.onRangeIntChanged.AddListener(delegate(RangeInt range)
			{
				rData.SetValue(node, range);
			});
		}
		else if (underlyingType == typeof(RangeByte))
		{
			componentInChildren.rangeByte = ((RangeByte)value).CopyNonSerializedFieldsFrom(data.defaultValue);
			componentInChildren.onRangeByteChanged.AddListener(delegate(RangeByte range)
			{
				rData.SetValue(node, range);
			});
		}
		if (data.min != null)
		{
			componentInChildren.minRange = float.Parse(data.min.ToString());
		}
		if (data.max != null)
		{
			componentInChildren.maxRange = float.Parse(data.max.ToString());
		}
		return gameObject;
	}

	private static GameObject CreateImageRefPositionAngleView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		ReflectTreeData<UIFieldAttribute> rData = node.value;
		UIFieldAttribute data = rData.data;
		GameObject gameObject = GetGameObject(data.view ?? "UI/Reflection/ImageRef PositionAngle Standard", parent);
		gameObject.SetUILabel(data.label);
		UIPositionAngle componentInChildren = gameObject.GetComponentInChildren<UIPositionAngle>();
		ImageRefPositionAngle value = (ImageRefPositionAngle)rData.GetValue();
		componentInChildren.positionAngle = value.positionAngle;
		componentInChildren.SetImageRef(value.imageRef);
		componentInChildren.OnPositionAngleChange.AddListener(delegate(PositionAngle positionAngle)
		{
			PositionAngle positionAngle2 = value.positionAngle;
			value.positionAngle = positionAngle;
			if (rData.OnValueChanged != null && positionAngle2 != positionAngle)
			{
				rData.OnValueChanged();
			}
		});
		return gameObject;
	}

	private static GameObject CreateImageRefPositionView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		ReflectTreeData<UIFieldAttribute> rData = node.value;
		UIFieldAttribute data = rData.data;
		GameObject gameObject = GetGameObject(data.view ?? "UI/Reflection/ImageRef Position Standard", parent);
		gameObject.SetUILabel(data.label);
		UIPositionAngle componentInChildren = gameObject.GetComponentInChildren<UIPositionAngle>();
		Toggle componentInChildren2 = gameObject.GetComponentInChildren<Toggle>();
		ImageRefPosition value = (ImageRefPosition)rData.GetValue();
		componentInChildren2.isOn = value.enabled;
		componentInChildren.positionAngle = new PositionAngle(value, 0f, flipOrthogonalAxis: false);
		componentInChildren.SetImageRef(value.imageRef);
		componentInChildren2.onValueChanged.AddListener(delegate(bool enabled)
		{
			bool enabled2 = value.enabled;
			value.enabled = enabled;
			if (rData.OnValueChanged != null && enabled2 != enabled)
			{
				rData.OnValueChanged();
			}
		});
		componentInChildren.OnValueChange.AddListener(delegate(Vector2 position)
		{
			Vector2 position2 = value.position;
			value.position = position;
			if (rData.OnValueChanged != null && position2 != position)
			{
				rData.OnValueChanged();
			}
		});
		return gameObject;
	}

	private static GameObject CreateImageRefUVCoordsView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		GameObject gameObject = GetGameObject(node.value.data.view ?? "UI/Reflection/ImageRef UVCoords RawImage", parent);
		RawImageCrop rawImageCrop = gameObject.GetComponentInChildren<RawImageCrop>();
		ImageRefUVCoords imageRefUVCoords = (ImageRefUVCoords)node.value.GetValue();
		if (imageRefUVCoords.imageRef.IsValid())
		{
			imageRefUVCoords.imageRef.GetTexture2D(delegate(Texture2D texture)
			{
				object filter = node.value.data.filter;
				string text = node.value.data.filter as string;
				rawImageCrop.preferredSize = ((text != null) ? StringUtil.ParseInvariantInt2(text) : (((Int2?)filter) ?? ((Int2?)imageRefUVCoords.imageRef.category.PreferredSize()) ?? (Int2.one * imageRefUVCoords.imageRef.category.MaxSaveResolution())));
				if (node.value.data.max != null)
				{
					rawImageCrop.layoutElement.maxSize = Vector2.one * float.Parse(node.value.data.max.ToString());
				}
				rawImageCrop.rawImage.texture = texture;
				rawImageCrop.onUVChange.AddListener(delegate(UVCoords uvCoords)
				{
					imageRefUVCoords.uvCoords = uvCoords;
					if (node.value.OnValueChanged != null)
					{
						node.value.OnValueChanged();
					}
				});
				rawImageCrop.uvCoords = imageRefUVCoords.uvCoords ?? rawImageCrop.GetDefaultUVCoords();
			});
		}
		return gameObject;
	}

	private static void PostProcessCroppedImageRef(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, GameObject> uiGameObjects)
	{
		TreeNode<ReflectTreeData<UIFieldAttribute>> child = node.GetChild((TreeNode<ReflectTreeData<UIFieldAttribute>> n) => n.value.GetUnderlyingType() == typeof(ImageRef));
		if (child != null && uiGameObjects.ContainsKey(child))
		{
			GameObject gameObject = uiGameObjects[child];
			ReflectTreeData<UIFieldAttribute> value = child.value;
			value.OnValueChanged = (Action)Delegate.Combine(value.OnValueChanged, node.value.OnValueChanged);
			TreeNode<ReflectTreeData<UIFieldAttribute>> child2 = node.GetChild((TreeNode<ReflectTreeData<UIFieldAttribute>> n) => n.value.GetUnderlyingType() == typeof(ImageRefUVCoords));
			if (child2 != null && uiGameObjects.ContainsKey(child2))
			{
				GameObject gameObject2 = uiGameObjects[child2];
				RawImage componentInChildren = gameObject.GetComponentInChildren<RawImage>();
				gameObject2.transform.SetParent(componentInChildren.transform.parent, worldPositionStays: false);
				gameObject2.transform.SetSiblingIndex(componentInChildren.transform.GetSiblingIndex());
				componentInChildren.gameObject.SetActive(value: false);
			}
			TreeNode<ReflectTreeData<UIFieldAttribute>> child3 = node.GetChild((TreeNode<ReflectTreeData<UIFieldAttribute>> n) => n.value.GetUnderlyingType() == typeof(ImageCategoryType));
			if (child3 != null && uiGameObjects.ContainsKey(child3))
			{
				GameObject gameObject3 = uiGameObjects[child3];
				VerticalLayoutGroup componentInChildrenOnly = gameObject.GetComponentInChildren<CollapseFitter>().gameObject.GetComponentInChildrenOnly<VerticalLayoutGroup>();
				gameObject3.transform.SetParent(componentInChildrenOnly.transform, worldPositionStays: false);
				gameObject3.transform.SetAsFirstSibling();
			}
		}
	}

	private static GameObject CreateKeybindView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		ReflectTreeData<UIFieldAttribute> value = node.value;
		UIFieldAttribute data = value.data;
		GameObject gameObject = GetGameObject(data.view ?? "UI/Reflection/KeybindView", parent);
		gameObject.SetUILabel(data.label);
		KeybindView component = gameObject.GetComponent<KeybindView>();
		component.reflectedData = node.parent.value;
		component.keybind = (ProfileOptions.ControlOptions.KeyBind)value.GetValue();
		return gameObject;
	}

	private static GameObject CreateNodeGraphLinkView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		if (node.value.GetValue() == null)
		{
			object obj = Activator.CreateInstance(node.value.GetUnderlyingType(), nonPublic: true);
			((NodeGraph)obj).name = node.value.data.label;
			node.value.SetValue(node, obj);
		}
		GameObject gameObject = GetGameObject(node.value.data.view ?? "UI/Reflection/NodeGraphLinkView", parent).GetComponent<NodeGraphLinkView>().SetData((node.value.data.filter as string) ?? node.value.data.label, node.value.GetValue() as NodeGraph, node.value).gameObject;
		gameObject.SetActive(value: true);
		return gameObject;
	}

	private static GameObject CreateNodeDataFlagView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		NodeDataFlag nodeDataFlag = (NodeDataFlag)node.value.GetValue();
		Dictionary<string, string> uIListData = nodeDataFlag.nodeGraphFlags.GetUIListData();
		return CreateGenericComboBox(uIListData, node.value.data.label, delegate(string s)
		{
			nodeDataFlag.flag = s;
			node.value.OnValueChanged();
		}, uIListData.GetValueSafe(nodeDataFlag.flag), CategorySortType.Alphabetical, nodeDataFlag.nodeGraphFlags.GetUIListCategoryData(), null, 0, 24, null, parent);
	}

	private static GameObject CreateNodeDataTagView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		NodeDataTag nodeDataTag = (NodeDataTag)node.value.GetValue();
		if (nodeDataTag.graphTags == null)
		{
			return null;
		}
		Dictionary<string, string> uIListData = nodeDataTag.graphTags.GetUIListData(node.value.data.filter as HashSet<NodeDataTag>, node.value.GetValue() as NodeDataTag);
		return CreateGenericComboBox(uIListData, node.value.data.label, delegate(string s)
		{
			nodeDataTag.tag = s;
			node.value.OnValueChanged();
		}, uIListData.GetValueSafe(nodeDataTag.tag), CategorySortType.Alphabetical, null, null, 0, 24, null, parent);
	}

	private static GameObject CreateTypeView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		ReflectTreeData<UIFieldAttribute> rData = node.value;
		UIFieldAttribute data = rData.data;
		Type type = rData.GetValue() as Type;
		bool flag = node.parent != null && node.parent.value.GetUnderlyingType() == typeof(SpecificType);
		Type type2 = data.filter as Type;
		if (type2 == null && flag)
		{
			type2 = node.parent.value.data.filter as Type;
		}
		Func<Type, bool> func = null;
		if (rData.excludedValues != null)
		{
			func = (Type t) => rData.excludedValues(t);
		}
		if (func == null && flag && node.parent.value.excludedValues != null)
		{
			func = (Type t) => node.parent.value.excludedValues(t);
		}
		if (flag)
		{
			ReflectTreeData<UIFieldAttribute> reflectTreeData = rData;
			reflectTreeData.OnValueChanged = (Action)Delegate.Combine(reflectTreeData.OnValueChanged, node.parent.value.OnValueChanged);
		}
		return CreateTypeComboBox((type2 ?? typeof(object)).FullName, data.label, delegate(Type t)
		{
			rData.SetValue(node, t);
		}, derivedTypesOnly: false, (type != null) ? type.FullName : "", null, 0, func, parent);
	}

	private static GameObject _CreateSteamGroupView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		Steam.Friends.Group group = (Steam.Friends.Group)node.value.GetValue();
		return CreateSteamGroupView(group, delegate(Steam.Friends.Group g)
		{
			node.value.SetValue(node, g);
		}, parent, node.value.data.label, (Steam.Friends.Group g) => node.value.excludedValues.Any(g) && (ActiveCollectionNode != null || !g.Equals(group)));
	}

	public static GameObject CreateSteamGroupView(Steam.Friends.Group group, Action<Steam.Friends.Group> onValueChange, Transform parent = null, string label = null, Func<Steam.Friends.Group, bool> excludedValues = null, bool fitIntoParent = false)
	{
		Dictionary<string, Steam.Friends.Group> dictionary = Steam.Friends.CachedGroups.Where((Steam.Friends.Group g) => excludedValues == null || !excludedValues(g)).DistinctBy((Steam.Friends.Group g) => g.name).ToDictionary((Steam.Friends.Group g) => g.name, (Steam.Friends.Group g) => g);
		return CreateGenericComboBox(dictionary, label, onValueChange, group ? group : dictionary.Values.FirstOrDefault(), CategorySortType.Alphabetical, null, null, 0, 0, null, parent, fitIntoParent);
	}

	private static GameObject CreateQueryResultView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		return SteamQueryResultView.Create((Steam.Ugc.Query.Result)node.value.GetValue(), node.value.data.label, parent, (node.value.data.filter as Steam.Ugc.ItemType?).GetValueOrDefault()).gameObject;
	}

	public static void CreateTableEntryMaps(out Dictionary<string, LocalizedStringData.TableEntryId> tableEntryMap, out Dictionary<string, string> categoryMap, HashSet<TableReference> includedTables = null)
	{
		tableEntryMap = new Dictionary<string, LocalizedStringData.TableEntryId>();
		categoryMap = new Dictionary<string, string>();
		if (includedTables == null)
		{
			includedTables = new HashSet<TableReference>(((IEnumerable<StringTable>)LocalizationSettings.StringDatabase.GetAllTables().WaitForCompletion()).Select((Func<StringTable, TableReference>)((StringTable t) => t.TableCollectionName)));
		}
		foreach (StringTable item in includedTables.Select((TableReference id) => LocalizationSettings.StringDatabase.GetTable(id)))
		{
			string tableCollectionName = item.TableCollectionName;
			foreach (StringTableEntry value in item.Values)
			{
				string key = tableCollectionName + "/" + value.Key;
				tableEntryMap[key] = value;
				categoryMap[key] = tableCollectionName;
			}
		}
	}

	private static GameObject CreateLocalizedTableEntryView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		HashSet<TableReference> hashSet = new HashSet<TableReference> { "Common", "Constant" };
		if (DataRefControl.GetMainControlForDataType<DataRef<MessageData>>() != null)
		{
			hashSet.AddMany(new TableReference[2] { "Message", "Glossary" });
		}
		HashSet<LocalizedStringData.TableEntryId> hashSet2 = new HashSet<LocalizedStringData.TableEntryId>();
		LocalizedStringData.TableEntryId tableEntryId = (LocalizedStringData.TableEntryId)node.value.GetValue();
		StringTableEntry tableEntry = tableEntryId.tableEntry;
		if (tableEntry != null && hashSet.Add(tableEntry.Table.TableCollectionName))
		{
			foreach (StringTableEntry value in ((StringTable)tableEntry.Table).Values)
			{
				if (value.KeyId != (long)tableEntryId)
				{
					hashSet2.Add(new LocalizedStringData.TableEntryId(value));
				}
			}
		}
		CreateTableEntryMaps(out var tableEntryMap, out var categoryMap, hashSet);
		return CreateGenericComboBox(tableEntryMap, node.value.data.label, delegate(LocalizedStringData.TableEntryId entry)
		{
			node.value.SetValue(node.parent, entry);
		}, tableEntryId, CategorySortType.Alphabetical, categoryMap, hashSet2, 0, 24, null, parent, fitIntoParent: false, useFriendlyName: false);
	}

	private static GameObject CreateLocaleIdentifierView(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Transform parent, Action onValidate)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("UI/Combo Box Locale"), parent).SetUILabel(node.value.data.label ?? ">");
		gameObject.SetActive(value: true);
		ComboBox cb = gameObject.GetComponentInChildren<ComboBox>();
		cb.OnGenerate.AddListener(delegate
		{
			UIListGeneratorLocale.Generate(cb, (LocaleIdentifier)node.value.GetValue());
		});
		cb.OnSelectedValueChanged.AddListener(delegate(object locale)
		{
			node.value.SetValue(node.parent, locale);
		});
		return gameObject;
	}

	private static void PostProcessLocalizedStringData(TreeNode<ReflectTreeData<UIFieldAttribute>> node, Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, GameObject> uiGameObjects)
	{
		TreeNode<ReflectTreeData<UIFieldAttribute>> child = node.GetChild((TreeNode<ReflectTreeData<UIFieldAttribute>> n) => n.value.isMethod && n.value.data.label == "Add To Localization Table");
		if (child == null)
		{
			return;
		}
		GameObject addKeyButtonGameObject = uiGameObjects.GetValueOrDefault(child);
		if ((object)addKeyButtonGameObject == null)
		{
			return;
		}
		addKeyButtonGameObject.GetComponentInChildren<Button>()?.onClick.AddListener(delegate
		{
			using PoolKeepItemListHandle<TreeNode<ReflectTreeData<UIFieldAttribute>>> poolKeepItemListHandle = node.GetPath((TreeNode<ReflectTreeData<UIFieldAttribute>> stopAt) => stopAt.value.GetValue() is IDataContent obj && (bool)UIGeneratorType.GetActiveUI(obj));
			IDataContent data = poolKeepItemListHandle.value.First().value.self as IDataContent;
			ContentRef @ref = DataRefControl.GetRef(data);
			StringTable table = LocalizationSettings.StringDatabase.GetTable(@ref.specificTypeFriendly);
			string text = $"{@ref.key.fileId}/" + poolKeepItemListHandle.GetLocalizationKeyFromNodePath();
			LocalizedStringData localizedStringData = node.value.GetValue() as LocalizedStringData;
			CreatePopup("Confirm Localized Table Entry Creation", CreateMessageBox("<b>Data currently being edited will be saved automatically!</b>\nFollowing entry will be added to <b>" + table.TableCollectionName + "</b> table:\n<nobr><b>Key:</b> DATA_REF_NAME/" + text + "</nobr>\n<b>Value:</b> " + localizedStringData.rawText, TextAlignmentOptions.Center, 32, 1280, 300, 24f), null, buttons: new string[2] { "Confirm", "Cancel" }, parent: addKeyButtonGameObject.transform, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
			{
				if (!(s != "Confirm"))
				{
					localizedStringData.id.tableEntry.Key = data.GetTitle() + "/" + localizedStringData.id.tableEntry.Key;
					DataRefControl.SaveActiveEditControl();
					BeginProcessJob(uiGameObjects.Values.First().transform, null, Department.UI).Then().DoProcess(Job.WaitForDepartmentEmpty(Department.Content)).Then()
						.Do(delegate
						{
							UIGeneratorType.Validate(data);
						})
						.Then()
						.Do(EndProcess);
				}
			});
		});
	}

	private static void InitializeIListAddWrapper(TreeNode<ReflectTreeData<UIFieldAttribute>> parentNode, TreeNode<ReflectTreeData<UIFieldAttribute>> node)
	{
		if (ActiveCollectionMemberInfo == null)
		{
			return;
		}
		object collection = ActiveCollectionNode.value.originalCollection;
		Type type = collection.GetType();
		if (type.IsGenericHashSet())
		{
			if (type != typeof(HashSet<string>))
			{
				MethodInfo containsMethod2 = type.GetInheritedMethod("Contains");
				if (!node.value.GetUnderlyingType().IsKeyValuePair())
				{
					ReflectTreeData<UIFieldAttribute> value = node.value;
					value.excludedValues = (Func<object, bool>)Delegate.Combine(value.excludedValues, (Func<object, bool>)((object obj) => (bool)containsMethod2.Invoke(collection, new object[1] { obj })));
				}
				else
				{
					ReflectTreeData<UIFieldAttribute> value2 = node.children[0].value;
					value2.excludedValues = (Func<object, bool>)Delegate.Combine(value2.excludedValues, (Func<object, bool>)((object obj) => (bool)containsMethod2.Invoke(collection, new object[1] { ReflectionUtil.CreateInstanceSmart(node.value.GetUnderlyingType(), new object[2] { obj, null }) })));
				}
			}
		}
		else if (type.IsGenericIDictionary() && node.value.GetUnderlyingType().IsKeyValuePair())
		{
			MethodInfo containsMethod = type.GetInheritedMethod("ContainsKey");
			ReflectTreeData<UIFieldAttribute> value3 = node.children[0].value;
			value3.excludedValues = (Func<object, bool>)Delegate.Combine(value3.excludedValues, (Func<object, bool>)((object obj) => (bool)containsMethod.Invoke(collection, new object[1] { obj })));
			UIFieldValueAttribute attribute = ActiveCollectionMemberInfo.GetAttribute<UIFieldValueAttribute>();
			node.children[1].value.shouldHide = attribute?.hideInCollectionAdd ?? true;
		}
		else if (node.value.GetUnderlyingType().IsKeyValuePair())
		{
			UIFieldValueAttribute attribute2 = ActiveCollectionMemberInfo.GetAttribute<UIFieldValueAttribute>();
			node.children[1].value.shouldHide = attribute2?.hideInCollectionAdd ?? true;
		}
		string label = ((node.value.data != null) ? node.value.data.label : "");
		node.value.data = ActiveCollectionMemberInfo.GetAttribute<UIFieldCollectionItemAttribute>() ?? node.value.data ?? new UIFieldAttribute();
		if (node.value.data is UIFieldCollectionItemAttribute)
		{
			node.value.data.DynamicInitialize(ActiveCollectionNode);
		}
		node.value.data.readOnly = false;
		node.value.data.validateOnChange = false;
		node.value.data.label = label;
		if (node.value.data.defaultValue != null)
		{
			node.value.SetValue(parentNode, node.value.data.defaultValue);
		}
		if (node.value.data.filterMethod.IsNullOrEmpty())
		{
			node.value.data.filterMethod = ActiveCollectionNode.value.data.filterMethod;
		}
	}

	private static void _DefaultValueLogic(GameObject go, object defaultValue = null)
	{
		if (defaultValue != null)
		{
			DefaultValueHook componentInChildren = go.GetComponentInChildren<DefaultValueHook>();
			if (componentInChildren != null)
			{
				componentInChildren.defaultValue = defaultValue;
			}
		}
	}

	public static GameObject CreateSlider(string label, UnityAction<double> onValueChanged = null, double currentValue = 0.0, float minValue = 0f, float maxValue = 1f, bool wholeNumbersOnly = false, float stepSize = 0f, string resourcePath = null, object defaultValue = null, Transform parent = null, int significantDigits = 7)
	{
		resourcePath = resourcePath ?? "UI/Slider Standard";
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(resourcePath), parent).SetUILabel(label);
		gameObject.SetActive(value: true);
		Slider componentInChildren = gameObject.GetComponentInChildren<Slider>();
		if ((bool)componentInChildren)
		{
			componentInChildren.wholeNumbers = wholeNumbersOnly;
			componentInChildren.minValue = minValue;
			componentInChildren.maxValue = maxValue;
			componentInChildren.value = (float)currentValue;
			if (onValueChanged != null)
			{
				componentInChildren.onValueChanged.AddListener(delegate(float v)
				{
					onValueChanged(v);
				});
				componentInChildren.onValueChanged.Invoke(componentInChildren.value);
			}
			if (stepSize > 0f)
			{
				componentInChildren.gameObject.AddComponent<SliderStepper>().stepSize = stepSize;
			}
		}
		else
		{
			TMP_InputField inputField = gameObject.GetComponentInChildren<TMP_InputField>();
			if ((object)inputField != null)
			{
				inputField.contentType = (wholeNumbersOnly ? TMP_InputField.ContentType.IntegerNumber : TMP_InputField.ContentType.DecimalNumber);
				inputField.characterLimit = ((!wholeNumbersOnly) ? (2 + significantDigits) : 0);
				inputField.text = (wholeNumbersOnly ? currentValue.ToString(CultureInfo.CurrentCulture) : currentValue.ToString("G" + significantDigits, CultureInfo.CurrentCulture));
				if (onValueChanged != null)
				{
					inputField.onValueChanged.AddListener(delegate(string s)
					{
						if (double.TryParse(s, out var result2))
						{
							onValueChanged(result2);
						}
					});
					inputField.onSubmit.AddListener(delegate(string s)
					{
						if (double.TryParse(s, out var result))
						{
							double num = Math.Max(minValue, Math.Min(result, maxValue));
							if (result != num)
							{
								onValueChanged(num);
								inputField.text = num.ToString(CultureInfo.CurrentCulture);
							}
						}
					});
				}
			}
		}
		_DefaultValueLogic(gameObject, defaultValue);
		return gameObject;
	}

	public static GameObject CreateUIList<T>(List<T> listItems, Action<T> onValueChanged = null, Func<T, bool> canBeRemovedFromList = null, Action<T, GameObject> onRemoveClick = null, Func<T, UIListItemData> itemToData = null, string resourcePath = null, float? preferredWidth = null, float? preferredHeight = null, bool closePopupOnSelect = false, string searchText = null, CategorySortType? categorySort = null, bool openCategories = false, Func<T, string> getSearchString = null, Transform parent = null, Func<T, string> itemToString = null)
	{
		resourcePath = resourcePath ?? "UI/UIList Standard";
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(resourcePath), parent);
		UIList uiList = gameObject.GetComponentInChildren<UIList>();
		uiList.openCategoriesByDefault = openCategories;
		if (categorySort.HasValue)
		{
			uiList.categorySorting = categorySort.Value;
		}
		LayoutElement componentInChildren = gameObject.GetComponentInChildren<LayoutElement>();
		if (preferredWidth.HasValue)
		{
			componentInChildren.preferredWidth = preferredWidth.Value;
		}
		if (preferredHeight.HasValue)
		{
			componentInChildren.preferredHeight = preferredHeight.Value;
		}
		itemToData = itemToData ?? ((Func<T, UIListItemData>)((T item) => new UIListItemData((itemToString != null) ? itemToString(item) : item.ToString(), item, null, (getSearchString != null) ? getSearchString(item) : null)));
		uiList.OnGenerate.AddListener(delegate(UIList list)
		{
			list.Set(listItems.Select((T item) => itemToData(item)));
		});
		if (canBeRemovedFromList != null && onRemoveClick != null)
		{
			uiList.OnSet.AddListener(delegate(UIList list)
			{
				for (int i = 0; i < listItems.Count; i++)
				{
					T item2 = listItems[i];
					if (canBeRemovedFromList(item2))
					{
						Transform valueTransform = list.GetValueTransform(item2);
						if ((bool)valueTransform)
						{
							GameObject removeContainer = GetGameObject("UI/UIList Remove Container");
							removeContainer.GetComponentInChildren<Button>(includeInactive: true).onClick.AddListener(delegate
							{
								onRemoveClick(item2, removeContainer);
								list.RemoveValue(item2);
							});
							Transform parent2 = valueTransform.parent;
							int siblingIndex = valueTransform.GetSiblingIndex();
							valueTransform.SetParent(removeContainer.transform, worldPositionStays: false);
							removeContainer.transform.SetParent(parent2, worldPositionStays: false);
							removeContainer.transform.SetSiblingIndex(siblingIndex);
						}
					}
				}
			});
		}
		uiList.GenerateList();
		if (searchText != null)
		{
			uiList.SetSearchText(searchText);
		}
		if (onValueChanged != null)
		{
			uiList.OnSelectedValueChanged.AddListener(delegate(object obj)
			{
				onValueChanged((T)obj);
			});
		}
		if (closePopupOnSelect)
		{
			uiList.OnSelectedValueChanged.AddListener(delegate
			{
				uiList.GetComponentInParent<UIPopupControl>().Close();
			});
		}
		return gameObject;
	}

	private static IEnumerator _ContentRefSearchCloseDelay()
	{
		float timeout = 3f;
		IEnumerator wait = Job.WaitForDepartmentEmpty(Department.Content);
		while (wait.MoveNext())
		{
			yield return wait.Current;
			float num;
			timeout = (num = timeout - Time.unscaledDeltaTime);
			if (num < 0f)
			{
				timeout = (num = 3f);
				if (num > 0f)
				{
					Job.DebugDepartment(Department.Content);
				}
			}
		}
	}

	public static GameObject CreateDataSearchPopup(Type iDataContentType, Action<ContentRef> onDataSelected, Transform parent, Func<ContentRef, bool> validContent = null, bool mustBeCommitted = true, bool ignoreCreatorTypeFilter = false, bool allowHiddenResources = false, bool unloadContent = true)
	{
		GameObject popup = null;
		UnityAction<ContentRef> onSelected = delegate(ContentRef cRef)
		{
			popup.GetComponentInChildren<UIPopupControl>().Close();
			Job.Process(Job.WaitTillDestroyed(popup), Department.UI).Immediately().Do(delegate
			{
				onDataSelected(cRef);
			});
		};
		string title = $"Search <b>{(iDataContentType.IsGenericType ? iDataContentType.FriendlyName().FriendlyFromCamelOrPascalCase().Trim() : iDataContentType.GetUILabel())}</b>";
		GameObject mainContent = ContentRefSearcher.Create(iDataContentType, onSelected, validContent, mustBeCommitted, null, ignoreCreatorTypeFilter, allowHiddenResources, unloadContent);
		IEnumerator delayClose = _ContentRefSearchCloseDelay();
		popup = CreatePopup(title, mainContent, null, null, null, null, null, null, true, true, null, null, null, parent, delayClose, null);
		return popup;
	}

	public static GameObject CreateDataSearchPopup<C>(Action<DataRef<C>> onDataSelected, Transform parent, Func<DataRef<C>, bool> validContent = null, bool mustBeCommitted = true, bool ignoreCreatorTypeFilter = false, bool allowHiddenResources = false, bool unloadContent = true) where C : IDataContent
	{
		return CreateDataSearchPopup(typeof(C), delegate(ContentRef cRef)
		{
			onDataSelected(cRef as DataRef<C>);
		}, parent, (validContent != null) ? ((Func<ContentRef, bool>)((ContentRef cRef) => validContent(cRef as DataRef<C>))) : null, mustBeCommitted, ignoreCreatorTypeFilter, allowHiddenResources, unloadContent);
	}

	public static void CreateImageSearchPopup(ImageCategoryType category, Action<ImageRef> onImageSelected, Transform parent)
	{
		GameObject popup = null;
		UnityAction<ContentRef> onSelected = delegate(ContentRef cRef)
		{
			popup.GetComponentInChildren<UIPopupControl>().Close();
			Job.Process(Job.WaitTillDestroyed(popup), Department.UI).Immediately().Do(delegate
			{
				onImageSelected(cRef as ImageRef);
			});
		};
		string title = $"Search <b>{EnumUtil.FriendlyName(category)}</b> Images";
		GameObject mainContent = ContentRefSearcher.Create(category, onSelected);
		IEnumerator delayClose = _ContentRefSearchCloseDelay();
		popup = CreatePopup(title, mainContent, null, null, null, null, null, null, true, true, null, null, null, parent, delayClose, null);
	}

	public static void CreateAudioSearchPopup(AudioCategoryType category, Action<AudioRef> onAudioSelected, Transform parent)
	{
		GameObject popup = null;
		UnityAction<ContentRef> onSelected = delegate(ContentRef cRef)
		{
			popup.GetComponentInChildren<UIPopupControl>().Close();
			Job.Process(Job.WaitTillDestroyed(popup), Department.UI).Immediately().Do(delegate
			{
				onAudioSelected(cRef as AudioRef);
			});
		};
		string title = $"Search <b>{EnumUtil.FriendlyName(category)}</b> Audio";
		GameObject mainContent = ContentRefSearcher.Create(category, onSelected);
		IEnumerator delayClose = _ContentRefSearchCloseDelay();
		popup = CreatePopup(title, mainContent, null, null, null, null, null, null, true, true, null, null, null, parent, delayClose, null);
	}

	public static void CreateContentSearchPopup(ContentRef contentRef, Action<ContentRef> onContentSelected, Transform parent, string searchText = null, ContentCreatorTypeFlags? creatorTypeFilter = null)
	{
		onContentSelected = onContentSelected ?? ((Action<ContentRef>)delegate
		{
		});
		if (contentRef.isDataRef)
		{
			CreateDataSearchPopup(contentRef.dataType, onContentSelected, parent);
		}
		else if (contentRef is ImageRef)
		{
			CreateImageSearchPopup((contentRef as ImageRef).category, delegate(ImageRef imageRef)
			{
				onContentSelected(imageRef);
			}, parent);
		}
		else if (contentRef is AudioRef)
		{
			CreateAudioSearchPopup((contentRef as AudioRef).category, delegate(AudioRef audioRef)
			{
				onContentSelected(audioRef);
			}, parent);
		}
		if (!searchText.IsNullOrEmpty())
		{
			ContentRefSearcher.Instance.SetSearchText(searchText);
		}
		if (creatorTypeFilter.HasValue)
		{
			ContentRefSearcher.Instance.creatorTypes = creatorTypeFilter.Value;
		}
	}

	public static void CreateImageBrowserPopup(ImageCategoryType category, UnityAction<string> onFileSelected, Transform parent, Texture2D overwrittenContent = null)
	{
		string arg = (overwrittenContent ? "Overwrite" : "Import");
		GameObject gameObject = CreatePopup($"{arg} [{EnumUtil.FriendlyName(category)}] Image", null, null, null, null, null, null, null, true, true, null, null, null, parent, null, null);
		FileBrowserManager.ShowImageBrowser($"{arg} {EnumUtil.FriendlyName(category)} Image", gameObject.GetComponentInChildren<UIPopupControl>().mainContentContainer, overwrittenContent).onFileConfirm.AddListener(onFileSelected);
	}

	public static void CreateAudioBrowserPopup(AudioCategoryType category, UnityAction<string> onFileSelected, Transform parent, UnityEngine.Object overwrittenContent = null)
	{
		string arg = (overwrittenContent ? "Overwrite" : "Import");
		GameObject gameObject = CreatePopup($"{arg} [{EnumUtil.FriendlyName(category)}] Audio", null, null, null, null, null, null, null, true, true, null, null, null, parent, null, null);
		FileBrowserManager.ShowAudioBrowser($"{arg} {EnumUtil.FriendlyName(category)} Audio", gameObject.GetComponentInChildren<UIPopupControl>().mainContentContainer, overwrittenContent).onFileConfirm.AddListener(onFileSelected);
	}

	public static void ConfirmImportPopup<T>(string path, Action<string> onImport, Action<T> onCategorySet, bool isOverwrite, T category, T? defaultCategory, string type, Transform parent) where T : struct, IConvertible
	{
		if (isOverwrite || !defaultCategory.HasValue || EnumUtil<T>.Equals(defaultCategory.Value, category))
		{
			onImport(path);
			return;
		}
		string importDefault = "Import as " + EnumUtil.FriendlyName(defaultCategory) + " " + type;
		string text = "Import as " + EnumUtil.FriendlyName(category) + " " + type;
		string cancel = "Cancel";
		CreatePopup($"Confirm Import as {EnumUtil.FriendlyName(category)} {type}", CreateMessageBox($"Are you sure you wish to import {Path.GetFileName(path)} as {EnumUtil.FriendlyName(category)} {type}? It is recommended that you import as {EnumUtil.FriendlyName(defaultCategory)} {type}."), null, parent: parent, buttons: new string[3] { cancel, text, importDefault }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: false, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (!(s == cancel))
			{
				if (s == importDefault)
				{
					onCategorySet(defaultCategory.Value);
				}
				onImport(path);
			}
		});
	}

	public static async void CreateWorkshopDataSearchPopup(ContentRef dataRef, Action<ContentRef> onDataDownloaded, Transform parent, IEnumerable<KeyValuePair<string, string>> additionalKeyValueTags = null)
	{
		if (!Steam.CanUseWorkshop)
		{
			GameObject mainContent = CreateMessageBox("Please make sure you're connected to Steam.", TextAlignmentOptions.Center, 32, 600, 300, 24f);
			Transform parent2 = parent;
			CreatePopup("You Must Be Logged Into Steam To Search The Workshop", mainContent, null, null, null, null, null, null, true, true, null, null, null, parent2, null, null);
		}
		else
		{
			if (!(await ProfileManager.prefs.steam.ShowEULAIfNeeded(parent)))
			{
				return;
			}
			GameObject popup = null;
			UnityAction<ContentRef> onContentDownloaded = delegate(ContentRef cRef)
			{
				Job.Process(Job.WaitTillDestroyed(popup), Department.UI).Immediately().Do(delegate
				{
					onDataDownloaded(cRef);
				});
			};
			string title = $"Search Workshop For <b>{dataRef.specificTypeFriendly}</b>";
			GameObject mainContent2 = SteamWorkshopSearcher.Create(dataRef, onContentDownloaded, additionalKeyValueTags);
			Transform parent2 = parent;
			IEnumerator delayClose = _ContentRefSearchCloseDelay();
			popup = CreatePopup(title, mainContent2, null, null, null, null, null, null, true, true, null, null, null, parent2, delayClose, null);
		}
	}

	public static async void CreateWorkshopImageSearchPopup(ImageRef imageRef, Action<ImageRef> onImageDownloaded, Transform parent)
	{
		if (!(await ProfileManager.prefs.steam.ShowEULAIfNeeded(parent)))
		{
			return;
		}
		GameObject popup = null;
		UnityAction<ContentRef> onContentDownloaded = delegate(ContentRef cRef)
		{
			popup.GetComponentInChildren<UIPopupControl>().Close();
			Job.Process(Job.WaitTillDestroyed(popup), Department.UI).Immediately().Do(delegate
			{
				onImageDownloaded(cRef as ImageRef);
			});
		};
		string title = $"Search Workshop For <b>{EnumUtil.FriendlyName(imageRef.category)}</b> Images";
		GameObject mainContent = SteamWorkshopSearcher.Create(imageRef, onContentDownloaded);
		IEnumerator delayClose = _ContentRefSearchCloseDelay();
		popup = CreatePopup(title, mainContent, null, null, null, null, null, null, true, true, null, null, null, parent, delayClose, null);
	}

	public static async void CreateWorkshopAudioSearchPopup(AudioRef audioRef, Action<AudioRef> onAudioDownloaded, Transform parent)
	{
		if (!(await ProfileManager.prefs.steam.ShowEULAIfNeeded(parent)))
		{
			return;
		}
		GameObject popup = null;
		UnityAction<ContentRef> onContentDownloaded = delegate(ContentRef cRef)
		{
			popup.GetComponentInChildren<UIPopupControl>().Close();
			Job.Process(Job.WaitTillDestroyed(popup), Department.UI).Immediately().Do(delegate
			{
				onAudioDownloaded(cRef as AudioRef);
			});
		};
		string title = $"Search Workshop For <b>{EnumUtil.FriendlyName(audioRef.category)}</b> Audio";
		GameObject mainContent = SteamWorkshopSearcher.Create(audioRef, onContentDownloaded);
		IEnumerator delayClose = _ContentRefSearchCloseDelay();
		popup = CreatePopup(title, mainContent, null, null, null, null, null, null, true, true, null, null, null, parent, delayClose, null);
	}

	public static IEnumerator ShowWebBrowser(string url, Transform parent, string friendlyURL = null, bool skipConfirmation = false, string title = "Open Web Page Request", string message = "Would you like to open the following web page?", string confirm = "Open Web Page")
	{
		if (!skipConfirmation)
		{
			bool openWebPage = false;
			GameObject gameObject = CreatePopup(title, CreateMessageBox(message + "\n<u>" + (friendlyURL ?? url) + "</u>", TextAlignmentOptions.Left, 24, 800, 300, 24f), null, parent: parent, buttons: new string[2]
			{
				MessageData.UIPopupButton.Cancel.GetButton().Localize(),
				confirm
			}, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
			{
				openWebPage = s == confirm;
			});
			WebBrowserRequestTransform = gameObject.transform;
			gameObject.GetComponentInChildren<UIPopupControl>().onClose.AddListener(delegate
			{
				WebBrowserRequestTransform = null;
			});
			IEnumerator waitTillDestroy = Job.WaitTillDestroyed(gameObject);
			while (waitTillDestroy.MoveNext())
			{
				yield return null;
			}
			if (!openWebPage)
			{
				yield break;
			}
		}
		if (!Application.isEditor && Steam.OverlayEnabled)
		{
			SteamFriends.ActivateGameOverlayToWebPage(url);
			while (!Steam.OverlayActive)
			{
				yield return null;
			}
			while (Steam.OverlayActive)
			{
				yield return null;
			}
		}
		else
		{
			Application.OpenURL(url);
			while (Application.isFocused)
			{
				yield return null;
			}
			while (!Application.isFocused)
			{
				yield return null;
			}
		}
	}

	public static Job JoinDiscord(Transform parent)
	{
		return Job.Process(ShowWebBrowser("https://discord.gg/JFNSwsRGpA", parent, "", skipConfirmation: false, MessageData.UIPopupTitle.OpenExternalBrowser.GetTitle().Localize(), MessageData.UIPopupMessage.OpenDiscordBrowser.GetMessage().Localize(), MessageData.UIPopupButton.OpenExternalBrowser.GetButton().Localize()), Department.UI);
	}

	public static Job OpenStorePage(Transform parent)
	{
		return Job.Process(ShowWebBrowser("https://store.steampowered.com/app/1815570/Aces_and_Adventures/", parent, null, skipConfirmation: true));
	}

	public static GameObject CreateContentRefTagsPopup(ContentRef contentRef, Transform parent, IDataContent iDataContent = null, Action onSaveChanges = null, string confirmButton = null)
	{
		string originalTags = ((iDataContent == null) ? contentRef.tags : iDataContent.tags) ?? "";
		ContentRefTags tags = new ContentRefTags(originalTags);
		string save = confirmButton ?? "Save Changes";
		Action<string> saveChanges = delegate(string s)
		{
			tags.tags = tags.tags.ToTagString();
			if (!(s != save) && !(originalTags == tags))
			{
				if (iDataContent == null)
				{
					contentRef.tags = tags;
				}
				else
				{
					iDataContent.tags = tags;
				}
				if (onSaveChanges == null)
				{
					contentRef.SaveReferenceChanges();
				}
				else
				{
					onSaveChanges();
				}
			}
		};
		string title = string.Format("Edit Tags For [{0}] {1} {2}", contentRef.friendlyName, contentRef.GetFriendlyCategoryName(), (contentRef.type != "Data") ? contentRef.type : "");
		GameObject mainContent = CreateReflectedObject(tags, 1280f, 128f);
		string[] buttons = new string[2] { save, "Cancel" };
		Action<string> onButtonClick = saveChanges;
		GameObject popup = CreatePopup(title, mainContent, null, null, null, null, null, null, displayCloseButton: true, blockAllRaycasts: true, null, onButtonClick, null, parent, null, null, buttons);
		TMP_InputField componentInChildren = popup.GetComponentInChildren<TMP_InputField>();
		if (!componentInChildren.text.IsNullOrEmpty())
		{
			componentInChildren.text += " ";
		}
		componentInChildren.lineType = TMP_InputField.LineType.MultiLineSubmit;
		componentInChildren.FocusAndMoveToEnd();
		componentInChildren.onSubmit.AddListener(delegate
		{
			saveChanges(save);
			popup.GetComponentInChildren<UIPopupControl>().Close();
		});
		popup.GetComponentInChildren<ScrollRect>().movementType = ScrollRect.MovementType.Clamped;
		return popup;
	}

	public static GameObject CreateContentRefRenamePopup(ContentRef contentRef, Transform parent, Action<string> onNameChange = null)
	{
		string originalName = contentRef.name ?? "";
		ContentRefName name = new ContentRefName(originalName, contentRef.maxNameLength);
		string save = "Save Changes";
		Action<string> saveChanges = delegate(string s)
		{
			name.name = name.name.SetRedundantStringNull();
			if (!(s != save) && !(originalName == name) && name.name != null)
			{
				contentRef.name = name;
				contentRef.SaveReferenceChanges();
				if (onNameChange != null)
				{
					onNameChange(contentRef.name);
				}
			}
		};
		string title = $"Rename [{contentRef.friendlyName}] {contentRef.GetFriendlyCategoryName()} {contentRef.type}";
		GameObject mainContent = CreateReflectedObject(name, 1280f, 80f);
		string[] buttons = new string[2] { save, "Cancel" };
		Action<string> onButtonClick = saveChanges;
		GameObject popup = CreatePopup(title, mainContent, null, null, null, null, null, null, displayCloseButton: true, blockAllRaycasts: true, null, onButtonClick, null, parent, null, null, buttons);
		TMP_InputField componentInChildren = popup.GetComponentInChildren<TMP_InputField>();
		componentInChildren.FocusAndMoveToEnd(selectAll: true);
		componentInChildren.onSubmit.AddListener(delegate
		{
			saveChanges(save);
			popup.GetComponentInChildren<UIPopupControl>().Close();
		});
		popup.GetComponentInChildren<ScrollRect>().movementType = ScrollRect.MovementType.Clamped;
		return popup;
	}

	private static void _CreateContentRefUploadPopup(ContentRef contentRef, Transform parent, Action<bool> onUploadResult)
	{
		Steam.Ugc.CreateItemTransform = parent;
		BeginProcessJob(parent, "Uploading To Steam Workshop", Department.Content).Immediately().DoProcess(contentRef.UploadAsync().AsEnumerator()).Immediately()
			.ResultAction(delegate(bool success)
			{
				CreateLogTextPopup("Upload Result: " + success.ToText("<color=#77FF77FF>Success", "<color=#FF4444FF>Failure"), Log.EndLog(), 1600, 640, parent);
				if (onUploadResult != null)
				{
					onUploadResult(success);
				}
			})
			.Immediately()
			.Do(EndProcess);
	}

	public static async void CreateContentRefUploadPopup(ContentRef contentRef, Transform parent, Action<bool> onUploadResult = null)
	{
		if (!(await ProfileManager.prefs.steam.ShowEULAIfNeeded(parent)))
		{
			return;
		}
		ContentRef.UploadOptions uploadOptions = new ContentRef.UploadOptions(contentRef);
		CreatePopup($"Upload <b>{contentRef.name}</b> {contentRef.specificTypeFriendly} To Steam Workshop", CreateReflectedObject(uploadOptions, 900f, 640f), null, parent: parent, buttons: new string[2] { "Cancel", "Upload" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (!(s != "Upload"))
			{
				Log.BeginLog();
				_CreateContentRefUploadPopup(uploadOptions.CommitOptions(), parent, onUploadResult);
			}
		});
	}

	public static async void CreateSteamWorkshopItemInspectPopup(PublishedFileId_t publishedFileId, Transform parent, bool returnMetaData = false, bool returnAdditionalPreviews = true, Action<ContentRef> onUpdated = null)
	{
		GameObject mainContent;
		GameObject reflectedUI = (mainContent = CreateReflectedObject(null, 1280f, 420f));
		GameObject popup = CreatePopup("Workshop Item Details", mainContent, null, null, null, null, null, null, true, true, null, null, null, parent, null, null);
		Steam.Ugc.Query.Result itemResult = await Steam.Ugc.GetDetailsAsync(publishedFileId, returnMetaData, returnChildren: false, returnKeyValueTags: true, returnAdditionalPreviews);
		if (!itemResult)
		{
			return;
		}
		Steam.Ugc.Query.Result creatorResult = await Steam.Ugc.GetAuthorFile(itemResult.ownerId);
		if (!creatorResult)
		{
			return;
		}
		await Steam.Friends.GetPersonaNameAsync(itemResult.ownerId);
		if (!reflectedUI || !reflectedUI.activeInHierarchy)
		{
			return;
		}
		Steam.Ugc.InspectQueryResult obj = new Steam.Ugc.InspectQueryResult(creatorResult, itemResult, popup.GetComponentInChildren<UIPopupControl>().mainContentContainer, onUpdated);
		reflectedUI.GetComponentInChildren<UIGeneratorType>().GenerateFromObject(obj);
		Action<PublishedFileId_t, Steam.Ugc.UserItemVoteResult, Steam.Ugc.UserItemVoteResult> onVoteChange = delegate(PublishedFileId_t id, Steam.Ugc.UserItemVoteResult oldVote, Steam.Ugc.UserItemVoteResult newVote)
		{
			if (id == creatorResult)
			{
				ProfileManager.prefs.steam.SetAuthorLike(creatorResult.ownerId, newVote == Steam.Ugc.UserItemVoteResult.UpVote);
			}
		};
		Steam.Ugc.OnItemVoteChange += onVoteChange;
		await new AwaitCoroutine<object>(Job.WaitTillDestroyed(reflectedUI));
		Steam.Ugc.OnItemVoteChange -= onVoteChange;
	}

	public static Job CreateContentRefDownloadPopup(Steam.Ugc.Query.Result result, UnityAction<ContentRef> onContentDownloaded, Transform parent, Action onFinish = null, Action onDownloadFailed = null, string subMessage = null)
	{
		Log.BeginLog();
		Transform parent2 = parent;
		float? progress = (result.hasMetaData ? new float?(0f) : null);
		return BeginProcessJob(parent2, "Downloading Workshop Items", null, "UI/Processing Popup Standard", progress, null, subMessage).Immediately().DoProcess(ContentRef.DownloadAndInstallAsync(result).AsEnumerator()).Immediately()
			.ResultAction(delegate(ContentRef downloadedContent)
			{
				if ((bool)downloadedContent)
				{
					onContentDownloaded(downloadedContent);
				}
				else if (onDownloadFailed != null)
				{
					onDownloadFailed();
				}
				string message = Log.EndLog();
				if (!downloadedContent)
				{
					CreateLogTextPopup("Download Result: " + StringUtil.ToText(downloadedContent, "<color=#77FF77FF>Success", "<color=#FF4444FF>Failure"), message, 1600, 640, parent);
				}
				if (onFinish != null)
				{
					onFinish();
				}
			})
			.Immediately()
			.Do(EndProcess);
	}

	public static GameObject CreateLegalAgreementPopup(string title, string text, Action onTermsAccepted, Transform parent, int? fontSizeOverride = null)
	{
		Vector2? referenceResolution = ReferenceResolution;
		GameObject gameObject = CreatePopup(title, null, null, null, null, null, null, null, true, true, null, null, null, parent, null, referenceResolution);
		UIPopupControl popup = gameObject.GetComponentInChildren<UIPopupControl>();
		LegalAgreementView.Create((fontSizeOverride.HasValue ? ("<size=" + fontSizeOverride.Value + ">") : "") + text, popup.mainContentContainer).onTermsAccepted.AddListener(delegate
		{
			if (onTermsAccepted != null)
			{
				onTermsAccepted();
			}
			popup.Close();
		});
		return gameObject;
	}

	public static GameObject CreateSceneSelectPopup(IEnumerable<SceneRef> scenes, Transform parent = null, string popupTitle = "Select Scene", Vector2? center = null, Vector2? pivot = null, Action<SceneRef> onSceneSelected = null, Func<SceneRef, string> toString = null)
	{
		return CreatePopup(popupTitle.HasVisibleCharacter() ? popupTitle : "Select Scene", CreateUIList(scenes.ToList(), onSceneSelected ?? ((Action<SceneRef>)delegate(SceneRef sceneRef)
		{
			LoadScreenView.Load(sceneRef);
		}), null, null, null, null, null, null, closePopupOnSelect: false, null, null, openCategories: false, null, null, toString), null, null, null, center, pivot, null, true, true, null, null, null, parent, null, null);
	}

	public static Transform CreateOverlayedCanvas(Transform childOfCurrentCanvas)
	{
		Canvas componentInParent = childOfCurrentCanvas.GetComponentInParent<Canvas>();
		GameObject gameObject = new GameObject("Overlay Canvas");
		gameObject.transform.SetParent(componentInParent.transform.parent, worldPositionStays: false);
		Canvas canvas = gameObject.AddComponent<Canvas>();
		canvas.CopyFrom(componentInParent);
		int sortingOrder = canvas.sortingOrder + 1;
		canvas.sortingOrder = sortingOrder;
		CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
		canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
		canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
		canvasScaler.matchWidthOrHeight = 0.5f;
		gameObject.AddComponent<GraphicRaycaster>();
		gameObject.AddComponent<CanvasInputFocus>();
		return canvas.transform;
	}

	public static Transform CreateTemporaryOverlayedCanvas(Transform childOfCurrentCanvas)
	{
		Transform overlayCanvas = CreateOverlayedCanvas(childOfCurrentCanvas);
		Job.Process(Job.WaitForNoChildren(overlayCanvas)).Then().Do(delegate
		{
			UnityEngine.Object.Destroy(overlayCanvas.gameObject);
		});
		return overlayCanvas;
	}

	public static GameObject CreateEnumComboBox(string enumClassName, string label, UnityAction<object> onValueChanged = null, string defaultSelected = "", string resourcePath = null, Dictionary<string, string> categoryData = null, HashSet<string> exclude = null, int indentPixels = 24, int maxCount = 0, Transform parent = null, bool toggleListOnSelect = true)
	{
		Type type = EnumGenerator.GetType(enumClassName);
		bool flag = type.HasAttribute<FlagsAttribute>();
		resourcePath = (flag ? "UI/Combo Box Multi Standard" : (resourcePath ?? "UI/Combo Box Standard"));
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(resourcePath), parent).SetUILabel(label);
		gameObject.SetActive(value: true);
		ComboBox componentInChildren = gameObject.GetComponentInChildren<ComboBox>();
		componentInChildren.toggleListOnSelected = toggleListOnSelect;
		componentInChildren.indentPixels = indentPixels;
		StringTable table = UITable;
		componentInChildren.OnGenerate.AddListener(delegate(UIList c)
		{
			UIListGeneratorEnum.Generate(c, enumClassName, defaultSelected, categoryData, exclude, table);
		});
		componentInChildren.SetMaxCount(maxCount);
		if (onValueChanged != null)
		{
			if (!flag)
			{
				componentInChildren.OnSelectedValueChanged.AddListener(delegate(object obj)
				{
					onValueChanged(Enum.Parse(type, obj.ToString()));
				});
				return gameObject;
			}
			componentInChildren.OnSelectedTextChanged.AddListener(delegate(string enumString)
			{
				onValueChanged((!enumString.IsNullOrEmpty()) ? Enum.Parse(type, StringUtil.ParseFriendlyEnumFlags(enumString)) : Enum.ToObject(type, 0));
			});
		}
		return gameObject;
	}

	public static GameObject CreateEnumComboBox<C>(Action<C> onValueChanged, Transform parent = null, C? defaultSelected = null, bool fitIntoParent = true, string label = null, int indentPixels = 0, IEnumerable<C> exclude = null) where C : struct, IConvertible
	{
		Type typeFromHandle = typeof(C);
		GameObject gameObject = CreateEnumComboBox(typeFromHandle.FullName, label ?? ">", delegate(object obj)
		{
			onValueChanged((C)obj);
		}, (!defaultSelected.HasValue) ? "" : (typeFromHandle.HasAttribute<FlagsAttribute>() ? BitMask.ToString(typeFromHandle, defaultSelected.Value) : defaultSelected.Value.ToString()), null, null, parent: parent, indentPixels: indentPixels, exclude: (exclude != null) ? new HashSet<string>(exclude.Select(EnumUtil.Name)) : null);
		if (fitIntoParent)
		{
			LayoutElement componentInChildrenOnly = gameObject.GetComponentInChildrenOnly<LayoutElement>();
			componentInChildrenOnly.minHeight = componentInChildrenOnly.preferredHeight;
			(gameObject.transform as RectTransform).Stretch();
		}
		if (indentPixels <= 0)
		{
			UnityEngine.Object.Destroy(gameObject.GetComponentInChildren<IndentFitter>());
		}
		return gameObject;
	}

	public static GameObject CreateTypeComboBox(string baseClassName, string label, UnityAction<Type> onValueChanged = null, bool derivedTypesOnly = false, string defaultSelected = "", string resourcePath = null, int indentPixels = 0, Func<Type, bool> excludeType = null, Transform parent = null)
	{
		resourcePath = resourcePath ?? "UI/Combo Box Standard";
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(resourcePath), parent).SetUILabel(label);
		gameObject.SetActive(value: true);
		ComboBox componentInChildren = gameObject.GetComponentInChildren<ComboBox>();
		componentInChildren.indentPixels = indentPixels;
		componentInChildren.OnGenerate.AddListener(delegate(UIList c)
		{
			UIListGeneratorDerived.Generate(c, baseClassName, defaultSelected, !derivedTypesOnly, excludeType);
		});
		if (onValueChanged != null)
		{
			componentInChildren.OnSelectedValueChanged.AddListener(delegate(object type)
			{
				onValueChanged(type as Type);
			});
		}
		return gameObject;
	}

	public static GameObject CreateGenericComboBox<T>(Dictionary<string, T> data, string label, Action<T> onValueChanged = null, T defaultSelected = default(T), CategorySortType categorySort = CategorySortType.Alphabetical, Dictionary<string, string> categoryData = null, HashSet<T> exclude = null, int maxCount = 0, int indentPixels = 0, string resourcePath = null, Transform parent = null, bool fitIntoParent = false, bool useFriendlyName = true)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(resourcePath ?? "UI/Combo Box Standard"), parent).SetUILabel(label ?? ">");
		gameObject.SetActive(value: true);
		ComboBox componentInChildren = gameObject.GetComponentInChildren<ComboBox>();
		componentInChildren.indentPixels = indentPixels;
		componentInChildren.OnGenerate.AddListener(delegate(UIList c)
		{
			UIList.GenerateGeneric(c, data, defaultSelected, categorySort, categoryData, exclude, useFriendlyName);
		});
		componentInChildren.SetMaxCount(maxCount);
		if (onValueChanged != null)
		{
			componentInChildren.OnSelectedValueChanged.AddListener(delegate(object obj)
			{
				onValueChanged((T)obj);
			});
		}
		if (fitIntoParent)
		{
			LayoutElement componentInChildrenOnly = gameObject.GetComponentInChildrenOnly<LayoutElement>();
			componentInChildrenOnly.minHeight = componentInChildrenOnly.preferredHeight;
			(gameObject.transform as RectTransform).Stretch();
		}
		if (indentPixels <= 0)
		{
			UnityEngine.Object.Destroy(gameObject.GetComponentInChildren<IndentFitter>());
		}
		return gameObject;
	}

	public static GameObject CreateGenericComboBoxPopup<T>(string title, Dictionary<string, T> data, string label, Action<T> onValueChanged = null, T defaultSelected = default(T), CategorySortType categorySort = CategorySortType.Alphabetical, Dictionary<string, string> categoryData = null, HashSet<T> exclude = null, int maxCount = 0, int indentPixels = 0, Transform parent = null, float preferredWidth = 1280f, float preferredHeight = 720f)
	{
		GameObject gameObject = GetGameObject("UI/Reflection/UIReflectedObject");
		LayoutElement component = gameObject.GetComponent<LayoutElement>();
		component.preferredWidth = preferredWidth;
		component.preferredHeight = preferredHeight;
		GameObject popup = null;
		bool popupCreated = false;
		Action<T> onValueChanged2 = delegate(T t)
		{
			if (popupCreated)
			{
				if (onValueChanged != null)
				{
					onValueChanged(t);
				}
				popup.GetComponentInChildren<UIPopupControl>().Close();
			}
		};
		GameObject gameObject2 = CreateGenericComboBox(data, label, onValueChanged2, defaultSelected, categorySort, categoryData, exclude, maxCount, indentPixels, null, gameObject.GetComponentInChildren<UIGeneratorType>().transform);
		popup = CreatePopup(title, gameObject, null, null, null, null, null, null, true, true, null, null, null, parent, null, null);
		gameObject2.GetComponentInChildren<CollapseFitter>().ForceOpen();
		LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject2.transform as RectTransform);
		popupCreated = true;
		return gameObject;
	}

	public static GameObject CreateToggle(string label, bool isOn = false, UnityAction<bool> onValueChanged = null, string resourcePath = null, Transform parent = null)
	{
		resourcePath = resourcePath ?? "UI/Toggle Standard";
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(resourcePath), parent).SetUILabel(label);
		gameObject.SetActive(value: true);
		Toggle componentInChildren = gameObject.GetComponentInChildren<Toggle>();
		componentInChildren.isOn = isOn;
		if (onValueChanged != null)
		{
			componentInChildren.onValueChanged.AddListener(onValueChanged);
			componentInChildren.onValueChanged.Invoke(componentInChildren.isOn);
		}
		return gameObject;
	}

	public static GameObject CreateInputField(string label, UnityAction<string> onValueChanged = null, string defaultText = "", int? maxChars = null, InputField.ContentType? contentType = null, string resourcePath = null, Transform parent = null, bool readOnly = false, string placeHolderText = null)
	{
		resourcePath = resourcePath ?? "UI/Input Field Standard";
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(resourcePath), parent).SetUILabel(label);
		gameObject.SetActive(value: true);
		maxChars = maxChars ?? 32;
		TMP_InputField componentInChildren = gameObject.GetComponentInChildren<TMP_InputField>();
		if ((bool)componentInChildren)
		{
			componentInChildren.characterLimit = maxChars.Value;
			if (contentType.HasValue)
			{
				componentInChildren.contentType = contentType.Value.ToTMPContentType();
			}
			if (onValueChanged != null)
			{
				componentInChildren.onValueChanged.AddListener(onValueChanged);
				if (defaultText != null)
				{
					componentInChildren.text = defaultText;
				}
			}
			componentInChildren.readOnly = readOnly;
			if (placeHolderText.HasVisibleCharacter())
			{
				TextMeshProUGUI componentInChildren2 = UnityEngine.Object.Instantiate(componentInChildren.GetComponentInChildren<TextMeshProUGUI>().gameObject, componentInChildren.transform).GetComponentInChildren<TextMeshProUGUI>();
				componentInChildren2.text = placeHolderText;
				componentInChildren2.color = componentInChildren2.color.MultiplyAlpha(0.666f);
				componentInChildren.placeholder = componentInChildren2;
			}
		}
		else
		{
			InputField componentInChildren3 = gameObject.GetComponentInChildren<InputField>();
			componentInChildren3.characterLimit = maxChars.Value;
			if (contentType.HasValue)
			{
				componentInChildren3.contentType = contentType.Value;
			}
			if (onValueChanged != null)
			{
				componentInChildren3.onValueChanged.AddListener(onValueChanged);
				if (defaultText != null)
				{
					componentInChildren3.text = defaultText;
				}
			}
			componentInChildren3.readOnly = readOnly;
		}
		return gameObject;
	}

	public static GameObject CreateMargin(float height = 24f, Transform parent = null, float width = -1f)
	{
		GameObject gameObject = new GameObject("Margin", typeof(RectTransform));
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		LayoutElement layoutElement = gameObject.AddComponent<LayoutElement>();
		layoutElement.preferredHeight = height;
		layoutElement.preferredWidth = width;
		return gameObject;
	}

	public static GameObject CreateHeader(string headerText, Color32 tint, Transform parent = null, string resourcePath = null)
	{
		resourcePath = resourcePath ?? "UI/Header Standard";
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(resourcePath), parent);
		gameObject.GetComponentInChildren<TextMeshProUGUI>().text = headerText;
		gameObject.GetComponentsInChildren<Image>()[1].color = tint;
		return gameObject;
	}

	public static GameObject CreateCollapse(string label, string resourcePath = null, Transform parent = null, TMP_FontAsset fontOverride = null, float? maxSizeOverride = null, FontStyles? styleOverride = null)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(resourcePath ?? "UI/Collapse Standard"), parent);
		TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>();
		if ((bool)fontOverride)
		{
			componentInChildren.font = fontOverride;
		}
		if (maxSizeOverride.HasValue)
		{
			componentInChildren.fontSizeMax = maxSizeOverride.Value;
		}
		if (styleOverride.HasValue)
		{
			componentInChildren.fontStyle = styleOverride.Value;
		}
		componentInChildren.text = label;
		return gameObject;
	}

	public static GameObject CreateIList(IList list, Type listObjectType, string label, int maxCount = 0, string resourcePath = null, Action onAddOrRemove = null, bool showAddData = false, int indentPixels = 24, bool fixedSize = false, Func<object, bool> excludedValues = null, Func<object, object> onPrepareToAdd = null, Transform parent = null, int? pageSize = null)
	{
		resourcePath = resourcePath ?? "UI/IList Standard";
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(resourcePath), parent);
		gameObject.SetUILabel(label);
		if (showAddData)
		{
			Type type = (listObjectType.IsEnum ? listObjectType : ((listObjectType.IsKeyValuePair() && listObjectType.GetGenericArguments()[0].IsEnum) ? listObjectType.GetGenericArguments()[0] : null));
			if (type != null)
			{
				int length = Enum.GetValues(type).Length;
				maxCount = ((maxCount == 0) ? length : Mathf.Min(maxCount, length));
			}
		}
		gameObject.GetComponentInChildren<IListUIControl>().Initialize(list, listObjectType, maxCount, resourcePath, onAddOrRemove, indentPixels, showAddData, fixedSize, excludedValues, onPrepareToAdd, pageSize);
		return gameObject;
	}

	public static GameObject CreateMethodButton(string label, Action method, Transform parent = null, string resourcePath = null, Action onValidate = null)
	{
		resourcePath = resourcePath ?? "UI/Button Standard";
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(resourcePath), parent).SetUILabel(label);
		gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate
		{
			method();
		});
		if (onValidate != null)
		{
			gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate
			{
				onValidate();
			});
		}
		return gameObject;
	}

	public static GameObject CreateAudioEditor(AudioClip clip, string inputPath, int oggQuality = 10, string outputPath = null, float maxLength = 0f, bool forceMono = false, float? loudnessTarget = -16f, float maxPeak = -1f, bool dualMono = false, string resourcePath = "UI/Audio Editor Standard", Transform parent = null)
	{
		outputPath = outputPath ?? inputPath;
		GameObject gameObject = GetGameObject(resourcePath, parent);
		AudioEditorControl componentInChildren = gameObject.GetComponentInChildren<AudioEditorControl>();
		componentInChildren.inputPath = inputPath;
		componentInChildren.outputPath = outputPath;
		componentInChildren.oggQuality = oggQuality;
		componentInChildren.maxLength = maxLength;
		componentInChildren.forceMono = forceMono;
		componentInChildren.loudnessTarget = loudnessTarget;
		componentInChildren.maxPeak = maxPeak;
		componentInChildren.dualMono = dualMono;
		componentInChildren.SetAudioClip(clip);
		return gameObject;
	}

	public static GameObject CreateImageAlphaShapeEditor(Texture2D image, Transform parent = null, string resourcePath = "UI/Image AlphaShape Editor Standard")
	{
		GameObject gameObject = GetGameObject(resourcePath, parent);
		gameObject.GetComponentInChildren<FindAlphaShapeControl>().SetTexture(image);
		return gameObject;
	}

	public static GameObject CreateReflectedObject(object obj, float preferredWidth, float preferredHeight, bool persistUI = false, string resourcePath = "UI/Reflection/UIReflectedObject", Transform parent = null, bool autoHideScrollbar = false, Vector4? paddingOverride = null)
	{
		GameObject gameObject = GetGameObject(resourcePath, parent);
		UIGeneratorType componentInChildren = gameObject.GetComponentInChildren<UIGeneratorType>();
		componentInChildren.persistData = persistUI;
		if (obj is Type)
		{
			componentInChildren.GenerateFromType(obj);
		}
		else if (obj != null)
		{
			componentInChildren.GenerateFromObject(obj);
		}
		LayoutElement component = gameObject.GetComponent<LayoutElement>();
		component.preferredWidth = preferredWidth;
		component.preferredHeight = preferredHeight;
		if (paddingOverride.HasValue)
		{
			gameObject.GetComponentInChildren<VerticalLayoutGroup>().padding.SetPadding(paddingOverride.Value);
		}
		if (autoHideScrollbar)
		{
			gameObject.GetComponentInChildren<ScrollRect>().verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
		}
		ScrollRect componentInChildren2 = gameObject.GetComponentInChildren<ScrollRect>();
		if ((object)componentInChildren2 != null)
		{
			componentInChildren2.scrollSensitivity = 48f;
		}
		return gameObject;
	}

	public static GameObject CreateMessageBox(string message, TextAlignmentOptions alignment = TextAlignmentOptions.Center, int fontSize = 32, int width = 600, int height = 300, float? margin = null, string resourcePath = "UI/Message Box Standard")
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(resourcePath));
		LayoutElement component = gameObject.GetComponent<LayoutElement>();
		component.preferredWidth = width;
		component.preferredHeight = height;
		TextMeshProUGUI componentInChildren = gameObject.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		componentInChildren.alignment = alignment;
		componentInChildren.fontSize = fontSize;
		if (margin.HasValue)
		{
			componentInChildren.margin += Vector4.one * margin.Value;
		}
		gameObject.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f;
		gameObject.SetActive(value: true);
		componentInChildren.text = message;
		return gameObject;
	}

	public static GameObject CreateLogTextBox(string message, int width = 1600, int height = 640, Transform parent = null)
	{
		GameObject gameObject = GetGameObject("UI/Log Box Standard", parent);
		gameObject.GetComponent<LayoutElement>().SetPreferredSize(new Vector2(width, height));
		gameObject.GetComponentInChildren<TMP_InputField>().text = message;
		return gameObject;
	}

	public static GameObject CreateLogTextPopup(string title, string message, int width = 1600, int height = 640, Transform parent = null)
	{
		GameObject mainContent = CreateLogTextBox(message, width, height);
		Vector2? referenceResolution = ReferenceResolution;
		string[] buttons = new string[1] { "Okay" };
		return CreatePopup(title, mainContent, null, null, null, null, null, null, displayCloseButton: true, blockAllRaycasts: true, null, null, null, parent, null, referenceResolution, buttons);
	}

	public static GameObject CreatePopup(string title, GameObject mainContent, RectTransform sizeReference = null, Vector2? size = null, RectTransform centerReferece = null, Vector2? center = null, Vector2? pivot = null, Action onClose = null, bool displayCloseButton = true, bool blockAllRaycasts = true, string resourcePath = null, Action<string> onButtonClick = null, Color? rayCastBlockerColor = null, Transform parent = null, IEnumerator delayClose = null, Vector2? referenceResolution = null, params string[] buttons)
	{
		buttons = buttons ?? new string[0];
		GameObject gameObject = GetGameObject(resourcePath ?? "UI/Popup Standard");
		UIPopupControl component = gameObject.GetComponent<UIPopupControl>();
		component.transform.SetParent(parent ? parent : (DataRefControl.ActiveCanvas?.transform ?? CameraManager.Instance.GetUICanvasTransform()), worldPositionStays: false);
		component.SetFitterData(sizeReference, size ?? new Vector2(1f, 1f), centerReferece, center ?? new Vector2(0.5f, 0.5f), pivot ?? new Vector2(0.5f, 0.5f));
		component.SetBlocksAllRayCasts(blockAllRaycasts);
		if (rayCastBlockerColor.HasValue)
		{
			component.SetRayCastBlockerColor(rayCastBlockerColor.Value);
		}
		component.SetTitleData(title, displayCloseButton);
		component.closeOnRayCastBlockerClicked = displayCloseButton;
		component.SetMainContent(mainContent);
		if (onButtonClick != null)
		{
			component.onButtonClicked.AddListener(delegate(string s)
			{
				onButtonClick(s);
			});
		}
		if (onClose != null)
		{
			component.onClose.AddListener(delegate
			{
				onClose();
			});
		}
		component.delayClose = delayClose;
		component.SetButtons(buttons);
		component.referenceResolution = referenceResolution;
		return gameObject;
	}

	public static Job CreatePopupJob(string title, GameObject mainContent, RectTransform sizeReference = null, Vector2? size = null, RectTransform centerReferece = null, Vector2? center = null, Vector2? pivot = null, Action onClose = null, bool displayCloseButton = true, bool blockAllRaycasts = true, string resourcePath = null, Action<string> onButtonClick = null, Color? rayCastBlockerColor = null, Transform parent = null, string[] buttons = null, Department? department = null, IEnumerator delayClose = null, Vector2? referenceResolution = null)
	{
		UIPopupControl componentInChildren = CreatePopup(title, mainContent, sizeReference, size, centerReferece, center, pivot, onClose, displayCloseButton, blockAllRaycasts, resourcePath, onButtonClick, rayCastBlockerColor, parent, delayClose, referenceResolution, buttons).GetComponentInChildren<UIPopupControl>();
		string output = "";
		componentInChildren.onButtonClicked.AddListener(delegate(string s)
		{
			output = s;
		});
		return Job.Process(Job.WaitTillDestroyed(componentInChildren), department).Break(ref componentInChildren.onCloseAction).Immediately()
			.DoResult(() => output)
			.If((string s) => !s.IsNullOrEmpty());
	}

	public static Job BeginProcessJob(Transform parent, string message = null, Department? department = null, string resourcePath = "UI/Processing Popup Standard", float? progress = null, Action onCancel = null, string subMessage = null)
	{
		return Job.Action(delegate
		{
			_BeginProcess(parent, message, resourcePath, progress, onCancel, subMessage);
		}, department ?? Department.Content);
	}

	private static void _BeginProcess(Transform parent, string message, string resourcePath = "UI/Processing Popup Standard", float? progress = null, Action onCancel = null, string subMessage = null)
	{
		if (++_ActiveProcesses > 0)
		{
			_ActiveProcessPopup = _ActiveProcessPopup ?? UnityEngine.Object.Instantiate(Resources.Load<GameObject>(resourcePath), parent.GetComponentInParent<Canvas>().transform);
			if (!message.IsNullOrEmpty())
			{
				UpdateProcessMessage(message);
			}
			if (!subMessage.IsNullOrEmpty())
			{
				UpdateProcessSubMessage(subMessage);
			}
			if (progress.HasValue)
			{
				UpdateProcessProgress(progress.Value);
			}
			if (onCancel != null)
			{
				UpdateProcessOnCancel(onCancel);
			}
			_ActiveProcessPopup.transform.SetAsLastSibling();
			GameObject activeProcessPopup = _ActiveProcessPopup;
			InputManager.SetEventSystemEnabled(activeProcessPopup, enabled: false);
			_ActiveProcessPopup.GetOrAddComponent<DestroyHook>().OnDestroyed.AddListener(delegate
			{
				InputManager.SetEventSystemEnabled(activeProcessPopup, enabled: true);
			});
		}
	}

	public static void UpdateProcessMessage(string message)
	{
		if ((bool)_ActiveProcessPopup && _ActiveProcessPopup.activeInHierarchy)
		{
			_ActiveProcessPopup.GetComponent<ProcessPopupView>().SetText(message);
		}
	}

	public static void UpdateProcessSubMessage(string message)
	{
		if ((bool)_ActiveProcessPopup && _ActiveProcessPopup.activeInHierarchy)
		{
			_ActiveProcessPopup.GetComponent<ProcessPopupView>().SetSubText(message);
		}
	}

	public static void UpdateProcessProgress(float progressRatio)
	{
		if ((bool)_ActiveProcessPopup && _ActiveProcessPopup.activeInHierarchy)
		{
			_ActiveProcessPopup.GetComponent<ProcessPopupView>().SetProgress(Mathf.Clamp01(progressRatio));
		}
	}

	public static void UpdateProcessOnCancel(Action onCancel)
	{
		if ((bool)_ActiveProcessPopup && _ActiveProcessPopup.activeInHierarchy)
		{
			_ActiveProcessPopup.GetComponent<ProcessPopupView>().RegisterCancelRequest(onCancel);
		}
		else
		{
			ProcessPopupView.OnCancelRequested += onCancel;
		}
	}

	public static void EndProcess()
	{
		if (--_ActiveProcesses <= 0 && (bool)_ActiveProcessPopup)
		{
			_ActiveProcessPopup.Destroy();
			_ActiveProcessPopup = null;
		}
	}

	public static string AutoLocalize(string s, bool returnNullIfMissing = false)
	{
		return UITable.AutoLocalize(s, returnNullIfMissing);
	}

	public static void CreateUIControlsFromObject(Transform rootParent, object obj, Action responseToValueChanged = null, Action onValidate = null, int categoryIndentPixels = 24, TreeNode<ReflectTreeData<UIFieldAttribute>> overrideNode = null, string activeCategory = null, Action<string> onActiveCategoryChanged = null, string[] categoryIncludeFilter = null, string[] categoryExcludeFilter = null, int? activeCategoryIndex = null)
	{
		if (overrideNode == null)
		{
			LocalizeAttribute attribute = obj.GetType().GetAttribute<LocalizeAttribute>();
			UITable = ((attribute != null && attribute.reflectedUI) ? LocalizationSettings.StringDatabase.GetTable("UI") : null);
		}
		TreeNode<ReflectTreeData<UIFieldAttribute>> activeCollectionNode = ActiveCollectionNode;
		if (obj is IListUIControl.IIListAddWrapper)
		{
			ActiveCollectionNode = (obj as IListUIControl.IIListAddWrapper).collectionNode;
		}
		IListUIControl listUIControl2 = ((overrideNode != null) ? IListUIControl.GetControlFromCollectionNode(overrideNode) : null);
		if (responseToValueChanged == null)
		{
			responseToValueChanged = delegate
			{
			};
		}
		if (onValidate == null)
		{
			onValidate = delegate
			{
			};
		}
		Action action = delegate
		{
			responseToValueChanged();
			onValidate();
		};
		obj.InvokeMethod("OnValidateUI", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		string validateMethod = "OnValidateUI";
		TreeNode<ReflectTreeData<UIFieldAttribute>> treeNode = overrideNode ?? ReflectionUtil.GetMembersWithAttribute(obj, (UIFieldAttribute ui) => ui.order, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, fallbackBindings: BindingFlags.Instance | BindingFlags.Public, fallbackAttributeLogic: UIFieldAttribute.CreateFromMemberInfo, initializeAttribute: UIFieldAttribute.InitializeFromMemberInfo, getCollectionItemAttribute: UIFieldAttribute.GetCollectionItemAttribute, validateMethod: validateMethod);
		Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, Dictionary<string, Transform>> dictionary = new Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, Dictionary<string, Transform>>();
		Dictionary<Transform, Transform> categoryRootObjects = new Dictionary<Transform, Transform>();
		Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, IUIContentContainer> dictionary2 = new Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, IUIContentContainer>();
		Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, Dictionary<string, Transform>> dictionary3 = new Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, Dictionary<string, Transform>>();
		if ((bool)listUIControl2)
		{
			dictionary2.Add(listUIControl2.collectionNode, listUIControl2);
		}
		Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, GameObject> dictionary4 = new Dictionary<TreeNode<ReflectTreeData<UIFieldAttribute>>, GameObject>();
		List<TreeNode<ReflectTreeData<UIFieldAttribute>>> list = new List<TreeNode<ReflectTreeData<UIFieldAttribute>>>();
		int num = 0;
		List<TreeNode<ReflectTreeData<UIFieldAttribute>>> list2 = treeNode.children.Where((TreeNode<ReflectTreeData<UIFieldAttribute>> n) => !n.ShouldHide()).ToList();
		List<string> list3 = ((overrideNode == null) ? (from a in list2.SelectValid((TreeNode<ReflectTreeData<UIFieldAttribute>> n) => n.value.memberInfo.GetAttribute<UICategoryAttribute>())
			select a.category).Distinct().ToList() : new List<string>());
		if (list3.Count > 0 && list2.Any((TreeNode<ReflectTreeData<UIFieldAttribute>> n) => n.value.memberInfo.GetAttribute<UICategoryAttribute>() == null))
		{
			list3.AddUnique("Misc");
		}
		if (list3.Count == 1)
		{
			list3.Clear();
		}
		if (!categoryIncludeFilter.IsNullOrEmpty() && categoryIncludeFilter.Any(list3.Contains))
		{
			list3.RemoveAll((string s) => !categoryIncludeFilter.Contains(s));
		}
		if (!categoryExcludeFilter.IsNullOrEmpty())
		{
			list3.RemoveAll(categoryExcludeFilter.Contains<string>);
		}
		if (list3.Count > 1)
		{
			CategorySortType categorySortType = obj.GetType().GetAttribute<UICategorySortAttribute>()?.sorting ?? CategorySortType.Appearance;
			switch (categorySortType)
			{
			case CategorySortType.Alphabetical:
				list3.Sort();
				break;
			default:
			{
				Dictionary<string, int> categoryCounts = (from n in list2
					group n by n.GetCategoryTab()).ToDictionary((IGrouping<string, TreeNode<ReflectTreeData<UIFieldAttribute>>> g) => g.Key, (IGrouping<string, TreeNode<ReflectTreeData<UIFieldAttribute>>> g) => g.Count());
				int multiplier = ((categorySortType == CategorySortType.AscendingNumberInCategory) ? 1 : (-1));
				list3.Sort((string a, string b) => multiplier * (categoryCounts[a] - categoryCounts[b]));
				break;
			}
			case CategorySortType.Appearance:
				break;
			}
			if (activeCategory.IsNullOrEmpty() || !list3.Contains(activeCategory))
			{
				activeCategory = ((activeCategoryIndex < list3.Count) ? list3[activeCategoryIndex.Value] : list3.First());
			}
			GameObject gameObject = GetGameObject("UI/CategoryTab Container", rootParent);
			rootParent = GetGameObject("UI/Category Data Container", rootParent).transform;
			foreach (string item in list3)
			{
				string c = item;
				GameObject gameObject2 = CreateMethodButton(AutoLocalize(item), delegate
				{
					onActiveCategoryChanged?.Invoke(c);
					onValidate();
				}, gameObject.transform, "UI/Button CategoryTab");
				if (c == activeCategory)
				{
					gameObject2.GetComponentInChildren<LayoutElement>().preferredHeight += 12f;
					gameObject2.GetComponentInChildren<SpriteToggleSwapper>().isOn = true;
					gameObject2.AddComponent<SelectedCategoryTag>();
				}
			}
		}
		else if (list3.Count > 0)
		{
			if (activeCategory.IsNullOrEmpty() || !list3.Contains(activeCategory))
			{
				activeCategory = list3.First();
			}
		}
		else
		{
			activeCategory = null;
		}
		if (treeNode != null && treeNode.value.self is IListUIControl.IIListAddWrapper)
		{
			InitializeIListAddWrapper((treeNode.value.self as IListUIControl.IIListAddWrapper).collectionNode, treeNode);
		}
		foreach (TreeNode<ReflectTreeData<UIFieldAttribute>> item2 in treeNode.DepthFirstEnumNodes((TreeNode<ReflectTreeData<UIFieldAttribute>> node) => (node == overrideNode || node.parent == null || !_HasTypeView(node.parent.value)) && !node.ShouldHide() && node.IsInActiveCategory(activeCategory)))
		{
			if (!item2.value.CanReadAndWrite || ((bool)listUIControl2 && !(listUIControl2 = null)))
			{
				continue;
			}
			GameObject goToAdd = null;
			Component component = null;
			ReflectTreeData<UIFieldAttribute> rData = item2.value;
			TreeNode<ReflectTreeData<UIFieldAttribute>> tNode = item2;
			Type underlyingType = rData.GetUnderlyingType();
			Type type2 = (underlyingType.IsGenericType ? underlyingType.GetGenericTypeDefinition() : null);
			Action<TreeNode<ReflectTreeData<UIFieldAttribute>>> ifExists = TypeSpecialInitializations.GetIfExists(underlyingType);
			if (TypeSpecialInitializations.Count > 0)
			{
				if (ifExists == null && type2 != null)
				{
					ifExists = TypeSpecialInitializations.GetIfExists(type2);
				}
				ifExists?.Invoke(tNode);
			}
			if (MemberSpecialInitializations.Count > 0)
			{
				Type underlyingType2 = tNode.parent.value.GetUnderlyingType();
				Type type3 = (underlyingType2.IsGenericType ? underlyingType2.GetGenericTypeDefinition() : null);
				Action<TreeNode<ReflectTreeData<UIFieldAttribute>>, TreeNode<ReflectTreeData<UIFieldAttribute>>> ifExists2 = MemberSpecialInitializations.GetIfExists(underlyingType2);
				if (ifExists2 == null && type3 != null)
				{
					ifExists2 = MemberSpecialInitializations.GetIfExists(type3);
				}
				ifExists2?.Invoke(tNode.parent, tNode);
			}
			Transform transform = ((tNode.parent != null && dictionary2.ContainsKey(tNode.parent)) ? dictionary2[tNode.parent].uiContentParent : rootParent);
			if (!rData.data.category.IsNullOrEmpty())
			{
				string[] array = rData.data.category.Split('|');
				string text = "";
				if (!dictionary.ContainsKey(tNode.parent))
				{
					dictionary.Add(tNode.parent, new Dictionary<string, Transform>());
				}
				Dictionary<string, Transform> dictionary5 = dictionary[tNode.parent];
				for (int i = 0; i < array.Length; i++)
				{
					text += array[i];
					if (!dictionary5.ContainsKey(text))
					{
						GameObject gameObject3 = CreateCollapse(array[i], null, transform);
						dictionary5.Add(text, gameObject3.GetComponentInChildren<CollapseFitter>().transform);
						categoryRootObjects.Add(dictionary5[text], gameObject3.transform);
						if (categoryIndentPixels > 0)
						{
							dictionary5[text].gameObject.AddComponent<IndentFitter>().indentAmountPixels = categoryIndentPixels;
						}
						if (rData.data.collapse == UICollapseType.Open)
						{
							gameObject3.GetComponentInChildren<CollapseFitter>().ForceOpen();
						}
					}
					transform = dictionary5[text];
					if (i < array.Length - 1)
					{
						text += "|";
					}
				}
			}
			if (rData.memberInfo != null)
			{
				UIMarginAttribute attribute2 = rData.memberInfo.GetAttribute<UIMarginAttribute>();
				if (attribute2 != null && (attribute2.shouldApplyIfFirstElement || num > 0))
				{
					CreateMargin(attribute2.amount, transform);
				}
				UIHeaderAttribute attribute3 = rData.memberInfo.GetAttribute<UIHeaderAttribute>();
				if (attribute3 != null && !tNode.parent.value.GetUnderlyingType().IsNullableStruct())
				{
					CreateHeader(AutoLocalize(attribute3.header), attribute3.tint, transform);
				}
			}
			UILayoutAttribute uILayoutAttribute = ((rData.memberInfo != null) ? rData.memberInfo.GetAttribute<UILayoutAttribute>(inherit: true, exactType: false) : null);
			if (uILayoutAttribute != null)
			{
				if (!dictionary3.ContainsKey(tNode.parent))
				{
					dictionary3.Add(tNode.parent, new Dictionary<string, Transform>());
				}
				Dictionary<string, Transform> dictionary6 = dictionary3[tNode.parent];
				Transform transform2;
				if (uILayoutAttribute.name == null)
				{
					transform2 = uILayoutAttribute.CreateLayoutObject(transform).transform;
				}
				else if (!dictionary6.ContainsKey(uILayoutAttribute.name))
				{
					Transform transform4 = (dictionary6[uILayoutAttribute.name] = uILayoutAttribute.CreateLayoutObject(transform).transform);
					transform2 = transform4;
				}
				else
				{
					transform2 = dictionary6[uILayoutAttribute.name];
				}
				transform = transform2;
			}
			UIFieldAttribute uiField = rData.data;
			bool flag = underlyingType.IsIntegral();
			bool flag2 = underlyingType.IsFloatingPoint();
			bool flag3 = flag || flag2;
			bool flag4 = underlyingType == typeof(bool);
			bool flag5 = underlyingType == typeof(string);
			bool flag6 = underlyingType == typeof(char);
			bool isEnum = underlyingType.IsEnum;
			bool isMethod = rData.isMethod;
			bool flag7 = underlyingType.IsGenericICollection();
			bool flag8 = !isMethod && underlyingType.IsUserClassOrStruct();
			string view = rData.data.view;
			object defaultValue = rData.data.defaultValue;
			Func<TreeNode<ReflectTreeData<UIFieldAttribute>>, Transform, Action, GameObject> func = _GetTypeViewFunc(underlyingType, type2, uiField.searchInterfaceViews);
			if (!uiField.excludedValuesMethod.IsNullOrEmpty())
			{
				MethodInfo excludeMethodInfo;
				TreeNode<ReflectTreeData<UIFieldAttribute>> excludeMethodTarget = tNode.GetNodeWithMethod(uiField.excludedValuesMethod, out excludeMethodInfo);
				if (excludeMethodInfo != null && excludeMethodInfo.ReturnType == typeof(bool) && excludeMethodInfo.GetParameters().Length == 1)
				{
					ReflectTreeData<UIFieldAttribute> reflectTreeData = rData;
					reflectTreeData.excludedValues = (Func<object, bool>)Delegate.Combine(reflectTreeData.excludedValues, (Func<object, bool>)((object v) => (bool)excludeMethodInfo.Invoke(excludeMethodTarget.value.self, new object[1] { v })));
				}
			}
			if (!isEnum && rData.excludedValues != null)
			{
				rData.dependentOn.Add(rData);
			}
			if (!uiField.onValueChangedMethod.IsNullOrEmpty())
			{
				tNode.Root.DepthFirstEnumNodes().Where(delegate(TreeNode<ReflectTreeData<UIFieldAttribute>> d)
				{
					if (d.value.data == null || d.value.data.onValueChangedMethod != uiField.onValueChangedMethod)
					{
						return false;
					}
					MethodInfo methodInfo2;
					TreeNode<ReflectTreeData<UIFieldAttribute>> nodeWithMethod = tNode.GetNodeWithMethod(uiField.onValueChangedMethod, out methodInfo2);
					if (nodeWithMethod == null)
					{
						return false;
					}
					MethodInfo methodInfo3;
					TreeNode<ReflectTreeData<UIFieldAttribute>> nodeWithMethod2 = d.GetNodeWithMethod(d.value.data.onValueChangedMethod, out methodInfo3);
					return nodeWithMethod == nodeWithMethod2;
				}).EffectAll(delegate(TreeNode<ReflectTreeData<UIFieldAttribute>> d)
				{
					rData.AddDependencyLink(d.value);
				});
			}
			if ((bool)UITable)
			{
				string label = uiField.label;
				uiField.label = AutoLocalize(uiField.label);
				if (uiField.tooltip.HasVisibleCharacter())
				{
					uiField.tooltip = AutoLocalize(label + "-tooltip", returnNullIfMissing: true) ?? uiField.tooltip;
				}
			}
			if (func != null)
			{
				goToAdd = func(tNode, transform, onValidate);
				if (uiField.collapse == UICollapseType.Open && uiField.category.IsNullOrEmpty() && (bool)goToAdd && (bool)goToAdd.GetComponentInChildren<CollapseFitter>())
				{
					goToAdd.GetComponentInChildren<CollapseFitter>().ForceOpen();
				}
			}
			else if (flag3)
			{
				UnityAction<double> onValueChanged = delegate(double value)
				{
					rData.SetValue(tNode, value);
				};
				float minValue = underlyingType.GetMinValue(0f - MathUtil.LargeNumber);
				float maxValue = underlyingType.GetMaxValue(MathUtil.LargeNumber);
				double num2 = Convert.ToDouble(rData.GetValue());
				if (uiField.min != uiField.max)
				{
					minValue = float.Parse(uiField.min.ToString());
					maxValue = float.Parse(uiField.max.ToString());
				}
				else
				{
					rData.SetValue(tNode, minValue);
					minValue = Mathf.Max(minValue, Convert.ToSingle(rData.GetValue()));
					rData.SetValue(tNode, maxValue);
					maxValue = Mathf.Min(maxValue, Convert.ToSingle(rData.GetValue()));
					rData.SetValue(tNode, num2);
				}
				if (Math.Abs(minValue - maxValue) < float.Epsilon)
				{
					continue;
				}
				goToAdd = CreateSlider(uiField.label, onValueChanged, num2, minValue, maxValue, flag, (uiField.stepSize != null) ? float.Parse(uiField.stepSize.ToString()) : _GetStepSize(maxValue - minValue), view, defaultValue, transform, (underlyingType == typeof(double)) ? 16 : 7);
				component = goToAdd.GetComponentInChildren<Slider>();
			}
			else if (flag4)
			{
				UnityAction<bool> onValueChanged2 = delegate(bool value)
				{
					rData.SetValue(tNode, value);
				};
				if (rData.isProperty || rData.excludedValues != null)
				{
					bool flag9 = Convert.ToBoolean(rData.GetValue());
					rData.SetValue(tNode, !flag9);
					if (flag9 == Convert.ToBoolean(rData.GetValue()))
					{
						continue;
					}
					rData.SetValue(tNode, flag9);
				}
				goToAdd = CreateToggle(uiField.label, Convert.ToBoolean(rData.GetValue()), onValueChanged2, view, transform);
				component = goToAdd.GetComponentInChildren<Toggle>();
			}
			else if (isEnum)
			{
				bool flag10 = uiField.min != null || uiField.max != null;
				UIEnumFilterFlags value2 = (uiField.filter as UIEnumFilterFlags?) ?? EnumUtil<UIEnumFilterFlags>.NoFlags;
				int min = int.MinValue;
				int max = int.MaxValue;
				UnityAction<object> onValueChanged3 = delegate(object value)
				{
					rData.SetValue(tNode, value);
				};
				bool flag11 = underlyingType.GetAttribute<FlagsAttribute>() != null;
				if (flag10)
				{
					min = ((uiField.min != null) ? ((int)Convert.ChangeType(Enum.Parse(underlyingType, uiField.min.ToString()), typeof(int))) : int.MinValue);
					max = ((uiField.max != null) ? ((int)Convert.ChangeType(Enum.Parse(underlyingType, uiField.max.ToString()), typeof(int))) : int.MaxValue);
					if (flag11)
					{
						onValueChanged3 = delegate(object value)
						{
							int num4 = (int)Convert.ChangeType(value, typeof(int));
							if ((num4 < min && num4 != -1) || num4 > max)
							{
								onValidate();
							}
							else
							{
								rData.SetValue(tNode, value);
							}
						};
					}
				}
				object obj3 = rData.GetValue();
				bool flag12 = rData.isProperty && rData.memberInfo.CanWrite();
				bool num3 = flag12 || flag10;
				HashSet<string> hashSet = null;
				if (num3 && !EnumUtil.HasFlag(value2, UIEnumFilterFlags.SkipIncludeAndExcludeLogic))
				{
					HashSet<string> hashSet2 = new HashSet<string>();
					hashSet = new HashSet<string>();
					if (flag12)
					{
						hashSet2.AddMany(ReflectionUtil.GetIncludedEnumMembers(tNode, rData));
						hashSet.AddMany(ReflectionUtil.GetExcludedEnumMembers(tNode, rData));
					}
					if (flag10)
					{
						hashSet2.IntersectNonEmpty(ReflectionUtil.GetEnumMembers(underlyingType, included: true, min, max));
						hashSet.UnionWith(ReflectionUtil.GetEnumMembers(underlyingType, included: false, min, max));
					}
					if (hashSet2.Count <= 1)
					{
						continue;
					}
					if (!flag11 && !hashSet2.Contains(obj3.ToString()))
					{
						obj3 = Enum.Parse(underlyingType, hashSet2.First());
					}
				}
				else if (rData.excludedValues != null)
				{
					hashSet = ReflectionUtil.GetExcludedEnumMembers(tNode, rData);
				}
				if (hashSet != null && overrideNode == null && Enum.GetValues(underlyingType).Length - hashSet.Count <= 1 && tNode.parent.value.originalCollection == null && tNode.parent.value.GetUnderlyingType().GetGenericTypeDefinitionSafe() != typeof(IListUIControl.IListAddWrapper<>))
				{
					continue;
				}
				goToAdd = CreateEnumComboBox(underlyingType.FullName, uiField.label, onValueChanged3, underlyingType.HasAttribute<FlagsAttribute>() ? BitMask.ToString(underlyingType, obj3) : obj3.ToString(), view, null, hashSet, 0, uiField.maxCount, transform, uiField.collapse != UICollapseType.Open);
				component = goToAdd.GetComponentInChildren<ComboBox>();
			}
			else if (flag5)
			{
				UnityAction<string> onValueChanged4 = delegate(string value)
				{
					rData.SetValue(tNode, value);
				};
				goToAdd = CreateInputField(uiField.label, onValueChanged4, (string)rData.GetValue(), (uiField.max != null) ? new int?(int.Parse(uiField.max.ToString())) : null, uiField.filter as InputField.ContentType?, view, transform, uiField.fixedSize);
				if (uiField.collapse != 0)
				{
					goToAdd.GetComponentInChildren<CollapseFitter>(includeInactive: true)?.ForceOpenSafe();
				}
				if (uiField.collapse == UICollapseType.Hide)
				{
					goToAdd.GetComponentInChildren<Button>(includeInactive: true)?.gameObject.Destroy();
				}
				component = goToAdd.GetComponentInChildren<InputField>();
			}
			else if (flag6)
			{
				UnityAction<string> onValueChanged5 = delegate(string value)
				{
					rData.SetValue(tNode, value.ToCharArray().FirstOrDefault());
				};
				char c2 = (char)rData.GetValue();
				string defaultText = c2.ToString();
				if (char.IsControl(c2))
				{
					defaultText = "";
				}
				goToAdd = CreateInputField(uiField.label, onValueChanged5, defaultText, 1, uiField.filter as InputField.ContentType?, view, transform);
				component = goToAdd.GetComponentInChildren<InputField>();
			}
			else if (flag7)
			{
				object value3 = rData.GetValue();
				Type type4 = ((!underlyingType.IsArray) ? value3.GetType().GetGenericArguments()[0] : underlyingType.GetElementType());
				ActiveCollectionNode = tNode.GetNodeWithMemberInfo();
				object originalCollection = ActiveCollectionNode.value.originalCollection;
				Func<object, bool> excludedValues = ((tNode.children.Count <= 0) ? null : (underlyingType.IsGenericIDictionary() ? ((Func<object, bool>)((object v) => tNode.children[0].children[0].value.excludedValues(v.InvokeMethod<object>("Key", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, checkProperties: true, Array.Empty<object>())))) : tNode.children[0].value.excludedValues));
				Func<object, object> onPrepareToAdd = null;
				if (underlyingType.IsGenericDictionary() && underlyingType.GetGenericArguments()[0] == typeof(string))
				{
					onPrepareToAdd = delegate(object addedPair)
					{
						addedPair.GetType().GetField("key", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(addedPair, ((IDictionary)originalCollection).GetUniqueKey(addedPair.InvokeMethod<string>("Key", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, checkProperties: true, Array.Empty<object>()).Trim().ReplaceNullOrEmpty()));
						return addedPair;
					};
				}
				else if (originalCollection.GetType() == typeof(HashSet<string>))
				{
					onPrepareToAdd = (object addedString) => (addedString as string).Trim().ReplaceNullOrEmpty().GetUniqueKey((object s) => (originalCollection as HashSet<string>).Contains(s as string));
				}
				uiField.validateOnChange = true;
				if (type4.IsAbstract && !rData.data.showAddData && type4.GetInheritanceHierarchyClasses().AtLeast(2))
				{
					rData.data.showAddData = true;
				}
				goToAdd = CreateIList(value3 as IList, type4, uiField.label, uiField.maxCount, view, delegate
				{
					rData.SignalCountChanged();
				}, (rData.data.showAddData || underlyingType.IsCollectionThatShouldShowAddData()) && !type4.IsGenericICollection(), 0, uiField.fixedSize || uiField.readOnly, excludedValues, onPrepareToAdd, transform, uiField.filter as int?);
				ActiveCollectionNode = null;
				IListUIControl listUIControl = goToAdd.GetComponentInChildren<IListUIControl>(includeInactive: true);
				dictionary2.Add(tNode, listUIControl);
				component = listUIControl;
				if (uiField.collapse == UICollapseType.Open)
				{
					goToAdd.GetComponentInChildren<CollapseFitter>().ForceOpen();
				}
				if ((bool)listUIControl.searcher)
				{
					listUIControl.searcher.onIndexMapChange.AddListener(delegate
					{
						listUIControl.RefreshForSearch();
					});
				}
			}
			else if (flag8)
			{
				if (uiField.label.FirstOrDefault() == '>')
				{
					UICollectionLabelAttribute attribute4 = underlyingType.GetAttribute<UICollectionLabelAttribute>();
					if (attribute4 != null)
					{
						uiField.label = attribute4.GetLabel(rData.GetValue());
					}
				}
				if (uiField.collapse != UICollapseType.Hide)
				{
					bool flag13 = tNode.parent.value.originalCollection != null;
					goToAdd = CreateCollapse(uiField.label, view, transform, flag13 ? MirzaFont : null, flag13 ? new float?(34f) : null, flag13 ? new FontStyles?(EnumUtil<FontStyles>.NoFlags) : null);
					dictionary2.Add(tNode, goToAdd.GetComponentInChildren<CollapseFitter>());
					component = goToAdd.GetComponentInChildren<CollapseFitter>();
					if (categoryIndentPixels > 0 && tNode.parent.value.GetUnderlyingType().ShouldIndentUIOfChildren())
					{
						component.gameObject.AddComponent<IndentFitter>().indentAmountPixels = categoryIndentPixels;
					}
					if (uiField.collapse == UICollapseType.Open)
					{
						(component as CollapseFitter).ForceOpen();
					}
					if (flag13)
					{
						PendingActionSignaler pendingActionSignaler = goToAdd.AddComponent<PendingActionSignaler>();
						string indexPrefix = goToAdd.GetUILabel().GetListIndexString() + ":" + StringUtil.COLLAPSE_SEPARATOR;
						Action updateLabel = delegate
						{
							goToAdd.SetUILabel(indexPrefix + rData.GetValue());
						};
						tNode.AddOnValueChangeListener(delegate
						{
							pendingActionSignaler.action = updateLabel;
						});
					}
				}
				else
				{
					goToAdd = new GameObject("Hidden Collapse");
					goToAdd.transform.SetParent(transform, worldPositionStays: false);
					component = goToAdd.AddComponent<UIContentContainer>();
					dictionary2.Add(tNode, component as UIContentContainer);
				}
				List<Type> list4 = (from t in underlyingType.GetAssignableClasses()
					where t.IsConcrete()
					orderby t.GetUIOrder()
					select t).ToList();
				if (rData.GetValue() == null)
				{
					object obj4 = Activator.CreateInstance(list4.FirstOrDefault() ?? underlyingType, nonPublic: true);
					rData.SetValue(tNode, obj4);
					ifExists?.Invoke(tNode);
					foreach (TreeNode<ReflectTreeData<UIFieldAttribute>> child in ReflectionUtil.GetMembersWithAttribute(obj4, (UIFieldAttribute ui) => ui.order, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, fallbackBindings: BindingFlags.Instance | BindingFlags.Public, fallbackAttributeLogic: UIFieldAttribute.CreateFromMemberInfo, initializeAttribute: UIFieldAttribute.InitializeFromMemberInfo, getCollectionItemAttribute: UIFieldAttribute.GetCollectionItemAttribute, validateMethod: validateMethod).children)
					{
						tNode.AddChild(child);
					}
				}
				if (underlyingType.IsReferenceType() && list4.Atleast(2u) && underlyingType.ShouldShowTypeComboBox())
				{
					rData.data.validateOnChange = true;
					Func<Type, bool> excludeType = null;
					if (!uiField.filterMethod.IsNullOrEmpty())
					{
						MethodInfo filterMethodInfo;
						TreeNode<ReflectTreeData<UIFieldAttribute>> filterMethodTarget = tNode.GetNodeWithMethod(uiField.filterMethod, out filterMethodInfo);
						if (filterMethodInfo == null && ActiveCollectionNode != null)
						{
							filterMethodTarget = ActiveCollectionNode.GetNodeWithMethod(uiField.filterMethod, out filterMethodInfo);
						}
						if (filterMethodInfo != null && filterMethodInfo.ReturnType == typeof(bool) && filterMethodInfo.GetParameters().Length == 1 && filterMethodInfo.GetParameters()[0].ParameterType == typeof(Type))
						{
							excludeType = delegate(Type type1)
							{
								MethodInfo methodInfo = filterMethodInfo;
								object self = filterMethodTarget.value.self;
								object[] parameters = new Type[1] { type1 };
								return (bool)methodInfo.Invoke(self, parameters);
							};
						}
					}
					GameObject gameObject4 = CreateTypeComboBox(underlyingType.FullName, (obj is IListUIControl.IIListAddWrapper) ? "Select Type To Add" : "Select Type", delegate(Type t)
					{
						object value4 = rData.GetValue();
						if (value4 == null || value4.GetType() != t)
						{
							rData.SetValue(tNode, Activator.CreateInstance(t, nonPublic: true));
						}
					}, derivedTypesOnly: false, rData.GetValue().GetType().FullName, null, 0, excludeType, (obj is IListUIControl.IIListAddWrapper) ? transform : component.transform);
					if (obj is IListUIControl.IIListAddWrapper)
					{
						tNode.children.Clear();
						if ((bool)goToAdd)
						{
							goToAdd.Destroy();
						}
						goToAdd = gameObject4.gameObject;
						component = gameObject4.GetComponentInChildren<ComboBox>();
					}
				}
			}
			else if (isMethod)
			{
				goToAdd = CreateMethodButton(uiField.label, delegate
				{
					InvokingMethodNode = tNode;
					rData.Invoke();
					InvokingMethodNode = null;
				}, transform, view, uiField.validateOnChange ? action : null);
				component = goToAdd.GetComponentInChildren<Button>();
			}
			if (goToAdd != null)
			{
				num++;
				dictionary4[tNode] = goToAdd;
				if (TypePostProcesses.ContainsKey(underlyingType) || (type2 != null && TypePostProcesses.ContainsKey(type2)))
				{
					list.Add(tNode);
				}
				if (uiField.readOnly && !(component is CollapseFitter))
				{
					goToAdd.AddComponent<ReadonlyToggle>();
				}
				_HookDependencies(goToAdd, tNode, component, responseToValueChanged, onValidate);
				uILayoutAttribute?.SetLayoutElementData(goToAdd);
				goToAdd.SetUITooltip(uiField.tooltip);
			}
		}
		foreach (TreeNode<ReflectTreeData<UIFieldAttribute>> item3 in list)
		{
			Type underlyingType3 = item3.value.GetUnderlyingType();
			TypePostProcesses[TypePostProcesses.ContainsKey(underlyingType3) ? underlyingType3 : underlyingType3.GetGenericTypeDefinition()](item3, dictionary4);
		}
		foreach (TreeNode<ReflectTreeData<UIFieldAttribute>> key in dictionary.Keys)
		{
			UICategorySortAttribute attribute5 = key.value.GetUnderlyingType().GetAttribute<UICategorySortAttribute>();
			if (attribute5 != null && attribute5.sorting != 0)
			{
				List<GameObject> list5 = dictionary[key].Values.Select((Transform t) => t.gameObject).ToList();
				SortCategoryControls(list5, attribute5.sorting);
				list5.EffectAll(delegate(GameObject go)
				{
					categoryRootObjects[go.transform].SetAsLastSibling();
				});
			}
		}
		ActiveCollectionNode = activeCollectionNode;
		if (overrideNode == null)
		{
			UITable = null;
		}
	}

	private static void _HookDependencies(GameObject goToAdd, TreeNode<ReflectTreeData<UIFieldAttribute>> reflectNode, Component control, Action responseToValueChange, Action onValidate)
	{
		ReflectTreeData<UIFieldAttribute> r = reflectNode.value;
		Action action = null;
		if (control != null)
		{
			if (control is Slider)
			{
				action = delegate
				{
					(control as Slider).value = Convert.ToSingle(r.GetValue());
				};
			}
			else if (control is Toggle)
			{
				action = delegate
				{
					(control as Toggle).isOn = Convert.ToBoolean(r.GetValue());
				};
			}
			else if (control is ComboBox)
			{
				action = delegate
				{
					(control as ComboBox).SelectText(Convert.ToString(r.GetValue()), toggleList: false);
				};
			}
			else if (control is InputField)
			{
				action = delegate
				{
					(control as InputField).text = Convert.ToString(r.GetValue());
				};
			}
		}
		if (action != null)
		{
			r.hook = action;
			if (r.data.dependentOn != null)
			{
				foreach (string dependOn in r.data.dependentOn)
				{
					TreeNode<ReflectTreeData<UIFieldAttribute>> sibling = reflectNode.GetSibling((TreeNode<ReflectTreeData<UIFieldAttribute>> node) => string.Compare(node.value.data.label, dependOn, StringComparison.OrdinalIgnoreCase) == 0);
					if (sibling != null && r.hookedDependencies.Add(sibling.value))
					{
						ReflectTreeData<UIFieldAttribute> value = sibling.value;
						value.OnSetValue = (Action)Delegate.Combine(value.OnSetValue, r.hook);
					}
				}
			}
			foreach (ReflectTreeData<UIFieldAttribute> item in r.dependentOn)
			{
				if (r.hookedDependencies.Add(item))
				{
					item.OnSetValue = (Action)Delegate.Combine(item.OnSetValue, r.hook);
				}
				if (item.hook != null && item.hookedDependencies.Add(r))
				{
					ReflectTreeData<UIFieldAttribute> reflectTreeData = r;
					reflectTreeData.OnSetValue = (Action)Delegate.Combine(reflectTreeData.OnSetValue, item.hook);
				}
			}
		}
		if (!r.data.onValueChangedMethod.IsNullOrEmpty())
		{
			MethodInfo methodInfo;
			TreeNode<ReflectTreeData<UIFieldAttribute>> nodeWithMethod = reflectNode.GetNodeWithMethod(r.data.onValueChangedMethod, out methodInfo);
			if (nodeWithMethod != null)
			{
				object methodObj = nodeWithMethod.value.self;
				ReflectTreeData<UIFieldAttribute> reflectTreeData2 = r;
				reflectTreeData2.OnValueChanged = (Action)Delegate.Combine(reflectTreeData2.OnValueChanged, (Action)delegate
				{
					methodInfo.Invoke(methodObj, new object[0]);
				});
			}
		}
		UIDeepValueChange uIDeepValueChange = ((r.memberInfo != null) ? r.memberInfo.GetAttribute<UIDeepValueChange>() : null);
		if (uIDeepValueChange != null)
		{
			if (uIDeepValueChange.updateLabel)
			{
				Action updateLabel = delegate
				{
					goToAdd.SetUILabel(r.data.label + ":" + StringUtil.COLLAPSE_SEPARATOR + _GetUIDeepValueChangeLabel(r.GetValue()).Replace('\n', ' '));
				};
				ReflectTreeData<UIFieldAttribute> reflectTreeData3 = r;
				reflectTreeData3.OnValueChanged = (Action)Delegate.Combine(reflectTreeData3.OnValueChanged, (Action)delegate
				{
					Job.Process(Job.WaitForOneFrame(), Department.UI).Immediately().Do(updateLabel);
				});
				updateLabel();
			}
			reflectNode.AddOnValueChangeListener(r.OnValueChanged);
		}
		if (responseToValueChange != null)
		{
			ReflectTreeData<UIFieldAttribute> reflectTreeData4 = r;
			reflectTreeData4.OnValueChanged = (Action)Delegate.Combine(reflectTreeData4.OnValueChanged, responseToValueChange);
		}
		if (onValidate != null && r.data.validateOnChange)
		{
			ReflectTreeData<UIFieldAttribute> reflectTreeData5 = r;
			reflectTreeData5.OnValueChanged = (Action)Delegate.Combine(reflectTreeData5.OnValueChanged, onValidate);
		}
	}

	private static string _GetUIDeepValueChangeLabel(object obj)
	{
		if (obj == null)
		{
			return "";
		}
		if (obj is ICollection)
		{
			return ((IEnumerable)obj).ToStringSmart();
		}
		return obj.ToString() ?? "";
	}

	public static GameObject GetGameObject(string resourcePath, Transform parent = null)
	{
		return UnityEngine.Object.Instantiate(GetGameObjectBlueprint(resourcePath), parent);
	}

	public static GameObject GetGameObjectBlueprint(string resourcePath)
	{
		if (!_Blueprints.ContainsKey(resourcePath) || !_Blueprints[resourcePath])
		{
			_Blueprints[resourcePath] = Resources.Load<GameObject>(resourcePath);
		}
		return _Blueprints[resourcePath];
	}

	public static void SortCategoryControls(List<GameObject> categoryObjectList, CategorySortType categorySorting)
	{
		switch (categorySorting)
		{
		case CategorySortType.DescendingNumberInCategory:
			categoryObjectList.Sort((GameObject a, GameObject b) => b.transform.childCount - a.transform.childCount);
			break;
		case CategorySortType.AscendingNumberInCategory:
			categoryObjectList.Sort((GameObject a, GameObject b) => a.transform.childCount - b.transform.childCount);
			break;
		case CategorySortType.Alphabetical:
			categoryObjectList.Sort((GameObject a, GameObject b) => string.Compare(a.transform.parent.gameObject.GetUILabel(), b.transform.parent.gameObject.GetUILabel(), StringComparison.OrdinalIgnoreCase));
			break;
		case CategorySortType.Appearance:
			break;
		}
	}

	private static float _GetStepSize(float range)
	{
		if (range >= 100f)
		{
			return 1f;
		}
		if (range >= 50f)
		{
			return 0.5f;
		}
		if (range >= 10f)
		{
			return 0.1f;
		}
		return 0.01f;
	}

	public static StringTable GetUITable()
	{
		return LocalizationSettings.StringDatabase.GetTable("UI");
	}

	public static Button AddButtonTo(GameObject go)
	{
		Button button = go.GetComponent<Button>();
		if (button == null)
		{
			button = go.AddComponent<Button>();
		}
		if (button.targetGraphic == null)
		{
			Graphic graphic = go.GetComponent<Graphic>();
			if (graphic == null)
			{
				graphic = go.AddComponent<Image>();
				graphic.color = new Color(0f, 0f, 0f, 0f);
			}
			button.targetGraphic = graphic;
		}
		return button;
	}

	public static Button AddButtonAndLinkTo<T>(GameObject go, Color? highlightColor = null, Color? pressedColor = null, Color? disabledColor = null) where T : Graphic
	{
		Button button = go.GetComponent<Button>();
		if (button == null)
		{
			button = go.AddComponent<Button>();
		}
		button.targetGraphic = go.GetComponentInChildren<T>();
		button.SetStateColors(highlightColor, pressedColor, null, disabledColor);
		return button;
	}

	public static EventTrigger AddEventHandler<T>(GameObject go, EventTriggerType type, UnityAction<T> handler) where T : BaseEventData
	{
		EventTrigger eventTrigger = go.GetComponent<EventTrigger>();
		if (eventTrigger == null)
		{
			eventTrigger = go.AddComponent<EventTrigger>();
			eventTrigger.triggers = new List<EventTrigger.Entry>();
		}
		EventTrigger.Entry entry = eventTrigger.triggers.Where((EventTrigger.Entry e) => e.eventID == type).FirstOrDefault();
		if (entry == null)
		{
			entry = new EventTrigger.Entry();
			entry.eventID = type;
			eventTrigger.triggers.Add(entry);
		}
		UnityAction<BaseEventData> call = delegate(BaseEventData baseEvent)
		{
			handler(baseEvent as T);
		};
		entry.callback.AddListener(call);
		return eventTrigger;
	}

	public static GameObject DragClone(GameObject draggedObject, GameObject visual = null, Func<GameObject, bool> findVisual = null, PointerEventData pointerEventData = null, float visualAlphaWhileDragging = 1f, float draggedVisualAlpha = 1f, float visualPreferredWidthMultiplier = 1f, float visualPreferredHeightMultiplier = 1f, bool centerDragOnDraggedObject = true, bool matchRotationToDragOverRect = false, bool destroyEventTriggerOnEndDrag = true)
	{
		GameObject gameObject = visual ?? ((findVisual == null) ? draggedObject : draggedObject.FindChild(findVisual, includeInactive: true, countsAsChild: true));
		if (gameObject == null)
		{
			return null;
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation);
		gameObject2.transform.SetWorldScale(gameObject.transform.GetWorldScale());
		gameObject2.AddComponent<DragClone>().Init(draggedObject, gameObject, pointerEventData, visualAlphaWhileDragging, draggedVisualAlpha, new Vector2(visualPreferredWidthMultiplier, visualPreferredHeightMultiplier), centerDragOnDraggedObject, matchRotationToDragOverRect, destroyEventTriggerOnEndDrag);
		return gameObject2;
	}

	public static void SetStateColors(this Selectable s, Color? highlight = null, Color? pressed = null, Color? normal = null, Color? disabled = null, float? multiplier = null, float? fadeDuration = null)
	{
		ColorBlock colors = default(ColorBlock);
		colors.normalColor = (normal.HasValue ? normal.Value : s.colors.normalColor);
		colors.highlightedColor = (highlight.HasValue ? highlight.Value : s.colors.highlightedColor);
		colors.pressedColor = (pressed.HasValue ? pressed.Value : s.colors.pressedColor);
		colors.disabledColor = (disabled.HasValue ? disabled.Value : s.colors.disabledColor);
		colors.colorMultiplier = (multiplier.HasValue ? multiplier.Value : s.colors.colorMultiplier);
		colors.fadeDuration = (fadeDuration.HasValue ? fadeDuration.Value : s.colors.fadeDuration);
		s.colors = colors;
	}

	public static bool IsNumeric(this InputField inputField)
	{
		if (inputField.contentType != InputField.ContentType.IntegerNumber)
		{
			return inputField.contentType == InputField.ContentType.DecimalNumber;
		}
		return true;
	}

	public static bool IsNumeric(this TMP_InputField inputField)
	{
		if (inputField.contentType != TMP_InputField.ContentType.IntegerNumber)
		{
			return inputField.contentType == TMP_InputField.ContentType.DecimalNumber;
		}
		return true;
	}

	public static bool CanScrollVertical(this ScrollRect scrollRect)
	{
		return scrollRect.content.rect.size.y > scrollRect.viewport.rect.size.y + 0.01f;
	}

	public static bool CanScrollHorizontal(this ScrollRect scrollRect)
	{
		return scrollRect.content.rect.size.x > scrollRect.viewport.rect.size.x + 0.01f;
	}

	public static bool TrySetSprite(this Graphic graphic, Sprite sprite)
	{
		if (graphic is Image)
		{
			((Image)graphic).sprite = sprite;
			return true;
		}
		if (graphic is UIGlow)
		{
			((UIGlow)graphic).sprite = sprite;
			return true;
		}
		return false;
	}

	public static void CopyFrom(this Canvas c, Canvas copyFrom)
	{
		c.gameObject.layer = copyFrom.gameObject.layer;
		c.renderMode = copyFrom.renderMode;
		c.worldCamera = copyFrom.worldCamera;
		c.planeDistance = copyFrom.planeDistance;
		c.targetDisplay = copyFrom.targetDisplay;
	}

	public static string TruncateToFitWidth(this Font font, string input, int fontSize, float width, FontStyle style = FontStyle.Normal, string truncateAppend = "...")
	{
		char[] array = input.ToCharArray();
		char[] array2 = truncateAppend.ToCharArray();
		StringBuilder stringBuilder = new StringBuilder();
		CharacterInfo info = default(CharacterInfo);
		float num = 0f;
		for (int i = 0; i < array2.Length; i++)
		{
			if (font.GetCharacterInfo(array2[i], out info, fontSize, style))
			{
				num += (float)info.advance;
			}
		}
		foreach (char c in array)
		{
			if (font.GetCharacterInfo(c, out info, fontSize, style))
			{
				num += (float)info.advance;
				if (!(num <= width))
				{
					break;
				}
				stringBuilder.Append(c);
			}
		}
		stringBuilder.Append(truncateAppend);
		return stringBuilder.ToString();
	}

	public static void TruncateToFitWidth(this Text text, float width, string truncateAppend = "...")
	{
		text.text = text.font.TruncateToFitWidth(text.text, text.fontSize, width, text.fontStyle, truncateAppend);
	}

	public static bool ShouldShow(this IShowCanDrag showCanDrag)
	{
		return showCanDrag?.ShouldShowCanDrag() ?? false;
	}

	public static bool ShouldShow(this IIgnoreShowCanDrag ignoreShowCanDrag)
	{
		if (ignoreShowCanDrag != null)
		{
			return !ignoreShowCanDrag.ShouldIgnoreShowCanDrag();
		}
		return true;
	}

	public static LayoutData GetTotalLayoutDataOfChildren(this RectTransform rect, int axis)
	{
		LayoutData result = default(LayoutData);
		foreach (Transform item in rect)
		{
			RectTransform rectTransform = item as RectTransform;
			if (!(rectTransform == null))
			{
				result.min += LayoutUtility.GetMinSize(rectTransform, axis);
				result.preferred += LayoutUtility.GetPreferredSize(rectTransform, axis);
				result.flexible += LayoutUtility.GetFlexibleSize(rectTransform, axis);
			}
		}
		return result;
	}

	public static LayoutData GetTotalLayoutDataOfChildrenSpecial(this RectTransform rect, int axis, Func<RectTransform, float, float> minLogic = null, Func<RectTransform, float, float> preferredLogic = null, Func<RectTransform, float, float> flexibleLogic = null)
	{
		LayoutData result = default(LayoutData);
		if (minLogic == null)
		{
			minLogic = (RectTransform r, float v) => v;
		}
		if (preferredLogic == null)
		{
			preferredLogic = (RectTransform r, float v) => v;
		}
		if (flexibleLogic == null)
		{
			flexibleLogic = (RectTransform r, float v) => v;
		}
		foreach (Transform item in rect)
		{
			RectTransform rectTransform = item as RectTransform;
			if (!(rectTransform == null))
			{
				result.min += minLogic(rectTransform, LayoutUtility.GetMinSize(rectTransform, axis));
				result.preferred += preferredLogic(rectTransform, LayoutUtility.GetPreferredSize(rectTransform, axis));
				result.flexible += flexibleLogic(rectTransform, LayoutUtility.GetFlexibleSize(rectTransform, axis));
			}
		}
		return result;
	}

	public static LayoutData GetMaxLayoutDataOfChildren(this RectTransform rect, int axis)
	{
		LayoutData result = default(LayoutData);
		foreach (Transform item in rect)
		{
			RectTransform rectTransform = item as RectTransform;
			if (!(rectTransform == null))
			{
				result.min = Math.Max(result.min, LayoutUtility.GetMinSize(rectTransform, axis));
				result.preferred = Math.Max(result.preferred, LayoutUtility.GetPreferredSize(rectTransform, axis));
				result.flexible = Math.Max(result.flexible, LayoutUtility.GetFlexibleSize(rectTransform, axis));
			}
		}
		return result;
	}

	public static LayoutData GetMaxLayoutDataOfChildrenSpecial(this RectTransform rect, int axis, Func<RectTransform, float, float> minLogic = null, Func<RectTransform, float, float> preferredLogic = null, Func<RectTransform, float, float> flexibleLogic = null)
	{
		LayoutData result = default(LayoutData);
		if (minLogic == null)
		{
			minLogic = (RectTransform r, float v) => v;
		}
		if (preferredLogic == null)
		{
			preferredLogic = (RectTransform r, float v) => v;
		}
		if (flexibleLogic == null)
		{
			flexibleLogic = (RectTransform r, float v) => v;
		}
		foreach (Transform item in rect)
		{
			RectTransform rectTransform = item as RectTransform;
			if (!(rectTransform == null))
			{
				result.min = Math.Max(result.min, minLogic(rectTransform, LayoutUtility.GetMinSize(rectTransform, axis)));
				result.preferred = Math.Max(result.preferred, preferredLogic(rectTransform, LayoutUtility.GetPreferredSize(rectTransform, axis)));
				result.flexible = Math.Max(result.flexible, flexibleLogic(rectTransform, LayoutUtility.GetFlexibleSize(rectTransform, axis)));
			}
		}
		return result;
	}

	public static LayoutData GetTotalLayoutDataOfRects(List<RectTransform> rects, int axis)
	{
		LayoutData result = default(LayoutData);
		for (int i = 0; i < rects.Count; i++)
		{
			RectTransform rect = rects[i];
			result.min += LayoutUtility.GetMinSize(rect, axis);
			result.preferred += LayoutUtility.GetPreferredSize(rect, axis);
			result.flexible += LayoutUtility.GetFlexibleSize(rect, axis);
		}
		return result;
	}

	public static RectTransform.Edge OppositeEdge(this RectTransform.Edge edge)
	{
		return edge switch
		{
			RectTransform.Edge.Bottom => RectTransform.Edge.Top, 
			RectTransform.Edge.Top => RectTransform.Edge.Bottom, 
			RectTransform.Edge.Left => RectTransform.Edge.Right, 
			RectTransform.Edge.Right => RectTransform.Edge.Left, 
			_ => RectTransform.Edge.Left, 
		};
	}

	public static int Axis(this RectTransform.Edge edge)
	{
		if (edge == RectTransform.Edge.Left || edge == RectTransform.Edge.Right)
		{
			return 0;
		}
		return 1;
	}

	public static float Pivot(this RectTransform.Edge edge)
	{
		if (edge == RectTransform.Edge.Left || edge == RectTransform.Edge.Bottom)
		{
			return 0f;
		}
		return 1f;
	}
}
