using RESessionSheet.Domain.Model;
using RESessionSheet.Domain.Rules;
using RESessionSheet.Domain.Serialization;

namespace RESessionSheet.Tests;

public class ItemCatalogSerializerTests
{
	private static ItemCatalog BuildSampleCatalog() => new()
	{
		Items = new List<ItemDefinition>
		{
			new()
			{
				Id = "pistol-m9-beretta",
				Name = "M9 Beretta",
				Category = InventoryCategory.Weapon,
				Description = "A reliable sidearm.",
				IconPath = "res://data/assets/items/placeholder.png",
				Shape = new List<string> { "X.", "X.", "XX" },
				Stackable = false,
			},
			new()
			{
				Id = "green-herb",
				Name = "Green Herb",
				Category = InventoryCategory.Health,
				Description = "A common healing herb.",
				IconPath = "res://data/assets/items/placeholder.png",
				Shape = new List<string> { "X" },
				Stackable = true,
			},
		},
	};

	[Fact]
	public void RoundTrip_PreservesAllItemFields()
	{
		var original = BuildSampleCatalog();

		string json = ItemCatalogSerializer.Serialize(original);
		var deserialized = ItemCatalogSerializer.Deserialize(json);

		Assert.Equal(2, deserialized.Items.Count);
		var pistol = deserialized.Items.Single(i => i.Id == "pistol-m9-beretta");
		Assert.Equal("M9 Beretta", pistol.Name);
		Assert.Equal(InventoryCategory.Weapon, pistol.Category);
		Assert.Equal("A reliable sidearm.", pistol.Description);
		Assert.Equal(new List<string> { "X.", "X.", "XX" }, pistol.Shape);
		Assert.False(pistol.Stackable);
	}

	[Fact]
	public void RoundTrip_ProducesIdenticalJsonOnReserialize()
	{
		var original = BuildSampleCatalog();

		string json1 = ItemCatalogSerializer.Serialize(original);
		var deserialized = ItemCatalogSerializer.Deserialize(json1);
		string json2 = ItemCatalogSerializer.Serialize(deserialized);

		Assert.Equal(json1, json2);
	}

	[Fact]
	public void Deserialize_UsesCamelCasePropertyNames()
	{
		var original = BuildSampleCatalog();
		string json = ItemCatalogSerializer.Serialize(original);

		Assert.Contains("\"iconPath\"", json);
		Assert.Contains("\"category\"", json);
		Assert.DoesNotContain("\"IconPath\"", json);
	}

	[Fact]
	public void Deserialize_CategoryEnum_SerializesAsCamelCaseString()
	{
		var original = BuildSampleCatalog();
		string json = ItemCatalogSerializer.Serialize(original);

		Assert.Contains("\"weapon\"", json);
		Assert.Contains("\"health\"", json);
	}

	[Fact]
	public void DeserializedShape_ParsesCorrectlyViaItemShape()
	{
		var original = BuildSampleCatalog();
		string json = ItemCatalogSerializer.Serialize(original);
		var deserialized = ItemCatalogSerializer.Deserialize(json);

		var pistol = deserialized.Items.Single(i => i.Id == "pistol-m9-beretta");
		var grid = ItemShape.Parse(pistol.Shape);

		Assert.Equal(4, ItemShape.CountOccupiedCells(grid));
		Assert.Equal(2, ItemShape.Width(grid));
		Assert.Equal(3, ItemShape.Height(grid));
	}

	[Fact]
	public void Deserialize_EmptyCatalog_DoesNotThrow()
	{
		var catalog = new ItemCatalog();
		string json = ItemCatalogSerializer.Serialize(catalog);
		var deserialized = ItemCatalogSerializer.Deserialize(json);

		Assert.Empty(deserialized.Items);
	}
}
