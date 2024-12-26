using UnityEngine;
using static DebugLogger;

public class ArrowIndicator : MonoBehaviour {
    private LineRenderer mainLine;
    private GameObject headObject1;
    private GameObject headObject2;
    private LineRenderer headLine1;
    private LineRenderer headLine2;
    private bool isVisible = false;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float arrowWidth = 0.2f;
    private float headLength = 0.5f;
    private float headAngle = 25f;
    private Color currentColor = Color.red;

    private void Awake() {
        SetupArrow();
    }

    private void SetupArrow() {
        // Setup main line
        mainLine = gameObject.AddComponent<LineRenderer>();
        mainLine.positionCount = 2;
        mainLine.startWidth = arrowWidth;
        mainLine.endWidth = arrowWidth;
        mainLine.material = new Material(Shader.Find("Sprites/Default"));
        mainLine.sortingOrder = 5000; // Ensure it renders above other UI elements
        mainLine.receiveShadows = false;
        mainLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mainLine.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        mainLine.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

        // Create arrow heads
        headObject1 = new GameObject("ArrowHead1");
        headObject2 = new GameObject("ArrowHead2");
        headObject1.transform.SetParent(transform);
        headObject2.transform.SetParent(transform);

        headLine1 = headObject1.AddComponent<LineRenderer>();
        headLine2 = headObject2.AddComponent<LineRenderer>();

        SetupHeadLine(headLine1);
        SetupHeadLine(headLine2);

        // Hide initially
        Hide();

        Log("Arrow indicator setup complete", LogTag.UI);
    }

    private void SetupHeadLine(LineRenderer line) {
        line.positionCount = 2;
        line.startWidth = arrowWidth * 1.2f;
        line.endWidth = 0f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingOrder = 5000;
        line.receiveShadows = false;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        line.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
    }

    public static ArrowIndicator Create(Transform parent = null) {
        GameObject arrowObj = new GameObject("ArrowIndicator");
        if (parent != null) {
            arrowObj.transform.SetParent(parent, false);
            arrowObj.transform.localPosition = Vector3.zero;
        }
        var indicator = arrowObj.AddComponent<ArrowIndicator>();
        Log("Created new arrow indicator", LogTag.UI);
        return indicator;
    }

    public void SetColor(Color newColor) {
        currentColor = newColor;
        if (mainLine != null && mainLine.material != null) {
            mainLine.material.color = currentColor;
        }
        if (headLine1 != null && headLine1.material != null) {
            headLine1.material.color = currentColor;
        }
        if (headLine2 != null && headLine2.material != null) {
            headLine2.material.color = currentColor;
        }
    }

    public void Show(Vector3 start, Vector3 end) {
        startPosition = start;
        endPosition = end;

        Log($"Showing arrow from {start} to {end}", LogTag.UI);

        if (mainLine == null || headLine1 == null || headLine2 == null) {
            LogWarning("Arrow components missing, recreating...", LogTag.UI);
            SetupArrow();
        }

        // Ensure Z position is consistent
        startPosition.z = 0;
        endPosition.z = 0;

        // Update main line
        mainLine.enabled = true;
        mainLine.SetPosition(0, startPosition);
        mainLine.SetPosition(1, endPosition);
        mainLine.material.color = currentColor;

        UpdateArrowHead();

        // Ensure visibility
        gameObject.SetActive(true);
        headObject1.SetActive(true);
        headObject2.SetActive(true);

        isVisible = true;
    }

    private void UpdateArrowHead() {
        if (headLine1 == null || headLine2 == null) return;

        Vector3 direction = (endPosition - startPosition).normalized;
        Vector3 right = Quaternion.Euler(0, 0, 90) * direction;

        Vector3 arrowTip = endPosition;
        Vector3 arrowBase = arrowTip - (direction * headLength);
        float baseWidth = headLength * Mathf.Tan(headAngle * Mathf.Deg2Rad);
        Vector3 arrowBaseLeft = arrowBase + (right * baseWidth);
        Vector3 arrowBaseRight = arrowBase - (right * baseWidth);

        headLine1.SetPosition(0, arrowBaseLeft);
        headLine1.SetPosition(1, arrowTip);
        headLine1.enabled = true;
        headLine1.material.color = currentColor;

        headLine2.SetPosition(0, arrowBaseRight);
        headLine2.SetPosition(1, arrowTip);
        headLine2.enabled = true;
        headLine2.material.color = currentColor;

        mainLine.SetPosition(1, arrowBase);
    }

    public void ShowSwapAction(Vector3 start1, Vector3 start2) {
        startPosition = start1;
        endPosition = start2;

        Log($"Showing swap arrow between {start1} and {start2}", LogTag.UI);

        if (mainLine == null || headLine1 == null || headLine2 == null) {
            LogWarning("Arrow components missing for swap, recreating...", LogTag.UI);
            SetupArrow();
        }

        // Ensure Z position is consistent
        startPosition.z = 0;
        endPosition.z = 0;

        // Use yellow for swap actions
        SetColor(Color.yellow);

        // Create curved path
        Vector3 midPoint = Vector3.Lerp(startPosition, endPosition, 0.5f);
        midPoint.y += 1.5f; // Add arc height

        // Update line positions for curved path
        mainLine.positionCount = 3;
        mainLine.SetPosition(0, startPosition);
        mainLine.SetPosition(1, midPoint);
        mainLine.SetPosition(2, endPosition);
        mainLine.enabled = true;

        // Update arrow heads
        float halfLength = headLength * 0.5f;
        Vector3 dir1 = (midPoint - startPosition).normalized;
        Vector3 dir2 = (endPosition - midPoint).normalized;

        // First arrow head
        Vector3 head1Pos = Vector3.Lerp(startPosition, midPoint, 0.3f);
        Vector3 head1Dir = (midPoint - head1Pos).normalized;
        SetupArrowHead(headLine1, head1Pos, head1Dir);

        // Second arrow head
        Vector3 head2Pos = Vector3.Lerp(midPoint, endPosition, 0.7f);
        Vector3 head2Dir = (endPosition - head2Pos).normalized;
        SetupArrowHead(headLine2, head2Pos, head2Dir);

        // Ensure visibility
        gameObject.SetActive(true);
        headObject1.SetActive(true);
        headObject2.SetActive(true);

        isVisible = true;
    }

    private void SetupArrowHead(LineRenderer headLine, Vector3 position, Vector3 direction) {
        Vector3 right = Quaternion.Euler(0, 0, 90) * direction;
        Vector3 tip = position + direction * headLength;
        Vector3 baseLeft = position + right * (headLength * Mathf.Tan(headAngle * Mathf.Deg2Rad));
        Vector3 baseRight = position - right * (headLength * Mathf.Tan(headAngle * Mathf.Deg2Rad));

        headLine.SetPosition(0, baseLeft);
        headLine.SetPosition(1, tip);
        headLine.enabled = true;
        headLine.material.color = currentColor;
    }

    public void Hide() {
        isVisible = false;

        if (mainLine != null) {
            mainLine.enabled = false;
        }
        if (headLine1 != null) {
            headLine1.enabled = false;
        }
        if (headLine2 != null) {
            headLine2.enabled = false;
        }

        gameObject.SetActive(false);
        if (headObject1 != null) headObject1.SetActive(false);
        if (headObject2 != null) headObject2.SetActive(false);

        Log("Arrow hidden", LogTag.UI);
    }

    public void UpdateEndPosition(Vector3 newEnd) {
        if (!isVisible) return;

        endPosition = newEnd;
        endPosition.z = 0;

        if (mainLine != null) {
            mainLine.SetPosition(1, endPosition);
            UpdateArrowHead();
        }
    }

    public bool IsVisible() {
        return isVisible;
    }

    private void OnDestroy() {
        if (mainLine != null && mainLine.material != null) {
            Destroy(mainLine.material);
        }
        if (headLine1 != null && headLine1.material != null) {
            Destroy(headLine1.material);
        }
        if (headLine2 != null && headLine2.material != null) {
            Destroy(headLine2.material);
        }
        if (headObject1 != null) {
            Destroy(headObject1);
        }
        if (headObject2 != null) {
            Destroy(headObject2);
        }
    }
}