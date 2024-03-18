﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace laba1_proga
{
    class VirtualMemoryManager
    {
        const int PageSize = 512;
        const string signature = "VM";

        const string filename = "VM.bin";

        private int arraySize;
        FileStream file;
        Page[] pages;

        public VirtualMemoryManager(string filename = "vm.bin", int size = 10000)
        {
            if (size <= 0)
            {
                throw new ArgumentException("Размер массива не может быть отрицательным или равным 0!");
            }
            arraySize = size;
            if (!File.Exists(filename))
            {
                using (FileStream fileStream = new FileStream(filename, FileMode.Create))
                {
                    Console.WriteLine("Файл создан");
                    // Рассчитываем количество страниц и записываем их в файл
                    int pageCount = (arraySize * sizeof(int) + PageSize - 1) / PageSize;

                    // Записываем сигнатуру "VM" как массив байтов
                    byte[] signatureBytes = Encoding.ASCII.GetBytes(signature);
                    fileStream.Write(signatureBytes, 0, signatureBytes.Length);

                    fileStream.Flush();


                    byte[] data = new byte[512]; // 512 байт + 128 бит = 528 байт на битовую карту и страницу

                    byte[] map = new byte[16];

                    for (int i = 0; i < pageCount; i++)
                    {
                        fileStream.Write(map, 0, map.Length);
                        fileStream.Write(data, 0, data.Length);
                    }
                }
            }
            using (FileStream fileStream = new FileStream(filename, FileMode.Open))
            {
                Console.WriteLine("Файл открыт");
                int pageCount = 3; //  количество страниц

                this.pages = new Page[pageCount];
                byte[] bitmapData = new byte[16];
                byte[] pageData = new byte[512];

                // Обходим каждую страницу в буфере
                for (int pageNumber = 0; pageNumber < pageCount; pageNumber++)
                {
                    this.pages[pageNumber] = new Page(pageNumber + 1);

                    // Пропускаем сигнатуру "VM" (2 байта)
                    fileStream.Seek(2 + (PageSize + 16) * pageNumber, SeekOrigin.Begin);


                    fileStream.Read(bitmapData, 0, bitmapData.Length);
                    pages[pageNumber].Bitmap = bitmapData;

                    fileStream.Read(pageData, 0, pageData.Length);

                    Console.WriteLine($"номер страницы [{pages[pageNumber].Index}]");
                    // Декодируем данные из буфера
                    for (int j = 0; j < pages[pageNumber].Data.Length; j++)
                    {
                        pages[pageNumber].Data[j] = BitConverter.ToInt32(pageData, j * sizeof(int));
                        if (pages[pageNumber].Data[j] != 0)
                        {
                            pages[pageNumber].Modified = true;
                            Console.WriteLine($"[{j}] {pages[pageNumber].Data[j]}");
                        }
                    }
                }
            }
        }

        int GetPageIndex(int index)
        {
            int PageNumber = (int)Math.Ceiling((double)((index + 1) * sizeof(int)) / PageSize);
            if (index == 0)
                PageNumber = 1;
            bool pageFound = false;
            foreach (Page page in this.pages)
            {
                if (page.Index == PageNumber)
                {

                    pageFound = true;
                    break;
                }
            }

            if (!pageFound)
            {
                DateTime oldTime = DateTime.MaxValue; // Ищем самую старую страницу
                int oldTimeIndex = -1;
                for (int i = 0; i < pages.Length; i++)
                {
                    if (pages[i].Time < oldTime)
                    {
                        oldTime = pages[i].Time;
                        oldTimeIndex = i;
                    }
                }

                // Действия по выбору самой старой страницы
                if (pages[oldTimeIndex].Modified) // Если страница была модифицирована
                {
                    // Выгружаем ее в файл на предназначенное ей место
                    using (FileStream fileStream = new FileStream(filename, FileMode.Open))
                    {
                        fileStream.Seek((pages[oldTimeIndex].Index - 1) * (PageSize + 16) + 2, SeekOrigin.Begin);
                        fileStream.Write(pages[oldTimeIndex].Bitmap, 0, pages[oldTimeIndex].Bitmap.Length);

                        using (BinaryWriter writer = new BinaryWriter(fileStream))
                        {
                            foreach (int data in pages[oldTimeIndex].Data)
                            {
                                writer.Write(data);
                            }
                        }

                    }
                }
                pages[oldTimeIndex] = new Page(PageNumber);

                using (FileStream fileStream = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    long position = (PageNumber - 1) * PageSize;

                    //загружаем битовую карту
                    fileStream.Seek(2 + 16 * (PageNumber - 1) + (PageNumber - 1) * PageSize, SeekOrigin.Begin);

                    byte[] bitmap = new byte[16];

                    fileStream.Read(bitmap, 0, bitmap.Length);

                    pages[oldTimeIndex].Bitmap = bitmap;


                    byte[] buffer = new byte[PageSize];

                    int bytesRead = fileStream.Read(buffer, 0, PageSize);


                    if (bytesRead > 0)
                    {
                        for (int i = 0; i < bytesRead / sizeof(int); i++)
                        {
                            pages[oldTimeIndex].Data[i] = BitConverter.ToInt32(buffer, i * sizeof(int));
                        }
                    }
                    else
                    {

                        Console.WriteLine("Ошибка чтения данных из файла.");
                    }
                }

                return oldTimeIndex + 1;
            }

            return PageNumber;
        }

        public int Read(int index)
        {
            if (index < 0 || index >= arraySize)
            {
                throw new IndexOutOfRangeException($"Индекс {index} выходит за границы массива.");
            }
            GetPageIndex(index);
            int pageNumber = (int)Math.Ceiling((double)((index + 1) * sizeof(int)) / PageSize);
            int pageIndexInPage = (index * sizeof(int)) % PageSize; // Смещение индекса внутри страницы
            int value = 0;

            using (FileStream fileStream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                long position = (pageNumber - 1) * PageSize + pageIndexInPage;

                if (pageNumber == 1)
                {
                    position = pageIndexInPage;
                }

                fileStream.Seek(2 + 16 * pageNumber + position, SeekOrigin.Begin);

                byte[] buffer = new byte[sizeof(int)];

                int bytesRead = fileStream.Read(buffer, 0, sizeof(int));

                if (bytesRead == sizeof(int))
                {
                    value = BitConverter.ToInt32(buffer, 0);
                }
                else
                {

                    Console.WriteLine("Ошибка чтения данных из файла.");
                }
            }
            return value;
        }

        public int Write(int value, int index)
        {
            if (index < 0 || index >= arraySize)
            {
                throw new IndexOutOfRangeException($"Индекс {index} выходит за границы массива.");
            }
            GetPageIndex(index);
            int pageNumber = (int)Math.Ceiling((double)((index + 1) * sizeof(int)) / PageSize);

            int pageIndexInPage = ((index * sizeof(int)) % PageSize) / sizeof(int); // Смещение индекса внутри страницы, номер элемента

            int byteIndex = pageIndexInPage / 8; // номер байта

            int byteOffcet = pageIndexInPage % 8; // смещение внутри байта

            byte mask = (byte)(1 << (7 - byteOffcet));

            using (FileStream fileStream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                long position = (pageNumber - 1) * (PageSize) + pageIndexInPage * sizeof(int); // Позиция в файле для записи данных

                //long currentPos = fileStream.Position;

                //смещение для битовой карты
                fileStream.Seek(2 + 16 * (pageNumber - 1) + (pageNumber - 1) * PageSize, SeekOrigin.Begin);

                byte[] bitmap = new byte[16];

                // вот тут надо прочитать файл и скопировать значения в массив bitmap
                fileStream.Read(bitmap, 0, bitmap.Length);

                bitmap[byteIndex] |= mask;

                fileStream.Seek(2 + 16 * (pageNumber - 1) + (pageNumber - 1) * PageSize, SeekOrigin.Begin);
                fileStream.Write(bitmap, 0, bitmap.Length);

                fileStream.Seek(2 + 16 * pageNumber + position, SeekOrigin.Begin);// 2 - сигнатура, 16 байт - битовая карта

                byte[] buffer = BitConverter.GetBytes(value);
                fileStream.Write(buffer, 0, buffer.Length);

                Console.WriteLine($"\nЗначение {value} записана по индексу {index} на страницу {pageNumber}");

                // обновляем данные страницы
                foreach (Page page in pages)
                {
                    if (page.Index == pageNumber)
                    {
                        page.Modified = true;
                        page.Bitmap = bitmap;
                        page.Data[pageIndexInPage] = value;
                    }
                }

            }
            PrintPage();
            return 1;
        }
        public void PrintDataPage(int pageNumber)
        {
            GetPageIndex(pageNumber);
            foreach (Page page in pages)
            {
                if (pageNumber == page.Index)
                {
                    foreach (var item in page.Data)
                    {
                        Console.Write($"{item} ");
                    }
                }
            }
        }

        public void PrintPage() // показывает какие страницы в памяти
        {
            Console.WriteLine("\nНомера страниц в памяти");
            foreach (var item in pages)
            {
                Console.Write($"{item.Index}  ");
            }
        }
    }

}
