using System;
using System.IO;
using UCalc.Data;

namespace UCalc
{
    public partial class App
    {
        private readonly RecentlyOpenedList _recentlyOpenedList;
        public static RecentlyOpenedList RecentlyOpenedList => ((App) Current)._recentlyOpenedList;

        public App()
        {
            var appDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/UCalc";
            Directory.CreateDirectory(appDataPath);

            _recentlyOpenedList = new RecentlyOpenedList($"{appDataPath}/recently.txt");
        }
    }
}