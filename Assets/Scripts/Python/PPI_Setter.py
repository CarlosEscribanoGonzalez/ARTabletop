from PIL import Image
import sys

def set_image_ppi(image_path, ppi):
    img = Image.open(image_path)
    img.save(image_path, dpi=(ppi, ppi))
    print(f"Imagen guardada con resolución {ppi} PPI: {image_path}")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("ERROR: NO SE HA ADJUNTADO LA IMAGEN")
        sys.exit()
    
    image_path = sys.argv[1] 
    if len(sys.argv) < 3:
        set_image_ppi(image_path, 305)
    else: 
        ppi = int(sys.argv[2])
        set_image_ppi(image_path, ppi)
