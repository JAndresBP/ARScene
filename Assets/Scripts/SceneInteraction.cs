using Assets.Scripts;
using MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static Microsoft.MixedReality.GraphicsTools.MeshInstancer;

[RequireComponent(typeof(ARPlaneManager))]
public class SceneInteraction : MonoBehaviour
{
    public ARMeshManager meshManager = null;
    private ARRaycastManager m_raycastManager;
    private ARPlaneManager m_planeManager;
    public GameObject indicator;
    public GameObject rootModel = null;

    private List<GameObject> savedPlanes = new List<GameObject>();

    private GameObject suMinimap = null;
    public EventManager eventManager = null;
    


    // Start is called before the first frame update
    void Start()
    {
        m_planeManager = GetComponent<ARPlaneManager>();
        m_raycastManager = GetComponent<ARRaycastManager>();
        eventManager.AddToRootSceneEvt += AddToRootModel;
        eventManager.LocateRotationResultEvt += LocateRotationResultEvt;
        eventManager.AddGOToRootSceneEvt += AddGOToRootSceneEvt;  
    }

    private void AddGOToRootSceneEvt(GameObject obj)
    {
        obj.transform.parent = rootModel.transform;
    }

    private void LocateRotationResultEvt(DetectionResponse detection, Vector3 position, Quaternion rotation)
    {
        Vector3 forward = new Pose(position, rotation).forward;
        List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

        if (m_raycastManager.Raycast(new Ray(position, forward), raycastHits, TrackableType.AllTypes))
        { 
            var firstHit = raycastHits.First();
            var rect = Instantiate(indicator);
            rect.transform.parent = rootModel.transform;
            rect.transform.position = firstHit.pose.position;
            rect.transform.rotation = rotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void ShowAllPlanes(bool showAllPlanes) {
        foreach (var trackeable in m_planeManager.trackables) {
            var planev = trackeable.GetComponent<RaycastPlane>();
            planev.keepItVisible = showAllPlanes;
            planev.SetVisible(showAllPlanes);
        }
    }

    public void ShowMesh(bool showMesh) {
        foreach (var mesh in meshManager.meshes) { 
            mesh.GetComponent<MeshRenderer>().enabled = showMesh;
        }
        
    }

    public void AddToRootModel(ARPlane plane) {

        var sceneElement = Instantiate(plane.gameObject);
        var c1 = sceneElement.GetComponent<RaycastPlane>();
        Destroy(c1);
        var c2 = sceneElement.GetComponent<ARPlane>();
        Destroy(c2);
        var c3 = sceneElement.GetComponent<StatefulInteractable>();
        Destroy(c3);

        var properties = sceneElement.AddComponent<SceneElementProperties>();
        properties.suObjectGUID = plane.trackableId;

        sceneElement.transform.parent = rootModel.transform;

        savedPlanes.Add(sceneElement);
    }

    public void MinimapOn() {

        ShowAllPlanes(false);
        ShowMesh(false);

        if (suMinimap == null)
        {
            suMinimap = Instantiate(rootModel);
            suMinimap.name = "Minimap";
            suMinimap.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
            suMinimap.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            rootModel.SetActive(false);
        }
    }

    public void MiniMapOff()
    {
        if (suMinimap != null)
        {
            DestroyImmediate(suMinimap);
            suMinimap = null;
        }
        rootModel.SetActive(true);
    }
}
