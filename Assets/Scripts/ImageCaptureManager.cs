using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Windows.WebCam;

public class ImageCaptureManager : MonoBehaviour
{
    [SerializeField]
    private Shader textureShader = null;

    [SerializeField]
    private TextMeshPro text = null;

    public UnityEvent CameraReadyEvent = null;

    private PhotoCapture photoCaptureObject = null;
    private Resolution cameraResolution = default(Resolution);
    private bool isCapturingPhoto, isReadyToCapturePhoto = false;
    private uint numPhotos = 0;
    private EventManager eventManager;

    // Start is called before the first frame update
    void Start()
    {
        eventManager = EventManager.Instance;

        var resolutions = PhotoCapture.SupportedResolutions;
        if (resolutions == null || resolutions.Count() == 0)
        {
            if (text != null)
            {
                text.text = "Resolutions not available. Did you provide web cam access?";
            }
            return;
        }

        cameraResolution = resolutions.OrderByDescending((res) => res.width * res.height).First();
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);

        if (text != null)
        {
            text.text = "Starting camera...";
        }
    }

    private void OnDestroy()
    {
        isReadyToCapturePhoto = false;

        if (photoCaptureObject != null)
        {
            photoCaptureObject.StopPhotoModeAsync(OnPhotoCaptureStopped);

            if (text != null)
            {
                text.text = "Stopping camera...";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        if (text != null)
        {
            text.text += "\nPhotoCapture created...";
        }

        photoCaptureObject = captureObject;

        CameraParameters cameraParameters = new CameraParameters(WebCamMode.PhotoMode)
        {
            hologramOpacity = 0.0f,
            cameraResolutionWidth = cameraResolution.width,
            cameraResolutionHeight = cameraResolution.height,
            pixelFormat = CapturePixelFormat.BGRA32
        };

        captureObject.StartPhotoModeAsync(cameraParameters, OnPhotoModeStarted);
    }
    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            isReadyToCapturePhoto = true;

            if (text != null)
            {
                text.text = "Ready!\nPress the above button to take a picture.";
            }

            if (CameraReadyEvent != null) {
                CameraReadyEvent.Invoke();
            }

        }
        else
        {
            isReadyToCapturePhoto = false;

            if (text != null)
            {
                text.text = "Unable to start photo mode!";
            }
        }
    }

    /// <summary>
    /// Takes a photo and attempts to load it into the scene using its location data.
    /// </summary>
    public void TakePhoto()
    {
        if (!isReadyToCapturePhoto || isCapturingPhoto)
        {
            return;
        }

        isCapturingPhoto = true;

        if (text != null)
        {
            text.text = "Taking picture...";
        }

        photoCaptureObject.TakePhotoAsync(OnPhotoCaptured);
    }

    private void OnPhotoCaptured(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        
        if (result.success)
        {
            var rawData = new List<byte>();
            photoCaptureFrame.CopyRawImageDataIntoBuffer(rawData);

            if (text != null)
            {
                text.text += "\nTook picture!";
            }

            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = $"Photo{numPhotos++}";
            quad.transform.parent = transform;

            float ratio = cameraResolution.height / (float)cameraResolution.width;
            quad.transform.localScale = new Vector3(quad.transform.localScale.x, quad.transform.localScale.x * ratio, quad.transform.localScale.z);

            Renderer quadRenderer = quad.GetComponent<Renderer>();
            quadRenderer.material = new Material(textureShader);
            Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);
            quadRenderer.sharedMaterial.SetTexture("_MainTex", targetTexture);

            if (photoCaptureFrame.hasLocationData)
            {
                photoCaptureFrame.TryGetCameraToWorldMatrix(out Matrix4x4 cameraToWorldMatrix);

                Vector3 position = cameraToWorldMatrix.GetColumn(3) - cameraToWorldMatrix.GetColumn(2);
                Quaternion rotation = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));

                photoCaptureFrame.TryGetProjectionMatrix(Camera.main.nearClipPlane, Camera.main.farClipPlane, out Matrix4x4 projectionMatrix);

                targetTexture.wrapMode = TextureWrapMode.Clamp;

                quadRenderer.sharedMaterial.SetMatrix("_WorldToCameraMatrix", cameraToWorldMatrix.inverse);
                quadRenderer.sharedMaterial.SetMatrix("_CameraProjectionMatrix", projectionMatrix);

                quad.transform.position = position;
                quad.transform.rotation = rotation;

                if (text != null)
                {
                    text.text += $"\nPosition: ({position.x}, {position.y}, {position.z})";
                    text.text += $"\nRotation: ({rotation.x}, {rotation.y}, {rotation.z}, {rotation.w})";
                }
            }
            else
            {
                if (text != null)
                {
                    text.text += "\nNo location data :(";
                }
            }

            StartCoroutine(sendDetectionRequest(rawData, quad.transform.position, quad.transform.rotation));
        }
        else
        {
            if (text != null)
            {
                text.text += "\nPicture taking failed: " + result.hResult;
            }
        }

        isCapturingPhoto = false;
    }

    private IEnumerator sendDetectionRequest(List<byte> rawData, Vector3 position, Quaternion rotation)
    {
        var data = new { b64Image = Convert.ToBase64String(rawData.ToArray()) };
        var jsondata = JsonUtility.ToJson(data);
        var uri = new Uri("http://127.0.0.1:8000");
        using (var webclient = UnityWebRequest.Post(uri, jsondata , "application/json"))
        {
            yield return webclient.SendWebRequest();

            if (webclient.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(webclient.error);
            }
            else
            {
                var responseStr = webclient.downloadHandler.text;
                var response = JsonUtility.FromJson<DetectionResponse>(responseStr);
                eventManager.LocateDetectionResult(response,position,rotation);
                Debug.Log($"Form upload complete! {responseStr}");
            }
        }
    }

    private void OnPhotoCaptureStopped(PhotoCapture.PhotoCaptureResult result)
    {
        if (text != null)
        {
            text.text = result.success ? "Photo mode stopped." : "Unable to stop photo mode.";
        }

        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
}

public class Detection {
    public int x { get; set; }
    public int y { get; set; }
    public int w { get; set; }
    public int h { get; set; }
    public string label { get; set; }
    public float confidence { get; set; }
}
public class DetectionResponse {
    public List<Detection> detectionList { get; set; }
}
