using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Path = System.IO.Path;


namespace CKAT
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       // private Queue<string> filesToDownload = new Queue<string>();
        private Queue<string[]> filesToDownload2 = new Queue<string[]>();
        private WebClient webClient;
        private string currentFile;
        string dir = "";
        string grups = "";
        string token = "";
        public MainWindow()
        {
            InitializeComponent();
           // String path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Resources\\_11.bmp");
            Bitmap src = Properties.Resources._11;
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            ImageBrush imBrush = new ImageBrush()
            {
                //ImageSource = new BitmapImage(new Uri(path))
                ImageSource =image
            };
            this.Background = imBrush;

            int percent = 0;
            // Параметры для POST-запроса
            string url = "http://skat.geo-atlas.ru/core.php";
            //string dir = "grups1";
            string pass = "8882";
           
            string id_pc = UniqueComputerIdentifier.GetComputerIdentifier(); //номер компьютера
            Dictionary<string, Dictionary<string, string>> iniFile = ReadIniFile("inibut.ini");
           // Dictionary<string, Dictionary<string, string>> iniFile = ReadIniFile("inibut_работает.ini");
            Dictionary<string, string> positionIni = iniFile["PERINF"];
            grups = positionIni["idcl"];
            dir = grups;
            token = positionIni["token"];

            byte count =0;
            bool allRight = true;
            while(count < 10 && allRight)
            {
                try
                {
                    allRight = false;
                    // Формирование POST-данных
                    string postData = $"dir={dir}&pass={pass}&grups={grups}&id_pc={id_pc}";

                    // Создание объекта запроса
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";

                    // Запись POST-данных в поток запроса
                    using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                    {
                        writer.Write(postData);
                    }

                    // Отправка запроса и получение ответа
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        // Чтение ответа
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            string responseText = reader.ReadToEnd();
                            // Обработка ответа
                            responseText = responseText.Remove(responseText.Length - 1);
                            //  ProcessResponse(responseText);

                            responseText = responseText.Remove(responseText.Length - 1);
                            // Разделение списка файлов с размерами
                            string[] files = responseText.Split(';');

                            // В основном методе, после разбора ответа
                            foreach (string file in files)
                            {
                                // Логика по подготовке очереди для загрузки...
                                // Разделение имени файла и его размера
                                string[] fileInfo = file.Split('-');
                                ;
                                string fileName = fileInfo[0].Replace("build/" + dir + "\\", "");
                                string fileSize = fileInfo[1];


                                // Создание папки для сохранения файлов, если она не существует
                                string directory = Path.GetDirectoryName(Path.GetFullPath(fileName));
                                if (!Directory.Exists(directory))
                                {
                                    Directory.CreateDirectory(directory);
                                }

                                // filesToDownload.Enqueue(file);
                                fileInfo[0] = fileInfo[0].Replace("build/" + dir + "\\", "");
                                filesToDownload2.Enqueue(fileInfo);
                            }

                            InitializeDownloader(); // Инициализация WebClient и настройка событий
                            DownloadNextFile(); // Начать загрузку первого файла в очереди


                        }
                    }
            }
                catch (Exception ex)
                {
                count++;
                allRight = true;
                Thread.Sleep(50);
            }
        }
            if (allRight)
            {
                // Создаем новый процесс
                Process process = new Process();
                // Задаем параметры запуска процесса
                process.StartInfo.FileName = "skat.exe";
                process.StartInfo.Arguments = token; // Первый аргумент
                // Запускаем процесс
                process.Start();
                Thread.Sleep(1000);

                Close();
            }
            

        }

       
        private void DownloadNextFile()
        {
            if (filesToDownload2.Count > 0)
            {
                // currentFile = filesToDownload.Dequeue();
                string[] huynya3000 = filesToDownload2.Dequeue();
                currentFile = huynya3000[0];
               
                string fileUrl = $"http://skat.geo-atlas.ru/build/{dir}/{currentFile}";

                LabelForText.Content = $"Файл {currentFile} качаем.";
                FileInfo existingFile = new FileInfo(currentFile);
                long fileSize = long.Parse(huynya3000[1]);

                if (!File.Exists(existingFile.FullName))
                {
                    webClient.DownloadFileAsync(new Uri(fileUrl), currentFile);
                }
                else if (existingFile.Length != fileSize)
                {
                    webClient.DownloadFileAsync(new Uri(fileUrl), currentFile);
                }
                else
                {
                    DownloadNextFile();
                }

                //webClient.DownloadFileAsync(new Uri(fileUrl), currentFile);
            }
            else
            {
                // Создаем новый процесс
                Process process = new Process();
                // Задаем параметры запуска процесса
                process.StartInfo.FileName = "skat.exe";
                process.StartInfo.Arguments = token; // Первый аргумент
                // Запускаем процесс
                process.Start();
                Thread.Sleep(1000);
                Close();
            }

        }
        private void InitializeDownloader()
        {
            webClient = new WebClient();
           
           // webClient.dow
            webClient.DownloadProgressChanged += (s, e) =>
            {
                progressBar.Value = e.ProgressPercentage;
                // Проверка размера файла
               
                    LabelForText.Content = $"Файл {currentFile} качаем.";
            };

            webClient.DownloadFileCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    // Обработка ошибок, если они есть.
                    LabelForText.Content = $"Ошибка при загрузке файла {currentFile}.";
                }
                else if (e.Cancelled)
                {
                    // Обработка ситуации отмены загрузки.
                    LabelForText.Content = $"Загрузка файла {currentFile} отменена.";
                }
                else
                {
                    // Файл успешно загружен.
                    LabelForText.Content = $"Файл {currentFile} загружен.";
                    DownloadNextFile(); // Запуск загрузки следующего файла в очереди.
                }
            };
        }


        private void MainWindow_Load(object sender, EventArgs e)
        {
            int percent = 0;
            // Параметры для POST-запроса
            string url = "http://skat.geo-atlas.ru/1.php";
            string dir = "grups1";
            string pass = "8882";
            string grups = "grups1";
            string id_pc = "test";

            // Формирование POST-данных
            string postData = $"dir={dir}&pass={pass}&grups={grups}&id_pc={id_pc}";

            // Создание объекта запроса
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            // Запись POST-данных в поток запроса
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(postData);
            }

            // Отправка запроса и получение ответа
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                // Чтение ответа
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseText = reader.ReadToEnd();
                    // Обработка ответа
                    responseText = responseText.Remove(responseText.Length - 1);
                    //  ProcessResponse(responseText);

                    responseText = responseText.Remove(responseText.Length - 1);
                    // Разделение списка файлов с размерами
                    string[] files = responseText.Split(';');
                    foreach (string file in files)
                    {
                        // Разделение имени файла и его размера
                        string[] fileInfo = file.Split('-');
                        ;
                        string fileName = fileInfo[0].Replace("build/grups1\\", "");
                        string fileSize = fileInfo[1];


                        // Создание папки для сохранения файлов, если она не существует
                        string directory = Path.GetDirectoryName(Path.GetFullPath(fileName));
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        // Проверка, существует ли файл
                        if (File.Exists(fileName))
                        {
                            // Проверка размера файла
                            FileInfo existingFile = new FileInfo(fileName);
                            if (existingFile.Length != long.Parse(fileSize))
                            {
                                // Скачивание файла
                                Console.WriteLine($"Файл {fileName} качаем.");
                                string fileUrl = "http://skat.geo-atlas.ru/build/grups1/" + fileName;
                                WebClient webClient = new WebClient();
                                webClient.DownloadFile(fileUrl, fileName);
                                Console.WriteLine($"Файл {fileName} обновлен.");
                            }
                            else
                            {
                                Console.WriteLine($"Файл {fileName} не требует обновления.");
                            }
                        }
                        else
                        {
                            // Скачивание файла, если его нет
                            Console.WriteLine($"Файл {fileName} качаем.");
                            string fileUrl = "http://skat.geo-atlas.ru:49800/build/grups1/" + fileName;
                            WebClient webClient = new WebClient();
                            webClient.DownloadFile(fileUrl, fileName);
                            Console.WriteLine($"Файл {fileName} загружен.");
                        }



                    }
                }



            }
        }

        static Dictionary<string, Dictionary<string, string>> ReadIniFile(string filePath)
        {
            var iniData = new Dictionary<string, Dictionary<string, string>>();
            string currentSection = null;

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.Substring(1, line.Length - 2);
                    iniData[currentSection] = new Dictionary<string, string>();
                }
                else if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith(";"))
                {
                    var keyValue = line.Split(new[] { '=' }, 2);

                    if (keyValue.Length != 2) continue;

                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim();

                    iniData[currentSection].Add(key, value);
                }
            }

            return iniData;
        }


        //    static void ProcessResponse(string responseText)
        //{


        //// Создаем новый процесс
        //Process process = new Process();
        //// Задаем параметры запуска процесса
        //process.StartInfo.FileName = "skat.exe";
        //process.StartInfo.Arguments = "ff2123eeee222"; // Первый аргумент
        //// Запускаем процесс
        //process.Start();
        //}
    }
}


