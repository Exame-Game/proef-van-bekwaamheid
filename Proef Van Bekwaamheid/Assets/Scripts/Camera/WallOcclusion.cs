using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerWallOcclusion : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private float _fadeAlpha = 0.15f;
    [SerializeField] private float _fadeSpeed = 8f;

    private Camera _cam;
    private Dictionary<Renderer, bool> _rendererStates = new Dictionary<Renderer, bool>();
    private HashSet<Renderer> _currentFrameHits = new HashSet<Renderer>();

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (_cam == null)
            return;

        Vector3 direction = transform.position - _cam.transform.position;
        float distance = direction.magnitude;

        _currentFrameHits.Clear();

        RaycastHit[] hits = Physics.RaycastAll(
            _cam.transform.position,
            direction.normalized,
            distance,
            _wallLayer
        );

        foreach (RaycastHit hit in hits)
            foreach (Renderer r in hit.collider.GetComponentsInChildren<Renderer>())
            {
                if (_currentFrameHits.Contains(r))
                    continue;

                _currentFrameHits.Add(r);
                _rendererStates[r] = true;
            }

        List<Renderer> keys = new List<Renderer>(_rendererStates.Keys);
        List<Renderer> toRemove = new List<Renderer>();

        foreach (Renderer r in keys)
        {
            if (r == null)
            {
                toRemove.Add(r);
                continue;
            }

            if (!_currentFrameHits.Contains(r))
                _rendererStates[r] = false;

            if (_rendererStates[r])
                FadeOut(r);
            else
            {
                bool fullyRestored = FadeIn(r);
                if (fullyRestored)
                    toRemove.Add(r);
            }
        }

        foreach (Renderer r in toRemove)
            _rendererStates.Remove(r);
    }

    public override void OnNetworkDespawn()
    {
        foreach (KeyValuePair<Renderer, bool> kvp in _rendererStates)
        {
            if (kvp.Key == null)
                continue;

            foreach (Material mat in kvp.Key.materials)
            {
                Color c = mat.color;
                c.a = 1f;
                mat.color = c;
                SetMaterialOpaque(mat);
            }
        }

        _rendererStates.Clear();
    }

    private void FadeOut(Renderer r)
    {
        foreach (Material mat in r.materials)
        {
            SetMaterialTransparent(mat);
            Color c = mat.color;
            c.a = Mathf.MoveTowards(c.a, _fadeAlpha, _fadeSpeed * Time.deltaTime);
            mat.color = c;
        }
    }

    private bool FadeIn(Renderer r)
    {
        bool isDone = true;

        foreach (Material mat in r.materials)
        {
            Color c = mat.color;
            c.a = Mathf.MoveTowards(c.a, 1f, _fadeSpeed * Time.deltaTime);
            mat.color = c;

            if (Mathf.Approximately(c.a, 1f))
                SetMaterialOpaque(mat);
            else
                isDone = false;
        }

        return isDone;
    }

    private void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Mode", 3);
        mat.SetFloat("_Surface", 1);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.renderQueue = 3000;
    }

    private void SetMaterialOpaque(Material mat)
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