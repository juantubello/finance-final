namespace FinanzasApp.Models;

public class LabelRule
{
    public int Id { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public List<Label> Labels { get; set; } = new();
}

public class LabelRuleRequest
{
    public string Keyword { get; set; } = string.Empty;
    public List<int> LabelIds { get; set; } = new();
}
