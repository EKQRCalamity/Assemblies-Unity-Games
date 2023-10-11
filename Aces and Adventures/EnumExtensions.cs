using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public static class EnumExtensions
{
	private static readonly KeyCode[] _ShiftKeyCodes = new KeyCode[2]
	{
		KeyCode.LeftShift,
		KeyCode.RightShift
	};

	private static readonly KeyCode[] _ControlKeyCodes = new KeyCode[2]
	{
		KeyCode.LeftControl,
		KeyCode.RightControl
	};

	private static readonly KeyCode[] _AltKeyCodes = new KeyCode[2]
	{
		KeyCode.LeftAlt,
		KeyCode.RightAlt
	};

	private static readonly ResourceBlueprint<GameObject> _ValueCostIconBlueprint = "GameState/Ability/Cost/Value/ValueIconBlueprint";

	private static string[] _PlayingCardValueShortText;

	private static readonly ResourceBlueprint<GameObject> _SuitCostIconBlueprint = "GameState/Ability/Cost/Suit/SuitIconBlueprint";

	private static readonly ResourceBlueprint<Sprite> _ClubCostIcon = "GameState/Ability/Cost/Suit/ClubIcon";

	private static readonly ResourceBlueprint<Sprite> _DiamondCostIcon = "GameState/Ability/Cost/Suit/DiamondIcon";

	private static readonly ResourceBlueprint<Sprite> _HeartCostIcon = "GameState/Ability/Cost/Suit/HeartIcon";

	private static readonly ResourceBlueprint<Sprite> _SpadeCostIcon = "GameState/Ability/Cost/Suit/SpadeIcon";

	private static readonly ResourceBlueprint<Sprite> _RedCostIcon = "GameState/Ability/Cost/Suit/RedIcon";

	private static readonly ResourceBlueprint<Sprite> _BlackCostIcon = "GameState/Ability/Cost/Suit/BlackIcon";

	private static readonly ResourceBlueprint<Sprite> _ColorlessCostIcon = "GameState/Ability/Cost/Suit/ColorlessIcon";

	private static readonly ResourceBlueprint<GameObject> _PlayingCardFilterCostLayout = "GameState/Ability/Cost/MultiCostLayout";

	private static readonly ResourceBlueprint<GameObject> _AdditionalCostBlueprint = "GameState/Ability/Cost/Additional/AdditionalCostBlueprint";

	private static readonly ResourceBlueprint<Sprite> _HPCostIcon = "GameState/Ability/Cost/Additional/HPIcon";

	private static readonly ResourceBlueprint<Sprite> _ShieldCostIcon = "GameState/Ability/Cost/Additional/ShieldIcon";

	private static readonly ResourceBlueprint<Sprite> _AttackCostIcon = "GameState/Ability/Cost/Additional/AttackIcon";

	private static readonly ResourceBlueprint<Sprite> _HeroAbilityCostIcon = "GameState/Ability/Cost/Additional/HeroAbilityIcon";

	private static string[] PlayingCardValueShortText => _PlayingCardValueShortText ?? (_PlayingCardValueShortText = (from i in Enumerable.Range(0, (int)(EnumUtil<PlayingCardValue>.Max + 1))
		select (PlayingCardValue)i).Select(delegate(PlayingCardValue value)
	{
		if (value > PlayingCardValue.Ten)
		{
			return EnumUtil.FriendlyName(value)[0].ToString();
		}
		int num = (int)value;
		return num.ToString();
	}).ToArray());

	public static bool Process(this LogicGateType gate, bool a)
	{
		return gate switch
		{
			LogicGateType.Any => a, 
			LogicGateType.All => a, 
			LogicGateType.OnlyOne => a, 
			LogicGateType.None => !a, 
			_ => throw new ArgumentOutOfRangeException("gate", gate, null), 
		};
	}

	public static bool Combine(this LogicGateType gate, bool a, bool b)
	{
		switch (gate)
		{
		case LogicGateType.Any:
			return a || b;
		case LogicGateType.All:
			return a && b;
		case LogicGateType.OnlyOne:
			return a ^ b;
		case LogicGateType.None:
			if (!a)
			{
				return !b;
			}
			return false;
		default:
			throw new ArgumentOutOfRangeException("gate", gate, null);
		}
	}

	public static bool Result(this LogicGateType gate, IEnumerable<bool> bools)
	{
		int num = 0;
		int num2 = 0;
		foreach (bool @bool in bools)
		{
			num += @bool.ToInt();
			num2++;
		}
		return gate switch
		{
			LogicGateType.Any => num > 0, 
			LogicGateType.All => num == num2, 
			LogicGateType.OnlyOne => num == 1, 
			LogicGateType.None => num == 0, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Vector3 ScaleMultiplier(this FlipAxisFlags flippedAxes)
	{
		Vector3 one = Vector3.one;
		if (flippedAxes.IsFlippedHorizontal())
		{
			one.x = -1f;
		}
		if (flippedAxes.IsFlippedVertical())
		{
			one.y = -1f;
		}
		return one;
	}

	public static bool IsFlippedHorizontal(this FlipAxisFlags flippedAxes)
	{
		return (flippedAxes & FlipAxisFlags.FlipHorizontal) != 0;
	}

	public static bool IsFlippedVertical(this FlipAxisFlags flippedAxes)
	{
		return (flippedAxes & FlipAxisFlags.FlipVertical) != 0;
	}

	public static FlipAxisFlags SetAxes(this FlipAxisFlags flippedAxes, FlipAxisFlags axesToSet, bool axesAreFlipped)
	{
		flippedAxes = ((!axesAreFlipped) ? (flippedAxes & (FlipAxisFlags)(~(uint)axesToSet)) : (flippedAxes | axesToSet));
		return flippedAxes;
	}

	public static string GetTag(this CameraType cameraType)
	{
		return cameraType switch
		{
			CameraType.Main => "MainCamera", 
			CameraType.WorldSpaceUI => "WorldSpaceUICamera", 
			CameraType.ScreenSpaceUI => "ScreenSpaceUICamera", 
			_ => throw new ArgumentOutOfRangeException("cameraType", cameraType, null), 
		};
	}

	public static int MaxImportResolution(this ImageCategoryType category)
	{
		return 2048;
	}

	public static Int2? MaxImportDimensions(this ImageCategoryType category)
	{
		return null;
	}

	public static int MaxSaveResolution(this ImageCategoryType category)
	{
		return 512;
	}

	public static bool UseMipmap(this ImageCategoryType category)
	{
		return true;
	}

	public static bool UsesAlpha(this ImageCategoryType category)
	{
		return false;
	}

	public static bool NeedsColorCorrectionForWorkshopImageSearch(this ImageCategoryType category)
	{
		return category.UsesAlpha();
	}

	public static Ushort2? PreferredSize(this ImageCategoryType category)
	{
		return category switch
		{
			ImageCategoryType.Ability => AbilityData.Cosmetic.IMAGE_SIZE, 
			ImageCategoryType.Enemy => EnemyData.Cosmetic.IMAGE_SIZE, 
			ImageCategoryType.Adventure => AdventureCard.IMAGE_SIZE, 
			_ => null, 
		};
	}

	public static Int2 MaxSizeInImageSearch(this ImageCategoryType category)
	{
		return new Int2(1024, 1024);
	}

	public static string GetExtension(this ImageRef.ImageFormat format)
	{
		return format switch
		{
			ImageRef.ImageFormat.png => ".png", 
			ImageRef.ImageFormat.jpg => ".jpg", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Vector2 Pivot(this TextAnchor anchor)
	{
		return anchor switch
		{
			TextAnchor.UpperLeft => new Vector2(0f, 1f), 
			TextAnchor.UpperCenter => new Vector2(0.5f, 1f), 
			TextAnchor.UpperRight => new Vector2(1f, 1f), 
			TextAnchor.MiddleLeft => new Vector2(0f, 0.5f), 
			TextAnchor.MiddleCenter => new Vector2(0.5f, 0.5f), 
			TextAnchor.MiddleRight => new Vector2(1f, 0.5f), 
			TextAnchor.LowerLeft => new Vector2(0f, 0f), 
			TextAnchor.LowerCenter => new Vector2(0.5f, 0f), 
			TextAnchor.LowerRight => new Vector2(1f, 0f), 
			_ => throw new ArgumentOutOfRangeException("anchor", anchor, null), 
		};
	}

	public static string Text(this TextBuilderAlign align)
	{
		return EnumUtil.FriendlyName(align, uppercase: false);
	}

	public static Vector3 GetAttachPosition(this AttachmentType type, Vector3 currentPosition, Vector3 targetPosition, ref Vector3 velocity, float acceleration, float damping, float deltaTime)
	{
		return type switch
		{
			AttachmentType.Immediate => targetPosition, 
			AttachmentType.Ease => MathUtil.EaseV3(ref currentPosition, targetPosition, acceleration, deltaTime), 
			AttachmentType.Spring => MathUtil.Spring(ref currentPosition, ref velocity, targetPosition, acceleration, damping, deltaTime), 
			AttachmentType.Gravity => MathUtil.Gravitate(ref currentPosition, ref velocity, targetPosition, acceleration, damping, deltaTime), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public static Vector2 AccelerationRange(this AttachmentType type)
	{
		return type switch
		{
			AttachmentType.Immediate => Vector2.zero, 
			AttachmentType.Ease => new Vector2(0f, 20f), 
			AttachmentType.Spring => new Vector2(0f, 200f), 
			AttachmentType.Gravity => new Vector2(0f, 20f), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public static Vector2 DampeningRange(this AttachmentType type)
	{
		return type switch
		{
			AttachmentType.Immediate => Vector2.zero, 
			AttachmentType.Ease => Vector2.zero, 
			AttachmentType.Spring => new Vector2(0f, 20f), 
			AttachmentType.Gravity => new Vector2(0f, 1f), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public static IEnumerable<Transform> Children(this WaitForChildrenType waitForChildren, Transform transform)
	{
		if (waitForChildren != WaitForChildrenType.ImmediateChildren)
		{
			return transform.ChildrenRecursive();
		}
		return transform.Children();
	}

	public static bool IsValid(this WaitForSpecificChildrenType waitForSpecificChildren, GameObject gameObject)
	{
		switch (waitForSpecificChildren)
		{
		case WaitForSpecificChildrenType.All:
			return true;
		case WaitForSpecificChildrenType.Visual:
			if (!gameObject.GetComponent<Renderer>())
			{
				return gameObject.GetComponent<ParticleSystem>();
			}
			return true;
		case WaitForSpecificChildrenType.AudioVisual:
			if (!WaitForSpecificChildrenType.Visual.IsValid(gameObject))
			{
				return gameObject.GetComponent<AudioSource>();
			}
			return true;
		default:
			throw new ArgumentOutOfRangeException("waitForSpecificChildren", waitForSpecificChildren, null);
		}
	}

	public static bool ShowData(this DataRefControlFilter filter)
	{
		return filter != DataRefControlFilter.Simple;
	}

	public static float GetLerpAmount(this EaseType easeType, float t, float easePower = 2f)
	{
		return easeType switch
		{
			EaseType.Linear => t, 
			EaseType.EaseInAndOut => MathUtil.CubicSplineInterpolant(t), 
			_ => Mathf.Pow(t, (easeType == EaseType.EaseIn) ? easePower : (1f / easePower)), 
		};
	}

	public static float Ease(this EaseType easeType, float start, float end, float t, float easePower = 2f)
	{
		return MathUtil.Lerp(start, end, easeType.GetLerpAmount(t, easePower));
	}

	public static float Wrap(this WrapMethod wrapMethod, float time)
	{
		switch (wrapMethod)
		{
		case WrapMethod.Mirror:
		{
			int num = (int)time;
			float num2 = time - (float)num;
			if (num % 2 != 0)
			{
				return 1f - num2;
			}
			return num2;
		}
		case WrapMethod.Clamp:
			return Mathf.Clamp01(time);
		case WrapMethod.Repeat:
			return time % 1f;
		default:
			throw new ArgumentOutOfRangeException("wrapMethod", wrapMethod, null);
		}
	}

	public static float WrapShift(this WrapMethod wrapMethod, float shift)
	{
		switch (wrapMethod)
		{
		case WrapMethod.Mirror:
		{
			int num = (int)shift;
			float num2 = Mathf.Abs(shift - (float)num);
			if (num % 2 != 0)
			{
				return 1f - num2;
			}
			return num2;
		}
		case WrapMethod.Clamp:
			return Mathf.Clamp01(shift);
		case WrapMethod.Repeat:
			if (!(shift >= 0f))
			{
				return 1f + shift % 1f;
			}
			return shift % 1f;
		default:
			throw new ArgumentOutOfRangeException("wrapMethod", wrapMethod, null);
		}
	}

	public static float Wrap(this WrapMethod? wrapMethod, float time)
	{
		if (!wrapMethod.HasValue)
		{
			return time;
		}
		return wrapMethod.Value.Wrap(time);
	}

	public static TMP_InputField.ContentType ToTMPContentType(this InputField.ContentType contentType)
	{
		return contentType switch
		{
			InputField.ContentType.Standard => TMP_InputField.ContentType.Standard, 
			InputField.ContentType.Autocorrected => TMP_InputField.ContentType.Autocorrected, 
			InputField.ContentType.IntegerNumber => TMP_InputField.ContentType.IntegerNumber, 
			InputField.ContentType.DecimalNumber => TMP_InputField.ContentType.DecimalNumber, 
			InputField.ContentType.Alphanumeric => TMP_InputField.ContentType.Alphanumeric, 
			InputField.ContentType.Name => TMP_InputField.ContentType.Name, 
			InputField.ContentType.EmailAddress => TMP_InputField.ContentType.EmailAddress, 
			InputField.ContentType.Password => TMP_InputField.ContentType.Password, 
			InputField.ContentType.Pin => TMP_InputField.ContentType.Pin, 
			InputField.ContentType.Custom => TMP_InputField.ContentType.Custom, 
			_ => throw new ArgumentOutOfRangeException("contentType", contentType, null), 
		};
	}

	public static AxisType NormalAxis(this PlaneAxes axes)
	{
		return axes switch
		{
			PlaneAxes.XY => AxisType.Z, 
			PlaneAxes.XZ => AxisType.Y, 
			PlaneAxes.YZ => AxisType.X, 
			_ => throw new ArgumentOutOfRangeException("axes", axes, null), 
		};
	}

	public static AxisType BinormalAxis(this PlaneAxes axes)
	{
		return axes switch
		{
			PlaneAxes.XY => AxisType.X, 
			PlaneAxes.XZ => AxisType.X, 
			PlaneAxes.YZ => AxisType.Y, 
			_ => throw new ArgumentOutOfRangeException("axes", axes, null), 
		};
	}

	public static AxisType TangentAxis(this PlaneAxes axes)
	{
		return axes switch
		{
			PlaneAxes.XY => AxisType.Y, 
			PlaneAxes.XZ => AxisType.Z, 
			PlaneAxes.YZ => AxisType.Z, 
			_ => throw new ArgumentOutOfRangeException("axes", axes, null), 
		};
	}

	public static PlaneAxes GetPlaneAxes(this AxisType axis)
	{
		return axis switch
		{
			AxisType.X => PlaneAxes.YZ, 
			AxisType.Y => PlaneAxes.XZ, 
			AxisType.Z => PlaneAxes.XY, 
			_ => throw new ArgumentOutOfRangeException("axis", axis, null), 
		};
	}

	public static Vector3 ToVector3(this AxisType axis)
	{
		return axis switch
		{
			AxisType.Y => Vector3.up, 
			AxisType.X => Vector3.right, 
			_ => Vector3.forward, 
		};
	}

	public static Vector3 ToVector3(this AxesFlags axes, float onValue = 1f, float offValue = 0f)
	{
		return new Vector3(((axes & AxesFlags.X) != 0) ? onValue : offValue, ((axes & AxesFlags.Y) != 0) ? onValue : offValue, ((axes & AxesFlags.Z) != 0) ? onValue : offValue);
	}

	public static KeyCode FromNumber(this KeyCode keyCode, int number)
	{
		if (number != 10)
		{
			return (KeyCode)(48 + Mathf.Clamp(number, 0, 9));
		}
		return KeyCode.Alpha0;
	}

	public static KeyCode ToKeyCode(this PointerEventData.InputButton button)
	{
		return button switch
		{
			PointerEventData.InputButton.Left => KeyCode.Mouse0, 
			PointerEventData.InputButton.Right => KeyCode.Mouse1, 
			PointerEventData.InputButton.Middle => KeyCode.Mouse2, 
			_ => throw new ArgumentOutOfRangeException("button", button, null), 
		};
	}

	public static bool IsValid(this PointerInputButtonFlags buttonFlags, PointerEventData.InputButton button)
	{
		return EnumUtil.HasFlag(buttonFlags, (PointerInputButtonFlags)(1 << (int)button));
	}

	public static KeyCode[] GetKeyCodes(this KeyModifier modifier)
	{
		return modifier switch
		{
			KeyModifier.Shift => _ShiftKeyCodes, 
			KeyModifier.Control => _ControlKeyCodes, 
			KeyModifier.Alt => _AltKeyCodes, 
			_ => throw new ArgumentOutOfRangeException("modifier", modifier, null), 
		};
	}

	public static string GetText(this KeyModifier modifier)
	{
		return modifier switch
		{
			KeyModifier.Shift => "Shift", 
			KeyModifier.Control => "Ctrl", 
			KeyModifier.Alt => "Alt", 
			_ => throw new ArgumentOutOfRangeException("modifier", modifier, null), 
		};
	}

	public static string GetText(this KeyModifiers modifiers)
	{
		string text = null;
		EnumerateFlags<KeyModifiers>.Enumerator enumerator = EnumUtil<KeyModifiers>.Flags(modifiers).GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyModifier modifier = EnumUtil<KeyModifiers>.ConvertFromFlag<KeyModifier>(enumerator.Current);
			text = ((text != null) ? (text + "+" + modifier.GetText()) : modifier.GetText());
		}
		return text;
	}

	public static GameObject Bubble<T>(this PointerEventBubbleType bubbleType, Component component, PointerEventData eventData, ExecuteEvents.EventFunction<T> callbackFunction, GameObject otherGameObject = null) where T : IEventSystemHandler
	{
		switch (bubbleType)
		{
		case PointerEventBubbleType.None:
			return null;
		case PointerEventBubbleType.Hierarchy:
			if ((bool)component.transform.parent)
			{
				return ExecuteEvents.ExecuteHierarchy(component.transform.parent.gameObject, eventData, callbackFunction);
			}
			return null;
		case PointerEventBubbleType.CachedRaycast:
			return eventData.BubbleEvent(component, callbackFunction);
		case PointerEventBubbleType.NewRaycast:
			return eventData.BubbleEvent(component, callbackFunction, useCachedRaycasts: false);
		case PointerEventBubbleType.OtherGameObject:
			if (!otherGameObject)
			{
				return null;
			}
			return ExecuteEvents.ExecuteHierarchy(otherGameObject, eventData, callbackFunction);
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public static void BubblePointerDrag<T>(this PointerEventBubbleType bubbleType, Component component, PointerEventData eventData, ExecuteEvents.EventFunction<T> callbackFunction) where T : IEventSystemHandler
	{
		GameObject pointerDrag = eventData.pointerDrag;
		GameObject pointerDrag2 = bubbleType.Bubble(component, eventData, callbackFunction);
		if (eventData.pointerDrag == pointerDrag)
		{
			eventData.pointerDrag = pointerDrag2;
		}
	}

	public static IOType Opposite(this IOType ioType)
	{
		if (ioType != 0)
		{
			return IOType.Input;
		}
		return IOType.Output;
	}

	public static float Spring(this SpringType springType, ref float position, ref float velocity, float springPosition, float targetPosition, float springConstant, float damping, float time)
	{
		return springType switch
		{
			SpringType.Standard => MathUtil.Spring(ref position, ref velocity, targetPosition, springConstant, damping, time), 
			SpringType.PushOnly => MathUtil.SpringPush(ref position, ref velocity, springPosition, targetPosition, springConstant, damping, time), 
			SpringType.PullOnly => MathUtil.SpringPull(ref position, ref velocity, springPosition, targetPosition, springConstant, damping, time), 
			_ => throw new ArgumentOutOfRangeException("springType", springType, null), 
		};
	}

	public static Vector2 Spring(this SpringType springType, ref Vector2 position, ref Vector2 velocity, Vector2 springPosition, Vector2 targetPosition, float springConstant, float damping, float time)
	{
		springType.Spring(ref position.x, ref velocity.x, springPosition.x, targetPosition.x, springConstant, damping, time);
		springType.Spring(ref position.y, ref velocity.y, springPosition.y, targetPosition.y, springConstant, damping, time);
		return position;
	}

	public static string GetText(this NodeData.Type type)
	{
		if (type != 0)
		{
			return "x";
		}
		return ">";
	}

	public static Color GetNubTint(this NodeData.Type type)
	{
		if (type != 0)
		{
			return new Color(1f, 0.6f, 0.6f, 1f);
		}
		return new Color(0.6f, 1f, 0.6f, 1f);
	}

	public static NodeData.Type Opposite(this NodeData.Type type)
	{
		if (type != 0)
		{
			return NodeData.Type.True;
		}
		return NodeData.Type.False;
	}

	public static int GetAxis(this RectTransform.Edge edge)
	{
		if (edge != 0 && edge != RectTransform.Edge.Right)
		{
			return 1;
		}
		return 0;
	}

	public static ExtremaType GetExtremaType(this RectTransform.Edge edge)
	{
		if (edge != 0 && edge != RectTransform.Edge.Bottom)
		{
			return ExtremaType.Max;
		}
		return ExtremaType.Min;
	}

	public static float GetAnchor(this RectTransform.Edge edge, RectTransform rectTransform)
	{
		return ((edge.GetExtremaType() == ExtremaType.Min) ? rectTransform.anchorMin : rectTransform.anchorMax)[edge.GetAxis()];
	}

	public static void SetAnchor(this RectTransform.Edge edge, RectTransform rectTransform, Vector2 anchor)
	{
		switch (edge)
		{
		case RectTransform.Edge.Left:
			rectTransform.anchorMin = rectTransform.anchorMin.SetAxis(0, anchor.x);
			break;
		case RectTransform.Edge.Right:
			rectTransform.anchorMax = rectTransform.anchorMax.SetAxis(0, anchor.x);
			break;
		case RectTransform.Edge.Top:
			rectTransform.anchorMax = rectTransform.anchorMax.SetAxis(1, anchor.y);
			break;
		case RectTransform.Edge.Bottom:
			rectTransform.anchorMin = rectTransform.anchorMin.SetAxis(1, anchor.y);
			break;
		default:
			throw new ArgumentOutOfRangeException("edge", edge, null);
		}
	}

	public static void SetNeighboringAnchors(this RectTransform.Edge edge, Vector2 minAnchorDimensions, RectTransform rectTransform, Vector2 neighboringMinAnchorDimensions, RectTransform neighboringRectTransform, Vector2 anchorPositionToSet)
	{
		int axis = edge.GetAxis();
		RectTransform.Edge edge2 = edge.OppositeEdge();
		float value = edge2.GetAnchor(rectTransform) + minAnchorDimensions[axis] * (float)edge2.GetExtremaType().DirectionToCenter();
		float value2 = edge.GetAnchor(neighboringRectTransform) + neighboringMinAnchorDimensions[axis] * (float)edge.GetExtremaType().DirectionToCenter();
		Vector2 v = default(Vector2);
		v[(int)edge2.GetExtremaType()] = value;
		v[(int)edge.GetExtremaType()] = value2;
		if (v.y < v.x)
		{
			v = Vector2.one * v.Average();
		}
		anchorPositionToSet[axis] = Mathf.Clamp(anchorPositionToSet[axis], v.x, v.y);
		edge.SetAnchor(rectTransform, anchorPositionToSet);
		edge2.SetAnchor(neighboringRectTransform, anchorPositionToSet);
	}

	public static int DirectionToCenter(this ExtremaType extremaType)
	{
		if (extremaType != 0)
		{
			return -1;
		}
		return 1;
	}

	public static Color GetTint(this NodeDataIconType iconType)
	{
		switch (iconType)
		{
		case NodeDataIconType.Level:
		case NodeDataIconType.Punish:
		case NodeDataIconType.Campaign:
			return new Color(1f, 0f, 0f);
		case NodeDataIconType.Challenge:
		case NodeDataIconType.Modify:
			return new Color(1f, 1f, 0f);
		case NodeDataIconType.Reward:
			return new Color(0f, 1f, 0f);
		case NodeDataIconType.Story:
		case NodeDataIconType.Dialogue:
			return new Color(1f, 0f, 1f);
		case NodeDataIconType.SubGraph:
		case NodeDataIconType.Random:
			return new Color(1f, 1f, 1f);
		case NodeDataIconType.FlagSet:
		case NodeDataIconType.FlagCheck:
			return new Color(0f, 1f, 1f);
		case NodeDataIconType.PersistedFlagSet:
		case NodeDataIconType.PersistedFlagCheck:
		case NodeDataIconType.SubGraphRef:
		case NodeDataIconType.GameUnlock:
			return new Color(0f, 0f, 1f);
		default:
			return new Color(1f, 1f, 1f);
		}
	}

	public static AUndoRecord.EntryType Opposite(this AUndoRecord.EntryType entryType)
	{
		if (entryType != 0)
		{
			return AUndoRecord.EntryType.Add;
		}
		return AUndoRecord.EntryType.Remove;
	}

	public static string GetText(this FlagCheckType flagCheck)
	{
		return flagCheck switch
		{
			FlagCheckType.GreaterThan => ">", 
			FlagCheckType.LessThan => "<", 
			FlagCheckType.EqualTo => "==", 
			FlagCheckType.NotEqualTo => "!=", 
			FlagCheckType.GreaterThanOrEqualTo => ">=", 
			FlagCheckType.LessThanOrEqualTo => "<=", 
			_ => throw new ArgumentOutOfRangeException("flagCheck", flagCheck, null), 
		};
	}

	public static bool Check(this FlagCheckType flagCheck, int flagValue, int value)
	{
		return flagCheck switch
		{
			FlagCheckType.GreaterThan => flagValue > value, 
			FlagCheckType.LessThan => flagValue < value, 
			FlagCheckType.EqualTo => flagValue == value, 
			FlagCheckType.NotEqualTo => flagValue != value, 
			FlagCheckType.GreaterThanOrEqualTo => flagValue >= value, 
			FlagCheckType.LessThanOrEqualTo => flagValue <= value, 
			_ => throw new ArgumentOutOfRangeException("flagCheck", flagCheck, null), 
		};
	}

	public static bool ShouldUndo(this UndoProcessType type, bool validBranch)
	{
		if (type != UndoProcessType.InvalidOnly)
		{
			return type == UndoProcessType.All;
		}
		return !validBranch;
	}

	public static string GetSymbol(this EnumFlagSetType setType)
	{
		return setType switch
		{
			EnumFlagSetType.Add => "+", 
			EnumFlagSetType.Set => "=", 
			_ => "-", 
		};
	}

	public static T Process<T>(this EnumFlagSetType setType, T flags, T change) where T : struct, IConvertible
	{
		return setType switch
		{
			EnumFlagSetType.Set => change, 
			EnumFlagSetType.Add => EnumUtil.AddSmart(flags, change), 
			EnumFlagSetType.Subtract => EnumUtil.SubtractSmart(flags, change), 
			_ => throw new ArgumentOutOfRangeException("setType", setType, null), 
		};
	}

	public static bool UsesAlpha(this TextureFormat format)
	{
		switch (format)
		{
		case TextureFormat.Alpha8:
		case TextureFormat.RGBA32:
		case TextureFormat.ARGB32:
		case TextureFormat.DXT5:
		case TextureFormat.RGBA4444:
		case TextureFormat.RGBAHalf:
		case TextureFormat.RGBAFloat:
		case TextureFormat.DXT5Crunched:
			return true;
		default:
			return false;
		}
	}

	public static int GetBytesPerPixel(this TextureFormat format)
	{
		if (format == TextureFormat.RGB24)
		{
			return 3;
		}
		return 4;
	}

	public static bool IsStreaming(this AudioCategoryType category)
	{
		return category switch
		{
			AudioCategoryType.Dialogue => true, 
			AudioCategoryType.Adventure => true, 
			AudioCategoryType.Music => true, 
			AudioCategoryType.Ambient => true, 
			_ => false, 
		};
	}

	public static bool IsStereo(this AudioCategoryType category)
	{
		return category switch
		{
			AudioCategoryType.Dialogue => true, 
			AudioCategoryType.Adventure => true, 
			AudioCategoryType.Music => true, 
			AudioCategoryType.Ambient => true, 
			_ => false, 
		};
	}

	public static float AssetCompressionQuality(this AudioCategoryType category)
	{
		return category switch
		{
			AudioCategoryType.Music => 0.85f, 
			AudioCategoryType.Ambient => 0.85f, 
			_ => 0.7f, 
		};
	}

	public static bool ForceMono(this AudioCategoryType category)
	{
		return !category.IsStereo();
	}

	public static float? LoudnessTarget(this AudioCategoryType category)
	{
		return category switch
		{
			AudioCategoryType.Music => null, 
			AudioCategoryType.Ambient => null, 
			_ => -16f, 
		};
	}

	public static float MaxPeak(this AudioCategoryType category)
	{
		return -1f;
	}

	public static float MaxLength(this AudioCategoryType category)
	{
		return 600f;
	}

	public static float Volume(this AudioVolumeType volume)
	{
		return volume switch
		{
			AudioVolumeType.Silent => 0f, 
			AudioVolumeType.Quiet => 0.2f, 
			AudioVolumeType.Soft => 0.5f, 
			AudioVolumeType.Normal => 0.7f, 
			AudioVolumeType.Loud => 0.85f, 
			AudioVolumeType.Max => 1f, 
			_ => throw new ArgumentOutOfRangeException("volume", volume, null), 
		};
	}

	public static bool IsDynamic(this NodeData.RandomizationType randomization)
	{
		return randomization != NodeData.RandomizationType.OneTime;
	}

	public static bool IsDynamic(this NodeData.RandomizationType? randomization)
	{
		if (randomization.HasValue)
		{
			return randomization.Value.IsDynamic();
		}
		return false;
	}

	public static string NamePrefix(this NodeData.RandomizationType randomization)
	{
		return randomization switch
		{
			NodeData.RandomizationType.Dynamic => "*", 
			NodeData.RandomizationType.OneTime => "", 
			_ => "**", 
		};
	}

	public static Color32 GetTint(this ContentCreatorType creatorType)
	{
		return creatorType switch
		{
			ContentCreatorType.Ours => new Color32(128, byte.MaxValue, 128, byte.MaxValue), 
			ContentCreatorType.Yours => new Color32(128, 128, byte.MaxValue, byte.MaxValue), 
			ContentCreatorType.Others => new Color32(byte.MaxValue, 128, 128, byte.MaxValue), 
			_ => throw new ArgumentOutOfRangeException("creatorType", creatorType, null), 
		};
	}

	public static string GetFolderPath(this Environment.SpecialFolder specialFolder)
	{
		return Environment.GetFolderPath(specialFolder);
	}

	public static bool CanSortResults(this FuzzyStringType type)
	{
		return type != FuzzyStringType.Standard;
	}

	public static bool IsValid(this ContentRefManager.TypeFlags flags, ContentRef cRef)
	{
		if (cRef is ImageRef)
		{
			return (flags & ContentRefManager.TypeFlags.Image) != 0;
		}
		if (cRef is AudioRef)
		{
			return (flags & ContentRefManager.TypeFlags.Audio) != 0;
		}
		return (flags & ContentRefManager.TypeFlags.Data) != 0;
	}

	public static bool Visible(this SceneRefVisibility visibility)
	{
		return EnumUtil.HasFlag(visibility, SceneRefVisibility.Build);
	}

	public static bool Visible(this ContentVisibility visibility, CSteamID ownerId, Steam.Ugc.IQueryType queryType)
	{
		if (ProfileManager.prefs.steam.dislikedAuthors.Contains(ownerId.m_SteamID))
		{
			return false;
		}
		switch (visibility)
		{
		case ContentVisibility.Public:
			return true;
		case ContentVisibility.FriendsOnly:
			if (!Steam.Friends.CachedFriends.Contains(ownerId))
			{
				return ownerId == Steam.SteamId;
			}
			return true;
		case ContentVisibility.ByCodeOnly:
			return queryType == Steam.Ugc.IQueryType.Specific;
		case ContentVisibility.Private:
			return ownerId == Steam.SteamId;
		default:
			throw new ArgumentOutOfRangeException("visibility", visibility, null);
		}
	}

	public static Vector2 Apply(this DynamicGridLayout.PreferredSizeType sizeType, Vector2 a, Vector2 b)
	{
		return sizeType switch
		{
			DynamicGridLayout.PreferredSizeType.Min => Vector2.Min(a, b), 
			DynamicGridLayout.PreferredSizeType.Average => (a + b) * 0.5f, 
			DynamicGridLayout.PreferredSizeType.Max => Vector2.Max(a, b), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Vector2 StartingValue(this DynamicGridLayout.PreferredSizeType sizeType, Vector2 average)
	{
		return sizeType switch
		{
			DynamicGridLayout.PreferredSizeType.Min => new Vector2(float.MaxValue, float.MaxValue), 
			DynamicGridLayout.PreferredSizeType.Average => average, 
			DynamicGridLayout.PreferredSizeType.Max => new Vector2(float.MinValue, float.MinValue), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static Transform GetTransform(this ProjectileTransformType transformType, ProjectileFlightSFX projectile)
	{
		return transformType switch
		{
			ProjectileTransformType.Back => projectile.backTransform, 
			ProjectileTransformType.Emitter => projectile.emitterTransform, 
			ProjectileTransformType.Center => projectile.innerTransform, 
			ProjectileTransformType.PointOfImpact => projectile.impactTransform, 
			ProjectileTransformType.Front => projectile.frontTransform, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static float GetValue(this Vector3AnimatorType type, Vector2 range, float elapsedTime, float noiseX = 1f, float noiseY = 0f)
	{
		return type switch
		{
			Vector3AnimatorType.Wave => MathUtil.Remap(Mathf.Sin(elapsedTime * MathF.PI), new Vector2(-1f, 1f), range), 
			Vector3AnimatorType.Noise => MathUtil.Lerp(range.x, range.y, Mathf.PerlinNoise(elapsedTime * noiseX, elapsedTime * noiseY)), 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public static float AngleInDegrees(this CardinalOrientation cardinalOrientation)
	{
		return cardinalOrientation switch
		{
			CardinalOrientation.Forward => 0f, 
			CardinalOrientation.Rightward => 90f, 
			CardinalOrientation.Leftward => -90f, 
			CardinalOrientation.Backward => 180f, 
			_ => throw new ArgumentOutOfRangeException("cardinalOrientation", cardinalOrientation, null), 
		};
	}

	public static Quaternion Rotate(this CardinalOrientation cardinalOrientation, Quaternion rotation)
	{
		return Quaternion.AngleAxis(cardinalOrientation.AngleInDegrees(), Vector3.up) * rotation;
	}

	public static Orients8 ToOrients8(this Orient8 orient8)
	{
		return (Orients8)(1 << (int)orient8);
	}

	public static bool IsDiagonal(this Orient8 orient8)
	{
		return (int)orient8 % 2 != 0;
	}

	public static Short2 Offset(this Orient8 orient8)
	{
		return orient8 switch
		{
			Orient8.Right => Short2.Right, 
			Orient8.RightUp => Short2.RightUp, 
			Orient8.Up => Short2.Up, 
			Orient8.LeftUp => Short2.LeftUp, 
			Orient8.Left => Short2.Left, 
			Orient8.LeftDown => Short2.LeftDown, 
			Orient8.Down => Short2.Down, 
			Orient8.RightDown => Short2.RightDown, 
			_ => throw new ArgumentOutOfRangeException("orient8", orient8, null), 
		};
	}

	public static Vector3 ToDirection(this Orient8 orient8)
	{
		return orient8.Offset().ToVector3().normalized;
	}

	public static Orient8 FromDirection(this Orient8 orient8, Vector3 direction, bool diagonalsOnly = false)
	{
		Short2 @short = new Short2(direction);
		float num = float.MaxValue;
		Orient8 result = orient8;
		Orient8[] values = EnumUtil<Orient8>.Values;
		foreach (Orient8 orient9 in values)
		{
			if (!diagonalsOnly || orient9.IsDiagonal())
			{
				int num2 = (@short - orient9.Offset()).LengthSquared();
				if ((float)num2 < num)
				{
					result = orient9;
					num = num2;
				}
			}
		}
		return result;
	}

	public static Quaternion GetRotation(this AbilityTileSFXOrientType orientation, Short2 position, PositionOrientPair activationOriginPositionOrient, PositionOrientPair activatorPositionOrient, PositionOrientPair? targetPositionOrient, System.Random random)
	{
		Vector3 forward = Vector3.forward;
		switch (orientation)
		{
		case AbilityTileSFXOrientType.Random:
			return Quaternion.Euler(0f, random.Range(0f, 360f), 0f);
		case AbilityTileSFXOrientType.ActivationOrientation:
			forward = activationOriginPositionOrient.orient.ToDirection();
			break;
		case AbilityTileSFXOrientType.ActivatorOrientation:
			forward = activatorPositionOrient.orient.ToDirection();
			break;
		case AbilityTileSFXOrientType.TargetOrientation:
		{
			PositionOrientPair obj = targetPositionOrient ?? activationOriginPositionOrient;
			forward = obj.orient.ToDirection();
			break;
		}
		case AbilityTileSFXOrientType.FromActivationOrientation:
			forward = ((position != activationOriginPositionOrient) ? (position - activationOriginPositionOrient).ToVector3().normalized : activationOriginPositionOrient.orient.ToDirection());
			break;
		case AbilityTileSFXOrientType.FromActivatorOrientation:
			forward = ((position != activatorPositionOrient) ? (position - activatorPositionOrient).ToVector3().normalized : activatorPositionOrient.orient.ToDirection());
			break;
		default:
			throw new ArgumentOutOfRangeException("orientation", orientation, null);
		case AbilityTileSFXOrientType.Identity:
			break;
		}
		return Quaternion.LookRotation(forward, Vector3.up);
	}

	public static string ToText(this PlayingCardValues values)
	{
		if (values == (PlayingCardValues.Jack | PlayingCardValues.Queen | PlayingCardValues.King))
		{
			return "Face";
		}
		return EnumUtil.FriendlyNameFlagRanges(values);
	}

	public static string ShortText(this PlayingCardValues values)
	{
		switch (EnumUtil.FlagCount(values))
		{
		case 0u:
			return "*";
		case 1u:
			return EnumUtil<PlayingCardValues>.ConvertFromFlag<PlayingCardValue>(values).ShortText();
		default:
		{
			if (values == (PlayingCardValues.Jack | PlayingCardValues.Queen | PlayingCardValues.King))
			{
				return "F";
			}
			string text = EnumUtil.FriendlyNameFlagRanges(values);
			if (text.Contains(','))
			{
				return text;
			}
			char c = text[text.Length - 1];
			return ((c == '+') ? EnumUtil.MinActiveFlag(values) : EnumUtil.MaxActiveFlag(values)).ShortText() + c;
		}
		}
	}

	public static GameObject GetCostIcon(this PlayingCardValues? values)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)_ValueCostIconBlueprint);
		gameObject.GetComponentInChildren<TextMeshProUGUI>().text = values.GetValueOrDefault().ShortText();
		return gameObject;
	}

	public static AbilityPreventedBy? GetAbilityPreventedBy(this PlayingCardValues? values, PlayingCardSuits? suits)
	{
		if (!values.HasValue && !suits.HasValue)
		{
			return AbilityPreventedBy.ResourceCard;
		}
		if (values.HasValue)
		{
			return (AbilityPreventedBy)((values switch
			{
				PlayingCardValues.Jack => new AbilityPreventedBy?(AbilityPreventedBy.ResourceJack), 
				PlayingCardValues.Queen => new AbilityPreventedBy?(AbilityPreventedBy.ResourceQueen), 
				PlayingCardValues.King => new AbilityPreventedBy?(AbilityPreventedBy.ResourceKing), 
				PlayingCardValues.Ace => new AbilityPreventedBy?(AbilityPreventedBy.ResourceAce), 
				_ => null, 
			}) ?? 59);
		}
		return suits switch
		{
			PlayingCardSuits.Club => AbilityPreventedBy.ResourceClub, 
			PlayingCardSuits.Diamond => AbilityPreventedBy.ResourceDiamond, 
			PlayingCardSuits.Heart => AbilityPreventedBy.ResourceHeart, 
			PlayingCardSuits.Spade => AbilityPreventedBy.ResourceSpade, 
			PlayingCardSuits.Club | PlayingCardSuits.Spade => AbilityPreventedBy.ResourceBlack, 
			PlayingCardSuits.Diamond | PlayingCardSuits.Heart => AbilityPreventedBy.ResourceRed, 
			_ => null, 
		};
	}

	public static ResourceCostIconType GetCostIconType(this PlayingCardValues values)
	{
		if (EnumUtil.FlagCount(values) == 1)
		{
			return ResourceCostIconType.Value;
		}
		if (values == (PlayingCardValues.Jack | PlayingCardValues.Queen | PlayingCardValues.King))
		{
			return ResourceCostIconType.FaceCard;
		}
		if (!EnumUtil.HasFlag(values, PlayingCardValues.Two))
		{
			return ResourceCostIconType.ValueOrHigher;
		}
		return ResourceCostIconType.ValueOrLower;
	}

	public static ResourceCostIconType GetCostIconType(this PlayingCardValues values, PlayingCardSuits suits)
	{
		uint num = EnumUtil.FlagCount(suits);
		if (num == 0 || num == 4)
		{
			return values.GetCostIconType();
		}
		if (EnumUtil.FlagCount(values) == 1)
		{
			return ResourceCostIconType.ValueAndSuit;
		}
		if (values == (PlayingCardValues.Jack | PlayingCardValues.Queen | PlayingCardValues.King))
		{
			return ResourceCostIconType.FaceCardAndSuit;
		}
		if (!EnumUtil.HasFlag(values, PlayingCardValues.Two))
		{
			return ResourceCostIconType.ValueOrHigherAndSuit;
		}
		return ResourceCostIconType.ValueOrLowerAndSuit;
	}

	public static PlayingCardValuesAceLow ToAceLow(this PlayingCardValues values)
	{
		if ((values & PlayingCardValues.Ace) == 0)
		{
			return (PlayingCardValuesAceLow)values;
		}
		return (PlayingCardValuesAceLow)(values | (PlayingCardValues)2);
	}

	public static bool IsFace(this PlayingCardValue value)
	{
		if (value >= PlayingCardValue.Jack)
		{
			return value <= PlayingCardValue.King;
		}
		return false;
	}

	public static string ShortText(this PlayingCardValue value)
	{
		return PlayingCardValueShortText[(int)value];
	}

	public static LocalizedString Localize(this PlayingCardValue value)
	{
		return MessageData.Instance.poker.value[value];
	}

	public static PlayingCardValueAceLow ToAceLow(this PlayingCardValue value)
	{
		if (value != PlayingCardValue.Ace)
		{
			return (PlayingCardValueAceLow)value;
		}
		return PlayingCardValueAceLow.One;
	}

	public static string ToText(this PlayingCardSuits suits)
	{
		return suits switch
		{
			PlayingCardSuits.Club | PlayingCardSuits.Spade => "Black", 
			PlayingCardSuits.Diamond | PlayingCardSuits.Heart => "Red", 
			_ => EnumUtil.FriendlyName(suits), 
		};
	}

	public static GameObject GetCostIcon(this PlayingCardSuits? suits)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)_SuitCostIconBlueprint);
		Image componentInChildren = gameObject.GetComponentInChildren<Image>();
		componentInChildren.sprite = suits switch
		{
			PlayingCardSuits.Club => _ClubCostIcon, 
			PlayingCardSuits.Diamond => _DiamondCostIcon, 
			PlayingCardSuits.Heart => _HeartCostIcon, 
			PlayingCardSuits.Spade => _SpadeCostIcon, 
			PlayingCardSuits.Diamond | PlayingCardSuits.Heart => _RedCostIcon, 
			PlayingCardSuits.Club | PlayingCardSuits.Spade => _BlackCostIcon, 
			_ => _ColorlessCostIcon, 
		};
		return gameObject;
	}

	public static PlayingCardColor? GetColor(this PlayingCardSuits suits)
	{
		return suits switch
		{
			PlayingCardSuits.Club | PlayingCardSuits.Spade => PlayingCardColor.Black, 
			PlayingCardSuits.Diamond | PlayingCardSuits.Heart => PlayingCardColor.Red, 
			_ => null, 
		};
	}

	public static ResourceCostIconType GetCostIconType(this PlayingCardSuits suits)
	{
		PlayingCardColor? color = suits.GetColor();
		if (color.HasValue)
		{
			PlayingCardColor valueOrDefault = color.GetValueOrDefault();
			if (valueOrDefault != PlayingCardColor.Red)
			{
				return ResourceCostIconType.Black;
			}
			return ResourceCostIconType.Red;
		}
		return EnumUtil<ResourceCostIconType>.Round((float)EnumUtil<PlayingCardSuits>.ConvertFromFlag<PlayingCardSuit>(suits));
	}

	private static IEnumerable<GameObject> GetCostIcons(this PlayingCard.Filter filter)
	{
		if (filter.filtersValue)
		{
			yield return filter.valueFilter.GetCostIcon();
			if (filter.filtersSuit)
			{
				yield return filter.suitFilter.GetCostIcon();
			}
		}
		else
		{
			yield return filter.suitFilter.GetCostIcon();
		}
	}

	public static GameObject GetCostIcon(this PlayingCard.Filter filter)
	{
		using PoolKeepItemListHandle<GameObject> poolKeepItemListHandle = Pools.UseKeepItemList(filter.GetCostIcons());
		if (poolKeepItemListHandle.Count == 1)
		{
			return poolKeepItemListHandle[0].GetOrAddComponent<LocalizedStringRef>().SetData(filter.GetCostIconType().GetTooltip(), filter.GetCostIconLocalizationVariables()).gameObject;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)_PlayingCardFilterCostLayout);
		foreach (GameObject item in poolKeepItemListHandle.value)
		{
			item.transform.SetParent(gameObject.transform, worldPositionStays: false);
		}
		return gameObject.GetOrAddComponent<LocalizedStringRef>().SetData(filter.GetCostIconType().GetTooltip(), filter.GetCostIconLocalizationVariables()).gameObject;
	}

	public static IEnumerable<GameObject> GetCostIcons(this AdditionalResourceCosts costs)
	{
		for (int x3 = 0; x3 < costs.hp; x3++)
		{
			yield return UnityEngine.Object.Instantiate((GameObject)_AdditionalCostBlueprint).GetComponentInChildren<Image>().SetSprite(_HPCostIcon)
				.transform.root.gameObject.GetOrAddComponent<LocalizedStringRef>().SetData(ResourceCostIconType.HP.GetTooltip()).gameObject;
		}
		for (int x3 = 0; x3 < costs.shield; x3++)
		{
			yield return UnityEngine.Object.Instantiate((GameObject)_AdditionalCostBlueprint).GetComponentInChildren<Image>().SetSprite(_ShieldCostIcon)
				.transform.root.gameObject.GetOrAddComponent<LocalizedStringRef>().SetData(ResourceCostIconType.Shield.GetTooltip()).gameObject;
		}
		for (int x3 = 0; x3 < costs.attack; x3++)
		{
			yield return UnityEngine.Object.Instantiate((GameObject)_AdditionalCostBlueprint).GetComponentInChildren<Image>().SetSprite(_AttackCostIcon)
				.transform.root.gameObject.GetOrAddComponent<LocalizedStringRef>().SetData(ResourceCostIconType.Attack.GetTooltip()).gameObject;
		}
	}

	public static string Sign(this ProcessDamageFunction function)
	{
		return function switch
		{
			ProcessDamageFunction.Add => "+", 
			ProcessDamageFunction.Subtract => "-", 
			ProcessDamageFunction.Set => "=", 
			ProcessDamageFunction.Divide => "1/", 
			_ => "*", 
		};
	}

	public static AttackResultType Opposite(this AttackResultType result)
	{
		return result switch
		{
			AttackResultType.Failure => AttackResultType.Success, 
			AttackResultType.Success => AttackResultType.Failure, 
			_ => AttackResultType.Tie, 
		};
	}

	public static AttackResultType? Opposite(this AttackResultType? result)
	{
		return result?.Opposite();
	}

	public static string PastTense(this AttackResultType result)
	{
		return result switch
		{
			AttackResultType.Failure => "Failed", 
			AttackResultType.Success => "Successful", 
			_ => "Tied", 
		};
	}

	public static string PastTense(this AttackResultType? result)
	{
		return result?.PastTense() ?? "";
	}

	public static AttackResultType GetAttackResultType(this PokerHand attackHand, PokerHand defenseHand)
	{
		int? num = attackHand?.CompareTo(defenseHand);
		if (!(num > 0))
		{
			if (!(num < 0))
			{
				return AttackResultType.Tie;
			}
			return AttackResultType.Failure;
		}
		return AttackResultType.Success;
	}

	public static DefenseResultType GetDefenseResultType(this AttackResultType attackResult)
	{
		return attackResult switch
		{
			AttackResultType.Success => DefenseResultType.Failure, 
			AttackResultType.Failure => DefenseResultType.Success, 
			AttackResultType.Tie => DefenseResultType.Tie, 
			_ => DefenseResultType.Tie, 
		};
	}

	public static Faction Opponent(this Faction faction)
	{
		if (faction != 0)
		{
			return Faction.Player;
		}
		return Faction.Enemy;
	}

	public static bool IsEnemy(this Faction faction, Faction otherFaction)
	{
		return faction != otherFaction;
	}

	public static List<ATarget> PostProcessTargets(this AAction.Target.Combatant.Spread spread, ActionContext context, List<ATarget> targets, AAction.Target.Combatant.Allegiance allegiance)
	{
		if (targets.Count == 0)
		{
			return targets;
		}
		switch (spread)
		{
		case AAction.Target.Combatant.Spread.None:
			return targets;
		case AAction.Target.Combatant.Spread.IncludeAdjacentTo:
			foreach (ATarget item in targets.EnumerateSafe())
			{
				foreach (AEntity adjacentEntity in item.gameState.GetAdjacentEntities(item as AEntity))
				{
					CheckAllegianceAndAdd(adjacentEntity);
				}
			}
			break;
		case AAction.Target.Combatant.Spread.AdjacentToOnly:
		{
			using (PoolKeepItemListHandle<ATarget> poolKeepItemListHandle4 = targets.EnumerateSafe())
			{
				targets.Clear();
				foreach (ATarget item2 in poolKeepItemListHandle4.value)
				{
					foreach (AEntity adjacentEntity2 in item2.gameState.GetAdjacentEntities(item2 as AEntity))
					{
						CheckAllegianceAndAdd(adjacentEntity2);
					}
				}
			}
			break;
		}
		case AAction.Target.Combatant.Spread.IncludeLeftOf:
			foreach (ATarget item3 in targets.EnumerateSafe())
			{
				if (!(item3 is AEntity aEntity5))
				{
					continue;
				}
				foreach (AEntity item4 in aEntity5.GetEntitiesToLeftOf())
				{
					CheckAllegianceAndAdd(item4);
				}
			}
			break;
		case AAction.Target.Combatant.Spread.LeftOfOnly:
		{
			using (PoolKeepItemListHandle<ATarget> poolKeepItemListHandle3 = targets.EnumerateSafe())
			{
				targets.Clear();
				foreach (ATarget item5 in poolKeepItemListHandle3.value)
				{
					if (!(item5 is AEntity aEntity4))
					{
						continue;
					}
					foreach (AEntity item6 in aEntity4.GetEntitiesToLeftOf())
					{
						CheckAllegianceAndAdd(item6);
					}
				}
			}
			break;
		}
		case AAction.Target.Combatant.Spread.IncludeRightOf:
			foreach (ATarget item7 in targets.EnumerateSafe())
			{
				if (!(item7 is AEntity aEntity3))
				{
					continue;
				}
				foreach (AEntity item8 in aEntity3.GetEntitiesToRightOf())
				{
					CheckAllegianceAndAdd(item8);
				}
			}
			break;
		case AAction.Target.Combatant.Spread.RightOfOnly:
		{
			using (PoolKeepItemListHandle<ATarget> poolKeepItemListHandle2 = targets.EnumerateSafe())
			{
				targets.Clear();
				foreach (ATarget item9 in poolKeepItemListHandle2.value)
				{
					if (!(item9 is AEntity aEntity2))
					{
						continue;
					}
					foreach (AEntity item10 in aEntity2.GetEntitiesToRightOf())
					{
						CheckAllegianceAndAdd(item10);
					}
				}
			}
			break;
		}
		case AAction.Target.Combatant.Spread.IncludeSelf:
			targets.Add(context.actor);
			break;
		case AAction.Target.Combatant.Spread.Self:
			targets.Clear();
			targets.Add(context.actor);
			break;
		case AAction.Target.Combatant.Spread.AllButTarget:
		{
			using (PoolKeepItemListHandle<ATarget> poolKeepItemListHandle = targets.EnumerateSafe())
			{
				targets.Clear();
				foreach (ATarget item11 in poolKeepItemListHandle.value)
				{
					if (!(item11 is AEntity aEntity))
					{
						continue;
					}
					foreach (AEntity item12 in context.gameState.turnOrderQueue)
					{
						if (item12 != aEntity)
						{
							CheckAllegianceAndAdd(item12);
						}
					}
				}
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("spread", spread, null);
		}
		return targets;
		void CheckAllegianceAndAdd(AEntity target)
		{
			if (allegiance == AAction.Target.Combatant.Allegiance.Any || target.GetAllegiance(context.actor) == allegiance.GetAllegiance())
			{
				targets.Add(target);
			}
		}
	}

	public static bool IncludesMainTarget(this AAction.Target.Combatant.Spread spread)
	{
		switch (spread)
		{
		case AAction.Target.Combatant.Spread.None:
		case AAction.Target.Combatant.Spread.IncludeAdjacentTo:
		case AAction.Target.Combatant.Spread.IncludeLeftOf:
		case AAction.Target.Combatant.Spread.IncludeRightOf:
		case AAction.Target.Combatant.Spread.IncludeSelf:
			return true;
		case AAction.Target.Combatant.Spread.AdjacentToOnly:
		case AAction.Target.Combatant.Spread.LeftOfOnly:
		case AAction.Target.Combatant.Spread.RightOfOnly:
		case AAction.Target.Combatant.Spread.Self:
		case AAction.Target.Combatant.Spread.AllButTarget:
			return false;
		default:
			throw new ArgumentOutOfRangeException("spread", spread, null);
		}
	}

	public static TargetCountType? TargetCount(this AAction.Target.Combatant.Spread spread)
	{
		if (spread != 0)
		{
			return (spread != AAction.Target.Combatant.Spread.Self) ? TargetCountType.MultiTarget : TargetCountType.SingleTarget;
		}
		return null;
	}

	public static bool UsesAllegiance(this AAction.Target.Combatant.Spread spread)
	{
		if (spread != 0 && spread != AAction.Target.Combatant.Spread.IncludeSelf)
		{
			return spread != AAction.Target.Combatant.Spread.Self;
		}
		return false;
	}

	public static bool Contains(this ResourceCard.Piles piles, ResourceCard.Pile? pile)
	{
		if (pile.HasValue)
		{
			return EnumUtil.HasFlagConvert(piles, pile.Value);
		}
		return false;
	}

	public static string ToText(this ResourceCard.Piles piles)
	{
		return piles switch
		{
			ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand => "ACT PILES", 
			ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand => "Combat Hand", 
			ResourceCard.Piles.Hand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand => "Hand & Combat", 
			(ResourceCard.Piles)0 => "<i>Null</i>", 
			_ => EnumUtil.FriendlyName(piles), 
		};
	}

	public static bool IsCombat(this ResourceCard.Pile pile)
	{
		return pile switch
		{
			ResourceCard.Pile.AttackHand => true, 
			ResourceCard.Pile.DefenseHand => true, 
			_ => false, 
		};
	}

	public static bool RegistersReactions(this Ability.Pile? pile)
	{
		return pile switch
		{
			Ability.Pile.Hand => true, 
			Ability.Pile.HeroAct => true, 
			_ => false, 
		};
	}

	public static bool Contains(this Ability.Piles piles, Ability.Pile? pile)
	{
		if (pile.HasValue)
		{
			return EnumUtil.HasFlagConvert(piles, pile.Value);
		}
		return false;
	}

	public static Color GetTint(this DefenseResultType defenseType, bool showInvalid = true)
	{
		return defenseType switch
		{
			DefenseResultType.Failure => Colors.FAILURE, 
			DefenseResultType.Tie => Colors.TIE, 
			DefenseResultType.Success => Colors.SUCCESS, 
			_ => showInvalid ? Colors.FAILURE : new Color(0f, 0f, 0f, 0f), 
		};
	}

	public static DefenseResultType FailureIfInvalid(this DefenseResultType defenseType)
	{
		if (defenseType == DefenseResultType.Invalid)
		{
			return DefenseResultType.Failure;
		}
		return defenseType;
	}

	public static DefenseResultType Opposite(this DefenseResultType defenseType)
	{
		return defenseType switch
		{
			DefenseResultType.Failure => DefenseResultType.Success, 
			DefenseResultType.Tie => DefenseResultType.Tie, 
			DefenseResultType.Success => DefenseResultType.Failure, 
			_ => DefenseResultType.Success, 
		};
	}

	public static Allegiance? GetAllegiance(this AAction.Target.Combatant.Allegiance allegiance)
	{
		return allegiance switch
		{
			AAction.Target.Combatant.Allegiance.Ally => Allegiance.Friend, 
			AAction.Target.Combatant.Allegiance.Enemy => Allegiance.Foe, 
			_ => null, 
		};
	}

	public static string GetText(this AAction.Target.Combatant.Allegiance allegiance, int count = 1)
	{
		return allegiance switch
		{
			AAction.Target.Combatant.Allegiance.Enemy => (count > 1) ? "Enemies" : "Enemy", 
			AAction.Target.Combatant.Allegiance.Ally => (count > 1) ? "Allies" : "Ally", 
			AAction.Target.Combatant.Allegiance.Any => (count > 1) ? "Combatants" : "Combatant", 
			_ => "", 
		};
	}

	public static bool IsTrait(this AbilityData.Type type)
	{
		return type >= AbilityData.Type.Trait;
	}

	public static bool IsBuffOrDebuff(this AbilityData.Type type)
	{
		if (type != AbilityData.Type.Buff)
		{
			return type == AbilityData.Type.Debuff;
		}
		return true;
	}

	public static AppliedPile GetAppliedPile(this AbilityData.Type type)
	{
		return type switch
		{
			AbilityData.Type.Buff => AppliedPile.Buff, 
			AbilityData.Type.Debuff => AppliedPile.Debuff, 
			_ => AppliedPile.Debuff, 
		};
	}

	public static int Max(this AbilityData.Rank rank)
	{
		return rank switch
		{
			AbilityData.Rank.Normal => 4, 
			AbilityData.Rank.Elite => 2, 
			AbilityData.Rank.Legendary => 1, 
			_ => 4, 
		};
	}

	public static Color GetColor(this AbilityData.Rank rank)
	{
		return rank switch
		{
			AbilityData.Rank.Elite => Colors.gameColors.eliteRank, 
			AbilityData.Rank.Legendary => Colors.gameColors.legendRank, 
			_ => Colors.gameColors.normalRank, 
		};
	}

	public static ProjectileMediaPack GetClickMedia(this AbilityData.Rank rank)
	{
		return ContentRef.Defaults.media.abilityRankMedia[rank];
	}

	public static ProjectileMediaPack GetBonusMedia(this AbilityData.Rank rank)
	{
		return ContentRef.Defaults.media.bonusRankMedia[rank];
	}

	public static string GetMeshName(this PrimitiveType primitive)
	{
		return primitive switch
		{
			PrimitiveType.Sphere => "Sphere.fbx", 
			PrimitiveType.Capsule => "Capsule.fbx", 
			PrimitiveType.Cylinder => "Cylinder.fbx", 
			PrimitiveType.Cube => "Cube.fbx", 
			PrimitiveType.Plane => "Plane.fbx", 
			PrimitiveType.Quad => "Quad.fbx", 
			_ => throw new ArgumentOutOfRangeException("primitive", primitive, null), 
		};
	}

	public static Mesh GetMesh(this PrimitiveType primitive)
	{
		return Resources.GetBuiltinResource<Mesh>(primitive.GetMeshName());
	}

	public static bool IsValid(this ReactionEntity reactionEntity, AEntity entity, ActionContext actionContext)
	{
		return reactionEntity switch
		{
			ReactionEntity.Owner => entity == actionContext.actor, 
			ReactionEntity.AppliedOn => entity == actionContext.target, 
			ReactionEntity.Enemy => actionContext.actor.GetAllegiance(entity) == Allegiance.Foe, 
			ReactionEntity.Anyone => true, 
			ReactionEntity.Summon => entity is Ability ability && ability.isSummon, 
			ReactionEntity.Ally => actionContext.actor.GetAllegiance(entity) == Allegiance.Friend, 
			ReactionEntity.OtherAlly => entity != actionContext.actor && actionContext.actor.GetAllegiance(entity) == Allegiance.Friend, 
			ReactionEntity.NotOwner => entity != actionContext.actor, 
			ReactionEntity.NotAppliedOn => entity != actionContext.target, 
			_ => throw new ArgumentOutOfRangeException("reactionEntity", reactionEntity, null), 
		};
	}

	public static bool OwnerOrAnyone(this ReactionEntity reactionEntity)
	{
		if (reactionEntity != 0)
		{
			return reactionEntity == ReactionEntity.Anyone;
		}
		return true;
	}

	public static AEntity GetTarget(this TargetOfReaction targetOfReaction, ReactionContext reactionContext, ActionContext actionContext)
	{
		return targetOfReaction switch
		{
			TargetOfReaction.TriggeredBy => reactionContext.triggeredBy, 
			TargetOfReaction.TriggeredOn => reactionContext.triggeredOn, 
			TargetOfReaction.Owner => actionContext.actor, 
			TargetOfReaction.Anyone => null, 
			TargetOfReaction.Summon => actionContext.gameState.activeSummon, 
			TargetOfReaction.Player => actionContext.gameState.player, 
			TargetOfReaction.Ability => actionContext.ability, 
			TargetOfReaction.AppliedOn => actionContext.gameState.turnOrderQueue.FirstOrDefault((AEntity entity) => entity is ACombatant aCombatant && aCombatant.appliedAbilities.GetCards().Contains(actionContext.ability)), 
			_ => throw new ArgumentOutOfRangeException("targetOfReaction", targetOfReaction, null), 
		};
	}

	public static IProjectileExtrema GetProjectileExtrema(this CardTargetType cardTargetType, ActionContext context)
	{
		return cardTargetType switch
		{
			CardTargetType.Target => context.target.view, 
			CardTargetType.Ability => context.ability?.view ?? context.ability?.owner.view ?? context.gameState.adventureDeck.GetCards(AdventureCard.Pile.SelectionHand).FirstOrDefault()?.view ?? context.gameState.player.view, 
			CardTargetType.AbilityOwner => context.ability.owner.view, 
			CardTargetType.TurnOrder => context.gameState.view.turnOrderTarget, 
			CardTargetType.Player => context.gameState.player.view, 
			CardTargetType.EnemyCombatant => context.gameState.activeCombat?.enemyCombatant.view ?? context.target.view, 
			CardTargetType.TriggeredBy => context.ability?.reactionContext.triggeredBy?.view ?? context.actor.view, 
			CardTargetType.TriggeredOn => context.ability?.reactionContext.triggeredOn?.view ?? context.target.view, 
			_ => null, 
		};
	}

	public static LocalizedString Localize(this CanAttackResult.PreventedBy preventedBy)
	{
		return MessageData.Instance.attack.preventedBy[preventedBy];
	}

	public static bool IsInvalidHand(this CanAttackResult.PreventedBy preventedBy)
	{
		return preventedBy >= CanAttackResult.PreventedBy.InvalidHand;
	}

	public static Ability.CanActivateResult? Message(this Ability.CanActivateResult? result)
	{
		return result?.Message();
	}

	public static LocalizedString LocalizeError(this AbilityPreventedBy preventedBy)
	{
		return MessageData.Instance.ability.preventedBy.error[preventedBy];
	}

	public static LocalizedString LocalizeReaction(this AbilityPreventedBy reaction)
	{
		return MessageData.Instance.ability.preventedBy.reaction[reaction];
	}

	public static ResourceCard.Piles? WildPiles(this AbilityPreventedBy? reaction)
	{
		if (reaction.HasValue && reaction.GetValueOrDefault() == AbilityPreventedBy.ReactionAboutToFailToAttack)
		{
			return ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand;
		}
		return null;
	}

	public static ResourceCard.Piles? EnemyWildPiles(this AbilityPreventedBy? reaction)
	{
		if (reaction.HasValue && reaction.GetValueOrDefault() == AbilityPreventedBy.ReactionAboutToFailToAttack)
		{
			return ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand;
		}
		return null;
	}

	public static string GetTooltip(this AbilityKeyword keyword)
	{
		return MessageData.Instance.ability.keyword.tooltip[keyword].Localize();
	}

	public static string GetTag(this AbilityKeyword keyword, Locale locale = null)
	{
		return MessageData.Instance.ability.keyword.tag[keyword].Localize(locale);
	}

	public static string GetSearchFilter(this AbilityKeyword keyword)
	{
		return MessageData.Instance.ability.keyword.searchFilter[keyword].Localize();
	}

	public static bool ShouldShowUpInItemTags(this AbilityKeyword keyword)
	{
		return keyword switch
		{
			AbilityKeyword.AbilityTagDefense => true, 
			AbilityKeyword.AbilityTagAsync => true, 
			AbilityKeyword.AbilityTagReaction => true, 
			AbilityKeyword.AbilityTagOnly => true, 
			_ => false, 
		};
	}

	public static AbilityPreventedBy? GetAbilityPreventedBy(this PokerHandType pokerHand)
	{
		return pokerHand switch
		{
			PokerHandType.Pair => AbilityPreventedBy.ResourcePair, 
			PokerHandType.ThreeOfAKind => AbilityPreventedBy.ResourceThreeOfAKind, 
			PokerHandType.Straight => AbilityPreventedBy.ResourceStraight, 
			PokerHandType.Flush => AbilityPreventedBy.ResourceFlush, 
			PokerHandType.FullHouse => AbilityPreventedBy.ResourceFullHouse, 
			PokerHandType.FourOfAKind => AbilityPreventedBy.ResourceFourOfAKind, 
			_ => null, 
		};
	}

	public static LocalizedString Localize(this PokerHandType pokerHand)
	{
		return MessageData.Instance.poker.hand[pokerHand];
	}

	public static LocalizedString LocalizeAbility(this DiscardCount count)
	{
		return MessageData.Instance.discard.count.ability[count];
	}

	public static LocalizedString LocalizeAbilityInstruction(this DiscardCount count)
	{
		return MessageData.Instance.discard.count.abilityInstruction[count];
	}

	public static LocalizedString LocalizeResource(this DiscardCount count)
	{
		return MessageData.Instance.discard.count.resource[count];
	}

	public static LocalizedString LocalizeResourceInstruction(this DiscardCount count)
	{
		return MessageData.Instance.discard.count.resourceInstruction[count];
	}

	public static LocalizedString LocalizeMulligan(this DiscardCount count)
	{
		return MessageData.Instance.discard.count.mulligan[count];
	}

	public static LocalizedString LocalizeMulliganInstruction(this DiscardCount count)
	{
		return MessageData.Instance.discard.count.mulliganInstruction[count];
	}

	public static LocalizedString Localize(this DiscardReason reason)
	{
		return MessageData.Instance.discard.reason[reason];
	}

	public static Texture2D GetTexture(this SpecialCursorImage? specialCursor)
	{
		return specialCursor switch
		{
			SpecialCursorImage.CursorAttack => InputManager.CursorAttack, 
			SpecialCursorImage.CursorDefend => InputManager.CursorDefend, 
			SpecialCursorImage.CursorBack => InputManager.CursorBack, 
			_ => null, 
		};
	}

	public static LocalizedString LocalizeLevelUp(this AbilityData.Category category)
	{
		return MessageData.Instance.ability.category.levelUp[category];
	}

	public static bool IsTrait(this AbilityData.Category category)
	{
		return category switch
		{
			AbilityData.Category.Ability => false, 
			AbilityData.Category.HeroAbility => false, 
			AbilityData.Category.Level1Trait => true, 
			AbilityData.Category.Level2Trait => true, 
			AbilityData.Category.Level3Trait => true, 
			AbilityData.Category.TrumpCard => false, 
			_ => throw new ArgumentOutOfRangeException("category", category, null), 
		};
	}

	public static int SortOrder(this AbilityData.Category category)
	{
		if (category == AbilityData.Category.TrumpCard)
		{
			return -1;
		}
		return (int)category;
	}

	public static LocalizedString Localize(this PickCount count)
	{
		return MessageData.Instance.pick.select[count];
	}

	public static LocalizedString LocalizeFlexible(this PickCount count)
	{
		return MessageData.Instance.pick.selectFlexible[count];
	}

	public static bool IsAvailability(this AbilityPreventedBy abilityPreventedBy)
	{
		if (abilityPreventedBy >= AbilityPreventedBy.NotAvailableDuringReaction)
		{
			return abilityPreventedBy <= AbilityPreventedBy.NotAvailableWhilePreparingDefense;
		}
		return false;
	}

	public static bool IsReaction(this AbilityPreventedBy abilityPreventedBy)
	{
		if (abilityPreventedBy < AbilityPreventedBy.OnlyAvailableDuringReaction || abilityPreventedBy > AbilityPreventedBy.ReactionCanWildEnemyDefense)
		{
			return abilityPreventedBy >= AbilityPreventedBy.ReactionDeathsDoorPlayer;
		}
		return true;
	}

	public static bool IsCardResource(this AbilityPreventedBy abilityPreventedBy)
	{
		if (abilityPreventedBy < AbilityPreventedBy.ResourceClub || abilityPreventedBy > AbilityPreventedBy.ResourceFourOfAKind)
		{
			return abilityPreventedBy == AbilityPreventedBy.ResourceCardValue;
		}
		return true;
	}

	public static bool IsResource(this AbilityPreventedBy abilityPreventedBy)
	{
		if (abilityPreventedBy < AbilityPreventedBy.ResourceHP || abilityPreventedBy > AbilityPreventedBy.ResourceFourOfAKind)
		{
			return abilityPreventedBy == AbilityPreventedBy.ResourceCardValue;
		}
		return true;
	}

	public static bool IsTargeting(this AbilityPreventedBy abilityPreventedBy)
	{
		if (abilityPreventedBy >= AbilityPreventedBy.NoValidTargets)
		{
			return abilityPreventedBy <= AbilityPreventedBy.NotEnoughAbilities;
		}
		return false;
	}

	public static LocalizedString Localize(this SelectableTargetType type)
	{
		return MessageData.Instance.target.type[type];
	}

	public static LocalizedString Localize(this PlayerTurnTutorial tutorial)
	{
		return MessageData.Instance.tutorial.playerTurn[tutorial];
	}

	public static Vector2 GetPivot(this TooltipAlignment alignment)
	{
		return alignment switch
		{
			TooltipAlignment.TopLeft => new Vector2(0f, 1f), 
			TooltipAlignment.TopCenter => new Vector2(0.5f, 1f), 
			TooltipAlignment.TopRight => new Vector2(1f, 1f), 
			TooltipAlignment.MiddleLeft => new Vector2(0f, 0.5f), 
			TooltipAlignment.MiddleCenter => new Vector2(0.5f, 0.5f), 
			TooltipAlignment.MiddleRight => new Vector2(1f, 0.5f), 
			TooltipAlignment.BottomLeft => new Vector2(0f, 0f), 
			TooltipAlignment.BottomCenter => new Vector2(0.5f, 0f), 
			TooltipAlignment.BottomRight => new Vector2(1f, 0f), 
			_ => Vector2.zero, 
		};
	}

	public static string GetMessage(this NumberOfAttacks attacks)
	{
		return MessageData.Instance.attack.remaining[attacks].Localize();
	}

	public static string GetTooltip(this Stone.Pile pile)
	{
		return MessageData.Instance.button.stoneTooltip[pile].Localize();
	}

	public static float GetScale(this StoneType stone)
	{
		if (stone == StoneType.Cancel)
		{
			return 0.94f;
		}
		return 1f;
	}

	public static LocalizedString Localize(this EndTurnPreventedBy preventedBy)
	{
		return MessageData.Instance.player.endTurnPreventedBy[preventedBy];
	}

	public static string GetTooltip(this ButtonCardType button)
	{
		return MessageData.Instance.button.buttonTooltip[button].Localize();
	}

	public static bool IsDynamic(this ActionContextTarget contextTarget)
	{
		return contextTarget switch
		{
			ActionContextTarget.Attacker => true, 
			ActionContextTarget.Defender => true, 
			ActionContextTarget.EnemyInActiveCombat => true, 
			ActionContextTarget.FirstEnemyInTurnOrder => true, 
			_ => false, 
		};
	}

	public static bool IsCombatCentric(this ActionContextTarget contextTarget)
	{
		return contextTarget switch
		{
			ActionContextTarget.Attacker => true, 
			ActionContextTarget.Defender => true, 
			ActionContextTarget.EnemyInActiveCombat => true, 
			_ => false, 
		};
	}

	private static float _HoloIntensityMultiplier(this PlayingCardSkinType skin)
	{
		return skin switch
		{
			PlayingCardSkinType.Enemy => 1.333f, 
			PlayingCardSkinType.Invernus => 1.333f, 
			_ => 1f, 
		};
	}

	private static float _HoloLuminancePower(this PlayingCardSkinType skin)
	{
		return skin switch
		{
			PlayingCardSkinType.Enemy => 0.075f, 
			PlayingCardSkinType.Invernus => 0.075f, 
			_ => 0.25f, 
		};
	}

	private static float? _HoloIntensityMultiplierForFaceCard(this PlayingCardSkinType skin)
	{
		if (skin == PlayingCardSkinType.Invernus)
		{
			return PlayingCardSkinType.Default._HoloIntensityMultiplier();
		}
		return null;
	}

	private static float? _HoloLuminancePowerForFaceCard(this PlayingCardSkinType skin)
	{
		if (skin == PlayingCardSkinType.Invernus)
		{
			return PlayingCardSkinType.Default._HoloLuminancePower();
		}
		return null;
	}

	public static float HoloIntensityMultiplier(this PlayingCardSkinType skin, bool isFaceCard)
	{
		if (!isFaceCard)
		{
			return skin._HoloIntensityMultiplier();
		}
		return skin._HoloIntensityMultiplierForFaceCard() ?? skin._HoloIntensityMultiplier();
	}

	public static float HoloLuminancePower(this PlayingCardSkinType skin, bool isFaceCard)
	{
		if (!isFaceCard)
		{
			return skin._HoloLuminancePower();
		}
		return skin._HoloLuminancePowerForFaceCard() ?? skin._HoloLuminancePower();
	}

	public static LocalizedString Localize(this AdventureTutorial tutorial)
	{
		return MessageData.Instance.tutorial.adventure[tutorial];
	}

	public static string GetVerb(this CombatTypes combatTypes)
	{
		return combatTypes switch
		{
			CombatTypes.Attack => "attacking", 
			CombatTypes.Defense => "defending", 
			_ => "combating", 
		};
	}

	public static int GetCompletionTime(this AdventureCompletionRank rank, int sRankTime)
	{
		return Mathf.RoundToInt((float)sRankTime * Mathf.Pow(2f, (float)(rank - 1)));
	}

	public static CroppedImageRef GetImage(this AdventureCompletionRank rank)
	{
		return ContentRef.Defaults.media.adventureCompletionRankMedia[rank]?.image;
	}

	public static ProjectileMediaPack GetClickMedia(this AdventureCompletionRank rank)
	{
		return ContentRef.Defaults.media.adventureCompletionRankMedia[rank]?.clickMedia;
	}

	public static AbilityData.Rank ToAbilityRank(this AdventureCompletionRank rank)
	{
		return rank switch
		{
			AdventureCompletionRank.SPlus => AbilityData.Rank.Legendary, 
			AdventureCompletionRank.S => AbilityData.Rank.Legendary, 
			AdventureCompletionRank.A => AbilityData.Rank.Elite, 
			_ => AbilityData.Rank.Normal, 
		};
	}

	public static AdventureDeckSticker ToAdventureDeckSticker(this AdventureCompletionRank rank)
	{
		return rank switch
		{
			AdventureCompletionRank.SPlus => AdventureDeckSticker.RankSPlus, 
			AdventureCompletionRank.S => AdventureDeckSticker.RankS, 
			AdventureCompletionRank.A => AdventureDeckSticker.RankA, 
			AdventureCompletionRank.B => AdventureDeckSticker.RankB, 
			_ => AdventureDeckSticker.RankC, 
		};
	}

	public static LocalizedString Localize(this PlayerClass playerClass)
	{
		return MessageData.Instance.player.playerClass[playerClass];
	}

	public static string GetText(this PlayerClass playerClass)
	{
		return MessageData.Instance.player.playerClass[playerClass].Localize();
	}

	public static LocalizedString LocalizeUnlockMessage(this PlayerClass playerClass)
	{
		return MessageData.Instance.player.playerClassUnlock[playerClass];
	}

	public static int SortOrder(this PlayerClass playerClass)
	{
		return playerClass switch
		{
			PlayerClass.Warrior => 0, 
			PlayerClass.Rogue => 1, 
			PlayerClass.Mage => 2, 
			PlayerClass.Hunter => 3, 
			PlayerClass.Enchantress => 4, 
			_ => throw new ArgumentOutOfRangeException("playerClass", playerClass, null), 
		};
	}

	public static AdventureDeckSticker ToAdventureDeckSticker(this PlayerClass playerClass)
	{
		return playerClass switch
		{
			PlayerClass.Rogue => AdventureDeckSticker.Rogue, 
			PlayerClass.Mage => AdventureDeckSticker.Mage, 
			PlayerClass.Enchantress => AdventureDeckSticker.Enchantress, 
			PlayerClass.Hunter => AdventureDeckSticker.Hunter, 
			_ => AdventureDeckSticker.Warrior, 
		};
	}

	public static string GetText(this AdventureResultType type)
	{
		return MessageData.Instance.game.adventureResult[type].Localize();
	}

	public static float GetEmissionMultiplier(this ProfileOptions.VideoOptions.QualityOptions.ParticleQuality quality)
	{
		return quality switch
		{
			ProfileOptions.VideoOptions.QualityOptions.ParticleQuality.Minimum => 0.05f, 
			ProfileOptions.VideoOptions.QualityOptions.ParticleQuality.ExtremelyLow => 0.1f, 
			ProfileOptions.VideoOptions.QualityOptions.ParticleQuality.VeryLow => 0.1f, 
			ProfileOptions.VideoOptions.QualityOptions.ParticleQuality.Low => 0.33f, 
			ProfileOptions.VideoOptions.QualityOptions.ParticleQuality.Medium => 0.5f, 
			ProfileOptions.VideoOptions.QualityOptions.ParticleQuality.High => 0.75f, 
			ProfileOptions.VideoOptions.QualityOptions.ParticleQuality.VeryHigh => 1f, 
			_ => 0.5f, 
		};
	}

	public static float GetLaunchMultiplier(this ProfileOptions.VideoOptions.QualityOptions.ParticleQuality quality)
	{
		return quality switch
		{
			ProfileOptions.VideoOptions.QualityOptions.ParticleQuality.ExtremelyLow => 0.501f, 
			ProfileOptions.VideoOptions.QualityOptions.ParticleQuality.Minimum => 0.25f, 
			_ => 1f, 
		};
	}

	public static float GetDensity(this ProfileOptions.VideoOptions.QualityOptions.FoliageQuality quality)
	{
		return quality switch
		{
			ProfileOptions.VideoOptions.QualityOptions.FoliageQuality.Off => 0f, 
			ProfileOptions.VideoOptions.QualityOptions.FoliageQuality.VeryLow => 0.25f, 
			ProfileOptions.VideoOptions.QualityOptions.FoliageQuality.Low => 0.5f, 
			ProfileOptions.VideoOptions.QualityOptions.FoliageQuality.Medium => 0.75f, 
			ProfileOptions.VideoOptions.QualityOptions.FoliageQuality.High => 1f, 
			_ => 0.5f, 
		};
	}

	public static DiceType GetDiceTypeFromMax(this DiceType diceType, int max)
	{
		max = Math.Max(6, max);
		DiceType[] values = EnumUtil<DiceType>.Values;
		foreach (DiceType diceType2 in values)
		{
			if ((int)diceType2 >= max)
			{
				return diceType2;
			}
		}
		return EnumUtil<DiceType>.Max;
	}

	public static string GetMessage(this DeckCreationMessage message)
	{
		return MessageData.Instance.deck.message[message].Localize();
	}

	public static LocalizedString Localize(this DeckCreationMessage message)
	{
		return MessageData.Instance.deck.message[message];
	}

	public static bool AutoSelect(this AutoSelectSingleTargetType autoSelect, Ability ability)
	{
		return autoSelect switch
		{
			AutoSelectSingleTargetType.Never => false, 
			AutoSelectSingleTargetType.Reactions => ability?.hasActiveReaction ?? false, 
			AutoSelectSingleTargetType.Always => true, 
			_ => throw new ArgumentOutOfRangeException("autoSelect", autoSelect, null), 
		};
	}

	public static string GetLabel(this NewGameType newGameType)
	{
		return MessageData.Instance.game.newGame[newGameType].Localize();
	}

	public static float GetCompletionTimeMultiplier(this NewGameType newGameType)
	{
		return newGameType switch
		{
			NewGameType.Summer => 1.2f, 
			NewGameType.Fall => 1.6f, 
			NewGameType.Winter => 2f, 
			_ => 1f, 
		};
	}

	public static LocalizedString Localize(this LevelUpMessages message)
	{
		return MessageData.Instance.game.levelUp[message];
	}

	public static string GetMessage(this LeafLevel leaf)
	{
		return MessageData.Instance.game.leaf[leaf].Localize();
	}

	public static string GetMessage(this RebirthLevel rebirth)
	{
		if (rebirth <= RebirthLevel.Zero)
		{
			return "";
		}
		return MessageData.Instance.game.rebirth[rebirth].Localize();
	}

	public static LocalizedString GetTitle(this MessageData.UIPopupTitle title)
	{
		return MessageData.Instance.popup.title[title];
	}

	public static LocalizedString GetMessage(this MessageData.UIPopupMessage message)
	{
		return MessageData.Instance.popup.message[message];
	}

	public static LocalizedString GetButton(this MessageData.UIPopupButton button)
	{
		return MessageData.Instance.popup.button[button];
	}

	public static LocalizedString Localize(this MessageData.GameTooltips tooltip)
	{
		return MessageData.Instance.game.tooltips[tooltip];
	}

	public static LocalizedString GetInspectedCardLocalize(this MessageData.GameTooltips tooltip)
	{
		if ((uint)(tooltip - 5) <= 1u)
		{
			return MessageData.GameTooltips.Ability.Localize();
		}
		return MessageData.GameTooltips.Card.Localize();
	}

	public static uint? GetDlcId(this PlayingCardSkinType skin)
	{
		return skin switch
		{
			PlayingCardSkinType.Invernus => 2097120u, 
			PlayingCardSkinType.LuckOfThePaw => 2367830u, 
			_ => null, 
		};
	}

	public static uint? GetDlcId(this TableSkinType skin)
	{
		if (skin == TableSkinType.Invernus)
		{
			return 2097060u;
		}
		return null;
	}

	public static uint? GetDlcId(this TokenSkinType skin)
	{
		if (skin == TokenSkinType.Invernus)
		{
			return 2097110u;
		}
		return null;
	}

	public static bool IsEncounter(this ProceduralNodeType type)
	{
		return type <= ProceduralNodeType.BossGroupEncounter;
	}

	public static bool IsEncounterOrInvernus(this ProceduralNodeType type)
	{
		if (!type.IsEncounter() && type != ProceduralNodeType.Invernus)
		{
			return type == ProceduralNodeType.FinalInvernus;
		}
		return true;
	}

	public static bool IsGroupEncounter(this ProceduralNodeType type)
	{
		if (type.IsEncounter())
		{
			return type >= ProceduralNodeType.GroupEncounter;
		}
		return false;
	}

	public static ProceduralEncounterDifficulty EncounterDifficulty(this ProceduralNodeType type)
	{
		return type switch
		{
			ProceduralNodeType.EliteEncounter => ProceduralEncounterDifficulty.Elite, 
			ProceduralNodeType.EliteGroupEncounter => ProceduralEncounterDifficulty.Elite, 
			ProceduralNodeType.BossEncounter => ProceduralEncounterDifficulty.Boss, 
			ProceduralNodeType.BossGroupEncounter => ProceduralEncounterDifficulty.Boss, 
			ProceduralNodeType.Invernus => ProceduralEncounterDifficulty.Boss, 
			ProceduralNodeType.FinalInvernus => ProceduralEncounterDifficulty.Boss, 
			_ => ProceduralEncounterDifficulty.Normal, 
		};
	}

	public static LocalizedString LocalizeName(this ProceduralNodeType type)
	{
		return MessageData.Instance.game.proceduralNodeNames[type];
	}

	public static int Size(this ProceduralNodeType type)
	{
		return type switch
		{
			ProceduralNodeType.BossEncounter => 150, 
			ProceduralNodeType.BossGroupEncounter => 150, 
			ProceduralNodeType.Invernus => 175, 
			ProceduralNodeType.FinalInvernus => 200, 
			ProceduralNodeType.Empty => 66, 
			_ => 100, 
		};
	}

	public static bool IsLikeNode(this ProceduralNodeType type, ProceduralNodeType otherType)
	{
		bool flag = type == otherType;
		if (!flag)
		{
			flag = type switch
			{
				ProceduralNodeType.Encounter => otherType == ProceduralNodeType.GroupEncounter, 
				ProceduralNodeType.GroupEncounter => otherType == ProceduralNodeType.Encounter, 
				ProceduralNodeType.ShortRest => otherType == ProceduralNodeType.Rest, 
				ProceduralNodeType.Rest => otherType == ProceduralNodeType.ShortRest, 
				_ => false, 
			};
		}
		return flag;
	}

	public static ProceduralNodeType SetEncounterDifficulty(this ProceduralNodeType type, ProceduralEncounterDifficulty difficulty)
	{
		return (ProceduralNodeType)(type.IsGroupEncounter().ToInt(3) + difficulty);
	}

	public static bool PreventRepeats(this ProceduralNodeType type)
	{
		if (!type.IsEncounter())
		{
			return type == ProceduralNodeType.Event;
		}
		return true;
	}

	public static bool ShowCardTooltips(this ProceduralNodeType type, bool fallback = false)
	{
		return type switch
		{
			ProceduralNodeType.BossEncounter => true, 
			ProceduralNodeType.BossGroupEncounter => true, 
			ProceduralNodeType.Invernus => true, 
			ProceduralNodeType.FinalInvernus => true, 
			_ => fallback, 
		};
	}

	public static AdventureCard.SelectInstruction GetInstruction(this AdventureCard.RepeatCard.DrawInstructionType type, byte count)
	{
		if (count == 0)
		{
			return null;
		}
		return type switch
		{
			AdventureCard.RepeatCard.DrawInstructionType.Draw => new AdventureCard.SelectInstruction.Draw(count), 
			AdventureCard.RepeatCard.DrawInstructionType.PickOne => new AdventureCard.SelectInstruction.Pick(1, count, flexiblePickCount: false), 
			AdventureCard.RepeatCard.DrawInstructionType.PickFlexible => new AdventureCard.SelectInstruction.Pick(count, count, flexiblePickCount: true), 
			_ => null, 
		};
	}

	public static AbilityKeyword GetTraitTag(this ItemCardType type)
	{
		return type switch
		{
			ItemCardType.Item => AbilityKeyword.AbilityTagEquipment, 
			ItemCardType.Encounter => AbilityKeyword.AbilityTagEncounterCondition, 
			_ => AbilityKeyword.AbilityTagCondition, 
		};
	}

	public static AbilityKeyword GetActivatableTag(this ItemCardType type)
	{
		return type switch
		{
			ItemCardType.Item => AbilityKeyword.AbilityTagItem, 
			ItemCardType.Encounter => AbilityKeyword.AbilityTagEncounterAbility, 
			_ => AbilityKeyword.AbilityTagCondition, 
		};
	}

	public static int SortOrder(this ItemCardType? type, bool isTrait)
	{
		if (isTrait)
		{
			return type switch
			{
				ItemCardType.Item => -10, 
				ItemCardType.Encounter => 10, 
				ItemCardType.Condition => 5, 
				_ => 0, 
			};
		}
		return type switch
		{
			ItemCardType.Item => -10, 
			ItemCardType.Encounter => -1, 
			ItemCardType.Condition => -5, 
			_ => 0, 
		};
	}

	public static bool IsCondition(this ItemCardType? type)
	{
		if (type != ItemCardType.Encounter)
		{
			return type == ItemCardType.Condition;
		}
		return true;
	}

	public static bool IsAdjustment(this StatAction.Function function)
	{
		return function != StatAction.Function.Set;
	}

	public static int Sign(this StatAction.Function function)
	{
		if (function != StatAction.Function.Subtract)
		{
			return 1;
		}
		return -1;
	}

	public static int SortOrder(this StatAction.Function function)
	{
		return function switch
		{
			StatAction.Function.Set => -3, 
			StatAction.Function.Multiply => -2, 
			StatAction.Function.Divide => -1, 
			_ => 0, 
		};
	}

	public static string Symbol(this StatAction.Function function)
	{
		return function switch
		{
			StatAction.Function.Add => "+", 
			StatAction.Function.Set => "=", 
			StatAction.Function.Subtract => "-", 
			StatAction.Function.Multiply => "*", 
			StatAction.Function.Divide => "/", 
			_ => "", 
		};
	}

	public static OutOfDateTranslationHandling? GetValue(this OutOfDateTranslationHandling type)
	{
		if (type == OutOfDateTranslationHandling.UseParentSetting)
		{
			return null;
		}
		return type;
	}

	public static string GetText(this AdventureCard.Piles piles)
	{
		if (piles == (AdventureCard.Piles.ActiveHand | AdventureCard.Piles.TurnOrder))
		{
			return "Encounter";
		}
		return EnumUtil.FriendlyName(piles);
	}

	public static LocalizedString GetTooltip(this ResourceCostIconType type)
	{
		return MessageData.Instance.ability.cost.tooltip[type];
	}

	public static bool IsSpecificValue(this ResourceCostIconType type)
	{
		if (type == ResourceCostIconType.Value || type == ResourceCostIconType.ValueAndSuit)
		{
			return true;
		}
		return false;
	}

	public static bool IsValueOrHigher(this ResourceCostIconType type)
	{
		if (type == ResourceCostIconType.ValueOrHigher || type == ResourceCostIconType.ValueOrHigherAndSuit)
		{
			return true;
		}
		return false;
	}

	public static LocalizedString LocalizeSuits(this PlayingCardColor color)
	{
		return MessageData.Instance.poker.colorSuits[color];
	}

	public static LocalizedString Localize(this PlayingCardSuit suit)
	{
		return MessageData.Instance.poker.suit[suit];
	}

	public static PlayingCardValue ToValue(this PlayingCardValueAceLow value)
	{
		if (value != PlayingCardValueAceLow.One)
		{
			return (PlayingCardValue)value;
		}
		return PlayingCardValue.Ace;
	}

	public static string GetText(this DamageSources sources)
	{
		if (sources == (DamageSources.Attack | DamageSources.Defense))
		{
			return "Combat";
		}
		return EnumUtil.FriendlyName(sources);
	}

	public static MSAAMode GetMode(this MultiSampleType type)
	{
		return type switch
		{
			MultiSampleType.UseQualityPreset => MSAAMode.FromHDRPAsset, 
			MultiSampleType.Off => MSAAMode.None, 
			MultiSampleType.X2 => MSAAMode.MSAA2X, 
			MultiSampleType.X4 => MSAAMode.MSAA4X, 
			MultiSampleType.X8 => MSAAMode.MSAA8X, 
			_ => MSAAMode.FromHDRPAsset, 
		};
	}

	public static int SortValue(this SpecialGameType type)
	{
		return type switch
		{
			SpecialGameType.Spiral => 0, 
			SpecialGameType.Procedural => 1, 
			SpecialGameType.Challenge => 2, 
			_ => 100, 
		};
	}
}
