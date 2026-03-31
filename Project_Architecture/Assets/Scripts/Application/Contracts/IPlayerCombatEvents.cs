using System;

public interface IPlayerCombatEvents
{
    event Action PhysicalAttackPerformed;
    event Action MagicAttackPerformed;
}
