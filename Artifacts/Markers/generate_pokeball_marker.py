"""Generate the Poké Ball marker with the existing payload and QR placement.
Requires Pillow, qrcode and OpenCV for the decode check.
"""
from pathlib import Path
import qrcode
from PIL import Image, ImageDraw, ImageFont
import cv2

ROOT = Path(__file__).resolve().parents[2]
OUTPUT = Path(__file__).resolve().parent / 'marker-pokeball.png'
PAYLOAD = 'ar-chicken://marker/001'
SIZE = 1024
image = Image.new('RGB', (SIZE, SIZE), '#ffffff')
draw = ImageDraw.Draw(image)
ink = '#434c55'
red = '#ed5265'
font_path = 'C:/Windows/Fonts/arialbd.ttf'
# The icon stays square and the QR keeps its original four-module quiet zone.
icon = Image.open(ROOT / 'Assets/pokeball.png').convert('RGBA')
icon.thumbnail((600, 600), Image.Resampling.LANCZOS)
# Upscale uniformly from the supplied 512px icon.
icon = icon.resize((600, 600), Image.Resampling.LANCZOS)
image.paste(icon, (32, 32), icon)
draw.text((710, 80), 'POKE', font=ImageFont.truetype(font_path, 66), fill=ink)
draw.text((710, 150), 'BALL', font=ImageFont.truetype(font_path, 66), fill=ink)
draw.line((716, 247, 960, 247), fill=red, width=10)
draw.text((716, 277), 'AR / 001', font=ImageFont.truetype(font_path, 32), fill=ink)
draw.text((48, 750), 'CHOOSE', font=ImageFont.truetype(font_path, 60), fill=ink)
draw.text((48, 822), 'YOUR PARTNER', font=ImageFont.truetype(font_path, 43), fill=ink)
draw.line((48, 920, 292, 920), fill=red, width=12)
qr = qrcode.QRCode(error_correction=qrcode.constants.ERROR_CORRECT_M, box_size=13, border=4)
qr.add_data(PAYLOAD)
qr.make(fit=True)
qr_image = qr.make_image(fill_color='black', back_color='white').convert('RGB')
assert qr_image.size == (429, 429)
image.paste(qr_image, (563, 563))
image.save(OUTPUT)
image.save(ROOT / 'Assets/Markers/marker-pokeball.png')
for size in (1024, 512, 360):
    pixels = cv2.imread(str(OUTPUT))
    pixels = cv2.resize(pixels, (size, size), interpolation=cv2.INTER_AREA)
    decoded, _, _ = cv2.QRCodeDetector().detectAndDecode(pixels)
    assert decoded == PAYLOAD, f'QR decoding failed at {size}px'
    print(f'QR decoding passed at {size}px')
print(OUTPUT)
