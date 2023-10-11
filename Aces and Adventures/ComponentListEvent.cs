using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ComponentListEvent : UnityEvent<List<Component>>
{
}
