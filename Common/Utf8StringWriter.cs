using System.IO;
using System.Text;

namespace Fiscalapi.Credentials.Common
{
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
