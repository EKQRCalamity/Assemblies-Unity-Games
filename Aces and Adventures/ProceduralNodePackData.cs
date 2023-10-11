using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf;
using TMPro;
using UnityEngine;

[ProtoContract]
[UIField]
public class ProceduralNodePackData : IDataContent
{
	[ProtoMember(1)]
	[UIField]
	private string _name;

	[ProtoMember(2)]
	[UIField(collapse = UICollapseType.Hide)]
	private ProceduralNodePack _pack;

	[ProtoMember(3)]
	[UIField(tooltip = "At which part of a procedural adventure should this node pack be placed in.\n<i>For organizational purposes.</i>")]
	private ProceduralPhaseType? _phase;

	[ProtoMember(4)]
	[UIField(min = 1, max = 9, tooltip = "Procedural Level that this node pack should be placed in.\n<i>For organizational purposes.</i>")]
	private int? _level;

	public ProceduralNodePack pack => _pack ?? (_pack = new ProceduralNodePack());

	public bool canUpgradeEncounters
	{
		get
		{
			ProceduralNodePack proceduralNodePack = _pack;
			if (proceduralNodePack == null)
			{
				return false;
			}
			return proceduralNodePack.type?.IsEncounterOrInvernus() == true;
		}
	}

	[ProtoMember(128)]
	public string tags { get; set; }

	private bool _hideUpgradeEncounters => !canUpgradeEncounters;

	public async Task<DataRef<ProceduralNodePackData>> CreateUpgradedEncounterDataRef(bool setActiveDataRefControl)
	{
		ProceduralNodePackData proceduralData = ProtoUtil.Clone(this);
		proceduralData._level++;
		string name = proceduralData._name.Trim();
		proceduralData._name = name;
		string fullTitle = proceduralData.GetTitle();
		DataRef<ProceduralNodePackData> dataRef = DataRef<ProceduralNodePackData>.Search().FirstOrDefault((DataRef<ProceduralNodePackData> d) => d.data.GetTitle() == fullTitle);
		if (dataRef != null)
		{
			Debug.Log("Procedural Pack Data by the name [" + fullTitle + "] already exists, upgrade will NOT be created.");
			return dataRef;
		}
		HashSet<DataRef<ProceduralNodePackData>> traversedPacks = new HashSet<DataRef<ProceduralNodePackData>>();
		await proceduralData._pack.ProcessKeyMappings(async (DataRef<ProceduralNodeData> nodeRef) => (!nodeRef.data.canUpgradeEncounter) ? nodeRef : (await nodeRef.data.CreateUpgradedEncounterDataRef(setActiveDataRefControl: false)), async (DataRef<ProceduralNodePackData> packRef) => (!packRef.data.canUpgradeEncounters) ? packRef : (await packRef.data.CreateUpgradedEncounterDataRef(setActiveDataRefControl: false)), traversedPacks);
		DataRef<ProceduralNodePackData> newProceduralDataRef = new DataRef<ProceduralNodePackData>(proceduralData);
		newProceduralDataRef.SaveFromUIWithoutValidation(proceduralData);
		await UIUtil.BeginProcessJob(DataRefControl.ActiveControl.transform, null, Department.UI).Then().DoProcess(Job.WaitForDepartment(Department.Content))
			.Then()
			.DoProcess(Job.WaitForOneFrame())
			.Then()
			.Do(delegate
			{
				newProceduralDataRef.SaveFromUIWithoutValidation(proceduralData);
			})
			.Then()
			.DoProcess(Job.WaitForDepartment(Department.Content))
			.Then()
			.Do(UIUtil.EndProcess)
			.Then()
			.Do(delegate
			{
				if (setActiveDataRefControl)
				{
					DataRefControl.ActiveControl.SetDataRef(newProceduralDataRef);
				}
			});
		Debug.Log("Created upgraded encounter pack [" + newProceduralDataRef.data.GetTitle() + "]");
		return newProceduralDataRef;
	}

	public string GetTitle()
	{
		ProceduralNodeType? proceduralNodeType = _pack?.type;
		object obj;
		if (proceduralNodeType.HasValue)
		{
			ProceduralNodeType valueOrDefault = proceduralNodeType.GetValueOrDefault();
			obj = EnumUtil.FriendlyName(valueOrDefault) + ".";
		}
		else
		{
			obj = "";
		}
		return (string)obj + (_phase.HasValue ? (EnumUtil.FriendlyName(_phase) + ".") : "") + (_level.HasValue ? $"{_level}." : "") + _name;
	}

	public string GetAutomatedDescription()
	{
		return "";
	}

	public List<string> GetAutomatedTags()
	{
		return null;
	}

	public void PrepareDataForSave()
	{
		_pack?.PrepareDataForSave();
	}

	public string GetSaveErrorMessage()
	{
		if (_name.HasVisibleCharacter())
		{
			return "";
		}
		return "Please enter a name before saving.";
	}

	public void OnLoadValidation()
	{
	}

	[UIField]
	[UIHideIf("_hideUpgradeEncounters")]
	private void _UpgradeEncounters()
	{
		ContentRef contentRef = DataRefControl.ActiveControl?.dataRef;
		DataRef<ProceduralNodePackData> proceduralPackRef = contentRef as DataRef<ProceduralNodePackData>;
		if (proceduralPackRef == null || !contentRef.hasSavedContent)
		{
			return;
		}
		UIUtil.CreatePopup("Create Upgrade of [" + proceduralPackRef.data.GetTitle() + "] Encounter Pack", UIUtil.CreateMessageBox("Would you like to create an upgraded version of [" + proceduralPackRef.data.GetTitle() + "] Encounter Pack?", TextAlignmentOptions.Center, 32, 1600), null, parent: DataRefControl.ActiveControl.transform, buttons: new string[2] { "Create", "Cancel" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: async delegate(string s)
		{
			if (s == "Create")
			{
				await proceduralPackRef.data.CreateUpgradedEncounterDataRef(setActiveDataRefControl: true);
			}
		});
	}

	[UIField]
	private void _DebugProbabilities()
	{
		using PoolWRandomDHandle<ProceduralNodePack.Selection> poolWRandomDHandle = pack.GetWeightedSelections();
		Debug.Log(poolWRandomDHandle.value.ToStringProbabilityDistribution());
	}
}
