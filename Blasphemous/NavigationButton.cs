using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;

public class NavigationButton : MonoBehaviour
{
	public Vector3 destination;

	public void ButtonPressed()
	{
		NavigationWidget componentInParent = GetComponentInParent<NavigationWidget>();
		if (Core.Logic.Penitent != null)
		{
			Core.Logic.Penitent.Teleport(destination);
		}
		else
		{
			Log.Error("Navigation", "Penitent is null.");
		}
		if (componentInParent != null)
		{
			componentInParent.Show(b: false);
		}
	}
}
