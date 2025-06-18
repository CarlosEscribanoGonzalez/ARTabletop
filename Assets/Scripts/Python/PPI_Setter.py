from PIL import Image
import sys

def set_image_ppi(image_path, ppi=305):
    img = Image.open(image_path)
    img.save(image_path, dpi=(ppi, ppi))
    print(f"Imagen guardada con resolución {ppi} PPI: {image_path}")

if __name__ == "__main__":
    image_path = sys.argv[1]
    set_image_ppi(image_path)