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

    public static bool HasClearSight(Camera camera, Transform targetRoot, Vector3 targetPosition)
    {
        if (camera == null || targetRoot == null)
        {
            return false;
        }

        Vector3 origin = camera.transform.position;
        Vector3 direction = targetPosition - origin;
        float distance = direction.magnitude;
        if (distance <= Mathf.Epsilon)
        {
            return true;
        }

        Ray ray = new Ray(origin, direction / distance);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        if (hits.Length == 0)
        {
            return true;
        }

        System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
        for (int i = 0; i < hits.Length; i++)
        {
            Transform hitTransform = hits[i].transform;
            if (hitTransform == null)
            {
                continue;
            }

            if (hitTransform == targetRoot
                || hitTransform.IsChildOf(targetRoot)
                || targetRoot.IsChildOf(hitTransform)
                || hitTransform.IsChildOf(camera.transform.root))
            {
                return true;
            }

            return false;
        }

        return true;
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
