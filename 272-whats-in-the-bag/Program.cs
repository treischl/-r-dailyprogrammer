using System;
using System.Collections.Generic;
using System.Linq;

namespace _272_whats_in_the_bag
{
    class Program
    {
        static Dictionary<char, int> TilesInBag = new Dictionary<char, int>() { { 'A', 9 }, { 'B', 2 }, { 'C', 2 }, { 'D', 4 }, { 'E', 12 }, { 'F', 2 }, { 'G', 3 }, { 'H', 2 }, { 'I', 9 }, { 'J', 1 }, { 'K', 1 }, { 'L', 4 }, { 'M', 2 }, { 'N', 6 }, { 'O', 8 }, { 'P', 2 }, { 'Q', 1 }, { 'R', 6 }, { 'S', 4 }, { 'T', 6 }, { 'U', 4 }, { 'V', 2 }, { 'W', 2 }, { 'X', 1 }, { 'Y', 2 }, { 'Z', 1 }, { '_', 2 } };
        static Dictionary<char, int> TilesInPlay = new Dictionary<char, int>();
        static Dictionary<char, int> TilePoints = new Dictionary<char, int>() { { 'A', 1 }, { 'B', 3 }, { 'C', 3 }, { 'D', 2 }, { 'E', 1 }, { 'F', 4 }, { 'G', 2 }, { 'H', 4 }, { 'I', 1 }, { 'J', 8 }, { 'K', 5 }, { 'L', 1 }, { 'M', 3 }, { 'N', 1 }, { 'O', 1 }, { 'P', 3 }, { 'Q', 10 }, { 'R', 1 }, { 'S', 1 }, { 'T', 1 }, { 'U', 1 }, { 'V', 4 }, { 'W', 4 }, { 'X', 8 }, { 'Y', 4 }, { 'Z', 10 }, { '_', 0 } };

        static void Main(string[] args)
        {
            Console.WriteLine("Please enter which tiles are in play.");

            bool validEntry = true;
            foreach (var letterInPlay in Console.ReadLine().GroupBy(tile => tile))
            {
                TilesInBag[letterInPlay.Key] -= letterInPlay.Count();
                TilesInPlay[letterInPlay.Key] = letterInPlay.Count();

                if (TilesInBag[letterInPlay.Key] < 0)
                {
                    Console.WriteLine($"Invalid input. More {letterInPlay.Key}'s have been taken from the bag than possible.");
                    validEntry = false;
                }
            }

            if (validEntry)
            {
                Console.WriteLine("\r\nDistribution of tiles in bag:");
                PrintTiles(TilesInBag);

                Console.WriteLine("\r\nDistribution of tiles in play:");
                PrintTiles(TilesInPlay);

                Console.WriteLine();
                Console.WriteLine($"Points in bag: {TilesInBag.Sum(tile => tile.Value * TilePoints[tile.Key])}");
                Console.WriteLine($"Points in play: {TilesInPlay.Sum(tile => tile.Value * TilePoints[tile.Key])}");
            }

            Console.ReadKey();
        }

        static void PrintTiles(Dictionary<char, int> tiles)
        {
            foreach (var tileCount in tiles.GroupBy(kvp => kvp.Value, kvp => kvp.Key).OrderByDescending(group => group.Key))
                Console.WriteLine($"{tileCount.Key}: {string.Join(", ", tileCount.Select(c => c).OrderBy(c => c))}");
        }
    }
}
