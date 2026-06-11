#!/usr/bin/env python3
"""PantryToPlate Final E2E UI Test Suite
Tests ALL buttons/features with robust timing and error recovery.
Does NOT use pm clear (avoid ANR). Assumes app already running.
"""

import subprocess, time, xml.etree.ElementTree as ET, os, re, sys, shlex

ADB = ["/Volumes/APPLE HDD ST2000DM001 Media/android-sdk/platform-tools/adb", "-s", "emulator-5554"]

results = []  # (name, passed, detail)

def run(cmd):
    r = subprocess.run(ADB + shlex.split(cmd), capture_output=True, text=True, timeout=60)
    return r.stdout

def dump():
    """Get UI tree. Returns None if fails."""
    r = run("shell uiautomator dump")
    time.sleep(1)
    r = run("pull /sdcard/window_dump.xml /tmp/e2e_dump.xml")
    try:
        return ET.parse("/tmp/e2e_dump.xml")
    except:
        return None

def find(text):
    """Find exact match. Returns {'x','y'} or None."""
    tree = dump()
    if tree is None: return None
    for n in tree.iter('node'):
        if n.get('text','') == text:
            b = n.get('bounds','')
            m = re.match(r"\[(\d+),(\d+)\]\[(\d+),(\d+)\]", b)
            if m:
                x1,y1,x2,y2 = map(int, m.groups())
                return {"x":(x1+x2)//2, "y":(y1+y2)//2}
    return None

def wait(text, timeout=30):
    for _ in range(timeout):
        if find(text): return True
        time.sleep(1)
    return False

def ensure_home():
    """Force-navigate to Home page."""
    # Try clicking Home tab
    run("shell input tap 120 1200")
    time.sleep(2)

def click(txt):
    el = find(txt)
    if el:
        run(f"shell input tap {el['x']} {el['y']}")
        time.sleep(2)
        return True
    return False

def type_text(txt):
    safe = txt.replace(" ", "%s")
    run('shell "input keyevent 123; for i in {1..30}; do input keyevent 67; done"')
    time.sleep(0.3)
    run(f"shell input text {safe}")
    time.sleep(0.5)

def scroll_down(times=5):
    for _ in range(times):
        run("shell input swipe 360 1000 360 200 100")
        time.sleep(0.3)

def tap(x, y):
    run(f"shell input tap {x} {y}")
    time.sleep(1.5)

def ss(label):
    run("shell screencap -p /sdcard/screen.png")
    dest = f"/Volumes/APPLE HDD ST2000DM001 Media/Projects/personal_project/new_learning/PantryToPlate/tests/journeys/screenshots/e2e_{label}.png"
    run(f"pull /sdcard/screen.png \"{dest}\"")
    print(f"  📸 {dest}")

def get_texts():
    tree = dump()
    if tree is None: return []
    return [n.get('text','').strip() for n in tree.iter('node') if n.get('text','').strip()]

def check(name, cond, detail=""):
    status = "✅" if cond else "❌"
    print(f"  {status} {name}")
    results.append((name, cond, detail))

# ================================
print("=" * 60)
print("PANTRYTOPLATE — FINAL E2E TEST SUITE")
print("=" * 60)

# --- Phase 1: Home page ---
print("\n--- PHASE 1: HOME PAGE ---")

# Ensure app is running
run("shell am start -n com.companyname.pantrytoplate/crc64458c71c84e3ef686.MainActivity")
time.sleep(10)

t = get_texts()
check("Home screen shown", "Available Recipes" in t, str(t))

# Load Recipes
check("Load Recipes clickable", click("Load Recipes"))
time.sleep(5)
ss("01_home_after_load")
t = get_texts()
items = [x for x in t if x not in ("Home","Available Recipes","Load Recipes","Add Recipe","Pantry","Shopping List",
                                    "No recipes available. Add ingredients to your pantry or add a new recipe!",
                                    "No recipes available.")]
check("Seed recipes loaded", len(items) > 0, f"Items: {items[:5]}")

# --- Phase 2: Recipe Detail ---
print("\n--- PHASE 2: RECIPE DETAIL NAVIGATION ---")

if items:
    recipe = items[0]
    check(f"Tapping recipe '{recipe}'", click(recipe))
    time.sleep(3)
    ss("02_recipe_detail")
    t = get_texts()
    check("Recipe name visible", recipe[:10] in str(t))
    check("Cook button present", "cooking" in str(t).lower() or "Got it" in str(t))
    check("Delete button present", "Delete" in str(t))
    check("Ingredients section visible", "Ingredients" in str(t))
    # Navigate back
    run("shell input keyevent 4")
    time.sleep(2)
else:
    check("Recipes exist", False, "No recipes in list")

# --- Phase 3: Pantry ---
print("\n--- PHASE 3: PANTRY INGREDIENT MANAGEMENT ---")

check("Pantry tab clickable", click("Pantry"))
time.sleep(3)
ss("03_pantry")

t = get_texts()
check("My Pantry title", "My Pantry" in t)
check("Add to Pantry button", "Add to Pantry" in t)

# Add Pasta
click("Ingredient name")
type_text("Pasta")
time.sleep(1)

click("0")
type_text("2")
time.sleep(1)

# Unit field might not be visible on first load - try harder
if not click("Unit (e.g., cups, lbs, oz)"):
    # Try partial match: find any field with "cups" or "Unit"
    tree = dump()
    if tree:
        for n in tree.iter('node'):
            txt = n.get('text','')
            if "cups" in txt or "Unit" in txt or "oz" in txt:
                b = n.get('bounds','')
                m = re.match(r"\[(\d+),(\d+)\]\[(\d+),(\d+)\]", b)
                if m:
                    x1,y1,x2,y2 = map(int, m.groups())
                    cx, cy = (x1+x2)//2, (y1+y2)//2
                    tap(cx, cy)
                    print(f"  Found unit field via partial match: '{txt}'")
                    break
    if not find("Unit"):
        print("  Unit field not found - typing in current field")
type_text("lbs")
time.sleep(1)

ss("04_pantry_filled")
check("Add to Pantry clickable", click("Add to Pantry"))
time.sleep(3)
ss("05_pantry_added")
t = get_texts()
check("Pasta in pantry list", find("Pasta") is not None, str(t))

# Add Tomatoes
click("Ingredient name")
type_text("Tomatoes")
time.sleep(1)

# Quantity - find current value
qty = find("0")
if qty:
    tap(qty['x'], qty['y'])
    type_text("3")
    time.sleep(1)

# Unit - try click
if not click("Unit (e.g., cups, lbs, oz)"):
    tree = dump()
    if tree:
        for n in tree.iter('node'):
            txt = n.get('text','')
            if "cups" in txt or "Unit" in txt:
                b = n.get('bounds','')
                m = re.match(r"\[(\d+),(\d+)\]\[(\d+),(\d+)\]", b)
                if m:
                    x1,y1,x2,y2 = map(int, m.groups())
                    tap((x1+x2)//2, (y1+y2)//2)
                    break
type_text("unit")
time.sleep(1)

click("Add to Pantry")
time.sleep(3)
check("Tomatoes in pantry", find("Tomatoes") is not None)

# --- Phase 4: Shopping List ---
print("\n--- PHASE 4: SHOPPING LIST ---")

check("Shopping List tab", click("Shopping List"))
time.sleep(3)
ss("06_shopping")
t = get_texts()
check("Shopping List title", "Shopping List" in t)
check("Load Shopping List present", "Load Shopping List" in t)
check("Clear Purchased present", "Clear Purchased" in t)
click("Load Shopping List")
time.sleep(3)
ss("07_shopping_loaded")
t = get_texts()
print(f"  Shopping list: {t}")

# --- Phase 5: Recipe Editor ---
print("\n--- PHASE 5: RECIPE EDITOR ---")

click("Home")
time.sleep(2)
check("Add Recipe clickable", click("Add Recipe"))
time.sleep(3)
ss("08_editor")
t = get_texts()
print(f"  Editor: {t}")
check("Recipe editor opened", "Recipe Name" in t or "Add Recipe" in t or "Import" in t)

# Fill recipe name
click("Recipe Name")
type_text("E2E Final Pasta")
time.sleep(1)

# Fill instructions
click("Instructions")
type_text("Boil water. Add pasta. Drain. Enjoy!")
time.sleep(1)

# Add ingredient row
check("+ Add Row clickable", click("+ Add Row"))
time.sleep(2)

# Fill ingredient in new row
row_name = find("Name")
if row_name:
    tap(row_name['x'], row_name['y'])
    type_text("Pasta")
    time.sleep(1)

row_qty = find("Qty")
if row_qty:
    tap(row_qty['x'], row_qty['y'])
    type_text("200")
    time.sleep(1)

row_unit = find("Unit")
if row_unit:
    # There are 2 "Unit" nodes - use the second one
    tree = dump()
    unit_positions = []
    if tree:
        for n in tree.iter('node'):
            if n.get('text','') == "Unit":
                b = n.get('bounds','')
                m = re.match(r"\[(\d+),(\d+)\]\[(\d+),(\d+)\]", b)
                if m:
                    x1,y1,x2,y2 = map(int, m.groups())
                    unit_positions.append(((x1+x2)//2,(y1+y2)//2))
    if len(unit_positions) >= 2:
        tap(unit_positions[1][0], unit_positions[1][1])  # Use 2nd Unit field (ingredient row, not pantry)
    else:
        tap(row_unit['x'], row_unit['y'])
    type_text("grams")
    time.sleep(1)

ss("09_recipe_filled")

# Scroll to Save button
scroll_down(8)
time.sleep(2)

check("Save Recipe clickable", click("Save Recipe"))
time.sleep(5)
ss("10_after_save")
t = get_texts()
check("Saved recipe - back to home", "Available Recipes" in t, str(t))

click("Load Recipes")
time.sleep(3)
t = get_texts()
check("E2E Final Pasta found", find("E2E Final Pasta") is not None, str(t))

# --- Phase 6: Delete Recipe ---
print("\n--- PHASE 6: DELETE RECIPE ---")

# Find E2E recipe
click("E2E Final Pasta")
time.sleep(3)
ss("11_before_delete")
t = get_texts()
check("Delete button visible", "Delete" in t)
click("Delete")
time.sleep(4)
ss("12_after_delete")
t = get_texts()
check("Back to home after delete", "Available Recipes" in t, str(t))

# --- Phase 7: Pantry swipe delete ---
print("\n--- PHASE 7: PANTRY SWIPE DELETE ---")

click("Pantry")
time.sleep(3)

pasta = find("Pasta")
if pasta:
    # Get full bounds
    tree = dump()
    if tree:
        for n in tree.iter('node'):
            if n.get('text','') == "Pasta":
                b = n.get('bounds','')
                m = re.match(r"\[(\d+),(\d+)\]\[(\d+),(\d+)\]", b)
                if m:
                    x1,y1,x2,y2 = map(int, m.groups())
                    # Swipe left from right edge of item
                    run(f"shell input swipe {x2-10} {(y1+y2)//2} 50 {(y1+y2)//2} 300")
                    time.sleep(3)
                    break
    still_there = find("Pasta") is not None
    check("Pasta swipe-deleted", not still_there)
else:
    check("Pasta found for deletion", False)

ss("13_after_pantry_delete")

# --- Phase 8: Tab Navigation ---
print("\n--- PHASE 8: TAB NAVIGATION ---")

for tab_name, tab_x in [("Home", 120), ("Pantry", 360), ("Shopping List", 600)]:
    tap(tab_x, 1200)
    time.sleep(2)
    t = get_texts()
    check(f"Tab '{tab_name}' navigable",
          any(tab_name[:5] in x for x in t), str(t[:5]))

ss("14_final")

# --- RESULTS ---
print("\n" + "=" * 60)
print("RESULTS")
print("=" * 60)
passed = sum(1 for _, p, _ in results if p)
failed = sum(1 for _, p, _ in results if not p)
total = len(results)
print(f"  Total: {total}  ✅ {passed}  ❌ {failed}  ({passed/total*100:.0f}%)")
for name, ok, detail in results:
    print(f"  {'✅' if ok else '❌'} {name}" + (f"  | {detail}" if detail else ""))

print(f"\n  Screenshots: /tmp/e2e_*.png")
sys.exit(0 if failed == 0 else 1)
