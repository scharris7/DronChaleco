// Controles para que el dron virtual se mueva con teclas V2
// Sacado de los controles del mental hay que ponerle potenciometros para velocidad variable
// Se queda estatico en el aire porque no tiene gravedad
// movimiento realista pero sube y baja muy toscamente
using System.Collections;
using UnityEngine;

public class Control : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float aceleracionMaxima = 10f;       // Aceleración máxima en m/s²
    [SerializeField] private float velocidadMaximaHorizontal = 15f; // Velocidad máxima horizontal en m/s
    [SerializeField] private float velocidadMaximaVertical = 10f;   // Velocidad máxima vertical en m/s
    [SerializeField] private float velocidadRotacion = 90f;       // Grados por segundo
    [SerializeField] private float suavizadoRotacion = 5f;

    [Header("Límites de Movimiento")]
    [SerializeField] private float alturaMaxima = 1000f;
    [SerializeField] private float alturaMinima = 0.5f;

    [Header("Física del Dron")]
    [SerializeField] private float masa = 1.5f;                   // Masa del dron en kg
    [SerializeField] private float fuerzaGravedad = 9.81f;        // Gravedad
    [SerializeField] private float coeficienteResistenciaAire = 0.5f; // Coeficiente de resistencia al aire

    private Rigidbody rb;

    // Entradas
    private float inputHorizontal;
    private float inputVertical;
    private float inputProfundidad;
    private float inputRotacion;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("¡ERROR! El dron necesita un Rigidbody para funcionar. Añade uno manualmente.");
            return;
        }

        rb.mass = masa;
        rb.useGravity = false; // Controlamos la gravedad manualmente para mayor precisión
        rb.linearDamping = 0f;          // Quitamos drag para controlar resistencia manualmente
        rb.angularDamping = 5f;

        Debug.Log("Controlador de dron inicializado correctamente");
    }

    void Update()
    {
        CapturarEntradas();
        MostrarDebugInfo();
    }

    void FixedUpdate()
    {
        AplicarFuerzas();
        AplicarRotacion();
        //LimitarAltura();
    }

    void CapturarEntradas()
    {
        inputHorizontal = Input.GetAxis("Horizontal");     // A/D - Izquierda/Derecha
        inputProfundidad = Input.GetAxis("Vertical");      // W/S - Adelante/Atrás

        inputVertical = 0f;
        if (Input.GetKey(KeyCode.Space)) inputVertical += 2f;      // Subir
        if (Input.GetKey(KeyCode.LeftControl)) inputVertical -= 2f;  // Bajar

        inputRotacion = 0f;
        if (Input.GetKey(KeyCode.Q)) inputRotacion -= 50f;  // Rotar izquierda
        if (Input.GetKey(KeyCode.E)) inputRotacion += 50f;  // Rotar derecha
    }

    void AplicarFuerzas()
    {
        // Gravedad
        Vector3 fuerzaGravedadVector = Vector3.down * fuerzaGravedad * masa;

        // Resistencia del aire proporcional a la velocidad (drag cuadrático simplificado)
        Vector3 resistenciaAire = -rb.linearVelocity * coeficienteResistenciaAire;

        // Dirección de movimiento deseada en local space
        Vector3 inputMovimientoLocal = new Vector3(inputHorizontal, 0f, inputProfundidad).normalized;

        // Convertir a world space
        Vector3 direccionMovimiento = transform.TransformDirection(inputMovimientoLocal);

        // Calcular fuerza de aceleración horizontal
        Vector3 velocidadHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        Vector3 fuerzaAceleracionHorizontal = Vector3.zero;

        if (inputMovimientoLocal.magnitude > 0.1f)
        {
            // Queremos acelerar hacia la dirección deseada
            Vector3 velocidadObjetivo = direccionMovimiento * velocidadMaximaHorizontal;
            Vector3 deltaVelocidad = velocidadObjetivo - velocidadHorizontal;

            // Limitamos la aceleración máxima
            Vector3 aceleracion = Vector3.ClampMagnitude(deltaVelocidad / Time.fixedDeltaTime, aceleracionMaxima);

            fuerzaAceleracionHorizontal = aceleracion * masa;
        }
        else
        {
            // Sin input, desacelerar suavemente (fuerza contraria a la velocidad)
            Vector3 desaceleracion = -velocidadHorizontal / Time.fixedDeltaTime;
            desaceleracion = Vector3.ClampMagnitude(desaceleracion, aceleracionMaxima);
            fuerzaAceleracionHorizontal = desaceleracion * masa;
        }

        // Control vertical: fuerza para subir o bajar
        float fuerzaVertical = 0f;

        if (inputVertical > 0)
        {
            // Subir: aplicar fuerza hacia arriba, compensando gravedad y acelerando
            fuerzaVertical = masa * (fuerzaGravedad + velocidadMaximaVertical); // Fuerza extra para subir rápido
        }
        else if (inputVertical < 0)
        {
            // Bajar: reducir fuerza de sustentación para caer
            fuerzaVertical = masa * (fuerzaGravedad - velocidadMaximaVertical);
        }
        else
        {
            // Hover: fuerza igual a gravedad para mantenerse en el aire
            fuerzaVertical = masa * fuerzaGravedad;
        }

        Vector3 fuerzaVerticalVector = Vector3.up * fuerzaVertical;

        // Suma total de fuerzas
        Vector3 fuerzaTotal = fuerzaGravedadVector + resistenciaAire + fuerzaAceleracionHorizontal + fuerzaVerticalVector;

        // Aplicar fuerza al Rigidbody
        rb.AddForce(fuerzaTotal);
    }

    void AplicarRotacion()
    {
        // Rotación suave en Y (yaw)
        float rotacionDeseada = inputRotacion * velocidadRotacion;
        float rotacionActual = Mathf.LerpAngle(rb.rotation.eulerAngles.y, rb.rotation.eulerAngles.y + rotacionDeseada * Time.fixedDeltaTime, suavizadoRotacion * Time.fixedDeltaTime);

        Quaternion rotacionObjetivo = Quaternion.Euler(0, rotacionActual, 0);
        rb.MoveRotation(rotacionObjetivo);

        // Inclinación sutil para realismo (pitch y roll)
        float inclinacionX = inputProfundidad * 15f; // Adelante/atrás
        float inclinacionZ = -inputHorizontal * 15f; // Izquierda/derecha

        Quaternion inclinacionObjetivo = Quaternion.Euler(inclinacionX, rb.rotation.eulerAngles.y, inclinacionZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, inclinacionObjetivo, suavizadoRotacion * Time.fixedDeltaTime);
    }

    void LimitarAltura()
    {
        Vector3 posicion = transform.position;

        if (posicion.y > alturaMaxima)
        {
            posicion.y = alturaMaxima;
            transform.position = posicion;

            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            }
        }

        if (posicion.y < alturaMinima)
        {
            posicion.y = alturaMinima;
            transform.position = posicion;

            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            }
        }
    }

    void MostrarDebugInfo()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"Posición del Dron: {transform.position}");
            Debug.Log($"Velocidad: {rb.linearVelocity.magnitude:F2} m/s");
            Debug.Log($"Altura: {transform.position.y:F2} m");
        }
    }

    public void Aterrizar()
    {
        StartCoroutine(AterrizajeAutomatico());
    }

    private IEnumerator AterrizajeAutomatico()
    {
        while (transform.position.y > alturaMinima + 0.1f)
        {
            Vector3 velocidadAterrizaje = Vector3.down * (velocidadMaximaVertical * 0.5f);
            rb.linearVelocity = new Vector3(0, velocidadAterrizaje.y, 0);
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector3.zero;
        Debug.Log("Dron aterrizado exitosamente");
    }

    public void ActivarHover()
    {
        rb.linearVelocity = Vector3.zero;
        Debug.Log("Modo Hover activado");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(5, alturaMaxima - alturaMinima, 5));
    }
}
