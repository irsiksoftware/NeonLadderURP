using UnityEngine;

public class TokenGatheredManager : InteractableEntitiesManager
{
    public delegate void TokenGatheredHandler(GameObject token, int totalCount);
    public event TokenGatheredHandler OnTokenGathered;

    public override void InteractWithEntity(GameObject entity)
    {
        base.InteractWithEntity(entity);
        // Logic for when a token is gathered
        OnTokenGathered?.Invoke(entity, interactableEntities.Count);
    }
}
