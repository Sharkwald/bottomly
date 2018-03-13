import unittest
from commands.google_search import GoogleSearchCommand


class TestGoogleSearchCommand(unittest.TestCase):
    def test_search_empty_input(self):
        command = GoogleSearchCommand()
        result = command.execute('')

        self.assertIsNone(result)

    def test_search_valid_input(self):
        command = GoogleSearchCommand()
        result = command.execute("stackoverflow site:en.wikipedia.org")

        self.assertIsNotNone(result)

if __name__ == '__main__':
    unittest.main()