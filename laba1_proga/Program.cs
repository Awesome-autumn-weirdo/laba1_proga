using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace laba1_proga
{
    class Program
    {
        static void Main()
        {
            try
            {
                VirtualMemoryManager a = new VirtualMemoryManager("vm.bin", 10000);
                a.PrintPage();
                //для начала проверим запись данных и работу битовой карты
                Console.WriteLine($"Значение по индексу =  {a.Read(1200)}");
                Console.WriteLine($"Значение по индексу =  {a.Read(1201)}");
                a.Write(12, 0); 
                a.Write(35, 6);
                a.Write(144, 8); 
                a.Write(5, 138);
                a.Write(32, 256); 
                a.Write(77, 358);
                a.Write(1, 1200); 
                a.Write(888, 1537);
                a.Write(1234, 7000);

                Console.WriteLine($"Значение по индексу =  {a.Read(1200)}");
                Console.WriteLine($"Значение по индексу =  {a.Read(1201)}");

            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"\nОшибка при инициализации объекта VirtualMemory: {ex.Message}");

            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine($"\nОшибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка: {ex.Message}");
            }
            Console.ReadLine();

        }
    }
}
