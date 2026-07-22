namespace RESessionSheet.Domain.Model;

public enum InventoryCategory
{
	Health,
	Weapon,
	Armor,
	Key,
	Ammo,
	Misc
}

public class InventoryItem
{
	public Guid Id { get; set; } = Guid.NewGuid();

	/// <summary>
	/// When set, this instance is linked to an ItemCatalog entry by id - Name/Category/Shape/IconPath
	/// should be looked up live from the catalog rather than duplicated here, so catalog edits stay
	/// in sync with every existing instance. Null means a freeform item (hybrid mode): Name/Category
	/// below are the actual source of truth for it.
	/// </summary>
	public string? CatalogId { get; set; }

	public string Name { get; set; } = "";
	/// <summary>Only meaningful when CatalogId is null; catalog-linked items use the catalog entry's category.</summary>
	public InventoryCategory Category { get; set; } = InventoryCategory.Misc;
	public int Quantity { get; set; } = 1;
	public string Notes { get; set; } = "";
	public bool Equipped { get; set; }

	// Forward-compat fields for the V2 grid inventory. Unused by V1's list UI, defaulted so no
	// migration is needed later, and persisted from day one so nothing is lost on upgrade.
	public int GridWidth { get; set; } = 1;
	public int GridHeight { get; set; } = 1;
	public int Rotation { get; set; }
	public string IconPath { get; set; } = "";
	public string StackId { get; set; } = "";
}

public class Inventory
{
	public List<InventoryItem> Items { get; set; } = new();
}
