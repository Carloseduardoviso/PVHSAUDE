using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Web.ModelBinders;

public class MoedaModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext context)
    {
        var result = context.ValueProvider.GetValue(context.ModelName);
        if (result == ValueProviderResult.None) return Task.CompletedTask;
        context.ModelState.SetModelValue(context.ModelName, result);
        var text = (result.FirstValue ?? "").Replace("R$", "").Trim();
        if (Regex.IsMatch(text, @"^(?:\d+|\d{1,3}(?:\.\d{3})+),\d{2}$") &&
            decimal.TryParse(text, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out var value))
            context.Result = ModelBindingResult.Success(value);
        else
            context.ModelState.TryAddModelError(context.ModelName, "Informe um valor válido, como R$ 150,00.");
        return Task.CompletedTask;
    }
}
