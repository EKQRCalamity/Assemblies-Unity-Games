namespace TriangleNet.Topology.DCEL;

public class HalfEdge
{
	internal int id;

	internal int mark;

	internal Vertex origin;

	internal Face face;

	internal HalfEdge twin;

	internal HalfEdge next;

	public int ID
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public int Boundary
	{
		get
		{
			return mark;
		}
		set
		{
			mark = value;
		}
	}

	public Vertex Origin
	{
		get
		{
			return origin;
		}
		set
		{
			origin = value;
		}
	}

	public Face Face
	{
		get
		{
			return face;
		}
		set
		{
			face = value;
		}
	}

	public HalfEdge Twin
	{
		get
		{
			return twin;
		}
		set
		{
			twin = value;
		}
	}

	public HalfEdge Next
	{
		get
		{
			return next;
		}
		set
		{
			next = value;
		}
	}

	public HalfEdge(Vertex origin)
	{
		this.origin = origin;
	}

	public HalfEdge(Vertex origin, Face face)
	{
		this.origin = origin;
		this.face = face;
		if (face != null && face.edge == null)
		{
			face.edge = this;
		}
	}

	public override string ToString()
	{
		return $"HE-ID {id} (Origin = VID-{origin.id})";
	}
}
