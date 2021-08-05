using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeOffPad : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(HideObject());

            Physics.IgnoreLayerCollision(8, 9, true);
            PlayerController_RB.Instance.WorldTransfer();
        }
    }

    IEnumerator HideObject()
    {
        yield return new WaitForSeconds(0.5f);
        this.gameObject.SetActive(false);
    }
}
