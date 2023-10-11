using UnityEngine;

public class IgnoreShowCanDrag : MonoBehaviour, IIgnoreShowCanDrag
{
	[SerializeField]
	private bool _ignoreShowCanDrag = true;

	public bool ignoreShowCanDrag
	{
		get
		{
			return _ignoreShowCanDrag;
		}
		set
		{
			_ignoreShowCanDrag = value;
		}
	}

	public bool ShouldIgnoreShowCanDrag()
	{
		if (this.IsActiveAndEnabled())
		{
			return ignoreShowCanDrag;
		}
		return false;
	}
}
