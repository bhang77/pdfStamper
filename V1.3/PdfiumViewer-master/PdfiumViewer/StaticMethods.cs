using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfView
{
    class StaticMethods
    {
        static public byte[] CloneStream(System.IO.Stream stm)
        {
            byte[]  retstm = null;

            try
            {
                if(stm!=null)
                {
                    byte[] bsData = new byte[stm.Length];
                    stm.Read(bsData, 0, bsData.Length);
                    retstm = new byte[bsData.Length];
                    System.Buffer.BlockCopy(bsData, 0, retstm, 0, retstm.Length);

                }
            }
            catch 
            { }
            return retstm;
        }

        static public bool SaveFile(string FileName, System.IO.Stream stm)
        {
            bool bRetval = false;
            try
            {
                if(stm!=null)
                {
                    byte[] bsData = new byte[stm.Length];
                    stm.Read(bsData, 0, bsData.Length);
                    System.IO.File.WriteAllBytes(FileName,bsData);
                }
            }
            catch 
            { }
            return bRetval;
        }
    }
}
