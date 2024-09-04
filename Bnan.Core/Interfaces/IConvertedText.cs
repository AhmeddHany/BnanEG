using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Core.Interfaces
{
    public interface IConvertedText
    {
        string Get_NoText(string Code_Number, string LangType);
        string Get_Currency(string Code_Number, string LangType);
        (string, string) ConvertNumber(string Our_No, string LangType);
    }
}
