"""Generate a neutral AR tracking candidate with a standards-encoded QR."""
from pathlib import Path
import random
import qrcode
from PIL import Image, ImageDraw, ImageFont

OUTPUT = Path(__file__).resolve().parent
PAYLOAD = 'ar-chicken://marker/001'
SIZE = 1024
image = Image.new('RGB', (SIZE, SIZE), '#f5f0e4')
draw = ImageDraw.Draw(image)

def font(size, bold=False):
    return ImageFont.truetype('C:/Windows/Fonts/' + ('arialbd.ttf' if bold else 'arial.ttf'), size)

ink = '#172c3b'
draw.rectangle((16, 16, 1007, 1007), outline=ink, width=8)
draw.text((48, 42), 'FIELD / 001', font=font(56, True), fill=ink)
draw.text((51, 111), 'AR TRACKING TEST', font=font(21), fill=ink)
# An asymmetric, angular collage gives the reference image features outside the QR.
polygons = [
    ([(48,180),(239,159),(317,269),(244,394),(61,351)], '#193e50'),
    ([(259,160),(477,181),(449,331),(331,291)], '#d47d36'),
    ([(48,381),(205,416),(174,597),(49,641)], '#d4a93c'),
    ([(223,411),(340,332),(476,367),(462,525),(348,573),(202,547)], '#507d73'),
    ([(50,672),(204,607),(303,706),(259,891),(75,941)], '#a5453a'),
    ([(327,606),(478,560),(495,756),(401,843),(301,782)], '#234d63'),
    ([(294,882),(405,862),(483,914),(463,967),(285,961)], '#b4a57a'),
    ([(531,173),(652,146),(727,231),(668,361),(543,330)], '#a5453a'),
    ([(754,165),(961,194),(930,350),(761,317),(708,263)], '#d4a93c'),
    ([(536,366),(661,397),(733,337),(962,388),(961,488),(811,462),(700,510),(555,488)], '#234d63'),
]
for points, color in polygons:
    draw.polygon(points, fill=color, outline=ink, width=5)
# Distinct internal paths and junctions; no repeating tile pattern.
paths = [
    [(80,220),(176,198),(254,271),(194,322),(111,291)],
    [(288,212),(403,235),(378,288)],
    [(84,436),(145,462),(119,550)],
    [(252,456),(320,407),(408,433),(391,493),(314,522)],
    [(96,733),(182,683),(242,736),(211,820),(132,854)],
    [(361,659),(434,615),(446,720),(375,766)],
    [(573,219),(628,193),(676,241),(631,301),(584,281)],
    [(788,221),(901,241),(873,293),(810,276)],
    [(583,422),(657,454),(725,402),(813,421),(913,436)],
]
for index, points in enumerate(paths):
    draw.line(points, fill='#fff8e7', width=9)
    x,y=points[index % len(points)]
    draw.rectangle((x-9,y-9,x+9,y+9), fill=ink)
# Small irregular landmarks add local contrast while retaining a readable design.
rng=random.Random(19)
for x,y in [(355,190),(155,359),(294,564),(72,614),(464,825),(524,102),(711,124),(939,96)]:
    w,h=rng.randint(12,26),rng.randint(15,33)
    draw.rectangle((x,y,x+w,y+h),fill=ink)
draw.text((54,955), 'N / 01', font=font(22,True), fill=ink)
draw.text((545,53), 'REFERENCE TARGET', font=font(24,True), fill=ink)
draw.text((545,88), 'KEEP THE FULL SQUARE VISIBLE', font=font(16), fill=ink)

qr=qrcode.QRCode(error_correction=qrcode.constants.ERROR_CORRECT_M, box_size=1, border=4)
qr.add_data(PAYLOAD)
qr.make(fit=True)
modules=qr.modules_count+8
box_size=444//modules
qr.box_size=box_size
qr_image=qr.make_image(fill_color='black',back_color='white').convert('RGB')
qr_x=SIZE-32-qr_image.width
qr_y=SIZE-32-qr_image.height
# The white QR image contains its own untouched four-module quiet zone.
image.paste(qr_image,(qr_x,qr_y))
image.save(OUTPUT/'marker-001.png')
(OUTPUT/'README.md').write_text(
    '# Marker 001 — tracking candidate\n\n'
    f'Payload: `{PAYLOAD}`\n\n'
    'Use the complete 1024 × 1024 PNG as the reference image, not a crop of its QR. '
    'Display the entire image on a second phone without cropping or stretching. '
    'Measure the displayed width; use that physical width in Unity. Suggested initial width: 6 cm.\n\n'
    f'QR: error correction M, {qr.modules_count} × {qr.modules_count} data-region modules, '
    f'{box_size} pixels per module, four-module white quiet zone. '
    f'QR image rectangle including quiet zone: x={qr_x}, y={qr_y}, width={qr_image.width}, height={qr_image.height} pixels. '
    'Image coordinates originate at the top left.\n\n'
    'This is an unvalidated AR tracking candidate. QR decoding is checked digitally; '
    'ARCore image-quality scoring and real-phone tracking still need validation. '
    'No Unity scene or image library has been changed.\n\n'
    'Regenerate with Python, qrcode and Pillow using generate_marker.py.\n',encoding='utf-8')
print(OUTPUT/'marker-001.png')
print(f'QR version {qr.version}; {qr_image.width}px square; payload: {PAYLOAD}')
