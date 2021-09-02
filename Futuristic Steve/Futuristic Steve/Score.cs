using System;
using System.Collections.Generic;
using System.Text;

namespace Futuristic_Steve
{
    struct Score
    {
        private string name;
        private int scoreNumber;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int ScoreNumber
        {
            get { return scoreNumber; }
            set { scoreNumber = value; }
        }

        public Score(string name, int scoreNumber)
        {
            this.name = name;
            this.scoreNumber = scoreNumber;
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}", name, scoreNumber);
        }
    }
}
