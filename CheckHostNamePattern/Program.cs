using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckHostNamePattern
{
    class Program
    {
        static void Main(string[] args)
        {
            NamePattern np = new NamePattern("Host001");            //  ホスト名が「Host001」の場合のみマッチ
            //NamePattern np = new NamePattern("Host001~005");      //  ホスト名が「Host001」～「Host005」の場合にマッチ
            //NamePattern np = new NamePattern("Host00%");          //  ホスト名が「Host00」で始まる名前の場合にマッチ
            //NamePattern np = new NamePattern("Host001a~010a");    //  ホスト名が「Host001a」～「Host010a」の場合にマッチ

            bool result = np.CheckName(Environment.MachineName);
            Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}
