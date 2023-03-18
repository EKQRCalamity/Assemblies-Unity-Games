using System;
using System.Collections.Generic;
using Framework.Util;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.GameControllers.Entities;

[RequireComponent(typeof(Collider2D))]
public class TriggerSensor : MonoBehaviour, ICollisionEmitter
{
	public delegate void ColliderTriggerEvent(Collider2D objectCollider);

	public delegate void EntityTriggerEvent(Entity entity);

	public LayerMask sensorLayerDetection;

	public string targetTag;

	private List<GameObject> objectList = new List<GameObject>();

	private List<Entity> entityList = new List<Entity>();

	public Collider2D SensorCollider2D { get; private set; }

	public bool PlayerInside { get; private set; }

	public bool ObjectsInside => objectList.Count > 0;

	public bool EntitiesInside => entityList.Count > 0;

	public List<GameObject> GetObjectsInside => objectList;

	public List<Entity> GetEntitiesInside => entityList;

	public event ColliderTriggerEvent OnColliderEnter;

	public event ColliderTriggerEvent OnColliderExit;

	public event EntityTriggerEvent OnEntityEnter;

	public event EntityTriggerEvent OnEntityExit;

	public event EventHandler<Collider2DParam> OnEnter;

	public event EventHandler<Collider2DParam> OnStay;

	public event EventHandler<Collider2DParam> OnExit;

	private void Awake()
	{
		SensorCollider2D = GetComponent<Collider2D>();
	}

	private void Reset()
	{
		GetComponent<Collider2D>().isTrigger = true;
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (col.CompareTag("Sensor") || (sensorLayerDetection.value & (1 << col.gameObject.layer)) <= 0 || (!targetTag.IsNullOrWhitespace() && !col.CompareTag(targetTag)))
		{
			return;
		}
		objectList.Add(col.gameObject);
		if (this.OnColliderEnter != null)
		{
			this.OnColliderEnter(col);
		}
		Entity componentInParent = col.GetComponentInParent<Entity>();
		if ((componentInParent != null && col.CompareTag("Penitent")) || col.CompareTag("NPC"))
		{
			entityList.Add(componentInParent);
			if (this.OnEntityEnter != null)
			{
				this.OnEntityEnter(componentInParent);
			}
			if (col.CompareTag("Penitent"))
			{
				PlayerInside = true;
			}
		}
	}

	private void OnTriggerExit2D(Collider2D col)
	{
		if (col.CompareTag("Sensor") || (sensorLayerDetection.value & (1 << col.gameObject.layer)) <= 0)
		{
			return;
		}
		OnTriggerExit2DNotify(col);
		if (!targetTag.IsNullOrWhitespace() && !col.CompareTag(targetTag))
		{
			return;
		}
		objectList.Remove(col.gameObject);
		if (this.OnColliderExit != null)
		{
			this.OnColliderExit(col);
		}
		Entity componentInParent = col.GetComponentInParent<Entity>();
		if ((componentInParent != null && col.CompareTag("Penitent")) || col.CompareTag("NPC"))
		{
			entityList.Remove(componentInParent);
			if (this.OnEntityExit != null)
			{
				this.OnEntityExit(componentInParent);
			}
			if (col.CompareTag("Penitent"))
			{
				PlayerInside = false;
			}
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if ((sensorLayerDetection.value & (1 << other.gameObject.layer)) > 0)
		{
			OnTriggerStay2DNotify(other);
		}
	}

	public void OnTriggerEnter2DNotify(Collider2D c)
	{
		if (this.OnEnter != null)
		{
			this.OnEnter(this, new Collider2DParam
			{
				Collider2DArg = c
			});
		}
	}

	public void OnTriggerStay2DNotify(Collider2D c)
	{
		if (this.OnStay != null)
		{
			this.OnStay(this, new Collider2DParam
			{
				Collider2DArg = c
			});
		}
	}

	public void OnTriggerExit2DNotify(Collider2D c)
	{
		if (this.OnExit != null)
		{
			this.OnExit(this, new Collider2DParam
			{
				Collider2DArg = c
			});
		}
	}
}
