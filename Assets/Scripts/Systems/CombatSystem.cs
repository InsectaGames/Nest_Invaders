using UnityEngine;

public class CombatSystem
{
    [SerializeField] private static int minDamage = 1;
    [SerializeField] private static int maxDamage = 10;

    // Por ahora a lo sencillo: Ataque de daño aleatorio.
    public static void PerformAttack(GameObject obj) =>
        ApplyDamage(obj, Utils.GetRandom(minDamage, maxDamage + 1));

    // Aplicar el daño.
    private static void ApplyDamage(GameObject obj, int damage = 0) =>
        obj.GetComponent<HealthComponent>()?.TakeDamage(damage);
}