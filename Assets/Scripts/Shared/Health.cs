using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] protected float currentHealth;
    public float GetHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;

    public event System.Action<float> Damaged;
    public event System.Action Died;

    public virtual void TakeDamage(float amount)
    {
        if (!IsAlive || amount <= 0f)
            return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);

        Damaged?.Invoke(amount);

        if (currentHealth <= 0f)
            Died?.Invoke();
    }
}