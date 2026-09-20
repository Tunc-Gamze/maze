"""Read-only asset linkage checks; not a replacement for Unity import/Play Mode."""
from pathlib import Path
import re

root = Path(__file__).resolve().parent.parent
assets = root / 'Assets'
guids = {}
errors = []
for meta in assets.rglob('*.meta'):
    match = re.search(r'^guid: (\w+)', meta.read_text(encoding='utf-8-sig'), re.M)
    if not match:
        continue
    guid = match[1]
    if guid in guids:
        errors.append(f'Duplicate GUID: {meta}')
    guids[guid] = Path(str(meta)[:-5])
    if not guids[guid].exists():
        errors.append(f'Orphan meta: {meta}')

checked = 0
for path in assets.rglob('*'):
    if path.suffix not in ('.unity', '.prefab', '.mat', '.asset', '.cs', '.txt'):
        continue
    if not Path(str(path) + '.meta').exists():
        errors.append(f'Missing meta: {path}')
    if path.suffix in ('.cs', '.txt'):
        continue
    raw = path.read_text(encoding='utf-8-sig')
    checked += 1
    for guid in re.findall(r'guid: ([a-f0-9]{32})', raw):
        if not guid.startswith('0000000000000000') and guid not in guids:
            errors.append(f'{path.name}: missing GUID {guid}')
    if path.suffix not in ('.unity', '.prefab'):
        continue
    ids = re.findall(r'^--- !u!\d+ &(-?\d+)', raw, re.M)
    if len(ids) != len(set(ids)):
        errors.append(f'{path.name}: duplicate fileID')
    for file_id in re.findall(r'\{fileID: (-?\d+)\}', raw):
        if file_id != '0' and file_id not in ids:
            errors.append(f'{path.name}: missing local fileID {file_id}')
    for file_id, guid in re.findall(r'\{fileID: (-?\d+), guid: (\w+), type: \d+\}', raw):
        target = guids.get(guid)
        if target and target.suffix in ('.prefab', '.asset', '.mat') and file_id != '100100000':
            target_ids = re.findall(r'^--- !u!\d+ &(-?\d+)', target.read_text(encoding='utf-8-sig'), re.M)
            if file_id not in target_ids:
                errors.append(f'{path.name}: missing external fileID {file_id} in {target.name}')

settings = (root / 'ProjectSettings/EditorBuildSettings.asset').read_text()
for scene in ('MainMenu', 'Gameplay'):
    if f'path: Assets/Scenes/{scene}.unity' not in settings:
        errors.append('Build scene missing: ' + scene)

if errors:
    raise SystemExit('\n'.join(errors))
print(f'{checked} serialized assets checked; GUID/fileID/meta checks passed.')
