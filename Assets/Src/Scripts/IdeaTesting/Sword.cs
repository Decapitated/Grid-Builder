using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Sword : MonoBehaviour
{
    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    Camera camera;

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

    public float maxHeight = 2f;
    public float holdScale = 1f;
    public float smoothTime = 1f;
    Vector3 velocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        renderer = GetComponentInChildren<Renderer>();
    }

    public float yScale = 2f;
    public float xScale = 0.75f;
    void Update()
    {
        RaycastHit hit;
        if (Utilities.ScreenSphereCast(0.1f, camera, out hit, layerMask))
        {
            mouseHover = hit.point;
            mouseNormal = hit.normal;
            isHovered = true;
        }
        else isHovered = false;

        if (Utilities.GetMouseButtonClicked(0))
        {
            bool justGrabbed = false;
            if(isHovered)
            {
                if (!isHeld)
                {
                    isHeld = true;
                    justGrabbed = true;
                }
            }
            if(isHeld && !justGrabbed) isHeld = false;
        }

        if(rigidbody.velocity.magnitude <= 0.1 && transform.rotation.eulerAngles.magnitude > 0.01)
        {
            if (Input.GetKey(KeyCode.R))
            {
                lerping = true;
                currentRotation = transform.eulerAngles;
            }
        }

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
        }
        
        if(isHeld)
        {
            /*
            Ray rayOrigin = camera.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
            var swordToCam = grabPoint.position - rayOrigin.origin;
            float distAlongLine = Vector3.Dot(swordToCam, rayOrigin.direction);
            Vector3 closestPoint = rayOrigin.origin + rayOrigin.direction * (distAlongLine * holdScale);
            heldTarget = closestPoint;
            */

            Vector2 mousePos = Input.mousePosition; mousePos.x -= (Screen.width / 2f); mousePos.y -= (Screen.height / 2f);
            var worldPoint = TransformPoint(mousePos.normalized);
            //heldTarget = worldPoint;
            Ray rayOrigin = camera.ScreenPointToRay(Input.mousePosition, Camera.MonoOrStereoscopicEye.Mono);
            var swordToCam = worldPoint - rayOrigin.origin;
            float distAlongLine = Vector3.Dot(swordToCam, rayOrigin.direction);
            Vector3 closestPoint = rayOrigin.origin + rayOrigin.direction * (distAlongLine * holdScale);
            heldTarget = closestPoint;
            var move = (heldTarget - grabPoint.position).normalized;
            //rigidbody.velocity = new(move.x, move.y, move.z);
            move.y *= yScale; move.x *= xScale; move.z *= xScale;
            //transform.LookAt(heldTarget);
            rigidbody.AddForceAtPosition(move * smoothTime, grabPoint.position, ForceMode.Force);
        }
    }

    Vector3 GetSwordTop() => renderer.bounds.center + new Vector3(0, renderer.bounds.extents.y, 0);

    Vector3 TransformPoint(Vector2 point)
    {
        var camPos = new Vector3(camera.transform.position.x, 0, camera.transform.position.z);
        var grabPos = new Vector3(grabPoint.position.x, 0, grabPoint.position.z);
        Vector3 camDir = grabPos - camPos;
        Quaternion rotation = Quaternion.LookRotation(camDir.normalized, Vector3.up);
        return grabPoint.position + rotation * new Vector3(point.x, 0, point.y);
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
