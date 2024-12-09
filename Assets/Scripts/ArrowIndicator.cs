using UnityEngine;

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
    private float headAngle = 30f;

    private void Awake() {
        SetupArrow();
    }

    private void SetupArrow() {
        // Setup main line
        mainLine = gameObject.AddComponent<LineRenderer>();
        mainLine.positionCount = 2;
        mainLine.startWidth = arrowWidth;
        mainLine.endWidth = arrowWidth;

        // Create separate GameObjects for the arrowheads to avoid component conflicts
        headObject1 = new GameObject("ArrowHead1");
        headObject2 = new GameObject("ArrowHead2");
        headObject1.transform.SetParent(transform);
        headObject2.transform.SetParent(transform);

        headLine1 = headObject1.AddComponent<LineRenderer>();
        headLine2 = headObject2.AddComponent<LineRenderer>();

        SetupHeadLine(headLine1);
        SetupHeadLine(headLine2);

        // Set material and color
        Color arrowColor = new Color(1f, 0f, 0f, 0.9f); // Red with 0.9 alpha
        SetupLineMaterial(mainLine, arrowColor);
        SetupLineMaterial(headLine1, arrowColor);
        SetupLineMaterial(headLine2, arrowColor);

        // Set sorting order to appear above cards
        mainLine.sortingOrder = 100;
        headLine1.sortingOrder = 100;
        headLine2.sortingOrder = 100;

        Hide();
    }

    private void SetupHeadLine(LineRenderer line) {
        line.positionCount = 2;
        line.startWidth = arrowWidth;
        line.endWidth = 0; // Make the head pointy
    }

    private void SetupLineMaterial(LineRenderer line, Color color) {
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = color;
        line.material = material;
    }

    public static ArrowIndicator Create(Transform parent = null) {
        GameObject arrowObj = new GameObject("AttackArrow");
        if (parent != null) {
            arrowObj.transform.SetParent(parent, false);
            arrowObj.transform.localPosition = Vector3.zero;
        }
        return arrowObj.AddComponent<ArrowIndicator>();
    }

    public void Show(Vector3 start, Vector3 end) {
        startPosition = start;
        endPosition = end;

        // Update main line
        if (mainLine != null) {
            mainLine.SetPosition(0, start);
            mainLine.SetPosition(1, end);
            mainLine.enabled = true;
        }

        UpdateArrowHead();
        isVisible = true;
    }

    private void UpdateArrowHead() {
        if (headLine1 == null || headLine2 == null) return;

        Vector3 direction = (endPosition - startPosition).normalized;
        Vector3 right = Quaternion.Euler(0, 0, 90) * direction;

        Vector3 arrowTip = endPosition;
        Vector3 arrowBase = arrowTip - direction * headLength;

        // Calculate the two points of the arrowhead
        Vector3 headPoint1 = arrowBase + right * headLength * Mathf.Tan(headAngle * Mathf.Deg2Rad);
        Vector3 headPoint2 = arrowBase - right * headLength * Mathf.Tan(headAngle * Mathf.Deg2Rad);

        // Set the positions for the arrowhead lines
        headLine1.SetPosition(0, arrowTip);
        headLine1.SetPosition(1, headPoint1);
        headLine1.enabled = true;

        headLine2.SetPosition(0, arrowTip);
        headLine2.SetPosition(1, headPoint2);
        headLine2.enabled = true;
    }

    public void Hide() {
        if (mainLine != null) mainLine.enabled = false;
        if (headLine1 != null) headLine1.enabled = false;
        if (headLine2 != null) headLine2.enabled = false;
        isVisible = false;
    }

    public void UpdateEndPosition(Vector3 newEnd) {
        if (isVisible) {
            endPosition = newEnd;
            if (mainLine != null) {
                mainLine.SetPosition(1, newEnd);
            }
            UpdateArrowHead();
        }
    }

    public bool IsVisible() {
        return isVisible;
    }

    private void OnDestroy() {
        if (headObject1 != null) Destroy(headObject1);
        if (headObject2 != null) Destroy(headObject2);

        if (mainLine != null && mainLine.material != null) Destroy(mainLine.material);
        if (headLine1 != null && headLine1.material != null) Destroy(headLine1.material);
        if (headLine2 != null && headLine2.material != null) Destroy(headLine2.material);
    }
}