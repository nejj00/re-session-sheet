using Godot;
using RESessionSheet.Domain.Model;
using RESessionSheet.Domain.Serialization;

namespace RESessionSheet.App.Services;

/// <summary>
/// Loads the item catalog from res://data/catalogs/items.json. Uses Godot's FileAccess rather than
/// System.IO, because res:// paths aren't real filesystem paths once exported (they're packed into
/// a .pck) - unlike SaveService, which reads/writes user:// paths that ARE real OS paths via
/// ProjectSettings.GlobalizePath.
/// </summary>
public class ItemCatalogService
{
	private const string CatalogPath = "res://data/catalogs/items.json";

	public IReadOnlyList<ItemDefinition> Items { get; }

	private readonly Dictionary<string, ItemDefinition> _byId;

	public ItemCatalogService()
	{
		var catalog = Load();
		Items = catalog.Items;
		_byId = catalog.Items.ToDictionary(i => i.Id);
	}

	public ItemDefinition? Find(string id) => _byId.GetValueOrDefault(id);

	private static ItemCatalog Load()
	{
		if (!Godot.FileAccess.FileExists(CatalogPath))
		{
			GD.PushWarning($"Item catalog not found at {CatalogPath}; starting with an empty catalog.");
			return new ItemCatalog();
		}

		using var file = Godot.FileAccess.Open(CatalogPath, Godot.FileAccess.ModeFlags.Read);
		string json = file.GetAsText();
		return ItemCatalogSerializer.Deserialize(json);
	}
}
