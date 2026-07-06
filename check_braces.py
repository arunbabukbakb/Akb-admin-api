import re

file_path = r"ProductCore\Controllers\TransferController.cs"
with open(file_path, "r", encoding="utf-8") as f:
    content = f.read()

get_method_regex = re.compile(
    r"\[HttpGet\]\s*(?:\[[^\]]+\]\s*)*public\s+(?:async\s+Task<IActionResult>|IActionResult)\s+Get(?:[a-zA-Z0-9]*)\s*\([^\)]*\)\s*\{",
    re.DOTALL
)

def get_matching_brace_index(text, start_index):
    brace_count = 0
    first_brace = text.find('{', start_index)
    if first_brace == -1:
        return -1, -1
    for idx in range(first_brace, len(text)):
        char = text[idx]
        if char == '{':
            brace_count += 1
        elif char == '}':
            brace_count -= 1
            if brace_count == 0:
                return first_brace, idx
    return -1, -1

matches = list(get_method_regex.finditer(content))
for match in matches:
    print("Match signature found:", match.group(0))
    first, last = get_matching_brace_index(content, match.start())
    print("first_brace:", first, "last_brace:", last)
    if first != -1 and last != -1:
        print("Method body starts with:")
        print(content[first:first+100])
        print("Method body ends with:")
        print(content[last-100:last+1])
