using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace UCalc.Data
{
    public class RecentlyOpenedItem
    {
        public string DisplayText { get; }
        public string Path { get; }

        public RecentlyOpenedItem(string path)
        {
            DisplayText = System.IO.Path.GetFileNameWithoutExtension(path);
            Path = path;
        }

        private bool Equals(RecentlyOpenedItem other)
        {
            return DisplayText == other.DisplayText && Path == other.Path;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((RecentlyOpenedItem) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DisplayText, Path);
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }

    public class RecentlyOpenedList : ICollection<RecentlyOpenedItem>, INotifyCollectionChanged
    {
        private const int MaxCount = 10;
        private readonly string _path;
        private readonly List<RecentlyOpenedItem> _list;

        public RecentlyOpenedList(string path)
        {
            _path = path;
            _list = new List<RecentlyOpenedItem>();

            Load(path);
        }

        private void Load(string path)
        {
            try
            {
                var lines = File.ReadAllLines(path);

                _list.AddRange(lines.Where(itemPath => itemPath != "").Take(MaxCount)
                    .Select(itemPath => new RecentlyOpenedItem(itemPath)));
            }
            catch (IOException)
            {
                // Do nothing
            }
        }

        private void Store(string path)
        {
            try
            {
                File.WriteAllLines(path, _list.Select(item => item.Path));
            }
            catch (IOException)
            {
                // Do nothing
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public IEnumerator<RecentlyOpenedItem> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public void Add(RecentlyOpenedItem item)
        {
            var index = _list.IndexOf(item);
            if (index != -1)
            {
                _list.RemoveAt(index);
            }

            _list.Insert(0, item);

            if (_list.Count > MaxCount)
            {
                _list.RemoveAt(MaxCount);
            }

            Store(_path);
        }

        public void Clear()
        {
            _list.Clear();

            Store(_path);
        }

        public bool Contains(RecentlyOpenedItem item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(RecentlyOpenedItem[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(RecentlyOpenedItem item)
        {
            var result = _list.Remove(item);

            Store(_path);
            return result;
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}