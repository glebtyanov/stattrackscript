using GlobalHotKeys;
using GlobalHotKeys.Native.Types;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Size = System.Drawing.Size;

namespace Scriptrunok
{
    public partial class MainWindow
    {
        #region Initialization

        private bool _isRunning;
        private readonly HotKeyManager _hotKeyManager;
        private CancellationTokenSource? _buyClickerTokenSource;
        private readonly Config _config;

        public MainWindow()
        {
            InitializeComponent();

            // creating config from defaults.json
            _config = new Config();

            DelayAfterBuyTextBox.Text = _config.SleepConfig.kpSleep.ToString();
            RefreshRateTextBox.Text = _config.SleepConfig.updSleep.ToString();

            // for hotkeys to work globally
            _hotKeyManager = new HotKeyManager();
            _hotKeyManager.Register(VirtualKeyCode.VK_F6, Modifiers.NoRepeat);
            _hotKeyManager.Register(VirtualKeyCode.VK_F7, Modifiers.NoRepeat);
            _hotKeyManager.HotKeyPressed.Subscribe((Action<HotKey>)StartAction);

            User32.AlwaysOnTop(Title);
        }

        #endregion

        #region StartLogic

        private void StartThreads()
        {
            _buyClickerTokenSource = new CancellationTokenSource();
            Task.WhenAll(
                Task.Run(() => BuyClicker(_buyClickerTokenSource.Token)),
                Task.Run(() => RefreshClicker(_buyClickerTokenSource.Token))
            ).ContinueWith(_ => { StartAction(isRestart: true); },
                TaskContinuationOptions.None);
        }

        private void StartAction(HotKey? _ = null)
        {
            try
            {
                if (_isRunning)
                {
                    Dispatcher.Invoke(() =>
                    {
                        IsStarted.Text = "[ОСТАНОВЛЕН]";
                        StartButton.Content = "Старт";
                    });
                    _isRunning = false;
                    _buyClickerTokenSource!.Cancel();

                    return;
                }

                _isRunning = true;
                Dispatcher.Invoke(() =>
                {
                    IsStarted.Text = "[ЗАПУЩЕН]";
                    StartButton.Content = "Стоп";
                });

                StartThreads();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void StartAction(bool isRestart = false)
        {
            try
            {
                if (_isRunning && isRestart)
                {
                    _buyClickerTokenSource!.Cancel();
                    StartThreads();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartAction(null);
        }

        #endregion

        #region Clickers

        private async Task RefreshClicker(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Click(_config.ButtonConfig.Upd.Xy);
                await Task.Delay(_config.SleepConfig.upSleep, cancellationToken);
                Click(_config.ButtonConfig.Upd.Xy);
                await Task.Delay(_config.SleepConfig.updSleep, cancellationToken);
            }
        }

        private async Task BuyClicker(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var lot in _config.ButtonConfig.Lots)
                {
                    // step 1
                    if (!CheckCoordsForColor(lot.Nakl, lot.Cvet))
                        continue;

                    Click(lot.Nakl);
                    await Task.Delay(_config.SleepConfig.kpSleep, cancellationToken);

                    for (int i = 0; i < 3; i++)
                    {
                        Click(_config.ButtonConfig.Podtverdit.Xy);
                    }

                    // 2
                    await Task.Delay(_config.SleepConfig.pdSleep, cancellationToken);

                    // 3
                    Click(_config.ButtonConfig.Nazad.Xy);
                    await Task.Delay(_config.SleepConfig.nazadSleep, cancellationToken);

                    Click(_config.ButtonConfig.Upd.Xy);
                    await Task.Delay(_config.SleepConfig.upSleep, cancellationToken);
                    Click(_config.ButtonConfig.Upd.Xy);

                    // 4
                    if (!CheckCoordsForColor(_config.ButtonConfig.Upd.Xy, _config.ButtonConfig.Upd.Cvet))
                    {
                        Click(_config.ButtonConfig.Upd.Xy);
                        await Task.Delay(300, cancellationToken);
                    }

                    // 5
                    if (CheckCoordsForColor(_config.ButtonConfig.Nazad.Xy, _config.ButtonConfig.Nazad.Cvet))
                    {
                        Click(_config.ButtonConfig.Nazad.Xy);
                        await Task.Delay(300, cancellationToken);
                        Click(_config.ButtonConfig.Upd.Xy);
                        await Task.Delay(_config.SleepConfig.upSleep, cancellationToken);
                        Click(_config.ButtonConfig.Upd.Xy);
                    }

                    // 6
                    if (CheckCoordsForColor(_config.ButtonConfig.Ok.Xy, _config.ButtonConfig.Ok.Cvet))
                    {
                        Click(_config.ButtonConfig.Ok.Xy);
                        await Task.Delay(300, cancellationToken);
                        Click(_config.ButtonConfig.Upd.Xy);
                        await Task.Delay(_config.SleepConfig.upSleep, cancellationToken);
                        Click(_config.ButtonConfig.Upd.Xy);
                    }
                }
            }
        }

        #endregion

        #region Color detect + Click logic

        // Used to get color of a pixel
        private static readonly Bitmap ScreenPixel = new(1, 1, PixelFormat.Format32bppArgb);
        private static readonly Graphics Graphics = Graphics.FromImage(ScreenPixel);

        private static int GetColorAt(int x, int y)
        {
            // Use a lock statement to ensure thread-safety
            lock (ScreenPixel)
            {
                Graphics.CopyFromScreen(x, y, 0, 0, new Size(1, 1), CopyPixelOperation.SourceCopy);
                return ScreenPixel.GetPixel(0, 0).ToArgb() * -1;
            }
        }

        private static bool CheckCoordsForColor(IReadOnlyList<int> xy, IReadOnlyList<int> range)
        {
            var number = GetColorAt(xy[0], xy[1]);
            if (range.Count <= 2) return number >= range[0] && number <= range[1];

            // Second check for sticker cases (they have 2 color ranges)  
            return (number >= range[0] && number <= range[1]) || (number >= range[2] && number <= range[3]);
        }

        private void Click(int[] xy)
        {
            SetCursorPos(xy[0], xy[1]);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, xy[0], xy[1], 0, 0);
        }

        // Windows functions and consts
        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        #endregion

        #region WPF Logic

        private void MainWindow_OnClosed(object? sender, EventArgs e)
        {
            _hotKeyManager.Dispose();
            Graphics.Dispose();
        }

        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }


        private void DelayAfterBuyTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (int.TryParse(DelayAfterBuyTextBox.Text, out int delay))
                {
                    _config.SleepConfig.kpSleep = delay;
                }
            });
        }

        private void RefreshRateTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (int.TryParse(RefreshRateTextBox.Text, out int delay))
                {
                    _config.SleepConfig.updSleep = delay;
                }
            });
        }

        #endregion
    }
}