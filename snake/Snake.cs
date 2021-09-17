using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snake
{
    struct Coordinate
    {
        public int x;
        public int y;
        public static Coordinate operator +(Coordinate a, Coordinate b) {
            a.x += b.x;
            a.y += b.y;
            return a;
        }

        public static bool operator ==(Coordinate a, Coordinate b) {
            return a.x == b.x && a.y == b.y;
        }
        public static bool operator !=(Coordinate a, Coordinate b) {
            return !(a == b);
        }
    }

    class Snake
    {
        private List<Coordinate> _snakeGrids; // grids where the snake is currently inside
        private List<Coordinate> _drawnGrids; // last grids that were drawn, used to make sure i don't have any leftovers
        private int _snakeLength; // snakeGrids.Length essentially but used for trimming

        private Coordinate _currentAcceleration;
        private Coordinate _currentHeadPosition;
        private int _gameWidth;
        private int _gameHeight;
        public Snake(int gameWidth, int gameHeight) {
            _snakeLength = 3;
            _currentAcceleration = new Coordinate { x = 1, y = 0 };
            _gameHeight = gameHeight;
            _gameWidth = gameWidth;
        }

        public void Tick() {
            _currentHeadPosition += _currentAcceleration;


            DrawSnake();
        }

        private void DrawSnake() {

            foreach (var drawnGrid in _drawnGrids)
            {
                if (_snakeGrids.Exists(snakeGrid => snakeGrid == drawnGrid)) // still valid
                    continue;

                Console.SetCursorPosition(drawnGrid.x + 1, drawnGrid.y + 1);
                Console.Write(" ");
            }

            foreach (var snakeGrid in _snakeGrids) {
                if (_drawnGrids.Exists(drawnGrid => drawnGrid == snakeGrid)) // already drawn
                    continue;

                Console.SetCursorPosition(snakeGrid.x + 1, snakeGrid.y + 1);
                Console.Write("#");
            }
            _drawnGrids = _snakeGrids; // now all snakes are drawn :)
        }
    }
}
