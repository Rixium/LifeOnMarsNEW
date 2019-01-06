namespace LoM.Serialization.Data
{
    public class WorldObjectData
    {

        public string Name;
        public bool HollowPlacement;
        public bool MergeWithNeighbors;
        public bool DragBuild;
        public bool Encloses;
        public bool CanRotate;
        public float MovementCost;
        public RendererData Renderer;
        public BehaviourData Behaviour;

    }
}