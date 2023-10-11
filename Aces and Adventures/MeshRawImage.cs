using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshRawImage : RawImage
{
	private MeshRenderer _meshRenderer;

	private Mesh _mesh;

	private MeshRenderer meshRenderer => this.CacheComponent(ref _meshRenderer);

	private Mesh mesh
	{
		get
		{
			Mesh obj = _mesh;
			if ((object)obj == null)
			{
				Mesh obj2 = GetComponent<MeshFilter>().mesh ?? new Mesh();
				Mesh mesh = obj2;
				_mesh = obj2;
				obj = mesh;
			}
			return obj;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		meshRenderer.enabled = true;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		meshRenderer.enabled = false;
	}

	protected override void UpdateMaterial()
	{
		base.UpdateMaterial();
		if ((bool)m_Material)
		{
			if (m_Material.GetInstanceID() >= 0)
			{
				m_Material = Object.Instantiate(m_Material);
			}
			meshRenderer.material = materialForRendering;
			meshRenderer.sharedMaterial.mainTexture = mainTexture;
		}
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		base.OnPopulateMesh(vh);
		vh.FillMesh(mesh);
		vh.Clear();
	}
}
