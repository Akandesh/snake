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
        LEFT,
        NONE
    }

    class Snake
    {
        private List<Coordinate> _snakeGrids; // grids where the snake is currently inside
        private List<Coordinate> _drawnGrids; // last grids that were drawn, used to make sure i don't have any leftovers
        private int _snakeLength; // snakeGrids.Length essentially but used for trimming
        private int _score;

        private Coordinate _foodCoordinate;
        private Coordinate _previousFoodCoordinate = new( );

        private Coordinate _currentAcceleration;
        private Coordinate _currentHeadPosition;

        private readonly int _gameWidth;
        private readonly int _gameHeight;

        public bool Running { get; private set; }

        private DirectionEvent _lastValidDirectionEvent = DirectionEvent.NONE;
        public Snake( int gameWidth, int gameHeight ) {
            Running = true;

            _snakeLength = 10;
            _currentAcceleration = new Coordinate { x = 0, y = 0 };
            _gameHeight = gameHeight;
            _gameWidth = gameWidth;

            _snakeGrids = new List<Coordinate>( );
            for ( int i = 0; i < 3; i++ ) {
                Coordinate curCoordinate = new Coordinate( ) { x = gameWidth / 2, y = gameHeight / 2 + i };
                if ( i == 0 ) {
                    _currentHeadPosition = curCoordinate;
                }
                _snakeGrids.Add( curCoordinate );
            }

            // Needed to be initialized so that DrawSnake foreach doesn't crash on first iteration
            _drawnGrids = new List<Coordinate>( );

            // Create initial food
            GenerateFood( );
        }

        public void Tick( ) {
            // Move the snakes head forward, body will follow
            _currentHeadPosition += _currentAcceleration;

            // Check if we're actually moving
            if ( _currentAcceleration.x != 0 || _currentAcceleration.y != 0 ) {

                // Check if we're colliding with ourselves
                if ( _snakeGrids.Exists( grid => grid == _currentHeadPosition ) ) {
                    Running = false;
                    DrawGameOver( );
                    return;
                }

                // Make it go from right border to left
                if ( _currentHeadPosition.x >= _gameWidth )
                    _currentHeadPosition.x = 1;
                // Left border to right
                if ( _currentHeadPosition.x < 1 )
                    _currentHeadPosition.x = _gameWidth - 1;
                // Bottom border to top
                if ( _currentHeadPosition.y >= _gameHeight )
                    _currentHeadPosition.y = 1;
                // Top border to bottom
                if ( _currentHeadPosition.y < 1 )
                    _currentHeadPosition.y = _gameHeight - 1;

                _snakeGrids.Insert( 0, _currentHeadPosition );

                // Only remove tail if snake is bigger than the it's meant to be.
                // length increases when given food
                while ( _snakeGrids.Count > _snakeLength )
                    _snakeGrids.RemoveAt( _snakeGrids.Count - 1 );
            }

            if ( _currentHeadPosition == _foodCoordinate ) {
                _snakeLength++;
                _score += 10;
                GenerateFood( );
            }

            // Display the changes
            DrawSnake( );
        }

        private void GenerateFood( ) {
            Random random = new( );
            _foodCoordinate = new Coordinate( ) {
                x = random.Next( 1, _gameWidth ),
                y = random.Next( 1, _gameHeight )
            };

            foreach ( var snakeGrid in _snakeGrids ) {
                // If the generated food is on the snake then it's not valid, we need to re-run
                if ( snakeGrid == _foodCoordinate ) {
                    // infinite loop when we win :^)
                    GenerateFood( );
                    return;
                }
            }
        }

        void DrawGameOver( ) {
            int innerWidth = _gameWidth / 4;
            int innerHeight = _gameHeight / 4;

            // Clearing the center region of the screen
            for ( int y = innerHeight; y < innerHeight * 3; y++ ) {
                for ( int x = innerWidth; x < innerWidth * 3; x++ ) {
                    Console.SetCursorPosition( x, y );
                    Console.Write( " " );
                }
            }

            var gameOverText = "Game Over";
            var scoreText = $"Your score was: {_score}";
            Console.SetCursorPosition( _gameWidth / 2 - gameOverText.Length / 2, _gameHeight / 2 - 2 );
            Console.Write( gameOverText );
            Console.SetCursorPosition( _gameWidth / 2 - scoreText.Length / 2, _gameHeight / 2 );
            Console.Write( scoreText );
        }

        public void OnDirectionEvent( DirectionEvent e ) {

            // Handle arrow keys
            switch ( e ) {
                case DirectionEvent.DOWN:
                    // Prohibit random 180 movements
                    if ( _lastValidDirectionEvent != DirectionEvent.UP ) {
                        _currentAcceleration.x = 0;
                        _currentAcceleration.y = 1;
                        _lastValidDirectionEvent = e;
                    }
                    break;
                case DirectionEvent.LEFT:
                    // Prohibit random 180 movements
                    if ( _lastValidDirectionEvent != DirectionEvent.RIGHT ) {
                        _currentAcceleration.x = -1;
                        _currentAcceleration.y = 0;
                        _lastValidDirectionEvent = e;
                    }
                    break;
                case DirectionEvent.RIGHT:
                    // Prohibit random 180 movements
                    if ( _lastValidDirectionEvent != DirectionEvent.LEFT ) {
                        _currentAcceleration.x = 1;
                        _currentAcceleration.y = 0;
                        _lastValidDirectionEvent = e;
                    }
                    break;
                case DirectionEvent.UP:
                    // Prohibit random 180 movements
                    if ( _lastValidDirectionEvent != DirectionEvent.DOWN ) {
                        _currentAcceleration.x = 0;
                        _currentAcceleration.y = -1;
                        _lastValidDirectionEvent = e;
                    }
                    break;
            }
        }

        private void DrawSnake( ) {
            Console.ForegroundColor = ConsoleColor.White;
            foreach ( var drawnGrid in _drawnGrids ) {
                // Still valid
                if ( _snakeGrids.Exists( snakeGrid => snakeGrid == drawnGrid ) )
                    continue;

                // Overwrites the drawn snake with a space
                Console.SetCursorPosition( drawnGrid.x, drawnGrid.y );
                Console.Write( " " );
            }

            foreach ( var snakeGrid in _snakeGrids ) {
                // Already drawn
                if ( _drawnGrids.Exists( drawnGrid => drawnGrid == snakeGrid ) )
                    continue;

                // Writes a # at the new snake position
                Console.SetCursorPosition( snakeGrid.x, snakeGrid.y );
                Console.Write( "#" );
            }

            // Drawing food
            if ( _previousFoodCoordinate != _foodCoordinate ) {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.SetCursorPosition( _foodCoordinate.x, _foodCoordinate.y );
                Console.Write( "#" );
                _previousFoodCoordinate = _foodCoordinate;
            }

            // Have to make a new copy of snakeGrids otherwise it uses a reference by default :(
            _drawnGrids = new List<Coordinate>( _snakeGrids ); // Now all snake grids are drawn :)
        }
    }
}
