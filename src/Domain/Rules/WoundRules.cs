using RESessionSheet.Domain.Model;

namespace RESessionSheet.Domain.Rules;

/// <summary>
/// Rules for the 7-tier / 14-box wound track (HealthTrack) and its interaction with Strain.
/// Confirmed from the rulebook's "Damage &amp; Health System" sheet: every 10 accumulated Strain
/// auto-applies one additional wound-tier tick, layered on top of the player's manual marks as a
/// visual overlay that is computed on read and never persisted or written back into ManualBoxes.
/// </summary>
public static class WoundRules
{
	/// <summary>
	/// Damage cost incurred per tick within each tier, in WoundTier order.
	/// Incapacitated (-1) is a sentinel: it requires a save roll or ally assistance rather than
	/// a flat damage cost, and is not auto-resolved here.
	/// </summary>
	public static readonly int[] DamageCostPerTier = { 0, 1, 1, 2, 2, 5, -1 };

	public static WoundTier TierOfBox(int boxIndex)
	{
		if (boxIndex < 0 || boxIndex >= HealthTrack.BoxCount)
		{
			throw new ArgumentOutOfRangeException(nameof(boxIndex), boxIndex, $"Box index must be between 0 and {HealthTrack.BoxCount - 1}.");
		}

		return (WoundTier)(boxIndex / HealthTrack.BoxesPerTier);
	}

	/// <summary>
	/// The effective (rendered) box state: each manually-marked box, plus a Strain-derived overlay
	/// that stacks on top of the highest manually-marked box (i.e. Strain pushes further down the
	/// track rather than redundantly re-filling already-marked boxes). Never mutates ManualBoxes.
	/// </summary>
	public static bool[] GetEffectiveBoxes(HealthTrack health, Strain strain)
	{
		var effective = (bool[])health.ManualBoxes.Clone();

		int highestManualIndex = -1;
		for (int i = 0; i < effective.Length; i++)
		{
			if (effective[i])
			{
				highestManualIndex = i;
			}
		}

		int ticks = strain.TicksFromStrain;
		int start = highestManualIndex + 1;
		for (int i = start; i < start + ticks && i < effective.Length; i++)
		{
			effective[i] = true;
		}

		return effective;
	}

	/// <summary>
	/// True once the effective box state reaches into the Incapacitated tier. The caller is
	/// responsible for the save-roll / ally-assistance resolution — this only detects the state.
	/// </summary>
	public static bool IsIncapacitated(bool[] effectiveBoxes)
	{
		int incapacitatedStart = (int)WoundTier.Incapacitated * HealthTrack.BoxesPerTier;
		for (int i = incapacitatedStart; i < effectiveBoxes.Length; i++)
		{
			if (effectiveBoxes[i])
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// True once both Incapacitated boxes are ticked — per the rulebook, the second tick removes
	/// self-saves entirely and requires party intervention.
	/// </summary>
	public static bool IsIncapacitatedSecondTick(bool[] effectiveBoxes)
	{
		int incapacitatedStart = (int)WoundTier.Incapacitated * HealthTrack.BoxesPerTier;
		for (int i = incapacitatedStart; i < effectiveBoxes.Length; i++)
		{
			if (!effectiveBoxes[i])
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Clears ManualBoxes ticks from the most severe end backward ("worst wound to best", per the
	/// rulebook), consuming DamageCostPerTier healing points per box — Bruised boxes are free.
	/// Incapacitated boxes have no defined healing cost in the rulebook (only a damage-taking
	/// special case), so each one is treated as costing 1 point, a reasonable default absent
	/// other guidance.
	/// </summary>
	public static void ApplyHealing(HealthTrack health, int healingPoints)
	{
		int remaining = healingPoints;
		for (int i = health.ManualBoxes.Length - 1; i >= 0; i--)
		{
			if (!health.ManualBoxes[i])
			{
				continue;
			}

			int cost = DamageCostPerTier[(int)TierOfBox(i)];
			if (cost < 0)
			{
				cost = 1;
			}

			if (cost > remaining)
			{
				break;
			}

			health.ManualBoxes[i] = false;
			remaining -= cost;
		}
	}

	/// <summary>Fully resets the wound track and strain — First Aid Spray's effect.</summary>
	public static void FullReset(HealthTrack health, Strain strain)
	{
		Array.Clear(health.ManualBoxes);
		strain.Value = 0;
	}
}
