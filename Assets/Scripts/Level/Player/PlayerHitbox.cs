using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    float knockBackPower;
    bool isHit;

    private void Start()
    {
        knockBackPower = Random.Range(3.5f, 7f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle") && !isHit)
        {
            isHit = true;
            AudioManager.SharedInstance.Stop("Engine2");
            AudioManager.SharedInstance.Play("Explosion3");
            Vector3 direction = transform.position - other.transform.position;
            transform.GetComponentInParent<Rigidbody>().AddForce(direction.normalized * knockBackPower, ForceMode.Impulse);
            StartCoroutine(transform.parent.GetComponent<PlayerController_RB>().DestroyPlayer());
        }
    }
}
