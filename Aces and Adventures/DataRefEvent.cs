using System;
using UnityEngine.Events;

[Serializable]
public class DataRefEvent<C> : UnityEvent<DataRef<C>> where C : IDataContent
{
}
