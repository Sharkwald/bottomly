import unittest
from commands.google_search import GoogleSearchCommand


class TestGoogleSearchCommand(unittest.TestCase):
    def test_empty_input(self):
        command = GoogleSearchCommand()
        result = command.execute('')

        self.assertIsNone(result)

    def test_garbage_input(self):
        command = GoogleSearchCommand()
        result = command.execute('asdhbhklasdvfbuioasvfbhuilasdvfbhjkl')

        self.assertIsNone(result)

    def test_valid_input(self):
        command = GoogleSearchCommand()
        result = command.execute("stackoverflow site:en.wikipedia.org")

        self.assertIsNotNone(result)
        self.assertEqual('Stack overflow - Wikipedia'.lower(), result['title'].lower())
        self.assertEqual('https://en.wikipedia.org/wiki/Stack_overflow'.lower(), result['link'].lower())

if __name__ == '__main__':
    unittest.main()