using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class RaycastPlane : MonoBehaviour
{
    [Header("Render Colors")]
    [Tooltip("Colors for the Scene Understanding Background objects")]
    public Color ColorForDoorObjects= new Color(0.953f, 0.475f, 0.875f, 1.0f);
    [Tooltip("Colors for the Scene Understanding Wall objects")]
    public Color ColorForWallObjects = new Color(0.953f, 0.494f, 0.475f, 1.0f);
    [Tooltip("Colors for the Scene Understanding Floor objects")]
    public Color ColorForFloorObjects = new Color(0.733f, 0.953f, 0.475f, 1.0f);
    [Tooltip("Colors for the Scene Understanding Ceiling objects")]
    public Color ColorForCeilingObjects = new Color(0.475f, 0.596f, 0.953f, 1.0f);
    [Tooltip("Colors for the Scene Understanding Platform objects")]
    public Color ColorForTableObjects = new Color(0.204f, 0.792f, 0.714f, 1.0f);
    [Tooltip("Colors for the Scene Understanding Unknown objects")]
    public Color ColorForBackgroundObjects = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    [Tooltip("Colors for the Scene Understanding Inferred objects")]
    public Color ColorForWindowObjects = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    [Tooltip("Colors for the World mesh")]
    public Color ColorForWorldObjects = new Color(0.0f, 1.0f, 1.0f, 1.0f);

    ARPlane m_Plane;
    public Mesh mesh { get; private set; }
    float? m_InitialLineWidthMultiplier;
    public bool keepItVisible = false;

    EventManager manager = null;

    private PlaneClassification planeClassification;

    void Awake()
    {
        mesh = new Mesh();
        m_Plane = GetComponent<ARPlane>();
        manager = EventManager.Instance;
    }

    private void OnEnable()
    {
        m_Plane.boundaryChanged += OnBoundaryChanged;
    }

    private void OnDisable()
    {
        m_Plane.boundaryChanged -= OnBoundaryChanged;
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            var lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                if (!m_InitialLineWidthMultiplier.HasValue)
                    m_InitialLineWidthMultiplier = lineRenderer.widthMultiplier;

                lineRenderer.widthMultiplier = m_InitialLineWidthMultiplier.Value * transform.lossyScale.x;
            }
            else
            {
                m_InitialLineWidthMultiplier = null;
            }

            transform.hasChanged = false;
        }

        if (m_Plane.subsumedBy != null)
        {
            //SetVisible(false);
        }

        if (planeClassification != m_Plane.classification)
        {
            var mr = GetComponent<MeshRenderer>();
            var mat = mr.sharedMaterial;
            switch (m_Plane.classification)
            {
                case UnityEngine.XR.ARSubsystems.PlaneClassification.Wall:
                    mat.color = ColorForWallObjects;
                    break;
                case UnityEngine.XR.ARSubsystems.PlaneClassification.Floor:
                    mat.color = ColorForFloorObjects;
                    break;
                case UnityEngine.XR.ARSubsystems.PlaneClassification.Ceiling:
                    mat.color = ColorForCeilingObjects;
                    break;
                case UnityEngine.XR.ARSubsystems.PlaneClassification.Table:
                    mat.color = ColorForTableObjects;
                    break;
                case UnityEngine.XR.ARSubsystems.PlaneClassification.Seat:
                    mat.color = ColorForTableObjects;
                    break;
                case UnityEngine.XR.ARSubsystems.PlaneClassification.Door:
                    mat.color = ColorForDoorObjects;
                    break;
                case UnityEngine.XR.ARSubsystems.PlaneClassification.Window:
                    mat.color = ColorForWindowObjects;
                    break;
                case UnityEngine.XR.ARSubsystems.PlaneClassification.Other:
                    mat.color = ColorForWorldObjects;
                    break;
            }
            planeClassification = m_Plane.classification;
        }
    }

    void OnBoundaryChanged(ARPlaneBoundaryChangedEventArgs eventArgs)
    {
        var boundary = m_Plane.boundary;
        if (!ARPlaneMeshGenerators.GenerateMesh(mesh, new Pose(transform.localPosition, transform.localRotation), boundary))
            return;

        var lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = boundary.Length;
            for (int i = 0; i < boundary.Length; ++i)
            {
                var point2 = boundary[i];
                lineRenderer.SetPosition(i, new Vector3(point2.x, 0, point2.y));
            }
        }

        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
            meshFilter.sharedMesh = mesh;

        var meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
            meshCollider.sharedMesh = mesh;
    }

    void SetRendererEnabled<T>(bool visible) where T : Renderer
    {
        var component = GetComponent<T>();
        if (component)
        {
            component.enabled = visible;
        }
    }

    public void SetVisible(bool visible)
    {
        SetRendererEnabled<MeshRenderer>(visible);
        //SetRendererEnabled<LineRenderer>(visible);
    }

    public void OnHoverEntered() {
        this.SetVisible(true);
    }

    public void OnHoveredExit() {
        if (!keepItVisible) {
            this.SetVisible(false);
        }
    }

    public void AddToRootSecene() { 
        //keepItVisible = !keepItVisible;
        //SetVisible(keepItVisible);
        manager.AddToRootSecene(m_Plane);
    }
}
