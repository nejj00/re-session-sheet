using Godot;
using RESessionSheet.App;
using RESessionSheet.Domain.Model;
using RESessionSheet.UI.Components;

namespace RESessionSheet.UI.Sheet.Panels;

/// <summary>
/// A structured item list, hybrid mode: items can be picked from the shared ItemCatalog (Name/
/// Category/shape/icon derived live from the catalog entry, so catalog edits stay in sync) or
/// added as freeform one-offs (fully editable, matching the old V1 behavior). Grid-related fields
/// on InventoryItem/ItemDefinition exist for the future V2 grid inventory but aren't exposed here.
/// Item identity is always by Guid, never list position.
/// </summary>
public partial class InventoryPanel : FramedPanel
{
	private VBoxContainer _itemsContainer = null!;
	private OptionButton _catalogPicker = null!;

	private static CharacterSheet Sheet => GameState.Instance.CurrentCharacter!.Sheet;
	private static IReadOnlyList<ItemDefinition> CatalogItems => GameState.Instance.ItemCatalog.Items;

	public override void _Ready()
	{
		base._Ready();
		BuildUi();
		RefreshItems();
	}

	private void BuildUi()
	{
		var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		vbox.AddThemeConstantOverride("separation", 10);
		Content.AddChild(vbox);

		vbox.AddChild(new Label { Text = "Inventory" });

		var scroll = new ScrollContainer
		{
			SizeFlagsVertical = SizeFlags.ExpandFill,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
		};
		vbox.AddChild(scroll);

		_itemsContainer = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		_itemsContainer.AddThemeConstantOverride("separation", 4);
		scroll.AddChild(_itemsContainer);

		vbox.AddChild(new HSeparator());

		var catalogRow = new HBoxContainer();
		catalogRow.AddThemeConstantOverride("separation", 8);

		_catalogPicker = new OptionButton { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		foreach (var def in CatalogItems)
		{
			_catalogPicker.AddItem(def.Name);
		}
		catalogRow.AddChild(_catalogPicker);

		var addFromCatalogButton = new Button { Text = "Add from Catalog" };
		addFromCatalogButton.Pressed += OnAddFromCatalogPressed;
		catalogRow.AddChild(addFromCatalogButton);
		vbox.AddChild(catalogRow);

		var addButton = new Button { Text = "Add Freeform Item" };
		addButton.Pressed += OnAddItemPressed;
		vbox.AddChild(addButton);
	}

	private void RefreshItems()
	{
		foreach (Node child in _itemsContainer.GetChildren())
		{
			child.QueueFree();
		}

		foreach (var item in Sheet.Inventory.Items)
		{
			_itemsContainer.AddChild(BuildItemRow(item));
		}
	}

	private Control BuildItemRow(InventoryItem item)
	{
		var row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 8);

		if (item.CatalogId != null)
		{
			var catalogEntry = GameState.Instance.ItemCatalog.Find(item.CatalogId);
			string displayName = catalogEntry?.Name ?? $"Unknown item ({item.CatalogId})";
			string categoryText = catalogEntry?.Category.ToString() ?? "?";

			row.AddChild(new Label { Text = displayName, CustomMinimumSize = new Vector2(140, 0) });
			row.AddChild(new Label { Text = categoryText, CustomMinimumSize = new Vector2(100, 0) });
		}
		else
		{
			var name = new LineEdit { Text = item.Name, PlaceholderText = "Name", CustomMinimumSize = new Vector2(140, 0) };
			name.TextChanged += t => { item.Name = t; SaveInventory(); };
			row.AddChild(name);

			var category = new OptionButton { CustomMinimumSize = new Vector2(100, 0) };
			foreach (InventoryCategory cat in Enum.GetValues<InventoryCategory>())
			{
				category.AddItem(cat.ToString());
			}
			category.Selected = (int)item.Category;
			category.ItemSelected += i => { item.Category = (InventoryCategory)i; SaveInventory(); };
			row.AddChild(category);
		}

		var quantity = new SpinBox { MinValue = 0, MaxValue = 999, Step = 1, Value = item.Quantity, CustomMinimumSize = new Vector2(70, 0) };
		quantity.ValueChanged += v => { item.Quantity = (int)v; SaveInventory(); };
		row.AddChild(quantity);

		var equipped = new CheckBox { Text = "Equipped", ButtonPressed = item.Equipped };
		equipped.Toggled += v => { item.Equipped = v; SaveInventory(); };
		row.AddChild(equipped);

		var notes = new LineEdit { Text = item.Notes, PlaceholderText = "Notes", SizeFlagsHorizontal = SizeFlags.ExpandFill };
		notes.TextChanged += t => { item.Notes = t; SaveInventory(); };
		row.AddChild(notes);

		var remove = new Button { Text = "Remove" };
		remove.Pressed += () => OnRemoveItemPressed(item);
		row.AddChild(remove);

		return row;
	}

	private void OnAddFromCatalogPressed()
	{
		int selectedIndex = _catalogPicker.Selected;
		if (selectedIndex < 0 || selectedIndex >= CatalogItems.Count)
		{
			return;
		}

		var def = CatalogItems[selectedIndex];
		Sheet.Inventory.Items.Add(new InventoryItem { CatalogId = def.Id, Quantity = 1 });
		RefreshItems();
		SaveInventory();
	}

	private void OnAddItemPressed()
	{
		Sheet.Inventory.Items.Add(new InventoryItem { Name = "" });
		RefreshItems();
		SaveInventory();
	}

	private void OnRemoveItemPressed(InventoryItem item)
	{
		Sheet.Inventory.Items.Remove(item);
		RefreshItems();
		SaveInventory();
	}

	private static void SaveInventory() => GameState.Instance.NotifyInventoryChanged();
}
