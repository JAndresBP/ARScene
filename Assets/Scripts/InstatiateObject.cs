using Assets.Scripts;
using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstatiateObject : MonoBehaviour
{
    EventManager eventManager = null;
    bool instantiating = false;
    // Start is called before the first frame update
    void Start()
    {
        eventManager = EventManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CloneObject() {
        if (!instantiating)
        {
            instantiating = true;
            var newObject = Instantiate<GameObject>(gameObject, Vector3.zero, Quaternion.identity);
            newObject.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 2.0f);
            newObject.transform.localScale = Vector3.one;
            var comp = newObject.GetComponent<StatefulInteractable>();
            Destroy(comp);

            newObject.AddComponent<BoundsControl>();
            newObject.AddComponent<Rigidbody>();
            newObject.AddComponent<ConstraintManager>();
            newObject.AddComponent<ObjectManipulator>();

            eventManager.AddGOToRootSecene(newObject);
            instantiating = false;  
        }
    }
}
