using System;
using System.Collections.Generic;

namespace LoM.Game
{
    public class WorldObjectChest
    {

        // TODO: We can access this dictionary from anywhere, but we will likely want one place where we should access it from, so we know how data is manipulated. 
        public static Dictionary<string, WorldObject> WorldObjectPrototypes = new Dictionary<string, WorldObject>();

        public static void LoadPrototypes()
        {
            CreatePrototype("Wall", true, true, 0);
            CreatePrototype("Door", false, false, 1);
        }

        private static void CreatePrototype(string name, bool hollowPlacement, bool mergeWithNeighbors,
            float movementCost)
        {
            if (WorldObjectPrototypes.ContainsKey(name))
            {
                Console.WriteLine($"Prototype with name {name} has already been created.");
                return;
            }

            WorldObjectPrototypes.Add(name,
                WorldObject.CreatePrototype(name, hollowPlacement, mergeWithNeighbors, movementCost));
        }

    }
}