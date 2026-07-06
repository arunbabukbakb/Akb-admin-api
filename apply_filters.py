import os
import re

directories = [
    r"ProductCore\Controllers",
    r"CompanyCore\Controllers",
    r"UserCore\Controllers"
]

def add_usings_if_needed(content):
    usings_needed = []
    if "using System.Text.Json;" not in content:
        usings_needed.append("using System.Text.Json;")
    if "using Models.DtoModels;" not in content:
        usings_needed.append("using Models.DtoModels;")
    
    if usings_needed:
        lines = content.splitlines()
        inserted = False
        for idx, line in enumerate(lines):
            if line.strip().startswith("using "):
                for u in usings_needed:
                    lines.insert(idx + 1, u)
                inserted = True
                break
        if not inserted:
            lines.insert(0, "\n".join(usings_needed))
        return "\n".join(lines)
    return content

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

def process_file(file_path):
    print(f"Processing: {file_path}")
    with open(file_path, "r", encoding="utf-8-sig") as f:
        content = f.read()

    get_method_regex = re.compile(
        r"\[HttpGet\]\s*(?:\[[^\]]+\]\s*)*public\s+(?:async\s+Task<IActionResult>|IActionResult)\s+Get(?:[a-zA-Z0-9]*)\s*\([^\)]*\)\s*\{",
        re.DOTALL
    )

    matches = list(get_method_regex.finditer(content))
    if not matches:
        print(f"No match for Get method in {os.path.basename(file_path)}")
        return

    content = add_usings_if_needed(content)

    matches = list(get_method_regex.finditer(content))
    offset = 0

    for match in matches:
        start = match.start() + offset
        end = match.end() + offset
        
        first_brace, last_brace = get_matching_brace_index(content, start)
        if first_brace == -1 or last_brace == -1:
            continue

        method_sig = content[start:first_brace].strip()
        method_body = content[first_brace:last_brace + 1]

        # 1. Update signature to include filterJson safely using index slicing
        sig_match = re.search(r"(Get[a-zA-Z0-9]*\s*\()([^\)]*)(\))", method_sig)
        if not sig_match:
            continue
        
        prefix_sig = sig_match.group(1)
        params = sig_match.group(2).strip()
        suffix_sig = sig_match.group(3)
        
        if "filterJson" in params:
            continue # already processed
            
        if params == "":
            new_params = "string? filterJson"
        else:
            new_params = "string? filterJson, " + params
            
        new_method_sig = method_sig[:sig_match.start()] + prefix_sig + new_params + suffix_sig + method_sig[sig_match.end():]

        # 2. Modify method_body content only
        try_match = re.search(r"try\s*\{", method_body)
        if try_match:
            try_idx = try_match.end()
            deserialization_code = """
                Dictionary<string, FilterCondition>? filters = null;
                if (!string.IsNullOrWhiteSpace(filterJson))
                {
                    filters = JsonSerializer.Deserialize<Dictionary<string, FilterCondition>>(filterJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
"""
            method_body = method_body[:try_idx] + deserialization_code + method_body[try_idx:]

        # GetAll().AsQueryable() -> GetAllQueriable(dynamicFilters: filters)
        method_body = re.sub(
            r"_db\.([a-zA-Z0-9]+)\.GetAll\(\)\.AsQueryable\(\)",
            r"_db.\1.GetAllQueriable(dynamicFilters: filters)",
            method_body
        )

        # GetAll(cond).AsQueryable() -> GetAllQueriable(cond, dynamicFilters: filters)
        method_body = re.sub(
            r"_db\.([a-zA-Z0-9]+)\.GetAll\(([^)]+)\)\.AsQueryable\(\)",
            r"_db.\1.GetAllQueriable(\2, dynamicFilters: filters)",
            method_body
        )

        def replace_db_calls(m):
            entity = m.group(1)
            method = m.group(2)
            args = m.group(3).strip()
            
            if "dynamicFilters" in args or "filters" in args:
                return m.group(0)
                
            if method == "GetAllQueriable":
                if args == "":
                    return f"_db.{entity}.GetAllQueriable(dynamicFilters: filters)"
                else:
                    return f"_db.{entity}.GetAllQueriable({args}, dynamicFilters: filters)"
            elif method == "GetAll":
                if args == "":
                    return f"_db.{entity}.GetAll(dynamicFilters: filters)"
                else:
                    return f"_db.{entity}.GetAll({args}, dynamicFilters: filters)"
            elif method == "GetAllPaginated":
                return f"_db.{entity}.GetAllPaginated({args}, dynamicFilters: filters)"
            return m.group(0)

        method_body = re.sub(
            r"_db\.([a-zA-Z0-9]+)\.(GetAllQueriable|GetAll|GetAllPaginated)\(([^)]*)\)",
            replace_db_calls,
            method_body
        )

        new_method_text = new_method_sig + "\n        " + method_body
        old_method_text = content[start:last_brace + 1]

        content = content[:start] + new_method_text + content[last_brace + 1:]
        offset += len(new_method_text) - len(old_method_text)

    with open(file_path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"Updated successfully: {file_path}")

for directory in directories:
    full_dir_path = os.path.join(r"e:\Projects\InifinityLab\Library Management\lms-api", directory)
    if os.path.exists(full_dir_path):
        for root, dirs, files in os.walk(full_dir_path):
            for file in files:
                if file.endswith("Controller.cs") and file != "UploadController.cs":
                    process_file(os.path.join(root, file))
