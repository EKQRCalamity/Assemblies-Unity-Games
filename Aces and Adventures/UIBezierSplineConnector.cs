using System;
using UnityEngine;

public class UIBezierSplineConnector : MonoBehaviour
{
	[Serializable]
	public class Node
	{
		public Transform transform;

		public AxisType axis;

		public bool negateAxis;

		private PositionDirection _positionDirection = new PositionDirection(Vector3.negativeInfinity, Vector3.negativeInfinity);

		public Vector3 position => _positionDirection.position;

		public Vector3 direction => _positionDirection.direction;

		public PositionDirection positionDirection => _positionDirection;

		public bool IsDirty()
		{
			return SetPropertyUtility.SetStruct(ref _positionDirection, new PositionDirection(transform, axis, negateAxis));
		}

		public Node SetData(Transform transform, AxisType axis = AxisType.X, bool negateAxis = false)
		{
			this.transform = transform;
			this.axis = axis;
			this.negateAxis = negateAxis;
			return this;
		}

		public static implicit operator bool(Node node)
		{
			if (node != null)
			{
				return node.transform;
			}
			return false;
		}
	}

	[SerializeField]
	protected UIBezierSpline _uiBezierSpline;

	[SerializeField]
	protected Node _start;

	[SerializeField]
	protected Node _end;

	[SerializeField]
	[Range(0f, 100f)]
	protected float _collinearCorrection = 1f;

	private Node _self;

	public UIBezierSpline uiBezierSpline
	{
		get
		{
			return this.CacheComponentInChildren(ref _uiBezierSpline);
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _uiBezierSpline, value))
			{
				LateUpdate();
			}
		}
	}

	public Node start => _start ?? (_start = new Node());

	public Node end => _end ?? (_end = new Node());

	private Node self
	{
		get
		{
			Node node = _self;
			if (node == null)
			{
				Node obj = new Node
				{
					transform = base.transform
				};
				Node node2 = obj;
				_self = obj;
				node = node2;
			}
			return node;
		}
	}

	public float collinearCorrection
	{
		get
		{
			return _collinearCorrection;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _collinearCorrection, value))
			{
				_UpdatePoints();
			}
		}
	}

	private void _UpdatePoints()
	{
		if (!uiBezierSpline)
		{
			return;
		}
		using PoolStructListHandle<Vector3> poolStructListHandle = Pools.UseStructList<Vector3>();
		Vector3 vector = end.position - start.position;
		float num = Vector3.Dot(start.direction, vector.normalized);
		poolStructListHandle.Add(start.position);
		if (num > 0f)
		{
			poolStructListHandle.Add(start.position.Lerp(start.position + Vector3.Project(vector, Vector3.Cross(start.direction, uiBezierSpline.transform.forward).normalized), (1f + num) * 0.5f).Lerp(end.position, num * 0.5f));
		}
		else
		{
			float num2 = Vector3.Dot(start.direction, end.direction);
			Vector3 vector2 = (start.position + end.position) * 0.5f;
			Vector3 vector3 = Vector3.Project(vector, start.direction);
			if (collinearCorrection > 0f && MathUtil.PointsAreCollinear(start.position, vector2, end.position, 0.1f / collinearCorrection))
			{
				vector3 -= Vector3.Cross(vector, base.transform.forward).normalized * collinearCorrection;
			}
			poolStructListHandle.Add(vector2 - vector3);
			if (num2 <= 0f)
			{
				poolStructListHandle.Add(vector2);
				poolStructListHandle.Add(vector2 + vector3);
			}
		}
		poolStructListHandle.Add(end.position);
		uiBezierSpline.SetDataWorld(poolStructListHandle);
	}

	private void LateUpdate()
	{
		if ((bool)start && (bool)end)
		{
			bool num = start.IsDirty();
			bool flag = end.IsDirty();
			bool flag2 = self.IsDirty();
			if (num || flag || flag2)
			{
				_UpdatePoints();
			}
		}
	}
}
