using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldCollision : MonoBehaviour
{
    [SerializeField] string[] _collisionTag;
    Material mat;

    void Start()
    {
        if (GetComponent<Renderer>())
        {
            mat = GetComponent<Renderer>().sharedMaterial;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < _collisionTag.Length; i++)
        {
            if (other.gameObject.tag.Equals(_collisionTag[i]))
            {
                GetComponentInParent<SkillSystem>().isShieldActive = false;
            }
        }
    }
}

