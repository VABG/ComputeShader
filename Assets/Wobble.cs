using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wobble : MonoBehaviour
{
    [SerializeField] float wobbleRL = .5f;
    [SerializeField] float wobbleRLFreq = .5f;
    float time = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(time*wobbleRLFreq) * wobbleRL);
    }
}
