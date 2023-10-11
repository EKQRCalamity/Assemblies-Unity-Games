using System;
using UnityEngine.Events;

[Serializable]
public abstract class PresetEvent<T> : UnityEvent<uint, T>
{
}
