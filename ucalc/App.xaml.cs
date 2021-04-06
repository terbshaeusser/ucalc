using System;
using System.IO;
using System.Windows;
using UCalc.Data;

namespace UCalc
{
    public partial class App
    {
        private readonly RecentlyOpenedList _recentlyOpenedList;
        private readonly Autosaver _autosaver;
        public static RecentlyOpenedList RecentlyOpenedList => ((App) Current)._recentlyOpenedList;
        public static Autosaver Autosaver => ((App) Current)._autosaver;

        public App()
        {
            var appDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/UCalc";
            Directory.CreateDirectory(appDataPath);

            _recentlyOpenedList = new RecentlyOpenedList($"{appDataPath}/recently.txt");
            _autosaver = new Autosaver($"{appDataPath}/running.txt", $"{appDataPath}/autosave.mr");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _autosaver.OnExit();

            base.OnExit(e);
        }
    }
}