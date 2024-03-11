using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;

namespace Assets.Scripts
{
    public class EventManager:MonoBehaviour
    {
        public event Action<ARPlane> AddToRootSceneEvt;
        public event Action<DetectionResponse, Vector3, Quaternion> LocateRotationResultEvt;
        public event Action<GameObject> AddGOToRootSceneEvt;
        
        public static EventManager Instance { get; private set; }

        private void Start()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
            
        }

        public void AddToRootSecene(ARPlane plane) {
            if (AddToRootSceneEvt != null)
            {
                AddToRootSceneEvt.Invoke(plane);
            }
        }

        internal void LocateDetectionResult(DetectionResponse response, Vector3 position, Quaternion rotation)
        {
            if (LocateRotationResultEvt != null)
            {
                LocateRotationResultEvt.Invoke(response, position, rotation);
            }
        }

        internal void AddGOToRootSecene(GameObject newObject)
        {
            if (AddGOToRootSceneEvt != null)
            {
                AddGOToRootSceneEvt.Invoke(newObject);
            }
        }
    }
}
