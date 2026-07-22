using Godot;
using RESessionSheet.App;
using RESessionSheet.Domain.Model;
using RESessionSheet.Domain.Rules;
using RESessionSheet.UI.Components;

namespace RESessionSheet.UI.Sheet.Panels;

/// <summary>
/// The 7-tier / 14-box wound track. ManualBoxes is the player-editable source of truth; the
/// Strain-derived overlay is recomputed on every load via WoundRules.GetEffectiveBoxes and never
/// persisted, so it can never desync. Armor Bonus HP is exposed as a directly-editable field for
/// V1 (Inventory doesn't auto-derive it from equipped armor — no per-item bonus-HP data field
/// exists yet, that's later-phase catalog work). Herb consumption reads real Inventory items,
/// identified by name (case-insensitive "Green Herb"/"Red Herb"/"Blue Herb"/"First Aid Spray") —
/// a simple convention rather than a new schema field, since Category/Name are already freeform.
/// </summary>
public partial class HealthPanel : FramedPanel
{
	private const string GreenHerbName = "Green Herb";
	private const string RedHerbName = "Red Herb";
	private const string BlueHerbName = "Blue Herb";
	private const string FirstAidSprayName = "First Aid Spray";

	private CappedCounter _woundTrack = null!;
	private SpinBox _strainSpin = null!;
	private SpinBox _armorBonusSpin = null!;
	private Label _incapacitatedLabel = null!;
	private Label _herbStatusLabel = null!;
	private SpinBox _greenHerbCount = null!;
	private SpinBox _redHerbCount = null!;
	private SpinBox _blueHerbCount = null!;
	private Button _firstAidSprayButton = null!;
	private bool _isLoading;

	private static CharacterSheet Sheet => GameState.Instance.CurrentCharacter!.Sheet;

	public override void _Ready()
	{
		base._Ready();
		BuildUi();
		LoadFromCharacter();
		GameState.Instance.InventoryChanged += RefreshHerbAvailability;
	}

	public override void _ExitTree()
	{
		if (GameState.Instance != null)
		{
			GameState.Instance.InventoryChanged -= RefreshHerbAvailability;
		}
	}

	private void BuildUi()
	{
		var vbox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		vbox.AddThemeConstantOverride("separation", 12);
		Content.AddChild(vbox);

		vbox.AddChild(new Label { Text = "Health — Wound Track" });

		var tierLabelsRow = new HBoxContainer();
		foreach (WoundTier tier in Enum.GetValues<WoundTier>())
		{
			tierLabelsRow.AddChild(new Label
			{
				Text = tier.ToString(),
				CustomMinimumSize = new Vector2(40, 0),
				HorizontalAlignment = HorizontalAlignment.Center,
			});
		}

		vbox.AddChild(tierLabelsRow);

		_woundTrack = new CappedCounter { BoxCount = HealthTrack.BoxCount, BoxColors = BuildTierColors() };
		_woundTrack.BoxToggled += OnBoxToggled;
		vbox.AddChild(_woundTrack);

		_incapacitatedLabel = new Label { Text = "" };
		vbox.AddChild(_incapacitatedLabel);

		vbox.AddChild(new HSeparator());

		var grid = new GridContainer { Columns = 2 };
		grid.AddThemeConstantOverride("h_separation", 12);
		grid.AddThemeConstantOverride("v_separation", 6);
		vbox.AddChild(grid);

		grid.AddChild(new Label { Text = "Strain" });
		_strainSpin = new SpinBox { MinValue = 0, MaxValue = 1000, Step = 1 };
		_strainSpin.ValueChanged += OnStrainChanged;
		grid.AddChild(_strainSpin);

		grid.AddChild(new Label { Text = "Armor Bonus HP" });
		_armorBonusSpin = new SpinBox { MinValue = 0, MaxValue = 10, Step = 1 };
		_armorBonusSpin.ValueChanged += OnArmorBonusChanged;
		grid.AddChild(_armorBonusSpin);

		vbox.AddChild(new HSeparator());
		vbox.AddChild(new Label { Text = "Herb Combos" });

		var herbRow = new HBoxContainer();
		herbRow.AddThemeConstantOverride("separation", 12);
		herbRow.AddChild(new Label { Text = "Green" });
		_greenHerbCount = new SpinBox { MinValue = 0, MaxValue = 3, Step = 1, CustomMinimumSize = new Vector2(60, 0) };
		herbRow.AddChild(_greenHerbCount);
		herbRow.AddChild(new Label { Text = "Red" });
		_redHerbCount = new SpinBox { MinValue = 0, MaxValue = 3, Step = 1, CustomMinimumSize = new Vector2(60, 0) };
		herbRow.AddChild(_redHerbCount);
		herbRow.AddChild(new Label { Text = "Blue" });
		_blueHerbCount = new SpinBox { MinValue = 0, MaxValue = 3, Step = 1, CustomMinimumSize = new Vector2(60, 0) };
		herbRow.AddChild(_blueHerbCount);

		var applyButton = new Button { Text = "Apply Herbs" };
		applyButton.Pressed += OnApplyHerbsPressed;
		herbRow.AddChild(applyButton);
		vbox.AddChild(herbRow);

		_firstAidSprayButton = new Button { Text = "Use First Aid Spray" };
		_firstAidSprayButton.Pressed += OnFirstAidSprayPressed;
		vbox.AddChild(_firstAidSprayButton);

		_herbStatusLabel = new Label { Text = "" };
		vbox.AddChild(_herbStatusLabel);
	}

	private static Color[] BuildTierColors()
	{
		var tierColors = new[]
		{
			new Color(0.55f, 0.65f, 0.25f), // Bruised
			new Color(0.75f, 0.7f, 0.2f),   // Hurt
			new Color(0.8f, 0.55f, 0.15f),  // Injured
			new Color(0.8f, 0.4f, 0.1f),    // Wounded
			new Color(0.75f, 0.25f, 0.1f),  // Mauled
			new Color(0.65f, 0.1f, 0.1f),   // Crippled
			new Color(0.4f, 0.05f, 0.4f),   // Incapacitated
		};

		var colors = new Color[HealthTrack.BoxCount];
		for (int i = 0; i < HealthTrack.BoxCount; i++)
		{
			colors[i] = tierColors[i / HealthTrack.BoxesPerTier];
		}

		return colors;
	}

	private void LoadFromCharacter()
	{
		_isLoading = true;
		var sheet = Sheet;

		_woundTrack.ManualBoxes = sheet.Health.ManualBoxes;
		_woundTrack.OverlayBoxes = WoundRules.GetEffectiveBoxes(sheet.Health, sheet.Strain);
		_strainSpin.Value = sheet.Strain.Value;
		_armorBonusSpin.Value = sheet.Health.BonusHpFromArmor;
		RefreshIncapacitatedLabel();
		RefreshHerbAvailability();

		_isLoading = false;
	}

	/// <summary>Catalog-linked items store their real name in the catalog, not in Name (which is
	/// blank for them) - resolve whichever one actually applies before matching by name.</summary>
	private static string EffectiveName(InventoryItem item) =>
		item.CatalogId != null
			? GameState.Instance.ItemCatalog.Find(item.CatalogId)?.Name ?? item.Name
			: item.Name;

	private static int CountItem(string name) =>
		Sheet.Inventory.Items
			.Where(i => string.Equals(EffectiveName(i).Trim(), name, StringComparison.OrdinalIgnoreCase))
			.Sum(i => i.Quantity);

	private void RefreshHerbAvailability()
	{
		int green = CountItem(GreenHerbName);
		int red = CountItem(RedHerbName);
		int blue = CountItem(BlueHerbName);
		int sprays = CountItem(FirstAidSprayName);

		_greenHerbCount.MaxValue = Math.Min(3, green);
		_greenHerbCount.Editable = green > 0;
		if (green <= 0)
		{
			_greenHerbCount.Value = 0;
		}
		else if (_greenHerbCount.Value > green)
		{
			_greenHerbCount.Value = green;
		}

		_redHerbCount.MaxValue = Math.Min(3, red);
		_redHerbCount.Editable = red > 0;
		if (red <= 0)
		{
			_redHerbCount.Value = 0;
		}
		else if (_redHerbCount.Value > red)
		{
			_redHerbCount.Value = red;
		}

		_blueHerbCount.MaxValue = Math.Min(3, blue);
		_blueHerbCount.Editable = blue > 0;
		if (blue <= 0)
		{
			_blueHerbCount.Value = 0;
		}
		else if (_blueHerbCount.Value > blue)
		{
			_blueHerbCount.Value = blue;
		}

		_firstAidSprayButton.Text = $"Use First Aid Spray ({sprays})";
		_firstAidSprayButton.Disabled = sprays <= 0;
	}

	private void OnApplyHerbsPressed()
	{
		int greenCount = (int)_greenHerbCount.Value;
		int redCount = (int)_redHerbCount.Value;
		int blueCount = (int)_blueHerbCount.Value;

		var colors = new List<HerbColor>();
		colors.AddRange(Enumerable.Repeat(HerbColor.Green, greenCount));
		colors.AddRange(Enumerable.Repeat(HerbColor.Red, redCount));
		colors.AddRange(Enumerable.Repeat(HerbColor.Blue, blueCount));

		var effect = HerbRecipes.Resolve(colors);
		if (effect == null)
		{
			_herbStatusLabel.Text = "That combination doesn't match a known herb recipe.";
			return;
		}

		ConsumeHerb(GreenHerbName, greenCount);
		ConsumeHerb(RedHerbName, redCount);
		ConsumeHerb(BlueHerbName, blueCount);

		ApplyHerbEffect(effect);

		_greenHerbCount.Value = 0;
		_redHerbCount.Value = 0;
		_blueHerbCount.Value = 0;

		LoadFromCharacter();
		GameState.Instance.NotifyHealthChanged();
		GameState.Instance.NotifyInventoryChanged();
	}

	private void ApplyHerbEffect(HerbEffect effect)
	{
		var sheet = Sheet;
		if (effect.HpHealed > 0)
		{
			WoundRules.ApplyHealing(sheet.Health, effect.HpHealed);
		}

		if (effect.StrainHealed > 0)
		{
			sheet.Strain.Value = Math.Max(0, sheet.Strain.Value - effect.StrainHealed);
		}

		var parts = new List<string>();
		if (effect.HpHealed > 0)
		{
			parts.Add($"+{effect.HpHealed} HP");
		}

		if (effect.StrainHealed > 0)
		{
			parts.Add($"+{effect.StrainHealed} Strain healed");
		}

		if (effect.GrantsSoak)
		{
			parts.Add("+1 Soak for 3 turns (not tracked — apply manually in combat)");
		}

		if (effect.GrantsPoisonImmunity)
		{
			parts.Add("Poison Immunity (not tracked — apply manually in combat)");
		}

		_herbStatusLabel.Text = "Applied: " + string.Join(", ", parts);
	}

	private void OnFirstAidSprayPressed()
	{
		if (CountItem(FirstAidSprayName) <= 0)
		{
			return;
		}

		ConsumeHerb(FirstAidSprayName, 1);
		WoundRules.FullReset(Sheet.Health, Sheet.Strain);

		_herbStatusLabel.Text = "First Aid Spray used — all wounds and Strain cleared.";
		LoadFromCharacter();
		GameState.Instance.NotifyHealthChanged();
		GameState.Instance.NotifyInventoryChanged();
	}

	private static void ConsumeHerb(string name, int count)
	{
		var items = Sheet.Inventory.Items;
		int remaining = count;
		for (int i = items.Count - 1; i >= 0 && remaining > 0; i--)
		{
			if (!string.Equals(EffectiveName(items[i]).Trim(), name, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			int take = Math.Min(remaining, items[i].Quantity);
			items[i].Quantity -= take;
			remaining -= take;
			if (items[i].Quantity <= 0)
			{
				items.RemoveAt(i);
			}
		}
	}

	private void RefreshIncapacitatedLabel()
	{
		var effective = WoundRules.GetEffectiveBoxes(Sheet.Health, Sheet.Strain);
		if (WoundRules.IsIncapacitatedSecondTick(effective))
		{
			_incapacitatedLabel.Text = "INCAPACITATED — party intervention required.";
		}
		else if (WoundRules.IsIncapacitated(effective))
		{
			_incapacitatedLabel.Text = "Incapacitated — save roll or ally assistance needed.";
		}
		else
		{
			_incapacitatedLabel.Text = "";
		}
	}

	private void OnBoxToggled(int index)
	{
		if (_isLoading)
		{
			return;
		}

		var boxes = Sheet.Health.ManualBoxes;
		boxes[index] = !boxes[index];
		LoadFromCharacter();
		GameState.Instance.NotifyHealthChanged();
	}

	private void OnStrainChanged(double value)
	{
		if (_isLoading)
		{
			return;
		}

		Sheet.Strain.Value = (int)value;
		LoadFromCharacter();
		GameState.Instance.NotifyHealthChanged();
	}

	private void OnArmorBonusChanged(double value)
	{
		if (_isLoading)
		{
			return;
		}

		Sheet.Health.BonusHpFromArmor = (int)value;
		GameState.Instance.NotifyHealthChanged();
	}
}
