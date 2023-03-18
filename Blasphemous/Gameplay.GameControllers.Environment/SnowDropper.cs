using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Environment;

[RequireComponent(typeof(ParticleSystem))]
public class SnowDropper : MonoBehaviour
{
	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private bool isAtPlayerPosition;

	public float maxHeight = 50f;

	private void Awake()
	{
		if (Core.Logic == null)
		{
		}
	}

	private void Start()
	{
		_penitent = Core.Logic.Penitent;
		if (_penitent != null && !isAtPlayerPosition)
		{
			keepPosition(_penitent.transform.position);
		}
	}

	private void LateUpdate()
	{
		if (_penitent != null && !_penitent.Status.Dead)
		{
			keepPosition(_penitent.transform.position);
		}
	}

	protected void keepPosition(Vector3 playerPosition)
	{
		base.transform.position = playerPosition;
	}
}
