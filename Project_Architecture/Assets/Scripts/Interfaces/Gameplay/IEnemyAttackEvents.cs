using System;

public interface IEnemyAttackEvents
{
    event Action MeleeAttackPerformed;
    event Action RangedAttackPerformed;
}
