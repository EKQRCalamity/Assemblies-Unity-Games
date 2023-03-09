using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITextAnimator : AbstractMonoBehaviour
{
	private const int DIFFERENCE = 1;

	[SerializeField]
	private float frameDelay = 0.07f;

	private Text text;

	private TMP_Text tmp_text;

	private string textString;

	private bool useTMP;

	protected override void Awake()
	{
		base.Awake();
		text = GetComponent<Text>();
		if (useTMP = text == null)
		{
			tmp_text = GetComponent<TMP_Text>();
			textString = tmp_text.text;
		}
		else
		{
			textString = text.text;
		}
	}

	private void Start()
	{
		StartCoroutine(anim_cr());
	}

	public void SetString(string s)
	{
		textString = s;
	}

	private IEnumerator anim_cr()
	{
		if (useTMP)
		{
			while (true)
			{
				tmp_text.text = string.Empty;
				for (int i = 0; i < textString.Length; i++)
				{
					TMP_Text tMP_Text = tmp_text;
					string text = tMP_Text.text;
					tMP_Text.text = text + "<size=" + (tmp_text.fontSize + (float)Random.Range(-1, 1)) + ">" + textString[i].ToString() + "</size>";
				}
				yield return new WaitForSeconds(frameDelay);
			}
		}
		while (true)
		{
			this.text.text = string.Empty;
			for (int j = 0; j < textString.Length; j++)
			{
				Text obj = this.text;
				string text = obj.text;
				obj.text = text + "<size=" + (this.text.fontSize + Random.Range(-1, 1)) + ">" + textString[j].ToString() + "</size>";
			}
			yield return new WaitForSeconds(frameDelay);
		}
	}
}
