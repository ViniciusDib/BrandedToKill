"""Gera os sprites do jogo (Pillow).

- Formas básicas do tutorial, brancas (a cor é trocada no Unity): quadrado, círculo, triângulo.
- 3 sprites simples vistos de cima: jogador (Kage), inimigo (capanga) e moeda.

Os personagens são desenhados "olhando para cima"; no jogo eles giram para a direção do movimento.
Uso: python gen_sprites.py
"""
import os

from PIL import Image, ImageDraw

OUT = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "Assets", "Sprites")
os.makedirs(OUT, exist_ok=True)


def save(img, name):
    img.save(os.path.join(OUT, name + ".png"))


# ---------------------------------------------------------------- formas do tutorial (64x64 = 1 unidade)
square = Image.new("RGBA", (64, 64), (255, 255, 255, 255))
save(square, "Square")

circle = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
ImageDraw.Draw(circle).ellipse([0, 0, 63, 63], fill=(255, 255, 255, 255))
save(circle, "Circle")

triangle = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
ImageDraw.Draw(triangle).polygon([(32, 2), (62, 60), (2, 60)], fill=(255, 255, 255, 255))
save(triangle, "Triangle")


# ---------------------------------------------------------------- personagens vistos de cima (32x32 = 1 unidade)
def character(suit, shirt, head, hair, rim, glasses=False):
    img = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse([3, 9, 28, 24], fill=rim)                # contorno neon
    d.ellipse([4, 10, 27, 23], fill=suit)              # ombros do terno
    d.polygon([(14, 10), (18, 10), (16, 16)], fill=shirt)  # gola / camisa
    d.rectangle([22, 2, 24, 13], fill=(30, 30, 38, 255))   # braço com a arma apontando para frente
    d.rectangle([22, 1, 24, 3], fill=(90, 95, 115, 255))   # cano
    d.ellipse([10, 9, 21, 20], fill=head)              # cabeça
    if hair:
        d.chord([10, 9, 21, 20], 0, 360, fill=hair)    # cabelo (visto de cima)
        d.chord([11, 9, 20, 15], 180, 360, fill=head)  # testa aparecendo na frente
    if glasses:
        d.rectangle([12, 11, 19, 12], fill=(10, 10, 14, 255))
    return img


save(character(suit=(20, 20, 30, 255), shirt=(40, 40, 52, 255), head=(201, 148, 116, 255),
               hair=(14, 12, 16, 255), rim=(42, 246, 255, 255)), "Player")
save(character(suit=(70, 72, 88, 255), shirt=(200, 30, 46, 255), head=(214, 166, 130, 255),
               hair=None, rim=(255, 60, 120, 255), glasses=True), "Enemy")

# ---------------------------------------------------------------- moeda de ouro (16x16 = meia unidade)
coin = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
d = ImageDraw.Draw(coin)
d.ellipse([1, 1, 14, 14], fill=(212, 160, 23, 255))
d.ellipse([2, 2, 13, 13], fill=(255, 210, 63, 255))
d.rectangle([6, 6, 9, 9], fill=(0, 0, 0, 0))           # furo quadrado no meio
d.rectangle([6, 6, 9, 9], outline=(138, 90, 0, 255))
d.point((4, 4), fill=(255, 246, 200, 255))
save(coin, "Coin")

print("Sprites gerados em", OUT)
