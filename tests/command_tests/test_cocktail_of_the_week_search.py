import os
import unittest

from commands.cocktail_of_the_week_search import CocktailOfTheWeekSearchCommand


class TestCocktailOfTheWeekSearchCommand(unittest.TestCase):

    def test_empty_input(self):
        command = CocktailOfTheWeekSearchCommand()
        result = command.execute()

        expected = 'Cocktail of the Week' + os.linesep + 'Raspberry Gin Cosmo - (£7.00)' + os.linesep + 'Whitley Neil Raspberry Gin, Triple Sec, Lime Juice, Cranberry'
        self.assertEqual(expected, result)

if __name__ == '__main__':
    unittest.main()
