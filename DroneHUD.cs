// Codigo para el HUD del dron Funciona bien
// El HUD se ve muy plastilina, mejorar eso desde los canvas de Unity
using UnityEngine;
using TMPro;

public class DroneHUD : MonoBehaviour
{
    [Header("Referencias del dron")]
    [SerializeField] private Transform dronTransform;
    [SerializeField] private Rigidbody dronRigidbody;
    [SerializeField] private Camera dronCamera; // asigna la cámara del dron al Canvas o aquí

    [Header("UI (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI alturaText;
    [SerializeField] private TextMeshProUGUI velocidadText;
    [SerializeField] private TextMeshProUGUI inclinacionText;

    [Header("Horizon")]
    [SerializeField] private RectTransform horizonRect; // la Image del horizonte
    [SerializeField] private float pitchToYpixels = 3f; // cuánto mueve el horizonte por grado de pitch
    [SerializeField] private float maxPitchOffset = 100f; // límite en píxeles
    [SerializeField] private float smoothSpeed = 8f; // suavizado UI

    void Awake()
    {
        // fallback sencillo si no asignaste desde el inspector
        if (dronCamera == null) dronCamera = Camera.main;
        if (dronRigidbody == null && dronTransform != null) dronRigidbody = dronTransform.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        if (dronTransform == null) return;

        // --- ALTURA ---
        float altura = dronTransform.position.y;
        alturaText.text = $"Altitud: {altura:F1} m";

        // --- VELOCIDAD ---
        float velocidad = (dronRigidbody != null) ? dronRigidbody.linearVelocity.magnitude : 0f;
        velocidadText.text = $"Rapidez: {velocidad:F1} (m/s)";

        // --- INCLINACIÓN: Pitch (X), Roll (Z), Yaw (Y) ---
        Vector3 euler = dronTransform.eulerAngles;
        float pitch = NormalizeAngle(euler.x);
        float roll = NormalizeAngle(euler.z);
        float yaw = NormalizeAngle(euler.y);
        inclinacionText.text = $"Pitch: {pitch:F1}°  |  Roll: {roll:F1}°  |  Yaw: {yaw:F1}°";

        // --- HORIZONTE ARTIFICIAL ---
        if (horizonRect != null)
        {
            // rotación: el horizonte rota inverso al roll del dron
            float targetAngle = -roll;
            Vector3 currentEuler = horizonRect.localEulerAngles;
            // evitar salto 0-360 al interpolar
            float currentZ = NormalizeAngle(currentEuler.z);
            float z = Mathf.Lerp(currentZ, targetAngle, Time.deltaTime * smoothSpeed);
            horizonRect.localRotation = Quaternion.Euler(0f, 0f, z);

            // desplazamiento vertical según pitch (más pitch → subir/bajar la línea)
            float targetYOffset = Mathf.Clamp(-pitch * pitchToYpixels, -maxPitchOffset, maxPitchOffset);
            Vector2 anchored = horizonRect.anchoredPosition;
            anchored.y = Mathf.Lerp(anchored.y, targetYOffset, Time.deltaTime * smoothSpeed);
            horizonRect.anchoredPosition = anchored;
        }
    }

    // Convierte ángulos 0..360 a -180..180
    private float NormalizeAngle(float a)
    {
        if (a > 180f) a -= 360f;
        return a;
    }
}
