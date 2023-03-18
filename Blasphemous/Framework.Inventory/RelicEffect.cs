using UnityEngine;

namespace Framework.Inventory;

[RequireComponent(typeof(Relic))]
public class RelicEffect : MonoBehaviour
{
	protected Relic Relic { get; private set; }

	private void Awake()
	{
		Relic = GetComponent<Relic>();
		OnAwake();
	}

	private void Start()
	{
		OnStart();
	}

	private void Update()
	{
		OnUpdate();
	}

	public void OnEquipInventoryObject()
	{
		OnEquipEffect();
	}

	public void OnUnEquipInventoryObject()
	{
		OnUnEquipEffect();
	}

	protected virtual void OnAwake()
	{
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	public virtual void OnEquipEffect()
	{
	}

	public virtual void OnUnEquipEffect()
	{
	}
}
