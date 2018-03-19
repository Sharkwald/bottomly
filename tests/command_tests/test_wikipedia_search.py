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


if __name__ == '__main__':
    unittest.main()
