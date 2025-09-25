namespace Combat
{
    // Generic damageable contract any target can implement
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }
}
