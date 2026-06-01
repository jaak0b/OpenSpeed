"""
Generates OpenSpeed.ico with sizes 16, 24, 32, 48, 64, 128, 256.

Design: dark rounded-square background, bold speedometer arc (blue),
        speed needle (white), small tick marks, tiny loco silhouette.
"""

import math
from PIL import Image, ImageDraw

BG       = (26,  32,  44)   # #1A202C  — app dark bg
BLUE     = (99, 179, 237)   # #63B3ED  — app accent
ORANGE   = (246, 173, 85)   # #F6AD55  — secondary accent
WHITE    = (255, 255, 255)
TRANSP   = (0,   0,   0, 0)

def draw_icon(size: int) -> Image.Image:
    S   = size
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d   = ImageDraw.Draw(img)

    # --- rounded background ---
    r = max(2, S // 6)
    d.rounded_rectangle([0, 0, S - 1, S - 1], radius=r, fill=BG)

    # Parameters scaled to icon size
    cx   = S * 0.50
    cy   = S * 0.56        # centre of arc, slightly below mid
    rad  = S * 0.36        # arc radius
    lw   = max(1, S // 14) # arc line width
    tw   = max(1, S // 22) # tick width

    # Arc: 210° to 330° (gap at bottom) — standard speedometer look
    arc_start = 210
    arc_end   = 330
    arc_box   = [cx - rad, cy - rad, cx + rad, cy + rad]

    # Shadow/glow: slightly thicker, dimmer arc underneath
    glow_color = (99, 179, 237, 60)
    glow_w = lw + max(1, S // 20)
    d.arc(arc_box, arc_start, arc_end, fill=(40, 60, 90), width=glow_w)

    # Main arc
    d.arc(arc_box, arc_start, arc_end, fill=BLUE, width=lw)

    # Tick marks (5 major ticks across the arc)
    n_ticks = 5
    total_span = (arc_end - arc_start) % 360 + 360  # 240°
    for i in range(n_ticks):
        angle_deg = arc_start + i * total_span / (n_ticks - 1)
        angle_rad = math.radians(angle_deg)
        inner = rad - lw * 1.5
        outer = rad + lw * 0.2
        x1 = cx + inner * math.cos(angle_rad)
        y1 = cy + inner * math.sin(angle_rad)
        x2 = cx + outer * math.cos(angle_rad)
        y2 = cy + outer * math.sin(angle_rad)
        d.line([x1, y1, x2, y2], fill=WHITE, width=tw)

    # Needle — pointing to ~80% of the arc (fast speed)
    needle_pct = 0.78
    needle_deg = arc_start + needle_pct * total_span
    needle_rad = math.radians(needle_deg)
    needle_len = rad * 0.82
    nx = cx + needle_len * math.cos(needle_rad)
    ny = cy + needle_len * math.sin(needle_rad)
    needle_w = max(1, S // 18)
    d.line([cx, cy, nx, ny], fill=WHITE, width=needle_w)

    # Centre dot
    dot_r = max(1, S // 20)
    d.ellipse([cx - dot_r, cy - dot_r, cx + dot_r, cy + dot_r], fill=ORANGE)

    # Tiny train silhouette at bottom (only at larger sizes)
    if S >= 32:
        train_y  = cy + rad * 0.55
        train_h  = S * 0.09
        train_w  = S * 0.38
        tx0 = cx - train_w / 2
        tx1 = cx + train_w / 2
        ty0 = train_y - train_h / 2
        ty1 = train_y + train_h / 2
        body_r = max(1, train_h // 3)
        d.rounded_rectangle([tx0, ty0, tx1, ty1], radius=body_r, fill=BLUE)
        # cab bump on right
        cab_w = train_w * 0.22
        cab_h = train_h * 0.6
        d.rounded_rectangle(
            [tx1 - cab_w, ty0 - cab_h * 0.7, tx1, ty1],
            radius=body_r, fill=BLUE
        )
        # wheels (two small dots)
        wr = max(1, int(train_h * 0.28))
        for wx in [cx - train_w * 0.22, cx + train_w * 0.18]:
            wy = ty1
            d.ellipse([wx - wr, wy - wr, wx + wr, wy + wr], fill=BG)
            d.ellipse([wx - wr + 1, wy - wr + 1, wx + wr - 1, wy + wr - 1],
                      fill=(60, 80, 110))

    return img


sizes = [16, 24, 32, 48, 64, 128, 256]
frames = [draw_icon(s) for s in sizes]

out = r"E:\Development\OpenSpeed\OpenSpeed Desktop\OpenSpeed\OpenSpeed.UI\app.ico"
frames[0].save(
    out,
    format="ICO",
    sizes=[(s, s) for s in sizes],
    append_images=frames[1:],
)
print(f"Saved {out}")
