using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace laba1_proga
{
    class Page
    {
        const int pageSize = 512;
        public int Index { get; set; }
        public bool Modified { get; set; }
        public DateTime Time { get; set; }
        public byte[] Bitmap { get; set; }
        public int[] Data { get; set; }

        public Page(int index)
        {
            Index = index;
            Modified = false;
            Time = DateTime.Now;
            Bitmap = new byte[pageSize / sizeof(int)]; // 128 бит. каждый бит соответсвует ячейки массива 
            Data = new int[pageSize / sizeof(int)]; // 128 элементов


        }
    }
}
