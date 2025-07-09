using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPos;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.1f;

    public FollowCamera2D followCamera2D;
    
    private bool isShaking = false;

    private void Update()
    {
        if (shakeDuration > 0)
        {
            if (!isShaking)
            {
                isShaking = true;
                if (followCamera2D != null)
                    followCamera2D.enabled = false;
            }

            transform.localPosition = originalPos + Random.insideUnitSphere * shakeMagnitude;

            shakeDuration -= Time.deltaTime;
            if (shakeDuration <= 0f)
            {
                shakeDuration = 0f;
                transform.localPosition = originalPos;
                isShaking = false;
                if (followCamera2D != null)
                    StartCoroutine(EnableFollowCameraAfterFrames(5));
            }
        }
    }

    private IEnumerator EnableFollowCameraAfterFrames(int frameCount)
    {
        for (int i = 0; i < frameCount; i++)
            yield return null;

        if (followCamera2D != null)
            followCamera2D.enabled = true;
    }

    public void Shake(float duration, float magnitude)
    {
        // Сохраняем позицию перед началом тряски
        originalPos = transform.localPosition;

        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}