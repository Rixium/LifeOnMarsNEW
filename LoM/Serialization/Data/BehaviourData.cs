namespace LoM.Serialization.Data
{
    public class BehaviourData
    {

        public string Name;
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