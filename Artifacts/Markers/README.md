# Marker 001 — tracking candidate

Payload: `ar-chicken://marker/001`

Use the complete 1024 × 1024 PNG as the reference image, not a crop of its QR. Display the entire image on a second phone without cropping or stretching. Measure the displayed width; use that physical width in Unity. Suggested initial width: 6 cm.

QR: error correction M, 25 × 25 data-region modules, 13 pixels per module, four-module white quiet zone. QR image rectangle including quiet zone: x=563, y=563, width=429, height=429 pixels. Image coordinates originate at the top left.

This is an unvalidated AR tracking candidate. QR decoding is checked digitally; ARCore image-quality scoring and real-phone tracking still need validation. No Unity scene or image library has been changed.

Regenerate with Python, qrcode and Pillow using generate_marker.py.
