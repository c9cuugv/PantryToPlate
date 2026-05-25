from ui_automator import UIAutomator
import time
import sys

def test_full_journey():
    auto = UIAutomator()
    
    try:
        # 1. Start App
        auto.start_app()
        auto.take_screenshot("app_launched")
        
        # 2. Add a Recipe
        print("--- Adding a Recipe ---")
        if not auto.wait_for("Add Recipe", timeout=60):
            print("Could not find 'Add Recipe' button on Home screen")
            sys.exit(1)
            
        auto.click(text="Add Recipe")
        auto.take_screenshot("recipe_editor_open")
        
        # In MAUI, the Entry/Editor with placeholder usually shows the placeholder text when empty
        auto.click(text="Recipe Name")
        auto.input_text("Automated Pasta")
        
        auto.click(text="Instructions")
        auto.input_text("Boil water. Add pasta. Eat.")
        auto.take_screenshot("recipe_data_filled")
        
        auto.click(text="Save Recipe")
        
        # Wait for return to home
        auto.wait_for("Available Recipes")
        auto.take_screenshot("returned_to_home")
        
        # Verify the recipe is there
        if auto.find_element(text="Automated Pasta"):
            print("SUCCESS: Automated Pasta found in list!")
        else:
            print("FAILED: Automated Pasta NOT found in list")
            sys.exit(1)
            
        # 3. Manage Pantry
        print("--- Managing Pantry ---")
        auto.click(text="Pantry") # Tab bar
        if not auto.wait_for("Add Ingredient"):
            print("Could not find 'Add Ingredient' section on Pantry screen")
            sys.exit(1)
        auto.take_screenshot("pantry_page")
        
        # Click and fill Ingredient name
        auto.click(text="Ingredient name")
        auto.input_text("Pasta")
        
        # Click and fill Quantity (which displays "0" initially due to decimal binding)
        auto.click(text="0")
        auto.input_text("2")
        
        # Click and fill Unit
        auto.click(text="Unit (e.g., cups, lbs, oz)")
        auto.input_text("lbs")
        auto.take_screenshot("pantry_data_filled")
        
        # Add to Pantry
        auto.click(text="Add to Pantry")
        time.sleep(2)
        
        # Take a screenshot to verify addition
        auto.take_screenshot("pantry_item_added")
        
        # Verify the item is now listed in the pantry
        if auto.find_element(text="Pasta"):
            print("SUCCESS: Pasta successfully added to pantry list!")
        else:
            print("FAILED: Pasta NOT found in pantry list")
            sys.exit(1)
            
        print("UI Test Completed Successfully!")
        
    except Exception as e:
        print(f"ERROR during UI test: {e}")
        auto.take_screenshot("error_state")
        sys.exit(1)

if __name__ == "__main__":
    test_full_journey()
