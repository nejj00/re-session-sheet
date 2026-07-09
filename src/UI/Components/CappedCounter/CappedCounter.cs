using Godot;

namespace RESessionSheet.UI.Components;

/// <summary>
/// A row of clickable boxes/ticks — used for the 14-box HP track. Click-to-toggle: clicking box i
/// emits BoxToggled(i) and waits for the parent to decide how to mutate real data and update
/// ManualBoxes in response; this component never mutates game data itself.
/// OverlayBoxes (e.g. Strain's computed overlay from WoundRules.GetEffectiveBoxes) render with a
/// distinct tint from manually-marked boxes but are never clickable/manual themselves.
/// BoxColors lets the parent tint boxes per group (e.g. wound tiers) without this component
/// knowing what a "tier" is, keeping it reusable for any capped/box-tracked resource.
/// </summary>
[Tool]
public partial class CappedCounter : Control
{
	[Signal]
	public delegate void BoxToggledEventHandler(int index);

	private const float BoxSize = 16f;
	private const float BoxSpacing = 20f;
	private const float BoxPadding = 4f;

	private int _boxCount = 14;

	[Export]
	public int BoxCount
	{
		get => _boxCount;
		set
		{
			_boxCount = Mathf.Max(1, value);
			UpdateMinimumSize();
			QueueRedraw();
		}
	}

	private bool[] _manualBoxes = new bool[14];

	public bool[] ManualBoxes
	{
		get => _manualBoxes;
		set
		{
			_manualBoxes = value;
			QueueRedraw();
		}
	}

	private bool[]? _overlayBoxes;

	public bool[]? OverlayBoxes
	{
		get => _overlayBoxes;
		set
		{
			_overlayBoxes = value;
			QueueRedraw();
		}
	}

	private Color[]? _boxColors;

	public Color[]? BoxColors
	{
		get => _boxColors;
		set
		{
			_boxColors = value;
			QueueRedraw();
		}
	}

	[Export] public bool ReadOnly { get; set; }

	public override Vector2 _GetMinimumSize()
	{
		return new Vector2(BoxCount * BoxSpacing, BoxSize + BoxPadding * 2);
	}

	public override void _Draw()
	{
		var defaultFilled = new Color(0.72f, 0.16f, 0.16f);
		var overlayFilled = new Color(0.85f, 0.55f, 0.15f);
		var emptyColor = new Color(0.28f, 0.28f, 0.30f);
		var borderColor = new Color(0.12f, 0.12f, 0.12f);

		for (int i = 0; i < BoxCount; i++)
		{
			var rect = new Rect2(BoxPadding + i * BoxSpacing, (Size.Y - BoxSize) / 2f, BoxSize, BoxSize);

			bool manual = i < _manualBoxes.Length && _manualBoxes[i];
			bool overlay = !manual && _overlayBoxes != null && i < _overlayBoxes.Length && _overlayBoxes[i];

			Color fill;
			if (manual)
			{
				fill = _boxColors != null && i < _boxColors.Length ? _boxColors[i] : defaultFilled;
			}
			else if (overlay)
			{
				fill = overlayFilled;
			}
			else
			{
				fill = emptyColor;
			}

			DrawRect(rect, fill, filled: true);
			DrawRect(rect, borderColor, filled: false, width: 1.2f);
		}
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (ReadOnly)
		{
			return;
		}

		if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouseButton)
		{
			int index = (int)(mouseButton.Position.X / BoxSpacing);
			index = Mathf.Clamp(index, 0, BoxCount - 1);
			EmitSignal(SignalName.BoxToggled, index);
			AcceptEvent();
		}
	}
}
