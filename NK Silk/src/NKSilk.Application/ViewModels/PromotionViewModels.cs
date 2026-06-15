namespace NKSilk.Application.ViewModels;

/// <summary>One cart line fed into the promotion engine.</summary>
public class PromotionLineVm
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

/// <summary>Outcome of evaluating offers + combos against a set of cart lines.</summary>
public class PromotionResultVm
{
    /// <summary>Effective unit price per input line, in the same order.</summary>
    public IReadOnlyList<decimal> EffectiveUnitPrices { get; set; } = new List<decimal>();
    /// <summary>Optional offer label per input line (null when no offer applies).</summary>
    public IReadOnlyList<string?> LineOfferTitles { get; set; } = new List<string?>();

    public decimal OfferSavings { get; set; }
    public decimal ComboSavings { get; set; }
    public IReadOnlyList<string> AppliedCombos { get; set; } = new List<string>();

    public decimal TotalSavings => OfferSavings + ComboSavings;
}
