using UnityEngine;

public class MeshData
{
	public Vector3[] vertices;

	public Vector3[] normals;

	public Vector4[] tangents;

	public Vector2[] uv;

	public Color[] colors;

	public int[] triangles;

	public int vertexCount => vertices.Length;

	public MeshData(Mesh mesh)
	{
		vertices = mesh.vertices;
		normals = mesh.normals;
		tangents = mesh.tangents;
		uv = mesh.uv;
		colors = mesh.colors;
		triangles = mesh.triangles;
	}

	public MeshData(int[] triangles, Vector3[] vertices, Vector3[] normals = null, Vector2[] uv = null, Color[] colors = null, Vector4[] tangents = null)
	{
		this.triangles = triangles;
		this.vertices = vertices;
		this.normals = normals ?? new Vector3[0];
		this.uv = uv ?? new Vector2[0];
		this.colors = colors ?? new Color[0];
		this.tangents = tangents ?? new Vector4[0];
	}

	public Bounds GetBounds()
	{
		if (vertices.IsNullOrEmpty())
		{
			return default(Bounds);
		}
		Bounds result = new Bounds(vertices[0], Vector3.zero);
		for (int i = 1; i < vertices.Length; i++)
		{
			result.Encapsulate(vertices[i]);
		}
		return result;
	}

	public void Transform(Vector3 translation, Quaternion rotation, Vector3 scale, Vector3? axisFlips = null, bool recalculateNormals = true)
	{
		axisFlips = axisFlips ?? Vector3.one;
		if (axisFlips.Value.CombinedSign() < 0)
		{
			InvertFaces();
		}
		scale = axisFlips.Value.Multiply(scale);
		if (vertices != null)
		{
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = (rotation * (vertices[i] + translation)).Multiply(scale);
			}
		}
		if (normals != null && recalculateNormals)
		{
			CalculateNormals(0f, tangents != null);
		}
	}

	public void Scale(Vector3 scale)
	{
		if (vertices != null)
		{
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = vertices[i].Multiply(scale);
			}
		}
	}

	public void InvertFaces()
	{
		if (triangles != null)
		{
			triangles.InvertFaces();
		}
	}

	public void CalculateNormals(float angleThreshold = 0f, bool generateTangents = true)
	{
		normals = ((angleThreshold < MathUtil.BigEpsilon) ? NormalSolver.CalculateNormalsSimple(vertices, triangles) : NormalSolver.CalculateNormals(vertices, triangles, angleThreshold));
		if (generateTangents)
		{
			_CalculateTangents();
		}
	}

	public void CopyToMesh(Mesh mesh)
	{
		mesh.vertices = vertices;
		mesh.normals = ((normals.Length == vertices.Length) ? normals : null);
		mesh.tangents = tangents;
		mesh.uv = uv;
		mesh.colors = colors;
		mesh.triangles = triangles;
	}

	public Mesh ToMesh()
	{
		Mesh mesh = new Mesh();
		CopyToMesh(mesh);
		return mesh;
	}

	private void _CalculateTangents()
	{
		tangents = NormalSolver.CalculateTangents(vertices, normals, uv, triangles);
	}
}
