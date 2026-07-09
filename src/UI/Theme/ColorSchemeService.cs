using Godot;

namespace RESessionSheet.UI.Theme;

/// <summary>
/// Named color scheme: a base accent color plus HSV-rotation parameters, mirroring Flare's
/// colorschemes.py (primary hex, hue, saturation, value). V1 ships a single scheme — this is
/// architecture-only scaffolding so additional schemes are a data-add later, not a rework.
/// </summary>
public record ColorScheme(string Name, Color PrimaryColor, float HueShift = 0f, float SaturationScale = 1f, float ValueScale = 1f);

public static class ColorSchemeService
{
	public static readonly ColorScheme Default = new("Default", new Color(0.72f, 0.16f, 0.16f));

	public static readonly IReadOnlyList<ColorScheme> Schemes = new[] { Default };

	/// <summary>
	/// Applies an HSV shift derived from the given scheme to a base color — the same mechanism
	/// Flare uses to recolor decorative art from a single base asset per theme, rather than
	/// shipping N pre-rendered asset sets.
	/// </summary>
	public static Color ApplyHueShift(Color baseColor, ColorScheme scheme)
	{
		float hue = Mathf.PosMod(baseColor.H + scheme.HueShift, 1f);
		float saturation = Mathf.Clamp(baseColor.S * scheme.SaturationScale, 0f, 1f);
		float value = Mathf.Clamp(baseColor.V * scheme.ValueScale, 0f, 1f);
		return Color.FromHsv(hue, saturation, value, baseColor.A);
	}
}
