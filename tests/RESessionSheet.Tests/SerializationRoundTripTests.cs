using RESessionSheet.Domain.Model;
using RESessionSheet.Domain.Serialization;

namespace RESessionSheet.Tests;

public class SerializationRoundTripTests
{
	private static CharacterFile BuildFullyPopulatedCharacter()
	{
		var file = new CharacterFile
		{
			SchemaVersion = CharacterFile.CurrentSchemaVersion,
			Id = Guid.NewGuid(),
			CreatedUtc = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
			LastModifiedUtc = new DateTime(2026, 7, 9, 8, 30, 0, DateTimeKind.Utc),
		};

		var sheet = file.Sheet;
		sheet.Name = "Aslan Zırhbaş";
		sheet.Age = "29";
		sheet.Gender = "Male";
		sheet.Height = "180cm";
		sheet.Weight = "78kg";
		sheet.EyeColor = "Brown";
		sheet.HairColor = "Black";
		sheet.SkinTone = "Olive";
		sheet.Demeanor = "Stoic";
		sheet.Rank = "Intermediate";
		sheet.FirstTeam = "Bravo";
		sheet.Backstory = "A paramedic caught up in the Raccoon City outbreak.\nLine two of backstory.";
		sheet.Role = "Intermediate";
		sheet.Talent = "Paramedic";
		sheet.Specialist = "Field Medicine";
		sheet.ArchetypeMerits.Add("Medical Intuition");
		sheet.ArchetypeFlaws.Add("Wanted by Umbrella");

		sheet.Attributes.Strength = 3;
		sheet.Attributes.Dexterity = 3;
		sheet.Attributes.Stamina = 2;
		sheet.Attributes.Charisma = 2;
		sheet.Attributes.Manipulation = 1;
		sheet.Attributes.Composure = 2;
		sheet.Attributes.Perception = 2;
		sheet.Attributes.Intelligence = 4;
		sheet.Attributes.Wits = 3;

		sheet.Skills.Get(SkillId.Medicine).Rating = 4;
		sheet.Skills.Get(SkillId.Medicine).Specialty = "Trauma Care";
		sheet.Skills.Get(SkillId.Firearms).Rating = 2;
		sheet.Skills.Get(SkillId.Stealth).Rating = 3;

		sheet.Virtues.Conscience = 3;
		sheet.Virtues.SelfControl = 2;
		sheet.Virtues.Courage = 4;

		sheet.Xp.Available = 1;
		sheet.Xp.Spent = 334;

		sheet.Health.ManualBoxes[0] = true;
		sheet.Health.ManualBoxes[1] = true;
		sheet.Health.ManualBoxes[3] = true;
		sheet.Health.BonusHpFromArmor = 2;

		sheet.Strain.Value = 15;

		sheet.Ap.Current = 9;
		sheet.Ap.Max = 15;

		sheet.Inventory.Items.Add(new InventoryItem
		{
			Name = "Green Herb",
			Category = InventoryCategory.Health,
			Quantity = 3,
			Notes = "Found in the garage",
			Equipped = false,
			GridWidth = 1,
			GridHeight = 1,
			Rotation = 0,
			IconPath = "res://icons/green_herb.png",
			StackId = "herb-green",
		});
		sheet.Inventory.Items.Add(new InventoryItem
		{
			Name = "Combat Knife",
			Category = InventoryCategory.Weapon,
			Quantity = 1,
			Equipped = true,
			GridWidth = 1,
			GridHeight = 3,
			Rotation = 90,
		});

		sheet.SessionLog.Add(new SessionLogEntry
		{
			SessionNumber = 1,
			XpAwarded = 3,
			RunningXpTotal = 3,
			Notes = "Met Joseph Kendo at the gun shop.",
			PlayedOnUtc = new DateTime(2026, 3, 17, 0, 0, 0, DateTimeKind.Utc),
		});
		sheet.SessionLog.Add(new SessionLogEntry
		{
			SessionNumber = 2,
			XpAwarded = 5,
			RunningXpTotal = 8,
			Notes = "Investigated the missing police officer.",
			PlayedOnUtc = null,
		});

		sheet.Merits.Add(new TaggedEntry { Name = "Tough as Nails", Description = "Permanent +1 Soak." });
		sheet.Flaws.Add(new TaggedEntry { Name = "Short Fuse", Description = "Risk of lashing out." });
		sheet.Traits.Add(new TaggedEntry { Name = "Left-handed" });
		sheet.Backgrounds.Add(new TaggedEntry { Name = "Umbrella Contact", Description = "An informant on the inside.", CatalogId = "bg-umbrella-contact" });

		sheet.WoundNotes.Add("Scar across left forearm from the lab incident.");

		return file;
	}

	[Fact]
	public void RoundTrip_ProducesIdenticalJsonOnReserialize()
	{
		var original = BuildFullyPopulatedCharacter();

		string json1 = CharacterFileSerializer.Serialize(original);
		var deserialized = CharacterFileSerializer.Deserialize(json1);
		string json2 = CharacterFileSerializer.Serialize(deserialized);

		Assert.Equal(json1, json2);
	}

	[Fact]
	public void RoundTrip_PreservesTopLevelIdentityFields()
	{
		var original = BuildFullyPopulatedCharacter();
		var json = CharacterFileSerializer.Serialize(original);
		var deserialized = CharacterFileSerializer.Deserialize(json);

		Assert.Equal(original.Id, deserialized.Id);
		Assert.Equal(original.SchemaVersion, deserialized.SchemaVersion);
		Assert.Equal(original.CreatedUtc, deserialized.CreatedUtc);
		Assert.Equal(original.LastModifiedUtc, deserialized.LastModifiedUtc);
	}

	[Fact]
	public void RoundTrip_PreservesSkillsDictionaryKeyedByEnum()
	{
		var original = BuildFullyPopulatedCharacter();
		var json = CharacterFileSerializer.Serialize(original);
		var deserialized = CharacterFileSerializer.Deserialize(json);

		Assert.Equal(4, deserialized.Sheet.Skills.Get(SkillId.Medicine).Rating);
		Assert.Equal("Trauma Care", deserialized.Sheet.Skills.Get(SkillId.Medicine).Specialty);
		Assert.Equal(2, deserialized.Sheet.Skills.Get(SkillId.Firearms).Rating);
		Assert.Equal(3, deserialized.Sheet.Skills.Get(SkillId.Stealth).Rating);
		// Untouched skills should still be present (pre-seeded), just at their default rating.
		Assert.Equal(0, deserialized.Sheet.Skills.Get(SkillId.Enigmas).Rating);
		Assert.Equal(23, deserialized.Sheet.Skills.Entries.Count);
	}

	[Fact]
	public void RoundTrip_PreservesHealthTrackManualBoxesExactly()
	{
		var original = BuildFullyPopulatedCharacter();
		var json = CharacterFileSerializer.Serialize(original);
		var deserialized = CharacterFileSerializer.Deserialize(json);

		Assert.Equal(original.Sheet.Health.ManualBoxes, deserialized.Sheet.Health.ManualBoxes);
		Assert.Equal(14, deserialized.Sheet.Health.ManualBoxes.Length);
	}

	[Fact]
	public void RoundTrip_PreservesInventoryItemsIncludingGridForwardCompatFields()
	{
		var original = BuildFullyPopulatedCharacter();
		var json = CharacterFileSerializer.Serialize(original);
		var deserialized = CharacterFileSerializer.Deserialize(json);

		Assert.Equal(2, deserialized.Sheet.Inventory.Items.Count);
		var knife = deserialized.Sheet.Inventory.Items.Single(i => i.Name == "Combat Knife");
		Assert.Equal(1, knife.GridWidth);
		Assert.Equal(3, knife.GridHeight);
		Assert.Equal(90, knife.Rotation);
		Assert.True(knife.Equipped);
	}

	[Fact]
	public void RoundTrip_PreservesSessionLogAndNullableDateTime()
	{
		var original = BuildFullyPopulatedCharacter();
		var json = CharacterFileSerializer.Serialize(original);
		var deserialized = CharacterFileSerializer.Deserialize(json);

		Assert.Equal(2, deserialized.Sheet.SessionLog.Count);
		Assert.NotNull(deserialized.Sheet.SessionLog[0].PlayedOnUtc);
		Assert.Null(deserialized.Sheet.SessionLog[1].PlayedOnUtc);
		Assert.Equal(8, deserialized.Sheet.SessionLog[1].RunningXpTotal);
	}

	[Fact]
	public void RoundTrip_PreservesTaggedEntriesAndCatalogIdNullability()
	{
		var original = BuildFullyPopulatedCharacter();
		var json = CharacterFileSerializer.Serialize(original);
		var deserialized = CharacterFileSerializer.Deserialize(json);

		Assert.Single(deserialized.Sheet.Merits);
		Assert.Null(deserialized.Sheet.Traits[0].CatalogId);
		Assert.Equal("bg-umbrella-contact", deserialized.Sheet.Backgrounds[0].CatalogId);
	}

	[Fact]
	public void RoundTrip_EmptyDefaultCharacter_DoesNotThrow()
	{
		var file = new CharacterFile();
		var json = CharacterFileSerializer.Serialize(file);
		var deserialized = CharacterFileSerializer.Deserialize(json);

		Assert.Equal(file.Id, deserialized.Id);
		Assert.Equal(23, deserialized.Sheet.Skills.Entries.Count);
	}

	[Fact]
	public void Serialize_UsesCamelCasePropertyNames()
	{
		var original = BuildFullyPopulatedCharacter();
		var json = CharacterFileSerializer.Serialize(original);

		Assert.Contains("\"schemaVersion\"", json);
		Assert.Contains("\"sheet\"", json);
		Assert.DoesNotContain("\"SchemaVersion\"", json);
	}
}
