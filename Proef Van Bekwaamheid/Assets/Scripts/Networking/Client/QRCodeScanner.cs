using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using System;
using System.Linq;
using System.Collections;

#if UNITY_EDITOR
using Unity.Multiplayer.PlayMode;
#endif

public class QRCodeScanner : MonoBehaviour
{
    public event Action<string> OnIPDecoded;

    [SerializeField] private RawImage _rawImagebackground;
    [SerializeField] private AspectRatioFitter _aspectRatioFitter;
    [SerializeField] private TextMeshProUGUI _textOut;
    [SerializeField] private RectTransform _scanArea;

    public TextMeshProUGUI text;
    public float scanInterval = 0.5f;
    public string decodedIPAddress;

    private WebCamTexture _camTexture;
    private bool _isCamAvailable;

    private void Start()
    {
#if UNITY_EDITOR
        string[] tags = CurrentPlayer.ReadOnlyTags();
        if (Array.IndexOf(tags, "Host") >= 0)
            return;
        else
            StartCoroutine(RequestCameraPermission());

#elif HOST_BUILD
        return;
#else
        StartCoroutine(RequestCameraPermission());
#endif
    }

    private IEnumerator RequestCameraPermission()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("<color=green>[QRCodeScanner] Camera toestemming verleend, wachten op camera...</color>");

            yield return StartCoroutine(WaitForCamera());
        }
        else
        {
            Debug.LogWarning("<color=orange>[QRCodeScanner] Camera toestemming geweigerd.</color>");
            _isCamAvailable = false;
            UIManager.Instance.SetClientUIState(ClientUIState.ManualConnection);
        }
    }

    private IEnumerator WaitForCamera()
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            if (WebCamTexture.devices.Length > 0)
            {
                Debug.Log($"<color=green>[QRCodeScanner] Camera gevonden na {elapsed:F1}s.</color>");
                SetUpCamera();
                yield break;
            }

            Debug.Log("<color=yellow>[QRCodeScanner] Nog geen camera beschikbaar, opnieuw proberen...</color>");
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }

        Debug.LogWarning("<color=orange>[QRCodeScanner] Timeout: geen camera gevonden na toestemming.</color>");
        _isCamAvailable = false;
        UIManager.Instance.SetClientUIState(ClientUIState.ManualConnection);
    }

    private void Update()
    {
        if (_isCamAvailable)
            UpdateCameraRender();
    }

    public void Scan()
    {
        if (!_isCamAvailable || _camTexture == null || !_camTexture.isPlaying)
        {
            Debug.LogWarning("<color=orange>[QRCodeScanner] Scan aangeroepen maar camera is nog niet beschikbaar.</color>");
            return;
        }

        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(_camTexture.GetPixels32(), _camTexture.width, _camTexture.height);

            if (result != null)
            {
                Debug.Log($"<color=green>[QRCodeScanner] QR code gedetecteerd: \"{result.Text}\"</color>");
                _textOut.text = result.Text;
                decodedIPAddress = result.Text;

                if (ValidateIPv4(decodedIPAddress))
                {
                    OnIPDecoded?.Invoke(decodedIPAddress);
                    UIManager.Instance.SetClientUIState(ClientUIState.Controller);
                }
            }
            else
            {
                _textOut.text = "Geen QR code gedetecteerd.";
                Debug.Log("<color=yellow>[QRCodeScanner] Geen QR code in huidig frame.</color>");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"<color=red>[QRCodeScanner] Fout tijdens scannen: {ex.Message}</color>");
            _textOut.text = "Fout bij scannen van QR code.";
        }
    }

    private void UpdateCameraRender()
    {
        if (!_isCamAvailable) return;

        float ratio = (float)_camTexture.width / _camTexture.height;
        _aspectRatioFitter.aspectRatio = ratio;
        int orientation = -_camTexture.videoRotationAngle;
        _rawImagebackground.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);
    }

    private void SetUpCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogWarning("<color=orange>[QRCodeScanner] Geen camera gevonden.</color>");
            _isCamAvailable = false;
            UIManager.Instance.SetClientUIState(ClientUIState.ManualConnection);
            return;
        }

        WebCamDevice selectedDevice = devices[0];
        foreach (WebCamDevice device in devices)
            if (!device.isFrontFacing)
            {
                selectedDevice = device;
                break;
            }

        int width = _scanArea.rect.width > 0 ? (int)_scanArea.rect.width : 1080;
        int height = _scanArea.rect.height > 0 ? (int)_scanArea.rect.height : 1920;

        _camTexture = new WebCamTexture(selectedDevice.name, width, height);
        _camTexture.Play();
        _rawImagebackground.texture = _camTexture;
        _isCamAvailable = true;

        Debug.Log($"<color=cyan>[QRCodeScanner] Camera gestart: {selectedDevice.name} at {width}x{height}</color>");
    }

    private bool ValidateIPv4(string ipString)
    {
        if (String.IsNullOrWhiteSpace(ipString))
            return false;

        string[] splitValues = ipString.Split('.');
        if (splitValues.Length != 4)
            return false;

        byte tempForParsing;
        return splitValues.All(r => byte.TryParse(r, out tempForParsing));
    }
}