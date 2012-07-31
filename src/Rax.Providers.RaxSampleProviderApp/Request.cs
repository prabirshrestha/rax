namespace Rax.Providers.RaxSampleProviderApp
{
    public class Request : DynamicDictionary
    {
        public string Method
        {
            get { return (string)this["Method"]; }
            set { this["Method"] = value; }
        }

        public string Path
        {
            get { return (string)this["Path"]; }
            set { this["Path"] = value; }
        }
    }
}