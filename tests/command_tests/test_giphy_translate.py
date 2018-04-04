import unittest
from commands import GiphyTranslateCommand


class TestGiphyTranslateCommand(unittest.TestCase):
    def test_empty_input(self):
        command = GiphyTranslateCommand()
        result = command.execute('')

        self.assertIsNone(result)

    def test_garbage_input(self):
        command = GiphyTranslateCommand()
        result = command.execute('asdhbhklasdvfbuioasvfbhuilasdvfbhjkl')

        self.assertIsNone(result)

    def test_valid_input(self):
        command = GiphyTranslateCommand()
        result = command.execute("pupper")

        self.assertIsNotNone(result)


if __name__ == '__main__':
    unittest.main()
