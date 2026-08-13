using UnityEngine;
using TMPro;

public class FloatingTextAnimator : MonoBehaviour
{
    public float FloatSpeed = 1.5f;
    public float Lifetime = 1.5f;
    
    private TextMeshPro textMeshPro;
    private Color startingColor;
    private float timer;
    private Camera mainCamera;

    void Start()
    {
        textMeshPro = GetComponent<TextMeshPro>();
        if (textMeshPro != null)
        {
            startingColor = textMeshPro.color;
        }
        
        mainCamera = Camera.main;
        timer = Lifetime;
        
        // Destroy this object automatically when the lifetime expires
        Destroy(gameObject, Lifetime);
    }

    void Update()
    {
        // 1. Move upwards
        transform.position += Vector3.up * FloatSpeed * Time.deltaTime;

        // 2. Face the camera perfectly (Billboarding)
        if (mainCamera != null)
        {
            // Matching the camera's rotation is better than LookAt, 
            // as it prevents the text from tilting weirdly at the edges of the screen.
            transform.rotation = mainCamera.transform.rotation;
        }

        // 3. Fade out over time
        if (textMeshPro != null)
        {
            timer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / (Lifetime * 0.5f)); // Start fading halfway through
            textMeshPro.color = new Color(startingColor.r, startingColor.g, startingColor.b, alpha);
        }
    }
}