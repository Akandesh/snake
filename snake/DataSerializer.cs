using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace snake
{
    class DataSerializer
    {
        public void BinarySerialize( object data, string filePath ) {
            FileStream fileStream = File.Create( filePath );
            BinaryFormatter bf = new( );
#pragma warning disable SYSLIB0011
            bf.Serialize( fileStream, data );
#pragma warning restore SYSLIB0011
            fileStream.Close( );
        }

        public object Deserialize( string filePath ) {
            object obj = null;
            if ( !File.Exists( filePath ) )
                return obj;

            FileStream fileStream = File.OpenRead( filePath );
            BinaryFormatter bf = new( );

#pragma warning disable SYSLIB0011
            obj = bf.Deserialize( fileStream );
#pragma warning restore SYSLIB0011
            fileStream.Close();

            return obj;
        }
    }
}
