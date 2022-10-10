import os
import unittest

from commands.cocktail_of_the_week_search import CocktailOfTheWeekSearchCommand


@unittest.skip("This command is broken to due a change in the output format from The Brass Monkey's website.")
class TestCocktailOfTheWeekSearchCommand(unittest.TestCase):

    def test_empty_input(self):
        command = CocktailOfTheWeekSearchCommand()
        result = command.execute()

        results = result.splitlines();

        self.assertEqual(len(results), 3)
        self.assertEqual(results[0], 'Cocktail of the Week')

if __name__ == '__main__':
    unittest.main()
