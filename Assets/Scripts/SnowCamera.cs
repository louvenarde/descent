using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SnowCamera : MonoBehaviour {
    public GameObject follow;
    public MeshRenderer previousPlane;
    public Material prevMaterial;

    public RenderTexture prevTarget = null;
    public RenderTexture target;
    public Camera cam;

	// Use this for initialization
	void Start ()
    {
        prevMaterial = new Material(previousPlane.sharedMaterial);
        previousPlane.sharedMaterial = prevMaterial;

        cam = GetComponent<Camera>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        previousPlane.transform.position = transform.position - new Vector3(5.0f / 256.0f, 30, 5.0f / 256.0f);

        //transform.position = follow.transform.position + new Vector3(0, 20, 0);
        transform.position = new Vector3(
            Mathf.Round(follow.transform.position.x),
            Mathf.Round(follow.transform.position.y) + 20,
            Mathf.Round(follow.transform.position.z)
        );

        Shader.SetGlobalMatrix("_SnowCameraMatrix", this.transform.worldToLocalMatrix);

        if (prevTarget != null) RenderTexture.ReleaseTemporary(prevTarget);
        prevTarget = target;
        if(prevTarget) prevTarget.filterMode = FilterMode.Point;
        prevMaterial.SetTexture("_MainTex", prevTarget);

        target = RenderTexture.GetTemporary(256, 256, 24, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
        target.filterMode = FilterMode.Bilinear;
        cam.targetTexture = target;

        Shader.SetGlobalTexture("_SnowRT", target);
    }
}
