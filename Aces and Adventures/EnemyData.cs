using System.ComponentModel;
using ProtoBuf;
using TMPro;
using UnityEngine;

[ProtoContract]
[UIField]
public class EnemyData : CombatantData
{
	[ProtoContract]
	[UIField]
	public class Cosmetic
	{
		public static readonly Ushort2 IMAGE_SIZE = new Ushort2(425, 396);

		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Hide)]
		private CroppedImageRef _image = new CroppedImageRef(ImageCategoryType.Enemy, IMAGE_SIZE);

		public CroppedImageRef image => _image;

		private bool _imageSpecified => _image;
	}

	[ProtoMember(1)]
	[UIField(order = 1000u, collapse = UICollapseType.Hide)]
	[UICategory("Cosmetic")]
	private Cosmetic _cosmetic;

	[ProtoMember(2)]
	[UIField(order = 100u, min = 0, max = 10)]
	[DefaultValue(1)]
	[UICategory("Main")]
	private float _experienceMultiplier = 1f;

	[ProtoMember(3)]
	[UIField(validateOnChange = true)]
	[UICategory("Main")]
	private DataRef<EnemyData> _upgradeOf;

	public Cosmetic cosmetic => _cosmetic ?? (_cosmetic = new Cosmetic());

	public int experience => Mathf.Max(0, Mathf.RoundToInt(GameVariables.Values.main.enemyExperienceMultiplier * _experienceMultiplier * (base.stats.GetExperienceValue() + base.traits.GetExperienceValue())));

	public DataRef<EnemyData> upgradeOf => _upgradeOf;

	public EnemyRank rank
	{
		get
		{
			EnemyData enemyData = this;
			int num = 0;
			while ((bool)enemyData._upgradeOf && ++num > 0)
			{
				enemyData = enemyData._upgradeOf.data;
			}
			return EnumUtil<EnemyRank>.Round(num);
		}
	}

	private bool _hideCopyName
	{
		get
		{
			if ((bool)_upgradeOf)
			{
				return !_nameLocalized;
			}
			return true;
		}
	}

	private bool _upgradeOfSpecified => _upgradeOf.ShouldSerialize();

	[UIField(validateOnChange = true, order = 1u)]
	[UICategory("Main")]
	[UIHideIf("_hideCopyName")]
	private void _CopyNameFromBaseEnemy()
	{
		_nameLocalized.rawText = "{}";
		_nameLocalized.variables.variables.Clear();
		LocalizedStringData.AVariable.LocalizedStringVariable value = new LocalizedStringData.AVariable.LocalizedStringVariable(MessageData.Instance.ability.nameTableEntryId, new LocalizedStringData.VariableGroup
		{
			variables = 
			{
				{
					LocalizedVariableName.Name,
					(LocalizedStringData.AVariable)new LocalizedStringData.AVariable.EnemyNameVariable(_upgradeOf.BaseRef())
				},
				{
					LocalizedVariableName.Level,
					(LocalizedStringData.AVariable)new LocalizedStringData.AVariable.IntVariable((int)rank)
				}
			}
		});
		_nameLocalized.variables[LocalizedVariableName.Var] = value;
	}

	[UIField]
	[UICategory("Main")]
	private void _CreateUpgradedVersionOfThisEnemy()
	{
		ContentRef contentRef = DataRefControl.ActiveControl?.dataRef;
		DataRef<EnemyData> enemyDataRef = contentRef as DataRef<EnemyData>;
		if (enemyDataRef == null || !contentRef.hasSavedContent)
		{
			return;
		}
		UIUtil.CreatePopup("Create Upgrade of [" + enemyDataRef.data.GetTitle() + "] Enemy", UIUtil.CreateMessageBox("Would you like to create an upgraded version of [" + enemyDataRef.data.GetTitle() + "] enemy?", TextAlignmentOptions.Center, 32, 1600), null, parent: DataRefControl.ActiveControl.transform, buttons: new string[2] { "Create", "Cancel" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (!(s != "Create"))
			{
				EnemyData enemyData = ProtoUtil.Clone(enemyDataRef.data);
				enemyData._upgradeOf = enemyDataRef;
				LocalizationUtil.ClearIds(enemyData);
				string title2 = $"{enemyDataRef.BaseRef().name} +{(int)enemyData.rank}";
				enemyData._nameLocalized.rawText = title2;
				DataRef<EnemyData> newEnemyDataRef = new DataRef<EnemyData>(enemyData);
				newEnemyDataRef.SaveFromUIWithoutValidation(enemyData);
				UIUtil.BeginProcessJob(DataRefControl.ActiveControl.transform, null, Department.UI).Then().DoProcess(Job.WaitForDepartment(Department.Content))
					.Then()
					.DoProcess(Job.WaitForOneFrame())
					.Then()
					.Do(delegate
					{
						ReflectionUtil.LocalizeDataRef(newEnemyDataRef, enemyData, title2);
						enemyData._CopyNameFromBaseEnemy();
						newEnemyDataRef.SaveFromUIWithoutValidation(enemyData);
					})
					.Then()
					.DoProcess(Job.WaitForDepartment(Department.Content))
					.Then()
					.Do(UIUtil.EndProcess)
					.Then()
					.Do(delegate
					{
						DataRefControl.ActiveControl.SetDataRef(newEnemyDataRef);
					});
			}
		});
	}
}
