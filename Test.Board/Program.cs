﻿using System;

namespace Test.Board
{
    class Program
    {
        static void Main(string[] args)
        {
            var board = Raspberry.Board.Current;

            if (!board.IsRaspberryPi)
                Console.WriteLine("System is not a Raspberry Pi");
            else
            {
                Console.WriteLine("Raspberry Pi running on {0} processor", board.Processor);
                Console.WriteLine("Firmware rev{0}, board model {1} rev{2}", board.Firmware, board.Model, board.Revision);
                Console.WriteLine();
                Console.WriteLine("Serial number: {0}", board.SerialNumber);
            }
        }
    }
}
