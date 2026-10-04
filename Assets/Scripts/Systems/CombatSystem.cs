using UnityEngine;

public class CombatSystem
{
    [SerializeField] private int minDamage = 1;
    [SerializeField] private int maxDamage = 10;

    // Por ahora a lo sencillo: Ataque de daño aleatorio.
    public void PerformAttack(GameObject obj) =>
        ApplyDamage(obj, Utils.GetRandom(minDamage, maxDamage + 1));

    // Aplicar el daño.
    private void ApplyDamage(GameObject obj, int damage = 0) =>
        obj.GetComponent<HealthComponent>()?.TakeDamage(damage);
}