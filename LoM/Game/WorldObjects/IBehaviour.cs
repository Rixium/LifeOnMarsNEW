namespace LoM.Game.WorldObjects
{
    public interface IBehaviour
    {

        bool IsPassable();
        void Update(float deltaTime);
        IBehaviour Clone();
        void SetOwner(WorldObject clonedCopy);

    }
}