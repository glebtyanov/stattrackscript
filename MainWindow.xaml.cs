using GlobalHotKeys;
using GlobalHotKeys.Native.Types;
using Serilog;
using Serilog.Core;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Threading;
using Size = System.Drawing.Size;
using WindowStyle = System.Windows.WindowStyle;

namespace StatTrackScript
{
	public partial class MainWindow
	{
		private bool _isRunning;
		private Logger? _logger;

		private readonly DispatcherTimer _timer;
		private Window? _overlayWindow;
		private HotKeyManager _hotKeyManager;
		public MainWindow()
		{
			InitializeComponent();
			ConfigureLogger();

			_timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(1)
			};
			_timer.Tick += Timer_Tick!;

			void StartAction(HotKey _)
			{
				Dispatcher.Invoke(() =>
				{

					if (_isRunning)
					{
						_isRunning = false;
						_timer.Stop();
						Log("Скрипт остановлен.");
						IsStarted.Text = "Скрипт не запущен";
						return;
					}

					_isRunning = true;
					_timer.Start();
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
				Log("Скрипт остановлен.");
				IsStarted.Text = "Скрипт не запущен";
				return;
			}

			_isRunning = true;
			_timer.Start();
			IsStarted.Text = "Скрипт запущен";
			Log("Скрипт запущен.");
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			Dispatcher.Invoke(RunScript);
		}

		private void RunScript()
		{
			if (!CheckCoordsForColor(1043, 276, 1, 10000000))
			{
				return;
			}

			Click(1282, 277);

			for (var i = 0; i < 3; i++)
			{
				Click(944, 617);
				Log($"Клик в координаты 944, 617, итерация {i + 1}.");
			}

			Click(613, 985);
			Thread.Sleep(300);

			Click(808, 233);
			Thread.Sleep(50);

			Click(808, 233);

			Log("Проверка 'ok.jpg'");

			if (CheckCoordsForColor(1294, 732, 9429842, 11029842))
			{
				Click(985, 620);
				Log("Изображение 'ok.jpg' найдено");
			}

			Log("Проверка 'zapros.jpg'");
			if (CheckCoordsForColor(889, 371, 5000000, 6688999))
			{
				Click(808, 233);
				Log("Изображение 'zapros.jpg' найдено");
			}

			Log("Проверка 'nazad.jpg'");
			if (CheckCoordsForColor(343, 968, 2429848, 7029842))
			{
				Click(613, 985);
				Log("Изображение 'nazad.jpg' найдено");
			}
		}

		private bool CheckCoordsForColor(int x, int y, int lowerThreshold, int higherThreshold)
		{
			// Узнаем цвет пикселя
			var color = GetPixelColor(x, y);
			var number = color.ToArgb() * -1;

			Log($"Цвет пикселя: {color}, Число: {number}");

			//Проверяем диапазон
			if (number >= lowerThreshold && number <= higherThreshold)
			{
				return true;
			}

			Log("Число вне диапазона, пропуск итерации.");
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

			Log($"Клик в координаты {x}, {y}.");

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
				Fill = System.Windows.Media.Brushes.Red,
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
				Background = System.Windows.Media.Brushes.Red,
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

		private void MainWindow_OnClosed(object? sender, EventArgs e)
		{
			_hotKeyManager.Dispose();
		}
	}
}