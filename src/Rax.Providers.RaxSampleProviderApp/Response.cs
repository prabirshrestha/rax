namespace Rax.Providers.RaxSampleProviderApp
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class Response : DynamicDictionary
    {
        public IDictionary<string, string[]> Headers
        {
            get { return (IDictionary<string, string[]>)this["Headers"]; }
            set { this["Headers"] = value; }
        }

        public void SetHeader(string key, params string[] value)
        {
            dynamic setHeader = this["SetHeader"];
            setHeader(key, value);
        }

        public Stream Stream
        {
            get { return (Stream)this["Stream"]; }
            set { this["Stream"] = value; }
        }

        public void Write(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            Stream.Write(bytes, 0, bytes.Length);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            Stream.Write(buffer, offset, count);
        }

        public void End()
        {
            dynamic end = this["End"];
            end();
        }
    }
}