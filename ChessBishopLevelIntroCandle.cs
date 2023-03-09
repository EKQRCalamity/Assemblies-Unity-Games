using UnityEngine;

public class ChessBishopLevelIntroCandle : MonoBehaviour
{
	public bool moving;

	[SerializeField]
	private GameObject glow;

	[SerializeField]
	private GameObject shadow;

	private void AniEvent_StartMove()
	{
		moving = true;
		glow.SetActive(value: false);
	}

	private void Update()
	{
		shadow.transform.position = new Vector3(shadow.transform.position.x, -40f);
	}
}
