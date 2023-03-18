using System.Collections.Generic;
using Framework.Managers;
using Tools.Level.Interactables;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

[RequireComponent(typeof(Collider2D))]
public class CollisionSensor : MonoBehaviour
{
	public delegate void CollisionEvent(Collider2D objectCollider);

	public delegate void EntityCollisionEvent(Entity entity);

	public LayerMask sensorLayerDetection = 32;

	private List<GameObject> objectList = new List<GameObject>();

	private List<Entity> entityList = new List<Entity>();

	public Collider2D SensorCollider2D { get; private set; }

	public event CollisionEvent SensorTriggerEnter;

	public event CollisionEvent SensorTriggerExit;

	public event CollisionEvent SensorTriggerStay;

	public event CollisionEvent OnColliderEnter;

	public event CollisionEvent OnColliderExit;

	public event EntityCollisionEvent OnEntityEnter;

	public event EntityCollisionEvent OnEntityExit;

	public event Core.SimpleEvent OnPenitentEnter;

	public event Core.SimpleEvent OnPenitentExit;

	private void Awake()
	{
		SensorCollider2D = GetComponent<Collider2D>();
		if (GetComponentInParent<Door>() != null)
		{
			BoxCollider2D boxCollider2D = (BoxCollider2D)SensorCollider2D;
			if (boxCollider2D.size.y > boxCollider2D.size.x)
			{
				boxCollider2D.size = new Vector2((!(boxCollider2D.size.x < 0.75f)) ? boxCollider2D.size.x : 0.75f, 4f);
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if ((sensorLayerDetection.value & (1 << col.gameObject.layer)) > 0)
		{
			objectList.Add(col.gameObject);
			if (this.SensorTriggerEnter != null && !col.CompareTag("Sensor"))
			{
				this.SensorTriggerEnter(col);
			}
			if (this.OnPenitentEnter != null && col.CompareTag("Penitent"))
			{
				this.OnPenitentEnter();
			}
			Entity componentInParent = col.GetComponentInParent<Entity>();
			if (componentInParent != null && !col.CompareTag("Sensor"))
			{
				entityList.Add(componentInParent);
			}
			if (componentInParent != null && this.OnEntityEnter != null && !col.CompareTag("Sensor"))
			{
				this.OnEntityEnter(componentInParent);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		if ((sensorLayerDetection.value & (1 << col.gameObject.layer)) > 0)
		{
			objectList.Remove(col.gameObject);
			if (this.SensorTriggerExit != null)
			{
				this.SensorTriggerExit(col);
			}
			if (this.OnPenitentExit != null && col.CompareTag("Penitent"))
			{
				this.OnPenitentExit();
			}
			Entity componentInParent = col.GetComponentInParent<Entity>();
			if (componentInParent != null)
			{
				entityList.Remove(componentInParent);
			}
			if (componentInParent != null && this.OnEntityExit != null)
			{
				this.OnEntityExit(componentInParent);
			}
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if ((sensorLayerDetection.value & (1 << other.gameObject.layer)) > 0 && this.SensorTriggerStay != null)
		{
			this.SensorTriggerStay(other);
		}
	}

	public bool IsColliding()
	{
		return objectList.Count > 0;
	}

	public Entity[] GetTouchedEntities()
	{
		return entityList.ToArray();
	}

	private void OnDisable()
	{
		objectList.Clear();
		entityList.Clear();
	}
}
