namespace LoM.Game.Components
{
    public interface IComponent
    {

        Character Character { get; set; }

        void Update(float deltaTime);
        
    }
}