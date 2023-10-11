using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class AUILayout3D : MonoBehaviour
{
	private static System.Random _random;

	[Header("Set Object Options")]
	public bool animateFirstSetObjects;

	public bool forceUniqueDisplayOfData;

	public bool autoRefill;

	[Header("Empty Drop Logic")]
	public AUILayout3D layoutToTransferToOnEmptyDrop;

	public bool removeIfLayoutToTransferToIsFull;

	[Header("Slots")]
	public List<UILayout3DSlot> slots;

	public bool copyDataFromFirstSlot = true;

	[Range(0f, 1f)]
	public float listDifferenceQueueTime = 0.1f;

	public Collider constrainDragWithinCollider;

	public bool useScaledTime;

	[Header("Events")]
	[SerializeField]
	protected UnityEvent _OnDataChanged;

	[SerializeField]
	protected BoolEvent _OnHasItemsChanged;

	[Header("Sound")]
	[SerializeField]
	protected UILayout3DSoundPack _soundPack;

	[Range(0f, 100f)]
	public float onRestReachedVolumeVelocityMax = 2f;

	[Range(0f, 10f)]
	public float onRestReachedVolumeVelocityPower = 2f;

	[Range(0.01f, 1f)]
	public float onClickVolumeTimeMax = 0.25f;

	[Range(0f, 3f)]
	public float onClickVolumeTimePower = 1f;

	protected float _elapsedQueueTime;

	protected bool? _hadItems;

	protected bool _setObjectsCalled;

	protected static System.Random _Random => _random ?? (_random = new System.Random());

	public int capacity => slots.Count;

	public bool isFull => !slots.Any((UILayout3DSlot slot) => slot.isOpen);

	public UnityEvent OnDataChanged => _OnDataChanged ?? (_OnDataChanged = new UnityEvent());

	public BoolEvent OnHasItemsChanged => _OnHasItemsChanged ?? (_OnHasItemsChanged = new BoolEvent());

	protected UILayout3DSoundPack soundPack => _soundPack ?? (_soundPack = ScriptableObject.CreateInstance<UILayout3DSoundPack>());

	public UILayout3DSlot firstSlot => slots[0];

	private void _CopySlotDataFromFirstSlot()
	{
		for (int i = 1; i < slots.Count; i++)
		{
			slots[i].CopyFrom(slots[0]);
		}
	}

	private UILayout3DSlot _GetFirstOpenSlot()
	{
		foreach (UILayout3DSlot slot in slots)
		{
			if (slot.isOpen)
			{
				return slot;
			}
		}
		return null;
	}

	protected void _TransferToEmptyDropLayout(UILayout3DSlot slot, GameObject go)
	{
		if (!layoutToTransferToOnEmptyDrop)
		{
			return;
		}
		UILayout3DSlot uILayout3DSlot = layoutToTransferToOnEmptyDrop._GetFirstOpenSlot();
		if ((bool)uILayout3DSlot)
		{
			uILayout3DSlot.SimulateOnDrop(go);
			return;
		}
		if (removeIfLayoutToTransferToIsFull)
		{
			slot.SetObject(null);
		}
		layoutToTransferToOnEmptyDrop._SignalOnTransferToFull(go);
	}

	protected void _OnObjectReachedRest(GameObject go)
	{
		soundPack.onRest.Play(go.transform, _Random, soundPack.mixerGroup, loop: false, 0f, 1f, 0f, 50f, 128, 0f, Mathf.Pow(Mathf.Clamp01(go.GetComponentInParent<UILayout3DSlot>().speed / onRestReachedVolumeVelocityMax.InsureNonZero()), onRestReachedVolumeVelocityPower));
	}

	protected void _OnObjectPointerOver(GameObject go)
	{
		soundPack.onPointerOver.Play(go.transform, _Random, soundPack.mixerGroup);
	}

	protected void _OnObjectPointerDown(GameObject go)
	{
		soundPack.onPointerDown.Play(go.transform, _Random, soundPack.mixerGroup);
	}

	protected void _OnObjectPointerClick(PointerEventData eventData)
	{
		soundPack.onPointerClick.Play(eventData.pointerCurrentRaycast.worldPosition, _Random, soundPack.mixerGroup, loop: false, 0f, 1f, 0f, 50f, 128, 0f, Mathf.Pow(Mathf.Clamp01(eventData.ClickHeldTime() / onClickVolumeTimeMax), onClickVolumeTimePower));
	}

	protected void _OnObjectBeginDrag(GameObject go)
	{
		soundPack.onBeginDrag.Play(go.transform, _Random, soundPack.mixerGroup);
	}

	protected virtual void _RegisterSlots()
	{
	}

	protected virtual void _SignalOnTransferToFull(GameObject go)
	{
	}

	public abstract void AddObject(object obj);

	protected virtual void Awake()
	{
		_RegisterSlots();
	}

	protected virtual void OnEnable()
	{
		if (copyDataFromFirstSlot)
		{
			_CopySlotDataFromFirstSlot();
		}
		if (!constrainDragWithinCollider)
		{
			return;
		}
		constrainDragWithinCollider.isTrigger = true;
		constrainDragWithinCollider.gameObject.layer = 2;
		foreach (UILayout3DSlot slot in slots)
		{
			slot.constrainDragWithinCollider = constrainDragWithinCollider;
		}
	}

	public void SetSlots(IEnumerable<UILayout3DSlot> newSlots)
	{
		slots = newSlots.ToList();
		_RegisterSlots();
	}

	public void SetDragEnabled(bool dragEnabled)
	{
		foreach (UILayout3DSlot slot in slots)
		{
			slot.SetDragEnabled(dragEnabled);
		}
	}

	public abstract void Clear();
}
