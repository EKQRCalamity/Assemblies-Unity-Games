using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Framework.Util;

public class PenitentOnElevatorFix : MonoBehaviour
{
	public GameObject FakeTPO;

	public void OnLeverActivation()
	{
		Penitent penitent = Core.Logic.Penitent;
		penitent.SpriteRenderer.enabled = false;
		penitent.DamageArea.enabled = false;
		penitent.Physics.EnablePhysics(enable: false);
		FakeTPO.SetActive(value: true);
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		component.flipX = penitent.SpriteRenderer.flipX;
	}

	public void OnDestinationReached()
	{
		FakeTPO.SetActive(value: false);
		Core.Logic.Penitent.Teleport(FakeTPO.transform.position);
		Penitent penitent = Core.Logic.Penitent;
		penitent.SpriteRenderer.enabled = true;
		penitent.DamageArea.enabled = true;
		penitent.Physics.EnablePhysics();
	}
}
