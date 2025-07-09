using UnityEngine;

public class FollowCamera2D : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // Игрок

    [Header("Camera Offset")]
    public Vector3 offset = new Vector3(0f, 5f, -10f); // Смещение камеры

    [Header("Follow Settings")]
    [Range(0.01f, 1f)] public float smoothSpeed = 0.125f; // Скорость сглаживания

    void LateUpdate()
    {
        if (target == null) return;

        // Желаемая позиция камеры с учетом смещения
        Vector3 desiredPosition = target.position + offset;

        // Плавное движение камеры к желаемой позиции
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Установка новой позиции
        transform.position = smoothedPosition;
    }
}