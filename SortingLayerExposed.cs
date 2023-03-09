using UnityEngine;

[ExecuteInEditMode]
public sealed class SortingLayerExposed : MonoBehaviour
{
	[SerializeField]
	private string sortingLayerName = "Default";

	[SerializeField]
	private int sortingOrder;

	public void OnValidate()
	{
		Apply();
	}

	public void OnEnable()
	{
		Apply();
	}

	private void Apply()
	{
		MeshRenderer component = base.gameObject.GetComponent<MeshRenderer>();
		component.sortingLayerName = sortingLayerName;
		component.sortingOrder = sortingOrder;
	}
}
