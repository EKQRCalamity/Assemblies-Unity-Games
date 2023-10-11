using System;

namespace TriangleNet;

public class Configuration
{
	public Func<IPredicates> Predicates { get; set; }

	public Func<TrianglePool> TrianglePool { get; set; }

	public Configuration()
		: this(() => RobustPredicates.Default, () => new TrianglePool())
	{
	}

	public Configuration(Func<IPredicates> predicates)
		: this(predicates, () => new TrianglePool())
	{
	}

	public Configuration(Func<IPredicates> predicates, Func<TrianglePool> trianglePool)
	{
		Predicates = predicates;
		TrianglePool = trianglePool;
	}
}
