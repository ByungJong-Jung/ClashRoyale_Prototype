using UnityEngine;

public interface IEntityHealthBar
{
    public void SetActiveHealthBar(bool inActivity);

    public void ProcessHealthBar(HealthComponent inHealthComponent);

    public void ProcessHearBarRotation();
}
