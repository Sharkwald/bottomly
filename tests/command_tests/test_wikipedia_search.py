import unittest
from commands.wikipedia_search import WikipediaSearchCommand


class TestWikipediaSearchCommand(unittest.TestCase):
    def test_empty_input(self):
        command = WikipediaSearchCommand()
        result = command.execute('')

        self.assertIsNone(result)

    def test_garbage_input(self):
        command = WikipediaSearchCommand()
        result = command.execute('asdhbhklasdvfbuioasvfbhuilasdvfbhjkl')

        self.assertIsNone(result)

    def test_valid_input(self):
        command = WikipediaSearchCommand()
        result = command.execute("python")

        self.assertIsNotNone(result)

    def test_possible_matches_search(self):
        command = WikipediaSearchCommand()
        result = command.execute("nuggets")

        self.assertIsNotNone(result)
        self.assertFalse(" may refer to " in result['text'])


if __name__ == '__main__':
    unittest.main()
