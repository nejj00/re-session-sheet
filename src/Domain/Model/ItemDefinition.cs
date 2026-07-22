namespace RESessionSheet.Domain.Model;

/// <summary>
/// A shared catalog entry describing a pickable item type - as opposed to InventoryItem, which is
/// a character's owned instance. Shape stores one canonical (unrotated) occupancy grid; rotated
/// variants are computed on demand via Domain.Rules.ItemShape, not stored per-entry.
/// </summary>
public class ItemDefinition
{
	public string Id { get; set; } = "";
	public string Name { get; set; } = "";
	public InventoryCategory Category { get; set; } = InventoryCategory.Misc;
	public string Description { get; set; } = "";

	/// <summary>Path to the item's icon, e.g. "res://data/assets/items/pistol_m9.png". May point at a placeholder until real art exists.</summary>
	public string IconPath { get; set; } = "";

	/// <summary>Canonical (unrotated) shape rows, e.g. ["X.", "X.", "XX"]. 'X' = occupied, '.' = empty.</summary>
	public List<string> Shape { get; set; } = new() { "X" };

	public bool Stackable { get; set; }
}

public class ItemCatalog
{
	public List<ItemDefinition> Items { get; set; } = new();
}
