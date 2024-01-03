using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using Serilog;
using Serilog.Core;
using Path = System.IO.Path;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace StatTrackScript
{
    public partial class MainWindow
    {
        private bool _isRunning;
        private Logger? _logger;

        private readonly DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            ConfigureLogger();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick!;
        }

        private void ConfigureLogger()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning)
            {
                Log("Попытка запуска, но уже запущено.");
                return;
            }

            _isRunning = true;
            _timer.Start();
            Log("Скрипт запущен.");
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRunning)
            {
                Log("Попытка остановки, но уже остановлено.");
                return;
            }

            _isRunning = false;
            _timer.Stop();
            Log("Скрипт остановлен.");
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Добавьте код для перемещения окна (если нужно)
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(RunScript);
        }

        private void RunScript()
        {
            if (CheckCoordsForColor(1043, 276, 1, 10000000))
            {
                return;
            }

            Click(1282, 277);
            Log("Клик в координаты 1282, 277.");

            for (var i = 0; i < 3; i++)
            {
                Click(944, 617);
                Log($"Клик в координаты 944, 617, итерация {i + 1}.");
            }

            Click(613, 985);
            Log("Клик в координаты 613, 985.");
            Thread.Sleep(300);

            Click(808, 233);
            Log("Клик в координаты 808, 233.");
            Thread.Sleep(50);

            Click(808, 233);
            Log("Клик в координаты 808, 233.");

            Log("Проверка 'ok.jpg'");
            
            if (CheckCoordsForColor(1294, 732, 9429842, 11029842))
            {
                Click(985, 620);
                Log("Изображение 'ok.jpg' найдено, клик в координаты 985, 620.");
            }

            Log("Проверка 'zapros.jpg'");
            if (CheckCoordsForColor(889, 371, 5000000, 6688999))
            {
                Click(808, 233);
                Log("Изображение 'zapros.jpg' найдено, клик в координаты 808, 233.");
            }

            Log("Проверка 'nazad.jpg'");
            if (CheckCoordsForColor(343, 968, 2429848, 7029842))
            {
                Click(613, 985);
                Log("Изображение 'nazad.jpg' найдено, клик в координаты 613, 985.");
            }
        }

        private bool CheckCoordsForColor(int x, int y, int lowerThreshold, int higherThreshold)
        {
            var color = GetPixelColor(x, y);
            var number = color.ToArgb() * -1;
            
            Log($"Цвет пикселя: {color}, Число: {number}");

            if (number < lowerThreshold || number > higherThreshold)
            {
                Log("Число вне диапазона, пропуск итерации.");
                return true;
            }

            return false;
        }

        private void Log(string message)
        {
            // Записать сообщение в консоль и в файл журнала
            _logger.Information(message);
        }

        // Импортируем функцию для симуляции кликов мышью
        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        // Константы для нажатия и отпускания левой кнопки мыши
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        // Функция для клика мышью в заданных координатах
        private static void Click(int x, int y)
        {
            // Перемещаем курсор в нужную позицию
            SetCursorPos(x, y);
            // Нажимаем и отпускаем левую кнопку мыши
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, x, y, 0, 0);

            Thread.Sleep(100);
        }

        // Функция для получения цвета пикселя на экране
        private static Color GetPixelColor(int x, int y)
        {
            // Создаем битмап размером 1x1 пиксель
            var bitmap = new Bitmap(1, 1);
            // Рисуем на нем копию экрана
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(x, y, 0, 0, new Size(1, 1));
            }

            // Возвращаем цвет единственного пикселя
            return bitmap.GetPixel(0, 0);
        }

        // Функция для поиска изображения на экране
        private static bool FindImage(string fileName)
        {
            // Загружаем изображение из файла
            var image = new Bitmap(fileName);
            // Получаем ширину и высоту изображения
            var width = image.Width;
            var height = image.Height;

            // Перебираем все пиксели на экране
            for (var x = 0; x < SystemParameters.WorkArea.Width - width; x++)
            {
                for (var y = 0; y < SystemParameters.WorkArea.Height - height; y++)
                {
                    // Сравниваем пиксели изображения с пикселями экрана
                    var match = true;
                    for (var i = 0; i < width && match; i++)
                    {
                        for (var j = 0; j < height && match; j++)
                        {
                            // Если пиксели не совпадают, то прерываем цикл
                            Console.WriteLine($"{x + i} {j}");

                            if (image.GetPixel(i, j) != GetPixelColor(x + i, y + j))
                            {
                                match = false;
                            }
                        }
                    }

                    // Если все пиксели совпали, то возвращаем true
                    if (match)
                    {
                        return true;
                    }
                }
            }

            // Если не нашли совпадения, то возвращаем false
            return false;
        }
    }
}