using System.Collections.Generic;
using UnityEngine;

public class ToggleRequest : MonoBehaviour
{
	public bool requestedValue = true;

	[SerializeField]
	protected BoolEvent _onChange;

	private HashSet<object> _requests;

	private HashSet<object> requests => _requests ?? (_requests = new HashSet<object>(ReferenceEqualityComparer<object>.Default));

	public BoolEvent onChange => _onChange ?? (_onChange = new BoolEvent());

	public void Add(object request)
	{
		if (requests.Add(request) && requests.Count == 1)
		{
			onChange.Invoke(requestedValue);
		}
	}

	public void Remove(object request)
	{
		if (requests.Remove(request) && requests.Count == 0)
		{
			onChange.Invoke(!requestedValue);
		}
	}

	public void Request(object request, bool add)
	{
		if (add)
		{
			Add(request);
		}
		else
		{
			Remove(request);
		}
	}
}
