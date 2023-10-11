using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf;
using TMPro;
using UnityEngine;

[ProtoContract]
[UIField]
[Localize]
public class ProceduralNodeData : IDataContent
{
	[ProtoMember(1)]
	[UIField(tooltip = "Name of the node.\n<i>For organizational purposes.</i>")]
	private string _name;

	[ProtoMember(2)]
	[UIField(tooltip = "Decides the icon node will use on the map.", validateOnChange = true)]
	private ProceduralNodeType _type;

	[ProtoMember(3)]
	[UIField(tooltip = "At which part of a procedural adventure should this node be placed in.\n<i>For organizational purposes.</i>")]
	private ProceduralPhaseType? _phase;

	[ProtoMember(4)]
	[UIField(min = 1, max = 9, tooltip = "Procedural Level that this node should be placed in.\n<i>For organizational purposes.</i>")]
	private int? _level;

	[ProtoMember(5, OverwriteList = true)]
	[UIField("On Node Selected Instructions", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open, maxCount = 10)]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<AdventureCard.SelectInstruction> _onSelectInstructions;

	[ProtoMember(6, OverwriteList = true)]
	[UIField(maxCount = 0, collapse = UICollapseType.Open)]
	[UIFieldCollectionItem]
	private List<AdventureCard> _cards;

	[ProtoMember(7, OverwriteList = true)]
	[UIField]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	private List<PostProcessGraphInstruction> _postProcessInstructions;

	public ProceduralNodeType type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	public IEnumerable<AdventureCard> cards
	{
		get
		{
			IEnumerable<AdventureCard> enumerable = _cards;
			return enumerable ?? Enumerable.Empty<AdventureCard>();
		}
	}

	public bool hasCards => !_cards.IsNullOrEmpty();

	public IEnumerable<AdventureCard.SelectInstruction> onSelectInstructions
	{
		get
		{
			if (_type.IsEncounterOrInvernus() && (_onSelectInstructions?.OfType<AdventureCard.SelectInstruction.PlayMusic>().None() ?? false))
			{
				yield return new AdventureCard.SelectInstruction.CombatMusic(_type.EncounterDifficulty(), _phase);
			}
			if (_onSelectInstructions != null)
			{
				foreach (AdventureCard.SelectInstruction onSelectInstruction in _onSelectInstructions)
				{
					yield return onSelectInstruction;
				}
			}
			if (_type.IsEncounterOrInvernus() && _cards.OfType<AdventureCard.Encounter>().None())
			{
				yield return new AdventureCard.SelectInstruction.Encounter();
			}
		}
	}

	public bool canUpgradeEncounter
	{
		get
		{
			if (_type.IsEncounterOrInvernus())
			{
				return _cards?.OfType<AdventureCard.Enemy>().Any() ?? false;
			}
			return false;
		}
	}

	[ProtoMember(128)]
	public string tags { get; set; }

	private bool _hideUpgradeEncounter => !canUpgradeEncounter;

	public void GenerateCards(GameState state)
	{
		state.adventureDeck.Add(_cards.GenerateCards(state).Reverse(), AdventureCard.Pile.Draw);
	}

	public async Task<DataRef<ProceduralNodeData>> CreateUpgradedEncounterDataRef(bool setActiveDataRefControl)
	{
		ProceduralNodeData proceduralData = ProtoUtil.Clone(this);
		proceduralData._level++;
		string title = proceduralData._name.Trim();
		if (char.IsNumber(title[title.Length - 1]))
		{
			title = title.ReplaceAtIndex(title.Length - 1, (proceduralData._level - 1).GetValueOrDefault().ToString()[0]);
		}
		else
		{
			title += " +1";
		}
		proceduralData._name = title;
		string fullTitle = proceduralData.GetTitle();
		DataRef<ProceduralNodeData> dataRef = DataRef<ProceduralNodeData>.Search().FirstOrDefault((DataRef<ProceduralNodeData> d) => d.data.GetTitle() == fullTitle);
		if (dataRef != null)
		{
			Debug.Log("Procedural Node Data by the name [" + fullTitle + "] already exists, upgrade will NOT be created.");
			return dataRef;
		}
		for (int i = 0; i < proceduralData._cards.Count; i++)
		{
			AdventureCard adventureCard = proceduralData._cards[i];
			AdventureCard.Enemy enemy = adventureCard as AdventureCard.Enemy;
			if (enemy != null)
			{
				enemy.enemy = DataRef<EnemyData>.Search().FirstOrDefault((DataRef<EnemyData> d) => ContentRef.Equal(d.data.upgradeOf, enemy.enemy)) ?? enemy.enemy;
			}
		}
		LocalizationUtil.ClearIds(proceduralData);
		DataRef<ProceduralNodeData> newProceduralDataRef = new DataRef<ProceduralNodeData>(proceduralData);
		newProceduralDataRef.SaveFromUIWithoutValidation(proceduralData);
		await UIUtil.BeginProcessJob(DataRefControl.ActiveControl.transform, null, Department.UI).Then().DoProcess(Job.WaitForDepartment(Department.Content))
			.Then()
			.DoProcess(Job.WaitForOneFrame())
			.Then()
			.Do(delegate
			{
				ReflectionUtil.LocalizeDataRef(newProceduralDataRef, proceduralData, title);
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
		Debug.Log("Created upgraded encounter node [" + newProceduralDataRef.data.GetTitle() + "]");
		return newProceduralDataRef;
	}

	public void OnGraphGenerated(GameState state)
	{
		if (_cards.IsNullOrEmpty())
		{
			return;
		}
		foreach (AdventureCard card in _cards)
		{
			card.OnGraphGenerated(state);
		}
	}

	public void PostProcessGraph(System.Random random, ProceduralGraph graph, ProceduralNode node)
	{
		if (_postProcessInstructions.IsNullOrEmpty())
		{
			return;
		}
		foreach (PostProcessGraphInstruction postProcessInstruction in _postProcessInstructions)
		{
			postProcessInstruction.PostProcess(random, graph, node);
		}
	}

	public string GetTitle()
	{
		return EnumUtil.FriendlyName(_type) + "/" + (_phase.HasValue ? (EnumUtil.FriendlyName(_phase) + "/") : "") + (_level.HasValue ? $"{_level}/" : "") + _name;
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
	[UIHideIf("_hideUpgradeEncounter")]
	private void _UpgradeEncounter()
	{
		ContentRef contentRef = DataRefControl.ActiveControl?.dataRef;
		DataRef<ProceduralNodeData> proceduralDataRef = contentRef as DataRef<ProceduralNodeData>;
		if (proceduralDataRef == null || !contentRef.hasSavedContent)
		{
			return;
		}
		UIUtil.CreatePopup("Create Upgrade of [" + proceduralDataRef.data.GetTitle() + "] Encounter", UIUtil.CreateMessageBox("Would you like to create an upgraded version of [" + proceduralDataRef.data.GetTitle() + "] Encounter?", TextAlignmentOptions.Center, 32, 1600), null, parent: DataRefControl.ActiveControl.transform, buttons: new string[2] { "Create", "Cancel" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: async delegate(string s)
		{
			if (s == "Create")
			{
				await proceduralDataRef.data.CreateUpgradedEncounterDataRef(setActiveDataRefControl: true);
			}
		});
	}
}
