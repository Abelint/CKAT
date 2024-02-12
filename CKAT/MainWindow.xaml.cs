using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
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
using System.Security.Cryptography;


namespace CKAT
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // private Queue<string> filesToDownload = new Queue<string>();
        Thread thread;
       static  event Action<int> ValueReceived;
        static Label label;

        private Queue<string[]> filesToDownload2 = new Queue<string[]>();
        private WebClient webClient;
        private string currentFile;
        private long sizeFile = -1;
        string dir = "";
        string grups = "";
        string token = "";
        string pass = "";
        string url = "";
        string id_pc = "";
        
        ImageBrush Brushing(Bitmap src)
        {
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
                ImageSource = image
            };

            return imBrush;
        }
        
        public MainWindow()
        {
          
            InitializeComponent();
           // String path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "Resources\\_11.bmp");

            
            Bitmap src = Properties.Resources._11;
           
            this.Background = Brushing(src);
            src = Properties.Resources.krestik;
            krestikButton.Background = Brushing(src);
            
            src = Properties.Resources.svernut;
            svernutButton.Background = Brushing(src);

            int percent = 0;
            // Параметры для POST-запроса
            url = "http://skat.geo-atlas.ru/core.php";
            //string dir = "grups1";
            pass = "8882";
           
            id_pc = UniqueComputerIdentifier.GetComputerIdentifier(); //номер компьютера    // добавить хэширование
            byte[] bytes = Encoding.ASCII.GetBytes(id_pc);
            MD5 md5 = MD5.Create();
            byte[] hesh = md5.ComputeHash(bytes);
            id_pc =  "";
            foreach (byte b in hesh)
            {
                id_pc += Convert.ToString(b);
            }
            Dictionary<string, Dictionary<string, string>> iniFile = ReadIniFile("inibut.ini");
           // Dictionary<string, Dictionary<string, string>> iniFile = ReadIniFile("inibut_работает.ini");
            Dictionary<string, string> positionIni = iniFile["PERINF"];
            grups = positionIni["idcl"];
            dir = grups;
            token = positionIni["token"];
            Process[] process = Process.GetProcesses();
            foreach( Process processItem in process)
            {
                if (processItem.ProcessName == "skat" || processItem.ProcessName == "skat.exe")
                {
                    processItem.Kill();
                }
               

            }

           
            label = LabelForText;
                ValueReceived += HandleValueReceived;
                thread = new Thread(OtdelniyThread);
                thread.Start();
           


            


        }


        void OtdelniyThread()
        {

            //try
            //{
            int countNehorosho = 0;
        nehorosho:

            string key = File.ReadAllText("key");
                // Формирование POST-данных
                string postData = $"dir={dir}&pass={pass}&grups={grups}&id_pc={id_pc}&token_key={key}";

                // Создание объекта запроса
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

            try
            {
                // Запись POST-данных в поток запроса
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(postData);
                }
            }
            catch(Exception e)
            {
                countNehorosho++;
                Thread.Sleep(250);
                if (countNehorosho < 20) goto nehorosho;
                else
                {
                    //Создаем новый процесс
                       Process process = new Process();
                    // Задаем параметры запуска процесса
                    process.StartInfo.FileName = "skat.exe";
                    process.StartInfo.Arguments = token; // Первый аргумент
                                                         // Запускаем процесс
                    process.Start();
                    Thread.Sleep(1000);
                    Thread.CurrentThread.Abort();
                }

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
                        // Разделение списка файлов с размерами
                        string[] files = responseText.Split(';');

                        int countList = files.Length;
                        int count = 0;
                        // В основном методе, после разбора ответа

                        foreach (string file in files)
                        {
                            // Логика по подготовке очереди для загрузки...
                            // Разделение имени файла и его размера
                            string[] fileInfo = file.Split('-');
                            ;
                            string fileName = fileInfo[0].Replace("build/" + dir + "\\", "");
                            string fileSize = fileInfo[1];
                            Dispatcher.Invoke(new Action(() => {
                                //progressBar.Value = count * 100 / files.Length;
                                LabelForText.Content = "Загружаем " + fileName;
                            }));

                        // Создание папки для сохранения файлов, если она не существует
                        string directory = Path.GetDirectoryName(Path.GetFullPath(fileName));
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }


                            bool downyzacompom = false;
                            if (!File.Exists(fileName))
                            {
                                downyzacompom = true;
                            }
                            else if (File.Exists(fileName))
                            {
                                FileInfo existingFile = new FileInfo(fileName);
                                if(!(existingFile.Length == Convert.ToInt64(fileSize)))
                                {
                                    downyzacompom = true;
                                }
                            }
                          

                        if (downyzacompom)
                        {
                            webClient = new WebClient();
                            string fileUrl = $"http://skat.geo-atlas.ru/build/{dir}/{fileName}";

                            ebanutsya:
                            try
                            {
                                webClient.DownloadFile(fileUrl, fileName);
                            }
                            catch (Exception ex)
                            {
                                Thread.Sleep(250);
                                goto ebanutsya;
                            }
                           
                            
                        }

                        count++;
                        //progressBar.Value = count;
                        // filesToDownload.Enqueue(file);
                        //ValueReceived?.Invoke(count);
                        Dispatcher.Invoke(new Action(() => { 
                            progressBar.Value = count*100/files.Length;
                            //LabelForText.Content ="Загружаем "+fileName; 
                        }));
                        }

                    if (count == files.Length)
                    {
                        // Создаем новый процесс
                        Process process = new Process();
                        // Задаем параметры запуска процесса
                        process.StartInfo.FileName = "skat.exe";
                        process.StartInfo.Arguments = token; // Первый аргумент
                        // Запускаем процесс
                        process.Start();
                        Thread.Sleep(250);
                        Dispatcher.Invoke(new Action(() => {
                            //progressBar.Value = count * 100 / files.Length;
                            Close();
                        }));
                     
                    }

                }
            }
           

        }

        static  void HandleValueReceived(int value)
        {
            // Обрабатываем полученное значение в основном потоке
            //label.Content="";
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

       

        private void krestikButton_Click(object sender, RoutedEventArgs e)
        {
            if(thread != null) thread.Abort();
            Close();
        }

        private void svernutButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }

   
}


