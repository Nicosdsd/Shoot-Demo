using UnityEngine;

[ExecuteInEditMode]
public class FootSteps : MonoBehaviour
{
    public ParticleSystem system;
    public float delta = 1;
    public float gap = 0.5f;


    private Vector3 lastEmit;
    private int dir = 1;

    private void Start()
    {
        lastEmit = transform.position;
     
    }

    private void Update()
    {

        if (Vector3.Distance(lastEmit, transform.position) > delta)
        {
            var pos = transform.position + (transform.right * gap * dir);
            dir *= -1;
            
            ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
            ep.position = pos;
            ep.rotation = transform.rotation.eulerAngles.y;
            //ep.startLifetime = footprintLifetime; // 设置粒子生命周期
            AudioManager.Instance.PlaySound("脚印",transform.position);
            system.Emit(ep, 1);
            lastEmit = transform.position;
        }
    }
}