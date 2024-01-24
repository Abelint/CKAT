using System;
using System.Management;
using Microsoft.Win32;


namespace CKAT
{
    public class UniqueComputerIdentifier
    {
        public static string GetComputerIdentifier()
        {
            string identifier = string.Empty;
            try
            {
                // Создаем объект для запроса информации о компьютере
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystemProduct");

                foreach (ManagementObject obj in searcher.Get())
                {
                    // Получаем значение свойства "UUID", которое является уникальным идентификатором компьютера
                    identifier = obj["UUID"].ToString();
                    break; // Получаем только первый найденный идентификатор
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Ошибка при получении идентификатора компьютера: " + ex.Message);
                return "Ошибка при получении идентификатора компьютера: " + ex.Message;
            }

            return identifier;
        }

        public static string ComputerIdentifier()
        {
            return GetComputerIdentifier(); 
        }
    }
}
