using System.ComponentModel.DataAnnotations;

namespace ASTA.WorldApi;

public static class ValidationUtil
{
    public static (bool IsValid, IDictionary<string, string[]> Errors) Validate<T>(T model)
    {
        var ctx = new ValidationContext(model!);
        var results = new List<ValidationResult>();
        var ok = Validator.TryValidateObject(model!, ctx, results, validateAllProperties: true);

        var dict = results
            .SelectMany(r => r.MemberNames.DefaultIfEmpty(string.Empty)
                .Select(m => (Member: m, Error: r.ErrorMessage ?? "Invalid")))
            .GroupBy(x => x.Member)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Error).ToArray());

        return (ok, dict);
    }
}
