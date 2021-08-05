using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeachBall : MonoBehaviour
{
    float bouncyPower = 2;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Vector3 direction = transform.position - collision.transform.position;
            transform.GetComponentInParent<Rigidbody>().AddForce(direction.normalized * bouncyPower, ForceMode.Impulse);
        }
    }
}
