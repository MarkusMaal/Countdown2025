using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System;
using System.Globalization;

namespace Countdown2025
{
    public partial class MainWindow : Window
    {
        readonly DispatcherTimer timer = new();
        bool canClose = false;
        long totalDelay = 0;
        long tickCount = 0;
        bool testMode = false;
        int plusHours = 0;
        int plusMins = 0;

        // localizations (not the best way of storing these strings, but it'll do for such a simple application)
        string topLabelText = "Järgmise aastani on jäänud:";
        string[] lPluralTimeMeasurements = ["päeva", "tundi", "minutit", "sekundit"];
        string[] lSingularTimeMeasurements = ["päev", "tund", "minut", "sekund"];
        string happyNewYear = "Head uut aastat!";
        string title = "Uue aasta taimer (viivis: {0}ms)";
        string avgDelay = "Keskmine viivis on {0}ms";
        string delay = "Viivis";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // show English strings if display language is not Estonian
            if (!CultureInfo.CurrentCulture.EnglishName.Contains("estonian", StringComparison.CurrentCultureIgnoreCase))
            {
                topLabelText = "Time until next year:";
                lPluralTimeMeasurements = ["days", "hours", "minutes", "seconds"];
                lSingularTimeMeasurements = ["day", "hour", "minute", "second"];
                happyNewYear = "Happy new year!";
                title = "New year timer (delay: {0}ms)";
                avgDelay = "Average delay is {0}ms";
                delay = "Delay";
            }
            TopLabel.Content = topLabelText;
            this.Background = new SolidColorBrush(Color.FromArgb(162, 0, 0, 0));
            bool isOpaque = true;
            bool yearLock = false;
            bool firstPass = true;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            Random r = new();
            timer.Tick += (object? sender, EventArgs e) =>
            {
                DateTime now = DateTime.Now;
                DateTime nextyear = new(now.Year + 1, 1, 1, 0, 0, 0);
                TimeSpan delta = nextyear.Subtract(now);
                if (testMode)
                {
                    now = now.AddDays(delta.Days);
                    now = now.AddHours(plusHours);
                    now = now.AddMinutes(plusMins);
                    delta = nextyear.Subtract(now);
                }
                if (yearLock && (delta <= new TimeSpan(0)))
                {
                    SolidColorBrush bg = new(Color.FromArgb(162, (byte)r.Next(0, 255), (byte)r.Next(0, 255), (byte)r.Next(0, 255)));
                    this.Background = bg;
                    this.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, (byte)(255 - bg.Color.R), (byte)(255 - bg.Color.G), (byte)(255 - bg.Color.B)));
                    DDays.IsVisible = false;
                    DHours.IsVisible = false;
                    DMins.IsVisible = false;
                    DSecs.IsVisible = false;
                    LDays.IsVisible = false;
                    LHours.IsVisible = false;
                    LMins.IsVisible = false;
                    LSecs.IsVisible = false;
                    YearLabel.IsVisible = true;
                    YearLabel.Content = now.Year;
                    TopLabel.Content = happyNewYear;
                    return;
                }
                isOpaque = !isOpaque;
                DDays.Content = delta.Days;
                DHours.Content = delta.Hours;
                DMins.Content = delta.Minutes;
                DSecs.Content = delta.Seconds;
                if (delta.Days == 0)
                {
                    yearLock = true;
                }
                LDays.Content = lPluralTimeMeasurements[0];
                LHours.Content = lPluralTimeMeasurements[1];
                LMins.Content = lPluralTimeMeasurements[2];
                LSecs.Content = lPluralTimeMeasurements[3];
                if (delta.Days == 1) { LDays.Content = lSingularTimeMeasurements[0]; }
                if (delta.Hours == 1) { LHours.Content = lSingularTimeMeasurements[1]; }
                if (delta.Minutes == 1) { LMins.Content = lSingularTimeMeasurements[2]; }
                if (delta.Seconds == 1) { LSecs.Content = lSingularTimeMeasurements[3]; }
                if (!firstPass)
                {
                    tickCount++;
                    totalDelay += (long)(1000 - timer.Interval.TotalMilliseconds);
                }
                firstPass = false;
                timer.Interval = new TimeSpan(0, 0, 0, 0, 1000 - now.Millisecond);
                this.Title = string.Format(title, 1000 - timer.Interval.TotalMilliseconds);
            };
            timer.Start();
            if (this.Screens.Primary == null) { return; }
            this.Position = new Avalonia.PixelPoint((int)(this.Screens.Primary.Bounds.Width - this.Width - 20), 20);
        }

        private void Window_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
        {
            e.Cancel = !canClose;
        }

        async private void Window_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.F7)
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard(delay, string.Format(avgDelay, (totalDelay / tickCount)),
                ButtonEnum.Ok);

                await box.ShowAsync();
            } else if (e.Key == Avalonia.Input.Key.F6)
            {
                testMode = !testMode;
                if (!testMode)
                {
                    this.Background = new SolidColorBrush(Color.FromArgb(162, 0, 0, 0));
                    this.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    DDays.IsVisible = true;
                    DHours.IsVisible = true;
                    DMins.IsVisible = true;
                    DSecs.IsVisible = true;
                    LDays.IsVisible = true;
                    LHours.IsVisible = true;
                    LMins.IsVisible = true;
                    LSecs.IsVisible = true;
                    TopLabel.Content = topLabelText;
                    YearLabel.IsVisible = false;
                }
            } else if (e.Key == Avalonia.Input.Key.OemPlus)
            {
                if (e.KeyModifiers == Avalonia.Input.KeyModifiers.Control)
                {
                    plusMins++;
                }
                else
                {
                    plusHours++;
                }
            } else if (e.Key == Avalonia.Input.Key.OemMinus)
            {
                if (e.KeyModifiers == Avalonia.Input.KeyModifiers.Control)
                {
                    plusMins--;
                }
                else
                {
                    plusHours--;
                }
            }
            else if ((e.Key == Avalonia.Input.Key.C) && (e.KeyModifiers == Avalonia.Input.KeyModifiers.Control))
            {
                canClose = true;
                Close();
            }
        }

        private void Window_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (e.Pointer.IsPrimary)
            {
                this.BeginMoveDrag(e);
            }
        }
    }
}