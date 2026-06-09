using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Makes world-space TextMesh elements readable regardless of scene lighting.
/// </summary>
public static class WorldTextMeshUtility
{
    private static readonly Dictionary<string, Material> MaterialCache = new Dictionary<string, Material>();

    public static void ApplyReadableStyle(TextMesh textMesh, Color color)
    {
        if (textMesh == null || textMesh.font == null)
        {
            return;
        }

        textMesh.color = color;

        MeshRenderer renderer = textMesh.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = GetOrCreateMaterial(textMesh.font, color);
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
    }

    private static Material GetOrCreateMaterial(Font font, Color color)
    {
        string key = font.GetInstanceID() + "_" + ColorUtility.ToHtmlStringRGBA(color);
        if (MaterialCache.TryGetValue(key, out Material cachedMaterial) && cachedMaterial != null)
        {
            return cachedMaterial;
        }

        Shader shader = Shader.Find("GUI/Text Shader");
        if (shader == null)
        {
            shader = font.material != null ? font.material.shader : null;
        }

        Material material = shader != null
            ? new Material(shader)
            : new Material(font.material);

        material.name = $"ReadableWorldText_{font.name}_{ColorUtility.ToHtmlStringRGBA(color)}";

        if (font.material != null)
        {
            Texture mainTexture = font.material.mainTexture;
            material.mainTexture = mainTexture;

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", mainTexture);
            }
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        MaterialCache[key] = material;
        return material;
    }
}
