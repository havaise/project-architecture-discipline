using System.Collections.Generic;

public interface IEnemyStateRepository
{
    List<EnemySaveData> Capture();
    void Restore(List<EnemySaveData> enemiesData);
}
