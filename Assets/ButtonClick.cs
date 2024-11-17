using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour {
    private Button button;

    void Start() {
        // Get the Button component
        button = GetComponent<Button>();

        // Add a click listener
        button.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick() {
        Debug.Log("Button clicked!");
        // Add your button functionality here
    }

    void OnDestroy() {
        // Clean up the listener when the script is destroyed
        button.onClick.RemoveListener(OnButtonClick);
    }
}