namespace AI.Workshop.Common.Toon;

/// <summary>
/// Formats search results as TOON for reduced token usage
/// </summary>
public static class ToonSearchFormatter
{
    /// <summary>
    /// Formats search results as TOON instead of XML
    /// </summary>
    /// <typeparam name="T">Type of search result</typeparam>
    /// <param name="results">Search results</param>
    /// <param name="resultMapper">Function to map result to SearchResultData</param>
    /// <returns>TOON-formatted results string</returns>
    public static string FormatSearchResults<T>(
        IEnumerable<T> results,
        Func<T, SearchResultData> resultMapper)
    {
        var data = results.Select(resultMapper).ToArray();
        
        if (data.Length == 0)
        {
            return "results[0]:";
        }

        return ToonHelper.ToToon(new { results = data });
    }

    /// <summary>
    /// Formats search results with token comparison
    /// </summary>
    /// <typeparam name="T">Type of search result</typeparam>
    /// <param name="results">Search results</param>
    /// <param name="resultMapper">Function to map result to SearchResultData</param>
    /// <returns>Formatted results with token stats</returns>
    public static ToonSearchResult<T> FormatWithStats<T>(
        IEnumerable<T> results,
        Func<T, SearchResultData> resultMapper)
    {
        var data = results.Select(resultMapper).ToArray();
        var wrapper = new { results = data };
        
        var toon = ToonHelper.ToToon(wrapper);
        var savings = ToonHelper.EstimateTokenSavings(wrapper);

        return new ToonSearchResult<T>(
            ToonFormatted: toon,
            OriginalResults: data,
            TokenSavings: savings);
    }

    /// <summary>
    /// Formats a single search result as XML (traditional format)
    /// </summary>
    public static string FormatAsXml(SearchResultData result)
    {
        return $"<result filename=\"{result.Filename}\" page_number=\"{result.PageNumber}\">{result.Text}</result>";
    }

    /// <summary>
    /// Formats multiple search results as XML (traditional format)
    /// </summary>
    public static IEnumerable<string> FormatAllAsXml(IEnumerable<SearchResultData> results)
    {
        return results.Select(FormatAsXml);
    }

    /// <summary>
    /// Compares TOON vs XML format for the same results
    /// </summary>
    public static FormatComparison Compare<T>(
        IEnumerable<T> results,
        Func<T, SearchResultData> resultMapper)
    {
        var data = results.Select(resultMapper).ToList();
        
        // TOON format
        var toon = ToonHelper.ToToon(new { results = data });
        
        // XML format
        var xml = string.Join("\n", data.Select(FormatAsXml));

        return new FormatComparison(
            ToonFormat: toon,
            XmlFormat: xml,
            ToonLength: toon.Length,
            XmlLength: xml.Length,
            SavingsPercent: xml.Length > 0 ? (1 - (double)toon.Length / xml.Length) * 100 : 0);
    }
}

/// <summary>
/// Standard search result data structure
/// </summary>
public record SearchResultData(
    string Filename,
    int PageNumber,
    string Text);

/// <summary>
/// TOON-formatted search result with stats
/// </summary>
public record ToonSearchResult<T>(
    string ToonFormatted,
    SearchResultData[] OriginalResults,
    TokenSavingsInfo TokenSavings);

/// <summary>
/// Comparison between TOON and XML formats
/// </summary>
public record FormatComparison(
    string ToonFormat,
    string XmlFormat,
    int ToonLength,
    int XmlLength,
    double SavingsPercent)
{
    public override string ToString() =>
        $"TOON: {ToonLength} chars, XML: {XmlLength} chars, Savings: {SavingsPercent:F1}%";
}
