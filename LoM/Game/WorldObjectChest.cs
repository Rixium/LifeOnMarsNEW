using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using LoM.Game.WorldObjects;
using LoM.Serialization.Data;
using LoM.Util;
using Newtonsoft.Json;

namespace LoM.Game
{
    public class WorldObjectChest
    {

        public static Dictionary<string, WorldObject> WorldObjectPrototypes = new Dictionary<string, WorldObject>();

        public static void LoadPrototypes(ContentChest contentChest)
        {
            var prototypeDirectory = "Content\\Prototypes";

            var prototypes = LoadFromFiles(prototypeDirectory);

            foreach (var prototype in prototypes)
            {
                var renderer = CreateRenderer(prototype.Renderer, contentChest);
                var behaviour = CreateBehaviour(prototype.Behaviour, contentChest);
                CreatePrototype(
                    prototype,
                    behaviour, 
                    renderer);
            }
        }

        private static IBehaviour CreateBehaviour(BehaviourData behaviour, ContentChest contentChest)
        {
            if (behaviour == null)
                return null;

            switch (behaviour.Name)
            {
                case "DoorBehaviour":
                    return new DoorBehaviour
                    {
                        OpeningTime = float.Parse(behaviour.GetParameter("OpeningTime"))
                    };
                default:
                    return null;
            }
        }

        private static IRenderer CreateRenderer(RendererData renderer, ContentChest contentChest)
        {
            if (renderer == null)
                return null;

            switch (renderer.Name)
            {
                case "StaticRenderer":
                    return new StaticRenderer()
                    {
                        ContentChest = contentChest
                    };
                case "TransitionRenderer":
                    return new TransitionRenderer
                    {
                        ContentChest = contentChest,
                        TransitionTextures = renderer.Textures,
                        MaxTransition = renderer.Textures.Length - 1,
                        TransitionTime = float.Parse(renderer.GetParameter("TransitionTime"))
                    };
                default: return null;
            }
        }

        private static IEnumerable<WorldObjectData> LoadFromFiles(string directory)
        {
            if (Directory.Exists(directory) == false) return null;
            var worldObjectData = new List<WorldObjectData>();
            foreach (var file in Directory.GetFiles(directory))
            {
                var lines = File.ReadAllText(file);
                var worldObject = JsonConvert.DeserializeObject<WorldObjectData>(lines);
                worldObjectData.Add(worldObject);
            }
            return worldObjectData.ToArray();
        }

        private static void CreatePrototype(WorldObjectData prototype, IBehaviour behaviour, IRenderer renderer)
        {
            var name = prototype.Name;
            var hollowPlacement = prototype.HollowPlacement;
            var mergeWithNeighbors = prototype.MergeWithNeighbors;
            var dragBuild = prototype.DragBuild;
            var encloses = prototype.Encloses;
            var movementCost = prototype.MovementCost;
            var canRotate = prototype.CanRotate;

            if (WorldObjectPrototypes.ContainsKey(name))
            {
                Console.WriteLine($"Prototype with name {name} has already been created.");
                return;
            }

            var worldObject =
                WorldObject.CreatePrototype(name, hollowPlacement, mergeWithNeighbors, dragBuild, encloses,
                    movementCost, canRotate, behaviour, renderer);

            WorldObjectPrototypes.Add(name, worldObject);
        }

    }
}