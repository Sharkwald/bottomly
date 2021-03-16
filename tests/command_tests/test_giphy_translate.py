import unittest
from commands import GiphyTranslateCommand


class TestGiphyTranslateCommand(unittest.TestCase):
    def test_empty_input(self):
        command = GiphyTranslateCommand()
        result = command.execute('')

        self.assertIsNone(result)

    def test_valid_input(self):
        command = GiphyTranslateCommand()
        result = command.execute("puppers and kittehs")

        self.assertIsNotNone(result)


if __name__ == '__main__':
    unittest.main()
