using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snake
{
    [Serializable]
    class HighScore
    {
        public string Name { get; set; }
        public int Score { get; set; }
    }

    [Serializable]
    class HighScores
    {
        public List<HighScore> Scores;
    }
}
