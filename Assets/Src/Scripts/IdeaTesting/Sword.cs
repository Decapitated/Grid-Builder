using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
public class Sword : MonoBehaviour
{
    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    FollowCam followCam;

    Rigidbody rigidbody;
    Renderer renderer;

    #region Lerping

    Vector3 currentRotation = Vector3.zero;
    bool lerping = false;
    float lerpStart;
    float lerpDistance;
    #endregion

    #region Mouse Hover Vars

    bool isHovered = false;
    Vector3 mouseHover { get; set; }
    Vector3 mouseNormal { get; set; }
    #endregion

    [SerializeField]
    private Transform grabPoint;
    bool isHeld = false;
    Vector3 heldTarget = Vector3.zero;

    public float speed = 1f;
    public float maxHeight = 2f;
    public float holdScale = 1f;
    public float smoothTime = 1f;
    Vector3 velocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        //rigidbody = GetComponent<Rigidbody>();
        renderer = GetComponentInChildren<Renderer>();
    }

    public float yScale = 2f;
    public float xScale = 0.75f;
    public ForceMode forceMode = ForceMode.Force;

    Vector3 prevDir;
    float prevTime;
    bool firstTime = true;
    public float CurrentVelocity { get; private set; } = 0;

    void Update()
    {
        var tempTex = followCam.camera.targetTexture;
        followCam.camera.targetTexture = null;
        RaycastHit hit;
        if (Utilities.ScreenSphereCast(0.1f, followCam.camera, out hit, layerMask))
        {
            mouseHover = hit.point;
            mouseNormal = hit.normal;
            isHovered = true;
        }
        else isHovered = false;
        followCam.camera.targetTexture = tempTex;

        if (Utilities.GetMouseButtonClicked(0))
        {
            bool justGrabbed = false;
            if(isHovered)
            {
                if (!isHeld)
                {
                    isHeld = true;
                    grabPoint.transform.position = mouseHover;
                    justGrabbed = true;
                    currentRotation = transform.eulerAngles;
                }
            }
            if(isHeld && !justGrabbed) isHeld = false;
        }

        if(Input.GetKey(KeyCode.W)){
            transform.position += transform.forward * speed * Time.deltaTime;
            
        }

        /*if(rigidbody.velocity.magnitude <= 0.1 && transform.rotation.eulerAngles.magnitude > 0.01)
        {
            if (Input.GetKey(KeyCode.R))
            {
                lerping = true;
                currentRotation = transform.eulerAngles;
            }
        }*/
        /*
        if(lerping)
        {
            currentRotation = new Vector3(
                Mathf.LerpAngle(currentRotation.x, 0, Time.deltaTime),
                Mathf.LerpAngle(currentRotation.y, 0, Time.deltaTime),
                Mathf.LerpAngle(currentRotation.z, 0, Time.deltaTime));

            
            if (transform.rotation.eulerAngles.magnitude < 0.01)
            {
                lerping = false;
                transform.eulerAngles = Vector3.zero;
            }
            else transform.eulerAngles = currentRotation;
        }*/
        
        if(isHeld)
        {
            Vector2 mousePos = Input.mousePosition; mousePos.x -= (Screen.width / 2f); mousePos.y -= (Screen.height / 2f);
            var center = transform.position;
            var worldPoint = TransformPoint(mousePos.normalized, center);
            heldTarget = worldPoint;
            /*//heldTarget = GetClosestPointToScreenRay(worldPoint, followCam.camera);
            var move = (heldTarget - grabPoint.position).normalized;
            //rigidbody.velocity = new(move.x, move.y, move.z);
            move.y *= yScale; move.x *= xScale; move.z *= xScale;
            //transform.LookAt(heldTarget);
            rigidbody.AddForceAtPosition(move * smoothTime, grabPoint.position, forceMode);*/
            var move = (new Vector3(heldTarget.x, 0, heldTarget.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
            center.y = maxHeight;

            var currentTime = Time.deltaTime;
            if (!firstTime)
            {
                CurrentVelocity = Mathf.Abs(Vector3.Angle(prevDir, move) / (currentTime - prevTime));

                if (Input.GetKey(KeyCode.E))
                {
                    print("Current Velocity: " + CurrentVelocity);
                }
            } else firstTime = false;

            prevDir = move;
            prevTime = currentTime;

            move = Vector3.RotateTowards(transform.forward, move, Time.deltaTime * smoothTime, 1f);
            transform.SetPositionAndRotation(center, Quaternion.LookRotation(Vector3.up, move));
        }
        followCam.MoveCamera(transform.position);
    }

    Vector3 GetSwordTop() => renderer.bounds.center + new Vector3(0, renderer.bounds.extents.y, 0);

    Vector3 GetClosestPointToScreenRay(Vector3 worldPoint, Camera camera)
    {
        Ray rayOrigin = camera.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
        var swordToCam = worldPoint - rayOrigin.origin;
        float distAlongLine = Vector3.Dot(swordToCam, rayOrigin.direction);
        Vector3 closestPoint = rayOrigin.origin + rayOrigin.direction * (distAlongLine * holdScale);
        return closestPoint;
    }

    Vector3 TransformPoint(Vector2 point, Vector3 center, float scale = 1f)
    {
        var camPos = new Vector3(followCam.camera.transform.position.x, 0, followCam.camera.transform.position.z);
        var centerPos = new Vector3(center.x, 0, center.z);
        Vector3 camDir = centerPos - camPos;
        Quaternion rotation = Quaternion.LookRotation(camDir.normalized, Vector3.up);

        return center + rotation * (new Vector3(point.x, 0, point.y) * scale);
    }

    float MinMax(float value, float min, float max) => Mathf.Min(max, Mathf.Max(value, min));

    static Material lineMaterial;
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    void OnRenderObject()
    {
        CreateLineMaterial();
        // set the current material
        lineMaterial.SetPass(0);

        //if (HoveredHex is not null) DrawSolidHex(HoveredHex, hoverColor);
        if (isHovered && mouseHover != null)
        {
            GL.Begin(GL.LINES);
            GL.Color(Color.blue);

            GL.Vertex3(mouseHover.x, mouseHover.y, mouseHover.z);
            var temp = mouseHover + mouseNormal * 0.25f;
            GL.Vertex3(temp.x, temp.y, temp.z);

            GL.End();
        }

        if (isHeld)
        {
            GL.Begin(GL.LINES);
            GL.Color(Color.blue);

            GL.Vertex3(heldTarget.x, heldTarget.y, heldTarget.z);
            var temp = grabPoint.position;
            GL.Vertex3(temp.x, temp.y, temp.z);

            GL.End();
        }
    }
}
