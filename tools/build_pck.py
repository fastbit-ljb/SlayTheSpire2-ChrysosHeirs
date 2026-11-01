"""Build ChrysosHeirs.pck.

Adapted from BigHammer's build_pck.py. The game (Godot 4.5.1 runtime
template) can only load textures that went through the import pipeline, so the
mod pck must ship, per image:
  <name>.png.import      import metadata remapping the path to the ctex
  .godot/imported/<name>.png-<md5>.ctex   CompressedTexture2D, WebP-embedded

Textures are scanned from assets/images/<category>/<name>.png and packed to
res://images/<category>/<name>.png (categories: relics, powers, events, cards,
relics_small... anything you like). Localization JSON under
localization/<lang>/*.json is packed to ChrysosHeirs/localization/<lang>/...
Images are optional: the pck builds fine with localization only (art phase
can come later); missing images are skipped with a note.
"""
import hashlib
import io
import struct
import sys
from pathlib import Path

from PIL import Image

sys.path.insert(0, str(Path(__file__).parent))
from pck_tool import write_pck  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent  # repo root
ASSETS = ROOT / "assets" / "images"
PROJECT = ROOT
OUT = ROOT / "build" / "ChrysosHeirs.pck"

FORMAT_RGBA8 = 5
DATA_FORMAT_WEBP = 2

# Category -> max texture size guard (cards are tall, relics square).
SIZES = {
    "relics": (256, 256),
    "powers": (256, 256),
}


def make_ctex(webp_bytes: bytes, width: int, height: int) -> bytes:
    header = b"GST2"
    header += struct.pack("<I", 1)  # version
    header += struct.pack("<II", width, height)
    header += struct.pack("<I", 0)  # data format flags (not streamed, no detect bits)
    header += struct.pack("<I", 0)  # mipmap limit
    header += struct.pack("<III", 0, 0, 0)  # reserved
    header += struct.pack("<I", DATA_FORMAT_WEBP)
    header += struct.pack("<HH", width, height)
    header += struct.pack("<I", 0)  # mipmaps
    header += struct.pack("<I", FORMAT_RGBA8)
    header += struct.pack("<I", len(webp_bytes))
    return header + webp_bytes


def imported_name(res_path: str) -> str:
    digest = hashlib.md5(res_path.encode("utf-8")).hexdigest()
    stem = res_path.rsplit("/", 1)[-1]
    return f"{stem}-{digest}.ctex"


def texture_entries(res_path: str, image_path: Path) -> dict:
    im = Image.open(image_path)
    expected = SIZES.get(res_path.split("/")[1])
    if expected and im.size != expected:
        raise SystemExit(f"{image_path} must be {expected[0]}x{expected[1]}, got {im.size}")
    if im.mode != "RGBA":
        im = im.convert("RGBA")
    buf = io.BytesIO()
    im.save(buf, format="WEBP", lossless=True)
    webp_bytes = buf.getvalue()
    ctex_name = imported_name(res_path)
    import_text = f"""[remap]

importer="texture"
type="CompressedTexture2D"
path="res://.godot/imported/{ctex_name}"

[deps]

source_file="res://{res_path}"
dest_files=["res://.godot/imported/{ctex_name}"]

[params]
"""
    return {
        res_path + ".import": import_text.encode("utf-8"),
        ".godot/imported/" + ctex_name: make_ctex(webp_bytes, *im.size),
    }


def localization_entries() -> dict:
    entries = {}
    for lang in ("eng", "zhs"):
        loc_dir = PROJECT / "localization" / lang
        if not loc_dir.exists():
            continue
        for file in sorted(loc_dir.glob("*.json")):
            entries[f"ChrysosHeirs/localization/{lang}/{file.name}"] = file.read_bytes()
    return entries


def image_entries() -> dict:
    entries = {}
    if not ASSETS.exists():
        return entries
    for image_path in sorted(ASSETS.rglob("*.png")):
        category = image_path.parent.name
        res_path = f"images/{category}/{image_path.stem}.png"
        entries.update(texture_entries(res_path, image_path))
    return entries


def build() -> Path:
    files: dict[str, bytes] = {}
    files.update(image_entries())
    files.update(localization_entries())
    OUT.parent.mkdir(parents=True, exist_ok=True)
    write_pck(OUT, files, engine=(4, 5, 1))
    print(f"wrote {OUT} ({OUT.stat().st_size} bytes, {len(files)} files)")
    for p in sorted(files):
        print(" ", p)
    return OUT


if __name__ == "__main__":
    build()
