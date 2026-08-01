using System;
using System.Collections.Generic;
using System.Text;

namespace StarRupture.Services;

public static class ConsoleInput
{
    public static string ReadRequiredText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);

            string? input = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            Console.WriteLine("A value is required.");
        }
    }

    public static decimal ReadPositiveDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);

            if (decimal.TryParse(Console.ReadLine(), out decimal value) &&
                value > 0)
            {
                return value;
            }

            Console.WriteLine("Enter a number greater than zero.");
        }
    }

    public static int ReadPositiveInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);

            if (int.TryParse(Console.ReadLine(), out int value) &&
                value > 0)
            {
                return value;
            }

            Console.WriteLine("Enter a whole number greater than zero.");
        }
    }

    public static int ReadNumberInRange(string prompt, int minimum, int maximum)
    {
        while (true)
        {
            Console.Write(prompt);

            if (int.TryParse(Console.ReadLine(), out int value) &&
                value >= minimum &&
                value <= maximum)
            {
                return value;
            }

            Console.WriteLine($"Enter a whole number from {minimum} to {maximum}.");
        }
    }

    public static bool ReadYesNo()
    {
        while (true)
        {
            string? answer = Console.ReadLine()?.Trim();

            if (answer?.Equals("y", StringComparison.OrdinalIgnoreCase) == true || answer?.Equals("yes", StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            if (answer?.Equals("n", StringComparison.OrdinalIgnoreCase) == true || answer?.Equals("no", StringComparison.OrdinalIgnoreCase) == true)
            {
                return false;
            }

            Console.Write("Please enter y or n: ");
        }
    }
}