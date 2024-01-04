using GlobalHotKeys;
using GlobalHotKeys.Native.Types;
using Serilog;
using Serilog.Core;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using WindowStyle = System.Windows.WindowStyle;

namespace StatTrackScript
{
    public partial class MainWindow
    {
        private bool _isRunning;
        private Logger? _logger;

        private readonly DispatcherTimer _timer;
        private readonly DispatcherTimer _tenSecondTimer;
        private Window? _overlayWindow;
        private HotKeyManager _hotKeyManager;

        public MainWindow()
        {
            InitializeComponent();
            ConfigureLogger();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            _timer.Tick += Timer_Tick!;

            _tenSecondTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _tenSecondTimer.Tick += TenSecondTimer_Tick!;
            
            void StartAction(HotKey _)
            {
                Dispatcher.Invoke(() =>
                {
                    if (_isRunning)
                    {
                        _isRunning = false;
                        _timer.Stop();
                        _tenSecondTimer.Stop();
                        Log("Скрипт остановлен.");
                        IsStarted.Text = "Скрипт остановлен";
                        return;
                    }

                    _isRunning = true;
                    _timer.Start();
                    _tenSecondTimer.Start();
                    IsStarted.Text = "Скрипт запущен";
                    Log("Скрипт запущен.");
                });
            }

            _hotKeyManager = new HotKeyManager();
            _hotKeyManager.Register(VirtualKeyCode.VK_F6, Modifiers.NoRepeat);
            _hotKeyManager.Register(VirtualKeyCode.VK_F7, Modifiers.NoRepeat);
            _hotKeyManager.HotKeyPressed.Subscribe((Action<HotKey>)StartAction);
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
                _isRunning = false;
                _timer.Stop();
                _tenSecondTimer.Stop();
                Log("Скрипт остановлен.");
                IsStarted.Text = "Скрипт остановлен";
                return;
            }

            _isRunning = true;
            _timer.Start();
            _tenSecondTimer.Start();
            IsStarted.Text = "Скрипт запущен";
            Log("Скрипт запущен.");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(RunScript);
        }
        
        private void TenSecondTimer_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Click(763, 285);
                Thread.Sleep(200);
                Click(456, 964);
            });
        }

        private void RunScript()
        {
            Log("Проверка лота 1");
            if (CheckCoordsForColor(1078, 359, 1, 11000000))
            {
                Click(1401, 358);
                CommonClick();
            }

            Log("Проверка лота 2");
            if (CheckCoordsForColor(1080, 443, 1, 11000000))
            {
                Click(1391, 442);
                CommonClick();
            }
            
            Log("Проверка лота 3");
            if (CheckCoordsForColor(1079, 521, 1, 11000000))
            {
                Click(1392, 528);
                CommonClick();
            }
            
            Log("Проверка лота 4");
            if (CheckCoordsForColor(1078, 600, 1, 11000000))
            {
                Click(1409, 612);
                CommonClick();
            }
            
            Log("Проверка лота 5");
            if (CheckCoordsForColor(1080, 683, 1, 11000000))
            {
                Click(1395, 681);
                CommonClick();
            }
            
            Log("Проверка лота 6");
            if (CheckCoordsForColor(1080, 764, 1, 11000000))
            {
                Click(1394, 764);
                CommonClick();
            }
            
            Log("Проверка лота 7");
            if (CheckCoordsForColor(1078, 842, 1, 11000000))
            {
                Click(1409, 846);
                CommonClick();
            }

            if (CheckCoordsForColor(767, 300, 5000000, 7000000))
            {
                Click(767, 300);
                Thread.Sleep(300);
            }

            if (CheckCoordsForColor(977, 647, 8000000, 11200000))
            {
                Click(977, 647);
                Thread.Sleep(300);
            }

            return;

            void CommonClick()
            {
                Thread.Sleep(2);
                for (var i = 0; i < 3; i++)
                {
                    Click(950, 660);
                }

                Thread.Sleep(300);

                Click(467, 975);
                Thread.Sleep(100);

                Click(767, 299);
                Click(767, 299);
                Log("Лот куплен");
            }
        }

        #region Logic

        private static int GetColorAt(int x, int y)
        {
            var screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            using (var gDest = Graphics.FromImage(screenPixel))
            {
                using (var gSrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    var hSrcDc = gSrc.GetHdc();
                    var hDc = gDest.GetHdc();
                    BitBlt(hDc, 0, 0, 1, 1, hSrcDc, x, y,
                        (int)CopyPixelOperation.SourceCopy);
                    gDest.ReleaseHdc();
                    gSrc.ReleaseHdc();
                }
            }

            return screenPixel.GetPixel(0, 0).ToArgb() * -1;
        }

        private bool CheckCoordsForColor(int x, int y, int lowerThreshold, int higherThreshold)
        {
            var number = GetColorAt(x, y);
            //Проверяем диапазон
            if (number >= lowerThreshold && number <= higherThreshold)
            {
                return true;
            }

            return false;
        }

        private void Log(string message)
        {
            // Записать сообщение в консоль и в файл журнала
            _logger?.Information(message);
        }

        // Импортируем функцию для симуляции кликов мышью
        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc,
            int ySrc, int dwRop);

        // Константы для нажатия и отпускания левой кнопки мыши
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        // Функция для клика мышью в заданных координатах
        private void Click(int x, int y)
        {
            // Перемещаем курсор в нужную позицию
            SetCursorPos(x, y);
            // Нажимаем и отпускаем левую кнопку мыши
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, x, y, 0, 0);
            Thread.Sleep(1);
            Log($"Клик в координаты {x}, {y}.");
        }
        #endregion

        #region Testing

        private void TestClick(int x, int y)
        {
            ShowPointOnOverlay(x, y);
            Log($"Тест клик в координаты {x}, {y}");
            Thread.Sleep(1000);
        }

        private void ShowPointOnOverlay(int x, int y)
        {
            var point = new Ellipse
            {
                Width = 5,
                Height = 5,
                Fill = Brushes.Red,
                Margin = new Thickness(x - 2.5, y - 2.5, 0, 0),
            };

            _overlayWindow?.Dispatcher.Invoke(() =>
            {
                _overlayWindow.Topmost = true;
                _overlayWindow.Show();
                _overlayWindow.Left = x - 2.5;
                _overlayWindow.Top = y - 2.5;
                _overlayWindow.Content = point;
            });
        }

        private void InitializeOverlayWindow()
        {
            _overlayWindow = new Window
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Red,
                ShowInTaskbar = false,
                Width = 3,
                Height = 3,
                Topmost = true,
                Top = 0,
                Left = 0,
                Content = null,
            };

            _overlayWindow.Closed += (_, _) => _overlayWindow = null;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (_isRunning)
                {
                    Log("Попытка тестирования, но уже запущено.");
                    return;
                }

                Log("Тест запущен.");

                if (!CheckCoordsForColor(1043, 276, 1, 10000000))
                {
                    return;
                }

                InitializeOverlayWindow();

                TestClick(1282, 277);

                for (var i = 0; i < 3; i++)
                {
                    TestClick(944, 617);
                }

                TestClick(613, 985);
                Thread.Sleep(300);

                TestClick(808, 233);
                Thread.Sleep(50);

                TestClick(808, 233);

                if (CheckCoordsForColor(1294, 732, 9429842, 11029842))
                {
                    TestClick(985, 620);
                }

                if (CheckCoordsForColor(889, 371, 5000000, 6688999))
                {
                    TestClick(808, 233);
                }

                if (CheckCoordsForColor(343, 968, 2429848, 7029842))
                {
                    TestClick(613, 985);
                }

                _overlayWindow?.Close();
            });
        }

        #endregion

        private void MainWindow_OnClosed(object? sender, EventArgs e)
        {
            _hotKeyManager.Dispose();
        }
    }
}