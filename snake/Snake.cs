using System;
using System.Collections.Generic;

namespace snake
{
    struct Coordinate
    {
        public int x;
        public int y;
        public static Coordinate operator +( Coordinate a, Coordinate b ) {
            a.x += b.x;
            a.y += b.y;
            return a;
        }

        public static bool operator ==( Coordinate a, Coordinate b ) {
            return a.x == b.x && a.y == b.y;
        }
        public static bool operator !=( Coordinate a, Coordinate b ) {
            return !( a == b );
        }
    }

    enum DirectionEvent
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
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
        public Snake( int gameWidth, int gameHeight ) {
            _snakeLength = 3;
            _currentAcceleration = new Coordinate { x = 1, y = 0 }; // init acceleration goes right for now, todo: make 0 :^)
            _gameHeight = gameHeight;
            _gameWidth = gameWidth;

            _snakeGrids = new List<Coordinate>( );
            //_drawnGrids = new List<Coordinate>(); // needed to be initialized so that DrawSnake foreach doesn't crash on first iteration
        }

        public void Tick( ) {
            _currentHeadPosition += _currentAcceleration;
            DrawSnake( );
        }

        public void OnDirectionEvent( DirectionEvent e ) {
            switch ( e ) {
                case DirectionEvent.DOWN:
                    _currentAcceleration.x = 0;
                    _currentAcceleration.y = 1;
                    break;
                case DirectionEvent.LEFT:
                    _currentAcceleration.x = -1;
                    _currentAcceleration.y = 0;
                    break;
                case DirectionEvent.RIGHT:
                    _currentAcceleration.x = 1;
                    _currentAcceleration.y = 0;
                    break;
                case DirectionEvent.UP:
                    _currentAcceleration.x = 0;
                    _currentAcceleration.y = -1;
                    break;
            }
        }

        private void DrawSnake( ) {

            foreach ( var drawnGrid in _drawnGrids ) {
                if ( _snakeGrids.Exists( snakeGrid => snakeGrid == drawnGrid ) ) // still valid
                    continue;

                Console.SetCursorPosition( drawnGrid.x, drawnGrid.y );
                Console.Write( " " );
            }

            foreach ( var snakeGrid in _snakeGrids ) {
                if ( _drawnGrids.Exists( drawnGrid => drawnGrid == snakeGrid ) ) // already drawn
                    continue;

                Console.SetCursorPosition( snakeGrid.x, snakeGrid.y );
                Console.Write( "#" );
            }
            _drawnGrids = _snakeGrids; // now all snakes are drawn :)
        }
    }
}
