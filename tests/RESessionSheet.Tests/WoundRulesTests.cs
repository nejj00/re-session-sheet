using RESessionSheet.Domain.Model;
using RESessionSheet.Domain.Rules;

namespace RESessionSheet.Tests;

public class WoundRulesTests
{
	[Theory]
	[InlineData(0, WoundTier.Bruised)]
	[InlineData(1, WoundTier.Bruised)]
	[InlineData(2, WoundTier.Hurt)]
	[InlineData(3, WoundTier.Hurt)]
	[InlineData(12, WoundTier.Incapacitated)]
	[InlineData(13, WoundTier.Incapacitated)]
	public void TierOfBox_MapsBoxIndexToCorrectTier(int boxIndex, WoundTier expected)
	{
		Assert.Equal(expected, WoundRules.TierOfBox(boxIndex));
	}

	[Fact]
	public void GetEffectiveBoxes_NoManualDamageNoStrain_AllEmpty()
	{
		var health = new HealthTrack();
		var strain = new Strain { Value = 0 };

		var effective = WoundRules.GetEffectiveBoxes(health, strain);

		Assert.All(effective, b => Assert.False(b));
	}

	[Theory]
	[InlineData(9, 0)]   // just under the first boundary — no tick yet
	[InlineData(10, 1)]  // exactly at the boundary — one tick
	[InlineData(19, 1)]  // just under the next boundary
	[InlineData(20, 2)]  // exactly at the boundary — two ticks
	[InlineData(25, 2)]
	[InlineData(30, 3)]
	public void Strain_TicksFromStrain_MatchesEvery10PointBoundary(int strainValue, int expectedTicks)
	{
		var strain = new Strain { Value = strainValue };
		Assert.Equal(expectedTicks, strain.TicksFromStrain);
	}

	[Fact]
	public void GetEffectiveBoxes_StrainOnly_FillsFromStartOfTrack()
	{
		var health = new HealthTrack();
		var strain = new Strain { Value = 25 }; // 2 ticks

		var effective = WoundRules.GetEffectiveBoxes(health, strain);

		Assert.True(effective[0]);
		Assert.True(effective[1]);
		Assert.False(effective[2]);
	}

	[Fact]
	public void GetEffectiveBoxes_StrainStacksOnTopOfManualBoxes_NotOverlappingThem()
	{
		var health = new HealthTrack();
		health.ManualBoxes[0] = true;
		health.ManualBoxes[1] = true;
		health.ManualBoxes[2] = true; // 3 manual boxes marked (Bruised x2, Hurt x1)
		var strain = new Strain { Value = 10 }; // 1 tick, should land on box 3, not re-mark box 0-2

		var effective = WoundRules.GetEffectiveBoxes(health, strain);

		Assert.True(effective[0]);
		Assert.True(effective[1]);
		Assert.True(effective[2]);
		Assert.True(effective[3]); // strain-derived tick stacked on top
		Assert.False(effective[4]);
	}

	[Fact]
	public void GetEffectiveBoxes_NeverMutatesManualBoxes()
	{
		var health = new HealthTrack();
		health.ManualBoxes[0] = true;
		var strain = new Strain { Value = 50 };

		WoundRules.GetEffectiveBoxes(health, strain);

		Assert.True(health.ManualBoxes[0]);
		Assert.False(health.ManualBoxes[1]);
		Assert.All(health.ManualBoxes[2..], b => Assert.False(b));
	}

	[Fact]
	public void GetEffectiveBoxes_StrainOverflowBeyondTrack_CapsAtLastBox_DoesNotThrow()
	{
		var health = new HealthTrack();
		var strain = new Strain { Value = 1000 }; // way more ticks than boxes available

		var effective = WoundRules.GetEffectiveBoxes(health, strain);

		Assert.All(effective, Assert.True);
	}

	[Fact]
	public void IsIncapacitated_FalseWhenBelowIncapacitatedTier()
	{
		var effective = new bool[HealthTrack.BoxCount];
		for (int i = 0; i < 11; i++) effective[i] = true; // fills through Crippled

		Assert.False(WoundRules.IsIncapacitated(effective));
	}

	[Fact]
	public void IsIncapacitated_TrueOnFirstIncapacitatedTick()
	{
		var effective = new bool[HealthTrack.BoxCount];
		for (int i = 0; i < 13; i++) effective[i] = true; // first Incapacitated box (index 12) ticked

		Assert.True(WoundRules.IsIncapacitated(effective));
		Assert.False(WoundRules.IsIncapacitatedSecondTick(effective));
	}

	[Fact]
	public void IsIncapacitatedSecondTick_TrueOnlyWhenBothIncapacitatedBoxesTicked()
	{
		var effective = new bool[HealthTrack.BoxCount];
		for (int i = 0; i < HealthTrack.BoxCount; i++) effective[i] = true;

		Assert.True(WoundRules.IsIncapacitated(effective));
		Assert.True(WoundRules.IsIncapacitatedSecondTick(effective));
	}

	[Fact]
	public void ApplyHealing_ClearsBruisedBoxesForFree()
	{
		var health = new HealthTrack();
		health.ManualBoxes[0] = true;
		health.ManualBoxes[1] = true; // both Bruised boxes, cost 0 each

		WoundRules.ApplyHealing(health, 0);

		Assert.False(health.ManualBoxes[0]);
		Assert.False(health.ManualBoxes[1]);
	}

	[Fact]
	public void ApplyHealing_ClearsFromMostSevereBoxBackward_StoppingAtFirstUnaffordableBox()
	{
		var health = new HealthTrack();
		health.ManualBoxes[0] = true; // Bruised, cost 0
		health.ManualBoxes[2] = true; // Hurt, cost 1
		health.ManualBoxes[4] = true; // Injured, cost 1

		WoundRules.ApplyHealing(health, 1); // enough for exactly one paid box

		Assert.False(health.ManualBoxes[4]); // worst wound healed first
		// Healing is a strict worst-to-best sequence: once box 2 can't be afforded, healing stops
		// there even though box 0 (free) comes later — you can't skip a worse unhealed wound.
		Assert.True(health.ManualBoxes[2]);
		Assert.True(health.ManualBoxes[0]);
	}

	[Fact]
	public void ApplyHealing_FreeBoxesBetweenAffordableOnes_AllGetCleared()
	{
		var health = new HealthTrack();
		health.ManualBoxes[0] = true; // Bruised, cost 0 — worst wound is here, nothing above it
		health.ManualBoxes[1] = true; // Bruised, cost 0

		WoundRules.ApplyHealing(health, 0);

		Assert.False(health.ManualBoxes[0]);
		Assert.False(health.ManualBoxes[1]);
	}

	[Fact]
	public void ApplyHealing_StopsWhenNotEnoughPointsForNextBox()
	{
		var health = new HealthTrack();
		health.ManualBoxes[10] = true; // Crippled, cost 5

		WoundRules.ApplyHealing(health, 4); // not enough to clear a cost-5 box

		Assert.True(health.ManualBoxes[10]);
	}

	[Fact]
	public void ApplyHealing_ExactAmount_ClearsBox()
	{
		var health = new HealthTrack();
		health.ManualBoxes[10] = true; // Crippled, cost 5

		WoundRules.ApplyHealing(health, 5);

		Assert.False(health.ManualBoxes[10]);
	}

	[Fact]
	public void ApplyHealing_NoWoundsMarked_DoesNothing()
	{
		var health = new HealthTrack();
		WoundRules.ApplyHealing(health, 100);
		Assert.All(health.ManualBoxes, Assert.False);
	}

	[Fact]
	public void FullReset_ClearsAllManualBoxesAndStrain()
	{
		var health = new HealthTrack();
		for (int i = 0; i < HealthTrack.BoxCount; i++) health.ManualBoxes[i] = true;
		var strain = new Strain { Value = 47 };

		WoundRules.FullReset(health, strain);

		Assert.All(health.ManualBoxes, Assert.False);
		Assert.Equal(0, strain.Value);
	}

	[Fact]
	public void DamageCostPerTier_MatchesConfirmedRulebookValues()
	{
		Assert.Equal(0, WoundRules.DamageCostPerTier[(int)WoundTier.Bruised]);
		Assert.Equal(1, WoundRules.DamageCostPerTier[(int)WoundTier.Hurt]);
		Assert.Equal(1, WoundRules.DamageCostPerTier[(int)WoundTier.Injured]);
		Assert.Equal(2, WoundRules.DamageCostPerTier[(int)WoundTier.Wounded]);
		Assert.Equal(2, WoundRules.DamageCostPerTier[(int)WoundTier.Mauled]);
		Assert.Equal(5, WoundRules.DamageCostPerTier[(int)WoundTier.Crippled]);
		Assert.Equal(-1, WoundRules.DamageCostPerTier[(int)WoundTier.Incapacitated]);
	}
}
