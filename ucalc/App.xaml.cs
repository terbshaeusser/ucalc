using UCalc.Data;

namespace UCalc
{
    public partial class App
    {
        private readonly RecentlyOpenedList _recentlyOpenedList;
        public static RecentlyOpenedList RecentlyOpenedList => ((App) Current)._recentlyOpenedList;

        public App()
        {
            _recentlyOpenedList = new RecentlyOpenedList("recently.txt");
        }
    }
}