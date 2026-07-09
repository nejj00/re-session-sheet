using Godot;

namespace RESessionSheet.UI.Components;

/// <summary>
/// A clickable 1..MaxValue dot-rating row (used for the 9 Attributes, 30 Skills, and 3 Virtues).
/// Click-to-set, not click-to-toggle: clicking dot N requests the rating be set to N.
/// This component never mutates game data itself — it only emits ValueRequested and waits for
/// the parent panel to accept or reject the change (e.g. via XpEngine) and update Value in response.
/// Rendering is procedural (drawn circles) for now; swapping in real dot art later only touches
/// _Draw, not the public Value/MaxValue/ReadOnly/ValueRequested contract.
/// </summary>
[Tool]
public partial class DotRating : Control
{
	[Signal]
	public delegate void ValueRequestedEventHandler(int newValue);

	private const float DotRadius = 7f;
	private const float DotSpacing = 20f;
	private const float DotPadding = 4f;

	private int _value;
	private int _maxValue = 6;

	[Export]
	public int Value
	{
		get => _value;
		set
		{
			_value = Mathf.Clamp(value, 0, MaxValue);
			QueueRedraw();
		}
	}

	[Export]
	public int MaxValue
	{
		get => _maxValue;
		set
		{
			_maxValue = Mathf.Max(1, value);
			UpdateMinimumSize();
			QueueRedraw();
		}
	}

	[Export] public bool ReadOnly { get; set; }

	public override Vector2 _GetMinimumSize()
	{
		return new Vector2(MaxValue * DotSpacing, DotRadius * 2 + DotPadding * 2);
	}

	public override void _Draw()
	{
		var filledColor = new Color(0.72f, 0.16f, 0.16f);
		var emptyColor = new Color(0.32f, 0.32f, 0.34f);
		var borderColor = new Color(0.12f, 0.12f, 0.12f);

		for (int i = 0; i < MaxValue; i++)
		{
			var center = new Vector2(DotPadding + DotRadius + i * DotSpacing, Size.Y / 2f);
			bool filled = i < Value;
			DrawCircle(center, DotRadius, filled ? filledColor : emptyColor);
			DrawArc(center, DotRadius, 0f, Mathf.Tau, 24, borderColor, 1.2f, true);
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
			int clickedDot = (int)(mouseButton.Position.X / DotSpacing) + 1;
			clickedDot = Mathf.Clamp(clickedDot, 1, MaxValue);
			EmitSignal(SignalName.ValueRequested, clickedDot);
			AcceptEvent();
		}
	}
}
