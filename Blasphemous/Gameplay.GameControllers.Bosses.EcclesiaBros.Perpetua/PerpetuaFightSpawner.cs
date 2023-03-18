using UnityEngine;

namespace Gameplay.GameControllers.Bosses.EcclesiaBros.Perpetua;

public class PerpetuaFightSpawner : MonoBehaviour
{
	public GameObject perpetuaPrefab;

	public Perpetua perpetua;

	public Transform initPerpetuaPosition;

	public GameObject debugIcon;

	private bool IsSpawned;

	private void Start()
	{
		LoadPerpetua();
	}

	private void LoadPerpetua()
	{
		perpetua = Object.Instantiate(perpetuaPrefab, base.transform.position, Quaternion.identity).GetComponent<Perpetua>();
		perpetua.transform.SetParent(base.transform.parent);
		perpetua.gameObject.SetActive(value: false);
	}

	public void DismissPerpetua()
	{
		if (IsSpawned && (bool)perpetua)
		{
			perpetua.Behaviour.Death();
		}
	}

	public void SpawnFightInPosition(Vector2 pos)
	{
		IsSpawned = true;
		perpetua.transform.position = new Vector2(pos.x, initPerpetuaPosition.position.y);
		perpetua.gameObject.SetActive(value: true);
		perpetua.PerpetuaPoints = GetComponentInChildren<PerpetuaPoints>();
		debugIcon.SetActive(value: true);
		perpetua.GetComponent<PerpetuaBehaviour>().InitIntro();
	}

	public void SpawnFight()
	{
		perpetua.transform.position = initPerpetuaPosition.position;
		perpetua.gameObject.SetActive(value: true);
		perpetua.PerpetuaPoints = GetComponentInChildren<PerpetuaPoints>();
		debugIcon.SetActive(value: true);
		perpetua.GetComponent<PerpetuaBehaviour>().InitIntro();
	}
}
