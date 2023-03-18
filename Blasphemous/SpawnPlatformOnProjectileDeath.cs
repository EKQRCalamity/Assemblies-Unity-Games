using Framework.Managers;
using Gameplay.GameControllers.Enemies.BellGhost;
using UnityEngine;

public class SpawnPlatformOnProjectileDeath : MonoBehaviour
{
	public GameObject platform;

	public LayerMask groundSnapLayerMask;

	public float heightOffset = 0.5f;

	private RaycastHit2D[] results;

	public bool onlyOnGround = true;

	private void Start()
	{
		PoolManager.Instance.CreatePool(platform, 2);
		ProjectileWeapon component = GetComponent<ProjectileWeapon>();
		results = new RaycastHit2D[1];
		component.OnProjectileDeath += P_OnProjectileDeath;
	}

	private void P_OnProjectileDeath(ProjectileWeapon obj)
	{
		bool hit = false;
		Vector2 pointBelow = GetPointBelow(base.transform.position, out hit);
		if (onlyOnGround && hit)
		{
			SpawnPlatform(pointBelow);
		}
	}

	public void SpawnPlatform(Vector2 p)
	{
		PoolManager.Instance.ReuseObject(platform, p, Quaternion.identity);
	}

	private Vector2 GetPointBelow(Vector2 p, out bool hit)
	{
		if (Physics2D.RaycastNonAlloc(p, Vector2.down, results, 100f, groundSnapLayerMask) > 0)
		{
			Debug.DrawLine(p, results[0].point, Color.cyan, 5f);
			hit = true;
			return results[0].point + Vector2.up * heightOffset;
		}
		hit = false;
		return p;
	}
}
