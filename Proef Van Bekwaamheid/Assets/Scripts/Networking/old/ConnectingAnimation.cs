using DG.Tweening;
using TMPro;
using UnityEngine;
/// <summary>
/// Animates a TMP text label in a ping-pong dot pattern:
///   Connecting ? Connecting. ? Connecting.. ? Connecting... ? Connecting.. ? Connecting. ? (repeat)
/// using DOTween Pro for smooth, looping playback.
/// 
/// Setup:
///   1. Attach this script to any GameObject.
///   2. Assign a TextMeshProUGUI (or TextMeshPro) reference in the Inspector.
///   3. (Optional) Tweak baseText, stepDuration, and fadeDuration to taste.
/// </summary>
public class ConnectingAnimation : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The TMP label that will display the animation.")]
    public TextMeshProUGUI label;

    [Header("Content")]
    [Tooltip("The root word shown before the dots.")]
    public string baseText = "Connecting";

    [Header("Timing")]
    [Tooltip("How long each dot-step is held before advancing (seconds).")]
    [Range(0.1f, 2f)]
    public float stepDuration = 0.4f;

    [Tooltip("Fade-in duration for each dot character (seconds).")]
    [Range(0f, 1f)]
    public float fadeDuration = 0.15f;

    // ?? internals ??????????????????????????????????????????????????????????????
    private Sequence _sequence;

    private static readonly string[] Steps =
    {
        ".",   // "Connecting."
        "..",  // "Connecting.."
        "...", // "Connecting..."
    };

    // ?? lifecycle ??????????????????????????????????????????????????????????????
    private void OnEnable()
    {
        if (label == null)
        {
            Debug.LogError($"[ConnectingAnimation] No TMP label assigned on {name}.", this);
            return;
        }

        Play();
    }

    private void OnDisable() => Stop();

    // ?? public API ?????????????????????????????????????????????????????????????
    public void Play()
    {
        Stop();
        _sequence = BuildSequence();
        _sequence.Play();
    }

    public void Stop()
    {
        _sequence?.Kill();
        _sequence = null;
    }

    // ?? sequence builder ???????????????????????????????????????????????????????
    private Sequence BuildSequence()
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < Steps.Length; i++)
        {
            // Capture for closure
            string dots = Steps[i];
            string fullText = baseText + dots;

            // Append a callback that sets the text, then hold for stepDuration
            seq.AppendCallback(() =>
            {
                label.text = fullText;
                AnimateLastChar();
            });

            seq.AppendInterval(stepDuration);
        }

        // Loop forever
        seq.SetLoops(-1, LoopType.Restart);

        return seq;
    }

    /// <summary>
    /// Briefly fade-punches the last character for a satisfying "pop" feel.
    /// Uses TMP vertex colours so the rest of the text is unaffected.
    /// </summary>
    private void AnimateLastChar()
    {
        if (fadeDuration <= 0f || label.text.Length == 0) return;

        // Force mesh update so TMP has fresh vertex data
        label.ForceMeshUpdate();

        int lastIndex = label.text.Length - 1;
        TMP_TextInfo textInfo = label.textInfo;

        // Safety check
        if (lastIndex < 0 || lastIndex >= textInfo.characterCount) return;

        TMP_CharacterInfo charInfo = textInfo.characterInfo[lastIndex];
        if (!charInfo.isVisible) return;

        int matIndex = charInfo.materialReferenceIndex;
        int vertIndex = charInfo.vertexIndex;

        Color32[] colours = textInfo.meshInfo[matIndex].colors32;

        // Grab current alpha from the label's colour
        byte targetAlpha = label.color.a < 1f
            ? (byte)(label.color.a * 255)
            : colours[vertIndex].a;

        // Start transparent
        for (int v = 0; v < 4; v++)
            colours[vertIndex + v].a = 0;

        label.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // Tween alpha to full over fadeDuration
        DOVirtual.Float(0f, 1f, fadeDuration, t =>
        {
            byte a = (byte)(t * targetAlpha);
            for (int v = 0; v < 4; v++)
                colours[vertIndex + v].a = a;

            label.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }).SetEase(Ease.OutCubic);
    }
}
