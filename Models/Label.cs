namespace FinanzasApp.Models;

public class Label
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AgregarLabelsRequest
{
    public List<string> Labels { get; set; } = new();
}

public class AgregarLabelsResponse
{
    public int GastoId { get; set; }
    public List<Label> Labels { get; set; } = new();
}

public class CategoryRule
{
    public int Id { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class CategoryRuleRequest
{
    public string Keyword { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}
