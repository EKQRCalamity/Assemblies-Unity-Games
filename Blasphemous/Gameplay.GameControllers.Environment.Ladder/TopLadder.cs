using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.Ladder;

[RequireComponent(typeof(BoxCollider2D))]
public class TopLadder : MonoBehaviour
{
	public LayerMask TargetLayerMask;

	public Gameplay.GameControllers.Penitent.Penitent Penitent { get; private set; }

	public bool TargetInside { get; private set; }

	private Collider2D Collider2D { get; set; }

	private void Start()
	{
		Penitent = Core.Logic.Penitent;
		Collider2D = GetComponent<BoxCollider2D>();
	}

	private void OnTriggerEnter2D(Collider2D target)
	{
		if ((TargetLayerMask.value & (1 << target.gameObject.layer)) > 0)
		{
			TargetInside = true;
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if ((TargetLayerMask.value & (1 << other.gameObject.layer)) > 0)
		{
			TargetInside = false;
		}
	}

	private void Update()
	{
		if (TargetInside && Penitent.Status.IsGrounded && Penitent.PlatformCharacterInput.isJoystickDown)
		{
			Vector3 position = new Vector3(Collider2D.bounds.center.x, Penitent.transform.position.y, Penitent.transform.position.z);
			Penitent.transform.position = position;
		}
	}
}
