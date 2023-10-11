using System.Collections.Generic;
using UnityEngine;

public class FloatRelayer
{
	private struct FloatRelay
	{
		public float startValue;

		public float endValue;

		public float animationTime;

		public float delay;

		private float _elapsedTime;

		public float elapsedTime => _elapsedTime;

		public bool finished
		{
			get
			{
				if (_elapsedTime > 0f)
				{
					return value == endValue;
				}
				return false;
			}
		}

		public float value => Mathf.Lerp(startValue, endValue, MathUtil.CubicSplineInterpolant(_t));

		private float _t => Mathf.Clamp01((_elapsedTime - delay) / animationTime);

		public FloatRelay(float startValue, float endValue, float animationTime, float delay)
		{
			this.startValue = startValue;
			this.endValue = endValue;
			this.animationTime = animationTime;
			this.delay = delay;
			_elapsedTime = 0f;
		}

		public FloatRelay Update(float elapsedTime)
		{
			_elapsedTime += elapsedTime;
			return this;
		}
	}

	private Queue<FloatRelay> _queue = new Queue<FloatRelay>();

	private FloatRelay? _relay;

	public float? value
	{
		get
		{
			if (!_relay.HasValue)
			{
				return null;
			}
			return _relay.Value.value;
		}
	}

	public void Add(float startValue, float endValue, float animationTime, float delay, int? maxQueueSize = null)
	{
		if (maxQueueSize.HasValue)
		{
			ClearQueueToSize(maxQueueSize.Value);
		}
		_queue.Enqueue(new FloatRelay(startValue, endValue, animationTime, delay));
	}

	public void Clear()
	{
		_queue.Clear();
	}

	public void ClearQueueToSize(int size)
	{
		if (size <= 0)
		{
			_queue.Clear();
			return;
		}
		while (_queue.Count > size)
		{
			_queue.Dequeue();
		}
	}

	public bool Update(float elapsed)
	{
		if (_relay.HasValue && _relay.Value.finished)
		{
			_relay = null;
		}
		if (!_relay.HasValue && _queue.Count > 0)
		{
			_relay = _queue.Dequeue();
		}
		if (!_relay.HasValue)
		{
			return false;
		}
		bool num = _relay.Value.elapsedTime == 0f;
		float num2 = _relay.Value.value;
		_relay = _relay.Value.Update(elapsed);
		if (!num)
		{
			return num2 != _relay.Value.value;
		}
		return true;
	}
}
