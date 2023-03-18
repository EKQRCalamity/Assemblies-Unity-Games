using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Actionables;

public class ActionableForce : MonoBehaviour, IActionable
{
	public float force = 10f;

	public bool fromPenitent = true;

	public Vector2 forceDirection;

	public Vector2 forceOriginOffset;

	public bool Locked { get; set; }

	[Button(ButtonSizes.Small)]
	public void Use()
	{
		Impact();
	}

	private void Impact(float mult = 1f)
	{
		Vector2 vector = Vector2.one;
		if (fromPenitent && Core.Logic.Penitent != null)
		{
			vector = ((Vector2)base.transform.position - ((Vector2)Core.Logic.Penitent.transform.position + forceOriginOffset)).normalized * force * mult;
		}
		GetComponent<Rigidbody2D>().AddForce(vector, ForceMode2D.Impulse);
	}

	public void HeavyUse()
	{
		Impact(2f);
	}
}
