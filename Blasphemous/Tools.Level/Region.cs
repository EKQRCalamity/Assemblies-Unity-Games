using Framework.Managers;
using UnityEngine;

namespace Tools.Level;

[RequireComponent(typeof(Collider2D))]
public class Region : MonoBehaviour
{
	private Collider2D regionCollider;

	public int EntitiesInside { get; private set; }

	public static event Core.RegionEvent OnRegionEnter;

	public static event Core.RegionEvent OnRegionExit;

	private void Start()
	{
		regionCollider = GetComponent<Collider2D>();
		regionCollider.isTrigger = true;
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (Region.OnRegionEnter != null)
		{
			Region.OnRegionEnter(this);
		}
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		if (Region.OnRegionExit != null)
		{
			Region.OnRegionExit(this);
		}
	}
}
