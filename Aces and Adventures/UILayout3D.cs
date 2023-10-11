using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UILayout3D<T> : AUILayout3D where T : class
{
	public struct QueueData
	{
		public readonly T data;

		public readonly short index;

		public readonly SimpleListDifferenceType differenceType;

		public QueueData(T data, short index, SimpleListDifferenceType differenceType)
		{
			this.data = data;
			this.index = index;
			this.differenceType = differenceType;
		}

		public override string ToString()
		{
			return $"Data: {data}, Index: {index}, DifferenceType: {differenceType}";
		}
	}

	private List<T> _activeData = new List<T>();

	private Queue<QueueData> _differenceQueue = new Queue<QueueData>();

	private Predicate<T> _IsValidPredicate;

	private Predicate<T> IsValidPredicate => _IsValidPredicate ?? (_IsValidPredicate = _IsValid);

	public event Action<List<T>> onActiveDataChange;

	public event Action<T> onDataAddedToLayout;

	public event Action<T> onDataRemovedFromLayout;

	public event Action<T> onTransferIntoFull;

	public event Action<T> onAddObject;

	protected override void _RegisterSlots()
	{
		_activeData.FillToCapacityWithDefault(slots.Count);
		for (int i = 0; i < slots.Count; i++)
		{
			int index = i;
			slots[i].OnSlottedObjectChanged.AddListener(delegate(GameObject go)
			{
				_SetSlotData(index, GetDataFromGameObject(go));
			});
			slots[i].OnObjectTransferredFromExternalLayout.AddListener(_OnObjectTransferredFromExternalLayout);
			slots[i].OnSlottedObjectTransferredToExternalLayout.AddListener(_OnSlottedObjectTransferredToExternalLayout);
			slots[i].OnObjectDroppedWithNoReceiver.AddListener(delegate(GameObject go)
			{
				_TransferToEmptyDropLayout(slots[index], go);
			});
			slots[i].OnObjectReachedRest.AddListener(base._OnObjectReachedRest);
			slots[i].OnObjectPointerOver.AddListener(base._OnObjectPointerOver);
			slots[i].OnObjectPointerDown.AddListener(base._OnObjectPointerDown);
			slots[i].OnObjectPointerClick.AddListener(base._OnObjectPointerClick);
			slots[i].OnObjectBeginDrag.AddListener(base._OnObjectBeginDrag);
		}
	}

	private bool _RejectDataDueToUniqueConstraint(T data, GameObject go = null)
	{
		if (!forceUniqueDisplayOfData)
		{
			return false;
		}
		if (!_activeData.Contains(data))
		{
			return false;
		}
		if (!go)
		{
			return true;
		}
		UILayout3DSlot componentInParent = go.GetComponentInParent<UILayout3DSlot>();
		bool num = _activeData.ContainsAtIndexOtherThan(data, slots.IndexOf(componentInParent));
		if (num)
		{
			componentInParent.SetObject(null);
		}
		return num;
	}

	private void _OnObjectTransferredFromExternalLayout(GameObject go, AUILayout3D otherLayout)
	{
		T dataKeyFromGameObject = GetDataKeyFromGameObject(go);
		if (!_RejectDataDueToUniqueConstraint(dataKeyFromGameObject, go))
		{
			if (this.onDataAddedToLayout != null)
			{
				this.onDataAddedToLayout(dataKeyFromGameObject);
			}
			if (otherLayout.autoRefill)
			{
				otherLayout.AddObject(dataKeyFromGameObject);
			}
		}
	}

	private void _OnSlottedObjectTransferredToExternalLayout(GameObject go, AUILayout3D otherLayout)
	{
		T dataKeyFromGameObject = GetDataKeyFromGameObject(go);
		if (this.onDataRemovedFromLayout != null)
		{
			this.onDataRemovedFromLayout(dataKeyFromGameObject);
		}
		if (autoRefill)
		{
			AddObject(dataKeyFromGameObject);
		}
	}

	protected override void _SignalOnTransferToFull(GameObject go)
	{
		_SignalOnTransferToFull(GetDataFromGameObject(go));
	}

	private void _SignalOnTransferToFull(T data)
	{
		if (this.onTransferIntoFull != null)
		{
			this.onTransferIntoFull(data);
		}
	}

	private void _SetSlotData(int index, T data)
	{
		if (ReflectionUtil.SafeEquals(_activeData[index], data))
		{
			return;
		}
		_activeData[index] = data;
		base.OnDataChanged.Invoke();
		if (this.onActiveDataChange == null)
		{
			return;
		}
		using PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList(_activeData);
		this.onActiveDataChange(poolKeepItemListHandle);
	}

	protected abstract bool _IsValid(T data);

	private IEnumerable<T> _InsertData(T data)
	{
		bool hasBeenInserted = false;
		foreach (T activeDatum in GetActiveData(includeInvalid: true))
		{
			T val;
			if (!hasBeenInserted)
			{
				bool flag;
				hasBeenInserted = (flag = !_IsValid(activeDatum));
				if (flag)
				{
					val = data;
					goto IL_007b;
				}
			}
			val = activeDatum;
			goto IL_007b;
			IL_007b:
			yield return val;
		}
	}

	private bool _AutoFinishSetObjects(List<T> objects)
	{
		if (_setObjectsCalled)
		{
			return false;
		}
		_setObjectsCalled = true;
		if (animateFirstSetObjects)
		{
			return false;
		}
		_SetObjectsImmediate(objects);
		return true;
	}

	private void _SetObjectsImmediate(List<T> objects)
	{
		for (int i = 0; i < objects.Count; i++)
		{
			slots[i].SetObjectImmediate(_IsValid(objects[i]) ? GenerateViewFromData(objects[i]) : null);
		}
	}

	private void Update()
	{
		if (_differenceQueue.Count > 0)
		{
			_elapsedQueueTime += GameUtil.GetDeltaTime(useScaledTime);
		}
		while (_elapsedQueueTime >= listDifferenceQueueTime && _differenceQueue.Count > 0)
		{
			QueueData queueData = _differenceQueue.Dequeue();
			slots[queueData.index].SetObject((queueData.differenceType != 0 && _IsValid(queueData.data)) ? GenerateViewFromData(queueData.data) : null);
			_elapsedQueueTime -= listDifferenceQueueTime;
		}
		if (SetPropertyUtility.SetStruct(ref _hadItems, _activeData.Find(IsValidPredicate) != null))
		{
			base.OnHasItemsChanged.Invoke(_hadItems.Value);
		}
	}

	public void SetObjects(IEnumerable<T> objects, IEqualityComparer<T> equalityComparer = null)
	{
		using PoolKeepItemListHandle<T> poolKeepItemListHandle = Pools.UseKeepItemList((forceUniqueDisplayOfData ? objects.Distinct() : objects).Take(base.capacity));
		_differenceQueue.Clear();
		if (_AutoFinishSetObjects(poolKeepItemListHandle))
		{
			return;
		}
		foreach (SimpleListDifferenceData<T> simpleDifference in _activeData.GetSimpleDifferences(poolKeepItemListHandle, equalityComparer))
		{
			_differenceQueue.Enqueue(new QueueData(simpleDifference.data, (short)simpleDifference.index, simpleDifference.type));
		}
	}

	public void AddObject(T obj)
	{
		if (_RejectDataDueToUniqueConstraint(obj))
		{
			return;
		}
		if (base.isFull)
		{
			_SignalOnTransferToFull(obj);
			return;
		}
		SetObjects(_InsertData(obj));
		if (this.onAddObject != null)
		{
			this.onAddObject(obj);
		}
	}

	public override void AddObject(object obj)
	{
		if (obj is T)
		{
			AddObject((T)obj);
		}
	}

	public abstract GameObject GenerateViewFromData(T data);

	public abstract T GetDataFromGameObject(GameObject gameObject);

	public virtual T GetDataKeyFromGameObject(GameObject gameObject)
	{
		return GetDataFromGameObject(gameObject);
	}

	public IEnumerable<T> GetActiveData(bool includeInvalid)
	{
		foreach (T item in _activeData.EnumerateSafe())
		{
			if (includeInvalid || _IsValid(item))
			{
				yield return item;
			}
		}
	}

	public override void Clear()
	{
		foreach (UILayout3DSlot slot in slots)
		{
			slot.Clear();
		}
		_differenceQueue.Clear();
		_activeData.SetItemsToDefault();
	}
}
