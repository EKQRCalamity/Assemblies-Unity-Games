using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Card Layout/CardHandLayoutSettings")]
public class CardHandLayoutSettings : ScriptableObject
{
	public struct PointerOverSettings
	{
		public static readonly PointerOverSettings Default;

		public readonly float pointerOverRaise;

		public PointerOverSettings(float pointerOverRaise)
		{
			this.pointerOverRaise = pointerOverRaise;
		}

		public void Set(CardHandLayoutSettings settings)
		{
			settings.pointerOverRaise = pointerOverRaise;
		}
	}

	public enum EnterTransitionOnEndDrag
	{
		Never,
		Always,
		OnDraggedWithinThreshold,
		OnDraggedBeyondThreshold,
		UseDragStayTarget
	}

	public enum SpacingBasedEnterTransitionType
	{
		AllButEndCard,
		WhenNotEmpty,
		Always
	}

	public enum CanDragWhen
	{
		Never,
		Always,
		WhenNotOffset
	}

	[Header("Hand Layout")]
	public float fanRotationDegrees = 20f;

	public float spacingAtMinCardCount = 1f;

	public float spacingAtMaxCardCount = 0.333f;

	public float groupSpacing = 0.333f;

	public int maxCardCount = 10;

	public float maxTotalSpacing;

	[HideInInspectorIf("_hideMaxTotalSpacingSettings", false)]
	public float alignmentCenterAtMaxTotalSpacing = -1f;

	[HideInInspectorIf("_hideTargetSpacingForAlignmentCenterCorrection", false)]
	public float targetSpacingForAlignmentCenterCorrection;

	[HideInInspectorIf("_hideTargetSpacingForAlignmentCenterCorrection", false)]
	public float alignmentCenterCorrectionPower = 0.5f;

	[Range(0f, 1f)]
	public float alignmentCenter = 0.5f;

	[Range(0f, 1f)]
	public float thicknessCenter = 0.5f;

	[Range(0f, 10f)]
	public float thicknessPadding = 1f;

	public bool useRealisticRotation;

	public bool reverseLayoutOrder;

	public bool useUnderlyingOrder = true;

	[Header("Pointer Over")]
	public float pointerOverSpacingLeft = 0.5f;

	public float pointerOverSpacingRight = 1f;

	public bool pointerOverSpacingOnlyIfGreater = true;

	public float pointerOverRaise = 0.5f;

	public bool correctPointerOverRaiseForScale;

	public float pointerOverUp;

	public bool pointerOverUpEntireHand;

	public float pointerOverSpacingSelf;

	public CardLayoutElement.PointerOverPadding defaultColliderPadding;

	public CardLayoutElement.PointerOverPadding pointerOverPadding;

	public float pointerOverEdgePadding;

	[Range(0f, 10f)]
	public float pointerOverAnticipateDistance;

	[Range(-0.5f, -0.05f)]
	public float pointerOverAnticipateShrinkAmount = -0.1f;

	public bool inputEnabled = true;

	public bool onlyTopCardHasInputEnabled;

	public bool applyFanningToPointerOver;

	[Header("Drag")]
	public CanDragWhen canDrag = CanDragWhen.Always;

	public float dragRotationPerSpeed = 30f;

	[Range(0f, 2f)]
	public float dragPlaneLerpDistance = 1f;

	[Range(1f, 100f)]
	public float dragThicknessPadding = 2f;

	public bool centerDragEntireHand;

	public bool useDragOffset;

	[Header("Transitions")]
	public float spacingBasedEnterTransition;

	[HideInInspectorIf("_hideSpacingBasedEnterType", false)]
	public SpacingBasedEnterTransitionType spacingBasedEnterType;

	public bool useDynamicTransitionTargets = true;

	public EnterTransitionOnEndDrag doEnterTransitionsOnEndDragStay;

	public bool adjustExitTargetsForPointerOver;

	public bool processExitTargets = true;

	[Header("Audio")]
	public bool playPointerOverSoundOnFirstCard = true;

	public bool playPointerOverSoundOnLastCard = true;

	private bool _hideSpacingBasedEnterType => spacingBasedEnterTransition == 0f;

	private bool _hideMaxTotalSpacingSettings => maxTotalSpacing <= 0f;

	private bool _hideTargetSpacingForAlignmentCenterCorrection
	{
		get
		{
			if (!_hideMaxTotalSpacingSettings)
			{
				return alignmentCenterAtMaxTotalSpacing < 0f;
			}
			return true;
		}
	}

	public PointerOverSettings GetPointerOverSettings()
	{
		return new PointerOverSettings(pointerOverRaise);
	}

	public void SetPointerOverSettings(PointerOverSettings settings)
	{
		settings.Set(this);
	}
}
