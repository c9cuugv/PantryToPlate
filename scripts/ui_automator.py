import subprocess
import time
import xml.etree.ElementTree as ET
import os
import re

if os.path.exists("/Users/deep/Library/Android/sdk"):
    ANDROID_HOME = "/Users/deep/Library/Android/sdk"
else:
    ANDROID_HOME = "/Volumes/APPLE HDD ST2000DM001 Media/android-sdk"
ADB = os.path.join(ANDROID_HOME, "platform-tools", "adb")
PACKAGE = "com.companyname.pantrytoplate"
ACTIVITY = "crc64458c71c84e3ef686.MainActivity"

class UIAutomator:
    def __init__(self, screenshot_dir="/Users/deep/.gemini/antigravity/brain/211b31ae-2c78-49b5-baf2-2db1fb5a41ba/ui_tests"):
        self.screenshot_dir = screenshot_dir
        os.makedirs(self.screenshot_dir, exist_ok=True)
        self.step_counter = 0

    def run_adb(self, cmd_str):
        adb_path = ADB
        import shlex
        args = [adb_path] + shlex.split(cmd_str)
        # print(f"DEBUG: Running {args}")
        result = subprocess.run(args, capture_output=True, text=True)
        if result.returncode != 0:
            print(f"ADB ERROR running '{cmd_str}': {result.stderr}")
        return result.stdout

    def start_app(self):
        print(f"Clearing app data for {PACKAGE}...")
        self.run_adb(f"shell pm clear {PACKAGE}")
        print(f"Launching {PACKAGE}...")
        # -S force stops, but we won't -W because it hangs on some MAUI apps
        res = self.run_adb(f"shell am start -S -n {PACKAGE}/{ACTIVITY}")
        print(f"Launch command sent.")
        time.sleep(30) # Heavy MAUI launch wait

    def get_ui_dump(self):
        dump_path = "current_ui.xml"
        for i in range(3):
            self.run_adb("shell uiautomator dump")
            self.run_adb(f"pull /sdcard/window_dump.xml {dump_path}")
            if os.path.exists(dump_path):
                return ET.parse(dump_path)
            print(f"UI dump failed, retrying ({i+1}/3)...")
            time.sleep(2)
        raise Exception("Failed to pull UI dump from device after retries")

    def find_element(self, text=None, content_desc=None):
        tree = self.get_ui_dump()
        root = tree.getroot()
        
        for node in root.iter('node'):
            match = True
            if text and node.get('text') != text:
                match = False
            if content_desc and node.get('content-desc') != content_desc:
                match = False
            
            if match:
                bounds = node.get('bounds')
                # Format: [x1,y1][x2,y2]
                m = re.match(r"\[(\d+),(\d+)\]\[(\d+),(\d+)\]", bounds)
                if m:
                    x1, y1, x2, y2 = map(int, m.groups())
                    return {"x": (x1 + x2) // 2, "y": (y1 + y2) // 2, "text": node.get('text')}
        return None

    def click(self, text=None, content_desc=None):
        el = self.find_element(text, content_desc)
        if el:
            print(f"Clicking '{text or content_desc}' at {el['x']},{el['y']}")
            self.run_adb(f"shell input tap {el['x']} {el['y']}")
            time.sleep(2)
            return True
        print(f"FAILED to find '{text or content_desc}'")
        return False

    def input_text(self, text, clear_first=True):
        print(f"Typing: {text}")
        if clear_first:
            # Move cursor to end, then delete up to 40 characters in a single fast command
            self.run_adb("shell \"input keyevent 123; for i in {1..40}; do input keyevent 67; done\"")
            time.sleep(0.5)
        # ADB input text doesn't like spaces well, replace with %s
        safe_text = text.replace(" ", "%s")
        self.run_adb(f"shell input text {safe_text}")
        time.sleep(1)
        # Dismiss Gboard software keyboard / floating stylus dock using ESC (keyevent 111)
        self.run_adb("shell input keyevent 111")
        time.sleep(1)

    def take_screenshot(self, label):
        self.step_counter += 1
        filename = f"step_{self.step_counter}_{label.lower().replace(' ', '_')}.png"
        path = os.path.join(self.screenshot_dir, filename)
        self.run_adb(f"shell screencap -p /sdcard/screen.png")
        self.run_adb(f"pull /sdcard/screen.png {path}")
        print(f"Screenshot saved: {path}")
        return path

    def wait_for(self, text, timeout=20):
        print(f"Waiting for '{text}'...")
        start = time.time()
        while time.time() - start < timeout:
            if self.find_element(text=text):
                return True
            time.sleep(2)
        return False
