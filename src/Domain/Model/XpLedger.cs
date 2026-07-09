namespace RESessionSheet.Domain.Model;

public class XpLedger
{
	public int Spent { get; set; }
	public int Available { get; set; }

	public int Total => Spent + Available;
}
