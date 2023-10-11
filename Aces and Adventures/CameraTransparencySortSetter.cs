using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraTransparencySortSetter : MonoBehaviour
{
	[SerializeField]
	protected TransparencySortMode _sortMode;

	public TransparencySortMode sortMode
	{
		get
		{
			return _sortMode;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _sortMode, value))
			{
				GetComponent<Camera>().transparencySortMode = value;
			}
		}
	}

	private void Start()
	{
		GetComponent<Camera>().transparencySortMode = _sortMode;
	}

	private void OnValidate()
	{
		GetComponent<Camera>().transparencySortMode = _sortMode;
	}
}
