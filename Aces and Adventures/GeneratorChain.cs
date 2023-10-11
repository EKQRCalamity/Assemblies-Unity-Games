using System;
using System.Collections;

public struct GeneratorChain<C, T>
{
	private T _data;

	private Func<C, T, IEnumerator> _generationLogic;

	public GeneratorChain(T data, Func<C, T, IEnumerator> generationLogic)
	{
		_data = data;
		_generationLogic = generationLogic;
	}

	public IEnumerator Chain(C content)
	{
		return _generationLogic(content, _data);
	}
}
