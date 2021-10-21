using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Drawing;
using static snake.HamiltonianCycle;
namespace snake
{
    enum MenuOptions
    {
        Play,
        AI,
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
        public bool AIShowcase = false;

        public Difficulty SelectedDifficulty;

        void EntryPoint( ) {
            SetupGame( );

            HamiltonianCycle = new HamiltonianCycle( );
            HamiltonianCycleData = HamiltonianCycle.GetHamiltonianCycleData( GameWidth, GameHeight );
            HamiltonianCycleData = CorrectHamiltonian( ); // gameplan is 2px smaller due to borders 


            bool gameRunning = true;
            while ( gameRunning ) {
                MenuOptions selectedMenu = MainMenu( );
                ClearGameBoard( );
                switch ( selectedMenu ) {
                    case MenuOptions.Play:
                        AIShowcase = false;
                        SelectDifficulty( );
                        GameLoop( );
                        ClearGameBoard( );
                        break;
                    case MenuOptions.AI:
                        AIShowcase = true;
                        GameLoop( );
                        ClearGameBoard( );
                        break;
                    case MenuOptions.Highscores:
                        DisplayHighscores( );
                        break;
                    case MenuOptions.Quit:
                        gameRunning = false;
                        break;
                }
            }

            Console.SetCursorPosition( 0, GameHeight + 1 );
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        HamiltonianCycleData CorrectHamiltonian( ) {
            HamiltonianCycleData ret = HamiltonianCycleData.Clone( ) as HamiltonianCycleData;
            var data = new Data( ) {
                MoveDirections = new DirectionEvent[ GameWidth + 2, GameHeight + 2 ],
                PointToSequenceNumber = new int[ GameWidth + 2, GameHeight + 2 ],
                SequenceNumberToPoint = new Point[ ( GameWidth + 2 ) * ( GameHeight + 2 ) ]
            };

            for ( int x = 1; x < ret.Data.MoveDirections.GetLength( 0 ) + 1; x++ ) {
                for ( int y = 1; y < ret.Data.MoveDirections.GetLength( 1 ) + 1; y++ ) {
                    data.MoveDirections[ x, y ] = ret.Data.MoveDirections[ x - 1, y - 1 ];
                }
            }

            HamiltonianCycle.GenerateSequence( GameWidth + 2, GameHeight + 2, data, 1 );
            ret.Data = data;
            return ret;
        }

        void SelectDifficulty( ) {
            Console.ForegroundColor = ConsoleColor.White;
            void DrawDifficulties( ref Difficulty difficulty ) {
                // Keeps the selected difficulty in range
                while ( !Enum.IsDefined( typeof( Difficulty ), difficulty ) ) {
                    if ( (int)difficulty < 0 )
                        difficulty++;
                    else if ( (int)difficulty > (int)Difficulty.Hard ) // Hard is the back of Difficulty
                        difficulty--;
                }

                Console.SetCursorPosition( GameWidth / 5, GameHeight / 2 );
                DrawDifficultySelection( "EASY", difficulty == Difficulty.Easy, 1 );
                DrawDifficultySelection( "NORMAL", difficulty == Difficulty.Normal, 2 );
                DrawDifficultySelection( "HARD", difficulty == Difficulty.Hard, 3 );
            }

            DrawDifficulties( ref SelectedDifficulty );

            bool waitingForSelection = true;
            while ( waitingForSelection ) {
                var key = Console.ReadKey( true );
                switch ( key.Key ) {
                    case ConsoleKey.RightArrow: {
                        SelectedDifficulty++;
                        DrawDifficulties( ref SelectedDifficulty );
                        break;
                    }
                    case ConsoleKey.LeftArrow: {
                        SelectedDifficulty--;
                        DrawDifficulties( ref SelectedDifficulty );
                        break;
                    }
                    case ConsoleKey.Enter:
                        waitingForSelection = false;
                        break;
                    default:
                        break;
                }
            }
            ClearGameBoard( );
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


                    // Init if null
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
            Point greetingLocation = new Point {
                X = GameWidth / 2 - greetingText.Length / 2,
                Y = GameHeight / 3
            };

            var selectedMenu = MenuOptions.Play;
            DrawMenuOptions( ref selectedMenu );

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition( greetingLocation.X, greetingLocation.Y );
            Console.Write( greetingText );

            // Hiding the cursor for the future
            Console.CursorVisible = false;


            Console.SetCursorPosition( 0, 0 );
            bool waitingForSelection = true;
            while ( waitingForSelection ) {
                var key = Console.ReadKey( true );
                switch ( key.Key ) {
                    case ConsoleKey.DownArrow: {
                        selectedMenu++;
                        DrawMenuOptions( ref selectedMenu );
                        break;
                    }
                    case ConsoleKey.UpArrow: {
                        selectedMenu--;
                        DrawMenuOptions( ref selectedMenu );
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
                if ( !AIShowcase && Console.KeyAvailable ) {
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

                if ( AIShowcase ) {
                    if ( Console.KeyAvailable ) {
                        var key = Console.ReadKey( true );
                        if ( key.Key == ConsoleKey.Escape ) {
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

                    //Maybe now exists a shortcut path.
                    if ( _snakeInstance.returnData.ShotCutMoveDirections.Count != 0 && _snakeInstance.DirectionValid( _snakeInstance.returnData.ShotCutMoveDirections[ 0 ] ) ) {
                        // Minimum one shortcut move direction exists. So it must be used!
                        _snakeInstance.OnDirectionEvent( _snakeInstance.returnData.ShotCutMoveDirections[ 0 ] );
                        _snakeInstance.returnData.ShotCutMoveDirections.RemoveAt( 0 );
                    } else {
                        // Otherwise follow the regular path
                        _snakeInstance.OnDirectionEvent(
                        HamiltonianCycleData.Data.MoveDirections[ _snakeInstance.returnData.HeadPosition.X,
                            _snakeInstance.returnData.HeadPosition.Y ] );
                    }
                }
                _snakeInstance.Tick( );
                Thread.Sleep( _snakeInstance.CurrentInterval );
            }
        }

        void ClearGameBoard( ) {
            for ( int y = 1; y <= GameHeight; y++ ) {
                Console.SetCursorPosition( 1, y );
                for ( int x = 1; x <= GameWidth; x++ ) {
                    Console.Write( " " );
                }
            }
        }

        private MenuOptions _lastMenuSelection = MenuOptions.Quit;
        void DrawMenuOptions( ref MenuOptions selectedMenu ) {
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

            DrawMainMenuOption( "PLAY", selectedMenu == MenuOptions.Play, 4 );
            DrawMainMenuOption( "DEMO", selectedMenu == MenuOptions.AI, 4, 1 );
            DrawMainMenuOption( "HIGHSCORES", selectedMenu == MenuOptions.Highscores, 4, 2 );
            DrawMainMenuOption( "QUIT", selectedMenu == MenuOptions.Quit, 4, 3 );
            _lastMenuSelection = selectedMenu;
        }

        void DrawMainMenuOption( string name, bool selected, int nameOffset, int offsetFromCenter = 0 ) {
            string playString = $"[{( selected ? '┼' : ' ' )}] {name}";
            Console.SetCursorPosition( GameWidth / 2 - nameOffset, GameHeight / 2 + offsetFromCenter );
            Console.Write( playString );
        }

        void DrawDifficultySelection( string name, bool selected, int index ) {
            string playString = $"[{( selected ? '┼' : ' ' )}] {name}";
            Point point = new Point {
                X = ( GameWidth / 5 ) * index,
                Y = GameHeight / 2
            };
            Console.SetCursorPosition( point.X, point.Y );
            Console.Write( playString );
        }

        void DisplayHighscores( ) {
            // Clear the area where text will go
            ClearGameBoard( );

            Difficulty selectedDifficulty = Difficulty.Normal;

            void DrawHighScoreOptions( ref Difficulty difficulty ) {
                // Keeps the selected difficulty in range
                while ( !Enum.IsDefined( typeof( Difficulty ), difficulty ) ) {
                    if ( (int)difficulty < 0 )
                        difficulty++;
                    else if ( (int)difficulty > (int)Difficulty.Hard ) // Hard is the back of Difficulty
                        difficulty--;
                }

                Console.SetCursorPosition( GameWidth / 5, GameHeight / 2 );
                DrawDifficultySelection( "EASY", difficulty == Difficulty.Easy, 1 );
                DrawDifficultySelection( "NORMAL", difficulty == Difficulty.Normal, 2 );
                DrawDifficultySelection( "HARD", difficulty == Difficulty.Hard, 3 );
            }

            DrawHighScoreOptions( ref selectedDifficulty );


            bool waitingForSelection = true;
            while ( waitingForSelection ) {
                var key = Console.ReadKey( true );
                switch ( key.Key ) {
                    case ConsoleKey.RightArrow: {
                        selectedDifficulty++;
                        DrawHighScoreOptions( ref selectedDifficulty );
                        break;
                    }
                    case ConsoleKey.LeftArrow: {
                        selectedDifficulty--;
                        DrawHighScoreOptions( ref selectedDifficulty );
                        break;
                    }
                    case ConsoleKey.Enter:
                        waitingForSelection = false;
                        break;
                    default:
                        break;
                }
            }

            ClearGameBoard( );

            // Write the header
            string highScoreString = $"[{selectedDifficulty}] HIGHSCORES";
            int offsetFromCenter = highScoreString.Length / 2;
            Console.SetCursorPosition( GameWidth / 2 - offsetFromCenter, GameHeight / 8 );
            Console.Write( highScoreString );


            var highScores = _snakeInstance.HighScores.Scores;
            highScores = highScores.FindAll( score => score.difficulty == selectedDifficulty );

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
            for ( int y = 0; y <= GameHeight + 1; y++ ) {
                bool onTop = y == 0;
                bool onBottom = y == GameHeight + 1;
                for ( int x = 0; x <= GameWidth + 1; x++ ) {
                    if ( onTop && x == 0 ) {
                        Console.Write( '╔' );
                        continue;
                    }
                    if ( onBottom && x == 0 ) {
                        Console.Write( '╚' );
                        continue;
                    }
                    if ( onTop && x == GameWidth + 1 ) {
                        Console.Write( '╗' );
                        continue;
                    }
                    if ( onBottom && x == GameWidth + 1 ) {
                        Console.Write( '╝' );
                        continue;
                    }
                    if ( onTop || onBottom ) {
                        Console.Write( "═" );
                        continue;
                    }
                    if ( x == 0 || x == GameWidth + 1 ) {
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
