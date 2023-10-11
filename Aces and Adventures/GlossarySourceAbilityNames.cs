using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

[Serializable]
public class GlossarySourceAbilityNames : GlossarySource
{
	public override async IAsyncEnumerable<Term> GetTermsAsync(Locale locale)
	{
		List<DataRef<AbilityData>> abilitiesToAddToGlossary = new List<DataRef<AbilityData>>(from a in DataRef<AbilityData>.Search()
			where a.data.type.IsTrait() || (a.data.characterClass.HasValue && !a.data.upgradeOf)
			select a);
		HashSet<LocalizedStringData.TableEntryId> idsToTranslateHash = new HashSet<LocalizedStringData.TableEntryId>(from a in abilitiesToAddToGlossary
			select a.data.nameLocalized.id into id
			where id
			select id);
		await LocalizationUtil.TranslateTableAsync(LocalizationSettings.StringDatabase.GetTable("Ability", locale), useGlossary: true, (LocalizedStringData.TableEntryId id) => idsToTranslateHash.Contains(id), "AbilityName");
		foreach (DataRef<AbilityData> item in abilitiesToAddToGlossary)
		{
			string abilitySourceName = item.data.name;
			string abilityTargetName = item.data.nameLocalized.localizedString.Localize(locale);
			if (!abilitySourceName.HasVisibleCharacter() || !abilityTargetName.HasVisibleCharacter())
			{
				continue;
			}
			StringTableEntry tableEntry = item.data.nameLocalized.id.GetTableEntry(locale);
			if (tableEntry != null && tableEntry.Value.HasVisibleCharacter())
			{
				GlossaryEntryMeta glossaryEntryMeta = tableEntry.GlossaryMeta();
				if (glossaryEntryMeta == null || glossaryEntryMeta.excludeFromDefault != ExcludeFromDefaultGlossary.Always)
				{
					bool verbatim = glossaryEntryMeta?.verbatim ?? true;
					bool properNoun = glossaryEntryMeta?.properNoun ?? true;
					ProtectedTermWhen protectedWhen = glossaryEntryMeta?.protectedTermWhen ?? ProtectedTermWhen.Verbatim;
					yield return new Term(abilitySourceName, abilityTargetName, verbatim, properNoun, protectedWhen);
					yield return new Term("<b>" + abilitySourceName + "</b>", "<b>" + abilityTargetName + "</b>", verbatim, properNoun, protectedWhen);
				}
			}
		}
	}
}
