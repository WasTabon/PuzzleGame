using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPos;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.1f;

    private void OnEnable()
    {
        originalPos = transform.localPosition;
    }

    private void Update()
    {
        if (shakeDuration > 0)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeMagnitude;

            shakeDuration -= Time.deltaTime;
            if (shakeDuration <= 0f)
            {
                shakeDuration = 0f;
                transform.localPosition = originalPos;
            }
        }
    }

    public void Shake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}