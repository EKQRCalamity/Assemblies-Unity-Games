using UnityEngine;

public class SaltbakerLevelTable : MonoBehaviour
{
	[SerializeField]
	private float skewFactor = 0.02f;

	[SerializeField]
	private MeshFilter tableMeshFilter;

	private Mesh tableMesh;

	private Vector3[] vertices;

	private CupheadLevelCamera cam;

	private void Start()
	{
		GetComponent<MeshRenderer>().sortingOrder = -1;
		tableMesh = tableMeshFilter.mesh;
		vertices = tableMesh.vertices;
		cam = CupheadLevelCamera.Current;
	}

	private void Update()
	{
		float num = Mathf.InverseLerp(cam.Right, cam.Left, cam.transform.position.x) - 0.5f;
		vertices[0].x = -0.5f + num * skewFactor;
		vertices[2].x = 0.5f + num * skewFactor;
		tableMesh.vertices = vertices;
		tableMesh.RecalculateBounds();
	}
}
