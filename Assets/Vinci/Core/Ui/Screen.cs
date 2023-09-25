using UnityEngine;

public abstract class Screen : MonoBehaviour
{
    public abstract void Initialize();

    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
}