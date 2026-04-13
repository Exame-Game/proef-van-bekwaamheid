using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerWallOcclusion : NetworkBehaviour
{
    [Header("Settings")]
    public LayerMask wallLayer;
    public float fadeAlpha = 0.15f;
    public float fadeSpeed = 8f;

    private Dictionary<Renderer, bool> _rendererStates = new Dictionary<Renderer, bool>();
    private HashSet<Renderer> _currentFrameHits = new HashSet<Renderer>();
    private Camera _cam;

    public override void OnNetworkSpawn()
    {
        _cam = Camera.main;
    }

    void LateUpdate()
    {
        if (_cam == null) return;

        Vector3 direction = transform.position - _cam.transform.position;
        float distance = direction.magnitude;

        _currentFrameHits.Clear();

        RaycastHit[] hits = Physics.RaycastAll(
            _cam.transform.position,
            direction.normalized,
            distance,
            wallLayer
        );

        foreach (RaycastHit hit in hits)
        {
            foreach (Renderer r in hit.collider.GetComponentsInChildren<Renderer>())
            {
                _currentFrameHits.Add(r);
                _rendererStates[r] = true;
            }
        }

        // Snapshot the keys so we can safely modify the dict during iteration
        List<Renderer> keys = new List<Renderer>(_rendererStates.Keys);

        List<Renderer> toRemove = new List<Renderer>();
        foreach (Renderer r in keys)
        {
            if (r == null) { toRemove.Add(r); continue; }

            if (!_currentFrameHits.Contains(r))
                _rendererStates[r] = false;

            if (_rendererStates[r])
            {
                FadeOut(r);
            }
            else
            {
                bool fullyRestored = FadeIn(r);
                if (fullyRestored) toRemove.Add(r);
            }
        }

        foreach (Renderer r in toRemove)
            _rendererStates.Remove(r);
    }

    void FadeOut(Renderer r)
    {
        foreach (Material mat in r.materials)
        {
            SetMaterialTransparent(mat);
            Color c = mat.color;
            c.a = Mathf.MoveTowards(c.a, fadeAlpha, fadeSpeed * Time.deltaTime);
            mat.color = c;
        }
    }

    // Returns true when fully restored to opaque
    bool FadeIn(Renderer r)
    {
        bool done = true;
        foreach (Material mat in r.materials)
        {
            Color c = mat.color;
            c.a = Mathf.MoveTowards(c.a, 1f, fadeSpeed * Time.deltaTime);
            mat.color = c;

            if (Mathf.Approximately(c.a, 1f))
            {
                SetMaterialOpaque(mat);
            }
            else
            {
                done = false;
            }
        }
        return done;
    }

    public override void OnNetworkDespawn()
    {
        foreach (var kvp in _rendererStates)
        {
            Renderer r = kvp.Key;
            if (r == null) continue;
            foreach (Material mat in r.materials)
            {
                Color c = mat.color;
                c.a = 1f;
                mat.color = c;
                SetMaterialOpaque(mat);
            }
        }
        _rendererStates.Clear();
    }

    void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Mode", 3);
        mat.SetFloat("_Surface", 1);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
    }

    void SetMaterialOpaque(Material mat)
    {
        mat.SetFloat("_Mode", 0);
        mat.SetFloat("_Surface", 0);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = -1;
    }
}