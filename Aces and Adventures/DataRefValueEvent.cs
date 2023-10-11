using System;
using UnityEngine.Events;

[Serializable]
public class DataRefValueEvent<C> : UnityEvent<C> where C : IDataContent
{
}
