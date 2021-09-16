using System;
using System.Threading;

namespace snake
{
    class Program
    {
        void main( ) {
            const int gameWidth = 80;
            const int gameHeight = 20;
            var snake = new Snake( gameWidth, gameHeight ); ;

            setupGame( gameWidth, gameHeight );
            return;
            while ( true ) {
                snake.Tick( );
                Thread.Sleep( 1000 / 15 );
            }
        }


        void setupGame( int width, int height ) {
            Console.Title = "Snake";
            drawBorder( width, height );
            
        }

        void drawBorder( int width, int height ) {
            Console.Clear( );
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            for ( int y = 0; y <= height; y++ ) {
                bool onTop = y == 0;
                bool onBottom = y == height;
                for ( int x = 0; x <= width; x++ ) {
                    bool onEdge = ( onTop && x == 0 ) ||
                                  ( onTop && x == width ) ||
                                  ( onBottom && x == 0 ) ||
                                  ( onBottom && x == width );
                    if ( onEdge ) {
                        Console.Write( '+' );
                        continue;
                    }

                    if ( x == 0 || x == width || onTop || onBottom ) {
                        Console.Write( '#' );
                        continue;
                    }

                    Console.Write( ' ' );
                }
                Console.WriteLine( );
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void Main( ) {
            new Program( ).main( ); // silly static escape
        }
    }
}
