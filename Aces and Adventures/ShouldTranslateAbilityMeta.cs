using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class ShouldTranslateAbilityMeta : ShouldTranslateMeta
{
	private static HashSet<LocalizedStringData.TableEntryId> _GlossaryIds;

	public static HashSet<LocalizedStringData.TableEntryId> GlossaryIds => _GlossaryIds ?? (_GlossaryIds = new HashSet<LocalizedStringData.TableEntryId>(from a in new List<DataRef<AbilityData>>(from a in DataRef<AbilityData>.Search()
			where a.data.type.IsTrait() || (a.data.characterClass.HasValue && !a.data.upgradeOf)
			select a)
		select a.data.nameLocalized.id into id
		where id
		select id));

	public override bool ShouldTranslate(LocalizedStringData.TableEntryId id)
	{
		return !GlossaryIds.Contains(id);
	}
}
