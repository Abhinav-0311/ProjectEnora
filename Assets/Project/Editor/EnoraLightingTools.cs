#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Dungeon kits often ship hundreds of Point lights with shadows; each point light uses multiple atlas
/// slots. The project URP asset (PC_RPAsset) has Additional Light Shadows disabled so only the
/// main Directional light casts real-time shadows — use this menu if you re-enable additional shadows
/// later and need to strip Point light shadows from props.
/// </summary>
public static class EnoraLightingTools
{
    [MenuItem("Enora/Lighting/Disable shadows on all Point lights in loaded scenes")]
    public static void DisablePointLightShadowsInLoadedScenes()
    {
        int count = 0;
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var light in root.GetComponentsInChildren<Light>(true))
                {
                    if (light.type != LightType.Point) continue;
                    if (light.shadows == LightShadows.None) continue;

                    Undo.RecordObject(light, "Disable point light shadows");
                    light.shadows = LightShadows.None;
                    EditorUtility.SetDirty(light);
                    count++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
        }

        Debug.Log($"Enora: disabled real-time shadows on {count} Point light(s) in loaded scenes. Save scenes (Ctrl+S) to persist.");
    }
}
#endif
