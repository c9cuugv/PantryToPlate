#!/usr/bin/env python3
import subprocess, time, xml.etree.ElementTree as ET, os, re, sys, shlex

ADB = ["/Volumes/APPLE HDD ST2000DM001 Media/android-sdk/platform-tools/adb", "-s", "emulator-5554"]
results = []

def run(cmd):
    r = subprocess.run(ADB + shlex.split(cmd), capture_output=True, text=True, timeout=60)
    return r.stdout

def dump():
    for _ in range(5):
        r = run("shell uiautomator dump")
        if "ERROR" not in r:
            break
        time.sleep(2)
    
    run("pull /sdcard/window_dump.xml /tmp/e2e_dump.xml")
    try:
        return ET.parse("/tmp/e2e_dump.xml")
    except Exception as e:
        print(f"  [DUMP ERROR] {e}")
        return None

def find(text, contains=False):
    tree = dump()
    if tree is None: return None
    for n in tree.iter('node'):
        txt = n.get('text','')
        match = (text.lower() in txt.lower()) if contains else (txt.lower() == text.lower())
        if match:
            b = n.get('bounds','')
            m = re.match(r"\[(\d+),(\d+)\]\[(\d+),(\d+)\]", b)
            if m:
                x1,y1,x2,y2 = map(int, m.groups())
                return {"x": (x1+x2)//2, "y": (y1+y2)//2, "text": txt}
    return None

def click(text, contains=False):
    el = find(text, contains)
    if el:
        print(f"  [CLICK] Tapping '{el['text']}' at ({el['x']}, {el['y']})")
        run(f"shell input tap {el['x']} {el['y']}")
        time.sleep(2)
        return True
    return False

def tap(x, y):
    run(f"shell input tap {x} {y}")
    time.sleep(1.5)

def scroll_down(times=2):
    print(f"  [SCROLL] Swiping up {times} times to scroll down...")
    for _ in range(times):
        run("shell input swipe 360 950 360 250 150")
        time.sleep(0.5)

def get_texts():
    tree = dump()
    if tree is None: return []
    return [n.get('text','').strip() for n in tree.iter('node') if n.get('text','').strip()]

def ss(label):
    run("shell screencap -p /sdcard/screen.png")
    dest = f"/Volumes/APPLE HDD ST2000DM001 Media/Projects/personal_project/new_learning/PantryToPlate/tests/journeys/screenshots/e2e_feature_{label}.png"
    run(f"pull /sdcard/screen.png \"{dest}\"")
    print(f"  📸 Saved screenshot: {dest}")

def check(name, cond, detail=""):
    status = "✅" if cond else "❌"
    print(f"  {status} {name}")
    results.append((name, cond, detail))

# ================================
print("=" * 60)
print("E2E TESTS FOR NEW PANTRYTOPLATE FEATURES")
print("=" * 60)

# Start/Restart app clean by clearing package data first to guarantee seeded DB
print("\n--- FRESH CLEAR & LAUNCH APP ---")
run("shell pm clear com.companyname.pantrytoplate")
time.sleep(2)
run("shell am start -n com.companyname.pantrytoplate/crc64458c71c84e3ef686.MainActivity")

# Wait robustly for app to launch and draw
print("  Waiting for app to start...")
app_ready = False
for i in range(15):
    t = get_texts()
    if "Recipes" in t or "Home" in t or "Load Recipes" in t:
        app_ready = True
        break
    time.sleep(2)

t = get_texts()
check("App launched and Home screen shown", app_ready, str(t))

# If we are not on Home tab, click Home tab
if "Recipes" not in t:
    click("Home")
    time.sleep(2)
    t = get_texts()

# 1. Pantry Ready Suggestions
print("\n--- CHECKPOINT 1: PANTRY READY SUGGESTIONS ---")
ss("01_home_screen")
check("Pantry Ready badges are visible on Home", "Pantry Ready" in t, str(t))

# 2. Cook confirmation dialog
print("\n--- CHECKPOINT 2: COOK CONFIRMATION DIALOG ---")
# Tap on Cheese Omelette (which should be pantry ready)
check("Tapping Cheese Omelette recipe", click("Cheese Omelette"))
time.sleep(3)
ss("02_recipe_detail")

t_detail = get_texts()
check("Recipe detail page loaded", "Got it! I'm cooking this" in t_detail, str(t_detail))

# Tap cook button
check("Tapping Cook Button", click("Got it! I'm cooking this"))
time.sleep(3)
ss("03_cook_dialog")

t_dialog = get_texts()
check("Confirmation dialog visible", "Great choice! Are you sure you want to make this?" in t_dialog or "Great choice" in str(t_dialog), str(t_dialog))

# Test cancel (No)
check("Tapping NO to cancel cooking", click("NO") or click("No"))
time.sleep(3)
t_cancel = get_texts()
check("Cooking was cancelled, remained on detail page", "Got it! I'm cooking this" in t_cancel, str(t_cancel))

# Tap cook button again
click("Got it! I'm cooking this")
time.sleep(3)

# Test confirm (Yes)
check("Tapping YES to confirm cooking", click("YES") or click("Yes"))
time.sleep(5) # Allow time for navigation and save

t_home = get_texts()
check("Navigated back to Home after confirmation", "Recipes" in t_home or "Home" in t_home, str(t_home))
ss("04_back_to_home")

# 3. Pantry Deduction & Shopping List addition
print("\n--- CHECKPOINT 3: PANTRY DEDUCTION & SHOPPING LIST SYNC ---")

# Go to Pantry and see if ingredients for Cheese Omelette (Egg, Cheese, Milk) are deducted
# Cheese Omelette needs 3 eggs, 60g cheese, 50ml milk.
# Initial seed is Egg: 12, Cheese: 200, Milk: 1000.
# So updated should be Egg: 9, Cheese: 140, Milk: 950.
check("Tapping Pantry Tab", click("Pantry"))
time.sleep(3)

# Scroll down to bring Egg, Cheese, Milk into view
scroll_down(3)
time.sleep(2)
ss("05_pantry_deducted")

t_pantry = get_texts()
print(f"  Pantry texts visible: {t_pantry}")
check("Eggs quantity deducted in pantry (should be 9.0)", "9.0" in t_pantry, str(t_pantry))
check("Cheese quantity deducted in pantry (should be 140.0)", "140.0" in t_pantry, str(t_pantry))
check("Milk quantity deducted in pantry (should be 950.0)", "950.0" in t_pantry, str(t_pantry))

# Go to Shopping List page
check("Tapping Shopping List Tab", click("Shopping List"))
time.sleep(3)

# Load Shopping List if needed
click("Load Shopping List")
time.sleep(3)
ss("06_shopping_synced")

t_shopping = get_texts()
check("Egg added to shopping list", "Egg" in t_shopping, str(t_shopping))
check("Cheese added to shopping list", "Cheese" in t_shopping, str(t_shopping))
check("Milk added to shopping list", "Milk" in t_shopping, str(t_shopping))

# 4. Put groceries back to pantry
print("\n--- CHECKPOINT 4: PUT GROCERIES BACK TO PANTRY ---")

# Check/purchase all 3 items (Egg, Cheese, Milk) on the shopping list.
egg_el = find("Egg")
if egg_el:
    print(f"  Found Egg label at ({egg_el['x']}, {egg_el['y']}). Tapping checkbox to its left...")
    tap(egg_el['x'] - 100, egg_el['y'])
    time.sleep(1.5)

cheese_el = find("Cheese")
if cheese_el:
    print(f"  Found Cheese label at ({cheese_el['x']}, {cheese_el['y']}). Tapping checkbox to its left...")
    tap(cheese_el['x'] - 100, cheese_el['y'])
    time.sleep(1.5)

milk_el = find("Milk")
if milk_el:
    print(f"  Found Milk label at ({milk_el['x']}, {milk_el['y']}). Tapping checkbox to its left...")
    tap(milk_el['x'] - 100, milk_el['y'])
    time.sleep(1.5)

ss("07_shopping_checked")

# Click "Put Back to Pantry" (we can also match partial "Put Back to")
check("Tapping Put Back to Pantry button", click("Put Back to", contains=True))
time.sleep(4)
ss("08_shopping_restocked")

t_shopping_after = get_texts()
check("Purchased items cleared from shopping list", "Egg" not in t_shopping_after and "Cheese" not in t_shopping_after and "Milk" not in t_shopping_after, str(t_shopping_after))

# Verify quantities in Pantry have been restocked
# Default quantities: Egg (+6 whole = 9+6=15), Cheese (+500g = 140+500=640), Milk (+1000ml = 950+1000=1950)
click("Pantry")
time.sleep(3)

# Scroll down to bring restocked items into view
scroll_down(3)
time.sleep(2)
ss("09_pantry_restocked")

t_pantry_after = get_texts()
print(f"  Pantry after restock: {t_pantry_after}")
check("Eggs restocked in pantry (should be 15.0)", "15.0" in t_pantry_after, str(t_pantry_after))
check("Cheese restocked in pantry (should be 640.0)", "640.0" in t_pantry_after, str(t_pantry_after))
check("Milk restocked in pantry (should be 1950.0)", "1950.0" in t_pantry_after, str(t_pantry_after))

# Navigate back to Home
click("Home")
time.sleep(2)

# ================================
print("\n" + "=" * 60)
print("FINAL RESULTS")
print("=" * 60)
passed = sum(1 for _, p, _ in results if p)
failed = sum(1 for _, p, _ in results if not p)
total = len(results)
print(f"  Total: {total}  ✅ {passed}  ❌ {failed}  ({passed/total*100:.0f}%)")
for name, ok, detail in results:
    print(f"  {'✅' if ok else '❌'} {name}")

sys.exit(0 if failed == 0 else 1)
