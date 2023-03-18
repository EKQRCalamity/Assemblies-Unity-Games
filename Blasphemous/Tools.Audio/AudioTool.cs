using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Audio;

[ExecuteInEditMode]
[SelectionBase]
public class AudioTool : MonoBehaviour
{
	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[ShowIf("IsEmitter", true)]
	[EventRef]
	protected string trackIdentifier;

	protected bool IsEmitter { get; set; }

	public bool PlayerInsideTrigger { get; private set; }

	protected virtual void BaseAwake()
	{
	}

	protected virtual void BaseStart()
	{
	}

	protected virtual void BaseDestroy()
	{
	}

	protected virtual void BaseUpdate()
	{
	}

	protected virtual void BaseTriggerEnter2D(Collider2D col)
	{
	}

	protected virtual void BaseTriggerExit2D(Collider2D col)
	{
	}

	protected virtual void BaseDrawGizmos()
	{
	}

	private void Awake()
	{
		BaseAwake();
	}

	private void Start()
	{
		BaseStart();
	}

	private void OnDestroy()
	{
		BaseDestroy();
	}

	private void OnDrawGizmos()
	{
		BaseDrawGizmos();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Penitent"))
		{
			PlayerInsideTrigger = true;
			BaseTriggerEnter2D(other);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.CompareTag("Penitent"))
		{
			PlayerInsideTrigger = false;
			BaseTriggerExit2D(other);
		}
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			BaseUpdate();
		}
	}
}
