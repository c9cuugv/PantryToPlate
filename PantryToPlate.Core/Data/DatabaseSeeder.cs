using PantryToPlate.Core.Models;

namespace PantryToPlate.Core.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Ingredients.FindAsync(1) != null) return; // already seeded

        // --- Staples (IsStaple = true) ---
        var staples = new[]
        {
            new Ingredient { Name = "Water",        IsStaple = true },
            new Ingredient { Name = "Cooking Oil",  IsStaple = true },
            new Ingredient { Name = "Salt",         IsStaple = true },
            new Ingredient { Name = "Black Pepper", IsStaple = true },
            new Ingredient { Name = "Sugar",        IsStaple = true },
            new Ingredient { Name = "Butter",       IsStaple = true },
        };
        db.Ingredients.AddRange(staples);
        await db.SaveChangesAsync();

        // --- Non-staple ingredients ---
        var ing = new Dictionary<string, Ingredient>();
        var names = new[]
        {
            "Chicken Breast","Pasta","Tomato","Onion","Garlic",
            "Rice","Egg","Potato","Carrot","Cabbage",
            "Ground Beef","Bread","Cheese","Milk","Flour",
            "Lemon","Spinach","Mushroom","Bell Pepper","Zucchini",
            "Tuna (can)","Kidney Beans","Canned Tomatoes","Broccoli","Cucumber",
            "Salmon","Shrimp","Soy Sauce","Ginger","Tofu",
        };
        foreach (var n in names)
        {
            var i = new Ingredient { Name = n, IsStaple = false };
            db.Ingredients.Add(i);
            ing[n] = i;
        }
        await db.SaveChangesAsync();

        // Helper
        static RecipeIngredient RI(Ingredient i, decimal qty, string unit) =>
            new() { Ingredient = i, IngredientId = i.Id, QuantityRequired = qty, Unit = unit };

        // --- 25 Recipes ---
        var recipes = new List<Recipe>
        {
            new() { Name = "Spaghetti Bolognese",
                Instructions = "Brown beef. Add canned tomatoes and onion. Simmer 20 min. Serve over pasta.",
                RequiredIngredients = [
                    RI(ing["Ground Beef"], 300, "grams"),
                    RI(ing["Pasta"], 200, "grams"),
                    RI(ing["Canned Tomatoes"], 1, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                    RI(ing["Garlic"], 2, "whole"),
                ]},

            new() { Name = "Fried Rice",
                Instructions = "Fry onion and garlic. Add cooked rice and egg. Stir-fry 5 min.",
                RequiredIngredients = [
                    RI(ing["Rice"], 200, "grams"),
                    RI(ing["Egg"], 2, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                    RI(ing["Garlic"], 2, "whole"),
                ]},

            new() { Name = "Chicken Stir-Fry",
                Instructions = "Slice chicken and vegetables. Stir-fry in oil with soy sauce 10 min.",
                RequiredIngredients = [
                    RI(ing["Chicken Breast"], 300, "grams"),
                    RI(ing["Bell Pepper"], 1, "whole"),
                    RI(ing["Broccoli"], 200, "grams"),
                    RI(ing["Soy Sauce"], 30, "ml"),
                    RI(ing["Garlic"], 2, "whole"),
                ]},

            new() { Name = "Omelette",
                Instructions = "Whisk eggs. Pour into pan. Add mushroom and cheese. Fold and serve.",
                RequiredIngredients = [
                    RI(ing["Egg"], 3, "whole"),
                    RI(ing["Mushroom"], 100, "grams"),
                    RI(ing["Cheese"], 50, "grams"),
                ]},

            new() { Name = "Tomato Pasta",
                Instructions = "Cook pasta. Sauté garlic and tomatoes. Toss together. Season.",
                RequiredIngredients = [
                    RI(ing["Pasta"], 200, "grams"),
                    RI(ing["Tomato"], 3, "whole"),
                    RI(ing["Garlic"], 3, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                ]},

            new() { Name = "Potato Soup",
                Instructions = "Boil diced potato and carrot. Blend. Season.",
                RequiredIngredients = [
                    RI(ing["Potato"], 4, "whole"),
                    RI(ing["Carrot"], 2, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                    RI(ing["Milk"], 200, "ml"),
                ]},

            new() { Name = "Scrambled Eggs on Toast",
                Instructions = "Scramble eggs in butter. Toast bread. Serve on top.",
                RequiredIngredients = [
                    RI(ing["Egg"], 3, "whole"),
                    RI(ing["Bread"], 2, "whole"),
                ]},

            new() { Name = "Chicken Rice Bowl",
                Instructions = "Pan-fry chicken. Serve over steamed rice with soy sauce.",
                RequiredIngredients = [
                    RI(ing["Chicken Breast"], 250, "grams"),
                    RI(ing["Rice"], 200, "grams"),
                    RI(ing["Soy Sauce"], 20, "ml"),
                    RI(ing["Garlic"], 2, "whole"),
                ]},

            new() { Name = "Tuna Pasta",
                Instructions = "Cook pasta. Mix with drained tuna, lemon juice, and onion.",
                RequiredIngredients = [
                    RI(ing["Pasta"], 200, "grams"),
                    RI(ing["Tuna (can)"], 1, "whole"),
                    RI(ing["Lemon"], 1, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                ]},

            new() { Name = "Vegetable Soup",
                Instructions = "Simmer carrot, potato, zucchini, and onion in water 25 min. Season.",
                RequiredIngredients = [
                    RI(ing["Carrot"], 2, "whole"),
                    RI(ing["Potato"], 2, "whole"),
                    RI(ing["Zucchini"], 1, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                ]},

            new() { Name = "Beef Tacos",
                Instructions = "Cook ground beef with onion and garlic. Serve in bread with tomato.",
                RequiredIngredients = [
                    RI(ing["Ground Beef"], 250, "grams"),
                    RI(ing["Tomato"], 2, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                    RI(ing["Bread"], 4, "whole"),
                    RI(ing["Cheese"], 60, "grams"),
                ]},

            new() { Name = "Garlic Mushroom Toast",
                Instructions = "Sauté mushrooms and garlic in butter. Serve on toasted bread.",
                RequiredIngredients = [
                    RI(ing["Mushroom"], 200, "grams"),
                    RI(ing["Garlic"], 3, "whole"),
                    RI(ing["Bread"], 2, "whole"),
                ]},

            new() { Name = "Spinach and Egg Stir-Fry",
                Instructions = "Fry garlic in oil. Add spinach. Push aside, scramble eggs in pan. Mix together.",
                RequiredIngredients = [
                    RI(ing["Spinach"], 200, "grams"),
                    RI(ing["Egg"], 2, "whole"),
                    RI(ing["Garlic"], 2, "whole"),
                ]},

            new() { Name = "Pancakes",
                Instructions = "Mix flour, egg, milk. Fry in butter. Serve with sugar.",
                RequiredIngredients = [
                    RI(ing["Flour"], 150, "grams"),
                    RI(ing["Egg"], 2, "whole"),
                    RI(ing["Milk"], 200, "ml"),
                ]},

            new() { Name = "Grilled Salmon",
                Instructions = "Brush salmon with oil and lemon juice. Grill 12 min. Season.",
                RequiredIngredients = [
                    RI(ing["Salmon"], 300, "grams"),
                    RI(ing["Lemon"], 1, "whole"),
                ]},

            new() { Name = "Shrimp Fried Rice",
                Instructions = "Fry garlic, add shrimp and rice. Season with soy sauce.",
                RequiredIngredients = [
                    RI(ing["Shrimp"], 200, "grams"),
                    RI(ing["Rice"], 200, "grams"),
                    RI(ing["Garlic"], 2, "whole"),
                    RI(ing["Soy Sauce"], 30, "ml"),
                    RI(ing["Egg"], 1, "whole"),
                ]},

            new() { Name = "Tofu Stir-Fry",
                Instructions = "Cube tofu. Fry with bell pepper, broccoli and soy sauce.",
                RequiredIngredients = [
                    RI(ing["Tofu"], 300, "grams"),
                    RI(ing["Bell Pepper"], 1, "whole"),
                    RI(ing["Broccoli"], 150, "grams"),
                    RI(ing["Soy Sauce"], 30, "ml"),
                    RI(ing["Ginger"], 10, "grams"),
                ]},

            new() { Name = "Cabbage Stir-Fry",
                Instructions = "Shred cabbage. Fry with garlic, soy sauce. Serve over rice.",
                RequiredIngredients = [
                    RI(ing["Cabbage"], 1, "whole"),
                    RI(ing["Garlic"], 3, "whole"),
                    RI(ing["Soy Sauce"], 25, "ml"),
                    RI(ing["Rice"], 150, "grams"),
                ]},

            new() { Name = "Cheese Omelette",
                Instructions = "Whisk eggs with milk. Cook in butter. Fill with cheese. Fold.",
                RequiredIngredients = [
                    RI(ing["Egg"], 3, "whole"),
                    RI(ing["Cheese"], 60, "grams"),
                    RI(ing["Milk"], 50, "ml"),
                ]},

            new() { Name = "Bean Rice Bowl",
                Instructions = "Cook kidney beans with onion and tomato. Serve on rice.",
                RequiredIngredients = [
                    RI(ing["Kidney Beans"], 1, "whole"),
                    RI(ing["Rice"], 200, "grams"),
                    RI(ing["Onion"], 1, "whole"),
                    RI(ing["Canned Tomatoes"], 1, "whole"),
                    RI(ing["Garlic"], 2, "whole"),
                ]},

            new() { Name = "Zucchini Pasta",
                Instructions = "Sauté zucchini and garlic. Toss with pasta.",
                RequiredIngredients = [
                    RI(ing["Zucchini"], 2, "whole"),
                    RI(ing["Pasta"], 200, "grams"),
                    RI(ing["Garlic"], 2, "whole"),
                    RI(ing["Lemon"], 1, "whole"),
                ]},

            new() { Name = "Carrot Ginger Soup",
                Instructions = "Simmer carrot and ginger in water. Blend smooth. Season.",
                RequiredIngredients = [
                    RI(ing["Carrot"], 4, "whole"),
                    RI(ing["Ginger"], 20, "grams"),
                    RI(ing["Onion"], 1, "whole"),
                ]},

            new() { Name = "Greek Salad",
                Instructions = "Chop tomato, cucumber, bell pepper. Toss with lemon juice.",
                RequiredIngredients = [
                    RI(ing["Tomato"], 3, "whole"),
                    RI(ing["Cucumber"], 1, "whole"),
                    RI(ing["Bell Pepper"], 1, "whole"),
                    RI(ing["Cheese"], 80, "grams"),
                    RI(ing["Lemon"], 1, "whole"),
                ]},

            new() { Name = "Potato and Egg Hash",
                Instructions = "Dice potatoes. Fry with onion until golden. Add eggs and scramble.",
                RequiredIngredients = [
                    RI(ing["Potato"], 3, "whole"),
                    RI(ing["Egg"], 2, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                ]},

            new() { Name = "Chicken Soup",
                Instructions = "Simmer chicken with carrot, potato and onion 30 min. Season.",
                RequiredIngredients = [
                    RI(ing["Chicken Breast"], 250, "grams"),
                    RI(ing["Carrot"], 2, "whole"),
                    RI(ing["Potato"], 2, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                ]},
        };

        db.Recipes.AddRange(recipes);
        await db.SaveChangesAsync();
    }
}
