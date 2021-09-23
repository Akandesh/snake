using System;
using System.Diagnostics;
using System.Threading;

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
        void EntryPoint( )
        {
            SetupGame( );
            MenuOptions selectedMenu = MainMenu( );

            ClearGameBoard( );
            switch ( selectedMenu ) {
                case MenuOptions.Play:
                    GameLoop( );
                    break;
                case MenuOptions.Quit:
                    Environment.Exit( 0 );
                    break;
                case MenuOptions.Highscores:
                    break;
            }

            Console.SetCursorPosition( 0, GameHeight + 1 );
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.ReadKey( );
        }


        void SetupGame( ) {
            Console.Title = "Snake";
            // Windows Only
            Console.SetWindowSize( GameWidth + 2, GameHeight + 2 );
            drawBorder( );
        }

        MenuOptions MainMenu( ) {
            const string greetingText = "Welcome to Snake! What's your name?";
            Coordinate greetingLocation = new Coordinate {
                x = GameWidth / 2 - greetingText.Length / 2,
                y = GameHeight / 3
            };

            var selectedMenu = MenuOptions.Play;
            DrawMenuOptions( greetingLocation, ref selectedMenu );

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition( greetingLocation.x, greetingLocation.y );
            Console.Write( greetingText );

            Console.ForegroundColor = ConsoleColor.White;
            const string nameString = "Name: ";
            Console.SetCursorPosition( GameWidth / 2 - nameString.Length, greetingLocation.y + 2 );
            Console.Write( nameString );

            var name = Console.ReadLine( );
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

            return selectedMenu;
        }

        void GameLoop( ) {
            var snake = new Snake( GameWidth, GameHeight ); ;

            while ( snake.Running ) {
                if ( Console.KeyAvailable ) {
                    var key = Console.ReadKey( );
                    switch ( key.Key ) {
                        case ConsoleKey.DownArrow:
                            snake.OnDirectionEvent( DirectionEvent.DOWN );
                            break;
                        case ConsoleKey.UpArrow:
                            snake.OnDirectionEvent( DirectionEvent.UP );
                            break;
                        case ConsoleKey.RightArrow:
                            snake.OnDirectionEvent( DirectionEvent.RIGHT );
                            break;
                        case ConsoleKey.LeftArrow:
                            snake.OnDirectionEvent( DirectionEvent.LEFT );
                            break;
                    }
                }
                snake.Tick( );
                Thread.Sleep( snake.CurrentInterval );
            }
        }

        void ClearGameBoard( ) {
            for ( int y = 1; y <= GameHeight - 2; y++ ) {
                Console.SetCursorPosition( 1, y );
                for ( int x = 1; x <= GameWidth - 2; x++ ) {
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


            string playString = $"[{( selectedMenu == MenuOptions.Play ? '*' : ' ' )}] PLAY";
            int offsetFromCenter = playString.Length / 2;
            Console.SetCursorPosition( GameWidth / 2 - offsetFromCenter, greetingLocation.y + 4 );
            Console.Write( playString );

            string highScoreString = $"[{( selectedMenu == MenuOptions.Highscores ? '*' : ' ' )}] HIGHSCORES";
            Console.SetCursorPosition( GameWidth / 2 - offsetFromCenter, greetingLocation.y + 5 );
            Console.Write( highScoreString );

            string quitString = $"[{( selectedMenu == MenuOptions.Quit ? '*' : ' ' )}] QUIT";
            Console.SetCursorPosition( GameWidth / 2 - offsetFromCenter, greetingLocation.y + 6 );
            Console.Write( quitString );
            _lastMenuSelection = selectedMenu;
        }

        void drawBorder( ) {
            Console.Clear( );
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            for ( int y = 0; y <= GameHeight; y++ ) {
                bool onTop = y == 0;
                bool onBottom = y == GameHeight;
                for ( int x = 0; x <= GameWidth; x++ ) {
                    bool onEdge = ( onTop && x == 0 ) ||
                                  ( onTop && x == GameWidth ) ||
                                  ( onBottom && x == 0 ) ||
                                  ( onBottom && x == GameWidth );
                    if ( onEdge ) {
                        Console.Write( '+' );
                        continue;
                    }

                    if ( x == 0 || x == GameWidth || onTop || onBottom ) {
                        Console.Write( '#' );
                        continue;
                    }

                    Console.Write( ' ' );
                }
                Console.WriteLine( );
            }
        }

        static void Main( ) {
            new Program( ).EntryPoint( ); // silly static escape
        }
    }
}
