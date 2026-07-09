namespace RESessionSheet.Domain.Model;

/// <summary>
/// The 7-tier wound track. Each tier occupies 2 boxes (14 boxes total).
/// </summary>
public enum WoundTier
{
	Bruised,
	Hurt,
	Injured,
	Wounded,
	Mauled,
	Crippled,
	Incapacitated
}

public class HealthTrack
{
	public const int BoxCount = 14;
	public const int BoxesPerTier = 2;

	/// <summary>
	/// Player-marked ticks, authoritative. Index 0..13, two boxes per WoundTier in tier order.
	/// The strain-derived overlay (see WoundRules.GetEffectiveBoxes) is computed on read and
	/// never stored here, so it can never desync from Strain.Value.
	/// </summary>
	public bool[] ManualBoxes { get; set; } = new bool[BoxCount];

	/// <summary>Bonus HP granted by equipped armor (Light +1, Medium +2, Heavy +3, Special +4).</summary>
	public int BonusHpFromArmor { get; set; }
}
