using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace snake
{
    public struct Coordinate
    {
        public int x;
        public int y;

        public bool Equals( Coordinate other ) {
            return this == other;
        }

        public override bool Equals( object obj ) {
            return obj is Coordinate other && Equals( other );
        }

        public override int GetHashCode( ) {
            return HashCode.Combine( x, y );
        }

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

    public enum DirectionEvent
    {
        NONE,
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    class Snake
    {
        private List<Coordinate> _drawnGrids; // last grids that were drawn, used to make sure i don't have any leftovers
        private int _snakeLength; // snakeGrids.Length essentially but used for trimming
        private int _score;

        private Coordinate _previousFoodCoordinate = new( );

        public ReturnData returnData;

        private Coordinate _currentVelocity;

        private readonly int _gameWidth;
        private readonly int _gameHeight;

        public bool Running { get; private set; }
        public int CurrentInterval { get; private set; }
        public HighScores HighScores { get; set; }

        private DirectionEvent _lastValidDirectionEvent = DirectionEvent.NONE;
        private DirectionEvent _lastTickDirectionEvent = DirectionEvent.NONE;

        private DirectionEvent _lastDrawn = DirectionEvent.NONE;
        public Snake( int gameWidth, int gameHeight ) {
            _gameHeight = gameHeight;
            _gameWidth = gameWidth;

            // Needed to be initialized so that DrawSnake foreach doesn't crash on first iteration
            _drawnGrids = new List<Coordinate>( );

            Reset( );
        }

        public void Reset( ) {
            Running = true;

            returnData = new( );
            returnData.SnakePositions = new List<Coordinate>( );
            returnData.ShotCutMoveDirections = new List<DirectionEvent>( );
            _lastValidDirectionEvent = DirectionEvent.NONE;
            _lastTickDirectionEvent = DirectionEvent.NONE;

            _snakeLength = 10;
            _currentVelocity = new Coordinate { x = 0, y = 0 };

            returnData.SnakePositions = new List<Coordinate>( );
            // snake starts with a visual length of 3
            for ( int i = 0; i < 3; i++ ) {
                Coordinate curCoordinate = new Coordinate( ) { x = 1/*_gameWidth / 2*/, y = _gameHeight / 2 /*+ i*/ };
                returnData.SnakePositions.Add( curCoordinate );
                if ( i == 0 ) {
                    returnData.HeadPosition = curCoordinate;
                }
            }

            // Create initial food
            GenerateFood( );
        }

        public void Tick( ) {
            // Move the snakes head forward, body will follow
            returnData.HeadPosition += _currentVelocity;
            // Saving which direction we last moved to make sure we don't do illegal U turns
            _lastTickDirectionEvent = _lastValidDirectionEvent;

            // Check if we're actually moving
            if ( _currentVelocity.x != 0 || _currentVelocity.y != 0 ) {

                Action GameOver = ( ) => {
                    Running = false;
                    DrawGameOver( );
                    return;
                };

                // Check if we're colliding with ourselves
                if ( returnData.SnakePositions.Exists( grid => grid == returnData.HeadPosition ) ) {
                    GameOver( );
                }

                // Make it go from right border to left
                if ( returnData.HeadPosition.x >= _gameWidth )
                    GameOver( );
                //_currentHeadPosition.x = 1;
                // Left border to right
                if ( returnData.HeadPosition.x < 1 )
                    GameOver( );
                //_currentHeadPosition.x = _gameWidth - 1;
                // Bottom border to top
                if ( returnData.HeadPosition.y >= _gameHeight )
                    GameOver( );
                //_currentHeadPosition.y = 1;
                // Top border to bottom
                if ( returnData.HeadPosition.y < 1 )
                    GameOver( );
                //_currentHeadPosition.y = _gameHeight - 1;

                returnData.SnakePositions.Insert( 0, returnData.HeadPosition );

                // Only remove tail if snake is bigger than the it's meant to be.
                // length increases when given food
                while ( returnData.SnakePositions.Count > _snakeLength )
                    returnData.SnakePositions.RemoveAt( returnData.SnakePositions.Count - 1 );
            }

            if ( returnData.HeadPosition == returnData.ApplePosition ) {
                _snakeLength++;
                _score += 10;
                GenerateFood( );
            }

            // Display the changes
            DrawSnake( );
        }

        private void GenerateFood( ) {
            Random random = new( );
            returnData.ApplePosition = new Coordinate( ) {
                x = random.Next( 1, _gameWidth ),
                y = random.Next( 1, _gameHeight )
            };

            foreach ( var snakeGrid in returnData.SnakePositions ) {
                // If the generated food is on the snake then it's not valid, we need to re-run
                if ( snakeGrid == returnData.ApplePosition ) {
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
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write( gameOverText );
            Console.SetCursorPosition( _gameWidth / 2 - scoreText.Length / 2, _gameHeight / 2 );
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write( scoreText );

            var enterNameText = "Enter your name to save your score or hit ESC:";
            Console.SetCursorPosition( _gameWidth / 2 - enterNameText.Length / 2, _gameHeight / 2 + 2 );
            Console.Write( enterNameText );



            Console.SetCursorPosition( _gameWidth / 2 - 1, _gameHeight / 2 + 3 );
            string inputName = String.Empty;
            bool hitEnter = false;

            while ( !hitEnter ) {
                var key = Console.ReadKey( true );
                hitEnter = key.Key == ConsoleKey.Enter;

                // exit while hitEnter remains false
                if ( key.Key == ConsoleKey.Escape )
                    break;
                if ( key.Key != ConsoleKey.Backspace ) {
                    if ( inputName.Length < 30 )
                        inputName += key.KeyChar;

                    inputName = Regex.Replace( inputName, @"\s", "" ); // replaces any whitespaces, anywhere
                } else {
                    inputName = Regex.Replace( inputName, @"\s", "" ); // as above
                    StringBuilder sb = new StringBuilder( inputName );
                    sb[ ^1 ] = ' ';
                    sb.Insert( 0, ' ' );
                    inputName = sb.ToString( );
                }

                Console.SetCursorPosition( _gameWidth / 2 - ( inputName.Length / 2 ), _gameHeight / 2 + 3 );
                Console.Write( inputName );
            }

            if ( !hitEnter || inputName.Trim( ).Length == 0 )
                return;

            HighScore newScore = new( ) {
                Name = inputName.Trim( ),
                Score = _score
            };

            HighScores.Scores.Add( newScore );

            HighScores.Scores.Sort( ( a, b ) => b.Score.CompareTo( a.Score ) );

            DataSerializer dataSerializer = new( );
            dataSerializer.BinarySerialize( HighScores, "scores.bin" ); // Writes HighScores to disk
        }

        public void OnDirectionEvent( DirectionEvent e ) {
            bool verticalMovement = e == DirectionEvent.UP || e == DirectionEvent.DOWN;
            // Handle arrow keys
            switch ( e ) {
                case DirectionEvent.DOWN:
                    // Prohibit random 180 movements
                    if ( _lastTickDirectionEvent != DirectionEvent.UP ) {
                        _currentVelocity.x = 0;
                        _currentVelocity.y = 1;
                        _lastValidDirectionEvent = e;
                    }
                    break;
                case DirectionEvent.LEFT:
                    // Prohibit random 180 movements
                    if ( _lastTickDirectionEvent != DirectionEvent.RIGHT ) {
                        _currentVelocity.x = -1;
                        _currentVelocity.y = 0;
                        _lastValidDirectionEvent = e;
                    }
                    break;
                case DirectionEvent.RIGHT:
                    // Prohibit random 180 movements
                    if ( _lastTickDirectionEvent != DirectionEvent.LEFT ) {
                        _currentVelocity.x = 1;
                        _currentVelocity.y = 0;
                        _lastValidDirectionEvent = e;
                    }
                    break;
                case DirectionEvent.UP:
                    // Prohibit random 180 movements
                    if ( _lastTickDirectionEvent != DirectionEvent.DOWN ) {
                        _currentVelocity.x = 0;
                        _currentVelocity.y = -1;
                        _lastValidDirectionEvent = e;
                    }
                    break;
            }
            // Slowing the game down when moving vertically because
            // there's a much greater distance between characters vertically
            // making it feel a lot faster
            //CurrentInterval = verticalMovement ? 1000 / 50 : 1000 / 75;
            CurrentInterval = 1;
        }

        private void DrawHamiltonian( DirectionEvent curEvent ) {
            if ( _lastDrawn == DirectionEvent.NONE ) {
                _lastDrawn = curEvent;
            }
            switch ( curEvent ) {
                case DirectionEvent.UP:
                    switch ( _lastDrawn ) {
                        case DirectionEvent.UP:
                            Console.Write( '║' );
                            break;
                        case DirectionEvent.RIGHT:
                            Console.Write( '╝' );
                            break;
                        case DirectionEvent.DOWN:
                            Console.Write( "ERROR" );
                            break;
                        case DirectionEvent.LEFT:
                            Console.Write( '╚' );
                            break;
                    }
                    break;
                case DirectionEvent.RIGHT:
                    switch ( _lastDrawn ) {
                        case DirectionEvent.UP:
                            Console.Write( '╔' );
                            break;
                        case DirectionEvent.DOWN:
                            Console.Write( '╚' );
                            break;
                        case DirectionEvent.LEFT:
                            Console.Write( "ERROR" );
                            break;
                        case DirectionEvent.RIGHT:
                            Console.Write( "═" );
                            break;
                    }
                    break;
                case DirectionEvent.DOWN:
                    switch ( _lastDrawn ) {
                        case DirectionEvent.UP:
                            Console.Write( "ERROR" );
                            break;
                        case DirectionEvent.RIGHT:
                            Console.Write( '╗' );
                            break;
                        case DirectionEvent.LEFT:
                            Console.Write( '╔' );
                            break;
                        case DirectionEvent.DOWN:
                            Console.Write( '║' );
                            break;
                    }
                    break;
                case DirectionEvent.LEFT:
                    switch ( _lastDrawn ) {
                        case DirectionEvent.UP:
                            Console.Write( '╗' );
                            break;
                        case DirectionEvent.RIGHT:
                            Console.Write( "ERROR" );
                            break;
                        case DirectionEvent.DOWN:
                            Console.Write( '╝' );
                            break;
                        case DirectionEvent.LEFT:
                            Console.Write( "═" );
                            break;
                    }
                    break;
            }
            _lastDrawn = curEvent;
        }

        private bool _drawnHamilton = false;
        private void DrawSnake( ) {
            Console.ForegroundColor = ConsoleColor.White;

            var hamiltonianCycle = Program.program.HamiltonianCycleData;
            if ( !_drawnHamilton ) {
                _lastDrawn = DirectionEvent.NONE;
                foreach ( var point in hamiltonianCycle.Data.SequenceNumberToPoint ) {
                    Console.SetCursorPosition( point.X, point.Y );
                    DrawHamiltonian( hamiltonianCycle.Data.MoveDirections[ point.X, point.Y ] );
                }
                _drawnHamilton = true;
            }

            _lastDrawn = DirectionEvent.NONE;
            foreach ( var drawnGrid in _drawnGrids ) {
                // Still valid
                if ( returnData.SnakePositions.Exists( snakeGrid => snakeGrid == drawnGrid ) )
                    continue;

                // Overwrites the drawn snake with a space
                Console.SetCursorPosition( drawnGrid.x, drawnGrid.y );

                var currentMove = hamiltonianCycle.Data.MoveDirections[ drawnGrid.x, drawnGrid.y ];

                DrawHamiltonian( currentMove );
                //Console.Write( Program.program.HamiltonianCycleData.Data.PointToSequenceNumber[ drawnGrid.x, drawnGrid.y ] );
            }

            foreach ( var snakeGrid in returnData.SnakePositions ) {
                // Already drawn
                if ( _drawnGrids.Exists( drawnGrid => drawnGrid == snakeGrid ) )
                    continue;

                // Writes a # at the new snake position
                Console.SetCursorPosition( snakeGrid.x, snakeGrid.y );
                Console.Write( "#" );
            }


            // Drawing food
            if ( _previousFoodCoordinate != returnData.ApplePosition ) {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.SetCursorPosition( returnData.ApplePosition.x, returnData.ApplePosition.y );
                Console.Write( "#" );
                _previousFoodCoordinate = returnData.ApplePosition;
            }



            // Have to make a new copy of snakeGrids otherwise it uses a reference by default :(
            _drawnGrids = new List<Coordinate>( returnData.SnakePositions ); // Now all snake grids are drawn :)
        }
    }
}
