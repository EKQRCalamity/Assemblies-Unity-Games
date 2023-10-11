using UnityEngine;

public class ForestMaker : MonoBehaviour
{
	public GameObject m_treePrefab;

	public int m_amount;

	public GameObject m_ground;

	public float m_radiusDistance;

	private void Start()
	{
		if (m_treePrefab == null)
		{
			return;
		}
		m_ground.transform.localScale = new Vector3(m_amount * 10, 1f, (float)(m_amount * 5) * 1.866f);
		for (int i = -m_amount / 2; i <= m_amount / 2; i++)
		{
			for (int j = -m_amount / 2; j <= m_amount / 2; j++)
			{
				if (!(Random.Range(0f, 1f) > 0.5f))
				{
					GameObject obj = Object.Instantiate(m_treePrefab);
					Vector3 zero = Vector3.zero;
					zero.x = ((float)i + (float)j * 0.5f - (float)(int)(((j < 0) ? ((float)j - 1f) : ((float)j)) / 2f)) * 2f * m_radiusDistance;
					zero.z = (float)j * 1.866f * m_radiusDistance;
					obj.transform.position = zero;
					float num = Random.Range(1f, 1.5f);
					obj.transform.localScale = Vector3.one * num;
					obj.transform.Rotate(Random.Range(-10f, 10f), Random.Range(-180f, 180f), Random.Range(-10f, 10f));
				}
			}
		}
	}
}
