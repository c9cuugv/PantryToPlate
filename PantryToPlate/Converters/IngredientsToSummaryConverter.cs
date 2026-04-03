using PantryToPlate.Core.Models;
using System.Globalization;

namespace PantryToPlate.Converters;

public class IngredientsToSummaryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is List<RecipeIngredient> ingredients && ingredients.Any())
        {
            var summary = string.Join(", ", ingredients.Select(i =>
                $"{i.QuantityRequired} {i.Unit} {i.Ingredient.Name}"));
            return summary;
        }
        return "No ingredients";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}