using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;
using static snake.HamiltonianCycle;
namespace snake
{
    enum MenuOptions
    {
        Play,
        Highscores,
        Quit
    }

    class Program
    {
        const int GameWidth = 80;
        const int GameHeight = 20;
        private Snake _snakeInstance = null;

        public HamiltonianCycle HamiltonianCycle;
        public HamiltonianCycleData HamiltonianCycleData;

        void EntryPoint( ) {
            SetupGame( );

            HamiltonianCycle = new HamiltonianCycle( );
            HamiltonianCycleData = HamiltonianCycle.GetHamiltonianCycleData( GameWidth - 2, GameHeight - 1 ); // gameplan is 2px smaller due to borders 

            bool gameRunning = true;
            while ( gameRunning ) {
                MenuOptions selectedMenu = MainMenu( );
                ClearGameBoard( );
                switch ( selectedMenu ) {
                    case MenuOptions.Play:
                        GameLoop( );
                        ClearGameBoard( );
                        break;
                    case MenuOptions.Quit:
                        gameRunning = false;
                        break;
                    case MenuOptions.Highscores:
                        DisplayHighscores( );
                        break;
                }
            }

            Console.SetCursorPosition( 0, GameHeight + 1 );
            Console.ForegroundColor = ConsoleColor.Gray;
        }


        void SetupGame( ) {
            // Initializing the snake instance
            if ( _snakeInstance == null )
                _snakeInstance = new Snake( GameWidth, GameHeight ); ;

            const string filePath = "scores.bin"; // Also one occurance in snake.cs
            if ( _snakeInstance.HighScores == null ) {
                // Reading save data or initializing it
                if ( File.Exists( filePath ) ) {
                    DataSerializer dataSerializer = new( );
                    _snakeInstance.HighScores = dataSerializer.Deserialize( filePath ) as HighScores;


                    // Init if null, did this to suppress warnings but *shouldn't* happen
                    _snakeInstance.HighScores ??= new HighScores( );
                    _snakeInstance.HighScores.Scores ??= new List<HighScore>( );
                } else {
                    _snakeInstance.HighScores = new HighScores( );
                    _snakeInstance.HighScores.Scores = new List<HighScore>( );
                }
            }

            Console.Title = "Snake";
            // Windows Only
#pragma warning disable CA1416 // Validate platform compatibility
            Console.SetWindowSize( GameWidth + 2, GameHeight + 2 );
#pragma warning restore CA1416 // Validate platform compatibility
            drawBorder( );
        }

        MenuOptions MainMenu( ) {
            const string greetingText = "Welcome to Snake!";
            Coordinate greetingLocation = new Coordinate {
                x = GameWidth / 2 - greetingText.Length / 2,
                y = GameHeight / 3
            };

            var selectedMenu = MenuOptions.Play;
            DrawMenuOptions( greetingLocation, ref selectedMenu );

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition( greetingLocation.x, greetingLocation.y );
            Console.Write( greetingText );

            // Hiding the cursor for the future
            Console.CursorVisible = false;

            bool waitingForSelection = true;
            while ( waitingForSelection ) {
                var key = Console.ReadKey( );
                switch ( key.Key ) {
                    case ConsoleKey.DownArrow: {
                        selectedMenu++;
                        DrawMenuOptions( greetingLocation, ref selectedMenu );
                        break;
                    }
                    case ConsoleKey.UpArrow: {
                        selectedMenu--;
                        DrawMenuOptions( greetingLocation, ref selectedMenu );
                        break;
                    }
                    case ConsoleKey.Enter:
                        waitingForSelection = false;
                        break;
                    default:
                        break;
                }
            }

            // Setting global selection back to default
            _lastMenuSelection = MenuOptions.Quit;
            return selectedMenu;
        }

        void GameLoop( ) {
            _snakeInstance.Reset( );
            ShortCut shortCut = new( );
            while ( _snakeInstance.Running ) {
                if ( Console.KeyAvailable ) {
                    var key = Console.ReadKey( true );
                    switch ( key.Key ) {
                        case ConsoleKey.DownArrow:
                            _snakeInstance.OnDirectionEvent( DirectionEvent.DOWN );
                            break;
                        case ConsoleKey.UpArrow:
                            _snakeInstance.OnDirectionEvent( DirectionEvent.UP );
                            break;
                        case ConsoleKey.RightArrow:
                            _snakeInstance.OnDirectionEvent( DirectionEvent.RIGHT );
                            break;
                        case ConsoleKey.LeftArrow:
                            _snakeInstance.OnDirectionEvent( DirectionEvent.LEFT );
                            break;
                    }
                }

                if ( _snakeInstance.returnData.ShotCutMoveDirections == null || _snakeInstance.returnData.ShotCutMoveDirections.Count == 0 ) {
                    switch ( HamiltonianCycleData.Case ) {
                        case Case.Case_1_And_3:
                            shortCut.CalcShortCutForCase1And3( _snakeInstance.returnData, HamiltonianCycleData );
                            break;
                        case Case.Case_2:
                            shortCut.CalcShortCutForCase2( _snakeInstance.returnData, HamiltonianCycleData );
                            break;
                        default:
                            throw new Exception( "Unknown case!" );
                    }
                }

                // Maybe now exists a shortcut path.
                if ( _snakeInstance.returnData.ShotCutMoveDirections.Count != 0 ) {
                    // Minimum one shortcut move direction exists. So it must be used!
                    _snakeInstance.OnDirectionEvent( _snakeInstance.returnData.ShotCutMoveDirections[ 0 ] );
                    _snakeInstance.returnData.ShotCutMoveDirections.RemoveAt( 0 );
                } else {
                    if ( _snakeInstance.returnData.HeadPosition.y == 2 ) {
                        Debug.Print( "Breakpoint" );
                    }
                    var test = HamiltonianCycleData.Data.MoveDirections[ _snakeInstance.returnData.HeadPosition.x - 1, _snakeInstance.returnData.HeadPosition.y - 1 ];


                    _snakeInstance.OnDirectionEvent(
                        HamiltonianCycleData.Data.MoveDirections[ _snakeInstance.returnData.HeadPosition.x - 1,
                            _snakeInstance.returnData.HeadPosition.y - 1 ] );
                }

                //Debug.Print( String.Format( "{0} : {1}", _snakeInstance._currentHeadPosition.x, _snakeInstance._currentHeadPosition.y ) );

                _snakeInstance.Tick( );
                Thread.Sleep( _snakeInstance.CurrentInterval );
            }
        }

        void ClearGameBoard( ) {
            for ( int y = 1; y <= GameHeight - 1; y++ ) {
                Console.SetCursorPosition( 1, y );
                for ( int x = 1; x <= GameWidth - 1; x++ ) {
                    Console.Write( " " );
                }
            }
        }

        private MenuOptions _lastMenuSelection = MenuOptions.Quit;
        void DrawMenuOptions( Coordinate greetingLocation, ref MenuOptions selectedMenu ) {
            // Keeps the selected menu in range
            while ( !Enum.IsDefined( typeof( MenuOptions ), selectedMenu ) ) {
                if ( (int)selectedMenu < 0 )
                    selectedMenu++;
                else if ( (int)selectedMenu > (int)MenuOptions.Quit ) // Quit is the back of MenuOptions
                    selectedMenu--;
            }

            // Prevents menu being redrawn when it doesn't need to be
            if ( selectedMenu == _lastMenuSelection )
                return;

            Console.ForegroundColor = ConsoleColor.White;


            string playString = $"[{( selectedMenu == MenuOptions.Play ? '┼' : ' ' )}] PLAY";
            int offsetFromCenter = playString.Length / 2;
            Console.SetCursorPosition( GameWidth / 2 - offsetFromCenter, GameHeight / 2 );
            Console.Write( playString );

            string highScoreString = $"[{( selectedMenu == MenuOptions.Highscores ? '┼' : ' ' )}] HIGHSCORES";
            Console.SetCursorPosition( GameWidth / 2 - offsetFromCenter, GameHeight / 2 + 1 );
            Console.Write( highScoreString );

            string quitString = $"[{( selectedMenu == MenuOptions.Quit ? '┼' : ' ' )}] QUIT";
            Console.SetCursorPosition( GameWidth / 2 - offsetFromCenter, GameHeight / 2 + 2 );
            Console.Write( quitString );
            _lastMenuSelection = selectedMenu;
        }

        void DisplayHighscores( ) {
            // Clear the area where text will go
            ClearGameBoard( );

            // Write the header
            string highScoreString = "HIGHSCORES";
            int offsetFromCenter = highScoreString.Length / 2;
            Console.SetCursorPosition( GameWidth / 2 - offsetFromCenter, GameHeight / 8 );
            Console.Write( highScoreString );


            var highScores = _snakeInstance.HighScores.Scores;

            int leftPosition = 4;
            int topPosition = GameHeight / 8 + 2;
            int maxElements = GameHeight - topPosition - 1;

            // Basically clamping the highscore count to fit inside the area
            int iterations = highScores.Count > maxElements ? maxElements : highScores.Count;

            // Output the scores
            for ( int i = 0; i < iterations; i++ ) {
                var score = highScores[ i ];
                Console.SetCursorPosition( leftPosition, topPosition + i );
                Console.Write( $"{i + 1}. {score.Name}" );
                Console.SetCursorPosition( GameWidth - GameWidth / 4, topPosition + i );
                Console.Write( score.Score );
            }

            Console.ReadKey( true );
            ClearGameBoard( );
        }

        void drawBorder( ) {
            Console.Clear( );
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.SetCursorPosition( 0, 0 );
            for ( int y = 0; y <= GameHeight; y++ ) {
                bool onTop = y == 0;
                bool onBottom = y == GameHeight;
                for ( int x = 0; x <= GameWidth; x++ ) {
                    if ( onTop && x == 0 ) {
                        Console.Write( '╔' );
                        continue;
                    }
                    if ( onBottom && x == 0 ) {
                        Console.Write( '╚' );
                        continue;
                    }
                    if ( onTop && x == GameWidth ) {
                        Console.Write( '╗' );
                        continue;
                    }
                    if ( onBottom && x == GameWidth ) {
                        Console.Write( '╝' );
                        continue;
                    }
                    if ( onTop || onBottom ) {
                        Console.Write( "═" );
                        continue;
                    }
                    if ( x == 0 || x == GameWidth ) {
                        Console.Write( '║' );
                        continue;
                    }
                    Console.Write( ' ' );
                }
                Console.WriteLine( );
            }
        }

        public static Program program = null;
        static void Main( ) {
            program = new Program( ); // silly static escape
            program.EntryPoint( );
        }
    }
}
