using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;

namespace snake
{
    public enum DirectionEvent
    {
        NONE,
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

   
    class Snake
    {
        private List<Point> _drawnGrids; // last grids that were drawn, used to make sure i don't have any leftovers
        private int _snakeLength; // snakeGrids.Length essentially but used for trimming
        private int _score;

        private Point _previousFoodPoint = new( );

        public ReturnData returnData;

        private Point _currentVelocity;

        private readonly int _gameWidth;
        private readonly int _gameHeight;

        public bool Running { get; private set; }
        public int CurrentInterval { get; private set; }
        public HighScores HighScores { get; set; }

        private DirectionEvent _lastValidDirectionEvent = DirectionEvent.NONE;
        private DirectionEvent _lastTickDirectionEvent = DirectionEvent.NONE;

        private DirectionEvent _lastDrawn = DirectionEvent.NONE;

        private Dictionary<Point, char> _PointToHamChar;

        public Snake( int gameWidth, int gameHeight ) {
            _gameHeight = gameHeight;
            _gameWidth = gameWidth;

            // Needed to be initialized so that DrawSnake foreach doesn't crash on first iteration
            _drawnGrids = new List<Point>( );

            _PointToHamChar = new Dictionary<Point, char>( );

            Reset( );
        }

        public void CombinePoints( ref Point a, in Point b ) {
            a.Y += b.Y;
            a.X += b.X;
        }

        public void Reset( ) {
            Running = true;
            _drawnHamilton = false;

            returnData = new( );
            returnData.SnakePositions = new List<Point>( );
            returnData.ShotCutMoveDirections = new List<DirectionEvent>( );
            _lastValidDirectionEvent = DirectionEvent.NONE;
            _lastTickDirectionEvent = DirectionEvent.NONE;

            _snakeLength = 10;
            _currentVelocity = new Point { X = 0, Y = 0 };

            returnData.SnakePositions = new List<Point>( );
            // snake starts with a visual length of 3
            for ( int i = 0; i < 3; i++ ) {
                Point curPoint = new Point( ) { X = 1/*_gameWidth / 2*/, Y = _gameHeight / 2 /*+ i*/ };
                returnData.SnakePositions.Add( curPoint );
                if ( i == 0 ) {
                    returnData.HeadPosition = curPoint;
                }
            }

            // Create initial food
            GenerateFood( );
        }

        public void Tick( ) {
            // Move the snakes head forward, body will follow
            CombinePoints( ref returnData.HeadPosition, _currentVelocity );
            // Saving which direction we last moved to make sure we don't do illegal U turns
            _lastTickDirectionEvent = _lastValidDirectionEvent;

            // Check if we're actually moving
            if ( _currentVelocity.X != 0 || _currentVelocity.Y != 0 ) {

                Action GameOver = ( ) => {
                    Running = false;
                    DrawGameOver( );
                    return;
                };

                // Check if we're colliding with ourselves
                if ( returnData.SnakePositions.Exists( grid => grid == returnData.HeadPosition ) ) {
                    GameOver( );
                    return;
                }

                // Make it go from right border to left
                if ( returnData.HeadPosition.X >= _gameWidth + 1 ) {
                    GameOver( );
                    return;
                }
                //_currentHeadPosition.X = 1;
                // Left border to right
                if ( returnData.HeadPosition.X < 1 ) {
                    GameOver( );
                    return;
                }
                //_currentHeadPosition.X = _gameWidth - 1;
                // Bottom border to top
                if ( returnData.HeadPosition.Y >= _gameHeight + 1 ) {
                    GameOver( );
                    return;
                }
                //_currentHeadPosition.Y = 1;
                // Top border to bottom
                if ( returnData.HeadPosition.Y < 1 ) {
                    GameOver( );
                    return;
                }
                //_currentHeadPosition.Y = _gameHeight - 1;

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
            returnData.ApplePosition = new Point( ) {
                X = random.Next( 1, _gameWidth ),
                Y = random.Next( 1, _gameHeight )
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


        public bool DirectionValid( DirectionEvent e ) {
            switch ( e ) {
                case DirectionEvent.DOWN:
                    // Prohibit random 180 movements
                    if ( _lastTickDirectionEvent == DirectionEvent.UP ) {
                        return false;
                    }
                    break;
                case DirectionEvent.LEFT:
                    // Prohibit random 180 movements
                    if ( _lastTickDirectionEvent == DirectionEvent.RIGHT ) {
                        return false;
                    }
                    break;
                case DirectionEvent.RIGHT:
                    // Prohibit random 180 movements
                    if ( _lastTickDirectionEvent == DirectionEvent.LEFT ) {
                        return false;
                    }
                    break;
                case DirectionEvent.UP:
                    // Prohibit random 180 movements
                    if ( _lastTickDirectionEvent == DirectionEvent.DOWN ) {
                        return false;
                    }
                    break;
            }
            return true;
        }

        public void OnDirectionEvent( DirectionEvent e ) {
            bool verticalMovement = e == DirectionEvent.UP || e == DirectionEvent.DOWN;
            // Handle arrow keys

            if ( DirectionValid( e ) ) {
                switch ( e ) {
                    case DirectionEvent.DOWN:
                        _currentVelocity.X = 0;
                        _currentVelocity.Y = 1;
                        _lastValidDirectionEvent = e;
                        break;
                    case DirectionEvent.LEFT:
                        _currentVelocity.X = -1;
                        _currentVelocity.Y = 0;
                        _lastValidDirectionEvent = e;

                        break;
                    case DirectionEvent.RIGHT:
                        _currentVelocity.X = 1;
                        _currentVelocity.Y = 0;
                        _lastValidDirectionEvent = e;
                        break;
                    case DirectionEvent.UP:
                        _currentVelocity.X = 0;
                        _currentVelocity.Y = -1;
                        _lastValidDirectionEvent = e;
                        break;
                }
            }
            // Slowing the game down when moving vertically because
            // there's a much greater distance between characters vertically
            // making it feel a lot faster
            CurrentInterval = verticalMovement ? 1000 / 50 : 1000 / 75;
            //CurrentInterval = 0;
        }

        private char GenHamiltonian( DirectionEvent curEvent ) {
            if ( _lastDrawn == DirectionEvent.NONE ) {
                _lastDrawn = curEvent;
            }
            char ret = 'E';
            switch ( curEvent ) {
                case DirectionEvent.UP:
                    switch ( _lastDrawn ) {
                        case DirectionEvent.UP:
                            ret =  '║';
                            break;
                        case DirectionEvent.RIGHT:
                            ret =  '╝';
                            break;
                        case DirectionEvent.DOWN:
                            ret =  'E';
                            break;
                        case DirectionEvent.LEFT:
                            ret =  '╚';
                            break;
                    }
                    break;
                case DirectionEvent.RIGHT:
                    switch ( _lastDrawn ) {
                        case DirectionEvent.UP:
                            ret =  '╔';
                            break;
                        case DirectionEvent.DOWN:
                            ret =  '╚';
                            break;
                        case DirectionEvent.LEFT:
                            ret =  'E';
                            break;
                        case DirectionEvent.RIGHT:
                            ret =  '═';
                            break;
                    }
                    break;
                case DirectionEvent.DOWN:
                    switch ( _lastDrawn ) {
                        case DirectionEvent.UP:
                            ret =  'E';
                            break;
                        case DirectionEvent.RIGHT:
                            ret =  '╗';
                            break;
                        case DirectionEvent.LEFT:
                            ret =  '╔';
                            break;
                        case DirectionEvent.DOWN:
                            ret =  '║';
                            break;
                    }
                    break;
                case DirectionEvent.LEFT:
                    switch ( _lastDrawn ) {
                        case DirectionEvent.UP:
                            ret =  '╗';
                            break;
                        case DirectionEvent.RIGHT:
                            ret =  'E';
                            break;
                        case DirectionEvent.DOWN:
                            ret =  '╝';
                            break;
                        case DirectionEvent.LEFT:
                            ret =  '═';
                            break;
                    }
                    break;
            }
            _lastDrawn = curEvent;
            return ret;
        }

        private bool _drawnHamilton = false;
        private void DrawSnake( ) {
            var hamiltonianCycle = Program.program.HamiltonianCycleData;
            bool AIShowcase = Program.program.AIShowcase;
            if ( !_drawnHamilton && AIShowcase ) {
                _lastDrawn = DirectionEvent.NONE;
                // Generates the hamiltonian map
                foreach ( var point in hamiltonianCycle.Data.SequenceNumberToPoint ) {
                    _PointToHamChar[ point ] = GenHamiltonian( hamiltonianCycle.Data.MoveDirections[ point.X, point.Y ] );
                }
                _drawnHamilton = true;
                _lastDrawn = DirectionEvent.NONE;

                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                // Draws the hamiltonian map
                foreach ( var keyValuePair in _PointToHamChar ) {
                    Console.SetCursorPosition( keyValuePair.Key.X, keyValuePair.Key.Y );
                    Console.Write( keyValuePair.Value );
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            foreach ( var drawnGrid in _drawnGrids ) {
                // Still valid
                if ( returnData.SnakePositions.Exists( snakeGrid => snakeGrid == drawnGrid ) )
                    continue;

                // Overwrites the drawn snake with a space
                Console.SetCursorPosition( drawnGrid.X, drawnGrid.Y );
                if ( AIShowcase )
                    Console.Write( _PointToHamChar[ drawnGrid ] );
                else
                    Console.Write( ' ' );
                
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach ( var snakeGrid in returnData.SnakePositions ) {
                // Already drawn
                if ( _drawnGrids.Exists( drawnGrid => drawnGrid == snakeGrid ) )
                    continue;

                // Writes a # at the new snake position
                Console.SetCursorPosition( snakeGrid.X, snakeGrid.Y );
                Console.Write( "#" );
            }


            // Drawing food
            if ( _previousFoodPoint != returnData.ApplePosition ) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition( returnData.ApplePosition.X, returnData.ApplePosition.Y );
                Console.Write( "#" );
                _previousFoodPoint = returnData.ApplePosition;
            }



            // Have to make a new copy of snakeGrids otherwise it uses a reference by default :(
            _drawnGrids = new List<Point>( returnData.SnakePositions ); // Now all snake grids are drawn :)
        }
    }
}
