using System;
using System.IO;
using System.Threading;
using UCalc.Data;
using UCalc.Models;

namespace UCalc
{
    public class Autosaver
    {
        private readonly string _runningPath;
        private SaverThread _saverThread;
        public string AutosavePath { get; }
        public bool CanRecover { get; private set; }

        public Model OpenModel
        {
            set
            {
                CanRecover = false;

                if (value != null)
                {
                    _saverThread = new SaverThread(15, AutosavePath, value);
                }
                else if (_saverThread != null)
                {
                    _saverThread.Terminate();
                    _saverThread = null;
                }
            }
        }

        public Autosaver(string runningPath, string autosavePath)
        {
            _runningPath = runningPath;
            AutosavePath = autosavePath;

            CanRecover = File.Exists(runningPath) && File.Exists(autosavePath);

            File.WriteAllText(runningPath, "");
        }

        public void OnExit()
        {
            if (!CanRecover)
            {
                try
                {
                    File.Delete(_runningPath);
                }
                catch (IOException)
                {
                    // Do nothing
                }
            }
        }

        private class SaverThread
        {
            private volatile bool _terminate;
            private readonly int _saveIntervalInS;
            private readonly string _autosavePath;
            private readonly Model _model;

            public SaverThread(int saveIntervalInS, string autosavePath, Model model)
            {
                _terminate = false;
                _saveIntervalInS = saveIntervalInS;
                _autosavePath = autosavePath;
                _model = model;

                new Thread(ThreadFunc).Start();
            }

            public void Terminate()
            {
                _terminate = true;
            }

            private void ThreadFunc()
            {
                var start = DateTime.Now;

                while (!_terminate)
                {
                    Thread.Sleep(500);

                    if ((DateTime.Now - start).Seconds >= _saveIntervalInS)
                    {
                        try
                        {
                            BillingLoader.Store(_autosavePath, _model.Dump());
                        }
                        catch (Exception)
                        {
                            // Do nothing
                        }

                        start = DateTime.Now;
                    }
                }
            }
        }
    }
}