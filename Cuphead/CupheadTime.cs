using System.Collections;
using System.Collections.Generic;
using GCFreeUtils;
using UnityEngine;

public static class CupheadTime
{
	public enum Layer
	{
		Default,
		Player,
		Enemy,
		UI
	}

	public class DeltaObject
	{
		public float this[Layer layer] => Time.deltaTime * GetLayerSpeed(layer) * GlobalSpeed;

		public static implicit operator float(DeltaObject d)
		{
			return d[Layer.Default] * GlobalSpeed;
		}
	}

	private static readonly DeltaObject delta;

	private static float globalSpeed;

	private static Dictionary<int, float> layers;

	public static GCFreeActionList OnChangedEvent;

	public static DeltaObject Delta => delta;

	public static float GlobalDelta => Time.deltaTime;

	public static float FixedDelta => Time.fixedDeltaTime * GlobalSpeed;

	public static float GlobalSpeed
	{
		get
		{
			return globalSpeed;
		}
		set
		{
			globalSpeed = Mathf.Clamp(value, 0f, 1f);
			OnChanged();
		}
	}

	static CupheadTime()
	{
		globalSpeed = 1f;
		OnChangedEvent = new GCFreeActionList(200, autoResizeable: true);
		delta = new DeltaObject();
		layers = new Dictionary<int, float>();
		Layer[] values = EnumUtils.GetValues<Layer>();
		Layer[] array = values;
		foreach (Layer key in array)
		{
			layers.Add((int)key, 1f);
		}
	}

	public static float GetLayerSpeed(Layer layer)
	{
		return layers[(int)layer];
	}

	public static void SetLayerSpeed(Layer layer, float value)
	{
		layers[(int)layer] = value;
		OnChanged();
	}

	public static void Reset()
	{
		SetAll(1f);
	}

	public static void SetAll(float value)
	{
		GlobalSpeed = value;
		Layer[] values = EnumUtils.GetValues<Layer>();
		foreach (Layer key in values)
		{
			layers[(int)key] = value;
		}
		OnChanged();
	}

	private static void OnChanged()
	{
		if (OnChangedEvent != null)
		{
			OnChangedEvent.Call();
		}
	}

	public static bool IsPaused()
	{
		return GlobalSpeed <= 1E-05f || PauseManager.state == PauseManager.State.Paused;
	}

	public static Coroutine WaitForSeconds(MonoBehaviour m, float time)
	{
		return m.StartCoroutine(waitForSeconds_cr(time, Layer.Default));
	}

	public static Coroutine WaitForSeconds(MonoBehaviour m, float time, Layer layer)
	{
		return m.StartCoroutine(waitForSeconds_cr(time, layer));
	}

	private static IEnumerator waitForSeconds_cr(float time, Layer layer)
	{
		float t = 0f;
		while (t < time)
		{
			t += Delta[layer];
			yield return null;
		}
	}

	public static Coroutine WaitForUnpause(MonoBehaviour m)
	{
		return m.StartCoroutine(waitForUnpause_cr());
	}

	private static IEnumerator waitForUnpause_cr()
	{
		while (GlobalSpeed == 0f)
		{
			yield return null;
		}
	}
}
