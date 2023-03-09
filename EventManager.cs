using System;
using System.Collections.Generic;

public class EventManager
{
	public delegate void EventDelegate<T>(T e) where T : GameEvent;

	private delegate void EventDelegate(GameEvent e);

	private static EventManager _instance;

	private Dictionary<Type, EventDelegate> delegates = new Dictionary<Type, EventDelegate>();

	private Dictionary<Delegate, EventDelegate> delegateLookup = new Dictionary<Delegate, EventDelegate>();

	public static EventManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new EventManager();
			}
			return _instance;
		}
	}

	public void AddListener<T>(EventDelegate<T> del) where T : GameEvent
	{
		if (!delegateLookup.ContainsKey(del))
		{
			EventDelegate eventDelegate = delegate(GameEvent e)
			{
				del((T)e);
			};
			delegateLookup[del] = eventDelegate;
			if (delegates.TryGetValue(typeof(T), out var value))
			{
				value = (delegates[typeof(T)] = (EventDelegate)Delegate.Combine(value, eventDelegate));
			}
			else
			{
				delegates[typeof(T)] = eventDelegate;
			}
		}
	}

	public void RemoveListener<T>(EventDelegate<T> del) where T : GameEvent
	{
		if (!delegateLookup.TryGetValue(del, out var value))
		{
			return;
		}
		if (delegates.TryGetValue(typeof(T), out var value2))
		{
			value2 = (EventDelegate)Delegate.Remove(value2, value);
			if (value2 == null)
			{
				delegates.Remove(typeof(T));
			}
			else
			{
				delegates[typeof(T)] = value2;
			}
		}
		delegateLookup.Remove(del);
	}

	public void Raise(GameEvent e)
	{
		if (delegates.TryGetValue(e.GetType(), out var value))
		{
			value(e);
		}
	}
}
