using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using RESessionSheet.Domain.Model;

namespace RESessionSheet.Domain.Serialization;

public static class LegacyImporter
{
	private static readonly string[] CheckboxMapping = new string[]
	{
		"UNKNOWN-total",
		"strength-1",
		"strength-2",
		"strength-3",
		"strength-4",
		"strength-5",
		"strength-6",
		"dexterity-1",
		"dexterity-2",
		"dexterity-3",
		"dexterity-4",
		"dexterity-5",
		"dexterity-6",
		"stamina-1",
		"stamina-2",
		"stamina-3",
		"stamina-4",
		"stamina-5",
		"stamina-6",
		"stamina-total",
		"charisma-1",
		"charisma-2",
		"charisma-3",
		"charisma-4",
		"charisma-5",
		"charisma-6",
		"manipulation-1",
		"manipulation-2",
		"manipulation-3",
		"manipulation-4",
		"manipulation-5",
		"manipulation-6",
		"appearance-1",
		"appearance-2",
		"appearance-3",
		"appearance-4",
		"appearance-5",
		"appearance-6",
		"appearance-total",
		"perception-1",
		"perception-2",
		"perception-3",
		"perception-4",
		"perception-5",
		"perception-6",
		"intelligence-1",
		"intelligence-2",
		"intelligence-3",
		"intelligence-4",
		"intelligence-5",
		"intelligence-6",
		"wits-1",
		"wits-2",
		"wits-3",
		"wits-4",
		"wits-5",
		"wits-6",
		"wits-total",
		"alertness-1",
		"alertness-2",
		"alertness-3",
		"alertness-4",
		"alertness-5",
		"alertness-6",
		"athletics-1",
		"athletics-2",
		"athletics-3",
		"athletics-4",
		"athletics-5",
		"athletics-6",
		"brawl-1",
		"brawl-2",
		"brawl-3",
		"brawl-4",
		"brawl-5",
		"brawl-6",
		"dodge-1",
		"dodge-2",
		"dodge-3",
		"dodge-4",
		"dodge-5",
		"dodge-6",
		"empathy-1",
		"empathy-2",
		"empathy-3",
		"empathy-4",
		"empathy-5",
		"empathy-6",
		"expression-1",
		"expression-2",
		"expression-3",
		"expression-4",
		"expression-5",
		"expression-6",
		"intimidation-1",
		"intimidation-2",
		"intimidation-3",
		"intimidation-4",
		"intimidation-5",
		"intimidation-6",
		"leadership-1",
		"leadership-2",
		"leadership-3",
		"leadership-4",
		"leadership-5",
		"leadership-6",
		"streetwise-1",
		"streetwise-2",
		"streetwise-3",
		"streetwise-4",
		"streetwise-5",
		"streetwise-6",
		"subterfuge-1",
		"subterfuge-2",
		"subterfuge-3",
		"subterfuge-4",
		"subterfuge-5",
		"subterfuge-6",
		"subterfuge-total",
		"animalken-1",
		"animalken-2",
		"animalken-3",
		"animalken-4",
		"animalken-5",
		"animalken-6",
		"drive-1",
		"drive-2",
		"drive-3",
		"drive-4",
		"drive-5",
		"drive-6",
		"etiquette-1",
		"etiquette-2",
		"etiquette-3",
		"etiquette-4",
		"etiquette-5",
		"etiquette-6",
		"firearms-1",
		"firearms-2",
		"firearms-3",
		"firearms-4",
		"firearms-5",
		"firearms-6",
		"melee-1",
		"melee-2",
		"melee-3",
		"melee-4",
		"melee-5",
		"melee-6",
		"performance-1",
		"performance-2",
		"performance-3",
		"performance-4",
		"performance-5",
		"performance-6",
		"repair-1",
		"repair-2",
		"repair-3",
		"repair-4",
		"repair-5",
		"repair-6",
		"security-1",
		"security-2",
		"security-3",
		"security-4",
		"security-5",
		"security-6",
		"stealth-1",
		"stealth-2",
		"stealth-3",
		"stealth-4",
		"stealth-5",
		"stealth-6",
		"survival-1",
		"survival-2",
		"survival-3",
		"survival-4",
		"survival-5",
		"survival-6",
		"survival-total",
		"bureaucracy-1",
		"bureaucracy-2",
		"bureaucracy-3",
		"bureaucracy-4",
		"bureaucracy-5",
		"bureaucracy-6",
		"computer-1",
		"computer-2",
		"computer-3",
		"computer-4",
		"computer-5",
		"computer-6",
		"enigmas-1",
		"enigmas-2",
		"enigmas-3",
		"enigmas-4",
		"enigmas-5",
		"enigmas-6",
		"investigation-1",
		"investigation-2",
		"investigation-3",
		"investigation-4",
		"investigation-5",
		"investigation-6",
		"law-1",
		"law-2",
		"law-3",
		"law-4",
		"law-5",
		"law-6",
		"linguistics-1",
		"linguistics-2",
		"linguistics-3",
		"linguistics-4",
		"linguistics-5",
		"linguistics-6",
		"medicine-1",
		"medicine-2",
		"medicine-3",
		"medicine-4",
		"medicine-5",
		"medicine-6",
		"occult-1",
		"occult-2",
		"occult-3",
		"occult-4",
		"occult-5",
		"occult-6",
		"politics-1",
		"politics-2",
		"politics-3",
		"politics-4",
		"politics-5",
		"politics-6",
		"science-1",
		"science-2",
		"science-3",
		"science-4",
		"science-5",
		"science-6",
		"science-total",
		"science-total",
		"conscience-1",
		"conscience-2",
		"conscience-3",
		"conscience-4",
		"conscience-5",
		"conscience-6",
		"selfcontrol-1",
		"selfcontrol-2",
		"selfcontrol-3",
		"selfcontrol-4",
		"selfcontrol-5",
		"selfcontrol-6",
		"courage-1",
		"courage-2",
		"courage-3",
		"courage-4",
		"courage-5",
		"courage-6",
		"wound-bruised",
		"wound-hurt",
		"wound-injured",
		"wound-wounded",
		"wound-mauled",
		"wound-crippled",
		"wound-incapacitated"
	};

	public static CharacterFile Import(string jsonString)
	{
		var file = new CharacterFile();
		var sheet = file.Sheet;

		var node = JsonNode.Parse(jsonString) as JsonObject;
		if (node == null) return file;

		SetString(node, "char-name", v => sheet.Name = v);
		SetString(node, "char-age", v => sheet.Age = v);
		SetString(node, "char-gender", v => sheet.Gender = v);
		SetString(node, "char-weight", v => sheet.Weight = v);
		SetString(node, "char-height", v => sheet.Height = v);
		SetString(node, "char-eye", v => sheet.EyeColor = v);
		SetString(node, "char-hair", v => sheet.HairColor = v);
		SetString(node, "char-skin", v => sheet.SkinTone = v);
		SetString(node, "char-demeanor", v => sheet.Demeanor = v);
		SetString(node, "char-rank", v => sheet.Rank = v);
		SetString(node, "char-first-team", v => sheet.FirstTeam = v);

		SetInt(node, "current-strain", v => sheet.Strain.Value = v);

		int expSpent = GetInt(node, "exp-spent");
		int expAvailable = GetInt(node, "exp-available");
		sheet.Xp.Spent = expSpent;
		sheet.Xp.Available = expAvailable;

		SetString(node, "activityLog", v => 
		{
			if (!string.IsNullOrWhiteSpace(v))
			{
				sheet.SessionLog.Add(new SessionLogEntry { Notes = v.Trim() });
			}
		});

		for (int j = 0; j < CheckboxMapping.Length; j++)
		{
			string key = $"checkbox-{j}";
			if (node.TryGetPropertyValue(key, out var prop) && prop is JsonObject obj)
			{
				bool isChecked = obj["checked"]?.GetValue<bool>() ?? false;
				string classes = obj["classes"]?.GetValue<string>() ?? "";

				string mapVal = CheckboxMapping[j];
				if (mapVal.StartsWith("UNKNOWN") || mapVal.EndsWith("-total")) continue;

				if (mapVal.StartsWith("wound-"))
				{
					int woundIndex = GetWoundIndex(mapVal);
					if (woundIndex != -1)
					{
						int boxesIdx = woundIndex * 2;
						if (classes.Contains("filled"))
						{
							sheet.Health.ManualBoxes[boxesIdx] = true;
							sheet.Health.ManualBoxes[boxesIdx + 1] = true;
						}
						else if (classes.Contains("active") || classes.Contains("crossed") || isChecked)
						{
							sheet.Health.ManualBoxes[boxesIdx] = true;
						}
					}
					continue;
				}

				if (isChecked)
				{
					var parts = mapVal.Split('-');
					if (parts.Length == 2 && int.TryParse(parts[1], out int level))
					{
						ApplyStat(sheet, parts[0], level);
					}
				}
			}
		}

		MapTaggedEntries(node, "merits-sub-section-text", sheet.Merits);
		MapTaggedEntries(node, "flaws-sub-section-text", sheet.Flaws);
		MapTaggedEntries(node, "other-traits-sub-section-text", sheet.Traits);
		MapTaggedEntries(node, "backgrounds-sub-section-text", sheet.Backgrounds);

		SetString(node, "inventory-textarea", v =>
		{
			if (!string.IsNullOrWhiteSpace(v))
			{
				sheet.Inventory.Items.Add(new InventoryItem
				{
					Name = "Legacy Inventory",
					Category = "Misc",
					Notes = v.Trim()
				});
			}
		});

		return file;
	}

	private static void SetString(JsonObject node, string key, Action<string> setter)
	{
		if (node.TryGetPropertyValue(key, out var val) && val != null)
		{
			string s = val.GetValue<string>();
			if (!string.IsNullOrEmpty(s)) setter(s);
		}
	}

	private static void SetInt(JsonObject node, string key, Action<int> setter)
	{
		if (node.TryGetPropertyValue(key, out var val) && val != null)
		{
			string s = val.GetValue<string>();
			if (!string.IsNullOrEmpty(s) && int.TryParse(s, out int i)) setter(i);
		}
	}

	private static int GetInt(JsonObject node, string key)
	{
		if (node.TryGetPropertyValue(key, out var val) && val != null)
		{
			string s = val.GetValue<string>();
			if (!string.IsNullOrEmpty(s) && int.TryParse(s, out int i)) return i;
		}
		return 0;
	}

	private static void MapTaggedEntries(JsonObject node, string key, List<TaggedEntry> list)
	{
		if (node.TryGetPropertyValue(key, out var val) && val is JsonArray arr)
		{
			foreach (var item in arr)
			{
				string s = item?.GetValue<string>() ?? "";
				if (!string.IsNullOrWhiteSpace(s))
				{
					list.Add(new TaggedEntry { Name = s.Trim() });
				}
			}
		}
	}

	private static int GetWoundIndex(string name) => name switch
	{
		"wound-bruised" => 0,
		"wound-hurt" => 1,
		"wound-injured" => 2,
		"wound-wounded" => 3,
		"wound-mauled" => 4,
		"wound-crippled" => 5,
		"wound-incapacitated" => 6,
		_ => -1
	};

	private static void ApplyStat(CharacterSheet sheet, string stat, int level)
	{
		switch (stat) {
			case "strength": sheet.Attributes.Strength = Math.Max(sheet.Attributes.Strength, level); break;
			case "dexterity": sheet.Attributes.Dexterity = Math.Max(sheet.Attributes.Dexterity, level); break;
			case "stamina": sheet.Attributes.Stamina = Math.Max(sheet.Attributes.Stamina, level); break;
			case "charisma": sheet.Attributes.Charisma = Math.Max(sheet.Attributes.Charisma, level); break;
			case "manipulation": sheet.Attributes.Manipulation = Math.Max(sheet.Attributes.Manipulation, level); break;
			case "appearance": sheet.Attributes.Appearance = Math.Max(sheet.Attributes.Appearance, level); break;
			case "perception": sheet.Attributes.Perception = Math.Max(sheet.Attributes.Perception, level); break;
			case "intelligence": sheet.Attributes.Intelligence = Math.Max(sheet.Attributes.Intelligence, level); break;
			case "wits": sheet.Attributes.Wits = Math.Max(sheet.Attributes.Wits, level); break;
			case "alertness": sheet.Skills.Get(SkillId.Alertness).Rating = Math.Max(sheet.Skills.Get(SkillId.Alertness).Rating, level); break;
			case "athletics": sheet.Skills.Get(SkillId.Athletics).Rating = Math.Max(sheet.Skills.Get(SkillId.Athletics).Rating, level); break;
			case "brawl": sheet.Skills.Get(SkillId.Brawl).Rating = Math.Max(sheet.Skills.Get(SkillId.Brawl).Rating, level); break;
			case "dodge": sheet.Skills.Get(SkillId.Dodge).Rating = Math.Max(sheet.Skills.Get(SkillId.Dodge).Rating, level); break;
			case "empathy": sheet.Skills.Get(SkillId.Empathy).Rating = Math.Max(sheet.Skills.Get(SkillId.Empathy).Rating, level); break;
			case "expression": sheet.Skills.Get(SkillId.Expression).Rating = Math.Max(sheet.Skills.Get(SkillId.Expression).Rating, level); break;
			case "intimidation": sheet.Skills.Get(SkillId.Intimidation).Rating = Math.Max(sheet.Skills.Get(SkillId.Intimidation).Rating, level); break;
			case "leadership": sheet.Skills.Get(SkillId.Leadership).Rating = Math.Max(sheet.Skills.Get(SkillId.Leadership).Rating, level); break;
			case "streetwise": sheet.Skills.Get(SkillId.Streetwise).Rating = Math.Max(sheet.Skills.Get(SkillId.Streetwise).Rating, level); break;
			case "subterfuge": sheet.Skills.Get(SkillId.Subterfuge).Rating = Math.Max(sheet.Skills.Get(SkillId.Subterfuge).Rating, level); break;
			case "animalken": sheet.Skills.Get(SkillId.AnimalKen).Rating = Math.Max(sheet.Skills.Get(SkillId.AnimalKen).Rating, level); break;
			case "drive": sheet.Skills.Get(SkillId.Drive).Rating = Math.Max(sheet.Skills.Get(SkillId.Drive).Rating, level); break;
			case "etiquette": sheet.Skills.Get(SkillId.Etiquette).Rating = Math.Max(sheet.Skills.Get(SkillId.Etiquette).Rating, level); break;
			case "firearms": sheet.Skills.Get(SkillId.Firearms).Rating = Math.Max(sheet.Skills.Get(SkillId.Firearms).Rating, level); break;
			case "melee": sheet.Skills.Get(SkillId.Melee).Rating = Math.Max(sheet.Skills.Get(SkillId.Melee).Rating, level); break;
			case "performance": sheet.Skills.Get(SkillId.Performance).Rating = Math.Max(sheet.Skills.Get(SkillId.Performance).Rating, level); break;
			case "repair": sheet.Skills.Get(SkillId.Repair).Rating = Math.Max(sheet.Skills.Get(SkillId.Repair).Rating, level); break;
			case "security": sheet.Skills.Get(SkillId.Security).Rating = Math.Max(sheet.Skills.Get(SkillId.Security).Rating, level); break;
			case "stealth": sheet.Skills.Get(SkillId.Stealth).Rating = Math.Max(sheet.Skills.Get(SkillId.Stealth).Rating, level); break;
			case "survival": sheet.Skills.Get(SkillId.Survival).Rating = Math.Max(sheet.Skills.Get(SkillId.Survival).Rating, level); break;
			case "bureaucracy": sheet.Skills.Get(SkillId.Bureaucracy).Rating = Math.Max(sheet.Skills.Get(SkillId.Bureaucracy).Rating, level); break;
			case "computer": sheet.Skills.Get(SkillId.Computer).Rating = Math.Max(sheet.Skills.Get(SkillId.Computer).Rating, level); break;
			case "enigmas": sheet.Skills.Get(SkillId.Enigmas).Rating = Math.Max(sheet.Skills.Get(SkillId.Enigmas).Rating, level); break;
			case "investigation": sheet.Skills.Get(SkillId.Investigation).Rating = Math.Max(sheet.Skills.Get(SkillId.Investigation).Rating, level); break;
			case "law": sheet.Skills.Get(SkillId.Law).Rating = Math.Max(sheet.Skills.Get(SkillId.Law).Rating, level); break;
			case "linguistics": sheet.Skills.Get(SkillId.Linguistics).Rating = Math.Max(sheet.Skills.Get(SkillId.Linguistics).Rating, level); break;
			case "medicine": sheet.Skills.Get(SkillId.Medicine).Rating = Math.Max(sheet.Skills.Get(SkillId.Medicine).Rating, level); break;
			case "occult": sheet.Skills.Get(SkillId.Occult).Rating = Math.Max(sheet.Skills.Get(SkillId.Occult).Rating, level); break;
			case "politics": sheet.Skills.Get(SkillId.Politics).Rating = Math.Max(sheet.Skills.Get(SkillId.Politics).Rating, level); break;
			case "science": sheet.Skills.Get(SkillId.Science).Rating = Math.Max(sheet.Skills.Get(SkillId.Science).Rating, level); break;
			case "conscience": sheet.Virtues.Conscience = Math.Max(sheet.Virtues.Conscience, level); break;
			case "selfcontrol": sheet.Virtues.SelfControl = Math.Max(sheet.Virtues.SelfControl, level); break;
			case "courage": sheet.Virtues.Courage = Math.Max(sheet.Virtues.Courage, level); break;
		}
	}
}
