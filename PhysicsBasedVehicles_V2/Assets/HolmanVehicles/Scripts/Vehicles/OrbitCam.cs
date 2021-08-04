using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCam : MonoBehaviour
{
    [SerializeField] private float OrbitDistance = 6;

   public enum CameraState{
        Follow,
        Orbit,
        Fixed,

        FirstPerson,
    }
    public CameraState cameraState;

    //public InputController inputC;
    public Vehicle Target;
    private Vector3 Position;
    public float zoom = 60f;
    public bool isfixed;
    private Camera cameracomp;
    private Vector2 camRot;
    // Start is called before the first frame update
    void Start()
    {
        cameracomp = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isfixed) logic();
    }
    void FixedUpdate()
    {
        if(isfixed) logic();
    }
    void logic(){
        switch (cameraState)
        {
            case CameraState.Follow:
                Follow();
                break;
            case CameraState.Orbit:
                Orbit();
                break;
            case CameraState.Fixed:
                Fixed();
                break;
        }
        zoom += Input.GetAxis("Mouse ScrollWheel") * 10f;
        zoom = Mathf.Clamp(zoom,20,90);
        cameracomp.fieldOfView = zoom;
    }
    void Follow()
    {
        Position = Target.transform.position;
        Position += Target.transform.up * 3f;
        Position -= Target.transform.forward * 3f;
        transform.position = Vector3.Lerp(transform.position, Position, 0.2f);
        transform.LookAt(Target.transform);
    }
    void Orbit(){
        camRot += new Vector2(-Input.GetAxisRaw("Mouse Y"),Input.GetAxisRaw("Mouse X"));
        camRot.x = Mathf.Clamp(camRot.x,-90,90);
        transform.rotation = Quaternion.Euler(camRot.x,camRot.y,0f);
        transform.position = Target.transform.position + Quaternion.Euler(camRot.x,camRot.y,0f) * new Vector3(0.0f, 0.0f, OrbitDistance);


    }
    void Fixed(){
        transform.position = Target.transform.position -Target.transform.forward * 3;
        transform.rotation = Target.transform.rotation;
    }
}
