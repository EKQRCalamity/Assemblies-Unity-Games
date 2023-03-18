using System.Collections;
using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.Physics;

public class EnemyBumper : MonoBehaviour
{
	[SerializeField]
	private BoxCollider2D _enemyBoxCollider;

	private PlatformCharacterController _enemyController;

	private Gameplay.GameControllers.Penitent.Penitent _penitent;

	private IEnumerator bumperCoroutine;

	private bool bumperEnemy;

	[Tooltip("The force applied to the enemy bumper displacement")]
	[Range(0f, 5f)]
	public float bumperForce;

	private int defaultLayerValue;

	private Enemy enemy;

	public bool enemyBumperIsActive;

	public LayerMask enemyLayer;

	private int enemyLayerValue;

	private bool includeEnemyLayer;

	private float widthCollider;

	public bool EnemyBumperIsActive
	{
		get
		{
			return enemyBumperIsActive;
		}
		set
		{
			enemyBumperIsActive = value;
		}
	}

	private void Awake()
	{
		widthCollider = _enemyBoxCollider.bounds.extents.x;
		bumperEnemy = false;
	}

	private void Start()
	{
		_penitent = Core.Logic.Penitent;
		enemy = GetComponentInParent<Enemy>();
		_enemyController = enemy.GetComponentInChildren<PlatformCharacterController>();
		enemyLayerValue = LayerMask.NameToLayer("Enemy");
		defaultLayerValue = LayerMask.NameToLayer("Penitent");
		enemyBumperIsActive = true;
	}

	private void Update()
	{
		if (_enemyController.IsGrounded)
		{
			IncludeEnemyLayer();
		}
		else
		{
			IncludeEnemyLayer(include: false);
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (!(_penitent == null) && (enemyLayer.value & (1 << collision.gameObject.layer)) > 0 && enemy.EnemyFloorChecker().IsGrounded && enemyBumperIsActive && _penitent.Status.IsGrounded && !_penitent.Status.Dead && !_penitent.IsDashing)
		{
			float magnitude = (_penitent.transform.position - base.transform.position).magnitude;
			float num = collision.bounds.extents.x + widthCollider;
			if (magnitude / 1.25f <= num && !bumperEnemy)
			{
				bumperEnemy = true;
				bumperCoroutine = EnemyBumperCoroutine(num);
				StartCoroutine(bumperCoroutine);
			}
		}
	}

	private IEnumerator EnemyBumperCoroutine(float playerWidthCollider)
	{
		if (_penitent == null)
		{
			yield return null;
		}
		float distance = playerWidthCollider + widthCollider;
		Vector2 dir = ((!(_penitent.transform.position.x >= base.transform.position.x)) ? Vector2.right : (-Vector2.right));
		while ((_penitent.transform.position - base.transform.position).magnitude <= distance * 1.25f && _penitent.Status.IsGrounded)
		{
			base.transform.parent.Translate(dir * bumperForce * Time.deltaTime, Space.World);
			yield return new WaitForEndOfFrame();
		}
		bumperEnemy = false;
	}

	public void IncludeEnemyLayer(bool include = true)
	{
		if (include && !includeEnemyLayer)
		{
			includeEnemyLayer = true;
			base.gameObject.layer = enemyLayerValue;
		}
		else if (!include && includeEnemyLayer)
		{
			includeEnemyLayer = false;
			base.gameObject.layer = defaultLayerValue;
		}
	}
}
