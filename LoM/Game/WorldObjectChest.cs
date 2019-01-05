using System;
using System.Collections.Generic;
using LoM.Game.WorldObjects;
using LoM.Util;

namespace LoM.Game
{
    public class WorldObjectChest
    {
        // TODO: We can access this dictionary from anywhere, but we will likely want one place where we should access it from, so we know how data is manipulated. 
        public static Dictionary<string, WorldObject> WorldObjectPrototypes = new Dictionary<string, WorldObject>();

        public static void LoadPrototypes(ContentChest contentChest)
        {
            var wallRenderer = new StaticRenderer {ContentChest = contentChest};
            CreatePrototype("Wall", true, true, true, true, 0, null, wallRenderer);

            var doorBehaviour = new DoorBehaviour();
            var doorRenderer = new TransitionRenderer();
            var doorTextures = new[]
            {
                "Door",
                "Door_1",
                "Door_2",
                "Door_3",
                "Door_4",
                "Door_5",
                "Door_6"
            };

            doorRenderer.TransitionTextures = doorTextures;
            doorRenderer.MaxTransition = 6;
            doorRenderer.TransitionTime = 0.3f;
            doorRenderer.ContentChest = contentChest;

            CreatePrototype("Door", false, false, false, true, 1, doorBehaviour, doorRenderer);
        }

        private static void CreatePrototype(string name, bool hollowPlacement, bool mergeWithNeighbors, bool dragBuild,
            bool encloses,
            float movementCost, IBehaviour behaviour, IRenderer renderer)
        {
            if (WorldObjectPrototypes.ContainsKey(name))
            {
                Console.WriteLine($"Prototype with name {name} has already been created.");
                return;
            }

            var worldObject =
                WorldObject.CreatePrototype(name, hollowPlacement, mergeWithNeighbors, dragBuild, encloses,
                    movementCost, behaviour, renderer);

            WorldObjectPrototypes.Add(name, worldObject);
        }
    }
}