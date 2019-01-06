namespace LoM.Serialization.Data
{

    public class RendererData
    {
        public string Name;
        public string[] Textures;
        
        public Parameters[] Parameters;

        public string GetParameter(string name)
        {
            foreach (var param in Parameters)
            {
                if (param.Name != name) continue;

                return param.Value;
            }

            return "";
        }

    }

}