using System;
using System.Collections;
using System.Collections.Generic;
using BezierSplines;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.BurntFace.Rosary;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BurntFace;

public class BurntFaceRosaryManager : MonoBehaviour
{
	public float speed;

	private float updateCounter;

	public List<BurntFaceRosaryBead> beads;

	public BezierSpline spline;

	public Transform center;

	public BurntFaceRosaryScriptablePattern patternDatabase;

	public GameObject rosaryBeadPrefab;

	public float radiusOffset;

	public int maxBeads = 8;

	public BurntFace owner;

	public float smoothTime = 1.5f;

	public AnimationCurve smoothCurve;

	public GameObject rechargeFX;

	private float _currentTimer;

	private string _currentPattern;

	private float _targetSpeed;

	private float _targetRadius;

	private float _lastRadius;

	private float _lastSpeed;

	private float _smoothChangeCounter;

	private const string DEACTIVATED_ROSARY_PATTERN = "EMPTY";

	public bool isRecharging;

	[FoldoutGroup("Debug", 0)]
	public float debugLastV;

	public event Action<BurntFaceRosaryManager> OnPatternEnded;

	public event Action OnAllBeadsDestroyedEvent;

	private void Awake()
	{
		InstantiateBeads();
	}

	private void Start()
	{
		ConfigureRosary();
		_lastSpeed = speed;
		_lastRadius = radiusOffset;
		_smoothChangeCounter = smoothTime;
		SetPatternFromDatabase("EMPTY");
	}

	private void InstantiateBeads()
	{
		for (int i = 0; i < maxBeads; i++)
		{
			UnityEngine.Object.Instantiate(rosaryBeadPrefab, base.transform);
		}
	}

	private void ConfigureRosary()
	{
		beads = new List<BurntFaceRosaryBead>(GetComponentsInChildren<BurntFaceRosaryBead>());
		float num = 1f / (float)beads.Count;
		for (int i = 0; i < beads.Count; i++)
		{
			Vector3 vector = beads[i].transform.position - center.position;
			beads[i].transform.position = spline.GetPoint(num * (float)i) + vector.normalized * radiusOffset;
			beads[i].Init(this);
		}
	}

	public void OnBeadDestroyed(BurntFaceRosaryBead b)
	{
		if (AllBeadsDestroyed())
		{
			OnAllBeadsDestroyed();
		}
	}

	private void OnAllBeadsDestroyed()
	{
		if (this.OnAllBeadsDestroyedEvent != null)
		{
			this.OnAllBeadsDestroyedEvent();
		}
	}

	public bool AllBeadsDestroyed()
	{
		bool flag = true;
		foreach (BurntFaceRosaryBead bead in beads)
		{
			flag = flag && bead.IsDestroyed();
		}
		return flag;
	}

	public void Clear()
	{
		EndPattern();
	}

	public void SetPatternFromDatabase(string id)
	{
		BurntFaceRosaryPattern pattern = patternDatabase.GetPattern(id);
		_currentPattern = pattern.ID;
		_targetSpeed = pattern.maxSpeed;
		_targetRadius = pattern.radiusOffset;
		_smoothChangeCounter = 0f;
		_currentTimer = pattern.activeTime;
		SetPattern(pattern);
	}

	private void SetPattern(BurntFaceRosaryPattern pattern)
	{
		foreach (BurntFaceRosaryBead bead in beads)
		{
			bead.SetPattern(pattern);
		}
	}

	public void HideBeads()
	{
		foreach (BurntFaceRosaryBead bead in beads)
		{
			bead.Hide();
		}
	}

	public void ShowBeads()
	{
		foreach (BurntFaceRosaryBead bead in beads)
		{
			bead.Show();
		}
	}

	public void Recharge()
	{
		EndPattern();
		StartCoroutine(RechargeCoroutine());
	}

	private IEnumerator RechargeCoroutine()
	{
		isRecharging = true;
		int i = 0;
		while (i < beads.Count)
		{
			PoolManager.Instance.ReuseObject(rechargeFX, beads[i].GetPosition(), Quaternion.identity);
			beads[i].Regenerate();
			i++;
			yield return new WaitForSeconds(1f);
		}
		isRecharging = false;
	}

	public void DestroyAllBeads()
	{
		foreach (BurntFaceRosaryBead bead in beads)
		{
			bead.ForceDestroy();
		}
	}

	public void RegenerateAllBeads()
	{
		foreach (BurntFaceRosaryBead bead in beads)
		{
			bead.Regenerate();
		}
	}

	private void UpdateSmoothChange()
	{
		if (_smoothChangeCounter < smoothTime)
		{
			_smoothChangeCounter += Time.deltaTime;
			if (_smoothChangeCounter > smoothTime)
			{
				_smoothChangeCounter = 1f;
				_lastSpeed = _targetSpeed;
				_lastRadius = _targetRadius;
			}
			float t = smoothCurve.Evaluate(_smoothChangeCounter / smoothTime);
			radiusOffset = Mathf.Lerp(_lastRadius, _targetRadius, t);
			speed = Mathf.Lerp(_lastSpeed, _targetSpeed, t);
		}
	}

	private void UpdateBeads()
	{
		UpdateSmoothChange();
		updateCounter = (updateCounter + Time.deltaTime * speed) % 1f;
		float num = 1f / (float)beads.Count;
		for (int i = 0; i < beads.Count; i++)
		{
			float num2 = (num * (float)i + updateCounter) % 1f;
			if (num2 < 0f)
			{
				num2 = 1f + num2;
			}
			if (i == 0)
			{
				debugLastV = num2;
			}
			BurntFaceRosaryBead burntFaceRosaryBead = beads[i];
			Vector3 vector = burntFaceRosaryBead.transform.position - center.position;
			burntFaceRosaryBead.transform.position = spline.GetPoint(num2) + vector.normalized * radiusOffset;
			burntFaceRosaryBead.SetLaserParentRotation(vector);
			float num3 = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			if (num3 < 0f)
			{
				num3 += 360f;
			}
			burntFaceRosaryBead.UpdateAngle(num3);
		}
	}

	private void UpdateActiveCounter()
	{
		if (!(_currentPattern == "EMPTY"))
		{
			if (_currentTimer > 0f)
			{
				_currentTimer -= Time.deltaTime;
			}
			else if (_currentTimer < 0f)
			{
				_currentTimer = 0f;
				EndPattern();
			}
		}
	}

	private void EndPattern()
	{
		SetPatternFromDatabase("EMPTY");
		if (this.OnPatternEnded != null)
		{
			this.OnPatternEnded(this);
		}
	}

	private void Update()
	{
		UpdateBeads();
		UpdateActiveCounter();
	}
}
