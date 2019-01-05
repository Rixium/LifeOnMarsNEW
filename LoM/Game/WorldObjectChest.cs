using System;
using System.Collections.Generic;
using LoM.Game.WorldObjects;

namespace LoM.Game
{
    public class WorldObjectChest
    {

        // TODO: We can access this dictionary from anywhere, but we will likely want one place where we should access it from, so we know how data is manipulated. 
        public static Dictionary<string, WorldObject> WorldObjectPrototypes = new Dictionary<string, WorldObject>();

        public static void LoadPrototypes()
        {
            CreatePrototype("Wall", true, true, true, true, 0, null, new StaticRenderer());
            CreatePrototype("Door", false, false, false, true, 1, new DoorBehaviour(), new DoorRenderer());
        }

        private static void CreatePrototype(string name, bool hollowPlacement, bool mergeWithNeighbors, bool dragBuild, bool encloses,
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