using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public static class UnityEventExtensions
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec__8<T>
	{
		public static readonly _003C_003Ec__8<T> _003C_003E9 = new _003C_003Ec__8<T>();

		public static Action<T> _003C_003E9__8_0;

		internal void _003CSafeInvoke_003Eb__8_0(T t)
		{
		}
	}

	public static readonly ExecuteEvents.EventFunction<IDeepPointerDownHandler> deepPointerDownHandler = Execute;

	public static readonly ExecuteEvents.EventFunction<IDeepPointerUpHandler> deepPointerUpHandler = Execute;

	public static readonly ExecuteEvents.EventFunction<IDeepPointerClickHandler> deepPointerClickHandler = Execute;

	public static readonly ExecuteEvents.EventFunction<IDragThresholdSetter> dragThresholdSetterHandler = Execute;

	public static bool SInvoke<T>(this UnityEvent<T> e, ref T currentValue, T newValue)
	{
		if (ReflectionUtil.SafeEquals(currentValue, newValue))
		{
			return false;
		}
		currentValue = newValue;
		e.Invoke(currentValue);
		return true;
	}

	public static bool CInvoke<T, K>(this UnityEvent<T> e, K currentValue, K newValue) where K : IConvertible
	{
		if (ReflectionUtil.SafeEquals(currentValue, newValue))
		{
			return false;
		}
		e.Invoke((T)Convert.ChangeType(newValue, typeof(T)));
		return true;
	}

	public static bool CInvoke<T, K>(this UnityEvent<T> e, ref K currentValue, K newValue) where K : IConvertible
	{
		if (ReflectionUtil.SafeEquals(currentValue, newValue))
		{
			return false;
		}
		currentValue = newValue;
		e.Invoke((T)Convert.ChangeType(currentValue, typeof(T)));
		return true;
	}

	public static void RInvoke<T>(this UnityEvent<T> e, T value)
	{
		e.Invoke(ReflectionUtil.GetDistinctValue(value));
		e.Invoke(value);
	}

	public static void RCInvoke<T, K>(this UnityEvent<T> e, K value) where K : IConvertible
	{
		e.Invoke((T)Convert.ChangeType(ReflectionUtil.GetDistinctValue(value), typeof(T)));
		e.Invoke((T)Convert.ChangeType(value, typeof(T)));
	}

	public static void AddListener(ref UnityEvent unityEvent, UnityAction action)
	{
		UnityEvent obj = unityEvent ?? new UnityEvent();
		UnityEvent unityEvent2 = obj;
		unityEvent = obj;
		unityEvent2.AddListener(action);
	}

	public static void AddListenerGeneric<E, T>(ref E unityEvent, UnityAction<T> action) where E : UnityEvent<T>, new()
	{
		E obj = unityEvent ?? new E();
		E val = obj;
		unityEvent = obj;
		val.AddListener(action);
	}

	public static void SafeInvoke(ref UnityEvent unityEvent)
	{
		UnityEvent obj = unityEvent ?? new UnityEvent();
		UnityEvent unityEvent2 = obj;
		unityEvent = obj;
		unityEvent2.Invoke();
	}

	public static Action<T> SafeInvoke<T>(this UnityEvent<T> unityEvent)
	{
		Action<T> action;
		if (unityEvent == null)
		{
			action = _003C_003Ec__8<T>._003C_003E9__8_0;
			if (action == null)
			{
				return _003C_003Ec__8<T>._003C_003E9__8_0 = delegate
				{
				};
			}
		}
		else
		{
			action = unityEvent.Invoke;
		}
		return action;
	}

	public static void SafeInvoke<E, T>(ref E unityEvent, T value) where E : UnityEvent<T>, new()
	{
		E obj = unityEvent ?? new E();
		E val = obj;
		unityEvent = obj;
		val.Invoke(value);
	}

	public static void InvokeIfActive<T>(this UnityEvent<T> unityEvent, Behaviour component, T value)
	{
		if ((bool)component && component.isActiveAndEnabled)
		{
			unityEvent.Invoke(value);
		}
	}

	public static void AddListenerUnique<T>(this UnityEvent<T> unityEvent, UnityAction<T> action)
	{
		unityEvent.RemoveListener(action);
		unityEvent.AddListener(action);
	}

	public static void AddListenerUnique(this UnityEvent unityEvent, UnityAction action)
	{
		unityEvent.RemoveListener(action);
		unityEvent.AddListener(action);
	}

	public static void AddSingleFireListener(this UnityEvent unityEvent, UnityAction action)
	{
		UnityAction wrapperAction = null;
		wrapperAction = delegate
		{
			action();
			unityEvent.RemoveListener(wrapperAction);
		};
		unityEvent.AddListener(wrapperAction);
	}

	public static void AddSingleFireListener<T>(this UnityEvent<T> unityEvent, UnityAction<T> action)
	{
		UnityAction<T> wrapperAction = null;
		wrapperAction = delegate(T t)
		{
			action(t);
			unityEvent.RemoveListener(wrapperAction);
		};
		unityEvent.AddListener(wrapperAction);
	}

	public static void InvokeLocalized(this StringEvent stringEvent, Component callingComponent, Func<string> getString)
	{
		if (stringEvent != null && getString != null && (bool)callingComponent)
		{
			stringEvent.Invoke(getString() ?? "");
			callingComponent.gameObject.GetOrAddComponent<StringEventLocalizer>().SetData(stringEvent, getString);
		}
	}

	private static void Execute(IDeepPointerDownHandler handler, BaseEventData eventData)
	{
		handler.OnDeepPointerDown(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
	}

	private static void Execute(IDeepPointerUpHandler handler, BaseEventData eventData)
	{
		handler.OnDeepPointerUp(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
	}

	private static void Execute(IDeepPointerClickHandler handler, BaseEventData eventData)
	{
		handler.OnDeepPointerClick(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
	}

	private static void Execute(IDragThresholdSetter handler, BaseEventData eventData)
	{
		handler.OnSetDragThreshold(ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
	}

	public static void SetDragThreshold(this IDragThresholdSetter dragThresholdSetter, int? dragThreshold)
	{
		EventSystem.current.pixelDragThreshold = dragThreshold ?? InputManager.I.PixelDragThreshold;
	}
}
