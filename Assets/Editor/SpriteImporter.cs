using UnityEditor;
using UnityEngine;

// Configura a importação dos sprites e sons automaticamente.
public class SpriteImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Sprites/")) return;
        var importer = (TextureImporter)assetImporter;
        bool shape = assetPath.Contains("Square") || assetPath.Contains("Circle") || assetPath.Contains("Triangle");
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = shape ? 64 : 32;   // formas: 64 px = 1 unidade; personagens: 32 px = 1 unidade
        importer.filterMode = shape ? FilterMode.Bilinear : FilterMode.Point; // pixel art sem borrar
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
    }

    void OnPreprocessAudio()
    {
        var importer = (AudioImporter)assetImporter;
        var settings = importer.defaultSampleSettings;
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.5f;
        importer.defaultSampleSettings = settings;
        importer.forceToMono = true;
    }
}
