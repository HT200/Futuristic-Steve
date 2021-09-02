using System;
using System.Collections.Generic;
using System.Text;

namespace Futuristic_Steve
{
    class SortedScores
    {
        private List<Score> list = new List<Score>();

        public int Count
        {
            get 
            { 
                if (list == null)
                {
                    return 0;
                }
                else
                {
                    return list.Count;
                }
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (list.Count == 0)
                {
                    return true;
                }
                else if (list == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public Score this[int i]
        {
            get
            {
                if (i < 0 || i >= Count)
                {
                    throw new IndexOutOfRangeException(
                        "Your index is bad.");
                }

                return list[i];
            }
        }

        public void Add(Score newData)
        {
            if (IsEmpty)
            {
                list.Add(newData);
            }
            else
            {
                bool added = false;
                for (int i = 0; i < Count; i++)
                {
                    if (newData.ScoreNumber >= list[i].ScoreNumber)
                    {
                        list.Insert(i, newData);
                        added = true;
                    }
                    if (added)
                    {
                        break;
                    }
                }
                if (!added)
                {
                    list.Add(newData);
                }
            }
            if (Count > 10)
            {
                list.RemoveAt(10);
            }
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(Score data)
        {
            foreach (Score s in list)
            {
                if (data.ScoreNumber == s.ScoreNumber)
                {
                    return true;
                }
            }
            return false;
        }

        public Score Min()
        {
            if (!IsEmpty)
            {
                return list[Count - 1];
            }
            else
            {
                return default;
            }
        }

        public Score Max()
        {
            if (!IsEmpty)
            {
                return list[0];
            }
            else
            {
                return default;
            }
        }
    }
}
