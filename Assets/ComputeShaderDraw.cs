using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ComputeShaderDraw : MonoBehaviour
{
    //Basics
    [SerializeField] ComputeShader shader;
    [SerializeField] Vector2Int resolution = new Vector2Int(1024, 1024);
    [SerializeField] FilterMode filterMode;
    [SerializeField] string[] setMaterialSlotsTo;
    [SerializeField] bool writeTextureSlots = true;
    RenderTexture rTexture;
    int resultID;
    Material material;

    //Additional
    ComputeBuffer boids;
    int deltaTimeID;
    int timeID;
    float time = 0;

    [SerializeField] int boidCount = 128;
    [SerializeField] float boidSpeed = 20.0f;
    [SerializeField] float boidTurnSpeed = 3.0f;
    [SerializeField] float boidLookAngle = 1.5f;

    [SerializeField] float boidSearchDistance = 2.0f;

    [SerializeField] float dissolveMult = .5f;
    [SerializeField] float blurMult = 1.0f;
    [SerializeField] float boidVelInDiv = 2.0f;
    [SerializeField] float boidCircularVelMult = 50.0f;
    float delay = 2.0f;
    bool active = false;
    float timeMult = 1f;

    bool updateBoids = true;

    struct Boid
    {
        public Vector2 pos;
        public Vector2 vel;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Basics
        // Get shader result ID
        resultID = Shader.PropertyToID("_Result");
        // Make texture
        rTexture = new RenderTexture(resolution.x, resolution.y, 0, RenderTextureFormat.ARGBFloat);
        // Enable RW
        rTexture.enableRandomWrite = true;
        // Set FilterMode        

        rTexture.Create();
        rTexture.wrapModeU = TextureWrapMode.Repeat;
        rTexture.wrapModeV = TextureWrapMode.Repeat;
        rTexture.filterMode = filterMode;


        // Set texture in shader
        shader.SetTexture(0, resultID, rTexture);
        shader.SetTexture(1, resultID, rTexture);

        // Set variables
        shader.SetInt("width", resolution.x);
        shader.SetInt("height", resolution.y);

        // Get material
        material = GetComponent<MeshRenderer>().sharedMaterial;
        // Set texture in material
        //material.SetTexture("Texture2D_766326ec42894792b0368e66938af36b", rTexture);
        for (int i = 0; i < setMaterialSlotsTo.Length; i++)
        {
            material.SetTexture(setMaterialSlotsTo[i], rTexture);
        }

        if (writeTextureSlots)
        {
            string[] s = material.GetTexturePropertyNames();
            for (int i = 0; i < s.Length; i++)
            {
                Debug.Log(s[i]);
            }
        }

        //Extra
        deltaTimeID = Shader.PropertyToID("deltaTime");
        timeID = Shader.PropertyToID("time");
        shader.SetFloat("dissolveMult", dissolveMult);
        shader.SetFloat("blurMult", blurMult);

        InitalizeBoids();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            updateBoids = !updateBoids;
        }

        if (Input.GetKey(KeyCode.Alpha1))
        {
            ChangeSettings(400, 150, 10, .7f, 1.2f, 50, .5f, 50);
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            ChangeSettings(400, 100, 20, .5f, 2.0f, 50, .5f, 50);
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            ChangeSettings(400, 30, 50, .7f, 3.0f, 0, 0.5f, 100);
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            ChangeSettings(200, 10, 20, 1.2f, 2.0f, 5, 0.25f, 100);
        }
        if (Input.GetKey(KeyCode.Alpha5))
        {
            ChangeSettings(600, 10, 50, 1.2f, 2.0f, 5, 0.5f, 100);
        }
        if (Input.GetKey(KeyCode.Alpha6))
        {
            ChangeSettings(800, 2000, 10, 1.75f, 3.0f, 500, 0.5f, 100);
        }
    }

    private void InitalizeBoids()
    {
        Random.InitState(0);
        List<Boid> boidList = new List<Boid>(boidCount);
        Vector2 center = new Vector2(resolution.x / 2, resolution.y / 2);
        for (int i = 0; i < boidCount; i++)
        {
            Vector2 pos = center + rotate(Vector2.up, Random.value * Mathf.PI * 2) * Random.value * resolution.x/2;

            boidList.Add(new Boid
            {
                //pos = new Vector2(Random.value * resolution.x, Random.value * resolution.y),
                //pos = center,
                pos = pos,
                //vel = -(center - pos).normalized * boidSpeed
                vel = (pos - center) - rotate(pos - center, .2f).normalized * boidSpeed
            }
            );
        }

        boids = new ComputeBuffer(boidList.Count, sizeof(float)*4);
        boids.SetData(boidList);

        shader.SetBuffer(1, "Boids", boids);

        shader.SetFloat("boidSpeed", boidSpeed);
        shader.SetFloat("boidTurnSpeed", boidTurnSpeed);
        shader.SetFloat("boidSearchDistance", boidSearchDistance);
        shader.SetFloat("boidLookAngle", boidLookAngle);
        shader.SetFloat("boidVelInDiv", boidVelInDiv);
        shader.SetFloat("boidCircularVelMult", boidCircularVelMult);

    }


    public void ChangeSettings(float boidSpeed, float boidTurnSpeed, float boidSearchDistance, float boidLookAngle, float boidVelInDiv, float boidCircularVelMult, float dissolveMult, float blurMult)
    {

        shader.SetFloat("boidSpeed", boidSpeed);
        shader.SetFloat("boidTurnSpeed", boidTurnSpeed);
        shader.SetFloat("boidSearchDistance", boidSearchDistance);
        shader.SetFloat("boidLookAngle", boidLookAngle);
        shader.SetFloat("boidVelInDiv", boidVelInDiv);
        shader.SetFloat("boidCircularVelMult", boidCircularVelMult);
        shader.SetFloat("dissolveMult", dissolveMult);
        shader.SetFloat("blurMult", blurMult);
    }

    public void SetSimVel(float t)
    {
        timeMult = t;
    }

    Vector2 rotate(Vector2 v, float delta)
    {
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }

    private void FixedUpdate()
    {
        if (!active)
        {
            delay -= Time.fixedDeltaTime;
            if (delay <= 0) active = true;
        }
        else
        {
            shader.SetFloat(deltaTimeID, Time.fixedDeltaTime*timeMult);
            shader.SetFloat(timeID, time += Time.fixedDeltaTime*timeMult);
            if (updateBoids) shader.Dispatch(1, boidCount / 64, 1, 1);
            shader.Dispatch(0, resolution.x / 8, resolution.y / 8, 1);
        }
    }

    private void OnDestroy()
    {
        boids.Dispose();
    }
}
