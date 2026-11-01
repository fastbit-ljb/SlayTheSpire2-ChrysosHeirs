"""Generate placeholder art so the mod is testable before the client's real
art arrives. Solid color + Chinese/English label, saved into
assets/images/<category>/<name>.png. Run build_pck.py afterwards.

Usage: python tools/make_placeholders.py
"""
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "assets" / "images"

FONT_PATH = r"C:\Windows\Fonts\msyh.ttc"

# (category, name, rgb, label)
ITEMS = [
    ("relics", "trial_record", (90, 90, 110), "试炼\n记录"),
    ("relics", "romance_resonance", (212, 175, 55), "浪漫\n共鸣"),
    ("relics", "golden_web", (184, 134, 11), "金丝\n织网"),
    ("relics", "golden_cocoon", (218, 165, 32), "黄金\n之茧"),
    ("relics", "weaver_gift", (240, 200, 100), "织者\n恩赐"),
    ("relics", "fate_spindle", (160, 120, 60), "命运\n纺锤"),
    ("relics", "golden_rose_finger", (205, 92, 92), "金玫\n之指"),
    ("powers", "gold_thread_power", (255, 215, 0), "金丝"),
    ("events", "trial_offer", (60, 40, 80), "先古试炼"),
    ("cards", "golden_thread_cut", (120, 90, 30), "金丝裁衣"),
]

SIZES = {
    "relics": (256, 256),
    "powers": (256, 256),
    "events": (640, 400),
    "cards": (420, 560),
}


def make(path: Path, size: tuple[int, int], rgb: tuple, label: str) -> None:
    im = Image.new("RGBA", size, (*rgb, 255))
    draw = ImageDraw.Draw(im)
    # darker border
    draw.rectangle([0, 0, size[0] - 1, size[1] - 1], outline=(20, 20, 20, 255), width=6)
    font_size = min(size) // (3 + label.count("\n"))
    font = ImageFont.truetype(FONT_PATH, font_size)
    bbox = draw.multiline_textbbox((0, 0), label, font=font, align="center")
    w, h = bbox[2] - bbox[0], bbox[3] - bbox[1]
    draw.multiline_text(((size[0] - w) / 2, (size[1] - h) / 2), label, font=font,
                        fill=(25, 20, 15, 255), align="center")
    path.parent.mkdir(parents=True, exist_ok=True)
    im.save(path)
    print("wrote", path.relative_to(ROOT))


def main() -> None:
    for category, name, rgb, label in ITEMS:
        make(OUT / category / f"{name}.png", SIZES[category], rgb, label)


if __name__ == "__main__":
    main()
