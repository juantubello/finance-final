using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using FinanzasApp.Models;

namespace FinanzasApp.Services;

public class BbvaPdfParser
{
    // Detecta líneas de consumo: DD-Mon-YY ... cupón 6 dígitos ... monto
    private static readonly Regex ExpenseLineRegex = new(
        @"^(\d{2}-[A-Za-z]{3}-\d{2})\s+(.+)\s+(\d{6})\s+(-?[\d.]+,\d{2})$",
        RegexOptions.Compiled);

    // "C.04/06" dentro de la descripción
    private static readonly Regex InstallmentRegex = new(
        @"\bC\.(\d{2})/(\d{2})\b",
        RegexOptions.Compiled);

    // Detecta cualquier indicador de divisa extranjera (incluso pegado al ID: "muMXYTIzTUSD")
    private static readonly Regex ForeignCurrencyPresenceRegex = new(
        @"(USD|EUR|GBP|CHF|BRL)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Fecha en formato argentino: DD-Mon-YY
    private static readonly Regex DatePattern = new(
        @"\d{2}-[A-Za-z]{3}-\d{2}",
        RegexOptions.Compiled);

    private static readonly Dictionary<string, string> CardholderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["J Fernandez Tubello"]  = "Juan",
        ["Camila V Montiel"]     = "Camila"
    };

    private static readonly Dictionary<string, int> MonthMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Ene"] = 1, ["Feb"] = 2, ["Mar"] = 3, ["Abr"] = 4,
        ["May"] = 5, ["Jun"] = 6, ["Jul"] = 7, ["Ago"] = 8,
        ["Sep"] = 9, ["Oct"] = 10, ["Nov"] = 11, ["Dic"] = 12
    };

    // -----------------------------------------------------------------------

    public ParsedCardStatement Parse(byte[] pdfBytes)
    {
        var lines = ExtractLines(pdfBytes);

        var cardType = DetectCardType(lines);

        ExtractStatementDates(lines,
            out var rawCloseDate, out var rawDueDate,
            out var rawNextClose, out var rawNextDue);

        var closeDate  = rawCloseDate  is not null ? ParseArgDate(rawCloseDate)  : null;
        var dueDate    = rawDueDate    is not null ? ParseArgDate(rawDueDate)    : null;
        var nextClose  = rawNextClose  is not null ? ParseArgDate(rawNextClose)  : null;
        var nextDue    = rawNextDue    is not null ? ParseArgDate(rawNextDue)    : null;

        int statementMonth = 0, statementYear = 0;
        if (closeDate is not null)
        {
            var parts = closeDate.Split('-');
            statementYear  = int.Parse(parts[0]);
            statementMonth = int.Parse(parts[1]);
        }

        return new ParsedCardStatement
        {
            CardType       = cardType,
            StatementMonth = statementMonth,
            StatementYear  = statementYear,
            CloseDate      = closeDate  ?? "",
            DueDate        = dueDate    ?? "",
            NextCloseDate  = nextClose,
            NextDueDate    = nextDue,
            Expenses       = ParseExpenses(lines)
        };
    }

    // -----------------------------------------------------------------------
    // Extracción de líneas del PDF
    // Agrupa palabras por proximidad de coordenada Y usando clustering:
    // nueva línea cuando el salto de Y es mayor a lineGap unidades.
    // -----------------------------------------------------------------------

    private static List<string> ExtractLines(byte[] pdfBytes)
    {
        using var doc = PdfDocument.Open(pdfBytes);
        var result = new List<string>();
        const double lineGap = 3.0;

        foreach (var page in doc.GetPages())
        {
            var words = page.GetWords()
                .Select(w => (X: w.BoundingBox.Left, Y: w.BoundingBox.Bottom, Text: w.Text))
                .OrderByDescending(w => w.Y) // arriba → abajo
                .ToList();

            if (words.Count == 0) continue;

            // Clustering: una nueva línea empieza cuando el salto en Y supera lineGap
            var lineGroups = new List<List<(double X, double Y, string Text)>>();
            var current    = new List<(double X, double Y, string Text)> { words[0] };

            for (int i = 1; i < words.Count; i++)
            {
                var avgY = current.Average(w => w.Y);
                if (Math.Abs(words[i].Y - avgY) <= lineGap)
                    current.Add(words[i]);
                else
                {
                    lineGroups.Add(current);
                    current = [words[i]];
                }
            }
            lineGroups.Add(current);

            result.AddRange(lineGroups
                .Select(g => string.Join(" ", g.OrderBy(w => w.X).Select(w => w.Text)).Trim())
                .Where(l => l.Length > 0));
        }

        return result;
    }

    // -----------------------------------------------------------------------
    // Extracción de fechas del header
    //
    // El PDF tiene dos tablas separadas:
    //
    // Tabla 1 (período actual):
    //   Fila etiquetas: "CIERRE ACTUAL   VENCIMIENTO ACTUAL   ..."
    //   Fila valores:   "26-Feb-26       06-Mar-26            ..."
    //
    // Tabla 2 (Otros períodos):
    //   Fila etiquetas: "CIERRE ANTERIOR  VENCIMIENTO ANTERIOR  PRÓXIMO CIERRE  PRÓXIMO VENCIMIENTO"
    //   Fila valores:   "29-Ene-26        06-Feb-26             26-Mar-26       09-Abr-26"
    //
    // Como las etiquetas y valores están en FILAS DISTINTAS, buscamos la etiqueta
    // y luego miramos las líneas siguientes para encontrar las fechas.
    // -----------------------------------------------------------------------

    private static void ExtractStatementDates(
        List<string> lines,
        out string? closeDate,  out string? dueDate,
        out string? nextClose,  out string? nextDue)
    {
        closeDate = dueDate = nextClose = nextDue = null;

        for (int i = 0; i < lines.Count - 1; i++)
        {
            var line = lines[i];

            // Tabla del período actual
            if (line.Contains("CIERRE ACTUAL", StringComparison.OrdinalIgnoreCase) &&
                !line.Contains("ANTERIOR",     StringComparison.OrdinalIgnoreCase))
            {
                // La fila siguiente debería tener: close_date  due_date  ...
                for (int j = i + 1; j < Math.Min(i + 4, lines.Count); j++)
                {
                    var dates = DatePattern.Matches(lines[j]);
                    if (dates.Count >= 2)
                    {
                        closeDate = dates[0].Value;
                        dueDate   = dates[1].Value;
                        break;
                    }
                    if (dates.Count == 1 && closeDate is null)
                        closeDate = dates[0].Value;
                }
            }

            // Tabla "Otros períodos" - etiqueta PRÓXIMO CIERRE
            if (line.Contains("PRÓXIMO CIERRE",  StringComparison.OrdinalIgnoreCase) ||
                line.Contains("PROXIMO CIERRE",  StringComparison.OrdinalIgnoreCase) ||
                line.Contains("PR\u00d3XIMO CIERRE", StringComparison.OrdinalIgnoreCase))
            {
                // La fila siguiente: close_ant  due_ant  next_close  next_due
                for (int j = i + 1; j < Math.Min(i + 4, lines.Count); j++)
                {
                    var dates = DatePattern.Matches(lines[j]);
                    if (dates.Count >= 4)
                    {
                        nextClose = dates[2].Value;
                        nextDue   = dates[3].Value;
                        break;
                    }
                    // Si hay menos de 4 fechas en la fila, tomamos las que haya
                    if (dates.Count >= 2)
                    {
                        nextClose = dates[0].Value;
                        nextDue   = dates[1].Value;
                        break;
                    }
                }
            }
        }
    }

    // -----------------------------------------------------------------------

    private static string DetectCardType(List<string> lines)
    {
        foreach (var line in lines.Take(30))
        {
            if (line.Contains("Visa",       StringComparison.OrdinalIgnoreCase)) return "VISA";
            if (line.Contains("Mastercard", StringComparison.OrdinalIgnoreCase)) return "MASTERCARD";
        }
        return "UNKNOWN";
    }

    // -----------------------------------------------------------------------
    // State machine: detecta secciones "Consumos [Nombre]" y parsea cada línea
    // -----------------------------------------------------------------------

    private static List<ParsedExpense> ParseExpenses(List<string> lines)
    {
        var expenses = new List<ParsedExpense>();
        string? currentCardholder = null;
        bool inExpenseSection = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("Consumos ", StringComparison.OrdinalIgnoreCase))
            {
                var rawName = line["Consumos ".Length..].Trim();
                currentCardholder = CardholderAliases.TryGetValue(rawName, out var alias)
                    ? alias
                    : rawName;
                inExpenseSection  = true;
                continue;
            }

            if (line.StartsWith("TOTAL CONSUMOS",    StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Impuestos, cargos", StringComparison.OrdinalIgnoreCase))
            {
                inExpenseSection  = false;
                currentCardholder = null;
                continue;
            }

            if (!inExpenseSection || currentCardholder is null) continue;

            var expense = TryParseExpenseLine(line, currentCardholder);
            if (expense is not null)
                expenses.Add(expense);
        }

        return expenses;
    }

    private static ParsedExpense? TryParseExpenseLine(string line, string cardholderName)
    {
        var m = ExpenseLineRegex.Match(line.Trim());
        if (!m.Success) return null;

        var rawDate     = m.Groups[1].Value;
        var description = m.Groups[2].Value.Trim();
        var rawAmount   = m.Groups[4].Value;

        // 1. Detectar divisa extranjera ANTES de limpiar la descripción
        bool isForeign = ForeignCurrencyPresenceRegex.IsMatch(description);

        // 2. Extraer info de cuotas
        int? installmentNumber = null, installmentTotal = null;
        var installMatch = InstallmentRegex.Match(description);
        if (installMatch.Success)
        {
            installmentNumber = int.Parse(installMatch.Groups[1].Value);
            installmentTotal  = int.Parse(installMatch.Groups[2].Value);
            description       = InstallmentRegex.Replace(description, "").Trim();
        }

        // 3. Limpiar monto embebido al final de la descripción
        //    (el banco lo repite visualmente en la misma celda para transacciones en USD)
        //    Ej: "Spotify USD 3,84" → "Spotify USD"
        description = Regex.Replace(description, @"\s+[\d.]+,\d{2}$", "").Trim();
        description = Regex.Replace(description, @"\s{2,}", " ").Trim();

        var amount = ParseArgAmount(rawAmount);
        var date   = ParseArgDate(rawDate) ?? rawDate;

        return new ParsedExpense
        {
            CardholderName    = cardholderName,
            Date              = date,
            Description       = description,
            InstallmentNumber = installmentNumber,
            InstallmentTotal  = installmentTotal,
            AmountArs         = isForeign ? null  : amount,
            AmountUsd         = isForeign ? amount : null
        };
    }

    // -----------------------------------------------------------------------
    // Helpers de conversión
    // -----------------------------------------------------------------------

    // "26-Feb-26" → "2026-02-26"
    private static string? ParseArgDate(string argDate)
    {
        var parts = argDate.Split('-');
        if (parts.Length != 3) return null;
        if (!int.TryParse(parts[0], out var day))           return null;
        if (!MonthMap.TryGetValue(parts[1], out var month)) return null;
        if (!int.TryParse(parts[2], out var shortYear))     return null;
        return $"{2000 + shortYear:D4}-{month:D2}-{day:D2}";
    }

    // "1.234,56" → 123456 (centavos) | "-838,93" → -83893
    private static long ParseArgAmount(string raw)
    {
        var negative   = raw.StartsWith('-');
        var normalized = raw.TrimStart('-').Replace(".", "").Replace(",", "");
        var value      = long.TryParse(normalized, out var r) ? r : 0;
        return negative ? -value : value;
    }
}
