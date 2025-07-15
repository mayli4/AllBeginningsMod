using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;
using Terraria.ModLoader;

namespace AllBeginningsMod.Common.Bestiary;

public class BestiaryHelper : ILoadable {
	private static Dictionary<string, IBestiaryInfoElement> _ConditionsByName;

	public static BestiaryHelper Self { get; internal set; }

	public void Load(Mod mod) {
		_ConditionsByName = new Dictionary<string, IBestiaryInfoElement>();
		LoadNestedClassConditions(typeof(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes));
		LoadNestedClassConditions(typeof(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events));
		LoadNestedClassConditions(typeof(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Invasions));
		LoadNestedClassConditions(typeof(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times));
		LoadNestedClassConditions(typeof(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Visuals));
	}

	public static IBestiaryInfoElement[] BuildEntry(ModNPC npc, string conditions) {
		string obj = $"Mods.{npc.Mod.Name}.Bestiary.{npc.Name}Bestiary";
		Language.GetOrRegister(obj, () => "");
        
		var flavorTextBestiaryInfoElement = new FlavorTextBestiaryInfoElement(obj);
		if (conditions == string.Empty) {
			return new IBestiaryInfoElement[1] { flavorTextBestiaryInfoElement };
		}
        
		string[] conditionsArray = conditions.Split(' ');
		var elementArray = new IBestiaryInfoElement[conditionsArray.Length + 1];
		elementArray[0] = flavorTextBestiaryInfoElement;
        
		for (int i = 1; i < elementArray.Length; i++) {
			elementArray[i] = _ConditionsByName[conditionsArray[i - 1]];
		}
		return elementArray;
	}

	private static void LoadNestedClassConditions(Type containerType) {
		foreach (FieldInfo item in from x in containerType.GetFields(BindingFlags.Static | BindingFlags.Public) where x.FieldType.IsAssignableTo(typeof(IBestiaryInfoElement)) select x) {
			IBestiaryInfoElement value = item.GetValue(null) as IBestiaryInfoElement;
			if (!_ConditionsByName.ContainsKey(item.Name)) {
				_ConditionsByName.Add(item.Name, value);
			}
			else {
				_ConditionsByName.Add(item.DeclaringType!.Name + "." + item.Name, value);
			}
		}
	}

    public void Unload() { }
}

public static class BestiaryExtensions {
    public static void AddInfo(this BestiaryEntry bestiaryEntry, ModNPC npc, string conditions) {
        bestiaryEntry.Info.AddRange(BestiaryHelper.BuildEntry(npc, conditions));
    }
}