using Gameplay.GameControllers.Bosses.PietyMonster.ThornProjectile;
using Gameplay.GameControllers.Enemies.Framework.Attack;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PietyMonster.Attack;

public class PietyBushManager : MonoBehaviour
{
	public GameObject PietyBushPrefab;

	public PietyMonster PietyMonster { get; set; }

	public BoxCollider2D Collider { get; set; }

	public int BushDamage { get; set; }

	private void Start()
	{
		Collider = GetComponent<BoxCollider2D>();
		if (PietyMonster == null)
		{
			Debug.Log("A Piety Monster prefab is needed");
		}
		if (PietyBushPrefab == null)
		{
			Debug.LogError("A Piety Monster's bush prefab is needed");
		}
	}

	private void Update()
	{
		if (!(PietyMonster != null))
		{
			PietyMonster = Object.FindObjectOfType<PietyMonster>();
		}
	}

	public void DestroyBushes()
	{
		PietyBush[] array = Object.FindObjectsOfType<PietyBush>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DestroyBush();
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.gameObject.CompareTag("NPC"))
		{
			return;
		}
		Gameplay.GameControllers.Bosses.PietyMonster.ThornProjectile.ThornProjectile componentInChildren = other.transform.root.GetComponentInChildren<Gameplay.GameControllers.Bosses.PietyMonster.ThornProjectile.ThornProjectile>();
		if (!componentInChildren)
		{
			return;
		}
		float x = componentInChildren.transform.position.x;
		componentInChildren.HitsOnFloor();
		Vector2 vector = new Vector2(x, Collider.bounds.max.y);
		if (!(PietyBushPrefab == null))
		{
			GameObject gameObject = Object.Instantiate(PietyBushPrefab, vector, Quaternion.identity);
			PietyBush component = gameObject.GetComponent<PietyBush>();
			if ((bool)component)
			{
				component.SetOwner(PietyMonster);
				component.GetComponent<IDirectAttack>().SetDamage(BushDamage);
			}
		}
	}
}
