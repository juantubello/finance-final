namespace FinanzasApp.Models;

public record ParsedExpense
{
    public string CardholderName { get; init; } = "";
    public string Date { get; init; } = "";           // "2026-02-05"
    public string Description { get; init; } = "";
    public int? InstallmentNumber { get; init; }
    public int? InstallmentTotal { get; init; }
    public long? AmountArs { get; init; }             // centavos, null si es gasto en USD
    public long? AmountUsd { get; init; }             // cents,    null si es gasto en ARS
}

public record ParsedCardStatement
{
    public string CardType { get; init; } = "";       // "VISA" | "MASTERCARD"
    public int StatementMonth { get; init; }
    public int StatementYear { get; init; }
    public string CloseDate { get; init; } = "";      // "2026-02-26"
    public string DueDate { get; init; } = "";        // "2026-03-06"
    public string? NextCloseDate { get; init; }
    public string? NextDueDate { get; init; }
    public List<ParsedExpense> Expenses { get; init; } = [];
}
