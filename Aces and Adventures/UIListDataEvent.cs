using System;
using System.Collections.Generic;
using UnityEngine.Events;

[Serializable]
public class UIListDataEvent : UnityEvent<IEnumerable<UIListItemData>>
{
}
