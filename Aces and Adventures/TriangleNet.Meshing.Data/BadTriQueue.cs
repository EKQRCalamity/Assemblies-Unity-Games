using TriangleNet.Geometry;
using TriangleNet.Topology;

namespace TriangleNet.Meshing.Data;

internal class BadTriQueue
{
	private const double SQRT2 = 1.4142135623730951;

	private BadTriangle[] queuefront;

	private BadTriangle[] queuetail;

	private int[] nextnonemptyq;

	private int firstnonemptyq;

	private int count;

	public int Count => count;

	public BadTriQueue()
	{
		queuefront = new BadTriangle[4096];
		queuetail = new BadTriangle[4096];
		nextnonemptyq = new int[4096];
		firstnonemptyq = -1;
		count = 0;
	}

	public void Enqueue(BadTriangle badtri)
	{
		count++;
		double num;
		int num2;
		if (badtri.key >= 1.0)
		{
			num = badtri.key;
			num2 = 1;
		}
		else
		{
			num = 1.0 / badtri.key;
			num2 = 0;
		}
		int num3 = 0;
		while (num > 2.0)
		{
			int num4 = 1;
			double num5 = 0.5;
			while (num * num5 * num5 > 1.0)
			{
				num4 *= 2;
				num5 *= num5;
			}
			num3 += num4;
			num *= num5;
		}
		num3 = 2 * num3 + ((num > 1.4142135623730951) ? 1 : 0);
		int num6 = ((num2 <= 0) ? (2048 + num3) : (2047 - num3));
		if (queuefront[num6] == null)
		{
			if (num6 > firstnonemptyq)
			{
				nextnonemptyq[num6] = firstnonemptyq;
				firstnonemptyq = num6;
			}
			else
			{
				int i;
				for (i = num6 + 1; queuefront[i] == null; i++)
				{
				}
				nextnonemptyq[num6] = nextnonemptyq[i];
				nextnonemptyq[i] = num6;
			}
			queuefront[num6] = badtri;
		}
		else
		{
			queuetail[num6].next = badtri;
		}
		queuetail[num6] = badtri;
		badtri.next = null;
	}

	public void Enqueue(ref Otri enqtri, double minedge, Vertex apex, Vertex org, Vertex dest)
	{
		BadTriangle badTriangle = new BadTriangle();
		badTriangle.poortri = enqtri;
		badTriangle.key = minedge;
		badTriangle.apex = apex;
		badTriangle.org = org;
		badTriangle.dest = dest;
		Enqueue(badTriangle);
	}

	public BadTriangle Dequeue()
	{
		if (firstnonemptyq < 0)
		{
			return null;
		}
		count--;
		BadTriangle badTriangle = queuefront[firstnonemptyq];
		queuefront[firstnonemptyq] = badTriangle.next;
		if (badTriangle == queuetail[firstnonemptyq])
		{
			firstnonemptyq = nextnonemptyq[firstnonemptyq];
		}
		return badTriangle;
	}
}
