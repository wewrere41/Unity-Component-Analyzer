using UnityEngine;

public class ExampleA : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private MeshRenderer _meshRenderer;
    private BoxCollider _boxCollider;

    private void Awake()
    {
        _rigidbody.velocity = Vector3.zero;
        _meshRenderer.enabled = false;
        _boxCollider.size = Vector3.zero;
    }
}