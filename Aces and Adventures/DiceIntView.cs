using System.Collections.Generic;
using UnityEngine;

public class DiceIntView : MonoBehaviour
{
	public const DiceType SMALLEST_DIE_TYPE = DiceType.D6;

	private static readonly Dictionary<DiceType, ResourceBlueprint<GameObject>> _Dice = new Dictionary<DiceType, ResourceBlueprint<GameObject>>
	{
		{
			DiceType.D4,
			"GameState/Dice/D4"
		},
		{
			DiceType.D6,
			"GameState/Dice/D6"
		},
		{
			DiceType.D8,
			"GameState/Dice/D8"
		},
		{
			DiceType.D10,
			"GameState/Dice/D10"
		},
		{
			DiceType.D12,
			"GameState/Dice/D12"
		},
		{
			DiceType.D20,
			"GameState/Dice/D20"
		}
	};

	public Transform[] faces;

	public Transform display;

	public ToggleAnimator3D liftAnimator;

	public SpringToTarget rotateSpring;

	[SerializeField]
	[HideInInspector]
	protected int _value;

	private Vector3 _displayLocalPosition;

	public int max => faces.Length;

	public int value
	{
		get
		{
			return _value;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _value, Mathf.Clamp(value, 1, max)))
			{
				_OnValueChange();
			}
		}
	}

	public DiceType diceType { get; set; }

	private static DiceIntView Create(DiceType diceType, Transform parent = null)
	{
		return Object.Instantiate((GameObject)_Dice[diceType]).transform.SetParentAndReturn(parent, worldPositionStays: false).GetComponent<DiceIntView>().SetData(diceType);
	}

	public static DiceIntView Create(int max, Transform parent = null)
	{
		return Create(EnumUtil<DiceType>.Min.GetDiceTypeFromMax(max), parent);
	}

	private void _OnValueChange()
	{
		display.localRotation = faces[value - 1].localRotation.Inverse();
		if (rotateSpring.positionEnabled)
		{
			display.localPosition = _displayLocalPosition - display.localRotation * faces[value - 1].localPosition;
		}
		liftAnimator.isOn = true;
	}

	private void Awake()
	{
		_displayLocalPosition = display.localPosition;
	}

	private void Update()
	{
		if (liftAnimator.isOn && rotateSpring.atTarget)
		{
			liftAnimator.isOn = false;
		}
	}

	public void SetValue(int newValue)
	{
		value = newValue;
	}

	public void SetValue(float newValue)
	{
		value = Mathf.RoundToInt(newValue);
	}

	public DiceIntView SetData(DiceType diceType)
	{
		this.diceType = diceType;
		return this;
	}

	public DiceIntView SetValueReturn(int newValue)
	{
		value = newValue;
		return this;
	}
}
