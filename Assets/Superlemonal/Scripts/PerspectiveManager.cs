﻿// Forced Perspective Illusion
// Project from Daniel C Menezes
// V1.32 - 17/09/2019
// GitHub: https://github.com/danielcmcg/Forced-Perspective-Illusion-Mechanic-for-Unity

using UnityEngine;
using UnityEngine.UI;

public class PerspectiveManager : MonoBehaviour {
    public GameObject gameobjecttodeactivate;
    public GameObject signatureobject;
    public bool isGrabbing = false;
    //bool toneAble = false;
    //bool tone2able = false;
    bool signatureAble = true;
    public float objectmass = 10000000000;

    //public Material yellowToon;
    //public Material blueToon;
    //public Material redToon;
    //public SpriteRenderer handspriterenderer;
    public Sprite handnograbby;
    public Sprite handaboutgrabby;
    public Sprite handgrabby;
    private Camera mainCamera;
    private Transform targetForTakenObjects;
    public GameObject pointer;
    //public Image pointer;
    private GameObject takenObject;
    private RaycastHit hit;
    private Ray ray;
    private float distanceMultiplier;
    private Vector3 scaleMultiplier;
    public LayerMask layerMask = ~(1 << 8);
    private int originalLayer;
    private float cameraHeight = 0;
    private float cosine;
    private float positionCalculation;
    private float lastPositionCalculation = 0;
    private Vector3 lastHitPoint = Vector3.zero;
    private Vector3 lastRotation = Vector3.zero;
    private float rayMaxRange = 1000f;
    private bool isRayTouchingSomething = true;
    private float lastRotationY;
    
    private Vector3 lastHit = Vector3.zero;
    private Vector3 centerCorrection = Vector3.zero;
    private float takenObjSize = 0;
    private int takenObjSizeIndex = 0;
    public placeHolder placeHolderScript;
    public enum grbmode{Normal, Rotating}
    public grbmode grabMode = grbmode.Normal;
    FMOD.Studio.EventInstance pop1instance;
    FMOD.Studio.EventInstance pop2instance;
    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        targetForTakenObjects = GameObject.Find("targetForTakenObjects").transform;
        pop1instance = FMODUnity.RuntimeManager.CreateInstance("event:/pop1");
        pop2instance = FMODUnity.RuntimeManager.CreateInstance("event:/pop2");
        //pointer = GameObject.Find("Pointer");
        //pointer.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, (Screen.height / 2) + (Screen.height / 10), 1));
        //pointer.transform.parent = mainCamera.transform;
    }

    void Update()
    {
        ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, (Screen.height / 2.5f) + (Screen.height / 10), 0));
        Debug.DrawRay(ray.origin, ray.direction * 200, Color.yellow);

        if (Physics.Raycast(ray, out hit, rayMaxRange, layerMask))
        {
            if (hit.transform.tag == "Getable" || hit.transform.tag == "PictureOnTheWall")
            {
                //pointer.GetComponent<MeshRenderer>().material = blueToon;
                pointer.GetComponent<Image>().sprite = handaboutgrabby;
            }
            else
            {
                //pointer.GetComponent<MeshRenderer>().material = yellowToon;
                pointer.GetComponent<Image>().sprite = handnograbby;
            }
            if (hit.transform.tag == "vendingButton")
            {
                if (hit.transform.gameObject.GetComponent<vendingButton>().hasBeenPressed == false)
                {
                    pointer.GetComponent<Image>().sprite = handaboutgrabby;
                }
            }
            else if(hit.transform.tag == "vendingButton")
            {
                if (hit.transform.gameObject.GetComponent<vendingButton>().hasBeenPressed == false)
                {
                    //pointer.GetComponent<MeshRenderer>().material = yellowToon;
                    pointer.GetComponent<Image>().sprite = handnograbby;
                }
            }
        }

        isRayTouchingSomething = Physics.Raycast(ray, out hit, rayMaxRange, layerMask);  

        if (takenObject != null)
        {
            //pointer.GetComponent<MeshRenderer>().material = redToon;
            pointer.GetComponent<Image>().sprite = handgrabby;
            //pop.Play();
        }
        else
        {
            targetForTakenObjects.position = hit.point;
        }

        if ((Input.GetMouseButtonDown(0)) && isRayTouchingSomething)
        {
            placeHolderScript.vend(ray, hit, isRayTouchingSomething, rayMaxRange, layerMask, objectmass);
            if (hit.transform.tag == "Getable")
            {
                hit.transform.gameObject.GetComponent<Outline>().enabled = true;
                isGrabbing = true;
                FMODUnity.RuntimeManager.PlayOneShot("event:/pop1");
                takenObject = hit.transform.gameObject;
                //pop1instance.setPitch(takenObject.transform.localScale.magnitude);
                //pop1instance.start();

                distanceMultiplier = Vector3.Distance(mainCamera.transform.position, takenObject.transform.position);
                scaleMultiplier = takenObject.transform.localScale;
                lastRotation = takenObject.transform.rotation.eulerAngles;
                lastRotationY = lastRotation.y - mainCamera.transform.eulerAngles.y;
                takenObject.transform.transform.parent = targetForTakenObjects; 

                if (takenObject.GetComponent<Rigidbody>() == null)
                {
                    takenObject.AddComponent<Rigidbody>();
                    takenObject.GetComponent<Rigidbody>().mass = objectmass;
                    takenObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                }
                takenObject.GetComponent<Rigidbody>().isKinematic = true;

                foreach (Collider col in takenObject.GetComponents<Collider>())
                {
                    col.isTrigger = true;
                }

                if (takenObject.GetComponent<MeshRenderer>() != null)
                {
                    //takenObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    //takenObject.GetComponent<MeshRenderer>().receiveShadows = false;
                }
                originalLayer = takenObject.layer;
                takenObject.gameObject.layer = 8;
                foreach (Transform child in takenObject.GetComponentsInChildren<Transform>())
                {
                    takenObject.GetComponent<Rigidbody>().isKinematic = true;
                    takenObject.GetComponent<Collider>().isTrigger = true;
                    child.gameObject.layer = 8;
                }

                takenObjSize = takenObject.GetComponent<Collider>().bounds.size.y;
                takenObjSizeIndex = 1;
                if (takenObject.GetComponent<Collider>().bounds.size.x > takenObjSize)
                {
                    takenObjSize = takenObject.GetComponent<Collider>().bounds.size.x;
                    takenObjSizeIndex = 0;
                }
                if (takenObject.GetComponent<Collider>().bounds.size.z > takenObjSize)
                {
                    takenObjSize = takenObject.GetComponent<Collider>().bounds.size.z;
                    takenObjSizeIndex = 2;
                }
            }
        }
    
        if (Input.GetMouseButton(0))
        {
            //targetForTakenObjects.transform.localScale = new Vector3(1, 1, 1);
            if (takenObject != null)
            {
                // recenter the object to the center of the mesh regardless  real pivot point
                if (takenObject.GetComponent<MeshRenderer>() != null)
                {
                    centerCorrection = takenObject.transform.position - takenObject.GetComponent<MeshRenderer>().bounds.center;
                }
                else if (takenObject.transform.GetChild(0).transform.GetChild(0).GetComponent<MeshRenderer>() != null)
                {
                    centerCorrection = takenObject.transform.position - takenObject.transform.GetChild(0).transform.GetChild(0).GetComponent<MeshRenderer>().bounds.center;
                }
    
                takenObject.transform.position = Vector3.Lerp(takenObject.transform.position, targetForTakenObjects.position + centerCorrection, Time.deltaTime * 20);
                if (grabMode == grbmode.Normal)
                {
                    if (takenObject.GetComponent<GettableObj>().rotationType == GettableObj.rotation.Free)
                    {
                        targetForTakenObjects.transform.localScale = new Vector3(1, 1, 1);
                        targetForTakenObjects.transform.parent = null;
                        takenObject.transform.rotation = Quaternion.Euler(new Vector3(lastRotation.x, lastRotationY + mainCamera.transform.eulerAngles.y, lastRotation.z));
                    }
                    else if (takenObject.GetComponent<GettableObj>().rotationType == GettableObj.rotation.Fixed)
                    {
                        targetForTakenObjects.transform.parent = mainCamera.transform;
                    }
                }
    
                
                cosine = Vector3.Dot(ray.direction, hit.normal);
                cameraHeight = Mathf.Abs(hit.distance * cosine);
                
                takenObjSize = takenObject.GetComponent<Collider>().bounds.size[takenObjSizeIndex];
                
                /*Collider[] colliders = Physics.OverlapBox(takenObject.transform.position, new Vector3(takenObjSize/2, takenObjSize/2, takenObjSize/2), takenObject.transform.rotation);
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != takenObject.GetComponent<Collider>())
                    {
                        Debug.Log(colliders[i].name);
                    }
                }*/
                
                positionCalculation = (hit.distance * takenObjSize / 2) / (cameraHeight);
                if (positionCalculation < rayMaxRange)
                {
                    lastPositionCalculation = positionCalculation;
                }
                
                // if the wall is more distant then the raycast max range, increase the size only untill the max range
                if (isRayTouchingSomething)
                {
                    lastHitPoint = hit.point;
                }
                else
                {
                    lastHitPoint = mainCamera.transform.position + ray.direction * rayMaxRange;
                }
                
                //targetForTakenObjects.position = Vector3.Lerp(targetForTakenObjects.position, lastHitPoint - (ray.direction * lastPositionCalculation), Time.deltaTime * 10);
                targetForTakenObjects.position = lastHitPoint - (ray.direction * lastPositionCalculation);
                
                takenObject.transform.localScale = scaleMultiplier * (Vector3.Distance(mainCamera.transform.position, takenObject.transform.position) / distanceMultiplier);
            }
        }

        if (isGrabbing)
        {
            HandleRotateObject();
            if (Input.GetMouseButtonDown(1))
            {
                grabMode = grbmode.Rotating;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                lastRotation = takenObject.transform.rotation.eulerAngles;
                lastRotationY = lastRotation.y - mainCamera.transform.eulerAngles.y;
                grabMode = grbmode.Normal;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (takenObject != null)
            {
                releaseObject();
            }
        }

        if (Physics.Raycast(ray, out hit, 2, layerMask))
        {
            if (hit.transform.tag == "agreementpaper")
            {
                if (signatureAble)
                {
                    pointer.GetComponent<Image>().sprite = handaboutgrabby;
                } else
                {
                    pointer.GetComponent<Image>().sprite = handnograbby;
                }
                if ((Input.GetMouseButtonDown(0)) && isRayTouchingSomething)
                {
                    if (hit.transform.tag == "agreementpaper")
                    {
                        if (signatureAble)
                        {
                            gameobjecttodeactivate.SetActive(false);
                            signatureobject.SetActive(true);
                            FMODUnity.RuntimeManager.PlayOneShot("event:/sig");
                            signatureAble = false;
                        }
                    }
                }
            }
        }
    }

    public void HandleRotateObject()
    {
        if (grabMode == grbmode.Rotating)
        {
            if (takenObject != null)
            {
                Vector3 objectRotation = new Vector3(takenObject.transform.rotation.eulerAngles.x, takenObject.transform.rotation.eulerAngles.y, takenObject.transform.rotation.eulerAngles.z);
                takenObject.transform.eulerAngles = new Vector3(objectRotation.x, objectRotation.y + -Input.GetAxis("Mouse X") * 3, objectRotation.z);
            }
        }
    }

    public void releaseObject()
    {
        grabMode = grbmode.Normal;
        isGrabbing = false;

        //toneAble = true;
        FMODUnity.RuntimeManager.PlayOneShot("event:/pop2");
        //pop2instance.setPitch(takenObject.transform.localScale.magnitude);
        //pop2instance.start();
        //toneAble = false;

        takenObject.GetComponent<Rigidbody>().isKinematic = false;
        takenObject.GetComponent<Outline>().enabled = false;

        foreach (Collider col in takenObject.GetComponents<Collider>())
        {
            col.isTrigger = false;
        }

        if (takenObject.GetComponent<MeshRenderer>() != null)
        {
            //takenObject.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            //takenObject.GetComponent<MeshRenderer>().receiveShadows = true;
        }
        takenObject.transform.parent = null;
        takenObject.gameObject.layer = originalLayer;
        foreach (Transform child in takenObject.GetComponentsInChildren<Transform>())
        {
            takenObject.GetComponent<Rigidbody>().isKinematic = false;
            takenObject.GetComponent<Collider>().isTrigger = false;
            child.gameObject.layer = originalLayer;
        }

        takenObject = null;
    }
}