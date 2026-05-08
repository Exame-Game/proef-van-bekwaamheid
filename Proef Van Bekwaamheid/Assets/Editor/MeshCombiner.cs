using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class MeshCombiner : EditorWindow
{
    private GameObject _targetPrefab;
    private string _savePath = "Assets/CombinedMeshes";
    private string _savePathMesh = "Assets/Art/3D/Models/InGame/Combined";
    private bool _keepOriginal = true;

    [MenuItem("Tools/Mesh Combiner")]
    public static void ShowWindow()
    {
        GetWindow<MeshCombiner>("Mesh Combiner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Mesh Combiner", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        _targetPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Target Prefab", _targetPrefab, typeof(GameObject), false);

        _savePath = EditorGUILayout.TextField("Save Path", _savePath);
        _savePathMesh = EditorGUILayout.TextField("Save Path", _savePathMesh);
        _keepOriginal = EditorGUILayout.Toggle("Keep Original Prefab", _keepOriginal);

        EditorGUILayout.Space();

        GUI.enabled = _targetPrefab != null;
        if (GUILayout.Button("Combine Meshes"))
            CombineMeshes();
        GUI.enabled = true;
    }

    private void CombineMeshes()
    {
        // Instantiate so we can safely read world transforms
        GameObject instance = (GameObject)Instantiate(_targetPrefab);

        MeshFilter[] meshFilters = instance.GetComponentsInChildren<MeshFilter>(true);

        if (meshFilters.Length == 0)
        {
            Debug.LogWarning("No MeshFilters found on prefab.");
            DestroyImmediate(instance);
            return;
        }

        // Collect all CombineInstances, transforming verts into root local space
        Matrix4x4 rootInverse = instance.transform.worldToLocalMatrix;
        List<CombineInstance> combineInstances = new List<CombineInstance>();

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;

            CombineInstance ci = new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = rootInverse * mf.transform.localToWorldMatrix
            };
            combineInstances.Add(ci);
        }

        // Combine into a single mesh
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
        combinedMesh.RecalculateNormals();
        combinedMesh.RecalculateBounds();
        combinedMesh.Optimize();

        // Save the mesh asset
        if (!Directory.Exists(_savePathMesh))
            Directory.CreateDirectory(_savePathMesh);

        string assetPath = $"{_savePathMesh}/{_targetPrefab.name}_Combined.asset";
        AssetDatabase.CreateAsset(combinedMesh, assetPath);

        // Apply combined mesh to the root, preserving all existing root components
        MeshFilter rootFilter = instance.GetComponent<MeshFilter>();
        if (rootFilter == null) rootFilter = instance.AddComponent<MeshFilter>();
        rootFilter.sharedMesh = combinedMesh;

        if (instance.GetComponent<MeshRenderer>() == null)
        {
            MeshRenderer sourceRenderer = instance.GetComponentInChildren<MeshRenderer>();
            MeshRenderer rootRenderer = instance.AddComponent<MeshRenderer>();
            if (sourceRenderer != null)
                rootRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
        }

        // Remove children that only have Transform, MeshFilter, MeshRenderer — they're now empty
        foreach (Transform child in instance.GetComponentsInChildren<Transform>(true))
        {
            if (child == instance.transform) continue;

            bool hasNonMeshComponents = false;
            foreach (Component c in child.GetComponents<Component>())
            {
                if (c is Transform) continue;
                if (c is MeshFilter) continue;
                if (c is MeshRenderer) continue;
                hasNonMeshComponents = true;
                break;
            }

            if (!hasNonMeshComponents)
                DestroyImmediate(child.gameObject);
        }

        string prefabPath = $"{_savePath}/{_targetPrefab.name}_Combined.prefab";
        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);

        // Cleanup
        DestroyImmediate(instance);

        if (!_keepOriginal)
        {
            string originalPath = AssetDatabase.GetAssetPath(_targetPrefab);
            AssetDatabase.DeleteAsset(originalPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Mesh combined and saved to: {prefabPath}");
        EditorUtility.DisplayDialog("Done", $"Combined mesh saved to:\n{prefabPath}", "OK");
    }
}