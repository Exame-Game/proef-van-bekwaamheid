using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using System;


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
                SetUpCamera();

    #elif HOST_BUILD
            return;
    #else
            SetUpCamera();
    #endif
    }

    private void Update()
    {
        if (_isCamAvailable)
            UpdateCameraRender();
        else if (!_isCamAvailable)
            ClientUIManager.Instance.SetUIState(ClientUIState.ManualConnection);
    }

    public void Scan()
    {
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(_camTexture.GetPixels32(), _camTexture.width, _camTexture.height);

            if (result != null)
            {
                Debug.Log($"<color=green>[QRCodeScanner] QR code detected — decoded text: \"{result.Text}\"</color>");
                _textOut.text = result.Text;
                decodedIPAddress = result.Text;
                OnIPDecoded?.Invoke(decodedIPAddress);
                ClientUIManager.Instance.SetUIState(ClientUIState.Disconnect);
            }
            else
            {
                _textOut.text = "No QR code detected.";
                Debug.Log("<color=yellow>[QRCodeScanner] No QR code detected in current frame.</color>");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"<color=red>[QRCodeScanner] Error during QR code scanning: {ex.Message}</color>");
            _textOut.text = "Error scanning QR code.";
        }
    }

    private void UpdateCameraRender()
    {
        if (!_isCamAvailable)
            return;

        float ratio = (float)_camTexture.width / (float)_camTexture.height;
        _aspectRatioFitter.aspectRatio = ratio;
        int orientation = -_camTexture.videoRotationAngle;

        _rawImagebackground.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);
    }

    //private void SetUpCamera()
    //{
    //    WebCamDevice[] devices = WebCamTexture.devices;
    //    if (devices.Length == 0)
    //    {
    //        Debug.LogWarning("<color=orange>[QRCodeScanner] No camera devices found on this system.</color>");
    //        _isCamAvailable = false;
    //        return;
    //    }

    //    for(int i = 0; i < devices.Length; i++)
    //        if (!devices[i].isFrontFacing)
    //            _camTexture = new WebCamTexture(devices[i].name, (int)_scanArea.rect.width, (int)_scanArea.rect.height);

    //    _camTexture.Play();
    //    _rawImagebackground.texture = _camTexture;
    //    _isCamAvailable = true;
    //}

    private void SetUpCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogWarning("<color=orange>[QRCodeScanner] No camera devices found.</color>");
            _isCamAvailable = false;
            return;
        }

        // Try back-facing first, fall back to any available camera
        WebCamDevice selectedDevice = devices[0];
        foreach (var device in devices)
        {
            if (!device.isFrontFacing)
            {
                selectedDevice = device;
                break;
            }
        }

        // Use a safe default resolution if scan area isn't laid out yet
        int width = _scanArea.rect.width > 0 ? (int)_scanArea.rect.width : 640;
        int height = _scanArea.rect.height > 0 ? (int)_scanArea.rect.height : 480;

        _camTexture = new WebCamTexture(selectedDevice.name, width, height);
        _camTexture.Play();
        _rawImagebackground.texture = _camTexture;
        _isCamAvailable = true;

        Debug.Log($"<color=cyan>[QRCodeScanner] Started camera: {selectedDevice.name} at {width}x{height}</color>");
    }
}
