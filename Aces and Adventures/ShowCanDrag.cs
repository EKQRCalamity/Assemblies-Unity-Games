using UnityEngine;

public class ShowCanDrag : MonoBehaviour, IShowCanDrag
{
	[SerializeField]
	private bool _showCanDrag = true;

	public bool showCanDrag
	{
		get
		{
			return _showCanDrag;
		}
		set
		{
			_showCanDrag = value;
		}
	}

	public bool ShouldShowCanDrag()
	{
		if (this.IsActiveAndEnabled())
		{
			return showCanDrag;
		}
		return false;
	}
}
