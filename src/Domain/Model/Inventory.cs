namespace RESessionSheet.Domain.Model;

public class InventoryItem
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string Name { get; set; } = "";
	/// <summary>Freeform in V1 (Herb/Weapon/Armor/Key/Ammo/Misc); a catalog-backed enum is a later-phase upgrade.</summary>
	public string Category { get; set; } = "";
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
