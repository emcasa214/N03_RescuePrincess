using UnityEngine;
using System.Collections;

public class RippleEffect : MonoBehaviour
{
    public AnimationCurve waveform = new AnimationCurve(
        new Keyframe(0.00f, 0.50f, 0, 0),
        new Keyframe(0.05f, 1.00f, 0, 0),
        new Keyframe(0.15f, 0.10f, 0, 0),
        new Keyframe(0.25f, 0.80f, 0, 0),
        new Keyframe(0.35f, 0.30f, 0, 0),
        new Keyframe(0.45f, 0.60f, 0, 0),
        new Keyframe(0.55f, 0.40f, 0, 0),
        new Keyframe(0.65f, 0.55f, 0, 0),
        new Keyframe(0.75f, 0.46f, 0, 0),
        new Keyframe(0.85f, 0.52f, 0, 0),
        new Keyframe(0.99f, 0.50f, 0, 0)
    );

    [Range(0.01f, 1.0f)]
    public float refractionStrength = 0.5f;

    public Color reflectionColor = Color.gray;

    [Range(0.01f, 1.0f)]
    public float reflectionStrength = 0.7f;

    [Range(0.01f, 5.0f)]
    public float waveSpeed = 1.25f;

    [Range(0.0f, 2.0f)]
    public float dropInterval = 0.5f;

    [SerializeField] private Shader shader;

    class Droplet
    {
        Vector2 position;
        float time;
        bool isActive;
        float delayTime = 0.1f;

        public Droplet()
        {
            time = 1000;
            isActive = false;
        }

        public void Reset(Vector2 pos)
        {
            position = pos;
            time = 0;
            isActive = true;
        }

        public void Update()
        {
            if (isActive)
            {
                time += Time.deltaTime*0.5f;
                if (time >= delayTime)
                {
                    isActive = false;
                }
            }
        }

        public Vector4 MakeShaderParameter(float aspect)
        {
            if (!isActive)
            {
                return new Vector4(0, 0, 1000, 0); // Vô hiệu hóa gợn sóng
            }
            return new Vector4(position.x * aspect, position.y, time, 0);
        }
    }

    Droplet droplet; // Chỉ sử dụng một Droplet
    Texture2D gradTexture;
    Material material;
    float timer;
    Camera cam;

    void UpdateShaderParameters()
    {
        if (cam == null) return;

        material.SetVector("_Drop1", droplet.MakeShaderParameter(cam.aspect));
        material.SetVector("_Drop2", new Vector4(0, 0, 1000, 0)); // Vô hiệu hóa
        material.SetVector("_Drop3", new Vector4(0, 0, 1000, 0)); // Vô hiệu hóa

        material.SetColor("_Reflection", reflectionColor);
        material.SetVector("_Params1", new Vector4(cam.aspect, 1, 1 / waveSpeed, 0));
        material.SetVector("_Params2", new Vector4(1, 1 / cam.aspect, refractionStrength, reflectionStrength));
    }

    void Awake()
    {
        if (shader == null)
        {
            Debug.LogError("Shader is not assigned in RippleEffect.", this);
            enabled = false;
            return;
        }

        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera component is missing in RippleEffect.", this);
            enabled = false;
            return;
        }

        droplet = new Droplet(); // Khởi tạo một Droplet duy nhất

        gradTexture = new Texture2D(2048, 1, TextureFormat.Alpha8, false);
        gradTexture.wrapMode = TextureWrapMode.Clamp;
        gradTexture.filterMode = FilterMode.Bilinear;
        for (var i = 0; i < gradTexture.width; i++)
        {
            var x = 1.0f / gradTexture.width * i;
            var a = waveform.Evaluate(x);
            gradTexture.SetPixel(i, 0, new Color(a, a, a, a));
        }
        gradTexture.Apply();

        material = new Material(shader);
        if (material == null)
        {
            Debug.LogError("Failed to create material from shader in RippleEffect.", this);
            enabled = false;
            return;
        }
        material.hideFlags = HideFlags.DontSave;
        material.SetTexture("_GradTex", gradTexture);

        UpdateShaderParameters();
    }

    void Update()
    {
        if (dropInterval > 0)
        {
            timer += Time.deltaTime;
            while (timer > dropInterval)
            {
                // Emit(Random.insideUnitCircle);
                timer -= dropInterval;
            }
        }

        droplet.Update(); // Cập nhật Droplet duy nhất

        UpdateShaderParameters();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material == null) return;
        Graphics.Blit(source, destination, material);
    }

    public void Emit(Vector2 pos)
    {
        droplet.Reset(pos); // Reset Droplet duy nhất
    }
}