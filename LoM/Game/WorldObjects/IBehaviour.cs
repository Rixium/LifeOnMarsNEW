namespace LoM.Game.WorldObjects
{
    public interface IBehaviour
    {
        
        bool IsPassable();
        void Update(float deltaTime);
        IBehaviour Clone(IRenderer renderer);
        void SetOwner(WorldObject clonedCopy);

    }
}