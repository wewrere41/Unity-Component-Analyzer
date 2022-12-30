using UnityEngine;

public class ExampleB : MonoBehaviour
{
    public Rigidbody _rigidbody;
    [SerializeField] private MeshRenderer _meshRenderer;
    private BoxCollider _boxCollider;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();

        _rigidbody.velocity = Vector3.zero;
        _meshRenderer.enabled = false;
        _boxCollider.size = Vector3.zero;
    }
}